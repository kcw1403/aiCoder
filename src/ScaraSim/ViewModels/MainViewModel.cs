using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ScaraSim.Kinematics;
using ScaraSim.Models;
using ScaraSim.Services;

namespace ScaraSim.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly RobotBuilderService _robotBuilder = new();
    private readonly MeshLoaderService _meshLoader = new();
    private readonly AabbCollisionService _collision = new();

    private readonly RobotDimensions _dim = new();
    private RobotVisualParts _robotParts;

    private readonly ObservableCollection<SceneObject> _environment = new();

    public MainViewModel()
    {
        _robotParts = _robotBuilder.BuildDefaultRobot(_dim);
        RebuildSceneModels();
        UpdatePose();
    }

    [ObservableProperty]
    private Model3DGroup _sceneModels = new();

    public double Link1Length
    {
        get => _dim.Link1Length;
        set
        {
            if (SetDimension(v => _dim.Link1Length = v, _dim.Link1Length, value))
                RebuildRobot();
        }
    }

    public double Link2Length
    {
        get => _dim.Link2Length;
        set
        {
            if (SetDimension(v => _dim.Link2Length = v, _dim.Link2Length, value))
                RebuildRobot();
        }
    }

    public double ToolLength
    {
        get => _dim.ToolLength;
        set
        {
            if (SetDimension(v => _dim.ToolLength = v, _dim.ToolLength, value))
                RebuildRobot();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JointSummary))]
    private double _q1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JointSummary))]
    private double _s2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JointSummary))]
    private double _q3;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JointSummary))]
    private double _q4;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JointSummary))]
    private double _q5;

    [ObservableProperty]
    private double _s2Max = 400.0;

    [ObservableProperty]
    private string _collisionStatus = "OK";

    [ObservableProperty]
    private bool _isColliding;

    [ObservableProperty]
    private string _statusMessage = "Ready.";

    public string JointSummary =>
        $"q1={Q1:F1}°  S2={S2:F1}mm  q3={Q3:F1}°  q4={Q4:F1}°  q5={Q5:F1}°";

    partial void OnQ1Changed(double value) => UpdatePose();
    partial void OnS2Changed(double value) => UpdatePose();
    partial void OnQ3Changed(double value) => UpdatePose();
    partial void OnQ4Changed(double value) => UpdatePose();
    partial void OnQ5Changed(double value) => UpdatePose();

    [RelayCommand]
    private void ResetPose()
    {
        Q1 = 0;
        S2 = 0;
        Q3 = 0;
        Q4 = 0;
        Q5 = 0;
    }

    [RelayCommand]
    private void LoadEnvironmentMesh()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load environment mesh",
            Filter = "Mesh files (*.stl;*.obj;*.3ds;*.ply)|*.stl;*.obj;*.3ds;*.ply|All files (*.*)|*.*",
            Multiselect = true,
        };

        if (dialog.ShowDialog() != true)
            return;

        foreach (var path in dialog.FileNames)
        {
            try
            {
                var obj = _meshLoader.LoadAsObject(path, SceneObjectKind.Environment, Colors.DarkSeaGreen);
                _environment.Add(obj);
                StatusMessage = $"Loaded: {Path.GetFileName(path)}";
            }
            catch (MeshLoadException ex)
            {
                StatusMessage = $"Load failed: {ex.Message}";
            }
        }

        RebuildSceneModels();
        UpdatePose();
    }

    [RelayCommand]
    private void LoadRobotPartMesh()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load robot part mesh (replaces default tool visual)",
            Filter = "Mesh files (*.stl;*.obj;*.3ds;*.ply)|*.stl;*.obj;*.3ds;*.ply|All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var obj = _meshLoader.LoadAsObject(dialog.FileName, SceneObjectKind.Environment, Colors.Goldenrod);
            _environment.Add(obj);
            StatusMessage = $"Loaded part: {Path.GetFileName(dialog.FileName)}";
            RebuildSceneModels();
            UpdatePose();
        }
        catch (MeshLoadException ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearEnvironment()
    {
        _environment.Clear();
        RebuildSceneModels();
        UpdatePose();
        StatusMessage = "Environment cleared.";
    }

    private JointState CurrentJoints => new(Q1, S2, Q3, Q4, Q5);

    private void UpdatePose()
    {
        _robotBuilder.ApplyPose(_robotParts, CurrentJoints, _dim);
        ApplyTransformsToSceneModels();
        RunCollisionCheck();
    }

    private void RebuildRobot()
    {
        _robotParts = _robotBuilder.BuildDefaultRobot(_dim);
        RebuildSceneModels();
        UpdatePose();
    }

    private void RebuildSceneModels()
    {
        _wrappers.Clear();
        var group = new Model3DGroup();
        group.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
        group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -3)));
        group.Children.Add(new DirectionalLight(
            Color.FromRgb(120, 120, 120), new Vector3D(1, 1, -1)));

        foreach (var part in _robotParts.All())
            group.Children.Add(WrapWithTransform(part));

        foreach (var env in _environment)
            group.Children.Add(WrapWithTransform(env));

        SceneModels = group;
    }

    private readonly Dictionary<SceneObject, Model3DGroup> _wrappers = new();

    private Model3D WrapWithTransform(SceneObject obj)
    {
        var wrapper = new Model3DGroup();
        wrapper.Children.Add(obj.Model);
        wrapper.Transform = obj.Transform;
        _wrappers[obj] = wrapper;
        return wrapper;
    }

    private void ApplyTransformsToSceneModels()
    {
        foreach (var (obj, wrapper) in _wrappers)
            wrapper.Transform = obj.Transform;
    }

    private void RunCollisionCheck()
    {
        var all = _robotParts.All().Concat(_environment).ToList();
        var result = _collision.Check(all);

        IsColliding = result.HasCollision;
        if (result.HasCollision)
        {
            var joined = string.Join(", ",
                result.Pairs.Select(p => $"{p.A}×{p.B}"));
            CollisionStatus = $"COLLISION: {joined}";
        }
        else
        {
            CollisionStatus = "OK";
        }
    }

    private bool SetDimension(Action<double> setter, double current, double value)
    {
        if (value <= 0 || Math.Abs(current - value) < 1e-6)
            return false;

        setter(value);
        return true;
    }
}

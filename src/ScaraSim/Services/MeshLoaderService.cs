using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ScaraSim.Models;

namespace ScaraSim.Services;

public sealed class MeshLoadException : Exception
{
    public MeshLoadException(string message, Exception? inner = null) : base(message, inner) { }
}

public sealed class MeshLoaderService
{
    private static readonly string[] SupportedExtensions =
        { ".stl", ".obj", ".3ds", ".ply", ".off", ".lwo" };

    public bool IsSupported(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());

    public Model3D Load(string path)
    {
        if (!File.Exists(path))
            throw new MeshLoadException($"File not found: {path}");

        if (!IsSupported(path))
            throw new MeshLoadException($"Unsupported mesh format: {Path.GetExtension(path)}");

        try
        {
            var importer = new ModelImporter
            {
                DefaultMaterial = MaterialHelper.CreateMaterial(Colors.SteelBlue),
            };

            var model = importer.Load(path);
            if (model is null || model.Children.Count == 0)
                throw new MeshLoadException($"No geometry found in: {path}");

            model.Freeze();
            return model;
        }
        catch (MeshLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MeshLoadException($"Failed to load mesh '{path}': {ex.Message}", ex);
        }
    }

    public SceneObject LoadAsObject(string path, SceneObjectKind kind, Color color)
    {
        var raw = Load(path);
        var recolored = Recolor(raw, color);
        recolored.Freeze();

        return new SceneObject
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Kind = kind,
            Model = recolored,
            LocalBounds = AABB.FromBounds(recolored.Bounds),
        };
    }

    private static Model3D Recolor(Model3D source, Color color)
    {
        var material = MaterialHelper.CreateMaterial(color);
        var group = new Model3DGroup();

        void Walk(Model3D node)
        {
            switch (node)
            {
                case Model3DGroup g:
                    foreach (var child in g.Children)
                        Walk(child);
                    break;
                case GeometryModel3D geo:
                    group.Children.Add(new GeometryModel3D
                    {
                        Geometry = geo.Geometry,
                        Material = material,
                        BackMaterial = material,
                        Transform = geo.Transform,
                    });
                    break;
            }
        }

        Walk(source);
        return group;
    }
}

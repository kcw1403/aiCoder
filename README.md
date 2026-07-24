# SCARA 3D Robot Simulator

A Windows desktop application that visualizes a **5-axis RPRR Dual-Yaw SCARA** robot in 3D, lets you jog each joint interactively, imports STL/OBJ meshes for robot parts and surrounding structures, and flags interference between bodies using fast AABB (axis-aligned bounding box) approximation.

Built with **.NET 8 (WPF)**, **HelixToolkit.Wpf** for 3D rendering, and **CommunityToolkit.Mvvm** for the MVVM layer.

> Scope note: this is a **visualization-focused** tool. Collision detection is an approximate broad-phase AABB check for on-screen feedback while jogging — precise mesh-level interference is intentionally out of scope.

## Features

- **3D viewport** with grid, coordinate axes, view cube, orbit/pan/zoom (HelixToolkit).
- **5-axis jogging** via sliders + numeric entry:
  - `q1` — base yaw (revolute)
  - `S2` — vertical carriage (prismatic, mm)
  - `q3` — link-1 yaw (revolute)
  - `q4` — link-2 yaw (revolute, the second "dual" yaw)
  - `q5` — tool yaw (revolute)
- **Adjustable link dimensions** at runtime — defaults: Link 1 = **440 mm**, Link 2 = **440 mm**, Tool = **340 mm**.
- **Mesh import** (STL / OBJ / 3DS / PLY) for environment structures and robot parts.
- **AABB interference check** that runs on every pose change; the status panel turns red and lists the colliding pairs. Adjacent robot links are skipped (they always touch by design).

## Requirements

- Windows 10 / 11
- .NET 8 SDK (`net8.0-windows`)
- Visual Studio 2022 (17.8+) or `dotnet` CLI

## Build & Run

```powershell
dotnet restore
dotnet build -c Release
dotnet run --project src/ScaraSim/ScaraSim.csproj
```

Or open `ScaraSim.sln` in Visual Studio and press F5.

## Project layout

```
ScaraSim.sln
src/ScaraSim/
  App.xaml(.cs)                     Application entry point
  Kinematics/
    RobotDimensions.cs              Adjustable link lengths (mm)
    JointState.cs                   Joint values + resolved chain pose
    ScaraForwardKinematics.cs       RPRR Dual-Yaw forward kinematics
  Models/
    AABB.cs                         Axis-aligned bounding box math
    SceneObject.cs                  Renderable body + world bounds
  Services/
    MeshLoaderService.cs            STL/OBJ import via HelixToolkit
    RobotBuilderService.cs          Builds link geometry + applies pose
    AabbCollisionService.cs         Broad-phase interference detection
  ViewModels/
    MainViewModel.cs                MVVM state, commands, jogging
    CollisionBrushConverter.cs      Status color binding
  Views/
    MainWindow.xaml(.cs)            3D viewport + control panel
```

## Kinematic model

All revolute axes are parallel to world **+Z** (classic SCARA). Planar reach is driven by `q1/q3/q4` and height purely by the prismatic `S2`; the tool hangs downward from the wrist by the tool length. See `ScaraForwardKinematics.cs` for the exact frame chain.

# ShipRenderTest - 3D Model Viewer & Debugger

Advanced development tool for viewing, inspecting, and debugging Wipeout 3D models and textures. Built with OpenTK and ImGui for real-time visualization of game assets.

## Overview

ShipRenderTest is a standalone application for working with Wipeout PRM (model) and CMP (texture) files. It provides a complete 3D environment with scene management, texture inspection, and real-time editing capabilities.

**Based on**: [wipeout-rewrite](https://github.com/phoboslab/wipeout-rewrite) by Dominic Szablewski

## Features

### 3D Viewport
- **Real-time OpenGL Rendering**
  - Perspective camera with 73.75° FOV (matching original game)
  - Orbit controls with smooth mouse interaction
  - Grid floor and coordinate axes for spatial reference
  - 3D orientation gizmo for camera direction

- **Camera Controls**
  - Left mouse drag: Orbit around target
  - Right mouse drag: Pan camera
  - Mouse wheel: Zoom in/out (work in progress)
  - Reset camera button: Return to default view

- **Rendering Options**
  - Wireframe mode toggle
  - Auto-rotate with configurable axis
  - Per-object visibility control
  - Face culling toggle

### Scene Management
- **Scene Hierarchy Panel**
  - Add multiple objects to scene
  - Create multiple cameras
  - Add directional lights
  - Select and rename objects
  - Delete objects from scene
  - Visual indicators for selected and active items

- **Multi-Object Support**
  - Load multiple PRM models simultaneously
  - Independent transforms per object
  - Automatic spacing to prevent overlaps
  - Object selection and focus

### Asset Browser
- **PRM File Browser**
  - Search and filter 171+ available models
  - Expandable multi-object PRM files
  - Visual indication of object count per file
  - Quick "Add to Scene" functionality
  - Recent files tracking

- **File Organization**
  - Automatic scanning of asset directories
  - Support for nested object indices
  - File path and name display
  - Persistent selection state

### Texture Inspector
- **Texture Preview Panel** ⭐ NEW
  - Visual thumbnails of all model textures
  - Adjustable preview size (64-256 pixels)
  - Grid layout adapts to window width
  - Displays texture ID, dimensions, and aspect ratio
  - Hover tooltips with detailed information
  - Shows texture handle validation status
  - Reports texture loading errors

- **Diagnostic Information**
  - Texture array status
  - Valid handle count
  - Textured primitive count
  - Mapping verification
  - CMP loading status

### Properties & Transform
- **Properties Panel**
  - Object name and type
  - Transform values (position, rotation, scale)
  - Geometry statistics (vertices, primitives)
  - Texture count and status
  - Visibility toggle
  - Wireframe mode per object

- **Transform Panel**
  - Precise position editing (X, Y, Z)
  - Rotation controls (Euler angles)
  - Uniform scale adjustment
  - Reset buttons for each component
  - Real-time preview of changes

- **Camera Panel**
  - FOV adjustment
  - Near/far plane configuration
  - Position and target editing
  - Camera mode switching
  - Multiple camera management

- **Light Panel**
  - Directional light controls
  - Color picker (RGB)
  - Intensity adjustment
  - Direction vector editing

### Settings & Configuration
- **Persistent Settings**
  - UI scale/DPI configuration
  - Panel visibility preferences
  - Auto-rotate settings
  - Grid and axes toggles
  - Window layout state
  - Recent files history

- **Viewport Info**
  - Vertex and primitive counts
  - Camera position display
  - Auto-rotate controls
  - Performance metrics

## Usage

### Running the Tool

```bash
# From project root
./test-ship-render.sh

# Or directly with dotnet
dotnet run --project tools/ShipRenderTest/ShipRenderTest.csproj

# Build only
dotnet build tools/ShipRenderTest/ShipRenderTest.csproj
```

### Loading Models

**Method 1: Asset Browser**
1. Click **File > Load Folder** or use the Asset Browser panel
2. Navigate to `assets/wipeout/common/`
3. Expand PRM files to see available objects
4. Select an object and click **+ Add to Scene**

**Method 2: File Dialog**
1. Use **File > Open Files** to select multiple PRMs
2. Models appear in Asset Browser
3. Add desired objects to scene

### Working with Textures

1. Load a model with textures (e.g., watchcasing from alopt.prm)
2. Select the object in Scene panel
3. Open **View > Textures** panel
4. Inspect texture previews and diagnostic info
5. Hover over textures for detailed information

**Troubleshooting Textures:**
- If "No textures loaded": CMP file was not found or failed to load
- If "Valid Handles: 0/N": OpenGL texture creation failed
- If "Primitives with Handles: 0": Texture mapping failed
- Check logs in `build/diagnostics/wipeout_render_log.txt`

### Scene Management

**Adding Objects:**
- Use Asset Browser to add models
- Objects are automatically spaced 50 units apart
- Each object gets a unique name

**Cameras:**
- Click **+ Camera** in Scene panel
- Switch active camera from dropdown
- Edit camera properties in Camera panel

**Lights:**
- Click **+ Light** in Scene panel
- Adjust color and direction in Light panel
- Multiple lights supported

**Transform Editing:**
- Select object in Scene panel
- Use Transform panel for precise editing
- Or use Properties panel for quick adjustments

### View Menu Options

- **Scene**: Hierarchy and object management
- **Asset Browser**: PRM file browser
- **Properties**: Selected object details
- **Textures**: Texture preview and diagnostics
- **Transform**: Position/rotation/scale editing
- **Camera**: Camera configuration
- **Light Properties**: Light settings
- **Settings**: Application preferences
- **Viewport Info**: Statistics and controls

## Architecture

### Core Components

**ShipRenderTestWindow.cs**
- Main application window (OpenTK GameWindow)
- Manages rendering loop and UI integration
- Handles input and viewport interaction
- Coordinates panel updates

**Scene Management (Core/)**
- `Scene.cs`: Object, camera, and light management
- `SceneObject.cs`: Wrapper for GameObject with transform
- `SceneCamera.cs`: Camera with properties
- `DirectionalLight.cs`: Light source data

**Rendering (Rendering/)**
- `SceneRenderer.cs`: 3D object rendering
- `ViewportRenderer.cs`: Camera and projection setup
- `WorldGrid.cs`: Grid floor rendering
- `ViewGizmo.cs`: 3D orientation widget

**UI Panels (UI/)**
- `SceneHierarchyPanel.cs`: Object tree view
- `AssetBrowserPanel.cs`: File browser
- `TexturePanel.cs`: Texture inspector ⭐
- `PropertiesPanel.cs`: Object properties
- `TransformPanel.cs`: Transform editing
- `CameraPanel.cs`: Camera settings
- `LightPanel.cs`: Light controls
- `SettingsPanel.cs`: Application settings
- `ViewportInfoPanel.cs`: Statistics display

**Managers (Managers/)**
- `CameraManager.cs`: Multiple camera handling
- `LightManager.cs`: Light source management
- `AppSettingsManager.cs`: Settings persistence
- `RecentFilesManager.cs`: File history

### Rendering Pipeline

```
OnRenderFrame()
  ├── Render 3D Scene to FBO
  │   ├── Clear viewport
  │   ├── Setup camera matrices
  │   ├── Render grid and axes (if enabled)
  │   ├── For each visible object:
  │   │   ├── Calculate transform matrix
  │   │   ├── Set scale
  │   │   └── Call GameObject.Draw()
  │   └── Render view gizmo
  │
  └── Render UI Overlay
      ├── ImGui.NewFrame()
      ├── Setup dockspace
      ├── Render all panels:
      │   ├── Viewport (displays FBO texture)
      │   ├── Scene hierarchy
      │   ├── Asset browser
      │   ├── Textures ⭐
      │   ├── Properties
      │   ├── Transform
      │   ├── Camera
      │   ├── Lights
      │   └── Settings
      └── ImGui.Render()
```

### Dependencies

- **OpenTK 4.x**: OpenGL bindings and windowing
- **ImGui.NET 1.90.5.1**: Immediate mode UI
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **Microsoft.Extensions.Logging**: Logging framework

## Technical Details

### Texture Loading Pipeline

1. **CMP File Loading** (`CmpImageLoader.cs`)
   - Read compressed archive header
   - Parse image count and sizes
   - LZSS decompress entire archive
   - Split into individual TIM images

2. **TIM Parsing** (`TimImageLoader.cs`)
   - Decode TIM header (type, dimensions)
   - Load palette (if paletted format)
   - Convert to RGBA8888 format
   - Handle transparency bit

3. **Texture Creation** (`TextureManager.cs`)
   - Create OpenGL texture objects
   - Upload pixel data
   - Cache texture handles
   - Map to primitive TextureIds

4. **Primitive Mapping** (`GameObject.cs`)
   - For each FT3/FT4/GT3/GT4 primitive
   - Assign TextureHandle from array
   - Normalize UVs by texture dimensions
   - Store in UVsF array for rendering

### Special Handling

**allsh.cmp Workaround:**
- TIM 11 is a duplicate of TIM 10 (exhaust texture)
- Replaced with transparent dummy to prevent overlap
- Only applied to allsh.cmp specifically

**Shadow Textures:**
- Ships 0-7 use shad1.tim through shad4.tim
- 2 ships per shadow texture (index >> 1 + 1)
- Loaded separately from main textures

**UV Coordinates:**
- Original integer UVs in 0-255 range
- Normalized by actual texture dimensions
- Stored as floats in 0.0-1.0 range

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `F1` | Toggle Scene panel |
| `F2` | Toggle Asset Browser |
| `F3` | Toggle Properties |
| `F4` | Toggle Transform |
| `Esc` | Close application |

## Configuration Files

**app_settings.json**
```json
{
  "UIScale": 1.0,
  "ShowViewport": true,
  "ShowTransform": true,
  "ShowProperties": true,
  "ShowTextures": true,
  "ShowAssetBrowser": true,
  "ShowGrid": true,
  "ShowAxes": true,
  "ShowGizmo": true,
  "WireframeMode": false,
  "AutoRotate": false,
  "AutoRotateAxis": 1
}
```

**recent_files.json**
```json
{
  "RecentItems": [
    {
      "FilePath": "/path/to/model.prm",
      "ObjectIndex": 0,
      "Timestamp": "2025-12-29T19:00:00Z"
    }
  ]
}
```

## Troubleshooting

### Textures Not Loading
1. Check if CMP file exists alongside PRM
2. Verify logs for "Failed to load CMP textures"
3. Ensure assets directory is correct
4. Check Textures panel diagnostic info

### Model Appears Dark
1. Check if textures loaded (Textures panel)
2. Verify light direction and intensity
3. Check if face culling is appropriate
4. Try wireframe mode to see geometry

### Performance Issues
1. Reduce UI scale in Settings
2. Close unused panels
3. Disable auto-rotate
4. Hide grid/axes if not needed

### Logs Location
- Main log: `build/diagnostics/wipeout_render_log.txt`
- Console output for immediate feedback
- LogLevel: Debug for detailed information

## Future Enhancements

- [ ] Vertex editing capabilities
- [ ] Normal visualization
- [ ] UV unwrapping display
- [ ] Primitive selection and inspection
- [ ] Export modified models
- [ ] Material editor
- [ ] Batch operations
- [ ] Model comparison view
- [ ] Animation preview (if implemented)

## Contributing

When adding new features:
1. Follow existing panel structure patterns
2. Use dependency injection for services
3. Add logging for diagnostics
4. Update settings for persistence
5. Implement IUIPanel interface for panels
6. Document in this README

## Acknowledgments

Based on asset formats and rendering logic from [wipeout-rewrite](https://github.com/phoboslab/wipeout-rewrite) by Dominic Szablewski (phoboslab).

ImGui.NET by mellinoe provides the excellent UI framework.

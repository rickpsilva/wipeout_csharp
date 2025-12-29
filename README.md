# Wipeout Rewrite - C# Port

A faithful C# port of the classic Wipeout (PlayStation, 1995) game engine, based on Dominic Szablewski's [wipeout-rewrite](https://github.com/phoboslab/wipeout-rewrite) project.

## About

This project aims to recreate the Wipeout experience using modern C# and .NET technologies while maintaining accuracy to the original PlayStation implementation. Development is guided by the proven C codebase from phoboslab's wipeout-rewrite, ensuring compatibility with the original game assets and behavior.

## Current Status

### ✅ Implemented
- **Core Engine**
  - Complete PRM (3D model) loader with primitive parsing (F3, F4, FT3, FT4, G3, G4, GT3, GT4)
  - TIM texture loader with palette support (4-bit, 8-bit, 16-bit formats)
  - CMP compressed texture archive loader with LZSS decompression
  - OpenGL rendering pipeline with texture mapping and UV normalization
  - 3D transformation system (position, rotation, scale)
  
- **Asset Pipeline**
  - Automatic CMP texture loading alongside PRM models
  - Texture handle mapping to primitives
  - Shadow texture support for ship models
  - Multi-object PRM file support

- **Tools**
  - **ShipRenderTest**: Advanced 3D model viewer and debugging tool
    - Real-time 3D visualization with free camera
    - Scene management with multiple objects
    - Texture preview panel with live inspection
    - Properties and transform editing
    - Asset browser for all game models
    - ImGui-based UI with docking support

### 🚧 In Progress
- Game logic and race mechanics
- Track rendering and collision detection
- Physics simulation
- AI systems
- Audio playback

## Features

### 3D Rendering System
- OpenGL 4.1+ rendering via OpenTK
- Real-time 3D model display with textured primitives
- Support for flat and Gouraud shading
- Proper UV coordinate mapping and texture application
- Depth testing and face culling
- Camera system (perspective and orbit controls)

### Asset Loading
- **PRM Files**: Native PlayStation model format
  - Vertex positions and normals
  - Primitive types (triangles, quads, textured variants)
  - Per-primitive and per-vertex colors
  - Object hierarchy support
- **CMP Files**: Compressed texture archives
  - LZSS decompression algorithm
  - Multiple TIM images per archive
  - Automatic texture handle assignment
- **TIM Files**: PlayStation texture format
  - 4-bit, 8-bit, and 16-bit color depths
  - Palette-based and true color modes
  - Transparency support

### Development Tools
- Comprehensive logging system with file output
- Unit test coverage for core components
- Debug visualization panels
- Model and texture inspection tools

## Requirements

- .NET 8.0 SDK or later
- Linux (tested on Ubuntu/Debian) or Windows
- OpenGL 4.1+ compatible GPU
- Original Wipeout game assets (not included)

## Building

```bash
# Build the project
dotnet build

```

## Running

```bash
# Run the main game
./run.sh

# Run the ShipRenderTest tool
./test-ship-render.sh

# Or use dotnet
dotnet run --project tools/ShipRenderTest/ShipRenderTest.csproj
```

## Testing

```bash
# Run all tests
./test.sh

# Run with coverage report
./test-coverage.sh
```

## Project Structure

```
wipeout_csharp/
├── src/
│   ├── Core/
│   │   ├── Entities/          # Game objects (GameObject, Ship)
│   │   └── Graphics/          # Model loading (ModelLoader, Mesh, Primitives)
│   ├── Infrastructure/
│   │   ├── Assets/            # Asset loaders (CMP, TIM)
│   │   └── Graphics/          # Rendering (GLRenderer, TextureManager)
│   ├── Factory/               # Object creation
│   └── Presentation/          # UI and menus
├── tools/
│   └── ShipRenderTest/        # 3D model viewer and debugger
│       ├── Core/              # Scene management
│       ├── Managers/          # Settings, lights, camera
│       ├── Rendering/         # Viewport, grid, gizmo
│       └── UI/                # ImGui panels
├── wipeout_csharp.Tests/     # Unit tests
└── assets/
    └── wipeout/               # Game assets (not included in repo)
        ├── common/            # Models (PRM) and textures (CMP)
        └── textures/          # Additional textures
```

## Asset Organization

The project expects Wipeout assets in the following structure:

```
assets/wipeout/
├── common/
│   ├── allsh.prm              # Ship models (8 ships)
│   ├── allsh.cmp              # Ship textures (51 textures)
│   ├── alopt.prm              # UI/menu objects
│   ├── alopt.cmp              # UI textures
│   └── ...
└── textures/
    ├── shad1.tim - shad4.tim  # Shadow textures
    └── ...
```

## Tools

### ShipRenderTest
A comprehensive 3D model viewer and debugging tool for inspecting game assets.

**Features:**
- Load and view any PRM model from the game
- Real-time 3D visualization with orbit camera
- Multi-object scene management
- Texture inspection with preview thumbnails
- Properties panel showing model statistics
- Transform editing (position, rotation, scale)
- Asset browser with search functionality
- Settings persistence

See [tools/ShipRenderTest/README.md](tools/ShipRenderTest/README.md) for detailed documentation.

## Documentation

- [ShipRenderTest Tool](tools/ShipRenderTest/README.md) - 3D model viewer documentation
- `docs/` - Additional technical documentation (to be organized)

## Key Technical Details

### Model Loading Pipeline
1. **PRM Parsing**: Read binary model file, extract vertices, normals, and primitives
2. **CMP Loading**: Decompress texture archive, create OpenGL textures
3. **Texture Mapping**: Assign texture handles to primitives based on TextureId
4. **UV Normalization**: Convert integer UV coordinates to normalized 0-1 range

### Rendering Order
Following the original C implementation:
1. GT3 (Gouraud textured triangles)
2. FT3 (Flat textured triangles)
3. FT4 (Flat textured quads)
4. G3 (Gouraud shaded triangles)
5. F3 (Flat colored triangles)
6. F4 (Flat colored quads)

### Special Handling
- **TIM 11 Workaround**: For allsh.cmp, TIM 11 is a duplicate of TIM 10 and is replaced with a transparent texture to prevent rendering artifacts
- **Shadow Textures**: Ships 0-7 use shad1.tim through shad4.tim (2 ships per shadow texture)
- **Face Culling**: Disabled for ship rendering to match original behavior

## Acknowledgments

This project is based on [wipeout-rewrite](https://github.com/phoboslab/wipeout-rewrite) by Dominic Szablewski (phoboslab), which provides an excellent reference implementation of the Wipeout engine in C. Many implementation details, algorithms, and approaches are directly ported from that codebase.

Original Wipeout game © 1995 Psygnosis Limited.

## License

See the [original project license](https://github.com/phoboslab/wipeout-rewrite/blob/master/LICENSE).


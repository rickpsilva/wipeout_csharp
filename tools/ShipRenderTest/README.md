# Ship Render Test & 3D Model Editor

Advanced tool for debugging and editing 3D ship models with a visual UI overlay.

## Features

### 3D Rendering
- Real-time 3D ship model visualization using OpenGL
- Perspective camera with full mouse control
- Automatic rotation mode (F key toggle)
- Camera controls:
  - Mouse drag: Rotate around model
  - Mouse wheel: Zoom in/out
  - Right-click drag: Pan camera

### Visual UI (ImGui Overlay)
- **Toolbar**: 
  - "Load Model" button to toggle model browser
  - "Reset Camera" button to restore default view
  
- **Model Browser Panel**:
  - Browse all 171+ available PRM (3D model) files
  - Click to select and load models
  - Highlighted selection indicator
  
- **Properties Panel**:
  - Display file information (name, size, path)
  - Show model statistics (polygons, textures, vertices)

- **Textures Panel**:
  - Visual preview of all textures used by the selected model
  - Adjustable preview size (64-256 pixels)
  - Displays texture ID, dimensions and aspect ratio
  - Hover over textures for detailed information
  - Grid layout automatically adjusts to window size
  
- **Rendering Options**:
  - Auto-rotate checkbox for continuous rotation

## Keyboard Controls

| Key | Action |
|-----|--------|
| `F` | Toggle auto-rotate mode |
| `3` | Toggle Y-axis rotation |
| `Space` | Next test configuration |
| `R` | Reset to initial state |
| `1`-`9` | Switch between test models |
| `Escape` | Close application |

## Running the Tool

```bash
# Using the test script
./test-ship-render.sh

# Or using dotnet directly
dotnet run --project tools/ShipRenderTest/ShipRenderTest.csproj
```

## Architecture

### Components

**ImGuiController.cs** - ImGui/OpenGL integration
- Shader compilation and font atlas generation
- GL state management for proper rendering
- BeginFrame/EndFrame lifecycle management

**ModelBrowser.cs** - Model management
- Scans for available PRM files
- Tracks selected model
- Returns model statistics

**ModelFileDialog.cs** - File discovery
- Locates PRM files in `wipeout/common/` directory
- Returns file paths and names

**ShipRenderTestWindow.cs** - Main application window
- OpenTK game window with 3D rendering
- ImGui overlay integration
- Camera and model control

### Rendering Pipeline

```
OnRenderFrame()
  1. Clear and setup 3D view
  2. Render 3D ship model
  3. ImGuiController.BeginFrame()
  4. Build UI (RenderUI method)
  5. ImGuiController.EndFrame()  <- Renders ImGui overlay
  6. SwapBuffers()
```

## Dependencies

- OpenTK 4.x (3D graphics)
- ImGui.NET 1.90.5.1 (UI overlay)
- OpenGL 4.1+ (Graphics backend)

## Known Limitations

- PRM file statistics are placeholders (show N/A)
- Model transformations not yet implemented
- ImGui input overlaps with game controls

## Build & Compilation

```bash
cd /home/rick/workspace/wipeout_csharp
dotnet build tools/ShipRenderTest/ShipRenderTest.csproj
```
- Contagem de primitivas
- Posição e ângulo da ship
- Escala atual

## Visualização

- **Cruz vermelha**: Centro da tela
- **Marcador verde**: Posição da ship
- **Fundo azul escuro**: Para contraste

## Identificação de Problemas

### Ship não aparece
- Verifique os logs de vértices - se estão em escala apropriada
- Use `+`/`-` para ajustar escala em tempo real
- Use setas para mover e encontrar a ship
- Pressione `R` para rodar e ver se há geometria

### Ship muito pequena
- Pressione `SPACE` para testar escalas maiores
- Use `+` para aumentar gradualmente

### Ship muito grande
- Pressione `SPACE` para testar escalas menores
- Use `-` para diminuir gradualmente

## Integração

Depois de encontrar a configuração ideal:
1. Note a escala e posição no log
2. Atualize `ShipPreview.Initialize()` com esses valores
3. Atualize `ModelLoader.CreateMockShipModelScaled()` se necessário

## Arquitetura

```
tools/ShipRenderTest/
├── Program.cs              # Entry point
├── ShipRenderTest.csproj   # Project file
└── README.md               # This file

src/Tools/
└── ShipRenderTest.cs       # Main test window
```

## Próximos Passos

Esta ferramenta é o primeiro passo para o **WipeoutStudio** proposto em `docs/ARCHITECTURE_IMPROVEMENTS.md`.

Features futuras:
- Múltiplas ships lado a lado
- Comparação de modelos (mock vs PRM real)
- Editor de vértices em tempo real
- Export de configurações
- Visualização de normals e UVs

# Guia de Desenvolvimento - WipeoutRewrite C#

## Setup Inicial

### Requisitos

```bash
# .NET SDK 8.0
dotnet --version  # Deve mostrar 8.x.x

# FFmpeg (para extração de frames do vídeo)
ffmpeg -version

# Linux packages (Ubuntu/Debian)
sudo apt-get install libgl1-mesa-dev libglu1-mesa-dev
```

### Build & Run

```bash
# Build
./build.sh
# ou
dotnet build

# Run
./run.sh
# ou
dotnet run
```

## Estrutura de Classes

### Hierarquia Principal

```
GameWindow (OpenTK)
    ↓
Game (gerencia estados e rendering)
    ├─ GLRenderer (rendering OpenGL)
    ├─ IntroVideoPlayer (vídeo intro)
    ├─ GameState (estado do jogo)
    └─ InputManager (input do jogador)
```

### Diagramas de Classe

#### GLRenderer

```csharp
class GLRenderer {
    // Dados
    private int _shaderProgram;
    private int _vbo, _vao;
    private int _atlasTexture, _spriteTexture;
    private float[] _vertexBuffer;
    private int _trisLen;
    
    // Inicialização
    public void Init(int width, int height)
    public void UpdateScreenSize(int width, int height)
    
    // Rendering
    public void BeginFrame()
    public void PushSprite(x, y, w, h, color)
    public void PushTri(p1, uv1, c1, p2, uv2, c2, p3, uv3, c3)
    public void EndFrame()
    public void Flush()
    
    // Vídeo
    public void RenderVideoFrame(textureId, videoW, videoH, windowW, windowH)
    
    // Texturas
    public void LoadSpriteTexture(path)
}
```

#### IntroVideoPlayer

```csharp
class IntroVideoPlayer : IDisposable {
    // Dados
    private int _textureId;
    private List<byte[]> _frames;
    private int _videoWidth, _videoHeight;
    private int _currentFrameIndex;
    private double _frameRate;
    private bool _isPlaying;
    
    // Métodos públicos
    public IntroVideoPlayer(string videoPath)
    public void Play()
    public void Skip()
    public void Update()
    public int GetTextureId()
    public int GetWidth()
    public int GetHeight()
    public bool IsPlaying { get; }
    
    // Métodos privados
    private void LoadAllFrames(string videoPath)
    public void Dispose()
}
```

#### Game

```csharp
class Game : GameWindow {
    // Componentes
    private GLRenderer _renderer;
    private IntroVideoPlayer _introPlayer;
    private GameState _gameState;
    
    // Ciclo de vida
    protected override void OnLoad()
    protected override void OnUpdateFrame(FrameEventArgs args)
    protected override void OnRenderFrame(FrameEventArgs args)
    protected override void OnResize(ResizeEventArgs e)
    protected override void OnUnload()
}
```

## Como Adicionar...

### Um Novo Modo de Jogo

1. **Adicionar ao enum:**
```csharp
// GameState.cs
public enum GameMode {
    Intro,
    Menu,
    Race,
    Pause,      // ← NOVO
    GameOver    // ← NOVO
}
```

2. **Adicionar lógica no Game.cs:**
```csharp
// OnRenderFrame()
else if (_gameState?.CurrentMode == GameMode.Pause) {
    _renderer.BeginFrame();
    // Renderizar menu de pausa
    _renderer.EndFrame();
}
```

### Uma Nova Ação de Input

1. **Adicionar ao enum:**
```csharp
// Input.cs
public enum GameAction {
    // ... existentes
    FireWeapon,  // ← NOVO
    UseBoost     // ← NOVO
}
```

2. **Mapear teclas:**
```csharp
// InputManager.cs - Initialize()
_actionKeys[GameAction.FireWeapon] = new[] { Keys.Space };
_actionKeys[GameAction.UseBoost] = new[] { Keys.LeftShift };
```

3. **Usar no jogo:**
```csharp
// Game.cs - OnUpdateFrame()
if (InputManager.IsActionPressed(GameAction.FireWeapon, KeyboardState)) {
    // Disparar arma
}
```

### Um Novo Sprite

1. **Carregar textura:**
```csharp
// Game.cs - OnLoad()
_renderer.LoadSpriteTexture("assets/meu_sprite.png");
```

2. **Desenhar:**
```csharp
// Game.cs - OnRenderFrame()
_renderer.BeginFrame();
_renderer.PushSprite(x, y, width, height, new Vector4(1, 1, 1, 1));
_renderer.EndFrame();
```

### Um Shader Customizado

1. **Criar fonte do shader:**
```csharp
private const string MyFragmentShader = @"
    #version 330 core
    in vec2 v_uv;
    out vec4 FragColor;
    uniform sampler2D texture0;
    
    void main() {
        vec4 color = texture(texture0, v_uv);
        // Efeito: inverter cores
        FragColor = vec4(1.0 - color.rgb, color.a);
    }
";
```

2. **Compilar e usar:**
```csharp
int program = CreateProgram(VertexShaderSource, MyFragmentShader);
GL.UseProgram(program);
```

## Debugging

### Ativar Logs de OpenGL

```csharp
// Game.cs - OnLoad()
GL.Enable(EnableCap.DebugOutput);
GL.DebugMessageCallback((source, type, id, severity, length, message, param) => {
    Console.WriteLine($"GL Debug: {message}");
}, IntPtr.Zero);
```

### Verificar Erros OpenGL

```csharp
void CheckGLError(string location) {
    ErrorCode error = GL.GetError();
    if (error != ErrorCode.NoError) {
        Console.WriteLine($"OpenGL Error at {location}: {error}");
    }
}

// Usar após chamadas GL importantes
GL.DrawArrays(...);
CheckGLError("DrawArrays");
```

### Debug de Vídeo

```csharp
// IntroVideoPlayer.cs - Update()
Console.WriteLine($"Frame {_currentFrameIndex}/{_frames.Count} - TextureID: {_textureId}");
```

### Ver Estatísticas de Rendering

```csharp
// GLRenderer.cs - Flush()
Console.WriteLine($"Flush: {_trisLen} triangles, {_trisLen * 3} vertices");
```

## Patterns & Best Practices

### 1. Batching Pattern

**Problema:** Muitas chamadas OpenGL são lentas

**Solução:** Acumular geometria e enviar de uma vez
```csharp
BeginFrame();
for (sprite in sprites) {
    PushSprite(sprite);  // Acumula no buffer
}
EndFrame();  // Envia tudo de uma vez
```

### 2. Resource Management

**Usar `IDisposable` para recursos OpenGL:**
```csharp
class MyRenderer : IDisposable {
    private int _texture;
    
    public void Init() {
        _texture = GL.GenTexture();
    }
    
    public void Dispose() {
        if (_texture != 0) {
            GL.DeleteTexture(_texture);
            _texture = 0;
        }
    }
}
```

### 3. State Management

**Evitar mudanças de estado desnecessárias:**
```csharp
// ❌ Ruim
foreach (sprite in sprites) {
    GL.BindTexture(TextureTarget.Texture2D, sprite.texture);
    DrawSprite(sprite);
}

// ✅ Bom
var spritesByTexture = GroupByTexture(sprites);
foreach (var group in spritesByTexture) {
    GL.BindTexture(TextureTarget.Texture2D, group.Key);
    foreach (sprite in group.Value) {
        DrawSprite(sprite);
    }
}
```

### 4. Error Handling

**Sempre validar recursos críticos:**
```csharp
public void LoadTexture(string path) {
    if (!File.Exists(path)) {
        throw new FileNotFoundException($"Texture not found: {path}");
    }
    
    try {
        // Carregar textura
    } catch (Exception ex) {
        Console.WriteLine($"Failed to load texture {path}: {ex.Message}");
        // Usar textura fallback
        UseDefaultTexture();
    }
}
```

## Performance Tips

### 1. Minimize State Changes

```csharp
// Agrupar por textura
// Agrupar por shader
// Agrupar por blend mode
```

### 2. Use VBOs Eficientemente

```csharp
// Reutilizar buffer existente
GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, size, data);

// Em vez de criar novo
// GL.BufferData(...) ← mais lento
```

### 3. Batch Sprites

```csharp
// ✅ Bom: 100 sprites = 1 draw call
BeginFrame();
for (i = 0; i < 100; i++) {
    PushSprite(...);
}
EndFrame();

// ❌ Ruim: 100 sprites = 100 draw calls
for (i = 0; i < 100; i++) {
    BeginFrame();
    PushSprite(...);
    EndFrame();
}
```

### 4. Profile com Timestamps

```csharp
var sw = Stopwatch.StartNew();
Render();
sw.Stop();
Console.WriteLine($"Render time: {sw.ElapsedMilliseconds}ms");
```

## Testing

### Manual Testing

```bash
# Teste básico
dotnet run

# Teste com fullscreen
# No jogo, pressionar F11

# Teste de input
# Usar W/A/S/D/Q/E e verificar movimento
```

### Unit Testing (Futuro)

```csharp
[Test]
public void TestInputMapping() {
    var manager = new InputManager();
    manager.Initialize();
    
    // Simular tecla W pressionada
    var state = CreateMockKeyboardState(Keys.W);
    Assert.IsTrue(manager.IsActionDown(GameAction.Accelerate, state));
}
```

## Troubleshooting Comum

### "Texture não aparece"

1. Verificar se arquivo existe
2. Verificar se textura foi bound (`GL.BindTexture`)
3. Verificar UVs (devem estar entre 0-1)
4. Verificar cor (deve ser branco 1,1,1,1)

### "Performance ruim"

1. Verificar número de draw calls (deveria ser baixo)
2. Verificar tamanho das texturas (reduzir se necessário)
3. Usar profiler para encontrar gargalos
4. Verificar se VSync está ativo

### "Crash ao resize"

1. Verificar se `GL.Viewport` é atualizado
2. Verificar se matrizes de projeção são recalculadas
3. Verificar se buffers são recriados se necessário

### "Vídeo não carrega"

1. Verificar se FFmpeg está instalado
2. Verificar se `intro.mpeg` existe em `assets/`
3. Verificar logs no console
4. Verificar espaço em disco (frames temporários)

## Convenções de Código

### Naming

```csharp
// Classes: PascalCase
public class GLRenderer { }

// Métodos: PascalCase
public void DrawSprite() { }

// Variáveis privadas: _camelCase
private int _textureId;

// Variáveis públicas: PascalCase
public int TextureId { get; }

// Constantes: UPPER_CASE
private const int MAX_SPRITES = 2048;
```

### Organização de Arquivo

```csharp
// 1. Usings
using System;
using OpenTK;

// 2. Namespace
namespace WipeoutRewrite {
    
    // 3. Classe
    public class MyClass {
        
        // 4. Campos privados
        private int _field;
        
        // 5. Propriedades públicas
        public int Property { get; }
        
        // 6. Construtores
        public MyClass() { }
        
        // 7. Métodos públicos
        public void Method() { }
        
        // 8. Métodos privados
        private void PrivateMethod() { }
    }
}
```

## Recursos Adicionais

- **OpenTK Learn:** https://opentk.net/learn/
- **LearnOpenGL:** https://learnopengl.com/
- **C# Coding Guidelines:** https://docs.microsoft.com/en-us/dotnet/csharp/
- **Game Programming Patterns:** https://gameprogrammingpatterns.com/

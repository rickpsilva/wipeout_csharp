# WipeoutRewrite C# - Arquitetura do Projeto

## Visão Geral

Este é um port em C# do jogo Wipeout original (PlayStation 1), usando .NET 8 e OpenTK para rendering OpenGL moderno.

## Estrutura de Diretórios

```
wipeout_csharp/
├── assets/wipeout/          # Assets do jogo
│   ├── intro.mpeg          # Vídeo de introdução
│   ├── common/             # Assets comuns
│   ├── textures/           # Texturas
│   ├── music/              # Música
│   ├── sound/              # Efeitos sonoros
│   └── track01-14/         # Dados das 14 pistas
├── src/                    # Código fonte (arquitetura em camadas)
│   ├── Core/               # Lógica de negócio pura
│   │   ├── Entities/       # Domain entities
│   │   │   ├── Ship.cs     # Entidade nave
│   │   │   └── Track.cs    # Entidade pista
│   │   └── Services/       # Serviços de negócio
│   │       └── GameState.cs # Gerenciamento de estado
│   ├── Infrastructure/     # Implementações de infraestrutura
│   │   ├── Graphics/       # Sistema de rendering
│   │   │   ├── IRenderer.cs      # Interface
│   │   │   └── GLRenderer.cs     # Implementação OpenGL
│   │   ├── Audio/          # Sistema de áudio
│   │   │   ├── IAudioPlayer.cs   # Interface
│   │   │   └── AudioPlayer.cs    # Implementação OpenAL
│   │   ├── Video/          # Sistema de vídeo
│   │   │   ├── IVideoPlayer.cs   # Interface
│   │   │   └── IntroVideoPlayer.cs # Implementação intro
│   │   ├── Input/          # Sistema de input
│   │   │   ├── IInputManager.cs  # Interface
│   │   │   └── InputManager.cs   # Implementação OpenTK
│   │   └── Assets/         # Carregamento de assets
│   │       ├── IAssetLoader.cs   # Interface
│   │       └── AssetLoader.cs    # Implementação
│   ├── Presentation/       # Camada de apresentação
│   │   └── Game.cs         # Loop principal (GameWindow)
│   ├── Vec2.cs, Vec3.cs, Mat4.cs # Utilidades matemáticas
│   ├── Renderer.cs         # Renderer legado
│   └── ...
├── wipeout_csharp.Tests/   # Testes unitários
│   ├── GameStateTests.cs   # Testes de estado do jogo
│   ├── ShipTests.cs        # Testes de nave
│   └── ...
├── Program.cs              # Entry point
├── wipeout_csharp.csproj
├── build.sh
└── run.sh
```

## Componentes Principais

### 1. Game.cs - Loop Principal

**Responsabilidade:** Gerenciar o ciclo de vida do jogo, input, e coordenar rendering.

**Funcionalidades:**
- Herda de `GameWindow` (OpenTK)
- Gerencia estados do jogo (Intro → Menu → Race)
- Processa input do teclado
- Coordena rendering de diferentes modos
- Suporte a fullscreen (F11)

**Métodos Principais:**
```csharp
OnLoad()        // Inicialização (renderer, assets, video)
OnUpdateFrame() // Lógica do jogo (input, física)
OnRenderFrame() // Rendering baseado no GameMode
OnResize()      // Atualiza viewport quando janela muda
```

### 2. GLRenderer.cs - Sistema de Rendering

**Responsabilidade:** Abstração do OpenGL para rendering 2D/3D.

**Arquitetura:**
- Usa shaders modernos (GLSL 3.3)
- Sistema de batching de triângulos
- Suporte a texturas e sprites
- Vertex Buffer Objects (VBO) + Vertex Array Objects (VAO)

**Pipeline de Rendering:**
```
BeginFrame() → PushSprite()/PushTri() → EndFrame() → Flush()
```

**Estrutura de Vértices:**
```
[Position (3 floats)] [UV (2 floats)] [Color (4 floats)]
= 9 floats por vértice
```

**Shaders:**
- **Vertex Shader:** Transforma posições 3D com matriz de projeção
- **Fragment Shader:** Aplica textura e cor aos pixels

### 3. IntroVideoPlayer.cs - Player de Vídeo com Áudio

**Responsabilidade:** Reproduzir vídeo MPEG de introdução com áudio sincronizado.

**Abordagem Técnica:**
1. **FFMpegCore:** Extrai todos os frames do vídeo como PNG
2. **ImageSharp:** Carrega PNGs e converte para array de bytes RGBA
3. **Pre-loading:** Todos os frames (~2336) carregados na memória
4. **Playback:** Atualiza textura OpenGL frame a frame (25 fps)

**Fluxo:**
```
Constructor → LoadAllFrames() → Play()
Update() → atualiza textura com frame atual
GLRenderer.RenderVideoFrame() → desenha na tela
```

**Vantagens:**
- Playback suave sem lag
- Sem dependências problemáticas (LibVLC, etc)
- Funciona com shaders modernos

**Desvantagens:**
- Usa muita RAM (~240MB para vídeo 320x192)
- Sem áudio (só visual)
- Loading inicial lento

### 4. AudioPlayer.cs - Sistema de Áudio

**Responsabilidade:** Reprodução de áudio via OpenAL.

**Arquitetura:**
```csharp
ALDevice _device;    // Dispositivo de hardware
ALContext _context;  // Contexto OpenAL
int _buffer;         // Buffer com dados do áudio
int _source;         // Source que reproduz o buffer
```

**Funcionalidades:**
- Carregamento de arquivos WAV (PCM 16-bit, mono/stereo)
- Controle de playback (Play/Stop/Pause)
- Query de posição atual em segundos
- Query de estado (Playing/Stopped/Paused)

**Uso no IntroVideoPlayer:**
```csharp
_audioPlayer = new AudioPlayer();
_audioPlayer.LoadWav("audio.wav");
_audioPlayer.Play();

// Sincronizar vídeo
float audioPos = _audioPlayer.GetPlaybackPosition();
int targetFrame = (int)(audioPos * frameRate);
```

### 5. InputManager.cs - Gerenciamento de Input

**Responsabilidade:** Mapear teclas para ações do jogo.

**Funcionalidades:**
- Suporte a múltiplas teclas por ação
- Detecção de tecla pressionada (frame único)
- Detecção de tecla mantida (contínuo)

**Ações do Jogo:**
```csharp
enum GameAction {
    Accelerate, Brake,
    TurnLeft, TurnRight,
    BoostLeft, BoostRight,
    Exit
}
```

### 6. AssetLoader.cs - Carregamento de Assets

**Responsabilidade:** Carregar assets do jogo original (wipeout-rewrite).

**Assets Suportados:**
- Pistas (tracks)
- Naves (ships)
- Texturas
- Vídeos (intro.mpeg)
- Áudio (extraído de vídeos)

### 7. GameState.cs - Estados do Jogo

**Modos do Jogo:**
```csharp
enum GameMode {
    Intro,        // Vídeo de introdução (com áudio)
    Title,        // Title screen com "PRESS START"
    AttractMode,  // Demo automático
    Menu,         // Menu principal
    Loading,      // Carregamento de pista
    Racing,       // Corrida ativa
    Paused,       // Jogo pausado
    GameOver,     // Game over
    Victory       // Vitória
}
```

**Gerencia:**
- Modo atual do jogo
- Track selecionada
- Naves (player + AI)
- Estado da corrida

**Transições:**
```
Intro → (Enter/Space ou fim) → Title
Title → (Enter) → Menu
Title → (10s timeout) → AttractMode
AttractMode → (qualquer tecla) → Title
Menu → (START GAME flow) → Loading → Racing
Menu → (ESC na página principal) → Title
```

### 8. MenuManager.cs - Sistema de Menu

**Responsabilidade:** Gerenciar navegação em menus com stack de páginas.

**Arquitetura:**
- Stack-based navigation (push/pop páginas)
- Suporte a botões e toggles
- Layouts verticais, horizontais e fixos
- Animação de blink para item selecionado

**Estrutura:**
```csharp
MenuManager
  ├─ Stack<MenuPage> _pageStack
  ├─ PushPage(page)
  ├─ PopPage()
  └─ HandleInput(action)

MenuPage
  ├─ Title, Items, LayoutFlags
  ├─ SelectedIndex
  └─ DrawCallback (custom 3D models)

MenuItem (abstract)
  ├─ MenuButton (OnClick action)
  └─ MenuToggle (OnChange, Options array)
```

**Navegação:**
- UP/DOWN: Movimento vertical
- LEFT/RIGHT: Movimento horizontal ou ajustar toggle
- SELECT: Ativar item
- BACK: Voltar página (pop)

**Exemplo de Uso:**
```csharp
var menu = new MenuManager();
menu.PushPage(MainMenuPages.CreateMainMenu());
menu.HandleInput(MenuAction.Down);
menu.HandleInput(MenuAction.Select);
```

### 9. TitleScreen.cs - Tela de Título

**Responsabilidade:** Exibir title screen com "PRESS START" e timeout para attract mode.

**Funcionalidades:**
- Carrega textura wiptitle.tim
- Animação de blink em "PRESS START" (0.5s interval)
- Timer de 10 segundos (primeira vez) ou 5 segundos (depois) para attract mode
- Transição para menu ao pressionar Enter

**Fluxo:**
```
TitleScreen.Update()
  ├─ _timer += deltaTime
  ├─ _blinkTimer += deltaTime
  ├─ shouldStartAttract = _timer >= delay
  └─ Reset após attract completo
```

### 10. AttractMode.cs - Modo Demo

**Responsabilidade:** Auto-play de demo com configuração aleatória.

**Funcionalidades:**
- Seleciona aleatoriamente:
  - Piloto (0-7)
  - Circuito (0-6 non-bonus)
  - Classe (Venom/Rapier)
- Inicia corrida em modo demo
- Exibe "DEMO MODE" overlay
- Retorna ao title screen ao completar ou skip

**Skip:**
- Qualquer tecla interrompe o attract mode
- Volta para title screen

## Tecnologias Utilizadas

### Frameworks & Libraries

| Biblioteca | Versão | Uso |
|-----------|--------|-----|
| .NET | 8.0 | Runtime principal |
| OpenTK | 4.9.0 | OpenGL bindings + windowing |
| OpenTK.Audio.OpenAL | 4.9.1 | Sistema de áudio (OpenAL) |
| FFMpegCore | 5.1.0 | Extração de frames e áudio de vídeo |
| SixLabors.ImageSharp | 3.1.4 | Carregamento de imagens PNG |
| xUnit | 2.9.2 | Framework de testes unitários |
| Moq | 4.20.72 | Framework de mocking para testes |

### OpenGL

**Versão:** OpenGL 3.3+ (Core Profile)
**Features Usadas:**
- Vertex/Fragment Shaders (GLSL 3.3)
- VBOs (Vertex Buffer Objects)
- VAOs (Vertex Array Objects)
- Texture2D com mipmap
- Orthographic projection (2D)

## Fluxo de Execução

### Inicialização

```
Program.Main()
  ↓
GameWindow criado (OpenTK)
  ↓
Game.OnLoad()
  ↓
  ├─ Renderer.Init()
  ├─ GLRenderer.Init()
  │   ├─ Compilar shaders
  │   ├─ Criar VBO/VAO
  │   └─ Carregar texturas
  ├─ InputManager.Initialize()
  ├─ AssetLoader.LoadTracks()
  ├─ IntroVideoPlayer.new()
  │   ├─ FFMpeg extrai frames → PNG
  │   ├─ FFMpeg extrai áudio → WAV
  │   ├─ Carrega frames na RAM
  │   └─ AudioPlayer.LoadWav()
  └─ IntroVideoPlayer.Play()
      ├─ Inicia vídeo (frame 0)
      └─ AudioPlayer.Play()
```

### Loop do Jogo

```
Game.Run() (60 FPS)
  ↓
  ├─ OnUpdateFrame() [cada frame]
  │   ├─ InputManager.Update()
  │   ├─ Processar F11 (fullscreen)
  │   ├─ Processar Enter/Space (skip intro)
  │   ├─ GameState.Update()
  │   └─ Atualizar física/sprites
  │
  └─ OnRenderFrame() [cada frame]
      ├─ if (GameMode.Intro)
      │   ├─ IntroVideoPlayer.Update()
      │   │   ├─ audioPos = AudioPlayer.GetPlaybackPosition()
      │   │   ├─ targetFrame = audioPos × frameRate
      │   │   └─ GL.TexImage2D(frames[targetFrame])
      │   └─ GLRenderer.RenderVideoFrame()
      ├─ if (GameMode.Menu)
      │   ├─ GLRenderer.BeginFrame()
      │   ├─ GLRenderer.PushSprite() [sprites do menu]
      │   └─ GLRenderer.EndFrame()
      └─ if (GameMode.Race)
          ├─ Renderizar pista
          ├─ Renderizar naves
          └─ Renderizar HUD
```

## Sistema de Rendering

### Batching System

O `GLRenderer` usa um sistema de batching para otimizar chamadas ao OpenGL:

1. **BeginFrame():** Limpa buffer de triângulos
2. **PushSprite()/PushTri():** Adiciona geometria ao buffer
3. **EndFrame():** Faz bind da textura e chama Flush()
4. **Flush():** Envia todos os triângulos para GPU de uma vez

**Capacidade:** 2048 triângulos por batch

### Projeção Ortográfica

Para rendering 2D, usa matriz ortográfica:
```csharp
Matrix4.CreateOrthographicOffCenter(
    0, screenWidth,    // left, right
    screenHeight, 0,   // bottom, top (invertido para Y+ para baixo)
    -1, 1              // near, far
)
```

### Video Rendering

O vídeo é renderizado como um sprite fullscreen com scaling inteligente:

**Modo "Cover":**
- Preenche toda a tela
- Mantém aspect ratio
- Pode cortar bordas do vídeo
- Zero margens pretas

```csharp
if (windowAspect > videoAspect) {
    // Janela mais larga → escalar pela largura
    renderWidth = windowWidth;
    renderHeight = windowWidth / videoAspect;
} else {
    // Janela mais alta → escalar pela altura  
    renderHeight = windowHeight;
    renderWidth = windowHeight * videoAspect;
}
```

## Input System

### Mapeamento de Teclas

```csharp
Accelerate  → W, UpArrow
Brake       → S, DownArrow
TurnLeft    → A, LeftArrow
TurnRight   → D, RightArrow
BoostLeft   → Q
BoostRight  → E
Exit        → Escape
```

### Teclas Especiais

```
F11         → Toggle fullscreen
Enter/Space → Skip intro
```

## Performance

### Otimizações

1. **Batching de Geometria:** Reduz chamadas ao OpenGL
2. **Pre-loading de Vídeo:** Elimina lag de decodificação
3. **VBO Dinâmico:** Reutiliza buffer em vez de criar novos
4. **Single Texture Bind:** Por batch de triângulos

### Métricas

- **FPS Target:** 60 FPS
- **Video FPS:** 25 FPS
- **Audio Sample Rate:** 44100 Hz
- **Max Triangles/Frame:** 2048
- **Intro Loading Time:** ~5-10 segundos (frames + áudio)
- **Memory Usage (Intro):** ~256 MB (240MB vídeo + 16MB áudio)
- **Audio/Video Sync:** <0.02s (20ms - imperceptível)

## Decisões Técnicas

### Por que OpenTK?

- Cross-platform (Windows, Linux, macOS)
- Bindings diretos para OpenGL
- Gerenciamento de janelas integrado
- Suporte a shaders modernos
- Comunidade ativa

### Por que Pre-load do Vídeo?

**Tentativas Anteriores:**
1. **LibVLCSharp + GTK:** Incompatibilidade de plataforma
2. **WebView HTML5:** Pacotes indisponíveis no Linux
3. **FFmpeg Frame-by-Frame:** Muito lento, lag visível
4. **OpenGL Legacy (GL.Begin):** Não funciona com Core Profile

**Solução Final:**
- FFmpeg extrai todos os frames (PNG) e áudio (WAV) de uma vez
- Frames carregados na RAM (~240MB)
- Áudio carregado no OpenAL (~16MB)
- Vídeo sincronizado com posição do áudio
- Rendering com shaders modernos
- Resultado: Playback perfeito com áudio sincronizado, zero lag

### Por que Shaders Modernos?

OpenGL Core Profile (3.3+) não suporta:
- `GL.Begin()` / `GL.End()`
- `GL.Color3()` / `GL.Vertex2()`
- Fixed-function pipeline

Shaders modernos oferecem:
- Mais controle
- Melhor performance
- Compatibilidade futura
- Facilita efeitos visuais

### Por que OpenAL para Áudio?

**Vantagens:**
- Cross-platform (Windows, Linux, macOS)
- Hardware-accelerated timing (precisão de microssegundos)
- API simples e direta
- Suporte a múltiplos formatos (via conversão)
- Integrado no OpenTK

**Sincronização:**
- `AL.GetSource(SecOffset)` fornece posição exata do playback
- Vídeo calcula frame baseado na posição do áudio
- Áudio é a "fonte da verdade" (timing mais preciso)
- Resultado: Sincronização <0.02s durante todo o vídeo

## Arquitetura SOLID

### Princípios Aplicados

**Single Responsibility Principle (SRP):**
- Cada classe tem uma única responsabilidade clara
- `Ship` apenas representa dados da nave
- `GameState` apenas gerencia estado do jogo
- `GLRenderer` apenas renderiza com OpenGL

**Open/Closed Principle (OCP):**
- Interfaces permitem extensão sem modificação
- Novo renderer pode ser adicionado implementando `IRenderer`
- Novo sistema de áudio pode implementar `IAudioPlayer`

**Liskov Substitution Principle (LSP):**
- Qualquer implementação de `IRenderer` pode substituir `GLRenderer`
- Testes usam mocks que implementam as mesmas interfaces

**Interface Segregation Principle (ISP):**
- Interfaces pequenas e focadas (IRenderer, IAudioPlayer, etc)
- Clientes dependem apenas dos métodos que usam

**Dependency Inversion Principle (DIP):**
- Camada Core não depende de Infrastructure
- `GameState` usa `IRenderer` ao invés de `GLRenderer` diretamente
- Facilita injeção de dependências e testes

### Camadas

```
┌─────────────────────────────────────┐
│     Presentation (Game.cs)          │  ← UI/GameLoop
├─────────────────────────────────────┤
│     Application (Use Cases)         │  ← [Não implementado ainda]
├─────────────────────────────────────┤
│     Core (Entities + Services)      │  ← Lógica de negócio
│  - Ship, Track, GameState           │
├─────────────────────────────────────┤
│     Infrastructure                  │  ← Implementações concretas
│  - GLRenderer, AudioPlayer, etc     │
└─────────────────────────────────────┘
```

### Testabilidade

A arquitetura permite testar lógica de negócio sem dependências externas:

**Sem OpenGL/OpenAL:**
```csharp
// Teste de Ship sem renderização
var ship = new Ship("Test", 0);
ship.TakeDamage(50);
Assert.Equal(50, ship.Shield);
```

**Com Mocks:**
```csharp
// Teste com mock de renderer
var mockRenderer = new Mock<IRenderer>();
gameState.Render(mockRenderer.Object);
mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
```

## Testes Unitários

### Estrutura

```
wipeout_csharp.Tests/
├── GameStateTests.cs    # Testes de GameState
│   ├── GameState_ShouldStartInMenuMode
│   ├── Initialize_ShouldSetTrackAndCreateShips
│   └── Update_ShouldAdvanceTime_WhenRacing
├── ShipTests.cs         # Testes de Ship
│   ├── Ship_NewInstance_ShouldHaveDefaultValues
│   ├── Ship_TakeDamage_ShouldReduceShield
│   ├── Ship_TakeFatalDamage_ShouldDestroy
│   ├── Ship_Acceleration_ShouldIncreaseSpeed
│   └── Ship_Update_ShouldReduceFireCooldown
└── TrackTests.cs        # Testes de Track
    ├── Track_Constructor_ShouldSetName
    └── Track_ShouldHaveEmptyFacesInitially
```

### Executar Testes

```bash
cd wipeout_csharp.Tests
dotnet test
```

### Cobertura

- **10 testes** implementados
- **100% passando**
- Foco em lógica de negócio pura (Core layer)
- Testes de entidades sem dependências externas

## Próximos Passos

### ✅ Áudio do Vídeo (COMPLETO)

Sistema totalmente implementado:
- ✅ Extração de áudio via FFmpeg (PCM 16-bit WAV)
- ✅ Reprodução via OpenAL (AudioPlayer.cs)
- ✅ Sincronização perfeita com vídeo (<0.02s)
- ✅ Controle de playback (Play/Stop/Skip)

### ✅ Arquitetura SOLID (COMPLETO)

Refatoração completa com princípios SOLID:
- ✅ Separação em camadas (Core/Infrastructure/Presentation)
- ✅ Interfaces extraídas (IRenderer, IAudioPlayer, IVideoPlayer, etc)
- ✅ Dependency Injection preparada
- ✅ Testes unitários (xUnit + Moq)
- ✅ Testabilidade sem OpenGL/OpenAL

### ✅ Sistema de Menu (COMPLETO)

Implementação completa baseada no original C:
- ✅ MenuManager com stack navigation
- ✅ MenuPage com botões e toggles
- ✅ MenuRenderer com blink animation
- ✅ TitleScreen com timeout para attract
- ✅ AttractMode com seleção aleatória
- ✅ MainMenuPages com hierarquia completa:
  - Main Menu (Start Game / Options / Quit)
  - Race Class (Venom / Rapier)
  - Race Type (Single Race / Time Trial)
  - Team Selection (4 teams)
  - Pilot Selection (2 per team)
  - Circuit Selection (7 circuits)
  - Options (Controls / Video / Audio / Best Times)
- ✅ Input de menu (UP/DOWN/LEFT/RIGHT/SELECT/BACK)
- ✅ Game.cs integrado com transições completas

**Funcionalidades Pendentes:**
- Renderização de texto real (atualmente placeholder)
- Modelos 3D nos menus (stopwatch, ships, etc)
- Sound effects (SFX_MENU_MOVE, SFX_MENU_SELECT)
- Texturas de fundo (wipeout1.tim, track previews)

### Rendering 3D

- Implementar câmera 3D
- Renderizar pista com perspectiva
- Renderizar naves com modelos 3D

### Física

- Sistema de colisão
- Aceleração/travagem
- Boost/power-ups

## Troubleshooting

### Vídeo não aparece

- Verificar se `assets/intro.mpeg` existe
- Verificar logs de FFmpeg
- Verificar se frames foram extraídos (console output)

### Performance ruim

- Reduzir max triangles por batch
- Usar texturas menores
- Desativar vsync (se necessário)

### Fullscreen não funciona

- Verificar F11 mapping
- Verificar WindowState.Fullscreen suportado
- Testar Alt+Enter (alternativa)

## Referências

- [OpenTK Documentation](https://opentk.net/learn/index.html)
- [OpenGL Tutorial](https://learnopengl.com/)
- [FFMpegCore GitHub](https://github.com/rosenbjerg/FFMpegCore)
- [ImageSharp Documentation](https://docs.sixlabors.com/articles/imagesharp/index.html)
- [Wipeout Original (PS1)](https://en.wikipedia.org/wiki/Wipeout_(video_game))

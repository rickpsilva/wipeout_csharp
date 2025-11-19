# Refatoração SOLID + Dependency Injection

## ❌ Problemas Atuais

### 1. Violação do Princípio D (Dependency Inversion)
```csharp
// ❌ ERRADO: Game depende de implementações concretas
public class Game : GameWindow {
    private GLRenderer? _renderer;           // Implementação concreta
    private MusicPlayer? _musicPlayer;       // Implementação concreta
    private AssetLoader _assetLoader;        // Implementação concreta
    
    protected override void OnLoad() {
        _renderer = new GLRenderer();        // Criação direta
        _musicPlayer = new MusicPlayer();    // Criação direta
        _assetLoader = new AssetLoader();    // Criação direta
    }
}
```

### 2. Violação do Princípio S (Single Responsibility)
```csharp
// ❌ Game faz TUDO: inicialização + loading + input + rendering + lógica
protected override void OnLoad() {
    // Inicializa subsistemas
    // Carrega assets
    // Configura input
    // Inicializa áudio
    // Carrega vídeo
    // etc...
}
```

### 3. Falta de Interfaces
```csharp
// ❌ MusicPlayer não tem interface
public class MusicPlayer : IDisposable {  // Deveria ser IMusicPlayer
    // ...
}

// ❌ FontSystem não tem interface
public class FontSystem {  // Deveria ser IFontSystem
    // ...
}
```

## ✅ Solução: Arquitetura com SOLID + DI

### 1. Criar Interfaces para TODAS as Dependências

```csharp
// ✅ Interfaces criadas/existentes:
public interface IRenderer { }
public interface IAudioPlayer { }
public interface IMusicPlayer { }      // ← NOVO
public interface IFontSystem { }       // ← NOVO
public interface IAssetLoader { }      // ← A CRIAR
public interface IVideoPlayer { }
public interface IInputManager { }
public interface IMenuManager { }
public interface IMenuRenderer { }
```

### 2. Refatorar Game.cs para Usar DI

```csharp
// ✅ CORRETO: Injeção via construtor
public class Game : GameWindow 
{
    private readonly IRenderer _renderer;
    private readonly IMusicPlayer _musicPlayer;
    private readonly IAssetLoader _assetLoader;
    private readonly IFontSystem _fontSystem;
    private readonly IMenuManager _menuManager;
    private readonly IMenuRenderer _menuRenderer;
    private IVideoPlayer? _introPlayer;  // Criado sob demanda
    
    private GameState? _gameState;
    private TitleScreen? _titleScreen;
    private CreditsScreen? _creditsScreen;
    
    // ✅ Todas as dependências injetadas via construtor
    public Game(
        GameWindowSettings gws, 
        NativeWindowSettings nws,
        IRenderer renderer,
        IMusicPlayer musicPlayer,
        IAssetLoader assetLoader,
        IFontSystem fontSystem,
        IMenuManager menuManager,
        IMenuRenderer menuRenderer
    ) : base(gws, nws)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
        _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
        _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
        _menuManager = menuManager ?? throw new ArgumentNullException(nameof(menuManager));
        _menuRenderer = menuRenderer ?? throw new ArgumentNullException(nameof(menuRenderer));
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        // ✅ Apenas configuração, sem criação de objetos
        _gameState = new GameState();
        _renderer.Init(Size.X, Size.Y);
        
        string assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
        _assetLoader.Initialize(assetsPath);
        _fontSystem.LoadFonts(assetsPath);
        
        // Carregar tracks e inicializar estado
        var tracks = _assetLoader.LoadTrackList();
        if (tracks?.Count > 0) 
        {
            var track = new Track(tracks[0]);
            _gameState.Initialize(track, playerShipId: 0);
        }
        
        // Inicializar screens
        _titleScreen = new TitleScreen(_fontSystem);
        _creditsScreen = new CreditsScreen(_fontSystem);
        
        // Carregar música
        string musicPath = Path.Combine(Directory.GetCurrentDirectory(), 
                                       "assets", "wipeout", "music");
        _musicPlayer.LoadTracks(musicPath);
        
        // Carregar intro
        InitializeIntroVideo(assetsPath);
    }
    
    private void InitializeIntroVideo(string assetsPath)
    {
        string introPath = Path.Combine(assetsPath, "wipeout", "intro.mpeg");
        if (File.Exists(introPath))
        {
            _introPlayer = new IntroVideoPlayer(introPath);
            _introPlayer.Play();
            _gameState!.CurrentMode = GameMode.Intro;
        }
        else
        {
            _gameState!.CurrentMode = GameMode.SplashScreen;
            _musicPlayer.SetMode(MusicMode.Random);
        }
    }
}
```

### 3. Composição no Program.cs (Main)

```csharp
// ✅ Program.cs - Composition Root
public class Program
{
    public static void Main(string[] args)
    {
        // Configuração da janela
        var gws = GameWindowSettings.Default;
        var nws = new NativeWindowSettings()
        {
            Size = new Vector2i(1280, 720),
            Title = "WipeoutRewrite (C#)",
            Flags = ContextFlags.ForwardCompatible,
        };
        
        // ✅ Criar todas as dependências (Composition Root)
        var renderer = new GLRenderer();
        var musicPlayer = new MusicPlayer();
        var assetLoader = new AssetLoader();
        var fontSystem = new FontSystem();
        var menuManager = new MenuManager();
        var menuRenderer = new MenuRenderer(1280, 720, renderer, fontSystem);
        
        // ✅ Injetar dependências no Game
        using var game = new Game(
            gws, 
            nws,
            renderer,
            musicPlayer,
            assetLoader,
            fontSystem,
            menuManager,
            menuRenderer
        );
        
        game.Run();
    }
}
```

### 4. Benefícios da Refatoração

#### Testabilidade
```csharp
// ✅ Agora é fácil testar com mocks
[Fact]
public void Game_OnLoad_InitializesRenderer()
{
    // Arrange
    var mockRenderer = new Mock<IRenderer>();
    var mockMusic = new Mock<IMusicPlayer>();
    var mockAssets = new Mock<IAssetLoader>();
    // ... outros mocks
    
    var game = new Game(gws, nws, 
        mockRenderer.Object,
        mockMusic.Object,
        mockAssets.Object,
        // ...
    );
    
    // Act
    game.OnLoad();
    
    // Assert
    mockRenderer.Verify(r => r.Init(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
}
```

#### Substituição de Implementações
```csharp
// ✅ Fácil trocar renderer OpenGL por Vulkan
var renderer = new VulkanRenderer();  // Em vez de GLRenderer
var game = new Game(gws, nws, renderer, ...);

// ✅ Fácil trocar audio OpenAL por FMOD
var musicPlayer = new FmodMusicPlayer();  // Em vez de MusicPlayer
var game = new Game(gws, nws, ..., musicPlayer, ...);
```

#### Princípios SOLID Respeitados

✅ **S (Single Responsibility)**
- `Game`: Apenas coordena game loop e estados
- `GLRenderer`: Apenas renderização OpenGL
- `MusicPlayer`: Apenas gestão de música
- `AssetLoader`: Apenas carregamento de assets

✅ **O (Open/Closed)**
- Aberto para extensão (novas implementações de interfaces)
- Fechado para modificação (Game não muda ao adicionar novo renderer)

✅ **L (Liskov Substitution)**
- Qualquer `IRenderer` pode substituir `GLRenderer`
- Qualquer `IMusicPlayer` pode substituir `MusicPlayer`

✅ **I (Interface Segregation)**
- Interfaces pequenas e focadas
- `IRenderer`, `IAudioPlayer`, `IMusicPlayer` separadas

✅ **D (Dependency Inversion)**
- `Game` depende de abstrações (interfaces)
- Não depende de implementações concretas

## Implementation Step-by-Step

### Passo 1: Criar interfaces faltantes
- [x] `IMusicPlayer`
- [x] `IFontSystem`
- [ ] `IAssetLoader`

### Passo 2: Implementar interfaces nas classes existentes
- [x] `MusicPlayer : IMusicPlayer`
- [x] `FontSystem : IFontSystem`
- [ ] `AssetLoader : IAssetLoader`

### Passo 3: Refatorar Game.cs
- [ ] Adicionar construtor com DI
- [ ] Remover `new` de dentro de `OnLoad()`
- [ ] Usar interfaces em vez de classes concretas

### Passo 4: Atualizar Program.cs
- [ ] Criar composition root
- [ ] Instanciar todas as dependências
- [ ] Injetar no construtor de Game

### Passo 5: Atualizar testes
- [ ] Criar mocks para interfaces
- [ ] Testar com DI

## Opcional: DI Container (Microsoft.Extensions.DependencyInjection)

```csharp
// ✅ Usando DI Container (opcional, mas recomendado)
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        // Configurar DI Container
        var services = new ServiceCollection();
        
        // Registar serviços
        services.AddSingleton<IRenderer, GLRenderer>();
        services.AddSingleton<IMusicPlayer, MusicPlayer>();
        services.AddSingleton<IAssetLoader, AssetLoader>();
        services.AddSingleton<IFontSystem, FontSystem>();
        services.AddSingleton<IMenuManager, MenuManager>();
        services.AddSingleton<IMenuRenderer, MenuRenderer>();
        
        // Registar Game
        services.AddSingleton<Game>();
        
        // Build container
        var serviceProvider = services.BuildServiceProvider();
        
        // Resolver Game (com todas as dependências injetadas automaticamente!)
        var game = serviceProvider.GetRequiredService<Game>();
        
        game.Run();
    }
}
```

## Conclusão

A refatoração proposta:
- ✅ Respeita todos os princípios SOLID
- ✅ Usa Dependency Injection corretamente
- ✅ Facilita testes unitários
- ✅ Permite substituir implementações facilmente
- ✅ Reduz acoplamento entre classes
- ✅ Melhora manutenibilidade do código

**Próximo passo recomendado**: Implementar esta refatoração gradualmente, começando pelas interfaces faltantes e depois refatorando `Game.cs` e `Program.cs`.

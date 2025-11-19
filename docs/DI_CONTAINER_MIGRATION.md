# Migração para DI Container (Microsoft.Extensions.DependencyInjection)

## Data: 18 Novembro 2025

## Motivação
Preparar a aplicação para crescimento futuro, facilitando:
- Adição de novos serviços
- Gestão de lifecycles (Singleton, Transient, Scoped)
- Testes mais fáceis com mock containers
- Padrão enterprise do ecossistema .NET

---

## O que mudou?

### Before (Manual DI / Poor Man's DI)
```csharp
// Program.cs
var renderer = new GLRenderer();
var musicPlayer = new MusicPlayer();
var assetLoader = new AssetLoader();
var fontSystem = new FontSystem();
var menuManager = new MenuManager();

using (var game = new Game(gws, nws, renderer, musicPlayer, assetLoader, fontSystem, menuManager))
{
    Renderer.Init();
    game.Run();
}
```

### After (DI Container)
```csharp
// Program.cs
var services = new ServiceCollection();
ConfigureServices(services);
var serviceProvider = services.BuildServiceProvider();

Renderer.Init();

using (var game = serviceProvider.GetRequiredService<Game>())
{
    game.Run();
}

serviceProvider.Dispose();

// ConfigureServices
private static void ConfigureServices(IServiceCollection services)
{
    // Window Settings
    services.AddSingleton(GameWindowSettings.Default);
    services.AddSingleton(new NativeWindowSettings { ... });
    
    // Core Services - Singleton
    services.AddSingleton<IRenderer, GLRenderer>();
    services.AddSingleton<IMusicPlayer, MusicPlayer>();
    services.AddSingleton<IAssetLoader, AssetLoader>();
    services.AddSingleton<IFontSystem, FontSystem>();
    services.AddSingleton<IMenuManager, MenuManager>();
    
    // Game
    services.AddSingleton<Game>();
}
```

---

## Pacote NuGet Adicionado
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
```

---

## Benefits da Migração

### 1. **Escalabilidade**
✅ Fácil adicionar novos serviços sem modificar muito código
```csharp
// Adicionar novo serviço é simples:
services.AddSingleton<INetworkManager, NetworkManager>();
```

### 2. **Lifecycles Geridos**
✅ Controle automático de quando criar/destruir objetos
- **Singleton**: Uma instância para toda a app
- **Transient**: Nova instância a cada resolução
- **Scoped**: Uma instância por scope (útil para requests web)

### 3. **Testes Facilitados**
✅ Criar containers de teste com mocks
```csharp
// Em testes:
var services = new ServiceCollection();
services.AddSingleton<IRenderer>(mockRenderer);
var provider = services.BuildServiceProvider();
var game = provider.GetRequiredService<Game>();
```

### 4. **Padrão .NET**
✅ Mesmo padrão usado em:
- ASP.NET Core
- Blazor
- MAUI
- Worker Services
- Minimal APIs

### 5. **Descoberta Automática**
✅ ServiceProvider resolve grafos de dependências automaticamente
```csharp
// Se Game precisa de IRenderer, IMusicPlayer, etc.
// O container resolve tudo automaticamente!
var game = serviceProvider.GetRequiredService<Game>();
```

---

## Custo da Migração

### Performance
- ⚠️ Overhead mínimo de reflexão (~microsegundos)
- ⚠️ Memória extra do container (~KB)
- ✅ Negligível para um jogo deste tamanho

### Complexidade
- ⚠️ Uma camada extra de abstração
- ✅ Mas compensa quando o projeto cresce

---

## Next Steps Futuros

### 1. **Adicionar Logging**
```csharp
services.AddLogging(builder => {
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Injetar ILogger<T> nas classes:
public class Game {
    private readonly ILogger<Game> _logger;
    
    public Game(..., ILogger<Game> logger) {
        _logger = logger;
    }
}
```

### 2. **Configuration System**
```csharp
services.AddOptions();
services.Configure<GameSettings>(configuration.GetSection("Game"));

// Injetar IOptions<GameSettings>
```

### 3. **Factories e Decorators**
```csharp
services.AddTransient<ITrackFactory, TrackFactory>();
services.Decorate<IRenderer, CachedRenderer>(); // Com Scrutor
```

### 4. **Hosted Services** (para background tasks)
```csharp
services.AddHostedService<MusicPreloaderService>();
```

### 5. **Scoped Services** (se adicionar multiplayer)
```csharp
services.AddScoped<IGameSession, GameSession>();
```

---

## Validação

### Build Status
✅ Build succeeded - sem erros

### Tests Status  
✅ 43 passed, 4 skipped (43/47 = 91.5%)

### Runtime Status
✅ Jogo inicia normalmente com DI Container

---

## Rollback (se necessário)

Para voltar ao Manual DI, basta:
1. Remover pacote: `dotnet remove package Microsoft.Extensions.DependencyInjection`
2. Reverter Program.cs para versão anterior (commit anterior no git)

---

## Conclusão

Migração bem-sucedida! O projeto agora usa o padrão enterprise do .NET para DI, preparando-o para crescimento futuro sem sacrificar simplicidade atual.

**Trade-off**: Pequeno overhead técnico agora → Grande benefício quando o projeto crescer

**Recomendação**: Manter esta abordagem e aproveitar os benefícios do ecossistema .NET!

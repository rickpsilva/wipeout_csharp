# Sistema de Logging

## Visão Geral

O WipeoutRewrite usa `Microsoft.Extensions.Logging` como sistema de logging abstrato, permitindo múltiplos destinos (console, ficheiro, database) de forma flexível e configurável.

---

## Pacotes Instalados

```xml
<PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="10.0.0" />
```

---

## Configuração (Program.cs)

```csharp
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
    
    // Níveis por namespace:
    builder.AddFilter("WipeoutRewrite", LogLevel.Debug);
    builder.AddFilter("Microsoft", LogLevel.Warning);
});
```

### Níveis de Log (por prioridade)

1. **Trace** - Informação muito detalhada (debug profundo)
2. **Debug** - Informação de debug (desenvolvimento)
3. **Information** - Fluxo normal da aplicação ✅ (padrão)
4. **Warning** - Situações anormais mas recuperáveis ⚠️
5. **Error** - Erros que previnem funcionalidade ❌
6. **Critical** - Falhas críticas que param a aplicação 🔥

---

## Uso nas Classes

### 1. Injetar ILogger<T> no Construtor

```csharp
public class AssetLoader : IAssetLoader
{
    private readonly ILogger<AssetLoader> _logger;

    public AssetLoader(ILogger<AssetLoader> logger)
    {
        _logger = logger;
    }
}
```

### 2. Usar Métodos de Logging

```csharp
// Information - Fluxo normal
_logger.LogInformation("Loaded {TrackCount} music tracks (WAV)", trackCount);

// Warning - Situação anormal
_logger.LogWarning("Music directory not found: {MusicPath}", musicPath);

// Error - Erro com exceção
_logger.LogError(ex, "Error loading music tracks");

// Debug - Apenas em desenvolvimento
_logger.LogDebug("Processing track {TrackIndex} of {TotalTracks}", i, total);
```

---

## Structured Logging (Message Templates)

✅ **CORRETO** - Structured Logging (permite queries e filtros):
```csharp
_logger.LogInformation("User {UserId} loaded track {TrackName}", userId, trackName);
```

❌ **INCORRETO** - String interpolation (perde estrutura):
```csharp
_logger.LogInformation($"User {userId} loaded track {trackName}");
```

**Por quê?** Structured logging permite:
- Filtrar por UserId ou TrackName
- Análise de performance
- Dashboards e métricas
- Busca eficiente em logs

---

## Providers Disponíveis

### 1. **Console** (atual) ✅
```csharp
builder.AddConsole();
```

**Formato atual:**
```
info: WipeoutRewrite.Infrastructure.Audio.MusicPlayer[0]
      Loaded 11 music tracks (WAV)
```

### 2. **File** (futuro)
```bash
dotnet add package Serilog.Extensions.Logging.File
```

```csharp
builder.AddFile("logs/wipeout-{Date}.txt");
```

### 3. **Database** (futuro)
```bash
dotnet add package Serilog.Sinks.MSSqlServer
# ou
dotnet add package Serilog.Sinks.PostgreSQL
```

```csharp
builder.AddSerilog(new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString, "Logs")
    .CreateLogger());
```

### 4. **Application Insights** (Azure)
```bash
dotnet add package Microsoft.Extensions.Logging.ApplicationInsights
```

### 5. **Seq** (Logging Server)
```bash
dotnet add package Serilog.Sinks.Seq
```

```csharp
builder.AddSerilog(new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger());
```

---

## Configuração Avançada

### appsettings.json (futuro)

Criar `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WipeoutRewrite": "Debug",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "[yyyy-MM-dd HH:mm:ss] "
    }
  }
}
```

Carregar no Program.cs:
```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});
```

---

## Exemplos de Uso por Cenário

### Startup da Aplicação
```csharp
_logger.LogInformation("========================================");
_logger.LogInformation("Iniciando WipeoutRewrite (C#)");
_logger.LogInformation("========================================");
```

### Carregamento de Assets
```csharp
_logger.LogInformation("AssetLoader initialized with base path: {BasePath}", basePath);
_logger.LogInformation("Loaded {TrackCount} music tracks (WAV)", trackCount);
```

### Avisos
```csharp
_logger.LogWarning("Asset path does not exist: {BasePath}", basePath);
_logger.LogWarning("Music directory not found: {MusicPath}", musicPath);
```

### Erros
```csharp
try
{
    // código
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error loading text file {RelativePath}", relativePath);
    return null;
}
```

### Performance Tracking
```csharp
using (_logger.BeginScope("Loading track {TrackName}", trackName))
{
    var stopwatch = Stopwatch.StartNew();
    
    // Carregar track
    
    _logger.LogDebug("Track loaded in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
}
```

---

## Classes Atualizadas

### ✅ AssetLoader
- Construtor aceita `ILogger<AssetLoader>`
- `Console.WriteLine` → `_logger.LogInformation/LogWarning/LogError`

### ✅ MusicPlayer
- Construtor aceita `ILogger<MusicPlayer>`
- Logs estruturados com placeholders

### ✅ FontSystem
- Construtor aceita `ILogger<FontSystem>`
- Substituição de `Console.WriteLine` por `_logger.LogInformation/LogWarning/LogError`

### ✅ IntroVideoPlayer
- Construtor aceita `ILogger<IntroVideoPlayer>`
- Uso de logging para eventos de vídeo e erros

### ✅ GameState
- Construtor aceita `ILogger<GameState>`
- Logging para transições de estado e eventos importantes

### ✅ Game
- Construtor aceita `ILogger<Game>` e `ILoggerFactory`
- Logging para inicialização, ciclo principal e erros

### ✅ TitleScreen
- Construtor aceita `ILogger<TitleScreen>`
- Logging para navegação e eventos de tela

### ✅ CmpImageLoader
- Construtor aceita `ILogger<CmpImageLoader>`
- Logging para carregamento de imagens e erros

### ✅ TimImageLoader
- Construtor aceita `ILogger<TimImageLoader>`
- Logging para carregamento de imagens e erros

### ✅ Track
- Construtor aceita `ILogger<Track>` (opcional)
- Logging para carregamento de pistas e eventos

### ✅ Ship
- Construtor aceita `ILogger<Ship>` (opcional)
- Logging para inicialização e eventos de navegação

### 🔄 Pendentes (próximas)
- GLRenderer
- MenuRenderer
- CreditsScreen
- AttractMode

---

## Migração de Console.WriteLine para ILogger

### Antes:
```csharp
Console.WriteLine($"✓ Loaded {count} tracks");
Console.WriteLine($"⚠ Warning: {message}");
Console.WriteLine($"✗ Error: {ex.Message}");
```

### Depois:
```csharp
_logger.LogInformation("Loaded {TrackCount} tracks", count);
_logger.LogWarning("{Message}", message);
_logger.LogError(ex, "Error occurred");
```

---

## Testes com Logging

### Opção 1: Mock ILogger
```csharp
[Fact]
public void LoadTracks_ShouldLogInformation()
{
    var mockLogger = new Mock<ILogger<MusicPlayer>>();
    var player = new MusicPlayer(mockLogger.Object);
    
    player.LoadTracks("/path");
    
    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Loaded")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

### Opção 2: NullLogger (sem verificação)
```csharp
var player = new MusicPlayer(NullLogger<MusicPlayer>.Instance);
```

---

## Filtros e Scopes

### Filtrar por Categoria
```csharp
builder.AddFilter("WipeoutRewrite.Infrastructure", LogLevel.Trace);
builder.AddFilter("WipeoutRewrite.Presentation", LogLevel.Debug);
```

### Usar Scopes
```csharp
using (_logger.BeginScope("Game Session {SessionId}", sessionId))
{
    _logger.LogInformation("Player joined");
    _logger.LogInformation("Track loaded");
    // Todos os logs terão SessionId no contexto
}
```

---

## Log Rotation (Ficheiros)

Com Serilog:
```csharp
.WriteTo.File(
    "logs/wipeout-.txt",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7,
    fileSizeLimitBytes: 10_000_000) // 10MB
```

---

## Próximos Passos

### Curto Prazo
- ✅ Console logging implementado
- ⬜ Migrar todas as classes de `Console.WriteLine` para `ILogger`
- ⬜ Adicionar logs de performance (tempo de loading)

### Médio Prazo
- ⬜ Adicionar file logging (Serilog)
- ⬜ Configuração via appsettings.json
- ⬜ Log rotation automática

### Longo Prazo
- ⬜ Database logging para análise
- ⬜ Dashboard de monitoring (Seq, Grafana)
- ⬜ Alerts automáticos para erros críticos

---

## Benefícios Alcançados

### ✅ Flexibilidade
Trocar de console para ficheiro/database sem mudar código

### ✅ Structured Logging
Logs consultáveis e analisáveis

### ✅ Níveis Configuráveis
Debug em dev, Info em prod

### ✅ Dependency Injection
Logger injetado automaticamente

### ✅ Performance
Logging assíncrono possível (Serilog)

### ✅ Padrão .NET
Mesmo padrão de ASP.NET Core, Blazor, etc.

---

## Recursos Adicionais

- [Microsoft.Extensions.Logging Docs](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Serilog (framework avançado)](https://serilog.net/)
- [Structured Logging Best Practices](https://messagetemplates.org/)
- [Log Levels Guide](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel)

---

## Conclusão

Sistema de logging implementado com sucesso usando `Microsoft.Extensions.Logging`. Pronto para expandir para file/database quando necessário, mantendo o código limpo e desacoplado.

**Status**: ✅ Produção Ready
**Próximo**: Migrar classes restantes de Console.WriteLine → ILogger

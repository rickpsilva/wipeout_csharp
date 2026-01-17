# WipeoutRewrite (C#)

Projeto convertido para C# e .NET 8, compatível com Linux.

## Estrutura
- `src/` - Código-fonte do jogo (arquitetura em camadas)
  - `Core/` - Lógica de negócio pura (sem dependências externas)
    - `Entities/` - Ship, Track (domain entities)
    - `Services/` - GameState, MenuManager, IMenuManager (gerenciamento de estado e menus)
  - `Infrastructure/` - Implementações de infraestrutura
    - `Graphics/` - GLRenderer (OpenGL), IRenderer
    - `Audio/` - AudioPlayer (OpenAL), IAudioPlayer
    - `Video/` - IntroVideoPlayer, IVideoPlayer
    - `Input/` - InputManager, IInputManager (input de jogo + navegação de menu)
    - `Assets/` - AssetLoader, IAssetLoader
    - `UI/` - MenuRenderer, IMenuRenderer (renderização de menus)
  - `Presentation/` - Game.cs (game loop), TitleScreen, AttractMode, Menus/MainMenuPages
  - Math utilities (`Vec2.cs`, `Vec3.cs`, `Mat4.cs`)
- `wipeout_csharp.Tests/` - Testes unitários (xUnit + Moq)
- `assets/` - Recursos do jogo (texturas, som, vídeo intro, dados)
- `docs/` - Documentação técnica

## Dependências
- .NET 8 SDK
- OpenTK 4.9.0 (renderização e input)
- OpenTK.Audio.OpenAL 4.9.1 (áudio - reprodução WAV)
- SixLabors.ImageSharp 3.1.4 (carregamento de PNG)
- FFMpegCore 5.1.0 (processamento de vídeo intro)
- xUnit 2.9.2 (testes unitários)
- Moq 4.20.72 (mocking para testes)
- **gcc** (para compilar conversor QOA→WAV)

### Conversão de Música QOA para WAV

O jogo original usa ficheiros `.qoa` (Quite OK Audio) que precisam ser convertidos para WAV. Um conversor está incluído em `tools/qoa2wav.c`:

```bash
# Compilar o conversor (apenas uma vez)
cd tools
gcc -O2 -o qoa2wav qoa2wav.c -lm

# Converter todos os ficheiros QOA para WAV
cd ../assets/wipeout/music
for f in *.qoa; do ../../../tools/qoa2wav "$f" "../music_wav/${f%.qoa}.wav"; done
```

Os ficheiros WAV convertidos ficam em `assets/wipeout/music_wav/` e são automaticamente carregados pelo `MusicPlayer`.

## Como compilar e executar

1. Instale o .NET 8 SDK (ex.: `sudo apt install dotnet-sdk-8.0`).
2. No directório `wipeout_csharp`, execute:

```bash
dotnet build
dotnet run
```

Alternativamente use os scripts de conveniência:

```bash
./build.sh
./run.sh
```

### Executar Testes Unitários

```bash
cd wipeout_csharp.Tests
dotnet test
```

Ou para ver mais detalhes:
```bash
dotnet test --verbosity normal
```

## Funcionalidades Implementadas

✅ **Vídeo de Introdução** com sincronização áudio/vídeo (<0.02s drift)
✅ **Splash Screen** com imagem wiptitle.tim e texto "PRESS ENTER" a piscar
✅ **Attract Mode** com créditos em scroll após 10s de inactividade
✅ **Sistema de Música** com reprodução aleatória de 11 faixas WAV
✅ **Game loop** com 60 FPS (OpenTK GameWindow)
✅ **Renderização 2D/3D** com OpenGL 3.3 (sprite + projeção ortográfica)
✅ **Sistema de áudio** com OpenAL (reprodução WAV, controlo de estado)
✅ **Sistema de input** com mapeamento de keybinds (10 actions)
✅ **Carregamento de assets** do projeto original (14 pistas, 8 naves)
✅ **Carregamento de texturas** CMP (LZSS) e TIM (PlayStation 1)
✅ **Sistema de fontes** com 3 tamanhos (drfonts.cmp)
✅ **Arquitetura SOLID** com injeção de dependências e interfaces
✅ **Testes unitários** (xUnit + Moq) - 280 testes passando
✅ **Sistema de Options** completo (Controls/Video/Audio/Best Times) - 48 testes, 100% cobertura
✅ Estruturas de dados (Track, Ship, GameState)

## Correções Recentes (Rendering Pipeline)

**Problema**: Ecra preto apesar de código de renderização estar a funcionar.

**Causa**: Vertex shader não estava a aplicar a transformação de projeção ortográfica.

**Solução**:
1. Adicionado uniforme `mat4 projection` ao vertex shader
2. Transformação: `gl_Position = projection * vec4(pos, 1.0);`
3. Matriz de projeção ortográfica configurada em `Flush()`:
   ```csharp
   var projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
   GL.UniformMatrix4(projLoc, false, ref projection);
   ```
4. Atributos de vértices (posição, UV, cor) configurados corretamente com stride e offset

**Resultado**: Sprites agora aparecem na tela com movimento controlado por teclado

## Controles
- `↑` `↓` - Acelerar/Travar
- `←` `→` - Virar esquerda/direita
- `Z` `X` - Boost esquerda/direita
- `Space` - Disparo
- `V` - Mudar arma
- `P` - Pausa
- `ESC` - Sair

## Fluxo do Jogo

1. **Intro Video** - `intro.mpeg` (recomendado por performance) ou `intro.mp4` como alternativa - com áudio sincronizado (Skip: Enter)
2. **Splash Screen** - Imagem `wiptitle.tim` com "PRESS ENTER" a piscar continuamente
   - Música inicia automaticamente (modo Random)
   - Timeout de 10s para attract mode
3. **Attract Mode** - Créditos em scroll com fundo escurecido
   - Música continua a tocar
   - Qualquer tecla volta ao splash screen
4. **Main Menu** - Navegação completa:
   - START GAME → Race Class → Race Type → Team → Pilot → Circuit → Race
  - OPTIONS → Controls / Video / Audio / Best Times (UI alinhada com opções do wipeout-rewrite: Screen Res, Post Effect, Fullscreen, Show FPS, UI Scale; Music/SFX Volume)
   - QUIT (com confirmação)

### Controles de Menu
- `↑` `↓` - Navegação vertical
- `←` `→` - Navegação horizontal / Ajustar toggles
- `Enter` - Selecionar
- `ESC` - Voltar

### Controles de Jogo
- `↑` `↓` - Acelerar/Travar
- `←` `→` - Virar esquerda/direita
- `Z` `X` - Boost esquerda/direita
- `Space` - Disparo
- `V` - Mudar arma
- `P` - Pausa
- `F11` - Toggle fullscreen

## Próximos passos da conversão

- [x] Sistema de menu (title screen, attract mode, menu principal)
- [x] Sistema de música (conversão QOA→WAV, reprodução aleatória)
- [x] Splash screen com textura wiptitle.tim
- [x] Attract mode com créditos em scroll
- [x] Sistema de Options (arquitetura completa com DI, factory pattern, validação, testes)
- [ ] UI de Options (páginas de menu para Controls/Video/Audio/Best Times)
- [ ] Persistência de Settings (JSON serialization/deserialization)
- [ ] Parser binário de dados de pista (TrackFace, geometria 3D)
- [ ] Renderização de pista em 3D
- [ ] Sistema de física e colisão
- [ ] IA de naves opponent
- [ ] Sistema de armas e potenciadores
- [ ] Som de efeitos (SFX_MENU_MOVE, etc)

## Notas Técnicas

### Renderização
- Immediate-mode rendering (PushSprite, PushTri, Flush)
- Buffer de vértices dinâmico (2048 triângulos max)
- ImageSharp para carregamento de PNG (compatibilidade WSL2)

### Áudio/Vídeo
- **AudioPlayer**: OpenAL para reprodução WAV (LoadWav, Play, Stop, Pause, IsPlaying)
- **MusicPlayer**: Gestão de faixas musicais com modos Random/Sequential/Loop
  - Carrega automaticamente ficheiros WAV de `assets/wipeout/music_wav/`
  - Transição automática entre faixas quando uma termina
  - Fallback para .qoa se WAV não existir (com aviso)
- **IntroVideoPlayer**: Sincronização áudio/vídeo baseada em posição OpenAL
- FFMpeg para extração de frames do vídeo intro (.mpeg mais eficiente, .mp4 suportado)
- Precisão de sync: <20ms (imperceptível)
- Conversão QOA→WAV usando `tools/qoa2wav.c` (baseado em qoa.h do projecto C original)

### Arquitetura
- **Separação de responsabilidades**: Core (business logic) isolado da Infrastructure
- **Dependency Injection**: Interfaces para todas as dependências externas
- **Testabilidade**: Lógica de negócio testável sem OpenGL/OpenAL
- Assets carregados de `assets/wipeout/`

### Sistema de Menu
- **MenuManager**: Stack-based page navigation (push/pop)
- **MenuPage**: Title, items (buttons/toggles), layout flags (vertical/horizontal/fixed)
- **MenuRenderer**: Text rendering, blink animation, anchor-based positioning
- **TitleScreen**: Timeout para attract mode (10s first time, 5s depois)
- **AttractMode**: Random pilot/circuit/class, auto-play demo
- **MainMenuPages**: Hierarquia completa de páginas (8 níveis de navegação)

### Testes
- **280 testes unitários** passando (100% sucesso)
- **ShipTests**: 7 testes (criação, dano, destruição, física)
- **GameStateTests**: 3 testes (estados, inicialização, tempo)
- **Options System**: 48 testes com 100% cobertura
  - ControlsSettings: 7 testes (steering, vibration, validation)
  - VideoSettings: 12 testes (resolution, FPS, anti-aliasing, brightness)
  - AudioSettings: 10 testes (volumes, mute, music modes)
  - BestTimesManager: 11 testes (record management, comparison logic)
  - OptionsFactory: 8 testes (factory pattern, DI integration)
- Cobertura focada em lógica pura sem dependências gráficas

### Sistema de Options
- **Arquitetura completa** seguindo SOLID principles
- **Dependency Injection** com factory pattern
- **5 interfaces** + **5 implementações**
- **Validação** em todos os settings (ranges, enumerations)
- **Reset Pattern** para restaurar defaults
- **Documentação completa**: 
  - [OPTIONS_ARCHITECTURE.md](src/Core/Services/OPTIONS_ARCHITECTURE.md) - Design e contratos
  - [OPTIONS_TESTS_README.md](wipeout_csharp.Tests/Core/Services/OPTIONS_TESTS_README.md) - Documentação de testes
  - [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) - Guia de integração
  - [OPTIONS_SUMMARY.md](OPTIONS_SUMMARY.md) - Resumo executivo
- **Próximas fases**: UI integration, JSON persistence, aplicação aos sistemas do jogo

## Debugging

Para verificar o estado da renderização:
```bash
# Ver carregamento de assets
timeout 5 dotnet run 2>&1 | grep "✓"

# Compilação com warnings
dotnet build
```

Janela esperada: 1280x720, fundo azul, sprites brancos móveis com setas do teclado.

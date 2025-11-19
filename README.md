# Wipeout Rewrite - C# Port

C# port of the Wipeout (PSX 1995) game engine.

## About

This project is a rewrite of the original Wipeout game for PlayStation (1995) using C# and modern game development practices.

## Features

- Full game logic implementation
- OpenGL rendering via Silk.NET
- SDL2 audio system
- Complete menu system with UI constants
- Ship physics and AI
- Track rendering and collision detection

## Requirements

- .NET 8.0 SDK
- Linux (tested on Ubuntu/Debian)
- OpenGL 3.3+ compatible GPU
- SDL2 (for audio)

## Building

```bash
./build.sh
```

## Running

```bash
./run.sh
```

## Testing

```bash
dotnet test
```

## Project Structure

```
src/
  Application/     - Application layer (services, game loop)
  Core/           - Domain entities (Ship, Track, GameState)
  Infrastructure/ - External dependencies (rendering, audio, input)
  Presentation/   - UI/Menu system
```

## Documentation

See the `docs/` folder for detailed documentation:

- [Architecture](docs/ARCHITECTURE.md)
- [Audio System](docs/AUDIO_SYSTEM.md)
- [Video System](docs/VIDEO_SYSTEM.md)
- [Logging System](docs/LOGGING_SYSTEM.md)
- [Development Guide](docs/DEVELOPMENT_GUIDE.md)
- [Testing Status](docs/TESTING_STATUS.md)
- [UI Constants](docs/UI_CONSTANTS.md)

## Original Project

Based on the C rewrite by Dominic Szablewski: https://github.com/phoboslab/wipeout-rewrite

## License

See original project license.


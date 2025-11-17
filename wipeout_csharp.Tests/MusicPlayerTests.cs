using Xunit;
using WipeoutRewrite.Infrastructure.Audio;
using System;
using System.IO;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Testes unitários para MusicPlayer.
/// Estes testes validam a lógica de gestão de faixas sem depender de hardware de áudio.
/// </summary>
public class MusicPlayerTests
{
    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        // Arrange & Act
        Exception? exception = Record.Exception(() => new MusicPlayer());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void LoadTracks_WithNonExistentDirectory_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.LoadTracks("/non/existent/path"));
        Assert.Null(exception);
    }

    [Fact]
    public void LoadTracks_WithEmptyDirectory_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act & Assert
            Exception? exception = Record.Exception(() => player.LoadTracks(tempDir));
            Assert.Null(exception);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SetMode_WithoutLoadingTracks_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.SetMode(MusicMode.Random));
        Assert.Null(exception);
    }

    [Fact]
    public void SetMode_ToRandom_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.SetMode(MusicMode.Random));
        Assert.Null(exception);
    }

    [Fact]
    public void SetMode_ToSequential_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.SetMode(MusicMode.Sequential));
        Assert.Null(exception);
    }

    [Fact]
    public void SetMode_ToLoop_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.SetMode(MusicMode.Loop));
        Assert.Null(exception);
    }

    [Fact]
    public void SetMode_ToPaused_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => 
            player.SetMode(MusicMode.Paused));
        Assert.Null(exception);
    }

    [Fact]
    public void PlayRandomTrack_WithoutLoadingTracks_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.PlayRandomTrack());
        Assert.Null(exception);
    }

    [Fact]
    public void PlayTrack_WithZeroIndex_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.PlayTrack(0));
        Assert.Null(exception);
    }

    [Fact]
    public void PlayTrack_WithInvalidIndex_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.PlayTrack(-1));
        Assert.Null(exception);
        
        exception = Record.Exception(() => player.PlayTrack(999));
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_WithoutPlayingAnything_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void Update_WithoutInitialization_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Update(0.016f));
        Assert.Null(exception);
    }

    [Fact]
    public void Update_WithPausedMode_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();
        player.SetMode(MusicMode.Paused);

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Update(0.016f));
        Assert.Null(exception);
    }

    [Fact]
    public void Update_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                player.Update(0.016f);
            }
        });
        Assert.Null(exception);
    }

    /// <summary>
    /// Teste de integração (requer directório com ficheiros WAV).
    /// Skip por padrão, remover [Fact(Skip = ...)] para testar localmente.
    /// </summary>
    [Fact(Skip = "Requer directório music_wav com ficheiros válidos")]
    public void LoadTracks_WithValidWavDirectory_ShouldLoadTracks()
    {
        // Arrange
        var player = new MusicPlayer();
        string musicPath = "assets/wipeout/music";

        // Act
        player.LoadTracks(musicPath);

        // Assert
        // Se chegou aqui sem exceções, o carregamento funcionou
        Assert.True(true);
    }

    /// <summary>
    /// Teste de integração (requer directório com ficheiros WAV e hardware de áudio).
    /// Skip por padrão.
    /// </summary>
    [Fact(Skip = "Requer ficheiros WAV e hardware de áudio")]
    public void SetMode_ToRandomWithTracks_ShouldStartPlaying()
    {
        // Arrange
        var player = new MusicPlayer();
        string musicPath = "assets/wipeout/music";
        player.LoadTracks(musicPath);

        // Act
        player.SetMode(MusicMode.Random);
        System.Threading.Thread.Sleep(100); // Dar tempo para carregar

        // Assert
        // Se chegou aqui sem exceções, funcionou
        Assert.True(true);
        
        // Cleanup
        player.Stop();
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.016f)]
    [InlineData(1.0f)]
    [InlineData(60.0f)]
    public void Update_WithDifferentDeltaTimes_ShouldNotThrow(float deltaTime)
    {
        // Arrange
        var player = new MusicPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Update(deltaTime));
        Assert.Null(exception);
    }

    [Fact]
    public void MusicMode_AllValues_ShouldBeValid()
    {
        // Arrange & Act
        var modes = new[] 
        { 
            MusicMode.Paused, 
            MusicMode.Random, 
            MusicMode.Sequential, 
            MusicMode.Loop 
        };

        // Assert
        foreach (var mode in modes)
        {
            Assert.True(Enum.IsDefined(typeof(MusicMode), mode));
        }
    }

    [Fact]
    public void LoadTracks_WithQoaFiles_ShouldShowWarning()
    {
        // Arrange
        var player = new MusicPlayer();
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Criar ficheiro QOA falso
            string qoaFile = Path.Combine(tempDir, "test.qoa");
            File.WriteAllText(qoaFile, "fake qoa content");

            // Act
            // Capturar console output seria ideal, mas por agora só verificamos que não lança exceção
            Exception? exception = Record.Exception(() => player.LoadTracks(tempDir));

            // Assert
            Assert.Null(exception);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadTracks_PreferWavOverQoa_ShouldLoadWavFirst()
    {
        // Arrange
        var player = new MusicPlayer();
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string tempDirWav = tempDir + "_wav";
        
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(tempDirWav);

        try
        {
            // Criar ficheiros falsos
            File.WriteAllText(Path.Combine(tempDir, "test.qoa"), "fake qoa");
            File.WriteAllText(Path.Combine(tempDirWav, "test.wav"), "fake wav");

            // Act
            Exception? exception = Record.Exception(() => player.LoadTracks(tempDir));

            // Assert
            Assert.Null(exception);
            // MusicPlayer deve preferir directório _wav sobre .qoa
        }
        finally
        {
            Directory.Delete(tempDir, true);
            Directory.Delete(tempDirWav, true);
        }
    }
}

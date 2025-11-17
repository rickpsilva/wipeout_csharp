using Xunit;
using Moq;
using WipeoutRewrite.Infrastructure.Audio;
using System;
using System.IO;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Testes unitários para AudioPlayer.
/// Nota: Testes reais de OpenAL requerem hardware de áudio.
/// Estes testes validam apenas a lógica sem dependências de hardware.
/// </summary>
public class AudioPlayerTests
{
    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        // Arrange & Act
        Exception? exception = Record.Exception(() => new AudioPlayer());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void LoadWav_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act
        var result = player.LoadWav(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LoadWav_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act
        var result = player.LoadWav("non_existent_file.wav");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LoadWav_WithInvalidWavFile_ShouldReturnFalse()
    {
        // Arrange
        var player = new AudioPlayer();
        string tempFile = Path.GetTempFileName();
        
        try
        {
            // Criar ficheiro inválido (não é WAV)
            File.WriteAllText(tempFile, "This is not a WAV file");

            // Act
            var result = player.LoadWav(tempFile);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Play_WithoutLoadingAudio_ShouldNotThrow()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Play());
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_WithoutLoadingAudio_ShouldNotThrow()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void Pause_WithoutLoadingAudio_ShouldNotThrow()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Pause());
        Assert.Null(exception);
    }

    [Fact]
    public void IsPlaying_WithoutLoadingAudio_ShouldReturnFalse()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act
        var result = player.IsPlaying();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPlaybackPosition_WithoutLoadingAudio_ShouldReturnZero()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act
        var position = player.GetPlaybackPosition();

        // Assert
        Assert.Equal(0.0f, position);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() => player.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var player = new AudioPlayer();

        // Act & Assert
        Exception? exception = Record.Exception(() =>
        {
            player.Dispose();
            player.Dispose();
            player.Dispose();
        });
        Assert.Null(exception);
    }

    /// <summary>
    /// Teste de integração real (requer hardware de áudio e ficheiro WAV válido).
    /// Skip por padrão, remover [Fact(Skip = ...)] para testar localmente.
    /// </summary>
    [Fact(Skip = "Requer hardware de áudio e ficheiro WAV válido")]
    public void LoadWav_WithValidFile_ShouldSucceed()
    {
        // Arrange
        var player = new AudioPlayer();
        string testWavPath = "assets/wipeout/music_wav/track01.wav";

        // Act
        var result = player.LoadWav(testWavPath);

        // Assert
        Assert.True(result);
        
        // Cleanup
        player.Dispose();
    }

    /// <summary>
    /// Teste de integração real (requer hardware de áudio).
    /// Skip por padrão.
    /// </summary>
    [Fact(Skip = "Requer hardware de áudio")]
    public void Play_AfterLoadingValidFile_ShouldPlay()
    {
        // Arrange
        var player = new AudioPlayer();
        string testWavPath = "assets/wipeout/music_wav/track01.wav";
        player.LoadWav(testWavPath);

        // Act
        player.Play();
        System.Threading.Thread.Sleep(100); // Dar tempo para iniciar

        // Assert
        Assert.True(player.IsPlaying());
        
        // Cleanup
        player.Stop();
        player.Dispose();
    }
}

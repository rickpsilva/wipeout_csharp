using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Audio;

public enum MusicMode
{
    Paused,
    Random,
    Sequential,
    Loop
}

public class MusicPlayer : IMusicPlayer
{
    private readonly ILogger<MusicPlayer> _logger;
    private AudioPlayer? _audioPlayer;
    private string[] _tracks;
    private int _currentTrackIndex = -1;
    private MusicMode _mode = MusicMode.Random;
    private Random _random = new Random();
    private bool _isInitialized = false;
    private bool _isPlaying = false;

    public MusicPlayer(ILogger<MusicPlayer> logger)
    {
        _logger = logger;
        _tracks = Array.Empty<string>();
    }

    public void LoadTracks(string musicPath)
    {
        try
        {
            // Tentar carregar WAV primeiro (convertidos)
            string wavPath = musicPath + "_wav";
            
            if (Directory.Exists(wavPath))
            {
                var wavFiles = Directory.GetFiles(wavPath, "*.wav");
                if (wavFiles.Length > 0)
                {
                    _tracks = wavFiles;
                    _isInitialized = true;
                    _logger.LogInformation("Loaded {TrackCount} music tracks (WAV)", _tracks.Length);
                    return;
                }
            }

            // Fallback para QOA (não funcional ainda)
            if (!Directory.Exists(musicPath))
            {
                _logger.LogWarning("Music directory not found: {MusicPath}", musicPath);
                return;
            }

            var qoaFiles = Directory.GetFiles(musicPath, "*.qoa");
            if (qoaFiles.Length == 0)
            {
                _logger.LogWarning("No music files found in: {WavPath}", wavPath);
                return;
            }

            _tracks = qoaFiles;
            _isInitialized = true;
            _logger.LogWarning("Loaded {TrackCount} .qoa tracks (playback not supported - convert to WAV)", _tracks.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading music tracks");
        }
    }

    public void SetMode(MusicMode mode)
    {
        _mode = mode;
        
        if (mode == MusicMode.Random && _isInitialized && _tracks.Length > 0)
        {
            PlayRandomTrack();
        }
    }

    public void PlayRandomTrack()
    {
        if (!_isInitialized || _tracks.Length == 0)
            return;

        int newIndex = _random.Next(0, _tracks.Length);
        PlayTrack(newIndex);
    }

    public void PlayTrack(int index)
    {
        if (!_isInitialized || index < 0 || index >= _tracks.Length)
            return;

        try
        {
            string trackPath = _tracks[index];
            string trackName = Path.GetFileNameWithoutExtension(trackPath);
            
            // Parar música atual
            Stop();
            
            // Tocar apenas se for WAV
            if (trackPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                _audioPlayer = new AudioPlayer();
                if (_audioPlayer.LoadWav(trackPath))
                {
                    _audioPlayer.Play();
                    _currentTrackIndex = index;
                    _isPlaying = true;
                    _logger.LogInformation("Playing: {TrackName}", trackName);
                }
                else
                {
                    _logger.LogWarning("Failed to load: {TrackName}", trackName);
                    _audioPlayer?.Dispose();
                    _audioPlayer = null;
                }
            }
            else
            {
                _logger.LogWarning("Cannot play {TrackName} - only WAV supported", trackName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing track {TrackIndex}", index);
        }
    }

    public void Update(float deltaTime)
    {
        // Verificar se a música terminou e tocar próxima
        if (_isPlaying && _audioPlayer != null && !_audioPlayer.IsPlaying())
        {
            _isPlaying = false;
            
            if (_mode == MusicMode.Random)
            {
                PlayRandomTrack();
            }
            else if (_mode == MusicMode.Sequential && _currentTrackIndex >= 0)
            {
                PlayTrack((_currentTrackIndex + 1) % _tracks.Length);
            }
            else if (_mode == MusicMode.Loop && _currentTrackIndex >= 0)
            {
                PlayTrack(_currentTrackIndex);
            }
        }
    }

    public void Stop()
    {
        _isPlaying = false;
        _audioPlayer?.Dispose();
        _audioPlayer = null;
    }

    public void Dispose()
    {
        Stop();
    }
}

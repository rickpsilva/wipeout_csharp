using System;
using System.IO;

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
    private AudioPlayer? _audioPlayer;
    private string[] _tracks;
    private int _currentTrackIndex = -1;
    private MusicMode _mode = MusicMode.Random;
    private Random _random = new Random();
    private bool _isInitialized = false;
    private bool _isPlaying = false;

    public MusicPlayer()
    {
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
                    Console.WriteLine($"✓ Loaded {_tracks.Length} music tracks (WAV)");
                    return;
                }
            }

            // Fallback para QOA (não funcional ainda)
            if (!Directory.Exists(musicPath))
            {
                Console.WriteLine($"⚠ Music directory not found: {musicPath}");
                return;
            }

            var qoaFiles = Directory.GetFiles(musicPath, "*.qoa");
            if (qoaFiles.Length == 0)
            {
                Console.WriteLine($"⚠ No music files found in: {musicPath}_wav");
                return;
            }

            _tracks = qoaFiles;
            _isInitialized = true;
            Console.WriteLine($"⚠ Loaded {_tracks.Length} .qoa tracks (playback not supported - convert to WAV)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error loading music tracks: {ex.Message}");
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
                    Console.WriteLine($"♪ Playing: {trackName}");
                }
                else
                {
                    Console.WriteLine($"⚠ Failed to load: {trackName}");
                    _audioPlayer?.Dispose();
                    _audioPlayer = null;
                }
            }
            else
            {
                Console.WriteLine($"⚠ Cannot play {trackName} - only WAV supported");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error playing track {index}: {ex.Message}");
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

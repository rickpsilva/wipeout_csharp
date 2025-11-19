using OpenTK.Audio.OpenAL;
using System;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Audio;

/// <summary>
/// Implementação OpenAL do IAudioPlayer.
/// Suporta WAV stereo 16-bit.
/// </summary>
public class AudioPlayer : IAudioPlayer, IDisposable
{
    private ALDevice _device;
    private ALContext _context;
    private int _buffer;
    private int _source;
    private bool _isInitialized;
    private bool _isDisposed;

    public AudioPlayer()
    {
        try
        {
            // Open default audio device
            _device = ALC.OpenDevice(null);
            if (_device == IntPtr.Zero)
            {
                Console.WriteLine("Warning: Failed to open OpenAL device");
                return;
            }

            // Criar contexto de áudio
            _context = ALC.CreateContext(_device, (int[])null!);
            if (_context == IntPtr.Zero)
            {
                Console.WriteLine("Warning: Failed to create OpenAL context");
                ALC.CloseDevice(_device);
                return;
            }

            ALC.MakeContextCurrent(_context);

            // Criar buffer e source
            _buffer = AL.GenBuffer();
            _source = AL.GenSource();

            _isInitialized = true;
            Console.WriteLine("AudioPlayer initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: AudioPlayer initialization failed: {ex.Message}");
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Carrega um arquivo WAV para o buffer de áudio.
    /// Suporta apenas WAV PCM 16-bit stereo ou mono.
    /// </summary>
    public bool LoadWav(string wavPath)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("Warning: AudioPlayer not initialized, cannot load WAV");
            return false;
        }

        if (!File.Exists(wavPath))
        {
            Console.WriteLine($"Warning: WAV file not found: {wavPath}");
            return false;
        }

        try
        {
            using var fs = File.OpenRead(wavPath);
            using var br = new BinaryReader(fs);

            // Ler header RIFF
            string riff = new string(br.ReadChars(4));
            if (riff != "RIFF")
            {
                Console.WriteLine("Warning: Not a valid WAV file (missing RIFF)");
                return false;
            }

            br.ReadInt32(); // File size - 8
            string wave = new string(br.ReadChars(4));
            if (wave != "WAVE")
            {
                Console.WriteLine("Warning: Not a valid WAV file (missing WAVE)");
                return false;
            }

            // Ler chunk "fmt "
            string fmt = new string(br.ReadChars(4));
            if (fmt != "fmt ")
            {
                Console.WriteLine("Warning: WAV format chunk not found");
                return false;
            }

            int fmtSize = br.ReadInt32();
            short audioFormat = br.ReadInt16();
            short channels = br.ReadInt16();
            int sampleRate = br.ReadInt32();
            br.ReadInt32(); // Byte rate
            br.ReadInt16(); // Block align
            short bitsPerSample = br.ReadInt16();

            // Verificar formato suportado
            if (audioFormat != 1) // 1 = PCM
            {
                Console.WriteLine($"Warning: Unsupported audio format: {audioFormat} (only PCM supported)");
                return false;
            }

            if (bitsPerSample != 16)
            {
                Console.WriteLine($"Warning: Unsupported bits per sample: {bitsPerSample} (only 16-bit supported)");
                return false;
            }

            // Pular bytes extras do fmt se existirem
            if (fmtSize > 16)
            {
                br.ReadBytes(fmtSize - 16);
            }

            // Procurar chunk "data"
            string dataChunk = new string(br.ReadChars(4));
            while (dataChunk != "data" && fs.Position < fs.Length)
            {
                int skipSize = br.ReadInt32();
                br.ReadBytes(skipSize);
                if (fs.Position >= fs.Length) break;
                dataChunk = new string(br.ReadChars(4));
            }

            if (dataChunk != "data")
            {
                Console.WriteLine("Warning: WAV data chunk not found");
                return false;
            }

            int dataSize = br.ReadInt32();
            byte[] audioData = br.ReadBytes(dataSize);

            // Determinar formato OpenAL
            ALFormat format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

            // Carregar dados no buffer
            AL.BufferData(_buffer, format, audioData, sampleRate);

            // Verificar erros
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                Console.WriteLine($"Warning: OpenAL error loading buffer: {error}");
                return false;
            }

            Console.WriteLine($"Loaded WAV: {channels}ch, {sampleRate}Hz, {bitsPerSample}bit, {dataSize} bytes");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load WAV: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Inicia a reprodução do áudio.
    /// </summary>
    public void Play()
    {
        if (!_isInitialized) return;

        try
        {
            // Anexar buffer ao source
            AL.Source(_source, ALSourcei.Buffer, _buffer);

            // Configurar source
            AL.Source(_source, ALSourcef.Gain, 1.0f); // Volume 100%
            AL.Source(_source, ALSourcef.Pitch, 1.0f); // Velocidade normal

            // Tocar
            AL.SourcePlay(_source);

            Console.WriteLine("Audio playback started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to play audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Para a reprodução do áudio.
    /// </summary>
    public void Stop()
    {
        if (!_isInitialized) return;

        try
        {
            AL.SourceStop(_source);
            Console.WriteLine("Audio playback stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to stop audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Pausa a reprodução do áudio.
    /// </summary>
    public void Pause()
    {
        if (!_isInitialized) return;

        try
        {
            AL.SourcePause(_source);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to pause audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Retorna true se o áudio está tocando.
    /// </summary>
    public bool IsPlaying()
    {
        if (!_isInitialized) return false;

        try
        {
            AL.GetSource(_source, ALGetSourcei.SourceState, out int state);
            return (ALSourceState)state == ALSourceState.Playing;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retorna a posição atual de playback em segundos.
    /// </summary>
    public float GetPlaybackPosition()
    {
        if (!_isInitialized) return 0f;

        try
        {
            AL.GetSource(_source, ALSourcef.SecOffset, out float position);
            return position;
        }
        catch
        {
            return 0f;
        }
    }

    /// <summary>
    /// Define a posição de playback em segundos.
    /// </summary>
    public void SetPlaybackPosition(float seconds)
    {
        if (!_isInitialized) return;

        try
        {
            AL.Source(_source, ALSourcef.SecOffset, seconds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to set playback position: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            if (_isInitialized)
            {
                Stop();
                AL.DeleteSource(_source);
                AL.DeleteBuffer(_buffer);
                ALC.MakeContextCurrent(ALContext.Null);
                ALC.DestroyContext(_context);
                ALC.CloseDevice(_device);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error disposing AudioPlayer: {ex.Message}");
        }

        _isDisposed = true;
    }
}

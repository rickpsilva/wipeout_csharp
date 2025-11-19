using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using FFMpegCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks;
using FFMpegCore.Pipes;
using WipeoutRewrite.Infrastructure.Audio;

namespace WipeoutRewrite.Infrastructure.Video
{
    /// <summary>
    /// FFmpeg implementation of IVideoPlayer.
    /// </summary>
    public class IntroVideoPlayer : IVideoPlayer
    {
        private readonly ILogger<IntroVideoPlayer> _logger;
        private int _textureId;
        private List<byte[]> _frames = new List<byte[]>();
        private int _videoWidth, _videoHeight;
        private int _currentFrameIndex = 0;
        
        private readonly double _frameRate;
        private bool _isPlaying = false;
        private bool _loadingComplete = false;
        private DateTime _playStartTime;
        private int _lastRenderedFrame = -1;
        
        private AudioPlayer? _audioPlayer;
        private string? _audioTempPath;

        public bool IsPlaying => _isPlaying && _currentFrameIndex < _frames.Count;

        public IntroVideoPlayer(string videoPath, ILogger<IntroVideoPlayer> logger)
        {
            _logger = logger;
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException("Video file not found.", videoPath);
            }

            _logger.LogInformation("Carregando vídeo de introdução...");

            // Get video information
            var mediaInfo = FFProbe.Analyse(videoPath);
            _videoWidth = mediaInfo.PrimaryVideoStream!.Width;
            _videoHeight = mediaInfo.PrimaryVideoStream!.Height;
            _frameRate = mediaInfo.PrimaryVideoStream.FrameRate;
            if (_frameRate <= 0 || double.IsNaN(_frameRate) || double.IsInfinity(_frameRate))
            {
                _frameRate = 25.0;
            }
            
            // Inicializar textura OpenGL
            _textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Pre-load all frames (more efficient than real-time decoding)
            LoadAllFrames(videoPath);
            _logger.LogInformation("{FrameCount} frames carregados ({VideoWidth}x{VideoHeight} @ {FrameRate:F1}fps)", _frames.Count, _videoWidth, _videoHeight, _frameRate);
            
            // Extract and load audio
            ExtractAndLoadAudio(videoPath);
        }

        private void LoadAllFrames(string videoPath)
        {
            // Extract all frames to temp folder
            string tempDir = Path.Combine(Path.GetTempPath(), $"wipeout_intro_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Extrair frames como PNG usando FFmpeg
                FFMpegArguments
                    .FromFileInput(videoPath)
                    .OutputToFile(Path.Combine(tempDir, "frame_%04d.png"), true, options => options
                        .WithVideoCodec("png"))
                    .ProcessSynchronously();

                // Load each frame into memory
                var frameFiles = Directory.GetFiles(tempDir, "frame_*.png");
                Array.Sort(frameFiles);

                foreach (var frameFile in frameFiles)
                {
                    using var image = Image.Load<Rgba32>(frameFile);
                    byte[] frameData = new byte[_videoWidth * _videoHeight * 4];
                    image.CopyPixelDataTo(frameData);
                    _frames.Add(frameData);
                }

                _loadingComplete = true;
            }
            finally
            {
                // Clean up temp folder
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private void ExtractAndLoadAudio(string videoPath)
        {
            try
            {
                _logger.LogInformation("Extraindo áudio do vídeo...");
                
                // Create temporary file for audio
                _audioTempPath = Path.Combine(Path.GetTempPath(), $"wipeout_intro_audio_{Guid.NewGuid()}.wav");
                
                // Extract audio as WAV using FFmpeg
                var result = FFMpegArguments
                    .FromFileInput(videoPath)
                    .OutputToFile(_audioTempPath, true, options => options
                        .WithAudioCodec("pcm_s16le")  // PCM 16-bit
                        .WithAudioSamplingRate(44100) // 44.1kHz
                        .ForceFormat("wav"))
                    .ProcessSynchronously();
                
                if (!result)
                {
                    _logger.LogWarning("FFmpeg falhou ao extrair áudio - continuando sem som");
                    return;
                }
                
                if (File.Exists(_audioTempPath))
                {
                    // Verificar tamanho do ficheiro
                    var fileInfo = new FileInfo(_audioTempPath);
                    if (fileInfo.Length < 1000)
                    {
                        _logger.LogWarning("Arquivo de áudio muito pequeno - possível falha na extração");
                        return;
                    }
                    
                    // Create audio player and load WAV
                    _audioPlayer = new AudioPlayer();
                    if (_audioPlayer.LoadWav(_audioTempPath))
                    {
                        _logger.LogInformation("Áudio carregado com sucesso");
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao carregar áudio - continuando sem som");
                        _audioPlayer?.Dispose();
                        _audioPlayer = null;
                    }
                }
                else
                {
                    _logger.LogWarning("Arquivo de áudio não foi criado - continuando sem som");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair áudio - continuando sem som");
                _audioPlayer = null;
            }
        }

        public void Play()
        {
            if (!_loadingComplete || _frames.Count == 0)
            {
                _logger.LogWarning("Sem frames para reproduzir");
                return;
            }

            _isPlaying = true;
            _currentFrameIndex = 0;
            _playStartTime = DateTime.UtcNow;
            
            // Start audio synchronized with video
            _audioPlayer?.Play();
            
            _logger.LogInformation("Reproduzindo intro...");
        }

        public void Skip()
        {
            _isPlaying = false;
            _currentFrameIndex = _frames.Count;
            
            // Stop audio
            _audioPlayer?.Stop();
            
            _logger.LogInformation("Intro saltada");
        }

        public void Update()
        {
            if (!_isPlaying || _frames.Count == 0)
                return;

            // Synchronize video with audio position (if available)
            // Otherwise, use elapsed real time
            float targetTimeSeconds;
            
            if (_audioPlayer != null && _audioPlayer.IsPlaying())
            {
                // Use audio position as reference (more accurate)
                targetTimeSeconds = _audioPlayer.GetPlaybackPosition();
            }
            else
            {
                // Fallback: usar tempo real decorrido
                var elapsed = DateTime.UtcNow - _playStartTime;
                targetTimeSeconds = (float)elapsed.TotalSeconds;
            }
            
            // Calcular qual frame deveria estar a mostrar baseado no tempo
            int targetFrame = (int)(targetTimeSeconds * _frameRate);
            
            // Limit to available frame count
            if (targetFrame >= _frames.Count)
            {
                targetFrame = _frames.Count - 1;
                _isPlaying = false;
                _audioPlayer?.Stop();
                Console.WriteLine("✓ Intro terminada");
                return;
            }
            
            // Only update if frame changed (avoid unnecessary work)
            if (targetFrame != _lastRenderedFrame)
            {
                _currentFrameIndex = targetFrame;
                _lastRenderedFrame = targetFrame;
                
                // Atualizar textura OpenGL
                GL.BindTexture(TextureTarget.Texture2D, _textureId);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                    _videoWidth, _videoHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 
                    _frames[_currentFrameIndex]);
                
                // Detailed log every second to verify synchronization
                if (_currentFrameIndex % 25 == 0)
                {
                    float audioPos = _audioPlayer?.GetPlaybackPosition() ?? 0f;
                    float videoPos = _currentFrameIndex / (float)_frameRate;
                    float diff = Math.Abs(audioPos - videoPos);
                    
                    Console.WriteLine($"▶ Frame {_currentFrameIndex}/{_frames.Count} | " +
                                    $"Vídeo: {videoPos:F2}s | Áudio: {audioPos:F2}s | " +
                                    $"Diff: {diff:F3}s");
                }
            }
        }

        public int GetTextureId() => _textureId;
        public int GetWidth() => _videoWidth;
        public int GetHeight() => _videoHeight;

        public void Dispose()
        {
            _isPlaying = false;
            _frames.Clear();
            if (_textureId != 0) GL.DeleteTexture(_textureId);
            
            // Clean up audio
            _audioPlayer?.Dispose();
            _audioPlayer = null;
            
            // Clean up temporary audio file
            if (_audioTempPath != null && File.Exists(_audioTempPath))
            {
                try { File.Delete(_audioTempPath); } catch { }
            }
        }
    }
}
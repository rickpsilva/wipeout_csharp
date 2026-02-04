using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Loads and manages circuit preview images from track.cmp
/// </summary>
public class TrackImageLoader : ITrackImageLoader
{
    private readonly ICmpImageLoader _cmpImageLoader;
    private readonly ITimImageLoader _timImageLoader;
    private readonly ILogger<TrackImageLoader> _logger;

    // Circuit mapping from track.cmp indices
    private static readonly string[] CircuitNames = new[]
    {
        "ALTIMA VII",      // Index 0
        "KARBONIS V",      // Index 1
        "TERRAMAX",        // Index 2
        "KORODERA",        // Index 3
        "ARRIDOS IV",      // Index 4
        "SILVERSTREAM",    // Index 5
        "FIRESTAR"         // Index 6
    };

    public TrackImageLoader(
        ICmpImageLoader cmpImageLoader,
        ITimImageLoader timImageLoader,
        ILogger<TrackImageLoader> logger)
    {
        _cmpImageLoader = cmpImageLoader ?? throw new ArgumentNullException(nameof(cmpImageLoader));
        _timImageLoader = timImageLoader ?? throw new ArgumentNullException(nameof(timImageLoader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads all circuit preview images from track.cmp
    /// </summary>
    public Dictionary<int, (byte[] pixels, int width, int height)> LoadAllTrackImages(string cmpPath)
    {
        var result = new Dictionary<int, (byte[] pixels, int width, int height)>();

        if (!File.Exists(cmpPath))
        {
            _logger.LogWarning("Track.cmp not found: {Path}", cmpPath);
            return result;
        }

        _logger.LogInformation("Loading track images from: {Path}", cmpPath);

        // Load compressed images from CMP
        byte[][] compressedImages = _cmpImageLoader.LoadCompressed(cmpPath);
        _logger.LogInformation("Loaded {Count} circuit images from CMP", compressedImages.Length);

        // Convert each TIM image to pixels
        for (int i = 0; i < compressedImages.Length && i < CircuitNames.Length; i++)
        {
            if (compressedImages[i].Length == 0)
            {
                _logger.LogWarning("Skipping empty image at index {Index}", i);
                continue;
            }

            try
            {
                var (pixels, width, height) = _timImageLoader.LoadTimFromBytes(compressedImages[i], transparent: true);
                result[i] = (pixels, width, height);
                _logger.LogInformation("Loaded circuit image {Index} ({Name}): {Width}x{Height}", 
                    i, CircuitNames[i], width, height);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load circuit image {Index} ({Name})", i, CircuitNames[i]);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the circuit name for a given index
    /// </summary>
    public string GetCircuitName(int index)
    {
        if (index >= 0 && index < CircuitNames.Length)
            return CircuitNames[index];
        return $"UNKNOWN {index}";
    }

    /// <summary>
    /// Gets the total number of circuits
    /// </summary>
    public int CircuitCount => CircuitNames.Length;
}

public interface ITrackImageLoader
{
    Dictionary<int, (byte[] pixels, int width, int height)> LoadAllTrackImages(string cmpPath);
    string GetCircuitName(int index);
    int CircuitCount { get; }
}

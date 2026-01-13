namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Default implementation of audio settings management.
/// </summary>
public class AudioSettings : IAudioSettings
{
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SoundEffectsVolume { get; set; } = 1.0f;
    public bool IsMuted { get; set; } = false;
    public bool MusicEnabled { get; set; } = true;
    public bool SoundEffectsEnabled { get; set; } = true;
    public string MusicMode { get; set; } = "Random";

    public void ResetToDefaults()
    {
        MasterVolume = 1.0f;
        MusicVolume = 0.7f;
        SoundEffectsVolume = 1.0f;
        IsMuted = false;
        MusicEnabled = true;
        SoundEffectsEnabled = true;
        MusicMode = "Random";
    }

    public bool IsValid()
    {
        return MasterVolume >= 0.0f && MasterVolume <= 1.0f &&
               MusicVolume >= 0.0f && MusicVolume <= 1.0f &&
               SoundEffectsVolume >= 0.0f && SoundEffectsVolume <= 1.0f &&
               (MusicMode == "Random" || MusicMode == "Sequential" || MusicMode == "Loop");
    }
}

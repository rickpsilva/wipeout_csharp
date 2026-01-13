namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Default implementation of video settings management.
/// Mirrors wipeout-rewrite save_t: fullscreen, internal_roll, ui_scale, show_fps, screen_res, post_effect.
/// UI Scale: 0=AUTO (default, dynamically calculated based on height), 1-4=fixed scale.
/// </summary>
public class VideoSettings : IVideoSettings
{
    public bool Fullscreen { get; set; } = false;
    public float InternalRoll { get; set; } = 0.0f;
    public uint UIScale { get; set; } = 0;  // 0=AUTO (matching C's default)
    public bool ShowFPS { get; set; } = false;
    public int ScreenResolution { get; set; } = 0; // 0=Native, 1=240p, 2=480p
    public int PostEffect { get; set; } = 0; // 0=None, 1=CRT

    public void ResetToDefaults()
    {
        Fullscreen = false;
        InternalRoll = 0.0f;
        UIScale = 0;  // 0=AUTO (matching C's default)
        ShowFPS = false;
        ScreenResolution = 0;
        PostEffect = 0;
    }

    public bool IsValid()
    {
        return UIScale >= 0 && UIScale <= 4 &&
               ScreenResolution >= 0 && ScreenResolution <= 2 &&
               PostEffect >= 0 && PostEffect <= 1 &&
               InternalRoll >= -180.0f && InternalRoll <= 180.0f;
    }
}

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Logical render resolution types.
/// </summary>
public enum ScreenResolutionType
{
    Native = 0,
    Res240p = 1,
    Res480p = 2
}

/// <summary>
/// Post effect types.
/// </summary>
public enum PostEffectType
{
    None = 0,
    CRT = 1
}

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
    public ScreenResolutionType ScreenResolution { get; set; } = ScreenResolutionType.Native;
    public PostEffectType PostEffect { get; set; } = PostEffectType.None;

    public void ResetToDefaults()
    {
        Fullscreen = false;
        InternalRoll = 0.0f;
        UIScale = 0;  // 0=AUTO (matching C's default)
        ShowFPS = false;
        ScreenResolution = ScreenResolutionType.Native;
        PostEffect = PostEffectType.None;
    }

    public bool IsValid()
    {
        return UIScale >= 0 && UIScale <= 4 &&
               ScreenResolution >= ScreenResolutionType.Native && ScreenResolution <= ScreenResolutionType.Res480p &&
               PostEffect >= PostEffectType.None && PostEffect <= PostEffectType.CRT &&
               InternalRoll >= -180.0f && InternalRoll <= 180.0f;
    }
}

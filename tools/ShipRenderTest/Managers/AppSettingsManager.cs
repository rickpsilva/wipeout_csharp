using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Application settings that can be saved/loaded from JSON.
/// </summary>
public class AppSettings
{
    #region properties
    public bool AutoRotate { get; set; } = false;
    public int AutoRotateAxis { get; set; } = 0;
    public bool ShowAssetBrowser { get; set; } = true;
    public bool ShowAxes { get; set; } = true;
    public bool ShowGizmo { get; set; } = true;
    public bool ShowGrid { get; set; } = true;
    public bool ShowProperties { get; set; } = true;
    public bool ShowTextures { get; set; } = true;
    public bool ShowTransform { get; set; } = true;
    public bool ShowViewport { get; set; } = true;
    public float UIScale { get; set; } = 1.0f;
    public bool WireframeMode { get; set; } = false;
    #endregion 
}

/// <summary>
/// Manages application settings persistence.
/// Implements ISettingsService following Dependency Inversion Principle.
/// </summary>
public class AppSettingsManager : ISettingsService
{
    private const string SettingsFileName = "app_settings.json";

    public AppSettings Settings => _settings;

    private readonly ILogger? _logger;
    private AppSettings _settings;

    public AppSettingsManager(ILogger? logger = null)
    {
        _logger = logger;
        _settings = new AppSettings();
        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFileName))
            {
                string json = File.ReadAllText(SettingsFileName);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    _settings = loaded;
                    _logger?.LogInformation("[SETTINGS] Loaded settings from {File}", SettingsFileName);
                    _logger?.LogInformation("[SETTINGS] UI Scale: {Scale}x", _settings.UIScale);
                }
            }
            else
            {
                _logger?.LogInformation("[SETTINGS] No settings file found, using defaults");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SETTINGS] Failed to load settings, using defaults");
            _settings = new AppSettings();
        }
    }

    public async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(SettingsFileName))
            {
                string json = await File.ReadAllTextAsync(SettingsFileName, cancellationToken);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    _settings = loaded;
                    _logger?.LogInformation("[SETTINGS] Loaded settings from {File}", SettingsFileName);
                    _logger?.LogInformation("[SETTINGS] UI Scale: {Scale}x", _settings.UIScale);
                }
            }
            else
            {
                _logger?.LogInformation("[SETTINGS] No settings file found, using defaults");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SETTINGS] Failed to load settings, using defaults");
            _settings = new AppSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(SettingsFileName, json);
            _logger?.LogInformation("[SETTINGS] Saved settings to {File}", SettingsFileName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SETTINGS] Failed to save settings");
        }
    }

    public async Task SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_settings, options);
            await File.WriteAllTextAsync(SettingsFileName, json, cancellationToken);
            _logger?.LogInformation("[SETTINGS] Saved settings to {File}", SettingsFileName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SETTINGS] Failed to save settings");
        }
    }

    public void SetUIScale(float scale)
    {
        _settings.UIScale = Math.Clamp(scale, 0.5f, 3.0f);
        SaveSettings();
    }
}
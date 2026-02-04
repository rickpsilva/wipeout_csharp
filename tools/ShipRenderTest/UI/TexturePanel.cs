using ImGuiNET;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel displaying textures from the selected model in the scene.
/// Shows texture previews with ID, dimensions and format information.
/// </summary>
public class TexturePanel : ITexturePanel
{
    public bool IsVisible { get; set; } = true;

    private readonly ILogger<TexturePanel> _logger;
    private readonly IScene _scene;
    private readonly ITextureManager _textureManager;
    private float _texturePreviewSize = 128f;
    private bool _showTexturePopup = false;
    private int _popupTextureId = 0;
    private int _popupTextureIndex = 0;
    private string _popupTextureSource = "";
    private float _popupZoomLevel = 1.0f;

    public TexturePanel(
        ILogger<TexturePanel> logger,
        IScene scene,
        ITextureManager textureManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, 500), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Textures", ref isVisible))
        {
            IsVisible = isVisible;

            // Use tabs to separate scene model textures from standalone CMP textures
            if (ImGui.BeginTabBar("TextureSourceTabs"))
            {
                // Tab 1: Scene Model Textures
                if (ImGui.BeginTabItem("Modelo em Cena"))
                {
                    RenderSceneModelTextures();
                    ImGui.EndTabItem();
                }

                // Tab 2: Standalone CMP Textures
                if (ImGui.BeginTabItem("Texturas CMP"))
                {
                    RenderStandaloneCmpTextures();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            // Render texture popup viewer (outside tabs to prevent duplicates)
            RenderTexturePopup();
        }
        ImGui.End();
    }

    private void RenderSceneModelTextures()
    {
        var selectedObject = _scene.SelectedObject;

        if (selectedObject?.Ship != null)
        {
            // Debug info
            ImGui.Text($"Model: {selectedObject.Name}");
            ImGui.Text($"Model Loaded: {(selectedObject.Ship.Model != null ? "Yes" : "No")}");
            ImGui.Text($"Texture Array: {(selectedObject.Ship.Texture != null ? "Yes" : "No")}");
            if (selectedObject.Ship.Texture != null)
            {
                ImGui.Text($"Texture Count: {selectedObject.Ship.Texture.Length}");

                // Count valid texture handles
                int validCount = 0;
                for (int i = 0; i < selectedObject.Ship.Texture.Length; i++)
                {
                    if (selectedObject.Ship.Texture[i] > 0)
                        validCount++;
                }
                ImGui.Text($"Valid Handles: {validCount}/{selectedObject.Ship.Texture.Length}");
            }

            if (selectedObject.Ship.Model != null)
            {
                int texturedPrimitives = selectedObject.Ship.Model.Primitives
                    .Count(p => p is FT3 || p is FT4 || p is GT3 || p is GT4);
                ImGui.Text($"Textured Primitives: {texturedPrimitives}/{selectedObject.Ship.Model.Primitives.Count}");

                // Check if primitives have texture handles assigned
                int primitivesWithHandles = 0;
                foreach (var prim in selectedObject.Ship.Model.Primitives)
                {
                    if (prim is FT3 ft3 && ft3.TextureHandle > 0) primitivesWithHandles++;
                    else if (prim is FT4 ft4 && ft4.TextureHandle > 0) primitivesWithHandles++;
                    else if (prim is GT3 gt3 && gt3.TextureHandle > 0) primitivesWithHandles++;
                    else if (prim is GT4 gt4 && gt4.TextureHandle > 0) primitivesWithHandles++;
                }
                ImGui.Text($"Primitives with Handles: {primitivesWithHandles}");
            }

            ImGui.Separator();

            if (selectedObject.Ship.Texture != null && selectedObject.Ship.Texture.Length > 0)
            {
                RenderTextureList(selectedObject.Ship.Texture, "Scene Model");
            }
            else
            {
                ImGui.TextDisabled("No textures loaded");
                ImGui.TextWrapped("This model may have textured primitives but the CMP file was not loaded or found.");

                if (ImGui.Button("Try Reload Textures"))
                {
                    _logger.LogInformation("[TexturePanel] Manual texture reload requested");
                    // Try to reload by checking for CMP file
                    // This is a workaround - ideally should not be needed
                }
            }
        }
        else
        {
            ImGui.TextDisabled("No model selected");
            ImGui.Separator();
            ImGui.TextWrapped("Select a model from the Scene panel to view its textures.");
        }
    }

    private void RenderStandaloneCmpTextures()
    {
        var standaloneTextures = _scene.StandaloneTextures;

        if (standaloneTextures.Count == 0)
        {
            ImGui.TextDisabled("No CMP textures loaded");
            ImGui.Separator();
            ImGui.TextWrapped("Load a CMP file from the Asset Browser to view its textures here.");
            return;
        }

        ImGui.Text($"Loaded CMPs: {standaloneTextures.Count}");
        ImGui.Separator();

        // Show each CMP file and its textures
        foreach (var kvp in standaloneTextures)
        {
            string cmpFileName = kvp.Key;
            int[] textures = kvp.Value;

            if (ImGui.CollapsingHeader($"{cmpFileName} ({textures.Length} textures)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                RenderTextureList(textures, cmpFileName);
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Limpar Todas CMP Textures"))
        {
            _scene.ClearStandaloneTextures();
            _logger.LogInformation("[TexturePanel] Cleared all standalone CMP textures");
        }
    }

    private void RenderTextureList(int[] textureIds, string sourceLabel)
    {
        ImGui.SeparatorText($"Textures ({textureIds.Length})");
        ImGui.Text("Click on any texture to view full size with zoom controls");
        ImGui.Separator();

        // Display textures in a grid
        float windowWidth = ImGui.GetContentRegionAvail().X;
        int columns = Math.Max(1, (int)(windowWidth / (_texturePreviewSize + 10)));

        for (int i = 0; i < textureIds.Length; i++)
        {
            int textureId = textureIds[i];

            // Start a new column if needed
            if (i > 0 && i % columns != 0)
            {
                ImGui.SameLine();
            }

            ImGui.BeginGroup();

            // Display texture preview
            if (textureId > 0)
            {
                // Get texture info
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int width);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int height);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                // Calculate aspect-correct preview size
                float aspectRatio = (float)width / height;
                float previewWidth = _texturePreviewSize;
                float previewHeight = _texturePreviewSize;

                if (aspectRatio > 1.0f)
                {
                    previewHeight = _texturePreviewSize / aspectRatio;
                }
                else
                {
                    previewWidth = _texturePreviewSize * aspectRatio;
                }

                // Display texture as image (UV coordinates fixed for correct orientation)
                ImGui.Image(
                    (IntPtr)textureId,
                    new System.Numerics.Vector2(previewWidth, previewHeight),
                    new System.Numerics.Vector2(0, 0), // UV min
                    new System.Numerics.Vector2(1, 1)  // UV max
                );

                // Click to open detailed viewer
                if (ImGui.IsItemClicked())
                {
                    _showTexturePopup = true;
                    _popupTextureId = textureId;
                    _popupTextureIndex = i;
                    _popupTextureSource = sourceLabel;
                    _popupZoomLevel = 1.0f;
                }

                // Tooltip with more details on hover
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"Texture #{i}");
                    ImGui.Text($"ID: {textureId}");
                    ImGui.Text($"Size: {width}x{height}");
                    ImGui.Text($"Aspect: {aspectRatio:F2}");
                    ImGui.Text("Click to view full size");
                    ImGui.EndTooltip();
                }

                // Label below texture
                ImGui.Text($"#{i} ({width}x{height})");
            }
            else
            {
                // Invalid texture
                ImGui.Dummy(new System.Numerics.Vector2(_texturePreviewSize, _texturePreviewSize));
                ImGui.TextDisabled($"#{i} (Invalid)");
            }

            ImGui.EndGroup();
        }
    }

    private void RenderTexturePopup()
    {
        if (!_showTexturePopup)
            return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));

        bool isOpen = true;
        if (ImGui.Begin($"Texture Viewer - #{_popupTextureIndex} ({_popupTextureSource})", ref isOpen, ImGuiWindowFlags.NoCollapse))
        {
            if (_popupTextureId > 0)
            {
                // Get texture dimensions
                GL.BindTexture(TextureTarget.Texture2D, _popupTextureId);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int width);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int height);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                // Zoom controls
                ImGui.Text($"Texture ID: {_popupTextureId} | Size: {width}x{height}");
                ImGui.Separator();

                if (ImGui.Button("Zoom -"))
                {
                    _popupZoomLevel = Math.Max(0.1f, _popupZoomLevel - 0.25f);
                }
                ImGui.SameLine();
                if (ImGui.Button("Zoom +"))
                {
                    _popupZoomLevel = Math.Min(10.0f, _popupZoomLevel + 0.25f);
                }
                ImGui.SameLine();
                if (ImGui.Button("Reset Zoom"))
                {
                    _popupZoomLevel = 1.0f;
                }
                ImGui.SameLine();
                ImGui.Text($"Zoom: {_popupZoomLevel:F2}x");

                ImGui.Separator();

                // Calculate display size with zoom
                float displayWidth = width * _popupZoomLevel;
                float displayHeight = height * _popupZoomLevel;

                // Scrollable region for large textures
                ImGui.BeginChild("TextureView", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.Border, ImGuiWindowFlags.HorizontalScrollbar);
                
                // Display texture with fixed UV coordinates
                ImGui.Image(
                    (IntPtr)_popupTextureId,
                    new System.Numerics.Vector2(displayWidth, displayHeight),
                    new System.Numerics.Vector2(0, 0), // UV min
                    new System.Numerics.Vector2(1, 1)  // UV max
                );
                
                ImGui.EndChild();
            }
        }
        ImGui.End();

        if (!isOpen)
        {
            _showTexturePopup = false;
        }
    }
}
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
                    RenderTextureList(selectedObject.Ship.Texture);
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
        ImGui.End();
    }

    private void RenderTextureList(int[] textureIds)
    {
        ImGui.SeparatorText($"Textures ({textureIds.Length})");

        // Texture preview size slider
        ImGui.SliderFloat("Preview Size", ref _texturePreviewSize, 64f, 256f);
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

                // Display texture as image
                ImGui.Image(
                    (IntPtr)textureId,
                    new System.Numerics.Vector2(previewWidth, previewHeight),
                    new System.Numerics.Vector2(0, 1), // UV min (flip Y)
                    new System.Numerics.Vector2(1, 0)  // UV max
                );

                // Tooltip with more details on hover
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"Texture #{i}");
                    ImGui.Text($"ID: {textureId}");
                    ImGui.Text($"Size: {width}x{height}");
                    ImGui.Text($"Aspect: {aspectRatio:F2}");
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
}
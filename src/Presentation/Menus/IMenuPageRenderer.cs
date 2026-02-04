using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Services;  // For MenuPage, IMenuManager, etc.

namespace WipeoutRewrite.Presentation.Menus;

/// <summary>
/// Handles menu page rendering logic, separating presentation concerns from game logic.
/// Provides methods to:
/// - Apply page layout and styling based on page definition
/// - Determine which preview to render for the current page
/// - Generate dynamic previews based on selected items
/// 
/// This eliminates scattered if statements throughout Game.cs
/// and makes menu rendering behavior clearer and more testable.
/// </summary>
public interface IMenuPageRenderer
{
    /// <summary>
    /// Apply layout and style properties from MenuPageDefinition to a MenuPage instance.
    /// </summary>
    void ApplyPageDefinition(MenuPage page, MenuPageDefinition definition);
    
    /// <summary>
    /// Check if this page should have a dynamic preview based on selection.
    /// For example, team selection shows both pilots for selected team.
    /// </summary>
    bool ShouldHaveDynamicPreview(string pageId, int selectedIndex);
    
    /// <summary>
    /// Create or update the dynamic preview layout for the current page.
    /// </summary>
    void UpdateDynamicPreview(MenuPage page, string pageId, IGameDataService gameDataService);
}

/// <summary>
/// Default implementation of menu page rendering logic.
/// Centralizes all menu-specific rendering decisions.
/// </summary>
public class MenuPageRenderer : IMenuPageRenderer
{
    public void ApplyPageDefinition(MenuPage page, MenuPageDefinition definition)
    {
        if (definition == null)
            return;
        
        page.Title = definition.Title;
        page.LayoutFlags = definition.LayoutFlags;
        page.TitlePos = definition.TitlePos;
        page.TitleAnchor = definition.TitleAnchor;
        
        if (definition.ItemsPos.HasValue)
            page.ItemsPos = definition.ItemsPos.Value;
        if (definition.ItemsAnchor.HasValue)
            page.ItemsAnchor = definition.ItemsAnchor.Value;
    }
    
    public bool ShouldHaveDynamicPreview(string pageId, int selectedIndex)
    {
        return pageId switch
        {
            MenuPageIds.Team => true,
            MenuPageIds.RaceClass => true,
            MenuPageIds.Pilot => true,
            _ => false
        };
    }
    
    public void UpdateDynamicPreview(MenuPage page, string pageId, IGameDataService gameDataService)
    {
        if (!ShouldHaveDynamicPreview(pageId, page.SelectedIndex))
            return;
        
        switch (pageId)
        {
            case MenuPageIds.Team:
                UpdateTeamSelectionPreview(page, gameDataService);
                break;
            case MenuPageIds.Pilot:
                UpdatePilotSelectionPreview(page, gameDataService);
                break;
            case MenuPageIds.RaceClass:
                UpdateRaceClassPreview(page, gameDataService);
                break;
        }
    }

    /// <summary>
    /// When a pilot is selected, show pilot ship in preview viewport.
    /// </summary>
    private void UpdatePilotSelectionPreview(MenuPage page, IGameDataService gameDataService)
    {
        if (page.SelectedIndex < 0 || page.SelectedIndex >= page.Items.Count)
            return;

        // Get the selected menu item - the Data field contains the pilot ID
        var selectedItem = page.Items[page.SelectedIndex];
        if (selectedItem != null)
        {
            // MenuItem.Data contains the pilot ID (set when menu was created)
            int pilotId = selectedItem.Data;
            
            var dynamicLayout = PreviewLayoutFactory.CreatePilotSelectionLayout(
                gameDataService, 
                pilotId);
            
            page.PreviewLayout = dynamicLayout;
        }
    }
    
    /// <summary>
    /// When a team is selected, show both pilots of that team side-by-side.
    /// </summary>
    private void UpdateTeamSelectionPreview(MenuPage page, IGameDataService gameDataService)
    {
        if (page.SelectedIndex < 0)
            return;
        
        // Create dynamic layout showing both pilots for selected team
        var dynamicLayout = PreviewLayoutFactory.CreateTeamSelectionLayout(
            gameDataService, 
            page.SelectedIndex);
        
        page.PreviewLayout = dynamicLayout;
    }
    
    /// <summary>
    /// When a race class is selected, show relevant race class preview.
    /// </summary>
    private void UpdateRaceClassPreview(MenuPage page, IGameDataService gameDataService)
    {
        // Implement race class preview logic if needed
    }
}

using StardewValley.Menus;

namespace WorldAtlas.Compatibility.BetterGameMenu;

public interface IBetterGameMenuApi
{
        /// <summary>
    /// Just check to see if the provided menu is a Better Game Menu,
    /// without actually casting it. This can be useful if you want to remove
    /// the menu interface from the API surface you use.
    /// </summary>
    /// <param name="menu">The menu to check</param>
    bool IsMenu(IClickableMenu menu);

    /// <summary>
    /// Get the current page of the provided Better Game Menu instance. If the
    /// provided menu is not a Better Game Menu, or a page is not ready, then
    /// return <c>null</c> instead.
    /// </summary>
    /// <param name="menu">The menu to get the page from.</param>
    IClickableMenu? GetCurrentPage(IClickableMenu menu);
}

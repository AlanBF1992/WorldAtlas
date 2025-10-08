using StardewValley;
using StardewValley.Menus;
using StardewValley.WorldMaps;

namespace WorldAtlas.Patches
{
    internal static class FarmerRendererPatches
    {
        internal static bool drawMiniPortratPrefix() // Portrat xD
        {
            if (ModEntry.GetGameMenuPage(Game1.activeClickableMenu) is not MapPage) return true;

            MapAreaPosition? playerPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, Game1.player.TilePoint)?.Data;

            return ModEntry.SelectedRegionInfo is null || ModEntry.SelectedRegionInfo.Region.Id == playerPosition?.Region.Id;
        }
    }
}

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.WorldMaps;
using WorldAtlas.Patches;

namespace WorldAtlas
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VanillaPatches(harmony);

            helper.Events.GameLoop.SaveLoaded += LoadRegions;
            helper.Events.Display.MenuChanged += ResetMapAfterClosingMenu;
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private static void VanillaPatches(Harmony harmony)
        {
            //QQQ
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MapPage), [typeof(int), typeof(int), typeof(int), typeof(int)], false),
                transpiler: new HarmonyMethod(typeof(MapPagePatches), nameof(MapPagePatches.ctorTranspiler)),
                postfix: new HarmonyMethod(typeof(MapPagePatches), nameof(MapPagePatches.ctorPostfix))
            );

            // Acciones al apretar left click
            harmony.Patch(
                original: AccessTools.Method(typeof(MapPage), nameof(MapPage.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MapPagePatches), nameof(MapPagePatches.receiveLeftClickPrefix))
            );

            // Don't draw MiniPortrait if menu is not in that region
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawMiniPortrat)),
                prefix: new HarmonyMethod(typeof(FarmerRendererPatches), nameof(FarmerRendererPatches.drawMiniPortratPrefix))
            );

            // Add the dropdown, hopefully
            harmony.Patch(
                original: AccessTools.Method(typeof(MapPage), nameof(MapPage.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MapPagePatches), nameof(MapPagePatches.drawPostfix))
            );
        }

        [EventPriority((EventPriority)int.MinValue)]
        internal static void LoadRegions(object? sender, SaveLoadedEventArgs e)
        {
            // Get regions, positions and locations from the game
            List<RegionInfo> allRegionsInfo = [.. Game1.locations
                .Select(location => new
                {
                    Location = location,
                    Position = WorldMapManager.GetPositionDataWithoutFallback(location, Point.Zero)
                })
                .Where(data => data.Position is not null)
                .DistinctBy(data => data.Position.Region.Id)
                .Select(data => new RegionInfo
                (
                    data.Location,
                    data.Position.Region,
                    data.Position.Region.Id
                ))];

            // Ginger Island is weird man.
            if (!allRegionsInfo.Select(x => x.DisplayName).Contains("GingerIsland"))
            {
                allRegionsInfo.Insert(1, new RegionInfo(Game1.getLocationFromName("IslandWest"),
                                                        ModEntry.GingerIsland,
                                                        ModEntry.GingerIsland.Id));
            }

            // Get regions id, name and visibility from data.json
            ModData? modData = ModEntry.ModHelper.Data.ReadJsonFile<ModData>("data.json");

            if (modData is null)
            {
                modData = new ModData();
                modData.Regions.AddRange(allRegionsInfo.Select(data => new MapRegionJson(data.Region.Id, data.Region.Id, true)));
            }

            modData.Regions.AddRange(
                allRegionsInfo
                    .Where(data => !modData.Regions.Select(r => r.Id).Contains(data.Region.Id))
                    .Select(data => new MapRegionJson(data.Region.Id, data.Region.Id, true))
            );

            ModEntry.ModHelper.Data.WriteJsonFile("data.json", modData);

            // Filter the visible ones
            ModEntry.AllVisibleRegionsInfo = [.. allRegionsInfo.Where(data => modData.Regions.Any(r => r.Id == data.Region.Id && r.IsVisible))];

            // Update visible game regions name
            foreach (var dataRegion in modData.Regions)
            {
                RegionInfo? gameRegion = ModEntry.AllVisibleRegionsInfo.SingleOrDefault(data => data.Region.Id == dataRegion.Id);
                if (gameRegion is not null)
                {
                    gameRegion.DisplayName = dataRegion.Name;
                    gameRegion.PageNum = dataRegion.PageNum;
                }
            }
        }

        private static void ResetMapAfterClosingMenu(object? sender, MenuChangedEventArgs e)
        {
            // This will fire after closing any menu sadly, but it works
            if (ModEntry.IsGameMenu(e.OldMenu) && !ModEntry.IsGameMenu(e.NewMenu))
            {
                ModEntry.SelectedRegionInfo = null;
                MapPagePatches.PageNumber = 0;
                MapPagePatches.SelectedComponentId = 0;
            }
        }
    }
}

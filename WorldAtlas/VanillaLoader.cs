using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.WorldMaps;
using WorldAtlas.Compatibility.GMCM;
using WorldAtlas.Patches;

namespace WorldAtlas
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VanillaPatches(harmony);

            helper.Events.GameLoop.GameLaunched += GMCMBasicConfig;
            helper.Events.GameLoop.SaveLoaded += LoadRegions;
            helper.Events.GameLoop.SaveLoaded += GMCMMapConfig;
            helper.Events.Display.MenuChanged += ResetMapAfterClosingMenu;
            helper.Events.Player.Warped += UpdateVisitedLocations;
        }

        /***********
         * Patches *
         ***********/
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

        /**********
         * Events *
         **********/
        private static void GMCMBasicConfig(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: ModEntry.ModManifest,
                reset: () => ModEntry.Config = new ModConfig(),
                save: () => ModEntry.ModHelper.WriteConfig(ModEntry.Config)
            );

            /******************
             * Basic Settings *
             ******************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => ModEntry.ModHelper.Translation.Get("gmcm-basic-config-title")
            );

            // Must Visit the Region First
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MustVisitRegion,
                setValue: (value) => ModEntry.Config.MustVisitRegion = value,
                name: () => ModEntry.ModHelper.Translation.Get("gmcm-must-visit-title"),
                tooltip: () => ModEntry.ModHelper.Translation.Get("gmcm-must-visit-tooltip")
            );

            // Paragraph
            configMenu.AddParagraph(
                mod: ModEntry.ModManifest,
                text: () => ModEntry.ModHelper.Translation.Get("gmcm-map-loading-paragraph")
            );
        }

        [EventPriority((EventPriority)int.MinValue)]
        private static void GMCMMapConfig(object? sender, SaveLoadedEventArgs e)
        {
            ModEntry.ModHelper.Events.GameLoop.SaveLoaded -= GMCMMapConfig;
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.AddPageLink(
                mod: ModEntry.ModManifest,
                pageId: "MapsOptions",
                text: () => ModEntry.ModHelper.Translation.Get("gmcm-map-config-title")
            );

            configMenu.AddPage(
                mod: ModEntry.ModManifest,
                pageId: "MapsOptions",
                pageTitle: () => ModEntry.ModHelper.Translation.Get("gmcm-map-config-title")
            );

            foreach (RegionInfo regionInfo in ModEntry.AvailableRegionsInfo)
            {
                configMenu.AddSectionTitle(
                    mod: ModEntry.ModManifest,
                    text: () => regionInfo.Region.Id
                );

                GMCMExtras.AddWideTextOption(
                    mod: ModEntry.ModManifest,
                    name: () => ModEntry.ModHelper.Translation.Get("gmcm-display-name-title"),
                    getValue: () => regionInfo.DisplayName,
                    setValue: (value) => {
                        regionInfo.DisplayName = value;
                        UpdateRegions(regionInfo.Region.Id, regionInfo.DisplayName);
                    },
                    tooltip: () => ModEntry.ModHelper.Translation.Get("gmcm-display-name-tooltip"),
                    width: 496
                );

                configMenu.AddBoolOption(
                    mod: ModEntry.ModManifest,
                    getValue: () => regionInfo.isVisible,
                    setValue: (value) => {
                        regionInfo.isVisible = value;
                        UpdateVisibleRegions();
                    },
                    name: () => ModEntry.ModHelper.Translation.Get("gmcm-is-visible-title"),
                    tooltip: () => ModEntry.ModHelper.Translation.Get("gmcm-is-visible-tooltip")
                );

                configMenu.AddTextOption(
                    mod: ModEntry.ModManifest,
                    getValue: () => regionInfo.PageNum.ToString(),
                    setValue: (value) => regionInfo.PageNum = int.Parse(value),
                    name: () => ModEntry.ModHelper.Translation.Get("gmcm-page-number-title"),
                    tooltip: () => ModEntry.ModHelper.Translation.Get("gmcm-page-number-tooltip"),
                    allowedValues: ["0", "1"],
                    formatAllowedValue: (value) =>
                    {
                        return value switch
                        {
                            "0" => ModEntry.ModHelper.Translation.Get("map-tab-main"),
                            _ => ModEntry.ModHelper.Translation.Get("map-tab-extras")
                        };
                    }
                );
            }
        }

        private static void UpdateVisitedLocations(object? sender, WarpedEventArgs e)
        {
            var position = WorldMapManager.GetPositionDataWithoutFallback(e.NewLocation, Point.Zero);
            if (position is null) return;

            var region = ModEntry.AvailableRegionsInfo.FirstOrDefault(s => s.Region.Id == position.Region.Id);
            if (region?.wasVisited != false) return;

            region.wasVisited = true;
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

        /***********
         * ModData *
         ***********/
        [EventPriority((EventPriority)(int.MinValue + 1))]
        internal static void LoadRegions(object? sender, SaveLoadedEventArgs e)
        {
            // Get regions, positions and locations from the game
            ModEntry.AvailableRegionsInfo = [.. Game1.locations
                .Select(location => new
                {
                    Location = location,
                    Position = WorldMapManager.GetPositionDataWithoutFallback(location, Point.Zero),
                    Visited = GameStateQuery.CheckConditions($"PLAYER_VISITED_LOCATION Current {location.Name}")
                })
                .Where(data => data.Position is not null)
                .OrderByDescending(data => data.Visited) // First the visited ones, so they are choosen by DistinctBy
                .DistinctBy(data => data.Position.Region.Id)
                .Select(data => new RegionInfo
                (
                    data.Location,
                    data.Position.Region,
                    data.Position.Region.Id,
                    data.Visited
                ))];

            // Ginger Island is weird man.
            if (!ModEntry.AvailableRegionsInfo.Select(x => x.DisplayName).Contains("GingerIsland"))
            {
                ModEntry.AvailableRegionsInfo.Insert(1, new RegionInfo(Game1.getLocationFromName("IslandSouth"),
                                                        ModEntry.GingerIsland,
                                                        ModEntry.GingerIsland.Id,
                                                        GameStateQuery.CheckConditions("PLAYER_VISITED_LOCATION Current IslandSouth")));
            }

            // Get regions id, name and visibility from data.json
            ModData? modData = ModEntry.ModHelper.Data.ReadJsonFile<ModData>("data.json");

            if (modData is null)
            {
                modData = new ModData();
                modData.Regions.AddRange(ModEntry.AvailableRegionsInfo.Select(data => new MapRegionJson(data.Region.Id, data.Region.Id, true)));
            }

            modData.Regions.AddRange(
                ModEntry.AvailableRegionsInfo
                    .Where(data => !modData.Regions.Select(r => r.Id).Contains(data.Region.Id))
                    .Select(data => new MapRegionJson(data.Region.Id, data.Region.Id, true))
            );

            ModEntry.ModHelper.Data.WriteJsonFile("data.json", modData);

            // Filter the visible ones
            ModEntry.AvailableRegionsInfo.ForEach(data => data.isVisible = modData.Regions.Any(r => r.Id == data.Region.Id && r.IsVisible));

            // Update visible game regions name
            foreach (var dataRegion in modData.Regions)
            {
                if (ModEntry.AvailableRegionsInfo.Find(data => data.Region.Id == dataRegion.Id) is RegionInfo gameRegion)
                {
                    gameRegion.DisplayName = dataRegion.Name;
                    gameRegion.PageNum = dataRegion.PageNum;
                }
            }

            UpdateVisibleRegions();
        }

        internal static void UpdateRegions(string updateId, string displayName)
        {
            ModData modData = ModEntry.ModHelper.Data.ReadJsonFile<ModData>("data.json")!;
            modData.Regions.Find(r => r.Id == updateId)!.Name = displayName;
            ModEntry.ModHelper.Data.WriteJsonFile("data.json", modData);
        }

        internal static void UpdateVisibleRegions()
        {
            ModEntry.VisibleRegionsInfo = [.. ModEntry.AvailableRegionsInfo.Where(data => data.isVisible)];
        }
    }
}

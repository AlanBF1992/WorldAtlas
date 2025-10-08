using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.WorldMaps;
using System.Reflection;
using System.Reflection.Emit;

namespace WorldAtlas.Patches
{
    internal static class MapPagePatches
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal readonly static List<RegionInfo> CurrentPageRegionInfo = [];
        internal readonly static List<ClickableComponent> pageNumberComponent = [];
        internal readonly static List<ClickableComponent> regionComponents = [];

        internal static int PageNumber { get; set; }
        internal static int SelectedComponentId { get; set; }

        internal static IEnumerable<CodeInstruction> ctorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo GetPositionDataInfo = AccessTools.Method(typeof(MapPagePatches), nameof(GetPositionData));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("MapPagePatcher.ctorTranspiler: IL code not found")
                    .Advance(1)
                    .SetOperandAndAdvance(
                        GetPositionDataInfo
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(ctorTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
        internal static MapAreaPositionWithContext? GetPositionData(GameLocation location, Point tile)
        {
            if (ModEntry.SelectedRegionInfo is null)
            {
                return WorldMapManager.GetPositionData(location, tile);
            }

            if (!tile.Equals(Game1.player.TilePoint)) return null;

            MapAreaPosition? playerPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, tile)?.Data;

            if (ModEntry.SelectedRegionInfo.Region.Id == playerPosition?.Region?.Id)
            {
                return WorldMapManager.GetPositionData(location, tile);
            }

            return WorldMapManager.GetPositionData(ModEntry.SelectedRegionInfo.Location, Point.Zero);
        }

        internal static void ctorPostfix()
        {
            PageNumber = 0;
            SelectedComponentId = 0;
            ModEntry.SelectedRegionInfo = null;
            createPageButtons();
            createRegionComponents();
        }

        internal static void drawPostfix(SpriteBatch b)
        {
            foreach (ClickableComponent c in pageNumberComponent)
            {
                var nameSize = Game1.smallFont.MeasureString(c.name);

                IClickableMenu.drawTextureBox(b, c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, c.myID == PageNumber? Color.LightGreen: Color.White);
                Utility.drawTextWithColoredShadow(b, c.name, Game1.smallFont,
                    new Vector2(c.bounds.X, c.bounds.Y) + new Vector2(c.bounds.Width/2, c.bounds.Height/2) - new Vector2(nameSize.X / 2, nameSize.Y / 2), //  - new Vector2(nameSize.X / 2, nameSize.Y) + new Vector2(c.bounds.Width / 2, c.bounds.Height)
                    Color.Black, Color.Black * 0.15f);
            }

            foreach (ClickableComponent c in regionComponents)
            {
                IClickableMenu.drawTextureBox(b, c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, c.myID == SelectedComponentId? Color.DeepSkyBlue: Color.White);
                Utility.drawTextWithColoredShadow(b, c.name, Game1.smallFont,
                    new Vector2(c.bounds.X + 16, c.bounds.Y + 16),
                    Color.Black, Color.Black * 0.15f);
            }
        }

        internal static void createPageButtons()
        {
            // Top buttons
            pageNumberComponent.Clear();

            pageNumberComponent.Add(
                new ClickableComponent(new Rectangle(16, 16, 160, 64), ModEntry.ModHelper.Translation.Get("MainPage"))
                {
                    myID = 0,
                    fullyImmutable = true
                });
            pageNumberComponent.Add(
                new ClickableComponent(new Rectangle(160 + 16, 16, 160, 64), ModEntry.ModHelper.Translation.Get("ExtrasPage"))
                {
                    myID = 1,
                    fullyImmutable = true
                });
        }

        internal static void createRegionComponents()
        {
            CurrentPageRegionInfo.Clear();

            CurrentPageRegionInfo.AddRange(ModEntry.AllVisibleRegionsInfo.Where(data => data.PageNum == PageNumber));

            regionComponents.Clear();
            int totalExtraLines = 0;

            var playerRegionId = WorldMapManager.GetPositionData(Game1.player.currentLocation, Game1.player.TilePoint)?.Data.Region.Id;

            for (int i = 0; i < CurrentPageRegionInfo.Count; i++)
            {
                int internalExtraLines = 0;

                var name = CurrentPageRegionInfo[i].DisplayName;
                var nameSize = Game1.smallFont.MeasureString(name);

                if (nameSize.X > 320 - 32)
                {
                    name = Game1.parseText(name, Game1.smallFont, 320 - 32);
                    internalExtraLines = name.Split(Environment.NewLine).Length - 1;
                }

                var c = new ClickableComponent(new Rectangle(16, 32 + 64 + i * 64 + totalExtraLines * 32, 320, 64 + internalExtraLines * 32), name)
                    {
                        myID = i + 2,
                        downNeighborID = ((i < 6) ? (i + 2 + 1) : (9175502)),
                        upNeighborID = ((i > 0) ? (i + 2 - 1) : (9175502)),
                        fullyImmutable = true
                    };

                regionComponents.Add(c);
                totalExtraLines += internalExtraLines;

                if ((ModEntry.SelectedRegionInfo is null && CurrentPageRegionInfo[i].Region.Id == playerRegionId) || (ModEntry.SelectedRegionInfo?.Region.Id == CurrentPageRegionInfo[i].Region.Id))
                {
                    SelectedComponentId = c.myID;
                    ModEntry.SelectedRegionInfo = CurrentPageRegionInfo[i];
                }
            }
        }

        internal static bool receiveLeftClickPrefix(MapPage __instance, int x, int y)
        {
            for (int i = 0; i < pageNumberComponent.Count; i++)
            {
                if (pageNumberComponent[i].containsPoint(x,y))
                {
                    PageNumber = pageNumberComponent[i].myID;
                    SelectedComponentId = 0;
                    createRegionComponents();
                    return false;
                }
            }

            for (int i = 0; i < regionComponents.Count; i++)
            {
                if (regionComponents[i].bounds.Contains(x, y))
                {
                    SelectedComponentId = regionComponents[i].myID;
                    ModEntry.SelectedRegionInfo = CurrentPageRegionInfo[i];
                    ReconstructPage(__instance);
                    return false;
                }
            }

            if(!__instance.points.Values.Any(p => p.containsPoint(x, y)))
            {
                PageNumber = 0;
                SelectedComponentId = 0;
                ModEntry.SelectedRegionInfo = null;

                // Better Game Menu doesn't destroy it, so we just rebuild it.
                // It should still change the tab, but it doesn't for some reason
                // so fuck it, I'll do it myself.
                if (Game1.activeClickableMenu is not GameMenu && __instance.readyToClose())
                {
                    createPageButtons();
                    createRegionComponents();
                    ReconstructPage(__instance);

                    dynamic menu = Game1.activeClickableMenu;
                    if (!menu.TryChangeTab(menu.LastTab))
                    {
                        __instance.exitThisMenu();
                    }

                    return false;
                }
            }

            return true;
        }

        internal static void ReconstructPage(MapPage __instance)
        {
            WorldMapManager.ReloadData();

            Point normalizedPlayerTile = __instance.GetNormalizedPlayerTile(Game1.player);

            MapAreaPositionWithContext? mapPosition = GetPositionData(Game1.player.currentLocation, normalizedPlayerTile);
            MapRegion mapRegion = mapPosition?.Data.Region ?? WorldMapManager.GetMapRegions().First();
            MapArea[] mapAreas = mapRegion.GetAreas();
            string? scrollText = (ModEntry.SelectedRegionInfo?.DisplayName)?? mapPosition?.Data.GetScrollText(normalizedPlayerTile);

            __instance.SetInstanceField("mapPosition", mapPosition);
            __instance.SetInstanceField("mapRegion", mapRegion);
            __instance.SetInstanceField("mapAreas", mapAreas);
            __instance.SetInstanceField("scrollText", scrollText);

            __instance.mapBounds = mapRegion.GetMapPixelBounds();

            int num = 1000;
            __instance.SetInstanceField("defaultComponentID", 1000);

            MapArea[] array = mapAreas;
            __instance.points.Clear();

            for (int i = 0; i < array.Length; i++)
            {
                foreach (MapAreaTooltip mapAreaTooltip in array[i].GetTooltips())
                {
                    Rectangle pixelArea = mapAreaTooltip.GetPixelArea();
                    pixelArea = new Rectangle(__instance.mapBounds.X + pixelArea.X, __instance.mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
                    num++;
                    __instance.points[mapAreaTooltip.NamespacedId] = new(pixelArea, mapAreaTooltip.NamespacedId)
                    {
                        myID = num,
                        label = mapAreaTooltip.Text
                    };
                    if (mapAreaTooltip.NamespacedId == "Farm/Default")
                    {
                        __instance.SetInstanceField("defaultComponentID", num);
                    }
                }
            }

            array = mapAreas;
            for (int i = 0; i < array.Length; i++)
            {
                foreach (MapAreaTooltip mapAreaTooltip2 in array[i].GetTooltips())
                {
                    if (__instance.points.TryGetValue(mapAreaTooltip2.NamespacedId, out var value2))
                    {
                        __instance.SetNeighborId(value2, "left", mapAreaTooltip2.Data.LeftNeighbor);
                        __instance.SetNeighborId(value2, "right", mapAreaTooltip2.Data.RightNeighbor);
                        __instance.SetNeighborId(value2, "up", mapAreaTooltip2.Data.UpNeighbor);
                        __instance.SetNeighborId(value2, "down", mapAreaTooltip2.Data.DownNeighbor);
                    }
                }
            }
        }
    }
}

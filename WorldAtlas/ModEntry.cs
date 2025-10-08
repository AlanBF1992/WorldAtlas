using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using WorldAtlas.Compatibility.BetterGameMenu;
using WorldAtlas.Compatibility.UIInfoSuite2;

namespace WorldAtlas
{
    public class ModEntry : Mod
    {
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IMonitor LogMonitor { get; internal set; } = null!;

        /// <summary>Simplified APIs for writing mods.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;

        /// <summary>Manifest of the mod.</summary>
        new public static IManifest ModManifest { get; internal set; } = null!;

        public static RegionInfo? SelectedRegionInfo { get; internal set; }

        public static List<RegionInfo> AllVisibleRegionsInfo { get; internal set; } = null!;

        public static IBetterGameMenuApi? BetterGameMenuApi { get; internal set; }

        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = Helper;
            ModManifest = base.ModManifest;

            Harmony harmony = new(ModManifest.UniqueID);

            // Vanilla Patches
            VanillaLoader.Loader(helper, harmony);
            LogMonitor.Log("Base Patches Loaded", LogLevel.Info);

            if (ModHelper.ModRegistry.IsLoaded("leclair.bettergamemenu"))
            {
                ModHelper.Events.GameLoop.GameLaunched += GetBGMApi;
            }

            if (ModHelper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2"))
            {
                UIInfoSuite2Loader.Loader(helper, harmony);
                LogMonitor.Log("UI Info Suite 2 Patches Loaded", LogLevel.Info);
            }
        }

        internal static void GetBGMApi(object? sender, GameLaunchedEventArgs e)
        {
            BetterGameMenuApi = ModHelper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu")!;
        }

        public static bool IsGameMenu(IClickableMenu? menu)
        {
            if (menu is null) return false;
            if (menu is GameMenu)
                return true;
            return BetterGameMenuApi?.IsMenu(menu) ?? false;
        }
        public static IClickableMenu? GetGameMenuPage(IClickableMenu? menu)
        {
            if (menu is null) return null;
            if (menu is GameMenu gameMenu)
                return gameMenu.GetCurrentPage();
            return BetterGameMenuApi?.GetCurrentPage(menu);
        }
    }
}

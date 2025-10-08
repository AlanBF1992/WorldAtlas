using HarmonyLib;
using StardewModdingAPI;
using WorldAtlas.Compatibility.UIInfoSuite2.Patches;

namespace WorldAtlas.Compatibility.UIInfoSuite2
{
    internal static class UIInfoSuite2Loader
    {
        internal static void Loader(IModHelper _, Harmony harmony)
        {
            UIInfoSuite2Patches(harmony);
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private static void UIInfoSuite2Patches(Harmony harmony)
        {
            //QQQ
            harmony.Patch(
                original: AccessTools.Method("UIInfoSuite2.UIElements.LocationOfTownsfolk:GetMapCoordinatesForNPC"),
                transpiler: new HarmonyMethod(typeof(LocationOfTownsfolkPatches), nameof(LocationOfTownsfolkPatches.GetMapCoordinatesForNPCTranspiler))
            );
            //QQQ2
            harmony.Patch(
                original: AccessTools.Method("UIInfoSuite2.UIElements.LocationOfTownsfolk:DrawNPC"),
                transpiler: new HarmonyMethod(typeof(LocationOfTownsfolkPatches), nameof(LocationOfTownsfolkPatches.DrawNPCTranspiler))
            );
        }
    }
}

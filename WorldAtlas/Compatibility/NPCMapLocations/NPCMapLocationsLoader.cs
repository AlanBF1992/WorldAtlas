using HarmonyLib;
using StardewModdingAPI;
using WorldAtlas.Compatibility.NPCMapLocations.Patches;

namespace WorldAtlas.Compatibility.NPCMapLocations
{
    internal static class NPCMapLocationsLoader
    {
        internal static void Loader(IModHelper _, Harmony harmony)
        {
            NPCMapLocationsPatches(harmony);
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private static void NPCMapLocationsPatches(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method("NPCMapLocations.ModEntry:GetWorldMapPosition"),
                transpiler: new HarmonyMethod(typeof(ModEntryPatches), nameof(ModEntryPatches.GetWorldMapPositionTranspiler))
            );
        }
    }
}

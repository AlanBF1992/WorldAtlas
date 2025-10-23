using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.WorldMaps;
using System.Reflection;
using System.Reflection.Emit;
using WorldAtlas.Patches;

namespace WorldAtlas.Compatibility.NPCMapLocations.Patches
{
    internal static class ModEntryPatches
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> GetWorldMapPositionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo BaseGetPositionDataInfo = AccessTools.Method(typeof(WorldMapManager), nameof(WorldMapManager.GetPositionData), [typeof(GameLocation) , typeof(Point)]);
                MethodInfo GingerPositionDataInfo = AccessTools.Method(typeof(ModEntryPatches), nameof(GingerPositionData));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call, BaseGetPositionDataInfo)
                    )
                    .ThrowIfNotMatch("(NPC)ModEntryPatches.GetWorldMapPositionTranspiler: IL code not found")
                    .SetOperandAndAdvance(GingerPositionDataInfo)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(GetWorldMapPositionTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static MapAreaPositionWithContext? GingerPositionData(GameLocation location, Point tile)
        {
            if (!location.InIslandContext())
            {
                return WorldMapManager.GetPositionData(location, tile);
            }

            if (ModEntry.GingerIsland.GetPositionData(location, tile) is MapAreaPosition MAP)
            {
                return new MapAreaPositionWithContext(MAP, location, tile);
            }

            return null;
        }
    }
}

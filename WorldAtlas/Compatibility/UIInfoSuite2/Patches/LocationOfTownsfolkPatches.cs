using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.WorldMaps;
using System.Reflection;
using System.Reflection.Emit;
using WorldAtlas.Patches;

namespace WorldAtlas.Compatibility.UIInfoSuite2.Patches
{
    internal static class LocationOfTownsfolkPatches
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> GetMapCoordinatesForNPCTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo GetPositionDataInfo = AccessTools.Method(typeof(LocationOfTownsfolkPatches), nameof(GetPositionDataSafeNew));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("LocationOfTownsfolkPatches.GetMapCoordinatesForNPCTranspiler: IL code not found")
                    .Advance(1)
                    .SetOperandAndAdvance(
                        GetPositionDataInfo
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(GetMapCoordinatesForNPCTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> DrawNPCTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo GetPositionDataInfo = AccessTools.Method(typeof(LocationOfTownsfolkPatches), nameof(GetPositionDataSafeNew));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Newobj),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("LocationOfTownsfolkPatches.DrawNPCTranspiler: IL code not found")
                    .Advance(1)
                    .SetOperandAndAdvance(
                        GetPositionDataInfo
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(DrawNPCTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static MapAreaPosition? GetPositionDataSafeNew(GameLocation location, Point _)
        {
            return (MapPagePatches.GetPositionData(location, Game1.player.TilePoint)?.Data) ?? WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero)?.Data;
        }
    }
}

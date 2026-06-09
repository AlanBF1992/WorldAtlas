using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

namespace WorldAtlas.Patches
{
    public static class IClickableMenuPatch
    {
        public static bool receiveGamePadButtonPrefix(IClickableMenu __instance, Buttons button)
        {
            if (__instance is not MapPage mapPage) return true;

            switch (button)
            {
                case Buttons.Back:
                    MapPagePatches.ChangePage(mapPage);
                    return false;
                case Buttons.LeftShoulder:
                    MapPagePatches.ChangeRegionUp(mapPage);
                    return false;
                case Buttons.RightShoulder:
                    MapPagePatches.ChangeRegionDown(mapPage);
                    return false;
            }

            return true;
        }



        public static bool receiveScrollWheelActionPrefix(IClickableMenu __instance, int direction)
        {
            if (__instance is not MapPage mapPage) return true;

            switch (direction)
            {
                case > 0:
                    MapPagePatches.ChangeRegionUp(mapPage);
                    return false;
                case < 0:
                    MapPagePatches.ChangeRegionDown(mapPage);
                    return false;
            }

            return true;
        }
    }
}

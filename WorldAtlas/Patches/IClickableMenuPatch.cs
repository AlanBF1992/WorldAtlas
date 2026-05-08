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
                    MapPagePatches.PageNumber = MapPagePatches.PageNumber == 0 ? 1 : 0;
                    MapPagePatches.SelectedComponentId = 2;
                    MapPagePatches.createRegionComponents();
                    break;
                case Buttons.LeftShoulder:
                    if (MapPagePatches.SelectedComponentId > 2)
                    {
                        MapPagePatches.SelectedComponentId--;
                    }
                    else
                    {
                        MapPagePatches.SelectedComponentId = MapPagePatches.regionComponents.Count + 1;
                    }
                    break;
                case Buttons.RightShoulder:
                    if (MapPagePatches.SelectedComponentId < MapPagePatches.regionComponents.Count + 1)
                    {
                        MapPagePatches.SelectedComponentId++;
                    }
                    else
                    {
                        MapPagePatches.SelectedComponentId = 2;
                    }
                    break;
            }

            ModEntry.SelectedRegionInfo = MapPagePatches.CurrentPageRegionInfo[MapPagePatches.SelectedComponentId - 2];
            MapPagePatches.ReconstructPage(mapPage);
            return false;

        }



        public static bool receiveScrollWheelActionPrefix(IClickableMenu __instance, int direction)
        {
            if (__instance is not MapPage mapPage) return true;

            switch (direction)
            {
                case < 0:
                    if (MapPagePatches.SelectedComponentId < MapPagePatches.regionComponents.Count + 1)
                    {
                        MapPagePatches.SelectedComponentId++;
                    }
                    else
                    {
                        MapPagePatches.SelectedComponentId = 2;
                    }
                    break;
                case > 0:
                    if (MapPagePatches.SelectedComponentId > 2)
                    {
                        MapPagePatches.SelectedComponentId--;
                    }
                    else
                    {
                        MapPagePatches.SelectedComponentId = MapPagePatches.regionComponents.Count + 1;
                    }
                    break;
            }

            ModEntry.SelectedRegionInfo = MapPagePatches.CurrentPageRegionInfo[MapPagePatches.SelectedComponentId - 2];
            MapPagePatches.ReconstructPage(mapPage);
            return false;
        }
    }
}

using StardewValley;
using StardewValley.WorldMaps;

namespace WorldAtlas
{
    public class ModData
    {
        public List<MapRegionJson> Regions { get; set; } = [];
    }

    public class MapRegionJson(string id, string name, bool isVisible)
    {
        public string Id { get; set; } = id;
        public string Name { get; set; } = name;
        public bool IsVisible { get; set; } = isVisible;
        public int PageNum { get; set; } = 0;
    }

    public class RegionInfo(GameLocation location, MapRegion region, string displayName)
    {
        public GameLocation Location { get; set; } = location;
        public MapRegion Region { get; set; } = region;
        public string DisplayName { get; set; } = displayName;
        public int PageNum { get; set; } = 0;
    }
}

using System.Collections.Generic;

public enum WayType
{
    Other,
    Water,
    Grass,
    Wood,
    Building,
    Brownfield,
    Industrial,
    Residential,
    Farmland,
    Wetland,
    Construction,
    Park,
    Canal,
    Commercial,
    Aerodrome,
    Runway,
    Taxiway
}

public class Way
{
    private const float floorHeight = 1;

    public long id;
    public List<long> nodeIndexes = new List<long>();
    public Dictionary<string, string> tags = new Dictionary<string, string>();
    public long nextWay;
    public WayType type = WayType.Other;

    public float height = 0;

    public void AddTag(string key, string value)
    {
        tags.Add(key, value);
        if (type == WayType.Other) type = GetType(key, value);
        if (tags.TryGetValue("building:levels", out string tempValue))
        {
            height = floorHeight * float.Parse(tempValue);
        }
        if (tags.TryGetValue("maxheight", out string tempValue2))
        {
            float.TryParse(tempValue2, out height);
        }
        if (tags.TryGetValue("level", out string tempValue3))
        {
            float.TryParse(tempValue3, out height);
        }
        if (tags.TryGetValue("isced:level", out string tempValue4))
        {
            float.TryParse(tempValue4, out height);
        }
        if (tags.TryGetValue("roof:levels", out string tempValue5))
        {
            float.TryParse(tempValue5, out height);
        }
        if (tags.TryGetValue("height", out string tempValue6))
        {
            float.TryParse(tempValue6, out height);
        }
        if(height == 0 && type == WayType.Building)
        {
            height = 1;
        }
        DebugTags.Instance.AddKey(key);
    }

    private WayType GetType(string key, string value)
    {
        if (key == "natural" && value == "water") return WayType.Water;
        if (key == "natural" && value == "wood") return WayType.Wood;
        if (key == "natural" && value == "scrub") return WayType.Wood;
        if (key == "natural" && value == "wetland") return WayType.Wetland;
        if (key == "natural" && value == "grassland") return WayType.Grass;
        if (key == "landuse" && value == "brownfield") return WayType.Brownfield;
        if (key == "landuse" && value == "construction") return WayType.Construction;
        if (key == "landuse" && value == "industrial") return WayType.Industrial;
        if (key == "landuse" && value == "residential") return WayType.Residential;
        if (key == "landuse" && value == "farmyard") return WayType.Farmland;
        if (key == "landuse" && value == "commercial") return WayType.Commercial;
        if (key == "landuse" && value == "retail") return WayType.Commercial;
        if (key == "leisure" && value == "garden") return WayType.Wood;
        if (key == "leisure" && value == "park") return WayType.Park;
        if (key == "waterway" && value == "canal") return WayType.Canal;
        if (key == "aerodrome" || value == "aerodrome") return WayType.Aerodrome;
        if (key == "area:aeroway" && value == "runway") return WayType.Runway;
        else if (key == "water") return WayType.Water;
        else if (value == "grass") return WayType.Grass;
        else if (key == "building") return WayType.Building;
        else if (key == "aeroway" && value == "taxiway") return WayType.Taxiway;
        return WayType.Other;
    }
}

using System.Collections.Generic;

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
        if(tags.TryGetValue("building:levels", out string tempValue))
        {
            height = floorHeight * float.Parse(tempValue);
        }
    }

    private WayType GetType(string key, string value)
    {
        if (key == "natural" && value == "water") return WayType.Water;
        if (key == "natural" && value == "wood") return WayType.Wood;
        if (key == "natural" && value == "scrub") return WayType.Wood;
        if (key == "natural" && value == "grassland") return WayType.Grass;
        if (key == "landuse" && value == "brownfield") return WayType.Brownfield;
        if (key == "landuse" && value == "industrial") return WayType.Industrial;
        if (key == "leisure" && value == "garden") return WayType.Wood;
        else if (key == "water") return WayType.Water;
        else if (value == "grass") return WayType.Grass;
        else if (key == "building") return WayType.Building;
        else if (key == "aeroway" && value == "taxiway") return WayType.Taxiway;
        return WayType.Other;
    }
}

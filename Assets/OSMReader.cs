using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public enum WayType
{
    Other,
    Water,
    Grass,
    Wood,
    Building,
    Brownfield,
    Industrial,
    Taxiway
}

public class OSMReader : MonoBehaviour
{
    public TextAsset osmData;
    public int lineCount;
    public Transform mapParent;
    public string dataPath;
    public int nodeCount;
    public int wayCount;

    public bool limitIDs = false;

    public List<long> excludeIDs;
    public List<long> exclusiveIDs;

    public List<string> skipKeys = new List<string>();

    public Dictionary<long, Node> nodes = new Dictionary<long, Node>();
    public List<Way> ways = new List<Way>();

    public List<(string, string)> keyValueSkip = new List<(string, string)> ();

    private WayRender render;
    private WayRender Render { get { if (render == null) render = GetComponent<WayRender>(); return render; } }

    public void Import()
    {
        XDocument document = XDocument.Parse(osmData.text);
        PopulateNodes(document);
        PopulateWays(document);
    }

    private void PopulateNodes(XDocument document)
    {
        nodes = new Dictionary<long, Node>();
        nodeCount = 0;
        foreach (XElement element in document.Descendants("node"))
        {
            Node newNode = new()
            {
                id = long.Parse(element.Attribute("id").Value),
                lat = float.Parse(element.Attribute("lat").Value),
                lon = float.Parse(element.Attribute("lon").Value)
            };
            nodes.Add(newNode.id, newNode);
            nodeCount++;
        }
    }

    private void PopulateWays(XDocument document)
    {
        ways = new List<Way>();
        foreach (XElement element in document.Descendants("way"))
        {
            Way newWay = new()
            {
                id = long.Parse(element.Attribute("id").Value)
            };
            PopulateTags(element, newWay);
            PopulateNodes(element, newWay);
            ways.Add(newWay);
        }
        wayCount = ways.Count;
    }

    private static void PopulateTags(XElement element, Way newWay)
    {
        foreach (XElement tag in element.Descendants("tag"))
        {
            string key = tag.Attribute("k").Value.ToLower();
            string value = tag.Attribute("v").Value.ToLower();
            newWay.AddTag(key, value);
        }
    }

    private void PopulateNodes(XElement element, Way newWay)
    {
        foreach (XElement nodes in element.Descendants("nd"))
        {
            long nodeIndex = long.Parse(nodes.Attribute("ref").Value);
            newWay.nodeIndexes.Add(nodeIndex);
            this.nodes[nodeIndex].ways.Add(newWay);
        }
    }

    public void DestroyChildern()
    {
        for(int i = mapParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(mapParent.GetChild(i).gameObject);
        }
    }

    private bool CheckSkipTags(Dictionary<string, string> tags)
    {
        foreach(KeyValuePair<string, string> tag in tags)
        {
            if(skipKeys.Contains(tag.Key)) return true;
        }
        return false;
    }

    private bool SkipWay(Way way)
    {
        if (way.tags.Count == 0) return true;
        if (CheckSkipTags(way.tags)) return true;
        if (limitIDs && !exclusiveIDs.Contains(way.id)) return true;
        if (limitIDs && excludeIDs.Contains(way.id)) return true;
        return false;
    }

    public void PopulateWays()
    {
        DestroyChildern();
        int offset = 0;
        if (ways.Count == 0) Import();

        for(int i = 0; i < lineCount; i++)
        {
            int index = i + offset;
            if (index >= ways.Count) break;
            Way way = ways[index];
            if (SkipWay(way))
            {
                offset++;
                i--;
                continue;
            }

            Render.RenderWay(way, mapParent);
        }
        mapParent.name = $"Main - {mapParent.childCount}";
    }
}

using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Node
{
    public long id;
    public float lat;
    public float lon;
    public List<Way> ways = new List<Way>();
}

public class Way
{
    public long id;
    public List<long> nodeIndexes = new List<long>();
    public Dictionary<string, string> tags = new Dictionary<string, string>();
    public long nextWay;
}

public class OSMReader : MonoBehaviour
{
    public int lineCount;
    public Material lineMaterial;
    public Transform mapParent;
    public string dataPath;
    public int nodeCount;
    public int wayCount;

    public Dictionary<long, Node> nodes = new Dictionary<long, Node>();
    public List<Way> ways = new List<Way>();

    public void Import()
    {
        XDocument document = XDocument.Load(dataPath);
        PopulateNodes(document);
        PopulateWays(document);
    }

    private void PopulateNodes(XDocument document)
    {
        nodes = new Dictionary<long, Node>();
        nodeCount = 0;
        foreach (XElement element in document.Descendants("node"))
        {
            Node newNode = new Node();
            newNode.id = long.Parse(element.Attribute("id").Value);
            newNode.lat = float.Parse(element.Attribute("lat").Value);
            newNode.lon = float.Parse(element.Attribute("lon").Value);
            nodes.Add(newNode.id, newNode);
            nodeCount++;
        }
    }

    private void PopulateWays(XDocument document)
    {
        ways = new List<Way>();
        wayCount = 0;
        foreach (XElement element in document.Descendants("way"))
        {
            Way newWay = new Way();
            XAttribute xAttribute = element.Attribute("id");
            newWay.id = long.Parse(xAttribute.Value);
            foreach(XElement tag in element.Descendants("tag"))
            {
                newWay.tags.Add(tag.Attribute("k").Value.ToLower(), tag.Attribute("v").Value.ToLower());
            }
            foreach (XElement nodes in element.Descendants("nd"))
            {
                long nodeIndex = long.Parse(nodes.Attribute("ref").Value);
                newWay.nodeIndexes.Add(nodeIndex);
                this.nodes[nodeIndex].ways.Add(newWay);
            }
            ways.Add(newWay);
            wayCount++;
        }
    }

    private void DestroyChildern(Transform map)
    {
        while(map.childCount > 0)
        {
            DestroyImmediate(map.GetChild(0));
        }
    }

    public void PopulateWays()
    {
        DestroyChildern(mapParent);
        List<long> usedWays = new List<long>();
        int offset = 0;
        Import();
        for(int i = 0; i < lineCount; i++)
        {
            int index = i + offset;
            if (index >= ways.Count) return;
            Way way = ways[index];
            if (usedWays.Contains(way.id)/* || !way.tags.ContainsKey("aeroway")*/)
            {
                offset++;
                i--;
                continue;
            }
            GameObject newWayObject = new GameObject($"Way {i} - {/*way.tags["aeroway"]*/0}");
            newWayObject.transform.SetParent(mapParent);

            WayData wayData = newWayObject.AddComponent<WayData>();
            wayData.data.Add($"ID = {way.id}");
            foreach(KeyValuePair<string, string> tag in way.tags)
            {
                wayData.data.Add($"{tag.Key} - {tag.Value}");
            }

            LineRenderer lineRenderer = newWayObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            List<Vector3> positions = new List<Vector3>();
            AddPositions(ref positions, ref usedWays, way);
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
    }

    private void AddPositions(ref List<Vector3> positions, ref List<long> usedWays, Way way)
    {
        usedWays.Add(way.id);
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= new Vector3(51.508f, 0, 0.070f);
            newPosition *= 10000;
            positions.Add(newPosition);
        }
        Node lastNode = nodes[way.nodeIndexes[way.nodeIndexes.Count - 1]];
        if (lastNode.ways.Count > 0)
        {
            foreach(Way wayId in lastNode.ways)
            {
                if (usedWays.Contains(wayId.id)) continue;
                //AddPositions(ref positions, ref usedWays, wayId);
                //break;
            }
        }
    }

    public void Checks()
    {
        Import();
        List<long> indexUsed = new List<long>();
        int duplicatCount = 0;

        foreach(Way way in ways)
        {
            foreach(long childElement in way.nodeIndexes)
            {
                if (indexUsed.Contains(childElement))
                {
                    duplicatCount++;
                    continue;
                }
                indexUsed.Add(childElement);
            }
        }
        Debug.Log($"Number of duplicates = {duplicatCount}");
    }
}
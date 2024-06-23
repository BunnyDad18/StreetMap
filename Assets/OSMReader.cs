using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Node
{
    public long id;
    public float lat;
    public float lon;
}

public class Way
{
    public List<long> nodeIndexes = new List<long>();
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
        nodes = new Dictionary<long, Node>();
        ways = new List<Way>();
        nodeCount = 0;
        wayCount = 0;
        XDocument document = XDocument.Load(dataPath);
        foreach (XElement element in document.Descendants("node"))
        {
            Node newNode = new Node();
            newNode.id = long.Parse(element.Attribute("id").Value);
            newNode.lat = float.Parse(element.Attribute("lat").Value);
            newNode.lon = float.Parse(element.Attribute("lon").Value);
            nodes.Add(newNode.id, newNode);
            nodeCount++;
        }

        foreach (XElement element in document.Descendants("way"))
        {
            Way newWay = new Way();
            foreach(XElement childElement in element.Descendants("nd"))
            {
                newWay.nodeIndexes.Add(long.Parse(childElement.Attribute("ref").Value));
            }
            ways.Add(newWay);
            wayCount++;
        }
    }

    public void PopulateWays()
    {
        Import();
        for(int i = 0; i < lineCount; i++)
        {
            Way way = ways[i];
            Debug.Log(way.nodeIndexes.Count);
            GameObject newWayObject = new GameObject($"Way {i}");
            newWayObject.transform.SetParent(mapParent);

            LineRenderer lineRenderer = newWayObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            List<Vector3> positions = new List<Vector3>();
            foreach (long childElement in way.nodeIndexes)
            {
                Node node = nodes[childElement];
                Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
                newPosition -= new Vector3(51.508f, 0, 0.070f);
                newPosition *= 10000;
                positions.Add(newPosition);
            }
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
    }
}

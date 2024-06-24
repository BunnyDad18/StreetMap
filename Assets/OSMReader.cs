using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public enum WayType
{
    Other,
    Water,
    Grass,
    Building,
}

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
    public WayType type = WayType.Other;
}

public class OSMReader : MonoBehaviour
{
    public TextAsset osmData;
    public int lineCount;
    public Material lineMaterial;
    public Material meshMaterial;
    public Transform mapParent;
    public string dataPath;
    public int nodeCount;
    public int wayCount;

    public List<string> skipKeys = new List<string>();

    public Dictionary<long, Node> nodes = new Dictionary<long, Node>();
    public List<Way> ways = new List<Way>();

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
                string key = tag.Attribute("k").Value.ToLower();
                string value = tag.Attribute("v").Value.ToLower();
                newWay.tags.Add(key, value);
                if(newWay.type == WayType.Other) newWay.type = GetType(key, value);
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

    private WayType GetType(string key, string value)
    {
        if (key == "natural" && value == "water") return WayType.Water;
        else if (key == "water") return WayType.Water;
        else if (value == "grass") return WayType.Grass;
        else if (key == "building") return WayType.Building;
        return WayType.Other;
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

    

    public void PopulateWays()
    {
        DestroyChildern();
        List<long> usedWays = new List<long>();
        int offset = 0;
        if (ways.Count == 0) Import();

        for(int i = 0; i < lineCount; i++)
        {
            int index = i + offset;
            if (index >= ways.Count) return;
            Way way = ways[index];
            if (usedWays.Contains(way.id) || way.tags.Count == 0 || CheckSkipTags(way.tags))
            {
                offset++;
                i--;
                continue;
            }
            usedWays.Add(way.id);

            Color wayColor = GetColor(way);

            if (way.type is WayType.Water or WayType.Grass)
            {
                GameObject newWayObject = SetupWayGameObject(way, i, index);
                List<Vector3> positions = new List<Vector3>();
                AddPositions(ref positions, way);
                SetupMesh(newWayObject, positions, wayColor);
                continue;
            }
            if (way.type == WayType.Building)
            {
                GameObject newWayObject = SetupWayGameObject(way, i, index);
                List<Vector3> positions = new List<Vector3>();
                AddPositions(ref positions, way);
                SetupMesh(newWayObject, positions, wayColor, 5);
                continue;
            }
            if (way.type == WayType.Other)
            {
                GameObject newWayObject = SetupWayGameObject(way, i, index);
                List<Vector3> positions = new List<Vector3>();
                AddPositions(ref positions, way);
                SetupLineRenderer(newWayObject, positions, wayColor);
                continue;
            }
        }
    }

    public Color GetColor(Way way)
    {
        if (way.type == WayType.Water) return Color.blue;
        if (way.type == WayType.Grass) return Color.green;
        if (way.type == WayType.Building) return Color.gray;
        return Color.white;
    }

    private GameObject SetupWayGameObject(Way way, int i, int index)
    {
        GameObject newWayObject = new GameObject($"Way {i} - {index} - {0}");
        newWayObject.transform.SetParent(mapParent);

        WayData wayData = newWayObject.AddComponent<WayData>();
        wayData.data.Add($"ID = {way.id}");
        wayData.type = way.type;
        foreach (KeyValuePair<string, string> tag in way.tags)
        {
            wayData.data.Add($"{tag.Key} - {tag.Value}");
        }
        return newWayObject;
    }

    private void SetupMesh(GameObject gameObject, List<Vector3> positions, Color color, float height = 0)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = OSMMeshBuilder.Get(positions, height);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(meshMaterial);
        renderer.sharedMaterial.color = color;
    }

    private void SetupLineRenderer(GameObject gameObject, List<Vector3> positions, Color color)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.startWidth = .1f;
        lineRenderer.endWidth = .1f;
    }

    private void AddPositions(ref List<Vector3> positions, Way way)
    {
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= new Vector3(51.508f, 0, 0.070f);
            newPosition.z *= -1;
            newPosition *= 10000;
            positions.Add(newPosition);
        }
        //Node lastNode = nodes[way.nodeIndexes[way.nodeIndexes.Count - 1]];
        //if (lastNode.ways.Count > 0)
        //{
        //    foreach(Way wayId in lastNode.ways)
        //    {
        //        if (usedWays.Contains(wayId.id)) continue;
        //        //AddPositions(ref positions, ref usedWays, wayId);
        //        //break;
        //    }
        //}
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

public class OSMMeshBuilder
{
    public static Mesh Get(List<Vector3> positions, float height)
    {
        //Polygon2D polygon = Polygon2D.Contour(ConvertToVector2Array(positions));

        //Triangulation2D triangulation = new Triangulation2D(polygon, 22.5f);

        //Mesh newMesh = triangulation.Build();

        ProBuilderMesh mesh = ProBuilderMesh.Create();
        mesh.CreateShapeFromPolygon(positions, height, false);

        mesh.ToMesh();
        mesh.Refresh();

        Mesh wayMesh = new Mesh();
        wayMesh.SetVertices(mesh.GetVertices().Select(o => o.position).ToList());
        List<int> indicies = new List<int>();
        mesh.faces.ToList().ForEach(o => { indicies.AddRange(o.indexes.ToList()); });
        wayMesh.SetIndices(indicies, MeshTopology.Triangles, 0);
        wayMesh.RecalculateBounds();
        wayMesh.RecalculateNormals();

        GameObject.DestroyImmediate(mesh.gameObject);

        return wayMesh;
    }

    private static Vector2[] ConvertToVector2Array(List<Vector3> vector3s)
    {
        List<Vector2> result = new List<Vector2>();
        foreach (Vector3 vector3 in vector3s)
        {
            result.Add(new Vector2(vector3.x, vector3.z));
        }
        return result.ToArray();
    }
}
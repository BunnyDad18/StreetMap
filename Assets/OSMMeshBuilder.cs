using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OSMMeshBuilder
{
    public static Mesh Get(List<Vector3> positions, float height)
    {
        Mesh wayMesh = new Mesh();
        Vector2[] vector2s = ConvertToVector2(positions);
        List<int> indices = new Triangulator(vector2s).Triangulate().ToList();

        if(height > 0)
        {
            AddHeight(ref positions, ref indices, height);
        }

        wayMesh.SetVertices(positions);
        wayMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        wayMesh.RecalculateBounds();
        wayMesh.RecalculateNormals();

        return wayMesh;
    }

    private static void AddHeight(ref List<Vector3> positions, ref List<int> indices, float height)
    {
        int count = positions.Count;
        for(int i = 0; i < count; i++)
        {
            positions.Add(positions[i]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions[i] += Vector3.up * height;
        }
    }

    private static Vector2[] ConvertToVector2(List<Vector3> vector3s)
    {
        Vector2[] vector2s = new Vector2[vector3s.Count];
        for(int i = 0; i < vector3s.Count; i++)
        {
            vector2s[i] = new Vector2(vector3s[i].x, vector3s[i].z);
        }
        return vector2s;
    }

    public static float GetAngle(Vector3 pA, Vector3 pB, Vector3 pC)
    {
        Vector3 v1 = pB - pA;
        Vector3 v2 = pC - pB;
        return Vector3.Angle(v1, v2);
    }
}
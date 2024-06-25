using System.Collections.Generic;

public class Node
{
    public long id;
    public float lat;
    public float lon;
    public List<Way> ways = new List<Way>();
}

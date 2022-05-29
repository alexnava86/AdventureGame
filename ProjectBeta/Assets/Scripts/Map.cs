using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Map
{
    public string MapName;
    public int TileCount;
    public int ObjectCount;
    public int LayerCount;
    public string MapFile;
    public MapNode[,,] Node;
    public bool hasBeenRead;

    public Map(string name, string mFile)
    {
        MapName = name;
        MapFile = mFile;
    }
}

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

[System.Serializable]
public class Map
{
    public string MapName;
    public int TileCount;
    public int ObjectCount;
    public int LayerCount;
    public MapNode[,,] Node;
    public bool hasBeenRead;
    public string MapData;

    public Map(string name, string text)
    {
        MapName = name;
        MapData = text;
    }
}
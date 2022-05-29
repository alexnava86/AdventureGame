using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Tilemaps;
using System;
using System.Text.RegularExpressions;
public class MapManager : MonoBehaviour
{
    public PhysicsMaterial2D tileMaterial;
    private TextAsset mapFile;
    [SerializeField]
    private Map map;
    private Tilemap tilemap; //
    private List<Tilemap> tilemaps = new List<Tilemap>(); //
    private Tile[] tileset;
    private List<GameObject> objects = new List<GameObject>();
    private int tileCount = 0;
    private int objectCount = 0;
    public delegate void Message(string msg);
    public static event Message OnMessage;
    public static MapManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        //Item.OnRemove += RemoveItemFromMap;
    }

    private void Start()
    {
        //if (hasBeenRead != true)
        {
            ReadMap();
        }
        //else
        {
            //LoadMap ();
        }
    }

    public void ReadMap()
    {
        mapFile = Resources.Load("Maps/" + Application.loadedLevelName) as TextAsset; //this line, line 134, line 135 eliminate the need for the SetMapData method?
        if (mapFile != null)
        {
            map = new Map(mapFile.name, mapFile.text);
            JSONNode json = JSON.Parse(mapFile.text);

            //uint flippedHorizontally = 0x80000000;
            //uint flippedVertically = 0x40000000;
            //uint flippedDiagonally = 0x20000000;
            int width;
            int height;
            int x;
            int y;
            uint tileID;
            uint objectID;
            int objectIDOffset;
            int tileIDOffset;
            //float xscale;
            //float yscale;

            map.LayerCount = json["layers"].AsArray.Count;
            for (int layer = 0; layer < json["layers"].AsArray.Count; layer++)
            {
                if (json["layers"][layer]["name"].ToString().Trim('"') == "Foreground")
                {
                    tilemaps.Add(CreateTilemap(json["layers"][layer]["name"].ToString().Trim('"'), 0.5f, 0.5f, 1f));
                    tilemaps[layer].gameObject.AddComponent<TilemapCollider2D>();
                    tilemaps[layer].gameObject.AddComponent<CompositeCollider2D>();
                    tilemaps[layer].gameObject.GetComponent<TilemapCollider2D>().usedByComposite = true;
                    tilemaps[layer].gameObject.GetComponent<CompositeCollider2D>().vertexDistance = 0f;
                    tilemaps[layer].gameObject.GetComponent<CompositeCollider2D>().edgeRadius = 0f;
                    tilemaps[layer].gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    tilemaps[layer].gameObject.GetComponent<Rigidbody2D>().sharedMaterial = this.tileMaterial;
                    tilemaps[layer].gameObject.layer = 13; // Foreground Layer
                }
                else
                {
                    tilemaps.Add(CreateTilemap(json["layers"][layer]["name"].ToString().Trim('"'), 0.5f, 0.5f, 1f));
                    //Debug.Log(tilemaps[layer].size);
                }

                if (json["layers"][layer]["type"].ToString().Trim('"') == "tilelayer")
                {
                    string path = json["tilesets"][0]["image"].ToString().Trim('"').Remove(0, 3); //Removes the "../" at the beginning of path...
                    path = path.Remove(path.Length - 4, 4); //Removes the ".png" file extension at the end of path...
                    string[] paths = path.Split('/');
                    path = "";
                    foreach (string s in paths)
                    {
                        if (s != paths[paths.Length - 1])
                            path += s + '/';
                    }
                    path = path + "Tiles/" + paths[paths.Length - 1] + "/";
                    tileset = Resources.LoadAll(path).OfType<Tile>().ToArray();
                    List<Tile> tiles = tileset.ToList();
                    tiles = tiles.OrderBy(i => Convert.ToInt16(Regex.Match(i.name, @"_[0-9]+").Captures[0].Value.Trim('_'))).ToList();
                    width = json["layers"][layer]["width"].AsInt;
                    height = json["layers"][layer]["height"].AsInt;
                    map.Node = new MapNode[map.LayerCount, width, height];

                    for (int i = 0; i < width * height; i++)
                    {
                        tileID = (json["layers"][layer]["data"][i].AsUInt);
                        //xscale = ((tileID & flippedHorizontally) != 0) ? -1.0f : 1.0f;
                        //yscale = ((tileID & flippedVertically) != 0) ? -1.0f : 1.0f;
                        //tileID = (tileID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));

                        if (tileID != 0)
                        {
                            tileIDOffset = json["tilesets"][0]["firstgid"].AsInt;
                            y = i / width;
                            x = i % width;
                            map.Node[layer, x, y].X = x;
                            map.Node[layer, x, y].Y = y;
                            tilemaps[layer].SetTile(new Vector3Int(x, -y, 0), tiles[(int)tileID - tileIDOffset]);
                            //Debug.Log(tilemaps[layer].GetTile(new Vector3Int(x, -y, 0)));
                            //Debug.Log(tileID - tileIDOffset);//tilemaps[layer].size);
                            //node [layer, x, y].tile.GetComponent<SpriteRenderer> ().material = tileMaterial;
                        }
                        tileCount = i;
                    }
                }
                else if (json["layers"][layer]["type"].ToString().Trim('"') == "objectgroup")
                {
                    GameObject currentObject;
                    objectCount += json["layers"][layer]["objects"].Count;

                    for (int i = 0; i < objectCount; i++)
                    {
                        objectID = json["layers"][layer]["objects"][i]["gid"].AsUInt;
                        //objectID = (objectID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));

                        if (objectID != 0)
                        {
                            try
                            {
                                objectIDOffset = json["tilesets"][layer]["firstgid"].AsInt;
                                string path = json["tilesets"][layer]["tiles"][(int)(objectID - objectIDOffset)]["image"].ToString().Trim('"').Remove(0, 3);
                                path = path.Remove(path.Length - 4, 4); // Remove the .png or other file extension from the filename...
                                path = path.Remove(0, 4); // Remove the enclosing folder name "Art"...
                                path = "Prefabs/" + path; // Place "Prefabs/" before corresponding folder path
                                width = json["layers"][layer]["objects"][i]["width"].AsInt;
                                height = json["layers"][layer]["objects"][i]["height"].AsInt;
                                x = (int)json["layers"][layer]["objects"][i]["x"].AsFloat + width / 2 + 2;
                                y = (int)json["layers"][layer]["objects"][i]["y"].AsFloat * -1 + 64;
                                //Debug.Log(json["layers"][layer]["objects"][i]);
                                currentObject = Instantiate(Resources.Load(path, typeof(GameObject)), new Vector2((float)x, (float)y), Quaternion.identity) as GameObject;
                                currentObject.GetComponent<SpriteRenderer>().sortingLayerName = "World";
                                currentObject.transform.position = new Vector3(currentObject.transform.position.x, currentObject.transform.position.y + currentObject.GetComponent<SpriteRenderer>().sprite.pivot.y, 0f);
                                //currentObject<SpriteRenderer>().material = //objectMaterial;
                                //json["layers"][layer]["objects"].Remove(i);
                            }
                            catch
                            {//(Exception e) {
                                if (OnMessage != null)
                                {
                                    //OnMessage (e.Message.ToString ());
                                    OnMessage("Prefab does not exist in corresponding folder.");
                                }
                            }
                        }
                    }
                }
            }
            if (OnMessage != null)
            {
                OnMessage.Invoke("TileCount=" + tileCount.ToString());
                OnMessage.Invoke("ObjectCount=" + objectCount.ToString());
            }
        }
        LoadMap();
    }

    private Tilemap CreateTilemap(string tilemapName, float x, float y, float alpha)
    {
        GameObject go = new GameObject(tilemapName);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();

        tm.tileAnchor = new Vector3(x, y, 0);
        tm.color = new Color(1f, 1f, 1f, alpha);
        go.transform.SetParent(this.gameObject.transform);
        tr.sortingLayerName = tilemapName;

        return tm;
    }
    public void RemoveItemFromMap(int objectID)
    {
        JSONNode json = JSON.Parse(map.MapFile);

        for (int layer = 0; layer < json["layers"].AsArray.Count; layer++)
        {
            if (json["layers"][layer]["type"].ToString().Trim('"') == "objectgroup")
            {
                if (json["layers"][layer]["objects"][objectID]["id"].AsInt == objectID + 1)
                {
                    json["layers"][layer]["objects"].Remove(objectID);
                }
            }
        }
        //map.MapFile = JsonUtility.ToJson(json, true);
        map.MapFile = json.ToString(); //.Split();
    }
    private void SaveMap()
    {
        string mapJSON = JsonUtility.ToJson(map.MapFile, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        string path = Application.persistentDataPath + "/Temp/Maps/" + mapFile.name + ".dat";

        if (File.Exists(path) != false)
        {
            file = File.Open(path, FileMode.Open);
            bf.Serialize(file, map.MapFile);
        }
        else
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Temp/Maps");
            file = File.Create(Application.persistentDataPath + "/Temp/Maps/" + mapFile.name + ".dat");
            bf.Serialize(file, map.MapFile);
        }
    }

    private void LoadMap()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        string path = Application.persistentDataPath + "/Temp/Maps/" + mapFile.name + ".dat";

        if (File.Exists(path) != false)
        {
            file = File.Open(path, FileMode.Open);
            string data = (string)bf.Deserialize(file);
            //bf.Serialize(file, data);
            file.Close();
            map.MapFile = data;
            //Debug.Log(map.MapFile);
        }
    }

    private void OnDisable()
    {
        SaveMap();
        //Item.OnRemove -= RemoveItemFromMap;
        //Debug.Log ("TEST"); //Triggers
    }
}
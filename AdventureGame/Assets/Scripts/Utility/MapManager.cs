using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Tilemaps;
using System;
using System.Text.RegularExpressions;
//using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
public class MapManager : MonoBehaviour
{
    #region Variables
    public PhysicsMaterial2D tileMaterial;
    [SerializeField]
    public Map map;
    private TextAsset mapFile;
    private Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();
    private Dictionary<int, AnimatedTile> tileAnimations = new Dictionary<int, AnimatedTile>();
    private Dictionary<int, Tilemap> tilemaps = new Dictionary<int, Tilemap>();
    private Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
    private int tileCount = 0;
    private int objectCount = 0;
    #endregion

    #region Properties
    public int groundLayerID { get; set; }
    public float MaxHeight { get; set; }
    public float MaxWidth { get; set; }
    public delegate void Message(string msg);
    public static event Message OnMessage;
    public static MapManager Instance { get; private set; }
    #endregion

    #region MonoBehaviour
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
        //if (hasBeenRead != true)
        {
            ReadMap();
        }
        //else
        {
            //LoadMap ();
        }
    }

    private void OnEnable()
    {
        //Item.OnRemove += RemoveItemFromMap;
    }

    private void Start()
    {

    }

    private void OnDisable()
    {
        //SaveMap();
        //Item.OnRemove -= RemoveItemFromMap;
        //Debug.Log ("TEST"); //Triggers
    }
    #endregion

    #region Methods
    public void ReadMap()
    {
        mapFile = Resources.Load("Maps/" + SceneManager.GetActiveScene().name) as TextAsset;//Application.loadedLevelName) as TextAsset; //this line, line 134, line 135 eliminate the need for the SetMapData method?
        if (mapFile != null)
        {
            map = new Map(mapFile.name, mapFile.text);
            JSONNode json = JSON.Parse(mapFile.text);

            uint flippedHorizontally = 0x80000000;
            uint flippedVertically = 0x40000000;
            uint flippedDiagonally = 0x20000000;
            int width;
            int height;
            int x;
            int y;
            uint tileID;
            uint objectID;
            int objectIDOffset;
            int tileIDOffset;
            float xscale;
            float yscale;
            float rotation;
            map.LayerCount = json["layers"].Count;

            for (int tilesets = 0; tilesets < json["tilesets"].Count; tilesets++)
            {
                List<Tile> tileset = new List<Tile>();
                string path;
                try
                {
                    //Load the tile path, and tiles...
                    tileIDOffset = json["tilesets"][tilesets]["firstgid"].AsInt;
                    path = json["tilesets"][tilesets]["image"].ToString().Trim('"').Remove(0, 3); //Removes the "../" at the beginning of path...
                    path = path.Remove(path.Length - 4, 4); //Removes the ".png" file extension at the end of path...
                    string[] paths = path.Split('/');
                    path = "";
                    foreach (string s in paths)
                    {
                        if (s != paths[paths.Length - 1])
                            path += s + '/';
                    }
                    path = path + paths[paths.Length - 1] + "/Tiles/";
                    tileset = Resources.LoadAll(path).OfType<Tile>().ToList();
                    tileset = tileset.OrderBy(i => Convert.ToInt16(Regex.Match(i.name, @"_[0-9]+").Captures[0].Value.Trim('_'))).ToList();

                    //Add the loaded tiles to the tile dictionary...
                    for (int tile = 0; tile < json["tilesets"][tilesets]["tilecount"].AsInt; tile++)
                    {
                        tiles.Add((int)tile + tileIDOffset, tileset[tile]);
                    }

                    //Now create and add the tile animations...
                    for (int tileanims = 0; tileanims < json["tilesets"][tilesets]["tiles"].AsArray.Count; tileanims++)
                    {
                        if (json["tilesets"][tilesets]["tiles"][tileanims]["animation"] != null)
                        {
                            List<Sprite> frames = new List<Sprite>();
                            AnimatedTile at = new AnimatedTile();
                            int animationID = json["tilesets"][tilesets]["tiles"][tileanims]["id"].AsInt + tileIDOffset;
                            int frameCount = json["tilesets"][tilesets]["tiles"][tileanims]["animation"].AsArray.Count;

                            for (int animFrames = 0; animFrames < frameCount; animFrames++)
                            {
                                tileID = json["tilesets"][tilesets]["tiles"][tileanims]["animation"][animFrames]["tileid"].AsUInt;
                                frames.Add(tiles[(int)tileID + tileIDOffset].sprite);
                            }
                            at.m_AnimatedSprites = frames.ToArray();
                            tileAnimations.Add(animationID, at);
                        }
                    }
                }

                //Create the object sets and add to the object dictionary...
                catch
                {
                    if (json["tilesets"][tilesets]["grid"] != null)
                    {
                        for (int obj = 0; obj < json["tilesets"][tilesets]["tiles"].Count; obj++)
                        {
                            objectID = json["tilesets"][tilesets]["tiles"][obj]["id"].AsUInt;
                            objectIDOffset = json["tilesets"][tilesets]["firstgid"].AsInt;
                            path = json["tilesets"][tilesets]["tiles"][obj]["image"].ToString().Trim('"').Remove(0, 3);
                            path = path.Remove(path.Length - 4, 4); // Remove the .png or other file extension from the filename...
                            path = path.Remove(0, 4); // Remove the enclosing folder name "Art"...
                            path = "Prefabs/" + path; // Place "Prefabs/" before corresponding folder path
                            objects.Add((int)objectID + objectIDOffset, Resources.Load(path) as GameObject);
                        }
                    }
                    else
                    {
                        //Debug.Log("ERROR");
                    }
                    if (OnMessage != null)
                    {
                        //OnMessage (e.Message.ToString ());
                    }
                }
            }

            for (int layer = 0; layer < map.LayerCount; layer++)
            {
                //Load the ground/collision layer
                if (json["layers"][layer]["name"].ToString().Trim('"') == "Ground")
                {
                    string tint = json["layers"][layer]["tintcolor"] != null ? json["layers"][layer]["tintcolor"].ToString() : "#ffffff";
                    float alpha = json["layers"][layer]["opacity"] != null ? json["layers"][layer]["opacity"].AsFloat : 1f;

                    tilemaps.Add(layer, CreateTilemap(json["layers"][layer]["name"].ToString().Trim('"'), 0.5f, 0.5f, tint, alpha)); tilemaps[layer].gameObject.AddComponent<TilemapCollider2D>();
                    tilemaps[layer].gameObject.AddComponent<CompositeCollider2D>();
                    tilemaps[layer].gameObject.GetComponent<TilemapCollider2D>().usedByComposite = true;
                    tilemaps[layer].gameObject.GetComponent<CompositeCollider2D>().vertexDistance = 0f;
                    tilemaps[layer].gameObject.GetComponent<CompositeCollider2D>().edgeRadius = 0f;
                    tilemaps[layer].gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    tilemaps[layer].gameObject.GetComponent<Rigidbody2D>().sharedMaterial = this.tileMaterial;
                    tilemaps[layer].gameObject.layer = LayerMask.NameToLayer("Ground"); // 13 Foreground Layer
                    tilemaps[layer].gameObject.GetComponent<TilemapRenderer>().sortingOrder = layer;
                    groundLayerID = layer;
                }
                else if (json["layers"][layer]["type"].ToString().Trim('"') != "objectgroup")
                {
                    string tint = json["layers"][layer]["tintcolor"] != null ? json["layers"][layer]["tintcolor"].ToString() : "#ffffff";
                    float alpha = json["layers"][layer]["opacity"] != null ? json["layers"][layer]["opacity"].AsFloat : 1f;

                    tilemaps.Add(layer, CreateTilemap(json["layers"][layer]["name"].ToString().Trim('"'), 0.5f, 0.5f, tint, alpha));
                    tilemaps[layer].gameObject.GetComponent<TilemapRenderer>().sortingOrder = layer;
                }

                if (json["layers"][layer]["type"].ToString().Trim('"') == "tilelayer")
                {
                    width = json["layers"][layer]["width"].AsInt;
                    height = json["layers"][layer]["height"].AsInt;
                    MaxWidth = (MaxWidth < width) ? width : MaxWidth;
                    MaxHeight = (MaxHeight < height) ? height : MaxHeight;
                    map.Node = new MapNode[map.LayerCount, width, height];

                    for (int i = 0; i < width * height; i++)
                    {
                        tileID = (json["layers"][layer]["data"][i].AsUInt);
                        xscale = ((tileID & flippedHorizontally) != 0) ? -1f : 1f;
                        yscale = ((tileID & flippedVertically) != 0) ? -1f : 1f;
                        rotation = ((tileID & flippedDiagonally) != 0) ? -90f : 0f;
                        tileID = (tileID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));

                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, Mathf.Round(rotation * 2f), rotation * xscale * yscale), new Vector3(xscale, yscale, 1f));

                        if (tileID != 0)
                        {
                            y = i / width;
                            x = i % width;
                            map.Node[layer, x, y].X = x;
                            map.Node[layer, x, y].Y = y;
                            tilemaps[layer].SetTile(new Vector3Int(x, -y, 0), tiles[(int)tileID]);
                            tilemaps[layer].SetTransformMatrix(new Vector3Int(x, -y, 0), matrix);
                            if (tileAnimations.ContainsKey((int)tileID))
                            {
                                tilemaps[layer].SetTile(new Vector3Int(x, -y, 0), tileAnimations[(int)tileID]);
                            }
                            tileCount++;
                        }
                    }
                    if (json["layers"][layer]["parallaxx"] != null || json["layers"][layer]["parallaxy"] != null)
                    {
                        float xOffset = json["layers"][layer]["offsetx"] != null ? json["layers"][layer]["offsetx"].AsFloat : 0f;
                        float yOffset = json["layers"][layer]["offsety"] != null ? json["layers"][layer]["offsety"].AsFloat : 0f;
                        float pxFactor = json["layers"][layer]["parallaxx"] != null ? json["layers"][layer]["parallaxx"].AsFloat : 1f;//json["layers"][layer]["parallaxx"].AsFloat;
                        float pyFactor = json["layers"][layer]["parallaxy"] != null ? json["layers"][layer]["parallaxy"].AsFloat : 1f;//json["layers"][layer]["parallaxy"].AsFloat;
                        AddParallaxEffectToLayer(tilemaps[layer], xOffset, yOffset, pxFactor, pyFactor);
                    }
                }
                else if (json["layers"][layer]["type"].ToString().Trim('"') == "objectgroup")
                {
                    GameObject currentLayer = new GameObject(json["layers"][layer]["name"].ToString().Trim('"'));
                    GameObject currentObject;
                    objectCount += json["layers"][layer]["objects"].Count;

                    currentLayer.transform.SetParent(this.gameObject.transform);
                    for (int i = 0; i < objectCount; i++)
                    {
                        objectID = json["layers"][layer]["objects"][i]["gid"].AsUInt;
                        xscale = ((objectID & flippedHorizontally) != 0) ? -1f : 1f;
                        yscale = ((objectID & flippedVertically) != 0) ? -1f : 1f;
                        rotation = json["layers"][layer]["objects"][i]["rotation"].AsFloat * -1;
                        objectID = (objectID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));

                        if (objectID != 0)
                        {
                            try
                            {
                                string tint = json["layers"][layer]["tintcolor"] != null ? json["layers"][layer]["tintcolor"].ToString() : "#ffffff";
                                float alpha = json["layers"][layer]["opacity"] != null ? json["layers"][layer]["opacity"].AsFloat : 1f;
                                Color color;
                                ColorUtility.TryParseHtmlString(tint.Trim('"'), out color);
                                color = new Color(color.r, color.g, color.b, alpha);
                                //width = json["layers"][layer]["objects"][i]["width"].AsInt;
                                //height = json["layers"][layer]["objects"][i]["height"].AsInt;
                                x = json["layers"][layer]["objects"][i]["x"].AsInt;// + (width / 2);
                                y = json["layers"][layer]["objects"][i]["y"].AsInt * -1 + 16;// + (height / 2);
                                currentObject = Instantiate(objects[(int)objectID], new Vector2(x, y), Quaternion.identity) as GameObject;
                                currentObject.transform.localScale = new Vector3(xscale, yscale, 1f);
                                currentObject.gameObject.GetComponent<SpriteRenderer>().sortingOrder = layer;
                                currentObject.gameObject.GetComponent<SpriteRenderer>().color = color;
                                currentObject.transform.SetParent(currentLayer.gameObject.transform);
                                currentObject.transform.Rotate(0, 0, rotation, Space.Self);
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
        map.TileCount = tileCount;
        map.ObjectCount = objectCount;
        //Debug.Log(MaxWidth);
    }

    private void SetCameraBounds()
    {
        //Bounds bounds = new Bounds(new Vector3(MaxWidth / 2, MaxHeight / 2, 0f), -MaxWidth );
    }

    private Tilemap CreateTilemap(string tilemapName, float x, float y, string c, float alpha)
    {
        GameObject go = new GameObject(tilemapName);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();

        Color color;
        ColorUtility.TryParseHtmlString(c.Trim('"'), out color);
        tm.tileAnchor = new Vector3(x, y, 0);
        tm.color = new Color(color.r, color.g, color.b, alpha);
        go.transform.SetParent(this.gameObject.transform);
        //tr.sortingLayerName = tilemapName;

        return tm;
    }

    private void AddParallaxEffectToLayer(Tilemap tm, float xOffset, float yOffset, float pxFactor, float pyFactor)
    {
        tm.gameObject.AddComponent<ParallaxLayer>();
        ParallaxLayer parallax = tm.GetComponent<ParallaxLayer>();
        parallax.layerXOffset = xOffset;
        parallax.layerYOffset = yOffset;
        parallax.parallaxXFactor = pxFactor;
        parallax.parallaxYFactor = pyFactor;
        parallax.cam = Camera.main.gameObject;
    }

    public void RemoveItemFromMap(int objectID)
    {
        JSONNode json = JSON.Parse(map.MapData);

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
        map.MapData = json.ToString(); //.Split();
    }

    private void SaveMap()
    {
        string mapJSON = JsonUtility.ToJson(map.MapData, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        string path = Application.persistentDataPath + "/Temp/Maps/" + mapFile.name + ".dat";

        if (File.Exists(path) != false)
        {
            file = File.Open(path, FileMode.Open);
            bf.Serialize(file, map.MapData);
        }
        else
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Temp/Maps");
            file = File.Create(Application.persistentDataPath + "/Temp/Maps/" + mapFile.name + ".dat");
            bf.Serialize(file, map.MapData);
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
            map.MapData = data;
            //Debug.Log(map.MapFile);
        }
    }
    #endregion

}
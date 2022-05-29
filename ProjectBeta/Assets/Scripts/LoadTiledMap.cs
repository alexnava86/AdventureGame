using UnityEngine;
using SimpleJSON;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LoadTiledMap : MonoBehaviour
{
	public struct Map
	{
		public TextAsset file;
		public Sprite [] tileSet;
		public Map [] linked;
	}
	//public Material tileMaterial;
	public TextAsset mapFile;
	private Sprite [] tileset;
	private List<Sprite> objectset = new List<Sprite> ();
	private MapNode [,] node;

	public MapNode [,] Node
	{
		get
		{
			return node;
		}
	}

	void Start ()
	{
		ReadMap ();
	}

	public void ReadMap ()
	{
		//TextAsset mapFile = Resources.Load("Maps/"+ Application.loadedLevelName); //this line, line 134, line 135 eliminate the need for the SetMapData method?
		JSONNode json = JSON.Parse (mapFile.text);
		string path = json ["tilesets"] [0] ["image"].ToString ().Trim ('"').Remove (0, 3);
		path = path.Remove (path.Length - 4, 4);
		tileset = Resources.LoadAll (path).OfType<Sprite> ().ToArray ();
		for (int i = 0; i < json ["tilesets"] [1] ["tiles"].Count; i++)
		{
			path = json ["tilesets"] [1] ["tiles"] [i] ["image"].ToString ().Trim ('"').Remove (0, 3);
			path = path.Remove (path.Length - 4, 4); // Remove the .png or other file extension from the filename...
			objectset.Add (Resources.LoadAll (path).OfType<Sprite> ().Single ());
		}

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
		//float xscale;
		//float yscale;

		for (int layer = 0; layer < json ["layers"].AsArray.Count; layer++)
		{
			if (json ["layers"] [layer] ["type"].ToString ().Trim ('"') == "tilelayer")
			{
				width = json ["layers"] [layer] ["width"].AsInt;
				height = json ["layers"] [layer] ["height"].AsInt;
				node = new MapNode [width, height];

				for (int i = 0; i < width * height; i++)
				{
					tileID = json ["layers"] [layer] ["data"] [i].AsUInt;
					//xscale = ((tileID & flippedHorizontally) != 0) ? -1.0f : 1.0f;
					//yscale = ((tileID & flippedVertically) != 0) ? -1.0f : 1.0f;
					//tileID = (tileID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));

					if (tileID != 0)
					{
						tileIDOffset = json ["tilesets"] [0] ["firstgid"].AsInt;
						y = (int)(i / width);
						x = (int)(i % width);
						node [x, y].X = x;
						node [x, y].Y = y;
						node [x, y].tile = new GameObject ("Tile" + i, typeof (SpriteRenderer), typeof (BoxCollider2D));//(GameObject)Instantiate(new GameObject(), new Vector2((float)x * 64f, (float)y * - 64f), Quaternion.identity);
																														//node [x, y].tile = new GameObject ("Tile" + i, typeof (SpriteRenderer));//(GameObject)Instantiate(new GameObject(), new Vector2((float)x * 64f, (float)y * - 64f), Quaternion.identity);
						node [x, y].tile.transform.position = new Vector2 (x * 16, y * -16);
						node [x, y].tile.GetComponent<SpriteRenderer> ().sprite = tileset [0];//[tileID - tileIDOffset];
						node [x, y].tile.GetComponent<SpriteRenderer> ().sortingLayerName = "Floor";
						//node [x, y].tile.GetComponent<SpriteRenderer> ().material = tileMaterial;
					}
				}
			}
			else if (json ["layers"] [layer] ["type"].ToString ().Trim ('"') == "objectgroup")
			{
				for (int i = 0; i < json ["layers"] [layer] ["objects"].Count; i++)
				{
					objectID = json ["layers"] [layer] ["objects"] [i] ["gid"].AsUInt;
					//objectID = (objectID & ~(flippedHorizontally | flippedVertically | flippedDiagonally));
					if (objectID != 0)
					{
						objectIDOffset = json ["tilesets"] [1] ["firstgid"].AsInt;
						x = json ["layers"] [layer] ["objects"] [i] ["x"].AsInt;
						y = json ["layers"] [layer] ["objects"] [i] ["y"].AsInt;
						//Instantiate(objectset[objectID - objectIDOffset], new Vector2(x, y), Quaternion.identity);
						GameObject obj = new GameObject (json ["tilesets"] [1] ["tiles"] [i] ["image"].ToString ().Trim ('"').Remove (0, 3), typeof (SpriteRenderer));
						Debug.Log (x + "," + y);//(obj.name);
						obj.transform.position = new Vector2 ((float)x + 38, (float)y * -1 + 12);
						obj.GetComponent<SpriteRenderer> ().sprite = objectset [(int)(objectID - objectIDOffset)];
						obj.GetComponent<SpriteRenderer> ().sortingLayerName = "World";
						//node[x, y].tile.GetComponent<SpriteRenderer>().material = tileMaterial;
					}
				}
			}
		}
	}
	private void GetTileset () //REMOVE
	{
		switch (Application.loadedLevelName)
		{
		case "Test":
			//objects = new GameObject[2];
			//objects[0] = Resources.Load("Prefabs/Objects/Homes/House", typeof(GameObject)) as GameObject;
			//objects[1] = Resources.Load("Prefabs/Objects/Nature/Tree1", typeof(GameObject)) as GameObject;
			//tileset = Resources.LoadAll("Art/Tilesets/1st Continent/Roads&Grass").OfType<Sprite>().ToArray();
			break;
		}
	}
}


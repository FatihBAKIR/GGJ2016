﻿using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Assets.EditTool;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class EditTool : MonoBehaviour
{
    private LevelLoader.LevelInfo levelInfo;
    private Material[] materials;
    private GameObject[] agentsPrefabs;

    public GameObject materialParent;
    public GameObject materialEntryPrefab;
    public GameObject materialText;
    public Button saveButton;
    public InputField filenameInput;

    private Material _currentMat;
    private GameObject _tilePref;

    private List<Tile> _tiles = new List<Tile>();
    private List<GameObject> _tileObjects = new List<GameObject>();
    private List<Agent> _agents = new List<Agent>();
    private Dictionary<string, int> _tileInfoIDs = new Dictionary<string, int>();
    private Dictionary<string, int> _agentInfoIDs = new Dictionary<string, int>();
    // Use this for initialization

    void Awake()
    {
        _tilePref = Resources.Load<GameObject>("Tile");
    }

    void CreateTile(Coord coordinates, Material material)
    {
        var tile = new Tile(new TileInfo(material), coordinates, 0);
        var go = Object.Instantiate(_tilePref, Grid.CoordToPosition(coordinates), Quaternion.identity) as GameObject;
        go.GetComponent<Renderer>().material = material;
        tile.Obj = go;

        _tileObjects.Add(go);
        _tiles.Add(tile);
    }

    void Start()
    {
        materials = Resources.LoadAll<Material>("Tiles");
        _currentMat = materials[0];
        agentsPrefabs = Resources.LoadAll<GameObject>("Agents");

        levelInfo = new LevelLoader.LevelInfo();
        levelInfo.Meta = new LevelLoader.LevelInfo.LevelMeta() { Width = 16, Height = 16 };
        levelInfo.TileInfos = new LevelLoader.LevelInfo.TilePrefInfo[materials.Length];
        levelInfo.AgentInfos = new LevelLoader.LevelInfo.AgentPrefInfo[agentsPrefabs.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            levelInfo.TileInfos[i] = new LevelLoader.LevelInfo.TilePrefInfo() { Mat = materials[i].name };
            _tileInfoIDs.Add(materials[i].name, i);
        }

        for (int i = 0; i < agentsPrefabs.Length; i++)
        {
            levelInfo.AgentInfos[i] = new LevelLoader.LevelInfo.AgentPrefInfo() { Prefab = agentsPrefabs[i].name };
            _agentInfoIDs.Add(agentsPrefabs[i].name, i);
        }

        saveButton.onClick.AddListener(() => SaveFile(filenameInput.text));
        PopulateMaterials(materialParent);
    }

    void SaveFile(string filename)
    {
        FileStream fs = new FileStream("./Assets/Resources/Levels/Level" + filename + ".json", FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        UpdateLevelInfo();
        sw.Write(JsonConvert.SerializeObject(levelInfo));
        sw.Close();
    }

    void PopulateMaterials(GameObject materialsParent)
    {
        foreach (var mat in materials)
        {
            Debug.LogWarning(mat.name);
            var button = GameObject.Instantiate(materialEntryPrefab) as GameObject;
            button.transform.FindChild("Text").GetComponent<Text>().text = mat.name;
            button.transform.SetParent(materialParent.transform);
            var m = mat;
            button.GetComponent<Button>().onClick.AddListener(() => SetMat(m));
        }
    }
    public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
    {
        int place = Source.LastIndexOf(Find);

        if (place == -1)
            return Source;

        string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        return result;
    }

    void UpdateLevelInfo()
    {
        var normX = - _tiles.Min((x) => x.Coordinate.X);
        var normY = - _tiles.Min((x) => x.Coordinate.Y);

        var maxX = _tiles.Max((x) => x.Coordinate.X);
        var maxY = _tiles.Max((x) => x.Coordinate.Y);
        var minX = _tiles.Min((x) => x.Coordinate.X);
        var minY = _tiles.Min((x) => x.Coordinate.Y);

        levelInfo.Meta.Width = levelInfo.Meta.Height = System.Math.Max(maxY - minY + 1, maxX - minX + 1);
        
        levelInfo.Tiles = new LevelLoader.LevelInfo.TileInstanceInfo[_tiles.Count];
        for (int i = 0; i < _tiles.Count; i++)
        {
            var t = _tiles[i];
            levelInfo.Tiles[i] = new LevelLoader.LevelInfo.TileInstanceInfo()
            {
                Coord = new int[] { t.Coordinate.X + normX, t.Coordinate.Y + normY},
                Elevation = t.Elevation,
                InfoNo = _tileInfoIDs[t.Info.Material.name]
            };
        }

        levelInfo.Agents = new LevelLoader.LevelInfo.AgentInstanceInfo[_agents.Count];
        for (int i = 0; i < _agents.Count; i++)
        {
            var a = _agents[i];
            levelInfo.Agents[i] = new LevelLoader.LevelInfo.AgentInstanceInfo()
            {
                Coords = new int[] { a.Position.X + normX, a.Position.Y + normY},
                PrefId = _agentInfoIDs[ReplaceLastOccurrence(a.name, "(Clone", "")]
            };
        }

        Debug.Log(JsonConvert.SerializeObject(levelInfo));
    }

    void SetMat(Material mat)
    {
        _currentMat = mat;
        materialText.GetComponent<Text>().text = mat.name;
    }
    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 point;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            Utils.LinePlaneIntersection(out point, r.origin, r.direction, Vector3.up, Vector3.zero);
            var coord = Grid.PositionToCoord(point);
            point = Grid.CoordToPosition(coord) + Vector3.up * 0.5f;

            bool onTile = _tiles.Any((x) => x.Coordinate.X == coord.X && x.Coordinate.Y == coord.Y);
            var select_tile = (onTile) ? _tiles.Where((x) => x.Coordinate.X == coord.X && x.Coordinate.Y == coord.Y).First() : null;
            // Draw square line around the tile
            {
                var frw = Vector3.forward * .5f;
                var rgh = Vector3.right * .5f;
                DrawingInterface.DrawPolyLine(new List<Vector3>() { point + frw + rgh, point - frw + rgh,
                                                                    point - frw - rgh, point + frw - rgh},
                                                                    onTile ? Color.red : Color.yellow, -1, true);
            }

            if (Input.GetMouseButton(0))
            {
                if (!onTile)
                    CreateTile(coord, _currentMat);
            }
            if (Input.GetMouseButton(1))
            {
                if (onTile)
                {
                    _tileObjects.Remove(select_tile.Obj);
                    Destroy(select_tile.Obj);
                    _tiles.Remove(select_tile);
                    select_tile = null;
                }
            }
        }
    }
}
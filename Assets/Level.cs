using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Tile
{
    public GameObject Obj { get; set; }
    public TileInfo Info { get; private set; }
    public float Elevation { get; private set; }
    public Coord Coordinate { get; private set; }

    public Tile(TileInfo info, Coord pos, float height)
    {
        Info = info;
        Coordinate = pos;
        Elevation = height;
    }

    public Agent[] AgentsOnTile()
    {
        return Object.FindObjectsOfType<Agent>().Where(o => o.Position == Coordinate).ToArray();
    }

    public Agent[] AgentsOnTile(Func<Agent, bool> filter)
    {
        return Object.FindObjectsOfType<Agent>().Where(agent => (agent.Position == Coordinate && filter(agent))).ToArray();
    }
}

public class Grid
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly Tile[,] _tiles;

    public Grid(int w, int h)
    {
        _tiles = new Tile[h, w];
        Width = w;
        Height = h;
    }

    public static Coord PositionToCoord(Vector3 pos)
    {
        return new Coord((int)pos.x, (int)pos.z);
    }

    public static Vector3 CoordToPosition(Coord crd)
    {
        return new Vector3(crd.X, 0, crd.Y);
    }

    public Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(x, 0, y);
    }

    public void Set(Tile t)
    {
        _tiles[t.Coordinate.X, t.Coordinate.Y] = t;
    }

    public Tile Get(int x, int y)
    {
        return _tiles[x, y];
    }

    public Tile[] GetLoS(int x, int y, int radius)
    {
        List<Tile> losTiles = new List<Tile>();
        
        for (int i = -radius; i < radius + 1; i++)
        {
            for (int j = -(radius - Math.Abs(i)); j < 1 + (radius - Math.Abs(i)); j++)
            {
                if (x + i < 0 || x + i >= Width || y + j < 0 || y + j >= Height) continue;

                Vector3 origin = CoordToPosition(x, y) + (_tiles[x, y] == null ? 0 : _tiles[x, y].Elevation + 1) * Vector3.up;
                Vector3 direction = CoordToPosition(x + i, y + j) + Vector3.up - origin;
                float distance = direction.magnitude;
                direction.Normalize();

                if (!Physics.Raycast(origin, direction, distance, 1 << LayerMask.NameToLayer("Grid")))
                    losTiles.Add(_tiles[x + i, y + j]);
            }
        }

        return losTiles.ToArray();
    }

    public Tile[] Get(int x, int y, int r)
    {
        List<Tile> tiles = new List<Tile>();
        for (int i = Mathf.Max(y - r, 0); i < Mathf.Min(y + r + 1, Height); i++)
        {
            for (int j = Mathf.Max(x - r, 0); j < Mathf.Min(x + r + 1, Width); j++)
            {
                if (_tiles[j, i] != null && Coord.Distance(_tiles[j, i].Coordinate, new Coord(x, y)) <= r)
                {
                    tiles.Add(_tiles[j, i]);
                }
            }
        }
        return tiles.ToArray();
    }
}

public class Level
{
    private readonly Grid _grid;

    private readonly List<Agent> _agents;

    private readonly TileInfo[] _initialTiles;
    private readonly AgentInfo[] _initialAgents;

    private readonly Dictionary<string, int> _agentMap;

    private readonly Func<Agent>[] _initializers;

    public Level(Grid g, AgentInfo[] agents, TileInfo[] tiles, Func<Agent>[] agentInitializers, Dictionary<string, int> agentMap)
    {
        _grid = g;

        _initialAgents = agents;
        _initialTiles = tiles;

        _initializers = agentInitializers;

        _agentMap = agentMap;

        _agents = new List<Agent>();
    }

    public Tile Get(Coord c)
    {
        return Get(c.X, c.Y);
    }

    public Tile[] Get(Coord c, int r)
    {
        return Get(c.X, c.Y, r);
    }

    public Tile Get(int x, int y)
    {
        return _grid.Get(x, y);
    }

    public Tile[] Get(int x, int y, int r)
    {
        return _grid.Get(x, y, r);
    }

    public Tile[] GetLoS(Coord c, int r)
    {
        return _grid.GetLoS(c.X, c.Y, r);
    }

    public void LoadToScene()
    {
        if (CurrentLevel != null)
        {
            CurrentLevel.Clear();
            _level = null;
        }

        GameObject tilePref = Resources.Load<GameObject>("Tile");

        for (int i = 0; i < _grid.Height; i++)
        {
            for (int j = 0; j < _grid.Width; j++)
            {
                if (_grid.Get(j, i) == null)
                {
                    continue;
                }
                var go = Object.Instantiate(tilePref, Grid.CoordToPosition(_grid.Get(j, i).Coordinate) + _grid.Get(j, i).Elevation * Vector3.up, Quaternion.identity) as GameObject;
                go.GetComponent<Renderer>().material = _grid.Get(j, i).Info.Material;
                _grid.Get(j, i).Obj = go;
            }
        }

        foreach (var initializer in _initializers)
        {
            _agents.Add(initializer());
        }

        _level = this;
    }

    public void Clear()
    {

    }

    public void Destroy(Agent a)
    {
        _delete[_agents.IndexOf(a)] = true;
    }

    bool[] _delete;
    public void NextTurn()
    {
        _delete = new bool[_agents.Count];

        foreach (var agent in _agents)
        {
            agent.Step();
        }

        for (int i = _delete.Length - 1; i >= 0; i--)
        {
            if (_delete[i])
            {
                Object.Destroy(_agents[i].gameObject);
                _agents.RemoveAt(i);
            }
        }
    }

    static Level _level;

    public static Level CurrentLevel
    {
        get { return _level; }
    }

    public int AgentIdByName(string name)
    {
        return _agentMap[name];
    }

    public Agent Instantiate(string name, Coord coord)
    {
        Debug.Log(coord + " -> " + Grid.CoordToPosition(coord));
        var go = Object.Instantiate(_initialAgents[AgentIdByName(name)].Prefab, Grid.CoordToPosition(coord), Quaternion.identity) as GameObject;
        _agents.Add(go.GetComponent<Agent>());
        return go.GetComponent<Agent>();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class Tile
{
    public GameObject Obj { get; set; }
    public TileInfo Info { get; private set; }
    public float Elevation { get; private set; }
    public Coord Coordinate { get; private set; }
    public Vector3 Position
    {
        get
        {
            return Grid.CoordToPosition(Coordinate) + (Elevation + 0.5f) * Vector3.up;
        }
    }

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

    public void Elevate(float delta)
    {
        this.Elevation += delta;
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

    public Vector3 CoordSurfacePosition(Coord crd)
    {
        return _tiles[crd.X, crd.Y].Position;
    }

    public Vector3 CoordAgentCenterPosition(Coord crd)
    {
        return CoordSurfacePosition(crd) + 0.5f * Vector3.up;
    }

    public void Set(Tile t)
    {
        _tiles[t.Coordinate.X, t.Coordinate.Y] = t;
    }

    public void Set(int x, int y, Tile t)
    {
        _tiles[x, y] = t;
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

public enum SeeState
{
    Hidden = 0,
    Explored = 1,
    Ascending = 2,
    Revealed = 4
}

public class Promise
{
    public string From { get; private set; }

    public Promise()
    {
        Fulfilled = false;
        var stackTrace = new StackTrace();
        var stackFrames = stackTrace.GetFrames();

        From = stackFrames.Aggregate("", (s, frame) => s + " -> " + frame.GetMethod().Name);
    }

    public bool Fulfilled { get; private set; }

    public void Fulfill()
    {
        Fulfilled = true;
    }
}

public class Level
{
    public event Action RoundFinished = delegate { };
    public event Action LevelLoaded = delegate { };

    public event Action Failed = delegate { Debug.Log("our guy just died because of you"); };
    public event Action Success = delegate { Debug.Log("woohoooo"); };

    private readonly Grid _grid;

    private readonly SeeState[,] _canSee;

    public Grid Grid
    {
        get { return _grid; }
    }

    private readonly List<Agent> _agents;

    private readonly TileInfo[] _initialTiles;
    private readonly AgentInfo[] _initialAgents;

    private readonly Dictionary<string, int> _agentMap;

    private readonly Func<Agent>[] _initializers;
    private readonly List<Promise> _turnPromises;

    public Level(Grid g, AgentInfo[] agents, TileInfo[] tiles, Func<Agent>[] agentInitializers, Dictionary<string, int> agentMap)
    {
        _grid = g;

        _initialAgents = agents;
        _initialTiles = tiles;

        _initializers = agentInitializers;

        _agentMap = agentMap;

        _agents = new List<Agent>();
        _canSee = new SeeState[Grid.Width, Grid.Height];
        _turnPromises = new List<Promise>();
    }

    public Tile PlayerTile
    {
        get { return Get(Grid.PositionToCoord(Object.FindObjectOfType<Player>().transform.position)); }
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

    public int GetSeeState(Coord c)
    {
        return (int)_canSee[c.X, c.Y];
    }

    public void LoadToScene()
    {
        if (CurrentLevel != null)
        {
            _level = null;
        }
        _level = this;

        GameObject tilePref = Resources.Load<GameObject>("Tile");
        tilePref.GetComponent<MeshFilter>().sharedMesh = CubeGen.TileCube;

        for (int i = 0; i < _grid.Height; i++)
        {
            for (int j = 0; j < _grid.Width; j++)
            {
                if (_grid.Get(j, i) == null)
                {
                    continue;
                }

                var go = Object.Instantiate(tilePref, Grid.CoordToPosition(_grid.Get(j, i).Coordinate) + (_grid.Get(j, i).Elevation - 10) * Vector3.up, Quaternion.identity) as GameObject;
                go.GetComponent<Renderer>().material = _grid.Get(j, i).Info.Material;
                go.GetComponent<Renderer>().material.color = Color.black;
                Grid.Get(j, i).Obj = go;
                go.GetComponent<TileWorks>().SetState(Grid.Get(i, j), _canSee[i, j]);
            }
        }

        foreach (var initializer in _initializers)
        {
            var agent = initializer();

            foreach (var rend in agent.GetComponentsInChildren<Renderer>())
            {
                rend.enabled = false;
            }

            if (agent.GetComponent<Renderer>() != null)
            {
                agent.GetComponent<Renderer>().enabled = false;
            }

            _agents.Add(agent);
        }

        Camera.main.GetComponent<CameraController>().target.position = _agents.Where((x) => x.name == "Player(Clone)").First().transform.position;
        Camera.main.GetComponent<CameraController>().targetOffset = Vector3.zero;


        LevelLoaded();
        ResolveSight();
    }

    void ResolveSight()
    {
        var tilesSeen = GetLoS(Object.FindObjectOfType<Player>().Position, 2);
        foreach (var tile in tilesSeen)
        {
            if (tile == null)
                continue;

            Reveal(tile.Coordinate);
        }

        for (int j = 0; j < Grid.Height; j++)
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                if (Grid.Get(i, j) == null)
                    continue;

                Grid.Get(i, j).Obj.GetComponent<TileWorks>().SetState(Grid.Get(i, j), _canSee[i, j]);

                if (_canSee[i, j] > SeeState.Explored)
                {
                    _canSee[i, j]--;
                }
            }
        }
    }

    public void AddPromise(Promise p)
    {
        _turnPromises.Add(p);
    }

    public void Update()
    {
        bool allFulfilled = _turnPromises.Aggregate(true, (b, promise) => b && promise.Fulfilled);

        if (allFulfilled)
        {
            CheckConditions();
            NextTurn();

            if (_destroy)
            {
                Object.FindObjectOfType<Loader>().StartC(UnloadPart());
            }
        }

        if (_destroy)
        {
            foreach (var source in _turnPromises.Where(promise => !promise.Fulfilled))
            {
                Debug.Log(source.From);
            }
        }
    }

    bool[] _delete;
    public int TurnCount { get; private set; }

    void NextTurn()
    {
        if (_destroy)
        {
            return;
        }

        _delete = new bool[_agents.Count];
        TurnCount++;
        Debug.Log("Turn: " + TurnCount);

        _turnPromises.Clear();
        for (int i = _agents.Count - 1; i >= 0; i--)
        {
            if (_delete[i]) continue;
            
            _agents[i].Step();
        }

        for (int i = _delete.Length - 1; i >= 0; i--)
        {
            if (_delete[i])
            {
                Object.Destroy(_agents[i].gameObject);
                _agents.RemoveAt(i);
            }
        }

        ResolveSight();

        if (!_destroy)
        {
            RoundFinished();
        }
    }

    public void Destroy(Agent a)
    {
        if (a == null) return;

        Debug.Log(_agents.IndexOf(a));
        _delete[_agents.IndexOf(a)] = true;
    }

    public void CheckConditions()
    {
        Debug.Log(PlayerTile.Coordinate);

        if (PlayerTile.AgentsOnTile(agent => agent is Gate).Length > 0)
        {
            Success();
            _destroy = true;
        }
    }

    IEnumerator UnloadPart()
    {
        for (int j = 0; j < Grid.Height; j++)
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                if (Grid.Get(i, j) == null)
                    continue;

                Grid.Get(i, j).Obj.GetComponent<TileWorks>().Descend();
                Grid.Set(i, j, null);

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    static Level _level;

    private bool _destroy;
    public void Fail()
    {
        _destroy = true;
        Failed();
    }

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
        var go = Object.Instantiate(_initialAgents[AgentIdByName(name)].Prefab, _grid.CoordSurfacePosition(coord), Quaternion.identity) as GameObject;
        _agents.Add(go.GetComponent<Agent>());
        go.GetComponent<Agent>().Position = coord;
        go.GetComponent<Agent>().Init();
        return go.GetComponent<Agent>();
    }

    public void Reveal(Coord tile)
    {
        if (_canSee[tile.X, tile.Y] == SeeState.Hidden)
        {
            _canSee[tile.X, tile.Y] = SeeState.Ascending;
        }
        else
        {
            _canSee[tile.X, tile.Y] = SeeState.Revealed;
        }
    }
}

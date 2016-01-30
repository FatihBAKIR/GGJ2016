﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Tile
{
    public GameObject Obj { get; private set; }
    public TileInfo Info { get; private set; }
    public float Elevation { get; private set; }
    public Coord Coordinate { get; private set; }

    public Tile(TileInfo info, Coord pos, float height)
    {
        Info = info;
        Coordinate = pos;
        Elevation = height;
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

    public void Set(Tile t)
    {
        _tiles[t.Coordinate.X, t.Coordinate.Y] = t;
    }

    public Tile Get(int x, int y)
    {
        return _tiles[x, y];
    }

    public Tile[] Get(int x, int y, int r)
    {
        throw new NotImplementedException();
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

    public Tile Get(int x, int y)
    {
        return _grid.Get(x, y);
    }

    public Tile[] Get(int x, int y, int r)
    {
        return _grid.Get(x, y, r);
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
                var go = Object.Instantiate(tilePref, Grid.CoordToPosition(_grid.Get(j, i).Coordinate), Quaternion.identity) as GameObject;
                go.GetComponent<Renderer>().material = _grid.Get(j, i).Info.Material;
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
        var go = Object.Instantiate(_initialAgents[AgentIdByName(name)].Prefab, Grid.CoordToPosition(coord), Quaternion.identity) as GameObject;
        _agents.Add(go.GetComponent<Agent>());
        return go.GetComponent<Agent>();
    }
}
using System;
using UnityEngine;

public class Coord
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Coord(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Tile
{
    public GameObject Obj { get; private set; }
    public TileInfo Info { get; private set; }
    public float Elevation { get; private set; }
    public Coord Coordinate { get; private set; }
}

public class Grid
{
    private Tile[][] _tiles;

    public Coord PositionToCoord(Vector3 pos)
    {
        throw new NotImplementedException();
    }

    public Tile Get(int x, int y)
    {
        return _tiles[x][y];
    }

    public Tile[] Get(int x, int y, int r)
    {
        throw new NotImplementedException();
    }

    public Agent Instantiate(GameObject prefab, Coord coord)
    {
        throw new NotImplementedException();
    }
}

public class Level
{
    private Grid g;

    public Tile Get(int x, int y)
    {
        return g.Get(x, y);
    }

    public Tile[] Get(int x, int y, int r)
    {
        return g.Get(x, y, r);
    }



    private static Level _instance;

    public static Level Instance
    {
        get { return _instance; }
    }
}

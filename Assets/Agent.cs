using System;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool Blocks;
    private Coord _coord;
    public int Wait { get; set; }

    private Grid Grid
    {
        get
        {
            return Level.CurrentLevel.Grid;
        }
    }
    public Coord Position
    {
        get
        {
            return _coord;
        }
        set 
        { 
            _coord = value;
            transform.position = Grid.CoordSurfacePosition(_coord);
        }
    }

    public void Step()
    {
        Wait = Math.Max(0, Wait - 1);

        DoStep();
    }

    protected abstract void DoStep();
}
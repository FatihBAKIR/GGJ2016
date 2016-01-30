using System;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    private Coord _coord;
    public int Wait { get; set; }

    public Coord Position
    {
        get
        {
            return _coord;
        }
        set 
        { 
            _coord = value;
            transform.position = Grid.CoordToPosition(_coord);
        }
    }

    public void Step()
    {
        Wait = Math.Max(0, Wait - 1);

        DoStep();
    }

    protected abstract void DoStep();
}
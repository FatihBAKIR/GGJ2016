using System;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool Blocks;
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
            transform.position = Level.CurrentLevel.Grid.CoordSurfacePosition(_coord);
        }
    }

    public void Step()
    {
        Wait = Math.Max(0, Wait - 1);

        DoStep();
    }

    void Update()
    {
        GetComponent<Renderer>().enabled = Level.CurrentLevel.GetSeeState(Position) > (int)SeeState.Explored;
    }

    protected abstract void DoStep();
}
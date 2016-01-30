using System;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool Blocks;
    private Coord _coord;
    public int Wait { get; set; }

    public string Target { get; set; }
    public string Id { get; set; }

    public Coord Position
    {
        get
        {
            return _coord;
        }
        set
        {
            _coord = value;
            transform.position = Level.CurrentLevel.Grid.CoordAgentCenterPosition(_coord);
        }
    }

    public void Step()
    {
        Wait = Math.Max(0, Wait - 1);
        
        DoStep();
    }

    void Update()
    {
        bool active = Level.CurrentLevel.GetSeeState(Position) > (int) SeeState.Explored;

        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = active;
        }

        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = active;
        }
    }

    protected abstract void DoStep();
}
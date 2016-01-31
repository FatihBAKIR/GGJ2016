using UnityEngine;
using System.Collections;

public class Trap : Agent
{
    public int Radius;
    int _startTime;

    protected override void DoStart()
    {
        Debug.Log("trap at " + Level.CurrentLevel.TurnCount);
        _startTime = Level.CurrentLevel.TurnCount;
    }

    protected override void DoStep()
    {
        if (Level.CurrentLevel.TurnCount < _startTime + 2)
        {
            return;
        }

        bool explode = false;
        var neighbors = Level.CurrentLevel.Get(Position, Radius);

        foreach (var neighbor in neighbors)
        {
            var guys = neighbor.AgentsOnTile(agent => agent is LerpAgent);
            foreach (var guy in guys)
            {
                guy.Wait += 3;

                explode = true;
            }
        }

        if (explode)
        {
            Debug.Log("boom");
            Level.CurrentLevel.Destroy(this);
        }
    }
}
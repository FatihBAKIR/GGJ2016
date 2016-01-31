using UnityEngine;
using System.Collections;

public class Trap : Agent
{
    private static GameObject _stun;
    static Trap()
    {
        _stun = Resources.Load<GameObject>("Particles/Stun");
    }

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
                var stun = Instantiate(_stun);
                stun.name = "Stun";
                stun.transform.parent = guy.transform;
                stun.transform.localPosition = new Vector3(0, 1, 0);
                explode = true;
            }
        }

        if (explode)
        {
            var p = Instantiate(Resources.Load<GameObject>("Particles/StunBlast"));
            p.transform.position = Level.CurrentLevel.Grid.CoordAgentCenterPosition(Position);
            Destroy(p, 1.2f);
            Debug.Log("boom");
            Level.CurrentLevel.Destroy(this);
        }
    }
}
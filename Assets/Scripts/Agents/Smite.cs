using UnityEngine;
using System.Collections;

public class Smite : Agent {
    protected override void DoStep()
    {
        var guys = Level.CurrentLevel.Get(Position).AgentsOnTile(agent => agent is LerpAgent);
        Debug.Log(guys[0]);
        Level.CurrentLevel.Destroy(guys[0]);
        Level.CurrentLevel.Destroy(this);

        GameObject go = Resources.Load<GameObject>("SmiteParticle");
        var g = Instantiate(go);
        g.transform.position = Level.CurrentLevel.Grid.CoordAgentCenterPosition(Position);
        Destroy(g, 1.2f);
    }
}

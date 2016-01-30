using UnityEngine;
using System.Collections;

public class Gate : Agent {

    protected override void DoStep()
    {
        if (Level.CurrentLevel.Get(Position).AgentsOnTile(agent => agent is Player).Length > 0)
        {
        }
    }
}

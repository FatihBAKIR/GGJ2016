using UnityEngine;
using System.Collections;

public class LerpAgent : Agent
{
    protected override void DoStep()
    {
        var comp = gameObject.GetComponent<MoveTowards>();
        comp.SetTarget(Position + new Coord(1,0));
    }
}

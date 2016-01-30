using UnityEngine;
using System.Collections;

public class LerpAgent : Agent
{
    readonly WalkCommand com = new WalkCommand();
    void Start()
    {
        com.SetSource(this);
    }

    protected override void DoStep()
    {
        com.Apply(Position + new Coord(1, 0));
    }
}

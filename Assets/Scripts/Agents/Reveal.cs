using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Reveal : Agent
{
    protected override void DoStart()
    {
        var agents = FindObjectsOfType<Agent>();

        foreach (var agent in agents.Where(a => !(a is Reveal)))
        {
            var tile = agent.Position;

            foreach (var t in Level.CurrentLevel.Get(tile, 1))
            {
                Level.CurrentLevel.Reveal(t.Coordinate);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Agents
{
    class Decoy : Agent
    {
        protected override void DoStep()
        {
            var los = Level.CurrentLevel.GetLoS(Position, 4);

            foreach (var tile in los)
            {
                if (tile == null)
                {
                    continue;
                }

                foreach (var agent in tile.AgentsOnTile(agent => agent is LerpAgent))
                {
                    agent.GetComponent<LerpAgent>().PreferedTarget = this;
                }
            }
        }
    }
}

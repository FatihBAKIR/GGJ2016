using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Agents
{
    class Decoy : Agent
    {
        private static GameObject _poof;
        static Decoy()
        {
            _poof = Resources.Load<GameObject>("Particles/Mist");
        }

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

        public void Poof()
        {
            var poof = Instantiate(_poof, Level.CurrentLevel.Grid.CoordAgentCenterPosition(Position), Quaternion.identity);
            Destroy(poof, 1.2f);
            Level.CurrentLevel.Destroy(this);
        }
    }
}

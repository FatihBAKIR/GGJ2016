using UnityEngine;
using System.Collections;
using System.Linq;

public class WallRemove : Agent {
    
    protected override void DoStep()
    {
        if (Level.CurrentLevel.Get(Position).AgentsOnTile(agent => agent is Player).Length > 0)
        {
            Debug.Log(string.Format("{0} is destroyed!", Target));
            Level.CurrentLevel.Destroy(FindObjectsOfType<Agent>().FirstOrDefault(agent => agent.Id == Target));
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}

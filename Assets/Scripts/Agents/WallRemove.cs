using UnityEngine;
using System.Linq;

public class WallRemove : Agent
{
    private bool _done;
    protected override void DoStep()
    {
        if (_done)
        {
            return;
        }

        if (Level.CurrentLevel.Get(Position).AgentsOnTile(agent => agent is Player).Length > 0)
        {
            _done = true;
            Debug.Log(string.Format("{0} is destroyed!", Target));
            Level.CurrentLevel.Destroy(FindObjectsOfType<Agent>().FirstOrDefault(agent => agent.Id == Target));
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}

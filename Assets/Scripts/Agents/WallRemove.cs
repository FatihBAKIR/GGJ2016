using UnityEngine;
using System.Linq;

public class WallRemove : Sigil
{
    private bool _done;
    private bool _activated;
    private bool _finalized;

    public override void Activate()
    {
        _activated = true;
        _done = false;
        _t = 0;
    }

    protected override void DoStep()
    {
        if (!_done || _finalized) return;

        Level.CurrentLevel.Destroy(FindObjectsOfType<Agent>().FirstOrDefault(agent => agent.Id == Target));
        Destroy(transform.GetChild(0).gameObject);
        Debug.Log(string.Format("{0} is destroyed!", Target));
        _finalized = true;
    }

    private float _t;
    protected override void DoUpdate()
    {
        if (!_activated || _done)
        {
            return;
        }

        _t += Time.deltaTime;

        Debug.Log(_t);
        if (_t >= 1.2f)
        {
            _done = true;
            Completed();
        }
    }
}

using System;
using UnityEngine;
using System.Collections;

public class MoveTowards : MonoBehaviour
{
    public event Action<MoveTowards> MoveComplete = delegate { }; 

    private Coord _movingTo;
    private float _t;

    void Start()
    {
        _movingTo = GetComponent<Agent>().Position;
        _t = -1;
    }

    public void Update()
    {
        if (_t < 0) return;

        transform.position = Vector3.Lerp(Level.CurrentLevel.Grid.CoordSurfacePosition(GetComponent<Agent>().Position), Level.CurrentLevel.Grid.CoordSurfacePosition(_movingTo), _t);

        _t += Time.deltaTime * 2;

        if (_t < 1) return;

        GetComponent<Agent>().Position = _movingTo;
        _t = -1;

        MoveComplete(this);
    }

    public void SetTarget(Coord c)
    {
        _movingTo = c;
        _t = 0;
    }
}

using UnityEngine;
using System.Collections;

public class MoveTowards : MonoBehaviour {

    private Coord _movingTo;
    private float _t;

    void Start()
    {
        _movingTo = GetComponent<Agent>().Position;
    }

    public void Update()
    {
        transform.position = Vector3.Lerp(Grid.CoordToPosition(GetComponent<Agent>().Position), Grid.CoordToPosition(_movingTo), _t);

        _t += Time.deltaTime;

        if (_t >= 1)
        {
            GetComponent<Agent>().Position = _movingTo;
        }
    }

    public void SetTarget(Coord c)
    {
        _movingTo = c;
        _t = 0;
    }
}

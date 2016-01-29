using UnityEngine;

public class Agent : MonoBehaviour
{
    protected Coord Position;

    public virtual void Step()
    {
    }
}

public class Trap : Agent
{
    public int Radius { get; private set; }

    public override void Step()
    {
        foreach (var tile in Level.Instance.Get(Position.X, Position.Y, Radius))
        {
            
        }

        base.Step();
    }
}
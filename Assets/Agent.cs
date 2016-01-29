using UnityEngine;

public class Agent : MonoBehaviour
{
    public Coord Position { get; set; }

    public virtual void Step()
    {
    }
}
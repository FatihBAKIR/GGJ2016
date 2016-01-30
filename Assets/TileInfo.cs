using Newtonsoft.Json;
using UnityEngine;

public class TileInfo
{
    public bool CanWalk { get; private set; }
    public Material Material { get; private set; }

    public TileInfo(Material mat) : this(mat, true)
    {
        
    }

    public TileInfo(Material mat, bool canWalk)
    {
        Material = mat;
        CanWalk = canWalk;
    }
}

public class AgentInfo
{
    public GameObject Prefab;
}
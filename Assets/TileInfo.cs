using Newtonsoft.Json;
using UnityEngine;

public class TileInfo
{
    public Material Material { get; private set; }

    public TileInfo(Material mat)
    {
        Material = mat;
    }
}

public class AgentInfo
{
    public GameObject Prefab;
}
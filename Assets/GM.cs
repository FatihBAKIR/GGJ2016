using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    void Awake()
    {
        LevelLoader.LoadLevel("Resources/Levels/Level0.json");
    }
}

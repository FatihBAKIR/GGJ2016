using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    void Awake()
    {
        LevelLoader.LoadLevel("Level0");
    }
}

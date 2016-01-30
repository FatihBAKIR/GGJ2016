using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    void Awake()
    {
        var level = LevelLoader.LoadLevel("Level1");
        level.LoadToScene();
    }
}

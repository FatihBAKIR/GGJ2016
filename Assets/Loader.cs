using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    void Awake()
    {
        var level = LevelLoader.LoadLevel("Level0");
        level.LoadToScene();
    }

    void Update()
    {
        if (Input.GetKeyUp("a"))
        {
            TileCommand c = new SpawnCommand("Trap");
            c.SetSource(FindObjectOfType<Player>());
            c.Apply(new Coord(1, 0));
        }
    }
}

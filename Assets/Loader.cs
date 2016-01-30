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

        if (Input.GetKeyUp("n"))
        {
            Level.CurrentLevel.NextTurn();
        }

        if (Input.GetKeyUp("m"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid")))
            {
                WalkCommand walk = new WalkCommand();
                walk.SetSource(FindObjectOfType<Player>());
                walk.Apply(Grid.PositionToCoord(hit.transform.position));
            }
        }
    }
}

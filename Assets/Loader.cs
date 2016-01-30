using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Loader : MonoBehaviour
{
    private Dictionary<KeyCode, Action> _keys;

    void Awake()
    {
        var level = LevelLoader.LoadLevel("Level0");
        level.LevelLoaded += LevelOnLevelLoaded;
        level.LoadToScene();

        _keys = new Dictionary<KeyCode, Action>
        {
            { KeyCode.A, SummonTrap },
            { KeyCode.M, MovePlayer },
        };
    }

    private void LevelOnLevelLoaded()
    {
        Level.CurrentLevel.SightResolveComplete += CurrentLevelOnSightResolveComplete;
        Level.CurrentLevel.ResolveSight();
    }

    private TileCommand _currentTile = null;
    private Tile[] _possibleTiles = null;
    private bool _acceptAny = false;

    void SetCurrentTile(TileCommand cmd)
    {
        SetCurrentTile(cmd, FindObjectOfType<Player>());
    }

    void SetCurrentTile(TileCommand cmd, Agent src)
    {
        _currentTile = cmd;
        _currentTile.SetSource(src);

        if (cmd.Range == 0)
        {
            _acceptAny = true;
        }
        else
        {
            var tiles = Level.CurrentLevel.Get(src.Position, cmd.Range);
            _possibleTiles = tiles.Where(tile => cmd.CanApply(tile.Coordinate)).ToArray();

            foreach (var tile in _possibleTiles)
            {
                Debug.Log(tile.Coordinate);
                Vector3 topCenter = Level.CurrentLevel.Grid.CoordSurfacePosition(tile.Coordinate);
                Vector3 dir = Camera.main.transform.position - topCenter;

                Ray r = new Ray(topCenter, dir);

                foreach (var hit in Physics.RaycastAll(r))
                {
                    if (hit.transform.gameObject.GetComponent<TileWorks>() == null)
                    {
                        continue;
                    }
                    Debug.Log(string.Format("Tile at {0} blocks movement to {1}", Grid.PositionToCoord(hit.transform.position), tile.Coordinate));
                    hit.transform.gameObject.GetComponent<TileWorks>().FadeOut();
                }
            }
        }

        cmd.CommandComplete += CmdOnCommandComplete;
    }

    private void CmdOnCommandComplete(Command command)
    {
        Level.CurrentLevel.ResolveSight();

        if (command is WalkCommand)
        {
            foreach (var tile in FindObjectsOfType<TileWorks>())
            {
                tile.FadeIn();
            }
        }
    }

    private bool _first = true;
    private void CurrentLevelOnSightResolveComplete()
    {
        if (_first)
        {
            _first = false;
            return;
        }

        Level.CurrentLevel.NextTurn();
    }

    void SummonTrap()
    {
        SetCurrentTile(new SpawnCommand("Trap"));
    }

    void MovePlayer()
    {
        SetCurrentTile(new WalkCommand());
    }

    void CheckTileClick()
    {
        if (_currentTile == null) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0) && Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid")))
        {
            var coord = Grid.PositionToCoord(hit.transform.position);

            if (_currentTile.CanApply(coord))
            {
                _currentTile.Apply(coord);
            }

            _currentTile = null;
        }
    }

    void Update()
    {
        CheckTileClick();

        foreach (var action in _keys)
        {
            if (Input.GetKeyUp(action.Key))
            {
                action.Value();
            }
        }
    }
}

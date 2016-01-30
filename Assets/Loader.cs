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
        level.LoadToScene();

        _keys = new Dictionary<KeyCode, Action>
        {
            { KeyCode.A, SummonTrap },
            { KeyCode.M, MovePlayer },
        };
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
            foreach (var tile in tiles)
            {
                
            }
        }

        cmd.CommandComplete += CmdOnCommandComplete;
    }

    private void CmdOnCommandComplete(Command command)
    {
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

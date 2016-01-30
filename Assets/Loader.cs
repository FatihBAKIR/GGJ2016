using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public void StartC(IEnumerator generator)
    {
        StartCoroutine(generator);
    }

    private Dictionary<KeyCode, Action> _keys;

    void Awake()
    {
        var level = LevelLoader.LoadLevel("Levelgb");
        level.LoadToScene();
        LevelOnRoundFinished();
        level.RoundFinished += LevelOnRoundFinished;

        _keys = new Dictionary<KeyCode, Action>
        {
            { KeyCode.A, SummonTrap },
            { KeyCode.M, MovePlayer },
            { KeyCode.D, SummonDecoy },
        };

        GameObject.Find("Spell1").GetComponent<Button>().onClick.AddListener(SummonTrap);
        GameObject.Find("Spell2").GetComponent<Button>().onClick.AddListener(MovePlayer);
        GameObject.Find("Spell3").GetComponent<Button>().onClick.AddListener(SummonDecoy);

        _lastTrapTurn = -4;
    }

    private void LevelOnRoundFinished()
    {
        Level.CurrentLevel.AddPromise(_playerPromise = new Promise());
    }

    private int _lastTrapTurn;

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
        if (command is WalkCommand)
        {
            foreach (var tile in FindObjectsOfType<TileWorks>())
            {
                tile.FadeIn();
            }
        }

        if (command is SpawnCommand && _isCmdTrap)
        {
            _lastTrapTurn = Level.CurrentLevel.TurnCount;
        }
    }

    private bool _isCmdTrap = false;
    void SummonTrap()
    {
        if (_lastTrapTurn + 4 > Level.CurrentLevel.TurnCount)
        {
            Debug.Log("you can place one trap every 4 turns");
            return;
        }

        _isCmdTrap = true;
        SetCurrentTile(new SpawnCommand("Trap"));
    }

    void MovePlayer()
    {
        SetCurrentTile(new WalkCommand());
    }

    void SummonDecoy()
    {
        _isCmdTrap = false;
        SetCurrentTile(new SpawnCommand("Decoy"));
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

            _playerPromise.Fulfill();
            _currentTile = null;
        }
    }

    private Promise _playerPromise;
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

        Level.CurrentLevel.Update();
    }
}

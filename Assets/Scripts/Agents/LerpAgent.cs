using UnityEngine;
using System.Collections;
using System.Linq;

public class LerpAgent : Agent
{
    public int Sight;
    readonly WalkCommand _cmd = new WalkCommand();

    void Start()
    {
        _cmd.SetSource(this);
    }

    protected override void DoStep()
    {
        var tiles = Level.CurrentLevel.Get(Position, _cmd.Range);
        var possible = tiles.Where(tile => _cmd.CanApply(tile.Coordinate)).ToArray();

        if (possible.Length == 0) return;

        var los = Level.CurrentLevel.GetLoS(Position, Sight);

        Coord? target = null;
        foreach (var tile in los)
        {
            if (tile == null) continue;
            
            if (tile.AgentsOnTile(agent => agent is Player).Length > 0)
            {
                target = tile.Coordinate;
                break;
            }
        }

        if (target != null)
        {
            var nearest = possible[0];
            var dist = Coord.Distance(target.Value, nearest.Coordinate);

            foreach (var tile in possible)
            {
                if (Coord.Distance(target.Value, tile.Coordinate) >= dist) continue;

                nearest = tile;
                dist = Coord.Distance(target.Value, tile.Coordinate);
            }

            _cmd.Apply(nearest.Coordinate);
        }
        else
        {
            if (Random.Range(0, 100) < 60)
            {
                _cmd.Apply(possible[Random.Range(0, possible.Length)].Coordinate);
            }
        }
    }
}

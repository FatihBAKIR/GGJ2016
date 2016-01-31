using UnityEngine;
using System.Linq;
using Assets.Scripts.Agents;

public class LerpAgent : Agent
{
    public int Sight;
    readonly WalkCommand _cmd = new WalkCommand();

    public Agent PreferedTarget;

    void Start()
    {
        _cmd.SetSource(this);
    }

    protected override void DoStep()
    {
        if (Wait > 0)
        {
            return;
        }

        var tiles = Level.CurrentLevel.Get(Position, _cmd.Range);
        var possible = tiles.Where(tile => _cmd.CanApply(tile.Coordinate)).ToArray();

        if (possible.Length == 0) return;

        var los = Level.CurrentLevel.GetLoS(Position, Sight);

        Coord? target = null;
        
        if (PreferedTarget == null)
        {
            foreach (var tile in los)
            {
                if (tile == null) continue;

                if (tile.AgentsOnTile(agent => agent is Player).Length > 0)
                {
                    target = tile.Coordinate;
                    break;
                }
            }
        }
        else
        {
            var dist = Coord.Distance(PreferedTarget.Position, Position);
            if (dist > 4)
            {
                PreferedTarget = null;
                DoStep();
                return;
            }

            target = PreferedTarget.Position;
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

            if (Coord.Distance(target.Value, Position) <= 1.15f)
            {
                if (Level.CurrentLevel.Get(target.Value).AgentsOnTile(agent => agent is Player).Length > 0)
                {
                    Level.CurrentLevel.Fail();
                    return;
                }
                
                if(Level.CurrentLevel.Get(target.Value).AgentsOnTile(agent => agent is Decoy).Length > 0)
                {
                    var decoy = Level.CurrentLevel.Get(target.Value).AgentsOnTile(agent => agent is Decoy).FirstOrDefault();
                    Level.CurrentLevel.Destroy(decoy);
                }
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

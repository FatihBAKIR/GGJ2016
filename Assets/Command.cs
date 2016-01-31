﻿using System;
using UnityEngine;

abstract class Command
{
    public event Action<Command> CommandComplete = delegate { };

    protected int Cost = 1;
    protected Agent Source;

    public void SetSource(Agent src)
    {
        Source = src;
    }

    public void Apply()
    {
        Source.Wait += Cost;
        DoApply();
    }

    protected virtual void DoApply()
    {
    }

    protected void Completed()
    {
        CommandComplete(this);
    }
}

abstract class TileCommand : Command
{
    public void Apply(Coord tile)
    {
        Source.Wait += Cost;
        DoApply(tile);
    }

    public virtual int Range
    {
        get { return 0; }
    }

    public virtual bool CanApply(Coord tile)
    {
        return true;
    }

    protected abstract void DoApply(Coord tile);
}

abstract class AgentCommand : Command
{
    public void Apply(Agent agent)
    {
        Source.Wait += Cost;
        DoApply(agent);
    }

    protected abstract void DoApply(Agent agent);
}

sealed class SpawnCommand : TileCommand
{
    private readonly string _name;

    public SpawnCommand(string agentName)
    {
        _name = agentName;
    }

    protected override void DoApply(Coord tile)
    {
        Level.CurrentLevel.Instantiate(_name, tile);
        Completed();
    }
}

sealed class WalkCommand : TileCommand
{
    enum Dir
    {
        PosX,
        PosZ,
        NegX,
        NegZ
    }

    Dir GetDir(Coord target)
    {
        Coord move = target - Source.Position;

        if (move == new Coord(1, 0))
        {
            return Dir.PosX;
        }
        if (move == new Coord(0, 1))
        {
            return Dir.PosZ;
        }
        if (move == new Coord(-1, 0))
        {
            return Dir.NegX;
        }
        if (move == new Coord(0, -1))
        {
            return Dir.NegZ;
        }

        throw new Exception("Never going to happen");
    }

    public override int Range
    {
        get { return 1; }
    }

    public override bool CanApply(Coord tile)
    {
        return Coord.Distance(tile, Source.Position) <= Range &&
               Level.CurrentLevel.Get(tile).AgentsOnTile(agent => agent.Blocks).Length == 0 &&
               Level.CurrentLevel.Get(tile).Elevation <= Level.CurrentLevel.Get(Source.Position).Elevation + 0.5f &&
               Level.CurrentLevel.Get(tile).Coordinate != Source.Position &&
               Level.CurrentLevel.Get(tile).Info.CanWalk;
    }

    private Promise _pr;

    protected override void DoApply(Coord tile)
    {
        Level.CurrentLevel.AddPromise(_pr = new Promise());

        Source.GetComponent<MoveTowards>().MoveComplete += OnMoveComplete;
        Source.GetComponent<MoveTowards>().SetTarget(tile);

        switch (GetDir(tile))
        {
            case Dir.PosX:
                Source.GetComponentInChildren<SkeletonAnimation>().transform.eulerAngles = new Vector3(0, 30, 0);
                break;
            case Dir.PosZ:
                Source.GetComponentInChildren<SkeletonAnimation>().transform.eulerAngles = new Vector3(0, 240, 0);
                break;
            case Dir.NegX:
                Source.GetComponentInChildren<SkeletonAnimation>().transform.eulerAngles = new Vector3(0, 210, 0);
                break;
            case Dir.NegZ:
                Source.GetComponentInChildren<SkeletonAnimation>().transform.eulerAngles = new Vector3(0, 60, 0);
                break;
        }

        Source.GetComponentInChildren<SkeletonAnimation>().AnimationName = "walking";
    }

    private void OnMoveComplete(MoveTowards moveTowards)
    {
        Completed();
        Source.GetComponentInChildren<SkeletonAnimation>().AnimationName = "idle";
        Source.GetComponent<MoveTowards>().MoveComplete -= OnMoveComplete;
        _pr.Fulfill();
        _pr = null;
    }
}
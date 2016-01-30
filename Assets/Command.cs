using UnityEngine;

abstract class Command
{
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
}

abstract class TileCommand : Command
{
    public void Apply(Coord tile)
    {
        Source.Wait += Cost;
        DoApply(tile);
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
    }
}

sealed class WalkCommand : TileCommand
{
    protected override void DoApply(Coord tile)
    {
        Source.GetComponent<MoveTowards>().SetTarget(tile);
    }
}
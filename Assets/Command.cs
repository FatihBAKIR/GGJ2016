class Command
{
    protected Agent Source;

    public void SetSource(Agent src)
    {
        Source = src;
    }

    public virtual void Apply()
    {
    }
}

class TileCommand : Command
{
    public virtual void Apply(Coord tile)
    {
    }
}

class AgentCommand : Command
{
    public virtual void Apply(Agent agent)
    {
    }
}
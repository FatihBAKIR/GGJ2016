using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Sigil : Agent
{
    public event Action<Sigil> Activated = delegate { }; 

    public virtual void Activate()
    {
    }

    protected void Completed()
    {
        Activated(this);
    }
}

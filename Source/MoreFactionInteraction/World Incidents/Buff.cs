using System;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public abstract class Buff : IExposable, IEquatable<Buff>
{
    public bool Active;

    public bool Equals(Buff obj)
    {
        return obj != null && obj.GetType() == GetType();
    }

    public abstract void ExposeData();

    public virtual TechLevel MinTechLevel()
    {
        return TechLevel.Undefined;
    }

    public abstract void Apply();
    public abstract string Description();
    public abstract ThingDef RelevantThingDef();

    //public override int GetHashCode() //oof.
    //{
    //    return base.GetHashCode();
    //}
}
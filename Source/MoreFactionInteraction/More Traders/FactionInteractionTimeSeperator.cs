using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

public static class FactionInteractionTimeSeperator
{
    public static readonly SimpleCurve TimeBetweenInteraction =
    [
        new CurvePoint(0,
            GenDate.TicksPerDay * 8 * Mathf.Max(1,
                Find.FactionManager.AllFactionsVisible.Count(f =>
                    !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && !f.temporary),
                10)),

        new CurvePoint(50,
            GenDate.TicksPerDay * 5 * Mathf.Max(1,
                Find.FactionManager.AllFactionsVisible.Count(f =>
                    !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && !f.temporary),
                10)),

        new CurvePoint(100,
            GenDate.TicksPerDay * 3 * Mathf.Max(1,
                Find.FactionManager.AllFactionsVisible.Count(f =>
                    !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && !f.temporary),
                10))
    ];
}
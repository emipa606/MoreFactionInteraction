using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar;

public class CaravanArrivalAction_VisitFactionWarPeaceTalks : CaravanArrivalAction
{
    private FactionWarPeaceTalks factionWarPeaceTalks;

    public CaravanArrivalAction_VisitFactionWarPeaceTalks()
    {
    }

    public CaravanArrivalAction_VisitFactionWarPeaceTalks(FactionWarPeaceTalks factionWarPeaceTalks)
    {
        this.factionWarPeaceTalks = factionWarPeaceTalks;
    }

    public override string Label => "VisitPeaceTalks".Translate(factionWarPeaceTalks.Label);

    public override string ReportString => "CaravanVisiting".Translate(factionWarPeaceTalks.Label);

    public override void Arrived(Caravan caravan)
    {
        factionWarPeaceTalks.Notify_CaravanArrived(caravan);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref factionWarPeaceTalks, "factionWarPeaceTalks");
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile planetTile)
    {
        if (!base.StillValid(caravan, planetTile))
        {
            return base.StillValid(caravan, planetTile);
        }

        if (factionWarPeaceTalks?.Tile != planetTile)
        {
            return false;
        }

        return CanVisit(factionWarPeaceTalks);
    }

    public static FloatMenuAcceptanceReport CanVisit(FactionWarPeaceTalks factionWarPeaceTalks)
    {
        return factionWarPeaceTalks is { Spawned: true };
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan,
        FactionWarPeaceTalks factionWarPeaceTalks)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(factionWarPeaceTalks),
            () => new CaravanArrivalAction_VisitFactionWarPeaceTalks(factionWarPeaceTalks),
            "VisitPeaceTalks".Translate(factionWarPeaceTalks.Label),
            caravan, factionWarPeaceTalks.Tile,
            factionWarPeaceTalks);
    }
}
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.General;

//I'm fancy, I wrote an extension method.
public static class MFI_Utilities
{
    public static Faction EnemyInFactionWar(this Faction faction)
    {
        if (faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne)
        {
            return Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;
        }

        return faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo
            ? Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
            : null;

        //Dear reader: Resharper suggests the following:
        //
        //return faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
        //      ? Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo
        //      : (faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo
        //          ? Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
        //          : null);
        //
        // which is a nested ternary and just awful to read. Be happy I spared you.
    }

    public static bool IsPartOfFactionWar(this Faction faction)
    {
        return faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
               || faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;
    }

    public static bool TryGetBestArt(Caravan caravan, out Thing thing, out Pawn owner)
    {
        thing = null;
        var list = CaravanInventoryUtility.AllInventoryItems(caravan);
        var num = 0f;
        foreach (var current in list)
        {
            if (current.GetInnerIfMinified().GetStatValue(StatDefOf.Beauty) > num &&
                (current.GetInnerIfMinified().TryGetComp<CompArt>()?.Props?.canBeEnjoyedAsArt ?? false))
            {
                thing = current;
            }
        }

        if (thing != null)
        {
            owner = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
            return true;
        }

        owner = null;
        return false;
    }

    public static bool IsScenarioBlocked(this IncidentWorker incidentWorker)
    {
        return Find.Scenario.AllParts.Any(x => x is ScenPart_DisableIncident scenPart
                                               && scenPart.Incident == incidentWorker.def);
    }

    public static bool IsScenarioBlocked(this IncidentDef incidentDef)
    {
        return Find.Scenario.AllParts.Any(x => x is ScenPart_DisableIncident scenPart
                                               && scenPart.Incident == incidentDef);
    }

    public static bool CaravanOrRichestColonyHasAnyOf(ThingDef thingdef, Caravan caravan, out Thing thing)
    {
        if (CaravanInventoryUtility.TryGetThingOfDef(caravan, thingdef, out thing, out _))
        {
            return true;
        }

        var maps = Find.Maps.FindAll(x => x.IsPlayerHome);

        if (maps.NullOrEmpty())
        {
            return false;
        }

        maps.SortBy(x => x.PlayerWealthForStoryteller);
        var richestMap = maps.First();

        if (thingdef.IsBuildingArtificial)
        {
            return FindBuildingOrMinifiedVersionThereOf(thingdef, richestMap, out thing);
        }

        var thingsOfDef = richestMap.listerThings.ThingsOfDef(thingdef);

        thing = thingsOfDef.FirstOrDefault();
        return thingsOfDef.Any();
    }

    private static bool FindBuildingOrMinifiedVersionThereOf(ThingDef thingdef, Map map, out Thing thing)
    {
        var buildingsOfDef = map.listerBuildings.AllBuildingsColonistOfDef(thingdef);
        if (buildingsOfDef.Any())
        {
            thing = buildingsOfDef.First();
            return true;
        }

        var minifiedBuilds = map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
        foreach (var t in minifiedBuilds)
        {
            if (t.GetInnerIfMinified().def != thingdef)
            {
                continue;
            }

            thing = t;
            return true;
        }

        thing = null;
        return false;
    }
}
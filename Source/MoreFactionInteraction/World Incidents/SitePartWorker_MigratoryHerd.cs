using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents;

public class SitePartWorker_MigratoryHerd : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return
            $"{base.GetPostProcessedThreatLabel(site, sitePart)} ({GenLabel.BestKindLabel(sitePart.parms.animalKind, Gender.None, true)})";
    }

    public override void PostMapGenerate(Map map)
    {
        var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
        incidentParms.forced = true;
        //this part is forced to bypass CanFireNowSub, to solve issue with scenario-added incident.
        var queuedIncident = new QueuedIncident(
            new FiringIncident(DefDatabase<IncidentDef>.GetNamed("MFI_HerdMigration_Ambush"), null, incidentParms),
            Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerDay / 2, GenDate.TicksPerDay));
        Find.Storyteller.incidentQueue.Add(queuedIncident);
    }

    private static bool TryFindAnimalKind(PlanetTile tile, out PawnKindDef animalKind)
    {
        var fallback = PawnKindDefOf.Thrumbo;

        animalKind = (from k in DefDatabase<PawnKindDef>.AllDefs
            where k.RaceProps.CanDoHerdMigration &&
                  Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)
            select k).RandomElementByWeightWithFallback(x => x.race.GetStatValueAbstract(StatDefOf.Wildness), fallback);

        return animalKind != fallback;
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
    {
        var siteCoreOrPartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        if (TryFindAnimalKind(tile, out siteCoreOrPartParams.animalKind))
        {
            siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints,
                siteCoreOrPartParams.animalKind.combatPower);
        }

        return siteCoreOrPartParams;
    }
}
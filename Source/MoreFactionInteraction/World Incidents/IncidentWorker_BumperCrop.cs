using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents;

public class IncidentWorker_BumperCrop : IncidentWorker
{
    private static readonly IntRange OfferDurationRange = new IntRange(10, 30);

    public override float BaseChanceThisGame => base.BaseChanceThisGame
                                                + (float)(Find.FactionManager.AllFactionsVisible
                                                              .Where(faction =>
                                                                  !faction.defeated && !faction.IsPlayer &&
                                                                  !faction.HostileTo(Faction.OfPlayer) &&
                                                                  !faction.temporary)
                                                              .Average(faction =>
                                                                  faction.GoodwillWith(Faction.OfPlayer)) /
                                                          100);

    public override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && TryGetRandomAvailableTargetMap(out var map)
                                         && RandomNearbyGrowerSettlement(map.Tile) != null
                                         && VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(
                                             RandomNearbyGrowerSettlement(map.Tile).Tile);
    }

    public override bool TryExecuteWorker(IncidentParms parms)
    {
        TryGetRandomAvailableTargetMap(out var map);
        if (map == null)
        {
            return false;
        }

        var settlement = RandomNearbyGrowerSettlement(map.Tile);

        if (settlement == null)
        {
            return false;
        }

        var component = settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>();

        if (!TryGenerateBumperCrop(component, map))
        {
            return false;
        }

        Find.LetterStack.ReceiveLetter("MFI_LetterLabel_HarvestRequest".Translate(),
            "MFI_LetterHarvestRequest".Translate(
                settlement.Label,
                (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0")
            ), LetterDefOf.PositiveEvent, settlement, settlement.Faction);
        return true;
    }

    private static bool TryGenerateBumperCrop(WorldObjectComp_SettlementBumperCropComp target, Map map)
    {
        var num = RandomOfferDuration(map.Tile, target.parent.Tile);
        if (num < 1)
        {
            return false;
        }

        target.expiration = Find.TickManager.TicksGame + num;
        return true;
    }

    private static Settlement RandomNearbyGrowerSettlement(int originTile)
    {
        return Find.WorldObjects.Settlements
            .Where(settlement => settlement is { Visitable: true } &&
                                 settlement.GetComponent<TradeRequestComp>() != null &&
                                 !settlement.GetComponent<TradeRequestComp>().ActiveRequest &&
                                 settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>() != null &&
                                 !settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>()
                                     .ActiveRequest &&
                                 Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f &&
                                 Find.WorldReachability.CanReach(originTile, settlement.Tile))
            .RandomElementWithFallback();
    }

    private static int RandomOfferDuration(int tileIdFrom, int tileIdTo)
    {
        var offerValidForDays = OfferDurationRange.RandomInRange;
        var travelTimeByCaravan = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
        var daysWorthOfTravel = (float)travelTimeByCaravan / GenDate.TicksPerDay;
        var b = Mathf.CeilToInt(Mathf.Max(daysWorthOfTravel + 1f, daysWorthOfTravel * 1.1f));
        offerValidForDays = Mathf.Max(offerValidForDays, b);
        if (offerValidForDays > OfferDurationRange.max)
        {
            return -1;
        }

        return GenDate.TicksPerDay * offerValidForDays;
    }

    private static bool TryGetRandomAvailableTargetMap(out Map map)
    {
        return Find.Maps
            .Where(maps =>
                maps.IsPlayerHome && AtLeast2HealthyColonists(maps) &&
                RandomNearbyGrowerSettlement(maps.Tile) != null)
            .TryRandomElement(out map);
    }

    private static bool AtLeast2HealthyColonists(Map map)
    {
        var pawnList = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
        var healthyColonists = 0;

        foreach (var pawn in pawnList)
        {
            if (!pawn.IsFreeColonist || HealthAIUtility.ShouldSeekMedicalRest(pawn))
            {
                continue;
            }

            healthyColonists++;
            if (healthyColonists >= 2)
            {
                return true;
            }
        }

        return false;
    }
}
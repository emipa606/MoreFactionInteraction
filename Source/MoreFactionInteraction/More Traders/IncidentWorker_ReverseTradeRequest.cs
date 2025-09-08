using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class IncidentWorker_ReverseTradeRequest : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && TryGetRandomAvailableTargetMap(out var map)
                                         && RandomNearbyTradeableSettlement(map.Tile) != null
                                         && CommsConsoleUtility.PlayerHasPoweredCommsConsole(map);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryGetRandomAvailableTargetMap(out var map))
        {
            return false;
        }

        var settlement = RandomNearbyTradeableSettlement(map.Tile);

        if (settlement?.Faction?.leader == null)
        {
            return false;
        }

        //TODO: look into making the below dynamic based on requester's biome, faction, pirate outpost vicinity and other stuff.
        var thingCategoryDef = DetermineThingCategoryDef();

        var letterToSend = DetermineLetterToSend(thingCategoryDef);
        var feeRequest = Math.Max(Rand.Range(150, 300), (int)parms.points);
        var categorylabel = thingCategoryDef == ThingCategoryDefOf.PlantFoodRaw
            ? $"{thingCategoryDef.label} items"
            : thingCategoryDef.label;
        var diaNode = new DiaNode(letterToSend.Translate(
            settlement.Faction.leader.LabelShort,
            settlement.Faction.def.leaderTitle,
            settlement.Faction.Name,
            settlement.Label,
            categorylabel,
            feeRequest
        ).AdjustedFor(settlement.Faction.leader));

        var traveltime = CalcuteTravelTimeForTrader(settlement.Tile, map);
        var accept = new DiaOption("RansomDemand_Accept".Translate())
        {
            action = () =>
            {
                //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                parms.faction = settlement.Faction;
                var traderKind = DefDatabase<TraderKindDef>.GetNamed($"MFI_EmptyTrader_{thingCategoryDef}");

                traderKind.stockGenerators.First(x => x.HandlesThingDef(ThingDefOf.Silver)).countRange.max +=
                    feeRequest;
                traderKind.stockGenerators.First(x => x.HandlesThingDef(ThingDefOf.Silver)).countRange.min +=
                    feeRequest;

                traderKind.label = $"{thingCategoryDef.label} " + "MFI_Trader".Translate();
                parms.traderKind = traderKind;
                parms.forced = true;
                parms.target = map;

                Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival,
                    Find.TickManager.TicksGame + traveltime, parms);
                TradeUtility.LaunchSilver(map, feeRequest);
            }
        };
        var acceptLink = new DiaNode("MFI_TraderSent".Translate(
            settlement.Faction.leader?.LabelShort,
            traveltime.ToStringTicksToPeriodVague(false)
        ).CapitalizeFirst());
        acceptLink.options.Add(DiaOption.DefaultOK);
        accept.link = acceptLink;

        if (!TradeUtility.ColonyHasEnoughSilver(map, feeRequest))
        {
            accept.Disable("NeedSilverLaunchable".Translate(feeRequest.ToString()));
        }

        var reject = new DiaOption("MFI_Reject".Translate())
        {
            action = () => { },
            resolveTree = true
        };

        diaNode.options = [accept, reject];

        Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, settlement.Faction,
            title: "MFI_ReverseTradeRequestTitle".Translate(map.info.parent.Label).CapitalizeFirst()));
        Find.Archive.Add(new ArchivedDialog(diaNode.text,
            "MFI_ReverseTradeRequestTitle".Translate(map.info.parent.Label).CapitalizeFirst(), settlement.Faction));

        return true;
    }

    private static ThingCategoryDef DetermineThingCategoryDef()
    {
        var rand = Rand.RangeInclusive(0, 100);

        switch (rand)
        {
            case <= 33:
                return ThingCategoryDefOf.Apparel;
            case <= 66:
                return ThingCategoryDefOf.PlantFoodRaw;
            case < 90:
                return ThingCategoryDefOf.Weapons;
            default:
                return ThingCategoryDefOf.Medicine;
        }
    }

    private static string DetermineLetterToSend(ThingCategoryDef thingCategoryDef)
    {
        if (thingCategoryDef == ThingCategoryDefOf.PlantFoodRaw)
        {
            return "MFI_ReverseTradeRequest_Blight";
        }

        return Rand.RangeInclusive(0, 4) switch
        {
            0 => "MFI_ReverseTradeRequest_Pyro",
            1 => "MFI_ReverseTradeRequest_Mechs",
            2 => "MFI_ReverseTradeRequest_Caravan",
            3 => "MFI_ReverseTradeRequest_Pirates",
            4 => "MFI_ReverseTradeRequest_Hardship",
            _ => "MFI_ReverseTradeRequest_Pyro"
        };
    }

    private static Settlement RandomNearbyTradeableSettlement(int originTile)
    {
        return Find.WorldObjects.Settlements.Where(settlement =>
            settlement is { Visitable: true, Faction.leader: not null } &&
            settlement.GetComponent<TradeRequestComp>() != null &&
            !settlement.GetComponent<TradeRequestComp>().ActiveRequest &&
            Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f &&
            Find.WorldReachability.CanReach(originTile, settlement.Tile)).RandomElementWithFallback();
    }

    private static bool TryGetRandomAvailableTargetMap(out Map map)
    {
        return Find.Maps.Where(x => x.IsPlayerHome && RandomNearbyTradeableSettlement(x.Tile) != null)
            .TryRandomElement(out map);
    }

    private int CalcuteTravelTimeForTrader(int originTile, Map map)
    {
        var travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(originTile, map.Tile, null);
        return Math.Min(travelTime, GenDate.TicksPerDay * 4);
    }
}
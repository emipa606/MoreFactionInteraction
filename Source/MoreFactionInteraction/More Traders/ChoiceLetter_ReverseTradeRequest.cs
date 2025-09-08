using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

[Obsolete]
public class ChoiceLetter_ReverseTradeRequest : ChoiceLetter
{
    public Faction faction;
    public int fee = 100;
    public IncidentParms incidentParms;
    public Map map;
    public ThingCategoryDef thingCategoryDef;
    public PlanetTile tile;

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
            }
            else
            {
                var traveltime = CalcuteTravelTimeForTrader(tile);
                var accept = new DiaOption("Accept".Translate())
                {
                    action = () =>
                    {
                        //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                        //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                        incidentParms.faction = faction;
                        var traderKind = DefDatabase<TraderKindDef>.GetNamed($"MFI_EmptyTrader_{thingCategoryDef}");

                        traderKind.stockGenerators.First(x => x.HandlesThingDef(ThingDefOf.Silver)).countRange
                            .max += fee;
                        traderKind.stockGenerators.First(x => x.HandlesThingDef(ThingDefOf.Silver)).countRange
                            .min += fee;

                        traderKind.label = $"{thingCategoryDef.label} " + "MFI_Trader".Translate();
                        incidentParms.traderKind = traderKind;
                        incidentParms.forced = true;
                        incidentParms.target = map;

                        Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival,
                            Find.TickManager.TicksGame + traveltime, incidentParms);
                        TradeUtility.LaunchSilver(map, fee);
                    }
                };
                var acceptLink = new DiaNode("MFI_TraderSent".Translate(
                    faction.leader?.LabelShort,
                    traveltime.ToStringTicksToPeriodVague(false)
                ).CapitalizeFirst());
                acceptLink.options.Add(Option_Close);
                accept.link = acceptLink;

                if (!TradeUtility.ColonyHasEnoughSilver(map, fee))
                {
                    accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
                }

                yield return accept;

                var reject = new DiaOption("MFI_Reject".Translate())
                {
                    action = () => Find.LetterStack.RemoveLetter(this),
                    resolveTree = true
                };
                yield return reject;
                yield return Option_Postpone;
            }
        }
    }

    public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(map) &&
                                                 !faction.HostileTo(Faction.OfPlayer);

    private int CalcuteTravelTimeForTrader(int originTile)
    {
        var travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(originTile, map.Tile, null);
        return Math.Min(travelTime, GenDate.TicksPerDay * 4);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref thingCategoryDef, "MFI_thingCategoryDef");
        Scribe_Deep.Look(ref incidentParms, "MFI_incidentParms");
        Scribe_References.Look(ref map, "MFI_map");
        Scribe_References.Look(ref faction, "MFI_faction");
        Scribe_Values.Look(ref fee, "MFI_fee");
        Scribe_Values.Look(ref tile, "MFI_tile");
    }
}
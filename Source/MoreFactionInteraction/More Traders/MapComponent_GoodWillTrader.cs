using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MoreFactionInteraction.General;
using RimWorld;
using Verse;

namespace MoreFactionInteraction;

public class MapComponent_GoodWillTrader : MapComponent
{
    private readonly List<IncidentDef> allowedIncidentDefs =
        new List<IncidentDef>
        {
            MFI_DefOf.MFI_QuestSpreadingPirateCamp,
            MFI_DefOf.MFI_DiplomaticMarriage,
            MFI_DefOf.MFI_ReverseTradeRequest,
            MFI_DefOf.MFI_BumperCropRequest,
            MFI_DefOf.MFI_HuntersLodge,
            IncidentDef.Named("MFI_MysticalShaman"),
            IncidentDefOf.TraderCaravanArrival
        };

    private readonly List<IncidentDef> incidentsInNeedOfValidFactionLeader =
        new List<IncidentDef>
        {
            MFI_DefOf.MFI_ReverseTradeRequest,
            MFI_DefOf.MFI_HuntersLodge
        };

    //working lists for ExposeData.
    private List<Faction> factionsListforInteraction = new List<Faction>();
    private List<Faction> factionsListforTimesTraded = new List<Faction>();
    private List<int> intListForInteraction = new List<int>();
    private List<int> intListforTimesTraded = new List<int>();
    private Dictionary<Faction, int> nextFactionInteraction = new Dictionary<Faction, int>();
    private List<QueuedIncident> queued;
    private Dictionary<Faction, int> timesTraded = new Dictionary<Faction, int>();

    //empty constructor
    public MapComponent_GoodWillTrader(Map map) : base(map)
    {
    }

    /// <summary>
    ///     Used to keep track of how many times the player traded with the faction and increase trader stock based on that.
    /// </summary>
    public Dictionary<Faction, int> TimesTraded
    {
        get
        {
            //intermingled :D
            foreach (var faction in NextFactionInteraction.Keys)
            {
                if (!timesTraded.Keys.Contains(faction))
                {
                    timesTraded.Add(faction, 0);
                }
            }

            //trust betrayed, reset count.
            timesTraded.RemoveAll(x => x.Key.temporary || x.Key.HostileTo(Faction.OfPlayerSilentFail));
            return timesTraded;
        }
    }

    private Dictionary<Faction, int> NextFactionInteraction
    {
        get
        {
            //initialise values
            if (nextFactionInteraction.Count == 0)
            {
                var friendlyFactions = from faction in Find.FactionManager.AllFactionsVisible
                    where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail &&
                          !Faction.OfPlayer.HostileTo(faction) && !faction.temporary
                    select faction;

                foreach (var faction in friendlyFactions)
                {
                    Rand.PushState(faction.loadID ^ faction.GetHashCode());
                    nextFactionInteraction.Add(faction,
                        Find.TickManager.TicksGame +
                        (int)Math.Round(Rand.RangeInclusive(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 8) *
                                        MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction));
                    Rand.PopState();
                }
            }

            if (nextFactionInteraction.RemoveAll(x => x.Key.HostileTo(Faction.OfPlayerSilentFail)) > 0)
            {
                CleanIncidentQueue(null, true);
            }


            //and the opposite
            while ((from faction in Find.FactionManager.AllFactionsVisible
                       where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail &&
                             !faction.HostileTo(Faction.OfPlayerSilentFail) &&
                             !nextFactionInteraction.ContainsKey(faction) && !faction.temporary
                       select faction).Any())
            {
                nextFactionInteraction.Add(
                    (from faction in Find.FactionManager.AllFactionsVisible
                        where !faction.HostileTo(Faction.OfPlayerSilentFail) && !faction.IsPlayer &&
                              !nextFactionInteraction.ContainsKey(faction) && !faction.temporary
                        select faction).First(),
                    Find.TickManager.TicksGame +
                    (int)Math.Round(Rand.RangeInclusive(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 8) *
                                    MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction));
            }

            return nextFactionInteraction;
        }
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();

        allowedIncidentDefs.RemoveAll(x => x.IsScenarioBlocked());

        queued = Traverse.Create(Find.Storyteller.incidentQueue).Field("queuedIncidents")
            .GetValue<List<QueuedIncident>>();

        if (queued == null)
        {
            throw new NullReferenceException("MFI failed to initialise IncidentQueue in MapComponent.");
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();

        //We don't need to run all that often
        if (Find.TickManager.TicksGame % 531 != 0 || GenDate.DaysPassed <= 8)
        {
            return;
        }

        CleanIncidentQueue(null);

        foreach (var kvp in NextFactionInteraction)
        {
            if (Find.TickManager.TicksGame < kvp.Value)
            {
                continue;
            }

            var faction = kvp.Key;
            var incident =
                IncomingIncidentDef(faction) ?? IncomingIncidentDef(faction); // "try again" null-check.
            var incidentParms = StorytellerUtility.DefaultParmsNow(incident.category, map);
            incidentParms.faction = faction;
            //forced, because half the time game doesn't feel like firing events.
            incidentParms.forced = true;

            //trigger incident somewhere between half a day and 3 days from now
            Find.Storyteller.incidentQueue.Add(incident,
                Find.TickManager.TicksGame + Rand.Range(GenDate.TicksPerDay / 2, GenDate.TicksPerDay * 3),
                incidentParms,
                2500);

            NextFactionInteraction[faction] =
                Find.TickManager.TicksGame
                + (int)(FactionInteractionTimeSeperator.TimeBetweenInteraction.Evaluate(
                            faction.PlayerGoodwill)
                        * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction);

            //kids, you shouldn't change values you iterate over.
            break;
        }
    }

    public override void MapRemoved()
    {
        base.MapRemoved();
        CleanIncidentQueue(map);
    }

    private IncidentDef IncomingIncidentDef(Faction faction)
    {
        return allowedIncidentDefs
            .Where(x => faction.leader != null || !incidentsInNeedOfValidFactionLeader.Contains(x))
            .RandomElementByWeight(x => x.Worker.BaseChanceThisGame);
    }

    private void CleanIncidentQueue(Map localMap, bool removeHostile = false)
    {
        if (queued == null)
        {
            FinalizeInit();
        }

        if (removeHostile)
        {
            queued?.RemoveAll(qi =>
                qi.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer) &&
                allowedIncidentDefs.Contains(qi.FiringIncident.def));
        }

        //Theoretically there is no way this should happen. Reality has proven me wrong.
        queued?.RemoveAll(qi => qi.FiringIncident.parms.target == null
                                || qi.FiringIncident.parms.target == localMap
                                || qi.FiringIncident.def == null
                                || qi.FireTick + GenDate.TicksPerDay < Find.TickManager.TicksGame);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref nextFactionInteraction, "MFI_nextFactionInteraction", LookMode.Reference,
            LookMode.Value, ref factionsListforInteraction, ref intListForInteraction);
        Scribe_Collections.Look(ref timesTraded, "MFI_timesTraded", LookMode.Reference, LookMode.Value,
            ref factionsListforTimesTraded, ref intListforTimesTraded);
    }
}
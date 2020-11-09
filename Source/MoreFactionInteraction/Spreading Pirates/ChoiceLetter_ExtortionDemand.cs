using System.Collections.Generic;
using Verse;
using RimWorld;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_ExtortionDemand : ChoiceLetter
    {
        public Map map;
        public Faction faction;
        public bool outpost = false;
        public int fee;
        public bool completed = false;

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
                    var accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            completed = true;
                            TradeUtility.LaunchSilver(map: map, fee: fee);
                            Find.LetterStack.RemoveLetter(@let: this);
                        },
                        resolveTree = true
                    };
                    if (!TradeUtility.ColonyHasEnoughSilver(map: map, fee: fee))
                    {
                        accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(fee.ToString()));
                    }
                    yield return accept;

                    var reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            completed = true;
                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.ThreatBig, target: map);
                            incidentParms.forced = true;
                            incidentParms.faction = faction;
                            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                            incidentParms.target = map;
                            if (outpost)
                            {
                                incidentParms.points *= 0.7f;
                            }

                            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms: incidentParms);
                            Find.LetterStack.RemoveLetter(@let: this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack => Find.Maps.Contains(item: map);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(refee: ref map, label: "map");
            Scribe_References.Look<Faction>(refee: ref faction, label: "faction");
            Scribe_Values.Look<int>(value: ref fee, label: "fee");
            Scribe_Values.Look(ref completed, "completed");
        }
    }
}

using System;
using System.Linq;
using MoreFactionInteraction.General;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class IncidentWorker_DiplomaticMarriage : IncidentWorker
    {
        private const int TimeoutTicks = GenDate.TicksPerDay;
        private Pawn betrothed;
        private Pawn marriageSeeker;

        public override float BaseChanceThisGame => Math.Max(0.01f,
            base.BaseChanceThisGame - StorytellerUtilityPopulation.PopulationIntent);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TryFindMarriageSeeker(out marriageSeeker)
                                             && TryFindBetrothed(out betrothed)
                                             && !this.IsScenarioBlocked();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindMarriageSeeker(out marriageSeeker))
            {
                if (Prefs.LogVerbose)
                {
                    Log.Warning("no marriageseeker");
                }

                return false;
            }

            if (!TryFindBetrothed(out betrothed))
            {
                if (Prefs.LogVerbose)
                {
                    Log.Warning("no betrothed");
                }

                return false;
            }

            var text = "MFI_DiplomaticMarriage"
                .Translate(marriageSeeker.LabelShort, betrothed.LabelShort, marriageSeeker.Faction.Name)
                .AdjustedFor(marriageSeeker);

            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, marriageSeeker);

            var choiceLetterDiplomaticMarriage =
                (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(def.letterLabel, text, def.letterDef);
            choiceLetterDiplomaticMarriage.title =
                "MFI_DiplomaticMarriageLabel".Translate(betrothed.LabelShort).CapitalizeFirst();
            choiceLetterDiplomaticMarriage.radioMode = false;
            choiceLetterDiplomaticMarriage.marriageSeeker = marriageSeeker;
            choiceLetterDiplomaticMarriage.betrothed = betrothed;
            choiceLetterDiplomaticMarriage.StartTimeout(TimeoutTicks);
            Find.LetterStack.ReceiveLetter(choiceLetterDiplomaticMarriage);
            //Find.World.GetComponent<WorldComponent_OutpostGrower>().Registerletter(choiceLetterDiplomaticMarriage);
            return true;
        }

        private bool TryFindBetrothed(out Pawn betrothed)
        {
            return (from potentialPartners in PawnsFinder
                    .AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                where !LovePartnerRelationUtility.HasAnyLovePartner(potentialPartners) ||
                      LovePartnerRelationUtility.ExistingMostLikedLovePartner(potentialPartners, false) ==
                      marriageSeeker
                select potentialPartners).TryRandomElementByWeight(
                marriageSeeker2 => marriageSeeker.relations.SecondaryLovinChanceFactor(marriageSeeker2), out betrothed);
        }

        private static bool TryFindMarriageSeeker(out Pawn marriageSeeker)
        {
            return (from x in Find.WorldPawns.AllPawnsAlive
                where x.Faction != null && !x.Faction.def.hidden && !x.Faction.def.permanentEnemy && !x.Faction.IsPlayer
                      && x.Faction.PlayerGoodwill <= 50 && !x.Faction.defeated &&
                      x.Faction.def.techLevel <= TechLevel.Medieval
                      && x.Faction.leader != null && !x.Faction.leader.IsPrisoner && !x.Faction.leader.Spawned
                      && !x.IsPrisoner && !x.Spawned && x.relations != null && x.RaceProps.Humanlike
                      && !SettlementUtility.IsPlayerAttackingAnySettlementOf(x.Faction) && !PeaceTalksExist(x.Faction)
                      && (!LovePartnerRelationUtility.HasAnyLovePartner(x) ||
                          LovePartnerRelationUtility.ExistingMostLikedLovePartner(x, false)?.Faction ==
                          Faction.OfPlayer)
                select x).TryRandomElement(out marriageSeeker); //todo: make more likely to select hostile.
        }

        private static bool PeaceTalksExist(Faction faction)
        {
            var peaceTalks = Find.WorldObjects.PeaceTalks;
            foreach (var peaceTalk in peaceTalks)
            {
                if (peaceTalk.Faction == faction)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
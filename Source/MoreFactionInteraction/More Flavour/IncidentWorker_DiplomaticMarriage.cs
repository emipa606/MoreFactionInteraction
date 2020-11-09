using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System;
using MoreFactionInteraction.General;

namespace MoreFactionInteraction
{
    public class IncidentWorker_DiplomaticMarriage : IncidentWorker
    {
        private Pawn marriageSeeker;
        private Pawn betrothed;
        private const int TimeoutTicks = GenDate.TicksPerDay;

        public override float BaseChanceThisGame => Math.Max(0.01f, base.BaseChanceThisGame - StorytellerUtilityPopulation.PopulationIntent);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && TryFindMarriageSeeker(marriageSeeker: out marriageSeeker)
                                                    && TryFindBetrothed(betrothed: out betrothed)
                                                    && !this.IsScenarioBlocked();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindMarriageSeeker(marriageSeeker: out marriageSeeker))
            {
                if (Prefs.LogVerbose)
                {
                    Log.Warning(text: "no marriageseeker");
                }

                return false;
            }
            if (!TryFindBetrothed(betrothed: out betrothed))
            {
                if (Prefs.LogVerbose)
                {
                    Log.Warning(text: "no betrothed");
                }

                return false;
            }

            var text = "MFI_DiplomaticMarriage".Translate(marriageSeeker.LabelShort, betrothed.LabelShort, marriageSeeker.Faction.Name).AdjustedFor(p: marriageSeeker);

            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, marriageSeeker);

            var choiceLetterDiplomaticMarriage = (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(label: def.letterLabel, text: text, def: def.letterDef);
            choiceLetterDiplomaticMarriage.title = "MFI_DiplomaticMarriageLabel".Translate(betrothed.LabelShort).CapitalizeFirst();
            choiceLetterDiplomaticMarriage.radioMode = false;
            choiceLetterDiplomaticMarriage.marriageSeeker = marriageSeeker;
            choiceLetterDiplomaticMarriage.betrothed = betrothed;
            choiceLetterDiplomaticMarriage.StartTimeout(duration: TimeoutTicks);
            Find.LetterStack.ReceiveLetter(@let: choiceLetterDiplomaticMarriage);
            //Find.World.GetComponent<WorldComponent_OutpostGrower>().Registerletter(choiceLetterDiplomaticMarriage);
            return true;
        }

        private bool TryFindBetrothed(out Pawn betrothed)
        {
            return (from potentialPartners in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                    where !LovePartnerRelationUtility.HasAnyLovePartner(potentialPartners) || LovePartnerRelationUtility.ExistingMostLikedLovePartner(potentialPartners, false) == marriageSeeker
                    select potentialPartners).TryRandomElementByWeight(weightSelector: (Pawn marriageSeeker2) => marriageSeeker.relations.SecondaryLovinChanceFactor(otherPawn: marriageSeeker2), result: out betrothed);
        }

        private static bool TryFindMarriageSeeker(out Pawn marriageSeeker)
        {
            return (from x in Find.WorldPawns.AllPawnsAlive
                    where x.Faction != null && !x.Faction.def.hidden && !x.Faction.def.permanentEnemy && !x.Faction.IsPlayer
                       && x.Faction.PlayerGoodwill <= 50 && !x.Faction.defeated && x.Faction.def.techLevel <= TechLevel.Medieval
                       && x.Faction.leader != null && !x.Faction.leader.IsPrisoner && !x.Faction.leader.Spawned
                    && !x.IsPrisoner && !x.Spawned && x.relations != null && x.RaceProps.Humanlike
                    && !SettlementUtility.IsPlayerAttackingAnySettlementOf(faction: x.Faction) && !PeaceTalksExist(faction: x.Faction)
                    && (!LovePartnerRelationUtility.HasAnyLovePartner(pawn: x) || LovePartnerRelationUtility.ExistingMostLikedLovePartner(p: x, allowDead: false)?.Faction == Faction.OfPlayer)
                    select x).TryRandomElement(result: out marriageSeeker); //todo: make more likely to select hostile.
        }

        private static bool PeaceTalksExist(Faction faction)
        {
            List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
            foreach (PeaceTalks peaceTalk in peaceTalks)
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

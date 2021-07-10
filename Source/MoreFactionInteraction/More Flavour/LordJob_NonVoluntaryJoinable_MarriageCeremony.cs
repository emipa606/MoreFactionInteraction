using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MoreFactionInteraction
{
    public class LordJob_NonVoluntaryJoinable_MarriageCeremony : LordJob
    {
        private const int TicksPerPartyPulse = 2200;

        private Trigger_TicksPassed afterPartyTimeoutTrigger;
        private Pawn firstPawn;

        private Pawn secondPawn;

        private IntVec3 spot;

        public LordJob_NonVoluntaryJoinable_MarriageCeremony()
        {
        }

        public LordJob_NonVoluntaryJoinable_MarriageCeremony(Pawn firstPawn, Pawn secondPawn, IntVec3 spot)
        {
            this.firstPawn = firstPawn;
            this.secondPawn = secondPawn;
            this.spot = spot;
        }

        public override bool LostImportantReferenceDuringLoading => firstPawn == null || secondPawn == null;

        public override bool AllowStartNewGatherings => false;

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            var lordToil_Party = new LordToil_Party(spot, GatheringDefOf.MarriageCeremony);
            stateGraph.AddToil(lordToil_Party);
            var lordToil_MarriageCeremony = new LordToil_MarriageCeremony(firstPawn, secondPawn, spot);
            stateGraph.AddToil(lordToil_MarriageCeremony);
            var lordToil_Party2 = new LordToil_Party(spot, GatheringDefOf.MarriageCeremony);
            stateGraph.AddToil(lordToil_Party2);
            var lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            var transition = new Transition(lordToil_Party, lordToil_MarriageCeremony);
            transition.AddTrigger(new Trigger_TickCondition(() => lord.ticksInToil >= 5000 && AreFiancesInPartyArea()));
            transition.AddPreAction(new TransitionAction_Message(
                "MessageMarriageCeremonyStarts".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.PositiveEvent, firstPawn));
            stateGraph.AddTransition(transition);
            var transition2 = new Transition(lordToil_MarriageCeremony, lordToil_Party2);
            transition2.AddTrigger(new Trigger_TickCondition(() =>
                firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, secondPawn)));
            transition2.AddPreAction(new TransitionAction_Message(
                "MessageNewlyMarried".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.PositiveEvent, new TargetInfo(spot, Map)));
            transition2.AddPreAction(new TransitionAction_Custom(AddAttendedWeddingThoughts));
            stateGraph.AddTransition(transition2);
            var transition3 = new Transition(lordToil_Party2, lordToil_End);
            transition3.AddTrigger(new Trigger_TickCondition(ShouldAfterPartyBeCalledOff));
            transition3.AddTrigger(new Trigger_PawnKilled());
            transition3.AddPreAction(new TransitionAction_Message(
                "MessageMarriageCeremonyCalledOff".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, Map)));
            stateGraph.AddTransition(transition3);
            afterPartyTimeoutTrigger = new Trigger_TicksPassed(7500);
            var transition4 = new Transition(lordToil_Party2, lordToil_End);
            transition4.AddTrigger(afterPartyTimeoutTrigger);
            transition4.AddPreAction(new TransitionAction_Message(
                "MessageMarriageCeremonyAfterPartyFinished".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.PositiveEvent, firstPawn));
            stateGraph.AddTransition(transition4);
            var transition5 = new Transition(lordToil_MarriageCeremony, lordToil_End);
            transition5.AddSource(lordToil_Party);
            transition5.AddTrigger(new Trigger_TickCondition(() =>
                lord.ticksInToil >= 120000 && (firstPawn.Drafted || secondPawn.Drafted ||
                                               !firstPawn.Position.InHorDistOf(spot, 4f) ||
                                               !secondPawn.Position.InHorDistOf(spot, 4f))));
            transition5.AddPreAction(new TransitionAction_Message(
                "MessageMarriageCeremonyCalledOff".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, Map)));
            stateGraph.AddTransition(transition5);
            var transition6 = new Transition(lordToil_MarriageCeremony, lordToil_End);
            transition6.AddSource(lordToil_Party);
            transition6.AddTrigger(new Trigger_TickCondition(ShouldCeremonyBeCalledOff));
            transition6.AddTrigger(new Trigger_PawnKilled());
            transition6.AddPreAction(new TransitionAction_Message(
                "MessageMarriageCeremonyCalledOff".Translate(firstPawn.LabelShort, secondPawn.LabelShort),
                MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, Map)));
            stateGraph.AddTransition(transition6);
            return stateGraph;
        }

        private bool AreFiancesInPartyArea()
        {
            return lord.ownedPawns.Contains(firstPawn) && lord.ownedPawns.Contains(secondPawn) &&
                   firstPawn.Map == Map && GatheringsUtility.InGatheringArea(firstPawn.Position, spot, Map) &&
                   secondPawn.Map == Map && GatheringsUtility.InGatheringArea(secondPawn.Position, spot, Map);
        }

        private bool ShouldCeremonyBeCalledOff()
        {
            return firstPawn.Destroyed || secondPawn.Destroyed ||
                   !firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, secondPawn) ||
                   spot.GetDangerFor(firstPawn, Map) != Danger.None ||
                   spot.GetDangerFor(secondPawn, Map) != Danger.None ||
                   !MarriageCeremonyUtility.AcceptableGameConditionsToStartCeremony(Map) ||
                   !MarriageCeremonyUtility.FianceCanContinueCeremony(firstPawn, secondPawn);
        }

        private bool ShouldAfterPartyBeCalledOff()
        {
            return firstPawn.Destroyed || secondPawn.Destroyed || firstPawn.Downed || secondPawn.Downed ||
                   spot.GetDangerFor(firstPawn, Map) != Danger.None ||
                   spot.GetDangerFor(secondPawn, Map) != Danger.None ||
                   !GatheringsUtility.AcceptableGameConditionsToContinueGathering(Map);
        }

        //public override float VoluntaryJoinPriorityFor(Pawn p)
        //{
        //    float result;
        //    if (this.IsFiance(p))
        //    {
        //        if (!MarriageCeremonyUtility.FianceCanContinueCeremony(p))
        //        {
        //            result = 0f;
        //        }
        //        else
        //        {
        //            result = VoluntarilyJoinableLordJobJoinPriorities.MarriageCeremonyFiance;
        //        }
        //    }
        //    else if (this.IsGuest(p))
        //    {
        //        if (!MarriageCeremonyUtility.ShouldGuestKeepAttendingCeremony(p))
        //        {
        //            result = 0f;
        //        }
        //        else
        //        {
        //            if (!this.lord.ownedPawns.Contains(p))
        //            {
        //                if (this.IsCeremonyAboutToEnd())
        //                {
        //                    result = 0f;
        //                    return result;
        //                }
        //                LordToil_MarriageCeremony lordToil_MarriageCeremony = this.lord.CurLordToil as LordToil_MarriageCeremony;
        //                IntVec3 intVec;
        //                if (lordToil_MarriageCeremony != null && !SpectatorCellFinder.TryFindSpectatorCellFor(p, lordToil_MarriageCeremony.Data.spectateRect, base.Map, out intVec, lordToil_MarriageCeremony.Data.spectateRectAllowedSides, 1, null))
        //                {
        //                    result = 0f;
        //                    return result;
        //                }
        //            }
        //            result = VoluntarilyJoinableLordJobJoinPriorities.MarriageCeremonyGuest;
        //        }
        //    }
        //    else
        //    {
        //        result = 0f;
        //    }
        //    return result;
        //}

        public override void ExposeData()
        {
            Scribe_References.Look(ref firstPawn, "firstPawn");
            Scribe_References.Look(ref secondPawn, "secondPawn");
            Scribe_Values.Look(ref spot, "spot");
        }

        public override string GetReport(Pawn pawn)
        {
            return "LordReportAttendingMarriageCeremony".Translate();
        }

        private bool IsCeremonyAboutToEnd()
        {
            return afterPartyTimeoutTrigger.TicksLeft < 1200;
        }

        private bool IsFiance(Pawn p)
        {
            return p == firstPawn || p == secondPawn;
        }

        private bool IsGuest(Pawn p)
        {
            return p.RaceProps.Humanlike && p != firstPawn && p != secondPawn &&
                   (p.Faction == firstPawn.Faction || p.Faction == secondPawn.Faction);
        }

        private void AddAttendedWeddingThoughts()
        {
            var ownedPawns = lord.ownedPawns;
            foreach (var pawn in ownedPawns)
            {
                if (pawn == firstPawn || pawn == secondPawn)
                {
                    continue;
                }

                if (firstPawn.Position.InHorDistOf(pawn.Position, 18f) ||
                    secondPawn.Position.InHorDistOf(pawn.Position, 18f))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AttendedWedding);
                }
            }
        }
    }
}
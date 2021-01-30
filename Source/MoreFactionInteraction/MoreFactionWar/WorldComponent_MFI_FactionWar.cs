﻿using RimWorld;
using Verse;
using RimWorld.Planet;
using System.Linq;
using MoreFactionInteraction.MoreFactionWar;
using System.Collections;
using System.Collections.Generic;

namespace MoreFactionInteraction
{
    public class WorldComponent_MFI_FactionWar : WorldComponent
    {
        private Faction factionOne;
        private Faction factionTwo;
        private bool    warIsOngoing;
        private bool    unrestIsBrewing;
        
        private int    factionOneBattlesWon = 1;
        private int    factionTwoBattlesWon = 1;

        private readonly List<Faction> allFactionsInVolvedInWar = new List<Faction>();

        public List<Faction> AllFactionsInVolvedInWar
        {
            get
            {
                if (allFactionsInVolvedInWar.Count == 0)
                {
                    allFactionsInVolvedInWar.Add(WarringFactionOne);
                    allFactionsInVolvedInWar.Add(WarringFactionTwo);
                }
                return allFactionsInVolvedInWar;
            }
        }

        public Faction WarringFactionOne => factionOne;

        public Faction WarringFactionTwo => factionTwo;

        public bool WarIsOngoing => warIsOngoing;
        public bool UnrestIsBrewing => unrestIsBrewing;
        public bool StuffIsGoingDown => unrestIsBrewing || warIsOngoing;

        private void SetFirstWarringFaction(Faction faction)
        {
            factionOne = faction;
        }

        private void SetSecondWarringFaction(Faction faction)
        {
            factionTwo = faction;
        }

        public WorldComponent_MFI_FactionWar(World world) : base (world: world)
        {
            this.world = world;
        }

        /// <summary>
        /// Starts the war and sets up stuff.
        /// </summary>
        /// <param name="factionOne"></param>
        /// <param name="factionInstigator"></param>
        /// <param name="selfResolved">Used in cases where we don't want to send standard letter. (i.e. DetermineWarAsIfNoPlayerInteraction)</param>
        public void StartWar(Faction factionOne, Faction factionInstigator, bool selfResolved)
        {
            warIsOngoing = true;
            unrestIsBrewing = false;
            SetFirstWarringFaction(factionOne);
            SetSecondWarringFaction(factionInstigator);
            factionOneBattlesWon = 1;
            factionTwoBattlesWon = 1;
            factionOne.TrySetRelationKind(factionInstigator, FactionRelationKind.Hostile, false);
            factionInstigator.TrySetRelationKind(factionOne, FactionRelationKind.Hostile, false);

            if (selfResolved)
            {
                return;
            }

            var peacetalks =
                (WorldObject) (Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == MFI_DefOf.MFI_FactionWarPeaceTalks) 
                            ?? GlobalTargetInfo.Invalid);

            Find.LetterStack.ReceiveLetter("MFI_FactionWarStarted".Translate(),
                                           "MFI_FactionWarExplanation".Translate(factionOne.Name, factionInstigator.Name),
                                           LetterDefOf.NegativeEvent, new GlobalTargetInfo(peacetalks));
        }

        public void StartUnrest(Faction factionOne, Faction factionTwo)
        {
            unrestIsBrewing = true;
            SetFirstWarringFaction(factionOne);
            SetSecondWarringFaction(factionTwo);
        }

        private void ResolveWar()
        {
            warIsOngoing = false;
            SetFirstWarringFaction(null);
            SetSecondWarringFaction(null);
            allFactionsInVolvedInWar.Clear();
            MainTabWindow_FactionWar.ResetBars();

            Find.LetterStack.ReceiveLetter("MFI_FactionWarOverLabel".Translate(), "MFI_FactionWarOver".Translate(WarringFactionOne, WarringFactionTwo), LetterDefOf.PositiveEvent);
        }

        public void AllOuttaFactionSettlements()
        {
            ResolveWar();
        }

        public float ScoreForFaction(Faction faction)
        {
            if (faction == factionOne)
            {
                return (float)factionOneBattlesWon / (factionOneBattlesWon + factionTwoBattlesWon);
            }

            if (faction == factionTwo)
            {
                return (float)factionTwoBattlesWon / (factionOneBattlesWon + factionTwoBattlesWon);
            }

            return 0f;
        }

        public void NotifyBattleWon(Faction faction)
        {
            if (faction == factionOne)
            {
                factionOneBattlesWon++;
            }

            if (faction == factionTwo)
            {
                factionTwoBattlesWon++;
            }

            if ((factionOneBattlesWon + factionTwoBattlesWon >= 12 && Rand.Chance(0.75f)) ||
                (factionOneBattlesWon + factionTwoBattlesWon >= 8 && Rand.Chance(0.25f)))
            {
                ResolveWar();
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref factionOne, "MFI_WarringFactionOne");
            Scribe_References.Look(ref factionTwo, "MFI_WarringFactionTwo");

            Scribe_Values.Look(ref warIsOngoing,    "MFI_warIsOngoing");
            Scribe_Values.Look(ref unrestIsBrewing, "MFI_UnrestIsBrewing");

            Scribe_Values.Look(ref factionOneBattlesWon, "MFI_factionOneScore", 1);
            Scribe_Values.Look(ref factionTwoBattlesWon, "MFI_factionTwoScore", 1);
        }

        public void DetermineWarAsIfNoPlayerInteraction(Faction faction, Faction factionInstigator)
        {
            unrestIsBrewing = false;

            DesiredOutcome rollForIntendedOutcome = Rand.Bool
                                                    ? DesiredOutcome.CURRY_FAVOUR_FACTION_ONE
                                                    : DesiredOutcome.BROKER_PEACE;

            if (faction.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
            {
                var dialogue = new FactionWarDialogue(faction.leader, faction, factionInstigator, null);
                dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

                Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                                               WarIsOngoing ? "MFI_FactionWarLeaderDecidedOnWar".Translate(faction, factionInstigator)
                                                            : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(faction, factionInstigator),
                                               WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
                return;
            }
            else if (factionInstigator.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
            {
                var dialogue = new FactionWarDialogue(factionInstigator.leader, factionInstigator, faction, null);
                dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

                Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                                               WarIsOngoing ? "MFI_FactionWarLeaderDecidedOnWar".Translate(factionInstigator, faction)
                                                            : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(factionInstigator, faction),
                                               WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
                return;
            }
        }
    }
}
using System.Collections.Generic;
using MoreFactionInteraction.MoreFactionWar;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class WorldComponent_MFI_FactionWar : WorldComponent
{
    private readonly List<Faction> allFactionsInVolvedInWar = [];
    private Faction factionOne;

    private int factionOneBattlesWon = 1;
    private Faction factionTwo;
    private int factionTwoBattlesWon = 1;
    private bool unrestIsBrewing;
    private bool warIsOngoing;

    public WorldComponent_MFI_FactionWar(World world) : base(world)
    {
        this.world = world;
    }

    public List<Faction> AllFactionsInVolvedInWar
    {
        get
        {
            if (allFactionsInVolvedInWar.Count != 0)
            {
                return allFactionsInVolvedInWar;
            }

            allFactionsInVolvedInWar.Add(WarringFactionOne);
            allFactionsInVolvedInWar.Add(WarringFactionTwo);

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

    /// <summary>
    ///     Starts the war and sets up stuff.
    /// </summary>
    /// <param name="one"></param>
    /// <param name="factionInstigator"></param>
    /// <param name="selfResolved">
    ///     Used in cases where we don't want to send standard letter. (i.e.
    ///     DetermineWarAsIfNoPlayerInteraction)
    /// </param>
    public void StartWar(Faction one, Faction factionInstigator, bool selfResolved)
    {
        warIsOngoing = true;
        unrestIsBrewing = false;
        SetFirstWarringFaction(one);
        SetSecondWarringFaction(factionInstigator);
        factionOneBattlesWon = 1;
        factionTwoBattlesWon = 1;
        one.SetRelationDirect(factionInstigator, FactionRelationKind.Hostile, false);
        factionInstigator.SetRelationDirect(one, FactionRelationKind.Hostile, false);

        if (selfResolved)
        {
            return;
        }

        var peacetalks =
            (WorldObject)(Find.WorldObjects.AllWorldObjects.FirstOrDefault(x =>
                              x.def == MFI_DefOf.MFI_FactionWarPeaceTalks)
                          ?? GlobalTargetInfo.Invalid);

        Find.LetterStack.ReceiveLetter("MFI_FactionWarStarted".Translate(),
            "MFI_FactionWarExplanation".Translate(one.Name, factionInstigator.Name),
            LetterDefOf.NegativeEvent, new GlobalTargetInfo(peacetalks));
    }

    public void StartUnrest(Faction one, Faction two)
    {
        unrestIsBrewing = true;
        SetFirstWarringFaction(one);
        SetSecondWarringFaction(two);
    }

    private void ResolveWar()
    {
        warIsOngoing = false;
        SetFirstWarringFaction(null);
        SetSecondWarringFaction(null);
        allFactionsInVolvedInWar.Clear();
        MainTabWindow_FactionWar.ResetBars();

        Find.LetterStack.ReceiveLetter("MFI_FactionWarOverLabel".Translate(),
            "MFI_FactionWarOver".Translate(WarringFactionOne, WarringFactionTwo), LetterDefOf.PositiveEvent);
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

        if (factionOneBattlesWon + factionTwoBattlesWon >= 12 && Rand.Chance(0.75f) ||
            factionOneBattlesWon + factionTwoBattlesWon >= 8 && Rand.Chance(0.25f))
        {
            ResolveWar();
        }
    }

    public override void ExposeData()
    {
        Scribe_References.Look(ref factionOne, "MFI_WarringFactionOne");
        Scribe_References.Look(ref factionTwo, "MFI_WarringFactionTwo");

        Scribe_Values.Look(ref warIsOngoing, "MFI_warIsOngoing");
        Scribe_Values.Look(ref unrestIsBrewing, "MFI_UnrestIsBrewing");

        Scribe_Values.Look(ref factionOneBattlesWon, "MFI_factionOneScore", 1);
        Scribe_Values.Look(ref factionTwoBattlesWon, "MFI_factionTwoScore", 1);
    }

    public void DetermineWarAsIfNoPlayerInteraction(Faction faction, Faction factionInstigator)
    {
        unrestIsBrewing = false;

        var rollForIntendedOutcome = Rand.Bool
            ? DesiredOutcome.CURRY_FAVOUR_FACTION_ONE
            : DesiredOutcome.BROKER_PEACE;

        if (faction.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
        {
            var dialogue = new FactionWarDialogue(faction.leader, faction, factionInstigator, null);
            dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

            Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                WarIsOngoing
                    ? "MFI_FactionWarLeaderDecidedOnWar".Translate(faction, factionInstigator)
                    : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(faction, factionInstigator),
                WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
        }
        else if (factionInstigator.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
        {
            var dialogue = new FactionWarDialogue(factionInstigator.leader, factionInstigator, faction, null);
            dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

            Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                WarIsOngoing
                    ? "MFI_FactionWarLeaderDecidedOnWar".Translate(factionInstigator, faction)
                    : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(factionInstigator, faction),
                WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
        }
    }
}
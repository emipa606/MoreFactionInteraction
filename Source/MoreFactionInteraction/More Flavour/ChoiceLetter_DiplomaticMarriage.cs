using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MoreFactionInteraction.MoreFactionWar;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class ChoiceLetter_DiplomaticMarriage : ChoiceLetter
{
    private readonly MethodInfo healIfPossibleMethodInfo =
        AccessTools.Method(typeof(PawnBanishUtility), "HealIfPossible");

    public Pawn betrothed;
    private int goodWillGainedFromMarriage;

    public Pawn marriageSeeker;

    public override bool CanShowInLetterStack => base.CanShowInLetterStack &&
                                                 PawnsFinder
                                                     .AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                                                     .Contains(value: betrothed);

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
                //possible outcomes: 
                //dowry
                //goodwill based on pawn value
                //wedding (doable)
                //bring us the betrothed? (complicated.)
                //betrothed picks a transport pod (meh)
                var accept = new DiaOption("Accept".Translate())
                {
                    action = () =>
                    {
                        goodWillGainedFromMarriage =
                            (int)MFI_DiplomacyTunings.PawnValueInGoodWillAmountOut.Evaluate(betrothed.MarketValue);
                        marriageSeeker.Faction.SetRelationDirect(Faction.OfPlayer,
                            (FactionRelationKind)Math.Min((int)marriageSeeker.Faction.PlayerRelationKind + 1, 2),
                            true, "LetterLabelAcceptedProposal".Translate());
                        marriageSeeker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodWillGainedFromMarriage,
                            false);
                        betrothed.relations.AddDirectRelation(PawnRelationDefOf.Fiance, marriageSeeker);

                        if (betrothed.GetCaravan() is { } caravan)
                        {
                            CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(betrothed,
                                caravan.PawnsListForReading);
                            healIfPossibleMethodInfo.Invoke(betrothed, null);
                            caravan.RemovePawn(betrothed);
                        }

                        DetermineAndDoOutcome(marriageSeeker, betrothed);
                        Find.LetterStack.RemoveLetter(this);
                    }
                };
                var dialogueNodeAccept = new DiaNode("MFI_AcceptedProposal"
                    .Translate(betrothed, marriageSeeker.Faction).CapitalizeFirst().AdjustedFor(marriageSeeker));
                dialogueNodeAccept.options.Add(Option_Close);
                accept.link = dialogueNodeAccept;

                var reject = new DiaOption("MFI_Reject".Translate())
                {
                    action = () =>
                    {
                        if (Rand.Chance(0.2f))
                        {
                            marriageSeeker.Faction.TryAffectGoodwillWith(Faction.OfPlayer,
                                MFI_DiplomacyTunings.GoodWill_DeclinedMarriage_Impact.RandomInRange);
                        }

                        Find.LetterStack.RemoveLetter(this);
                    }
                };
                var dialogueNodeReject = new DiaNode("MFI_DejectedProposal"
                    .Translate(marriageSeeker.LabelCap, marriageSeeker.Faction).CapitalizeFirst()
                    .AdjustedFor(marriageSeeker));
                dialogueNodeReject.options.Add(Option_Close);
                reject.link = dialogueNodeReject;

                yield return accept;
                yield return reject;
                yield return Option_Postpone;
            }
        }
    }

    private static void DetermineAndDoOutcome(Pawn marriageSeeker, Pawn betrothed)
    {
        if (Prefs.LogVerbose)
        {
            Log.Warning("Determine and do outcome after marriage.");
        }

        betrothed.SetFaction(!marriageSeeker.HostileTo(Faction.OfPlayer)
            ? marriageSeeker.Faction
            : null);

        Faction.OfPlayer.ideos?.RecalculateIdeosBasedOnPlayerPawns();

        //todo: maybe plan visit, deliver dowry, do wedding.
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref betrothed, "betrothed");
        Scribe_References.Look(ref marriageSeeker, "marriageSeeker");
    }
}
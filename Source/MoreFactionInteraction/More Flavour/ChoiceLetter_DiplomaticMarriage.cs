using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using MoreFactionInteraction.MoreFactionWar;
using System;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_DiplomaticMarriage : ChoiceLetter
    {
        private int goodWillGainedFromMarriage;
        public Pawn betrothed;
        public Pawn marriageSeeker;

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Contains(value: betrothed);

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
                    var accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            goodWillGainedFromMarriage = (int)MFI_DiplomacyTunings.PawnValueInGoodWillAmountOut.Evaluate(x: betrothed.MarketValue);
                            marriageSeeker.Faction.TrySetRelationKind(Faction.OfPlayer, (FactionRelationKind)Math.Min((int)marriageSeeker.Faction.PlayerRelationKind + 1, 2), true, "LetterLabelAcceptedProposal".Translate());
                            marriageSeeker.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: goodWillGainedFromMarriage, canSendMessage: false, canSendHostilityLetter: true, reason: "LetterLabelAcceptedProposal".Translate());
                            betrothed.relations.AddDirectRelation(def: PawnRelationDefOf.Fiance, otherPawn: marriageSeeker);

                                if (betrothed.GetCaravan() is Caravan caravan)
                                {
                                    CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(from: betrothed, candidates: caravan.PawnsListForReading);
                                    HealIfPossible(p: betrothed);
                                    caravan.RemovePawn(p: betrothed);
                                }
                            DetermineAndDoOutcome(marriageSeeker: marriageSeeker, betrothed: betrothed);
                            Find.LetterStack.RemoveLetter(this);
                        }
                    };
                    var dialogueNodeAccept = new DiaNode(text: "MFI_AcceptedProposal".Translate(betrothed, marriageSeeker.Faction).CapitalizeFirst().AdjustedFor(marriageSeeker));
                    dialogueNodeAccept.options.Add(item: Option_Close);
                    accept.link = dialogueNodeAccept;

                    var reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            if (Rand.Chance(0.2f))
                            {
                                marriageSeeker.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: MFI_DiplomacyTunings.GoodWill_DeclinedMarriage_Impact.RandomInRange, canSendMessage: true, canSendHostilityLetter: true, reason: "LetterLabelRejectedProposal".Translate());
                            }
                            Find.LetterStack.RemoveLetter(this);
                        }
                    };
                    var dialogueNodeReject = new DiaNode(text: "MFI_DejectedProposal".Translate(marriageSeeker.LabelCap, marriageSeeker.Faction).CapitalizeFirst().AdjustedFor(marriageSeeker));
                    dialogueNodeReject.options.Add(item: Option_Close);
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
                Log.Warning(text: "Determine and do outcome after marriage.");
            }

            betrothed.SetFaction(newFaction: !marriageSeeker.HostileTo(fac: Faction.OfPlayer)
                                                 ? marriageSeeker.Faction
                                                 : null);

            //todo: maybe plan visit, deliver dowry, do wedding.
        }

        private static void HealIfPossible(Pawn p)
        {
            var tmpHediffs = new List<Hediff>();
            tmpHediffs.AddRange(collection: p.health.hediffSet.hediffs);
            foreach (Hediff hediffTemp in tmpHediffs)
            {
                if (hediffTemp is Hediff_Injury hediffInjury && !hediffInjury.IsPermanent())
                {
                    p.health.RemoveHediff(hediff: hediffInjury);
                }
                else
                {
                    ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(def: hediffTemp.def);
                    if (immunityRecord != null)
                    {
                        immunityRecord.immunity = 1f;
                    }
                }
            }
            tmpHediffs.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(refee: ref betrothed, label: "betrothed");
            Scribe_References.Look<Pawn>(refee: ref marriageSeeker, label: "marriageSeeker");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class WorldObjectComp_SettlementBumperCropComp : WorldObjectComp
    {
        private const int basereward = 50;
        private const int workAmount = GenDate.TicksPerDay;
        private const float expGain = 6000f;
        private static readonly IntRange FactionRelationOffset = new IntRange(3, 8);

        private readonly Texture2D setPlantToGrowTex = HarmonyPatches.setPlantToGrowTex;
        public int expiration = -1;
        private int workLeft;
        private bool workStarted;

        public bool CaravanIsWorking => workStarted && Find.TickManager.TicksGame < workLeft;

        public bool ActiveRequest => expiration > Find.TickManager.TicksGame;

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (!ActiveRequest)
            {
                yield break;
            }

            var commandAction = new Command_Action
            {
                defaultLabel = "MFI_CommandHelpOutHarvesting".Translate(),
                defaultDesc = "MFI_CommandHelpOutHarvesting".Translate(),
                icon = setPlantToGrowTex,
                action = () =>
                {
                    {
                        if (!ActiveRequest)
                        {
                            Log.Error("Attempted to fulfill an unavailable request");
                            return;
                        }

                        if (BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.PlantHarvestYield) ==
                            null)
                        {
                            Messages.Message("MFI_MessageBumperCropNoGrower".Translate(), caravan,
                                MessageTypeDefOf.NegativeEvent);
                            return;
                        }

                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "MFI_CommandFulfillBumperCropHarvestConfirm".Translate(caravan.LabelCap),
                            () => NotifyCaravanArrived(caravan)));
                    }
                }
            };


            if (BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.PlantHarvestYield) == null)
            {
                commandAction.Disable("MFI_MessageBumperCropNoGrower".Translate());
            }

            yield return commandAction;
        }

        public override string CompInspectStringExtra()
        {
            if (ActiveRequest)
            {
                return "MFI_HarvestRequestInfo".Translate(
                    (expiration - Find.TickManager.TicksGame).ToStringTicksToDays());
            }

            return null;
        }

        public void Disable()
        {
            expiration = -1;
        }

        private void NotifyCaravanArrived(Caravan caravan)
        {
            workStarted = true;
            workLeft = Find.TickManager.TicksGame + workAmount;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().workWillBeDoneAtTick = workLeft;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = true;
            Disable();
        }

        public void DoOutcome(Caravan caravan)
        {
            workStarted = false;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = false;
            Outcome_Triumph(caravan);
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            var randomInRange = FactionRelationOffset.RandomInRange;
            parent.Faction?.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange);

            var allMembersCapableOfGrowing = AllCaravanMembersCapableOfGrowing(caravan);
            var totalYieldPowerForCaravan = CalculateYieldForCaravan(allMembersCapableOfGrowing);

            //TODO: Calculate a good amount
            //v1 (first vers): base of 20 * Sum of plant harvest yield * count * avg grow skill (20 * 2.96 * 3 * 14.5) = ~2579 or 20*5.96*6*20 = 14400
            //v2 (2018/08/03): base of 50 * avg of plant harvest yield * count * avg grow skill (50 * 0.99 * 3 * 14.5) = ~2153 or 40*0.99*6*20 = 4752 ((5940 for 50))
            //v3 (2018/12/18): base of 50 * avg of plant harvest yield * (count*0.75) * avg grow skill = (50 * 0.99 * (2.25) * 14.5 = ~1615 or (50 * 0.99 * (4.5) * 20 = 4455
            var totalreward = basereward * totalYieldPowerForCaravan * (allMembersCapableOfGrowing.Count * 0.75f)
                              * Mathf.Max(1,
                                  (float) allMembersCapableOfGrowing.Average(pawn =>
                                      pawn.skills.GetSkill(SkillDefOf.Plants).Level));

            var reward = ThingMaker.MakeThing(RandomRawFood());
            reward.stackCount = Mathf.RoundToInt(totalreward);
            CaravanInventoryUtility.GiveThing(caravan, reward);

            Find.LetterStack.ReceiveLetter("MFI_LetterLabelHarvest_Triumph".Translate(), GetLetterText(
                "MFI_Harvest_Triumph".Translate(
                    parent.Faction?.def.pawnsPlural, parent.Faction?.Name,
                    Mathf.RoundToInt(randomInRange),
                    reward.LabelCap
                ), caravan), LetterDefOf.PositiveEvent, caravan, parent.Faction);

            allMembersCapableOfGrowing.ForEach(pawn => pawn.skills.Learn(SkillDefOf.Plants, expGain, true));
        }

        //a long list of things to excluse stuff like milk and kibble. In retrospect, it may have been easier to get all plants and get their harvestables.
        private static ThingDef RandomRawFood()
        {
            return (from x in ThingSetMakerUtility.allGeneratableItems
                where x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.IsMeat
                      && !x.IsDrug && !x.HasComp(typeof(CompHatcher)) && !x.HasComp(typeof(CompIngredients))
                      && x.BaseMarketValue < 3 && (x.ingestible.preferability == FoodPreferability.RawBad ||
                                                   x.ingestible.preferability == FoodPreferability.RawTasty)
                select x).RandomElementWithFallback(ThingDefOf.RawPotatoes);
        }

        private static float CalculateYieldForCaravan(IEnumerable<Pawn> caravanMembersCapableOfGrowing)
        {
            return caravanMembersCapableOfGrowing.Select(x => x.GetStatValue(StatDefOf.PlantHarvestYield)).Average();
        }

        private static string GetLetterText(string baseText, Caravan caravan)
        {
            var text = new StringBuilder();
            text.Append(baseText).Append('\n');
            foreach (var pawn in AllCaravanMembersCapableOfGrowing(caravan))
            {
                text.Append('\n').Append("MFI_BumperCropXPGain".Translate(pawn.LabelShort, expGain));
            }

            return text.ToString();
        }

        private static List<Pawn> AllCaravanMembersCapableOfGrowing(Caravan caravan)
        {
            return caravan.PawnsListForReading.Where(pawn => !pawn.Dead && !pawn.Downed && !pawn.InMentalState
                                                             && caravan.IsOwner(pawn) &&
                                                             pawn.health.capacities.CanBeAwake
                                                             && !StatDefOf.PlantHarvestYield.Worker.IsDisabledFor(pawn))
                .ToList();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref expiration, "MFI_BumperCropExpiration");
            Scribe_Values.Look(ref workLeft, "MFI_BumperCropWorkLeft");
            Scribe_Values.Look(ref workStarted, "MFI_BumperCropWorkStarted");
        }
    }
}
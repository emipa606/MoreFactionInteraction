using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_CulturalSwap : EventRewardWorker
    {
        private static readonly float OVERPAYINGBY = 3f;
        private readonly EventDef eventDef = MFI_DefOf.MFI_CulturalSwap;

        public override Predicate<ThingDef> ValidatorFirstPlace =>
            x => /*x.stuffCategories.Contains(StuffCategoryDefOf.Metallic) &&*/
                (x.BaseMarketValue > 6 || x.smallVolume) && base.ValidatorFirstPlace(x);

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator,
            ThingSetMakerDef thingSetMakerDef)
        {
            var rewards = string.Empty;

            if (thingSetMakerDef != eventDef.rewardFirstLoser)
            {
                return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
            }

            foreach (var performer in caravan.PlayerPawnsForStoryteller)
            {
                if (performer.WorkTagIsDisabled(WorkTags.Artistic))
                {
                    continue;
                }

                pawn.skills.Learn(SkillDefOf.Artistic, eventDef.xPGainFirstLoser, true);
                TryAppendExpGainInfo(ref rewards, performer, SkillDefOf.Artistic, eventDef.xPGainFirstLoser);
            }

            return rewards + (Rand.Bool ? new TaggedString(string.Empty) :
                Rand.Bool ? new TaggedString("\n\n---\n\n")
                            + "MFI_AnnualExpoMedicalEmergency".Translate() :
                "\n\n---\n\n" + "MFI_AnnualExpoMedicalEmergencySerious".Translate());
        }

        public static DiaNode DialogueResolverArtOffer(string textResult, Thing broughtSculpture, Caravan caravan)
        {
            var marketValue = broughtSculpture.GetStatValue(StatDefOf.MarketValue);
            var resolver = new DiaNode(textResult.Translate(broughtSculpture, marketValue * OVERPAYINGBY, marketValue));
            var accept = new DiaOption("RansomDemand_Accept".Translate())
            {
                resolveTree = true,
                action = () =>
                {
                    broughtSculpture.Destroy();
                    var silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silver.stackCount = (int) (marketValue * OVERPAYINGBY);
                    CaravanInventoryUtility.GiveThing(caravan, silver);
                }
            };
            var reject = new DiaOption("RansomDemand_Reject".Translate())
            {
                resolveTree = true
            };
            resolver.options.Add(accept);
            resolver.options.Add(reject);
            return resolver;
        }
    }
}
using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_GameOfUrComp : EventRewardWorker
    {
        private readonly EventDef eventDef = MFI_DefOf.MFI_GameOfUrComp;

        public override Predicate<ThingDef> ValidatorFirstPlace => x => !x.isTechHediff && base.ValidatorFirstPlace(x);

        public override Predicate<ThingDef> ValidatorFirstLoser => x => !x.isTechHediff && base.ValidatorFirstLoser(x);

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator,
            ThingSetMakerDef thingSetMakerDef)
        {
            if (thingSetMakerDef != eventDef.rewardFirstOther)
            {
                return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
            }

            string rewards = "MFI_AnnualExpoMedicalEmergency".Translate();
            foreach (var brawler in caravan.PlayerPawnsForStoryteller)
            {
                if (brawler.WorkTagIsDisabled(WorkTags.Violent))
                {
                    continue;
                }

                brawler.skills.Learn(SkillDefOf.Melee, eventDef.xPGainFirstPlace, true);
                TryAppendExpGainInfo(ref rewards, brawler, SkillDefOf.Melee, eventDef.xPGainFirstPlace);
            }

            return rewards;
        }
    }
}
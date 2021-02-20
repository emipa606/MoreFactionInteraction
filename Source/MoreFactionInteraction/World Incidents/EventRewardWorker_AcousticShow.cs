using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_AcousticShow : EventRewardWorker
    {
        private readonly EventDef eventDef = MFI_DefOf.MFI_AcousticShow;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator,
            ThingSetMakerDef thingSetMakerDef)
        {
            if (thingSetMakerDef == eventDef.rewardFirstPlace)
            {
                GiveHappyThoughtsToCaravan(caravan, 20);
            }

            if (thingSetMakerDef == eventDef.rewardFirstLoser)
            {
                GiveHappyThoughtsToCaravan(caravan, 15);
            }

            if (thingSetMakerDef == eventDef.rewardFirstOther)
            {
                GiveHappyThoughtsToCaravan(caravan, 10);
            }

            return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
        }

        private static void GiveHappyThoughtsToCaravan(Caravan caravan, int amount)
        {
            foreach (var pawn in caravan.PlayerPawnsForStoryteller)
            {
                for (var i = 0; i < amount; i++)
                {
                    pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.AttendedParty);
                }
            }
        }
    }
}
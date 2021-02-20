using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class EventDef : Def
    {
        public List<SkillDef> learnedSkills;

        [MustTranslate] public string outcomeFirstLoser;

        [MustTranslate] public string outComeFirstOther;

        [MustTranslate] public string outComeFirstPlace;

        public StatDef relevantStat;
        public ThingSetMakerDef rewardFirstLoser;
        public ThingSetMakerDef rewardFirstOther;
        public ThingSetMakerDef rewardFirstPlace;

        [MustTranslate] public string theme;

        [MustTranslate] public string themeDesc;

        public Type workerClass = typeof(EventRewardWorker);

        [Unsaved] private EventRewardWorker workerInt;

        public float xPGainFirstLoser = 2000f;
        public float xPGainFirstOther = 1000f;

        public float xPGainFirstPlace = 4000f;

        //dunno why vanilla caches it but there's prolly good reason for it.
        public EventRewardWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (EventRewardWorker) Activator.CreateInstance(workerClass);
                }

                return workerInt;
            }
        }
    }
}
using System;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_ShootingComp : EventRewardWorker
    {
        public override Predicate<ThingDef> ValidatorFirstPlace => x => base.ValidatorFirstPlace(x)
                                                                        && x.techLevel >= TechLevel.Industrial
                                                                        && x.equipmentType == EquipmentType.Primary
                                                                        && x.GetStatValueAbstract(StatDefOf.MarketValue,
                                                                            GenStuff.DefaultStuffFor(x)) >= 100f;

        public override Predicate<ThingDef> ValidatorFirstLoser => x => base.ValidatorFirstLoser(x)
                                                                        && x.techLevel >=
                                                                        TechLevel
                                                                            .Spacer; //*bionics*, not wooden feet tyvm.

        public override Predicate<ThingDef> ValidatorFirstOther => x => base.ValidatorFirstOther(x)
                                                                        && x == ThingDefOf
                                                                            .RawPotatoes; //how nice, a representation of your shooting skills.
    }
}
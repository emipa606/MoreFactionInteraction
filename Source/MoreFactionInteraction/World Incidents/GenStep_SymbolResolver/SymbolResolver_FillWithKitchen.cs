using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    public class SymbolResolver_FillWithKitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var stoveElectric = DefDatabase<ThingDef>.GetNamedSilentFail("ElectricStove");
            var stoveFueled = DefDatabase<ThingDef>.GetNamedSilentFail("FueledStove");
            var tableButcher = DefDatabase<ThingDef>.GetNamedSilentFail("TableButcher");
            var spotButcher = DefDatabase<ThingDef>.GetNamedSilentFail("ButcherSpot");

            var map = BaseGen.globalSettings.map;
            ThingDef thingDef;
            if (rp.singleThingDef != null)
            {
                thingDef = rp.singleThingDef;
            }
            else if (rp.faction != null && rp.faction.def.techLevel >= TechLevel.Medieval)
            {
                thingDef = stoveElectric;
            }
            else
            {
                thingDef = Rand.Element(stoveFueled, ThingDefOf.Campfire, spotButcher);
            }

            var flipACoin = Rand.Bool;
            foreach (var potentialSpot in rp.rect)
            {
                if (flipACoin)
                {
                    if (potentialSpot.x % 3 != 0 || potentialSpot.z % 2 != 0)
                    {
                        continue;
                    }
                }
                else if (potentialSpot.x % 2 != 0 || potentialSpot.z % 3 != 0)
                {
                    continue;
                }

                var rot = !flipACoin ? Rot4.North : Rot4.West;
                if (GenSpawn.WouldWipeAnythingWith(potentialSpot, rot, thingDef, map,
                    x => x.def.category == ThingCategory.Building))
                {
                    continue;
                }

                var dontTouchMe = new IntVec2(thingDef.Size.x + 1, thingDef.Size.z + 1);
                if (BaseGenUtility.AnyDoorAdjacentCardinalTo(
                    GenAdj.OccupiedRect(potentialSpot, rot, dontTouchMe), map))
                {
                    continue;
                }

                var resolveParams = rp;
                resolveParams.rect = GenAdj.OccupiedRect(potentialSpot, rot, thingDef.Size);
                resolveParams.singleThingDef = Rand.Element(thingDef, tableButcher);
                resolveParams.thingRot = rot;
                var skipSingleThingIfHasToWipeBuildingOrDoesntFit = rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
                resolveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit =
                    !skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue ||
                    skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value;
                BaseGen.symbolStack.Push("thing", resolveParams);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class StockGenerator_BuyCategory : StockGenerator
{
    private const float maxValuePerUnit = 1000f;
    public ThingCategoryDef thingCategoryDef;

    public override bool HandlesThingDef(ThingDef thingDef)
    {
        //TODO: Look into maxTechLevelBuy. From what I can tell, nothing uses it.
        //TODO: Balance maxValuePerUnit. 1k is nonsense since traders generally don't have much more than that, but then again I also want some limit. Currently, ignores stuff, so golden helmets' ahoy.
        return thingCategoryDef.DescendantThingDefs.Contains(thingDef)
               && thingDef.tradeability != Tradeability.None
               && thingDef.BaseMarketValue / thingDef.VolumePerUnit < maxValuePerUnit;
    }

    public override IEnumerable<Thing> GenerateThings(PlanetTile planetTile, Faction faction = null)
    {
        yield break;
    }
}
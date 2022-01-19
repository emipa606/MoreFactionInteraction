using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents;

public class GenStep_HuntersLodge : GenStep
{
    private const int Size = 36;

    private static readonly List<CellRect> possibleRects = new List<CellRect>();

    public override int SeedPart => 735013949;

    public override void Generate(Map map, GenStepParams genStepParams)
    {
        if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect centralPoint))
        {
            centralPoint = CellRect.SingleCell(map.Center);
        }

        var faction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer
            ? Find.FactionManager.RandomEnemyFaction()
            : map.ParentFaction;
        ResolveParams resolveParams = default;
        resolveParams.rect = GetHuntersLodgeRect(centralPoint, map);
        resolveParams.faction = faction;

        ThingSetMakerParams maxFoodAndStuffForHuntersLodge = default;
        maxFoodAndStuffForHuntersLodge.totalMarketValueRange = new FloatRange(200, 500);
        maxFoodAndStuffForHuntersLodge.totalNutritionRange = new FloatRange(20, 50);

        //maxFoodAndStuffForHuntersLodge.filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);

        resolveParams.thingSetMakerParams = maxFoodAndStuffForHuntersLodge;

        BaseGen.globalSettings.map = map;
        BaseGen.globalSettings.minBuildings = 1;
        BaseGen.globalSettings.minBarracks = 1;
        BaseGen.symbolStack.Push("huntersLodgeBase", resolveParams);
        BaseGen.Generate();
    }

    private CellRect GetHuntersLodgeRect(CellRect centralPoint, Map map)
    {
        possibleRects.Add(new CellRect(centralPoint.minX - 1 - Size, centralPoint.CenterCell.z - 8, Size, Size));
        possibleRects.Add(new CellRect(centralPoint.maxX + 1, centralPoint.CenterCell.z - 8, Size, Size));
        possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.minZ - 1 - Size, Size, Size));
        possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.maxZ + 1, Size, Size));
        var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        if (possibleRects.Any())
        {
            return possibleRects.RandomElement();
        }

        return centralPoint;
    }
}
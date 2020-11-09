﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents
{
    public class GenStep_HuntersLodge : GenStep
    {
        private const int Size = 36;

        private static readonly List<CellRect> possibleRects = new List<CellRect>();

        public override int SeedPart => 735013949;

        public override void Generate(Map map, GenStepParams genStepParams)
        {
            if (!MapGenerator.TryGetVar<CellRect>(name: "RectOfInterest", var: out CellRect centralPoint))
            {
                centralPoint = CellRect.SingleCell(c: map.Center);
            }

            Faction faction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer
                                  ? Find.FactionManager.RandomEnemyFaction()
                                  : map.ParentFaction;
            ResolveParams resolveParams = default;
            resolveParams.rect = GetHuntersLodgeRect(centralPoint: centralPoint, map: map);
            resolveParams.faction = faction;

            ThingSetMakerParams maxFoodAndStuffForHuntersLodge = default;
            maxFoodAndStuffForHuntersLodge.totalMarketValueRange = new FloatRange(200, 500);
            maxFoodAndStuffForHuntersLodge.totalNutritionRange   = new FloatRange(20, 50);
            
            //maxFoodAndStuffForHuntersLodge.filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);

            resolveParams.thingSetMakerParams = maxFoodAndStuffForHuntersLodge;

            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push(symbol: "huntersLodgeBase", resolveParams: resolveParams);
            BaseGen.Generate();
        }

        private CellRect GetHuntersLodgeRect(CellRect centralPoint, Map map)
        {
            possibleRects.Add(item: new CellRect(minX: centralPoint.minX - 1 - Size, minZ: centralPoint.CenterCell.z - 8, width: Size, height: Size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.maxX + 1, minZ: centralPoint.CenterCell.z - 8, width: Size, height: Size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.CenterCell.x - 8, minZ: centralPoint.minZ - 1 - Size, width: Size, height: Size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.CenterCell.x - 8, minZ: centralPoint.maxZ + 1, width: Size, height: Size));
            var mapRect = new CellRect(minX: 0, minZ: 0, width: map.Size.x, height: map.Size.z);
            possibleRects.RemoveAll(match: (CellRect x) => !x.FullyContainedWithin(within: mapRect));
            if (possibleRects.Any<CellRect>())
            {
                return possibleRects.RandomElement<CellRect>();
            }
            return centralPoint;
        }
    }
}

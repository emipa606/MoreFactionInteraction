using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    //magic numbers. Magic numbers everywhere ;-;
    internal class MFI_SymbolResolver_BasePart_Outdoors_Division_Grid : SymbolResolver
    {
        private const int MinWidthOrHeight = 13;

        private const int MinRoomsPerRow = 2;

        private const int MaxRoomsPerRow = 4;

        private const int MaxPathwayWidth = 5;

        private const int MinRoomSize = 6;

        private const float AllowNonSquareRoomsInTheFirstStepChance = 0.2f;

        private static readonly List<Pair<Pair<int, int>, Pair<int, int>>> options =
            new List<Pair<Pair<int, int>, Pair<int, int>>>();

        private readonly List<Child> children =
            new List<Child>();

        private readonly List<Pair<int, int>> optionsX = new List<Pair<int, int>>();

        private readonly List<Pair<int, int>> optionsZ = new List<Pair<int, int>>();

        public override bool CanResolve(ResolveParams rp)
        {
            bool result;
            if (!base.CanResolve(rp))
            {
                result = false;
            }
            else if (rp.rect.Width < MinWidthOrHeight && rp.rect.Height < MinWidthOrHeight)
            {
                result = false;
            }
            else
            {
                FillOptions(rp.rect);
                result = optionsX.Any() && optionsZ.Any();
            }

            return result;
        }

        public override void Resolve(ResolveParams rp)
        {
            FillOptions(rp.rect);
            if (!Rand.Chance(AllowNonSquareRoomsInTheFirstStepChance))
            {
                if (TryResolveRandomOption(0, 0, rp))
                {
                    return;
                }

                if (TryResolveRandomOption(0, 1, rp))
                {
                    return;
                }
            }

            if (TryResolveRandomOption(1, 0, rp))
            {
                return;
            }

            if (TryResolveRandomOption(2, 0, rp))
            {
                return;
            }

            if (TryResolveRandomOption(2, 1, rp))
            {
                return;
            }

            if (!TryResolveRandomOption(999999, 999999, rp))
            {
                Log.Warning("Grid resolver could not resolve any grid size. params=" + rp);
            }
        }

        private void FillOptions(CellRect rect)
        {
            FillOptions(optionsX, rect.Width);
            FillOptions(optionsZ, rect.Height);
            if (optionsZ.Any(x => x.First > 1))
            {
                optionsX.RemoveAll(x =>
                    x.First >= 3 && GetRoomSize(x.First, x.Second, rect.Width) <= 7);
            }

            if (optionsX.Any(x => x.First > 1))
            {
                optionsZ.RemoveAll(x =>
                    x.First >= 3 && GetRoomSize(x.First, x.Second, rect.Height) <= 7);
            }
        }

        private void FillOptions(List<Pair<int, int>> outOptions, int length)
        {
            outOptions.Clear();
            for (var i = MinRoomsPerRow; i <= MaxRoomsPerRow; i++)
            {
                for (var j = 1; j <= MaxPathwayWidth; j++)
                {
                    var roomSize = GetRoomSize(i, j, length);
                    if (roomSize != -1 && roomSize >= MinRoomSize && roomSize >= (MinRoomsPerRow * j) - 1)
                    {
                        outOptions.Add(new Pair<int, int>(i, j));
                    }
                }
            }
        }

        private int GetRoomSize(int roomsPerRow, int pathwayWidth, int totalLength)
        {
            var num = totalLength - ((roomsPerRow - 1) * pathwayWidth);
            int result;
            if (num % roomsPerRow != 0)
            {
                result = -1;
            }
            else
            {
                result = num / roomsPerRow;
            }

            return result;
        }

        private bool TryResolveRandomOption(int maxWidthHeightDiff, int maxPathwayWidthDiff, ResolveParams rp)
        {
            options.Clear();
            foreach (var pair in optionsX)
            {
                var first = pair.First;
                var second = pair.Second;
                var roomSize = GetRoomSize(first, second, rp.rect.Width);
                foreach (var second1 in optionsZ)
                {
                    var first2 = second1.First;
                    var second2 = second1.Second;
                    var roomSize2 = GetRoomSize(first2, second2, rp.rect.Height);
                    if (Mathf.Abs(roomSize - roomSize2) <= maxWidthHeightDiff &&
                        Mathf.Abs(second - second2) <= maxPathwayWidthDiff)
                    {
                        options.Add(new Pair<Pair<int, int>, Pair<int, int>>(pair, second1));
                    }
                }
            }

            bool result;
            if (options.Any())
            {
                var pair = options
                    .RandomElement();
                ResolveOption(pair.First.First, pair.First.Second, pair.Second.First, pair.Second.Second, rp);
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        private void ResolveOption(int roomsPerRowX, int pathwayWidthX, int roomsPerRowZ, int pathwayWidthZ,
            ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            var roomSize = GetRoomSize(roomsPerRowX, pathwayWidthX, rp.rect.Width);
            var roomSize2 = GetRoomSize(roomsPerRowZ, pathwayWidthZ, rp.rect.Height);
            ThingDef thingDef = null;
            if (pathwayWidthX >= 3)
            {
                if (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Industrial)
                {
                    thingDef = ThingDefOf.StandingLamp;
                }
                else
                {
                    thingDef = ThingDefOf.TorchLamp;
                }
            }

            var floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
            var num = roomSize;
            for (var i = 0; i < roomsPerRowX - 1; i++)
            {
                var rect =
                    new CellRect(rp.rect.minX + num, rp.rect.minZ, pathwayWidthX, rp.rect.Height);
                var resolveParams = rp;
                resolveParams.rect = rect;
                resolveParams.floorDef = floorDef;
                resolveParams.streetHorizontal = false;
                BaseGen.symbolStack.Push("street", resolveParams);
                num += roomSize + pathwayWidthX;
            }

            var num2 = roomSize2;
            for (var j = 0; j < roomsPerRowZ - 1; j++)
            {
                var rect2 =
                    new CellRect(rp.rect.minX, rp.rect.minZ + num2, rp.rect.Width, pathwayWidthZ);
                var resolveParams2 = rp;
                resolveParams2.rect = rect2;
                resolveParams2.floorDef = floorDef;
                resolveParams2.streetHorizontal = true;
                BaseGen.symbolStack.Push("street", resolveParams2);
                num2 += roomSize2 + pathwayWidthZ;
            }

            num = 0;
            num2 = 0;
            children.Clear();
            for (var k = 0; k < roomsPerRowX; k++)
            {
                for (var l = 0; l < roomsPerRowZ; l++)
                {
                    var child =
                        new Child
                        {
                            rect = new CellRect(rp.rect.minX + num,
                                rp.rect.minZ +
                                num2,
                                roomSize,
                                roomSize2),
                            gridX = k,
                            gridY = l
                        };
                    children.Add(child);
                    num2 += roomSize2 + pathwayWidthZ;
                }

                num += roomSize + pathwayWidthX;
                num2 = 0;
            }

            MergeRandomChildren();
            children.Shuffle();
            foreach (var child in children)
            {
                if (thingDef != null)
                {
                    var c = new IntVec3(child.rect.maxX + 1, 0, child.rect.maxZ);
                    if (rp.rect.Contains(c) && c.Standable(map))
                    {
                        var resolveParams3 = rp;
                        resolveParams3.rect = CellRect.SingleCell(c);
                        resolveParams3.singleThingDef = thingDef;
                        BaseGen.symbolStack.Push("thing", resolveParams3);
                    }
                }

                var resolveParams4 = rp;
                resolveParams4.rect = child.rect;
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveParams4);
            }
        }

        private void MergeRandomChildren()
        {
            if (children.Count < 4)
            {
                return;
            }

            var num = GenMath.RoundRandom(children.Count / 6f);
            for (var i = 0; i < num; i++)
            {
                var child =
                    children.Find(x => !x.merged);
                if (child == null)
                {
                    break;
                }

                var child3 =
                    children.Find(x =>
                        x != child &&
                        (Mathf.Abs(x.gridX - child.gridX) == 1 && x.gridY == child.gridY ||
                         Mathf.Abs(x.gridY - child.gridY) == 1 && x.gridX == child.gridX));
                if (child3 == null)
                {
                    continue;
                }

                children.Remove(child);
                children.Remove(child3);
                var child2 =
                    new Child
                    {
                        gridX = Mathf.Min(child.gridX, child3.gridX),
                        gridY = Mathf.Min(child.gridY, child3.gridY),
                        merged = true,
                        rect = CellRect.FromLimits(Mathf.Min(child.rect.minX, child3.rect.minX),
                            Mathf.Min(child.rect.minZ, child3.rect.minZ),
                            Mathf.Max(child.rect.maxX, child3.rect.maxX),
                            Mathf.Max(child.rect.maxZ, child3.rect.maxZ))
                    };
                children.Add(child2);
            }
        }

        private class Child
        {
            public int gridX;

            public int gridY;

            public bool merged;
            public CellRect rect;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

internal class IncidentWorker_RoadWorks : IncidentWorker
{
    private const float maxRoadCoverage = 0.8f;
    private const float directConnectionChance = 0.7f;
    private Map map;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && TryGetRandomAvailableTargetMap(out var localMap)
                                         && CommsConsoleUtility.PlayerHasPoweredCommsConsole()
                                         && RandomNearbyTradeableSettlement(localMap.Tile) != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryGetRandomAvailableTargetMap(out map))
        {
            return false;
        }

        var Settlement = RandomNearbyTradeableSettlement(map.Tile);

        if (Settlement?.Faction == null)
        {
            return false;
        }

        var destination = Rand.Chance(directConnectionChance)
            ? map.Tile
            : AllyOfNearbySettlement(Settlement)?.Tile ?? map.Tile;

        var maxPriority = Settlement.Faction.def.techLevel >= TechLevel.Medieval ? 30 : 20;

        var roadToBuild = DefDatabase<RoadDef>.AllDefsListForReading.Where(x => x.priority <= maxPriority)
            .RandomElement();

        WorldPath path;

        var cost2 = 12000;
        var timeToBuild = 0;
        string letterTitle = "MFI_RoadWorks".Translate();
        var list = new List<WorldObject_RoadConstruction>();
        using (path = Find.WorldPathFinder.FindPath(destination, Settlement.Tile, null))
        {
            if (path == null || path == WorldPath.NotFound)
            {
                return true;
            }

            float roadCount = path.NodesReversed.Count(x => !Find.WorldGrid[x].Roads.NullOrEmpty()
                                                            && Find.WorldGrid[x].Roads.Any(roadLink =>
                                                                roadLink.road.priority >= roadToBuild.priority)
                                                            || Find.WorldObjects.AnyWorldObjectOfDefAt(
                                                                MFI_DefOf.MFI_RoadUnderConstruction, x));

            if (roadCount / path.NodesReversed.Count >= maxRoadCoverage)
            {
                return false;
            }

            //not 0 and - 1
            for (var i = 1; i < path.NodesReversed.Count - 1; i++)
            {
                cost2 += Caravan_PathFollower.CostToMove(CaravanTicksPerMoveUtility.DefaultTicksPerMove,
                    path.NodesReversed[i], path.NodesReversed[i + 1]);

                timeToBuild += (int)(2 * GenDate.TicksPerDay
                                       * WorldPathGrid.CalculatedMovementDifficultyAt(path.NodesReversed[i], true)
                                       * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(i, i + 1));

                if (!Find.WorldGrid[path.NodesReversed[i]].Roads.NullOrEmpty()
                    && Find.WorldGrid[path.NodesReversed[i]].Roads
                        .Any(roadLink => roadLink.road.priority >= roadToBuild.priority))
                {
                    timeToBuild /= 2;
                }

                var roadConstruction =
                    (WorldObject_RoadConstruction)WorldObjectMaker.MakeWorldObject(MFI_DefOf
                        .MFI_RoadUnderConstruction);
                roadConstruction.Tile = path.NodesReversed[i];
                roadConstruction.nextTile = path.NodesReversed[i + 1];
                roadConstruction.road = roadToBuild;
                roadConstruction.SetFaction(Settlement.Faction);
                roadConstruction.projectedTimeOfCompletion = Find.TickManager.TicksGame + timeToBuild;
                list.Add(roadConstruction);
            }

            cost2 /= 10;
            var node = new DiaNode("MFI_RoadWorksDialogue".Translate(Settlement, path.NodesReversed.Count, cost2));
            // {Settlement} wants {cost2 / 10} to build a road of {path.NodesReversed.Count}");
            var accept = new DiaOption("OK".Translate())
            {
                resolveTree = true,
                action = () =>
                {
                    TradeUtility.LaunchSilver(TradeUtility.PlayerHomeMapWithMostLaunchableSilver(), cost2);
                    foreach (var worldObjectRoadConstruction in list)
                    {
                        Find.WorldObjects.Add(worldObjectRoadConstruction);
                    }

                    list.Clear();
                }
            };

            if (!TradeUtility.ColonyHasEnoughSilver(TradeUtility.PlayerHomeMapWithMostLaunchableSilver(), cost2))
            {
                accept.Disable("NeedSilverLaunchable".Translate(cost2));
            }

            var reject = new DiaOption("RejectLetter".Translate())
            {
                resolveTree = true,
                action = () =>
                {
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        list[i] = null;
                    }

                    list.Clear();
                }
            };

            node.options.Add(accept);
            node.options.Add(reject);

            //Log.Message(stringBuilder.ToString());
            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(node, Settlement.Faction));
            Find.Archive.Add(new ArchivedDialog(node.text, letterTitle, Settlement.Faction));
        }

        return true;
    }

    private Settlement RandomNearbyTradeableSettlement(int originTile)
    {
        return (from settlement in Find.WorldObjects.Settlements
            where settlement.Visitable && settlement.Faction?.leader != null
                                       && settlement.trader is { CanTradeNow: true }
                                       && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                                       && Find.WorldReachability.CanReach(originTile, settlement.Tile)
            select settlement).RandomElementWithFallback();
    }

    private Settlement AllyOfNearbySettlement(Settlement origin)
    {
        return (from settlement in Find.WorldObjects.Settlements
            where settlement.Tile != origin.Tile
                  && (settlement.Faction == origin.Faction || settlement.Faction?.GoodwillWith(origin.Faction) >= 0)
                  && settlement.trader.CanTradeNow
                  && Find.WorldGrid.ApproxDistanceInTiles(origin.Tile, settlement.Tile) < 20f
                  && Find.WorldReachability.CanReach(origin.Tile, settlement.Tile)
            select settlement).RandomElement();
    }

    private bool TryGetRandomAvailableTargetMap(out Map localMap)
    {
        return Find.Maps.Where(m => m.IsPlayerHome).TryRandomElement(out localMap);
    }
}
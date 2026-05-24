using System.Linq;
using MoreFactionInteraction.More_Flavour;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class IncidentWorker_MysticalShaman : IncidentWorker
{
    private const int MinDistance = 8;
    private const int MaxDistance = 22;
    private static readonly IntRange TimeoutDaysRange = new(5, 15);

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (parms.forced)
        {
            return true;
        }

        return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null
                                         && !Find.WorldObjects.AllWorldObjects.Any(o =>
                                             o.def == MFI_DefOf.MFI_MysticalShaman)
                                         && Find.FactionManager.AllFactionsVisible.Where(f =>
                                             f.def.techLevel <= TechLevel.Neolithic
                                             && !f.HostileTo(Faction.OfPlayer) && !f.temporary).TryRandomElement(out _)
                                         && TryFindTile(out _, false)
                                         && TryGetRandomAvailableTargetMap(out _, false)
                                         && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!Find.FactionManager.AllFactionsVisible.Where(f => f.def.techLevel <= TechLevel.Neolithic
                                                               && !f.HostileTo(Faction.OfPlayer) && !f.temporary)
                .TryRandomElement(out var faction))
        {
            return false;
        }

        if (faction == null)
        {
            return false;
        }

        if (!TryGetRandomAvailableTargetMap(out var map, parms.forced))
        {
            return false;
        }

        if (map == null)
        {
            return false;
        }

        if (!TryFindTile(out var tile, parms.forced))
        {
            return false;
        }

        var fee = Rand.RangeInclusive(400, 1000);

        var diaNode = new DiaNode("MFI_MysticalShamanLetter".Translate(faction.Name, fee.ToString()));
        var accept = new DiaOption("Accept".Translate())
        {
            action = () =>
            {
                var mysticalShaman =
                    (MysticalShaman)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_MysticalShaman);
                mysticalShaman.Tile = tile;
                mysticalShaman.SetFaction(faction);
                var randomInRange = TimeoutDaysRange.RandomInRange;
                mysticalShaman.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
                Find.WorldObjects.Add(mysticalShaman);
                TradeUtility.LaunchSilver(map, fee);
            },
            resolveTree = true
        };
        if (!TradeUtility.ColonyHasEnoughSilver(map, fee))
        {
            accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
        }

        var reject = new DiaOption("MFI_Reject".Translate())
        {
            action = () => { },
            resolveTree = true
        };
        diaNode.options = [accept, reject];

        Find.WindowStack.Add(new Dialog_NodeTree(diaNode, title: def.letterLabel));
        Find.Archive.Add(new ArchivedDialog(diaNode.text, def.letterLabel));

        return true;
    }

    private static bool TryFindTile(out PlanetTile tile, bool force)
    {
        if (force)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 1, MaxDistance, true, canBeSpace: true);
        }

        return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true,
            validator: candidate => candidate.LayerDef.SurfaceTiles && !Find.WorldGrid[candidate].WaterCovered);
    }

    private bool TryGetRandomAvailableTargetMap(out Map map, bool force)
    {
        if (force)
        {
            return Find.Maps.Where(target => target.IsPlayerHome).TryRandomElement(out map)
                   || Find.Maps.Where(target => target.mapPawns.FreeColonistsSpawnedCount > 0).TryRandomElement(out map);
        }

        return Find.Maps
            .Where(target => target.IsPlayerHome
                             && target.Tile.LayerDef.SurfaceTiles
                             && RandomNearbyTradeableSettlement(target.Tile) != null)
            .TryRandomElement(out map);
    }

    private Settlement RandomNearbyTradeableSettlement(PlanetTile tile)
    {
        return Find.WorldObjects.SettlementBases.Where(settlement => settlement.Visitable
                                                                     && settlement.Tile.LayerDef.SurfaceTiles
                                                                     && settlement
                                                                         .GetComponent<TradeRequestComp>() != null
                                                                     && !settlement.GetComponent<TradeRequestComp>()
                                                                         .ActiveRequest
                                                                     && Find.WorldGrid.ApproxDistanceInTiles(tile,
                                                                         settlement.Tile) < MaxDistance &&
                                                                     Find.WorldReachability.CanReach(tile,
                                                                         settlement.Tile)
        ).RandomElementWithFallback();
    }
}
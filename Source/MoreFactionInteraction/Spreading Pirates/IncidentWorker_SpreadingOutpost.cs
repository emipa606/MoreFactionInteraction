using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

[UsedImplicitly]
public class IncidentWorker_SpreadingOutpost : IncidentWorker
{
    private readonly int maxDist = 30;
    private readonly int maxSites = 20;
    private readonly int minDist = 8;
    private Faction faction;

    public override float BaseChanceThisGame =>
        base.BaseChanceThisGame * MoreFactionInteraction_Settings.pirateBaseUpgraderModifier;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (parms.forced)
        {
            return true;
        }

        return base.CanFireNowSub(parms) && TryFindFaction(out faction)
                                         && TileFinder.TryFindNewSiteTile(out _, minDist, maxDist,
                                             validator: candidate => candidate.LayerDef.SurfaceTiles &&
                                                                     !Find.WorldGrid[candidate].WaterCovered)
                                         && TryGetRandomAvailableTargetMap(out _, false)
                                         && Find.World.worldObjects.Sites.Count <= maxSites;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryFindFaction(out faction))
        {
            return false;
        }

        if (!TryGetRandomAvailableTargetMap(out var map, parms.forced))
        {
            return false;
        }

        if (faction.leader == null)
        {
            return false;
        }

        var pirateTile = RandomNearbyHostileSettlement(map.Tile, parms.forced)?.Tile ?? PlanetTile.Invalid;

        if (pirateTile == PlanetTile.Invalid)
        {
            return false;
        }

        if (!TryFindOutpostTile(out var tile, parms.forced))
        {
            return false;
        }

        var site = SiteMaker.MakeSite(DefDatabase<SitePartDef>.GetNamedSilentFail("Outpost"), tile, faction);
        site.Tile = tile;
        site.sitePartsKnown = true;
        Find.WorldObjects.Add(site);
        SendStandardLetter(parms, site, faction.leader?.LabelShort ?? "MFI_Representative".Translate(),
            faction.def.leaderTitle, faction.Name);
        return true;
    }

    private Settlement RandomNearbyHostileSettlement(int originTile, bool force)
    {
        return Find.WorldObjects.Settlements
            .Where(settlement => settlement.Attackable
                                 && (force || settlement.Tile.LayerDef.SurfaceTiles)
                                 && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                                 && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                                 && settlement.Faction == faction)
            .RandomElementWithFallback();
    }

    private static bool TryFindOutpostTile(out PlanetTile tile, bool force)
    {
        if (force)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 1, 8, true, canBeSpace: true);
        }

        return TileFinder.TryFindNewSiteTile(out tile, 2, 8,
            validator: candidate => candidate.LayerDef.SurfaceTiles && !Find.WorldGrid[candidate].WaterCovered);
    }

    private static bool TryFindFaction(out Faction enemyFaction)
    {
        return Find.FactionManager.AllFactions
            .Where(x => !x.def.hidden && !x.defeated && x.HostileTo(Faction.OfPlayer) && x.def.permanentEnemy &&
                        !x.temporary)
            .TryRandomElement(out enemyFaction);
    }

    private bool TryGetRandomAvailableTargetMap(out Map map, bool force)
    {
        if (force)
        {
            return Find.Maps.Where(x => x.IsPlayerHome).TryRandomElement(out map)
                   || Find.Maps.Where(x => x.mapPawns.FreeColonistsSpawnedCount > 0).TryRandomElement(out map);
        }

        return Find.Maps
            .Where(x => x.IsPlayerHome
                        && x.Tile.LayerDef.SurfaceTiles
                        && RandomNearbyHostileSettlement(x.Tile, false) != null)
            .TryRandomElement(out map);
    }
}
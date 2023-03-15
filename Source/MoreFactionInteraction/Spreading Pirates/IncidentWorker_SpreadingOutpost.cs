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

    public override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && TryFindFaction(out faction)
                                         && TileFinder.TryFindNewSiteTile(out _, minDist, maxDist)
                                         && TryGetRandomAvailableTargetMap(out _)
                                         && Find.World.worldObjects.Sites.Count <= maxSites;
    }

    public override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryFindFaction(out faction))
        {
            return false;
        }

        if (!TryGetRandomAvailableTargetMap(out var map))
        {
            return false;
        }

        if (faction.leader == null)
        {
            return false;
        }

        var pirateTile = RandomNearbyHostileSettlement(map.Tile)?.Tile ?? Tile.Invalid;

        if (pirateTile == Tile.Invalid)
        {
            return false;
        }

        if (!TileFinder.TryFindNewSiteTile(out var tile, 2, 8, false, TileFinderMode.Near, pirateTile))
        {
            return false;
        }

        var site = SiteMaker.MakeSite(SitePartDefOf.Outpost, tile, faction);
        site.Tile = tile;
        site.sitePartsKnown = true;
        Find.WorldObjects.Add(site);
        SendStandardLetter(parms, site, faction.leader?.LabelShort ?? "MFI_Representative".Translate(),
            faction.def.leaderTitle, faction.Name);
        return true;
    }

    private Settlement RandomNearbyHostileSettlement(int originTile)
    {
        return Find.WorldObjects.Settlements
            .Where(settlement => settlement.Attackable
                                 && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                                 && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                                 && settlement.Faction == faction)
            .RandomElementWithFallback();
    }

    private static bool TryFindFaction(out Faction enemyFaction)
    {
        return Find.FactionManager.AllFactions
            .Where(x => !x.def.hidden && !x.defeated && x.HostileTo(Faction.OfPlayer) && x.def.permanentEnemy &&
                        !x.temporary)
            .TryRandomElement(out enemyFaction);
    }

    private bool TryGetRandomAvailableTargetMap(out Map map)
    {
        return Find.Maps
            .Where(x => x.IsPlayerHome && RandomNearbyHostileSettlement(x.Tile) != null)
            .TryRandomElement(out map);
    }
}
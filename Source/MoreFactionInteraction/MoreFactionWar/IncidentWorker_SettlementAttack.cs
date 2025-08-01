using System.Linq;
using System.Text;
using MoreFactionInteraction.General;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

public class IncidentWorker_SettlementAttack : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing;
    }

    //warning: logic ahead.
    //GOAL:
    //1. find random player tile
    //2. find warring faction settlement near it.
    //3. find closest allied (for now: faction = faction) near it
    //4. find the closest enemy faction near it.
    //5. If enemy is closer than ally, it's a win for enemy. 
    //6? If enemy is twice as close, base in question becomes enemy base? maybe.
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TileFinder.TryFindRandomPlayerTile(out var randomPlayerTile, true, x => FindTile(x) != -1))
        {
            return false;
        }

        var someRandomPreferablyNearbySettlement =
            RandomPreferablyNearbySettlementOfFactionInvolvedInWar(randomPlayerTile);

        if (someRandomPreferablyNearbySettlement?.Faction == null)
        {
            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().AllOuttaFactionSettlements();
            return false;
        }

        TileFinder.TryFindPassableTileWithTraversalDistance(someRandomPreferablyNearbySettlement.Tile,
            0, 66,
            out var tileContainingAlly,
            HasNearbyAlliedFaction);

        TileFinder.TryFindPassableTileWithTraversalDistance(someRandomPreferablyNearbySettlement.Tile,
            0, 66,
            out var tileContainingEnemy,
            HasNearbyEnemyFaction);

        Faction winner = null;
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("MFI_FactionWarBaseAttacked".Translate(someRandomPreferablyNearbySettlement.Faction,
            someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar()));
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        if (tileContainingEnemy == -1)
        {
            winner = someRandomPreferablyNearbySettlement.Faction;
            stringBuilder.Append("MFI_FactionWarBaseSuccessfullyDefended".Translate(
                someRandomPreferablyNearbySettlement.Faction,
                someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar()));
        }

        //winner is whoever is faster in sending in reinforcements.
        if (tileContainingAlly != -1 && tileContainingEnemy != -1)
        {
            winner =
                CalcuteTravelTimeForReinforcements(someRandomPreferablyNearbySettlement.Tile, tileContainingAlly) <
                CalcuteTravelTimeForReinforcements(someRandomPreferablyNearbySettlement.Tile, tileContainingEnemy)
                    ? someRandomPreferablyNearbySettlement.Faction
                    : someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar();

            string flavourText = winner == someRandomPreferablyNearbySettlement.Faction
                ? "MFI_FactionWarBaseSuccessfullyDefended".Translate(someRandomPreferablyNearbySettlement.Faction,
                    someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar())
                : "MFI_FactionWarBaseDefeated".Translate(someRandomPreferablyNearbySettlement.Faction,
                    someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());

            stringBuilder.Append(flavourText);
        }

        Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(winner);

        if (Rand.Bool && winner != someRandomPreferablyNearbySettlement.Faction)
        {
            stringBuilder.Append(" ");
            stringBuilder.Append(DestroyOldOutpostAndCreateNewAtSpot(someRandomPreferablyNearbySettlement));
        }

        Find.LetterStack.ReceiveLetter("MFI_FactionWarBaseBattleTookPlaceLabel".Translate(),
            stringBuilder.ToString(), LetterDefOf.NeutralEvent,
            new GlobalTargetInfo(someRandomPreferablyNearbySettlement.Tile),
            someRandomPreferablyNearbySettlement.Faction);

        return true;

        static int FindTile(int root)
        {
            if (TileFinder.TryFindPassableTileWithTraversalDistance(root, 7, 66, out var num))
            {
                return num;
            }

            return -1;
        }

        bool HasNearbyAlliedFaction(PlanetTile x)
        {
            return Find.WorldObjects.AnySettlementAt(x) &&
                   Find.WorldObjects.SettlementAt(x).Faction ==
                   someRandomPreferablyNearbySettlement.Faction;
        }

        bool HasNearbyEnemyFaction(PlanetTile x)
        {
            return Find.WorldObjects.AnySettlementAt(x) &&
                   Find.WorldObjects.SettlementAt(x).Faction ==
                   someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar();
        }
    }

    private static string DestroyOldOutpostAndCreateNewAtSpot(Settlement someRandomPreferablyNearbySettlement)
    {
        if (Rand.ChanceSeeded(0.5f, someRandomPreferablyNearbySettlement.ID))
        {
            var factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            factionBase.SetFaction(someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());
            factionBase.Tile = someRandomPreferablyNearbySettlement.Tile;
            factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
            Find.WorldObjects.Remove(someRandomPreferablyNearbySettlement);
            Find.WorldObjects.Add(factionBase);
            return "MFI_FactionWarBaseTakenOver".Translate(someRandomPreferablyNearbySettlement.Faction,
                someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());
        }

        var destroyedSettlement =
            (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
        destroyedSettlement.Tile = someRandomPreferablyNearbySettlement.Tile;
        var loserFaction = someRandomPreferablyNearbySettlement.Faction;
        Find.WorldObjects.Add(destroyedSettlement);
        Find.WorldObjects.Remove(someRandomPreferablyNearbySettlement);
        return "MFI_FactionWarBaseDestroyed".Translate(loserFaction, loserFaction.EnemyInFactionWar());
    }

    private static Settlement RandomPreferablyNearbySettlementOfFactionInvolvedInWar(int originTile)
    {
        var factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
        var factionTwo = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

        return (from settlement in Find.WorldObjects.Settlements
                where (settlement.Faction == factionOne || settlement.Faction == factionTwo) &&
                      Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 66f
                select settlement)
            .RandomElementWithFallback(
                RandomSettlementOfFactionInvolvedInWarThatCanBeABitFurtherAwayIDontParticularlyCare());
    }

    private static Settlement RandomSettlementOfFactionInvolvedInWarThatCanBeABitFurtherAwayIDontParticularlyCare()
    {
        var factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
        var factionTwo = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

        return (from settlement in Find.WorldObjects.Settlements
            where settlement.Faction == factionOne || settlement.Faction == factionTwo
            select settlement).RandomElementWithFallback();
    }

    private int CalcuteTravelTimeForReinforcements(int originTile, int destinationTile)
    {
        return CaravanArrivalTimeEstimator.EstimatedTicksToArrive(originTile, destinationTile, null);
    }
}
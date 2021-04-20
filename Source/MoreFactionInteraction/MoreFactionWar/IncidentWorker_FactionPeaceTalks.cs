using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class IncidentWorker_FactionPeaceTalks : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(21, 23);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && FoundTwoFactions()
                                             && TryFindTile(out _)
                                             && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
                                             && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>()
                                                 .UnrestIsBrewing;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!FoundTwoFactions())
            {
                return false;
            }

            if (!TryFindTile(out var tile))
            {
                return false;
            }

            var faction = TryFindFactions(out var instigatingFaction);

            if (faction == null)
            {
                return false;
            }

            var factionWarPeaceTalks =
                (FactionWarPeaceTalks) WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_FactionWarPeaceTalks);
            factionWarPeaceTalks.Tile = tile;
            factionWarPeaceTalks.SetFaction(faction);
            factionWarPeaceTalks.SetWarringFactions(faction, instigatingFaction);
            var randomInRange = TimeoutDaysRange.RandomInRange;
            factionWarPeaceTalks.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
            Find.WorldObjects.Add(factionWarPeaceTalks);

            var text = string.Format(def.letterText.AdjustedFor(faction.leader), faction.def.leaderTitle, faction.Name,
                instigatingFaction.Name, randomInRange).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(def.letterLabel, text, def.letterDef, factionWarPeaceTalks, faction);
            Find.World.GetComponent<WorldComponent_MFI_FactionWar>()
                .StartUnrest(faction, instigatingFaction);

            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 5, 13, false, false);
        }

        private static bool FoundTwoFactions()
        {
            return TryFindFactions(out _) != null;
        }

        private static Faction TryFindFactions(out Faction instigatingFaction)
        {
            var factions = Find.FactionManager.AllFactions
                .Where(x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy && x.leader != null);

            var alliedFaction = factions.RandomElement();

            var factionsPartTwo = Find.FactionManager.AllFactions
                .Where(x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy &&
                            x != alliedFaction && x.leader != null);

            return factionsPartTwo.TryRandomElement(out instigatingFaction) ? alliedFaction : null;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class IncidentWorker_FactionPeaceTalks : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 21, max: 23);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && FoundTwoFactions() 
                                                    && TryFindTile(tile: out _)                                        
                                                    && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing 
                                                    && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>().UnrestIsBrewing;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!FoundTwoFactions())
            {
                return false;
            }

            if (!TryFindTile(tile: out var tile))
            {
                return false;
            }

            Faction faction = TryFindFactions(instigatingFaction: out Faction instigatingFaction);

            if (faction == null)
            {
                return false;
            }

            var factionWarPeaceTalks = (FactionWarPeaceTalks)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_FactionWarPeaceTalks);
            factionWarPeaceTalks.Tile = tile;
            factionWarPeaceTalks.SetFaction(newFaction: faction);
            factionWarPeaceTalks.SetWarringFactions(factionOne: faction, factionInstigator: instigatingFaction);
            var randomInRange = TimeoutDaysRange.RandomInRange;
            factionWarPeaceTalks.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
            Find.WorldObjects.Add(o: factionWarPeaceTalks);

            var text = string.Format(format: def.letterText.AdjustedFor(p: faction.leader), faction.def.leaderTitle, faction.Name, instigatingFaction.Name, randomInRange).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(label: def.letterLabel, text: text, textLetterDef: def.letterDef, lookTargets: factionWarPeaceTalks, relatedFaction: faction);
            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartUnrest(factionOne: faction, factionTwo: instigatingFaction);

            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: 5, maxDist: 13, allowCaravans: false, preferCloserTiles: false);
        }

        private static bool FoundTwoFactions()
        {
            return TryFindFactions(instigatingFaction: out _) != null;
        }

        private static Faction TryFindFactions(out Faction instigatingFaction)
        {
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions
                                                .Where(predicate: x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy && x.leader != null);

            Faction alliedFaction = factions.RandomElement();

            IEnumerable<Faction> factionsPartTwo = Find.FactionManager.AllFactions
                                                       .Where(predicate: x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy && x != alliedFaction && x.leader != null);

            return factionsPartTwo.TryRandomElement(result: out instigatingFaction) ? alliedFaction : null;
        }
    }
}

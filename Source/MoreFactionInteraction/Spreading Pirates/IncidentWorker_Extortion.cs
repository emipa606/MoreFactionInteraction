using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class IncidentWorker_Extortion : IncidentWorker
    {
        private const int TimeoutTicks = GenDate.TicksPerDay;

        private Faction faction;
        private WorldObject worldObject;

        public override float BaseChanceThisGame
        {
            get
            {
                var modifier = (float)NearbyHostileEncampments().Count() / 10;
                return (def.baseChance * 1) + modifier;
            }
        }

        private static bool RandomNearbyHostileWorldObject(int originTile, out WorldObject encampment, out Faction faction)
        {
            encampment = NearbyHostileEncampments(forTile: originTile).RandomElementWithFallback();

            faction = encampment?.Faction;
            return faction != null;
        }

        private static IEnumerable<WorldObject> NearbyHostileEncampments(int forTile = -1)
        {
            if (Find.AnyPlayerHomeMap != null)
            {
                forTile = Find.AnyPlayerHomeMap.Tile;
            }
            else if (Find.CurrentMap != null)
            {
                forTile = Find.CurrentMap.Tile;
            }

            return from worldObject in Find.WorldObjects.AllWorldObjects
                   where (worldObject is Settlement || worldObject is Site)
                           && worldObject.Faction != null
                           && worldObject.Faction.HostileTo(other: Faction.OfPlayer)
                           && (!worldObject.GetComponent<TimeoutComp>()?.Active ?? true)
                           && worldObject.Faction.def.permanentEnemy
                           && Find.WorldGrid.ApproxDistanceInTiles(firstTile: forTile, secondTile: worldObject.Tile) < 20f
                           && (Find.WorldReachability.CanReach(startTile: forTile, destTile: worldObject.Tile) || forTile == -1)
                   select worldObject;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;
            return base.CanFireNowSub(parms: parms) && CommsConsoleUtility.PlayerHasPoweredCommsConsole(map: map)
                                                    && RandomNearbyHostileWorldObject(originTile: parms.target.Tile, encampment: out worldObject, faction: out faction);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;

            if (RandomNearbyHostileWorldObject(originTile: map.Tile, encampment: out worldObject, faction: out faction))
            {
                var extorsionDemand = Math.Max(val1: Rand.Range(min: 150, max: 300), val2: (int)parms.points) * NearbyHostileEncampments(forTile: map.Tile).Count();

                Pawn representative = faction.leader ?? ((worldObject is Settlement baese) ? baese.previouslyGeneratedInhabitants.FirstOrDefault() : null);

                var choiceLetterExtortionDemand = (ChoiceLetter_ExtortionDemand)LetterMaker.MakeLetter(label: def.letterLabel, text: "MFI_ExtortionDemand".Translate(
                    representative?.LabelShort ?? "MFI_Representative".Translate(),
                    faction.def.leaderTitle,
                    faction.Name,
                    worldObject.def.label,
                    worldObject.Label,
                    extorsionDemand
                ).AdjustedFor(p: faction.leader ?? Find.WorldPawns.AllPawnsAlive.Where(x => x.Faction == faction).RandomElement()), def: def.letterDef);

                choiceLetterExtortionDemand.title = "MFI_ExtortionDemandTitle".Translate(map.info.parent.Label).CapitalizeFirst();

                if (worldObject is Site)
                {
                    choiceLetterExtortionDemand.outpost = true;
                }

                choiceLetterExtortionDemand.radioMode = true;
                choiceLetterExtortionDemand.faction = faction;
                choiceLetterExtortionDemand.map = map;
                choiceLetterExtortionDemand.fee = extorsionDemand;
                choiceLetterExtortionDemand.StartTimeout(duration: TimeoutTicks);
                Find.LetterStack.ReceiveLetter(@let: choiceLetterExtortionDemand);
                Find.World.GetComponent<WorldComponent_OutpostGrower>().Registerletter(choiceLetterExtortionDemand);
                return true;
            }
            return false;
        }
    }
}
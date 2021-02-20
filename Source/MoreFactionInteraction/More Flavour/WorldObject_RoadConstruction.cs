using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class WorldObject_RoadConstruction : WorldObject
    {
        public int nextTile;

        //Path ?
        //Tick until done
        //Road to be
        public int projectedTimeOfCompletion;
        public RoadDef road;

        public override string GetInspectString()
        {
            return "MFI_EstTimeOfCompletion".Translate() +
                   $": {(projectedTimeOfCompletion - Find.TickManager.TicksGame).ToStringTicksToPeriodVague(false)}";
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame <= projectedTimeOfCompletion)
            {
                return;
            }

            Messages.Message("MFI_RoadSectionCompleted".Translate(), this, MessageTypeDefOf.TaskCompletion);
            Find.WorldGrid.OverlayRoad(Tile, nextTile, road); //OverlayRoad makes sure roads don't degrade
            Find.WorldObjects.Remove(this);
            Find.World.renderer.SetDirty<WorldLayer_Roads>();
            Find.World.renderer.SetDirty<WorldLayer_Paths>();
            Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(Tile);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref projectedTimeOfCompletion, "projectedTimeOfCompletion");
            Scribe_Defs.Look(ref road, "roadDef");
            Scribe_Values.Look(ref nextTile, "nextTile");
        }
    }
}
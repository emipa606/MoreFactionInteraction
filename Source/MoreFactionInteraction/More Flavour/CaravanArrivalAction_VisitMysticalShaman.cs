using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    internal class CaravanArrivalAction_VisitMysticalShaman : CaravanArrivalAction
    {
        private MysticalShaman mysticalShaman;

        public CaravanArrivalAction_VisitMysticalShaman()
        {
        }

        public CaravanArrivalAction_VisitMysticalShaman(MysticalShaman mysticalShaman)
        {
            this.mysticalShaman = mysticalShaman;
        }

        public override string Label => "VisitPeaceTalks".Translate(mysticalShaman.Label);
        public override string ReportString => "CaravanVisiting".Translate(mysticalShaman.Label);

        public override void Arrived(Caravan caravan)
        {
            mysticalShaman.Notify_CaravanArrived(caravan: caravan);
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MysticalShaman mysticalShaman)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(
                acceptanceReportGetter: () => CanVisit(mysticalShaman: mysticalShaman),
                arrivalActionGetter: () => new CaravanArrivalAction_VisitMysticalShaman(mysticalShaman: mysticalShaman),
                label: "VisitPeaceTalks".Translate(mysticalShaman.Label),
                caravan: caravan, pathDestination: mysticalShaman.Tile,
                revalidateWorldClickTarget: mysticalShaman);
        }

        public static FloatMenuAcceptanceReport CanVisit(MysticalShaman mysticalShaman)
        {
            return mysticalShaman != null && mysticalShaman.Spawned;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(refee: ref mysticalShaman, label: "mysticalShaman");
        }
    }
}
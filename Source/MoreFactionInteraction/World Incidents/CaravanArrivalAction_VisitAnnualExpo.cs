using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    internal class CaravanArrivalAction_VisitAnnualExpo : CaravanArrivalAction
    {
        private AnnualExpo annualExpo;

        public CaravanArrivalAction_VisitAnnualExpo()
        {
        }

        private CaravanArrivalAction_VisitAnnualExpo(AnnualExpo annualExpo)
        {
            this.annualExpo = annualExpo;
        }

        public override string Label => "VisitPeaceTalks".Translate(annualExpo.Label);
        public override string ReportString => "CaravanVisiting".Translate(annualExpo.Label);

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, AnnualExpo annualExpo)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(annualExpo),
                () => new CaravanArrivalAction_VisitAnnualExpo(annualExpo),
                "VisitPeaceTalks".Translate(annualExpo.Label),
                caravan, annualExpo.Tile,
                annualExpo);
        }

        private static FloatMenuAcceptanceReport CanVisit(AnnualExpo annualExpo)
        {
            return annualExpo is { Spawned: true };
        }

        public override void Arrived(Caravan caravan)
        {
            annualExpo.Notify_CaravanArrived(caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref annualExpo, "MFI_annualExpo");
        }
    }
}
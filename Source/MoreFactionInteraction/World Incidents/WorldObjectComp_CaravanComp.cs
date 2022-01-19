using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.World_Incidents;

public class WorldObjectComp_CaravanComp : WorldObjectComp
{
    public bool caravanIsWorking;
    public int workWillBeDoneAtTick;

    public override string CompInspectStringExtra()
    {
        if (CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)
                ?.GetComponent<WorldObjectComp_SettlementBumperCropComp>()?.CaravanIsWorking ?? false)
        {
            return "MFI_CaravanWorking".Translate();
        }

        return string.Empty;
    }

    public override void CompTick()
    {
        if (caravanIsWorking && Find.TickManager.TicksGame > workWillBeDoneAtTick)
        {
            CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)
                ?.GetComponent<WorldObjectComp_SettlementBumperCropComp>().DoOutcome((Caravan)parent);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref workWillBeDoneAtTick, "MFI_BumperCropWorkingCaravanWorkWillBeDoneAt");
        Scribe_Values.Look(ref caravanIsWorking, "MFI_BumperCropCaravanIsWorking");
    }
}
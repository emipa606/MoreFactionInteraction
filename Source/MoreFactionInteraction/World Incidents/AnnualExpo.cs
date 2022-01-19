using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class AnnualExpo : WorldObject
{
    public EventDef eventDef;
    public Faction host;

    public void Notify_CaravanArrived(Caravan caravan)
    {
        var pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, eventDef.relevantStat);
        if (pawn == null)
        {
            Messages.Message("MFI_AnnualExpoMessageNoRepresentative".Translate(), caravan,
                MessageTypeDefOf.NegativeEvent);
        }
        else
        {
            CameraJumper.TryJumpAndSelect(caravan);
            Find.WindowStack.Add(new Dialog_NodeTree(new AnnualExpoDialogue(pawn, caravan, eventDef, host)
                .AnnualExpoDialogueNode()));
            Find.WorldObjects.Remove(this);
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (var o in base.GetFloatMenuOptions(caravan))
        {
            yield return o;
        }

        foreach (var f in CaravanArrivalAction_VisitAnnualExpo.GetFloatMenuOptions(caravan,
                     this))
        {
            yield return f;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref eventDef, "MFI_EventDef");
        Scribe_References.Look(ref host, "MFI_ExpoHost");
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (stringBuilder.Length != 0)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(eventDef.LabelCap);
        return stringBuilder.ToString();
    }
}
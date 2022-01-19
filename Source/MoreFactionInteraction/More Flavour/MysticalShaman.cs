using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class MysticalShaman : WorldObject
{
    private Material cachedMat;

    public override Material Material
    {
        get
        {
            if (cachedMat == null)
            {
                cachedMat = MaterialPool.MatFrom(def.expandingIconTexture,
                    ShaderDatabase.WorldOverlayTransparentLit, Faction.Color,
                    WorldMaterials.WorldObjectRenderQueue);
            }

            return cachedMat;
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (var o in base.GetFloatMenuOptions(caravan))
        {
            yield return o;
        }

        foreach (var f in CaravanArrivalAction_VisitMysticalShaman.GetFloatMenuOptions(caravan, this))
        {
            yield return f;
        }
    }

    public void Notify_CaravanArrived(Caravan caravan)
    {
        var pawn = WorstCaravanPawnUtility.FindSickestPawn(caravan);
        if (pawn == null)
        {
            Find.WindowStack.Add(new Dialog_MessageBox("MFI_MysticalShamanFoundNoSickPawn".Translate()));
            //    Dialog_MessageBox.CreateConfirmation(text: "MFI_MysticalShamanFoundNoSickPawn".Translate(),
            //                                                 confirmedAct: () => Find.WorldObjects.Remove(o: this)));
        }
        else
        {
            CameraJumper.TryJumpAndSelect(caravan);
            var serum = ThingMaker.MakeThing(ThingDef.Named("MechSerumHealer")); //obj ref req, but me lazy.
            serum.TryGetComp<CompUseEffect_FixWorstHealthCondition>().DoEffect(pawn);
            Find.WindowStack.Add(
                new Dialog_MessageBox("MFI_MysticalShamanDoesHisMagic".Translate(pawn.LabelShort)));
        }

        Find.WorldObjects.Remove(this);
    }
}
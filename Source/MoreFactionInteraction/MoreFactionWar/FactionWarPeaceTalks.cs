using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using MoreFactionInteraction.MoreFactionWar;

namespace MoreFactionInteraction
{
    public class FactionWarPeaceTalks : WorldObject
    {
        private Material cachedMat;
        private Texture2D cachedExpandoIco;

        private Faction factionOne;
        private Faction factionInstigator;

        private bool canRemoveWithoutPostRemove;

        public override Material Material
        {
            get
            {
                if (cachedMat == null && Faction != null)
                {
                    cachedMat = MaterialPool.MatFrom(texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, color: factionOne?.Color ?? Color.white, renderQueue: WorldMaterials.WorldObjectRenderQueue);
                }

                return cachedMat;
            }
        }

        public override Color ExpandingIconColor => Color.white;

        public override Texture2D ExpandingIcon
        {
            get
            {
                if (cachedExpandoIco == null)
                {
                    cachedExpandoIco = MatFrom(texPath: def.expandingIconTexture, shader: ShaderDatabase.CutoutComplex, color: factionOne.Color, colorTwo: factionInstigator.Color, renderQueue: WorldMaterials.WorldObjectRenderQueue).GetMaskTexture();
                }

                return cachedExpandoIco;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan: caravan);
            if (pawn == null)
            {
                Messages.Message(text: "MessagePeaceTalksNoDiplomat".Translate(), lookTargets: caravan, def: MessageTypeDefOf.NegativeEvent, historical: false);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(target: caravan);
                var dialogue = new FactionWarDialogue(pawn: pawn, factionOne: factionOne, factionInstigator: factionInstigator, incidentTarget: caravan);
                var nodeRoot = dialogue.FactionWarPeaceTalks();
                Find.WindowStack.Add(window: new Dialogue_FactionWarNegotiation(factionOne: factionOne, factionInstigator: factionInstigator, nodeRoot: nodeRoot));
                canRemoveWithoutPostRemove = true;
                Find.WorldObjects.Remove(this);
            }
        }

        private static Material MatFrom(string texPath, Shader shader, Color color, Color colorTwo, int renderQueue)
        {
            var materialRequest = new MaterialRequest(tex: ContentFinder<Texture2D>.Get(itemPath: texPath), shader: shader)
            {
                renderQueue = renderQueue,
                color = colorTwo,
                colorTwo = color,
                maskTex = ContentFinder<Texture2D>.Get(itemPath: texPath + Graphic_Single.MaskSuffix, reportFailure: false),
            };
            return MaterialPool.MatFrom(req: materialRequest);
        }

        public void SetWarringFactions(Faction factionOne, Faction factionInstigator)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan: caravan))
            {
                yield return o;
            }

            foreach (FloatMenuOption f in CaravanArrivalAction_VisitFactionWarPeaceTalks.GetFloatMenuOptions(caravan: caravan, factionWarPeaceTalks: this))
            {
                yield return f;
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (!canRemoveWithoutPostRemove)
            {
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().DetermineWarAsIfNoPlayerInteraction(factionOne, factionInstigator);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref factionInstigator, "MFI_PeaceTalksFactionInstigator");
            Scribe_References.Look(ref factionOne, "MFI_PeaceTalksFactionOne");
        }
    }
}

using System.Collections.Generic;
using MoreFactionInteraction.MoreFactionWar;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction
{
    public class FactionWarPeaceTalks : WorldObject
    {
        private Texture2D cachedExpandoIco;
        private Material cachedMat;

        private bool canRemoveWithoutPostRemove;
        private Faction factionInstigator;

        private Faction factionOne;

        public override Material Material
        {
            get
            {
                if (cachedMat == null && Faction != null)
                {
                    cachedMat = MaterialPool.MatFrom(def.texture, ShaderDatabase.WorldOverlayTransparentLit,
                        factionOne?.Color ?? Color.white, WorldMaterials.WorldObjectRenderQueue);
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
                    cachedExpandoIco = MatFrom(def.expandingIconTexture, ShaderDatabase.CutoutComplex, factionOne.Color,
                        factionInstigator.Color, WorldMaterials.WorldObjectRenderQueue).GetMaskTexture();
                }

                return cachedExpandoIco;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            var pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
            if (pawn == null)
            {
                Messages.Message("MessagePeaceTalksNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent,
                    false);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(caravan);
                var dialogue = new FactionWarDialogue(pawn, factionOne, factionInstigator, caravan);
                var nodeRoot = dialogue.FactionWarPeaceTalks();
                Find.WindowStack.Add(new Dialogue_FactionWarNegotiation(factionOne, factionInstigator, nodeRoot));
                canRemoveWithoutPostRemove = true;
                Find.WorldObjects.Remove(this);
            }
        }

        private static Material MatFrom(string texPath, Shader shader, Color color, Color colorTwo, int renderQueue)
        {
            var materialRequest = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader)
            {
                renderQueue = renderQueue,
                color = colorTwo,
                colorTwo = color,
                maskTex = ContentFinder<Texture2D>.Get(texPath + Graphic_Single.MaskSuffix, false)
            };
            return MaterialPool.MatFrom(materialRequest);
        }

        public void SetWarringFactions(Faction factionOne, Faction factionInstigator)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (var o in base.GetFloatMenuOptions(caravan))
            {
                yield return o;
            }

            foreach (var f in CaravanArrivalAction_VisitFactionWarPeaceTalks.GetFloatMenuOptions(caravan, this))
            {
                yield return f;
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (!canRemoveWithoutPostRemove)
            {
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>()
                    .DetermineWarAsIfNoPlayerInteraction(factionOne, factionInstigator);
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
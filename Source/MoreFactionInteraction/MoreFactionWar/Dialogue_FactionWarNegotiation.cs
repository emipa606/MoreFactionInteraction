using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction
{
    public class Dialogue_FactionWarNegotiation : Dialog_NodeTree
    {
        private const float TitleHeight = 70f;
        private const float InfoHeight = 60f;
        private readonly Faction factionInstigator;
        private readonly Faction factionOne;

        public Dialogue_FactionWarNegotiation(Faction factionOne, Faction factionInstigator, DiaNode nodeRoot,
            bool delayInteractivity = false, bool radioMode = false, string title = null) : base(nodeRoot,
            delayInteractivity, radioMode, title)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override Vector2 InitialSize => new Vector2(720f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            var leftFactionLabel = new Rect(0f, 0f, inRect.width / 2f, TitleHeight);
            var leftBox = new Rect(0f, leftFactionLabel.yMax, leftFactionLabel.width, InfoHeight);
            var rightFactionLabel = new Rect(inRect.width / 2f, 0f, inRect.width / 2f, TitleHeight);
            var rightBox = new Rect(inRect.width / 2f, leftFactionLabel.yMax, leftFactionLabel.width, InfoHeight);

            Text.Font = GameFont.Medium;
            Widgets.Label(leftFactionLabel, factionOne.GetCallLabel());
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rightFactionLabel, factionInstigator.GetCallLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            Widgets.Label(leftBox, factionOne.GetInfoText());
            if (factionOne != null)
            {
                var playerRelationKind = factionOne.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                var factionOneRelactionBox = new Rect(leftBox.x,
                    leftBox.y + Text.CalcHeight(factionOne.GetInfoText(), leftBox.width) + Text.SpaceBetweenLines,
                    leftBox.width, 30f);
                Widgets.Label(factionOneRelactionBox, playerRelationKind.GetLabel());
            }

            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rightBox, factionInstigator.GetInfoText());

            if (factionInstigator != null)
            {
                var playerRelationKind = factionInstigator.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                var factionInstigatorRelactionBox = new Rect(rightBox.x,
                    rightBox.y + Text.CalcHeight(factionInstigator.GetInfoText(), rightBox.width) +
                    Text.SpaceBetweenLines, rightBox.width, 30f);
                Widgets.Label(factionInstigatorRelactionBox, playerRelationKind.GetLabel());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
            const float magicalNum = 147f;
            var middleRemainingRectForDialog = new Rect(0f, magicalNum, inRect.width, inRect.height - magicalNum);
            DrawNode(middleRemainingRectForDialog);
        }
    }
}
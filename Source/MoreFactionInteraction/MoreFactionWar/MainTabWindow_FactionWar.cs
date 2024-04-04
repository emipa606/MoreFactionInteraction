using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar;

[StaticConstructorOnStartup]
public class MainTabWindow_FactionWar : MainTabWindow_Factions
{
    private const float TitleHeight = 70f;
    private const float InfoHeight = 60f;

    private static Texture2D factionOneColorTexture;
    private static Texture2D factionTwoColorTexture;


    //[TweakValue("MainTabWindow_FactionWar", -100f, 150f)]
    private static readonly float yMaxOffset = 0;

    //[TweakValue("MainTabWindow_FactionWar", -50f, 50f)]
    private static readonly float yPositionBar = 33;

    //[TweakValue("MainTabWindow_FactionWar", -50f, 50f)]
    private static readonly float barHeight = 32;

    public static void ResetBars()
    {
        factionOneColorTexture = null;
        factionTwoColorTexture = null;
    }

    private static Texture2D FactionOneColorTexture(Faction factionOne)
    {
        if (factionOneColorTexture == null && factionOne != null)
        {
            factionOneColorTexture = SolidColorMaterials.NewSolidColorTexture(factionOne.Color);
        }

        return factionOneColorTexture;
    }

    private static Texture2D FactionTwoColorTexture(Faction factionInstigator)
    {
        if (factionTwoColorTexture == null && factionInstigator != null)
        {
            factionTwoColorTexture = SolidColorMaterials.NewSolidColorTexture(factionInstigator.Color);
        }

        return factionTwoColorTexture;
    }

    public override void DoWindowContents(Rect fillRect)
    {
        //regular faction tab if no war/unrest. Fancy tab otherwise.
        if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing)
        {
            base.DoWindowContents(fillRect);
        }
        else
        {
            DrawFactionWarBar(fillRect);

            // scooch down original. amount of offset depends on devmode or not (because of devmode "show all" button)
            var baseRect = fillRect;
            baseRect.y = Prefs.DevMode ? fillRect.y + 120f : fillRect.y + 75f;
            baseRect.yMax = fillRect.yMax + yMaxOffset;

            GUI.BeginGroup(baseRect);
            base.DoWindowContents(baseRect);
            GUI.EndGroup();
        }
    }

    public static void DrawFactionWarBar(Rect fillRect)
    {
        if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StuffIsGoingDown)
        {
            return;
        }

        var factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
        var factionInstigator = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

        var position = new Rect(fillRect.x, fillRect.y, fillRect.width, fillRect.height);
        GUI.BeginGroup(position);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        //4 boxes: faction name and faction call label.
        var leftFactionLabel = new Rect(0f, 0f, position.width / 2f, TitleHeight);
        var leftBox = new Rect(0f, leftFactionLabel.yMax, leftFactionLabel.width, InfoHeight);
        var rightFactionLabel = new Rect(position.width / 2f, 0f, position.width / 2f, TitleHeight);
        var rightBox = new Rect(position.width / 2f, leftFactionLabel.yMax, leftFactionLabel.width, InfoHeight);

        //big central box
        var centreBoxForBigFactionwar = new Rect(0f, leftFactionLabel.yMax, position.width, TitleHeight);
        Text.Font = GameFont.Medium;
        GUI.color = Color.cyan;
        Text.Anchor = TextAnchor.MiddleCenter;
        string factionWarStatus = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
            ? "MFI_FactionWarProgress".Translate()
            : "MFI_UnrestIsBrewing".Translate();
        Widgets.Label(centreBoxForBigFactionwar, factionWarStatus);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;

        //"scorecard" bar
        var leftFactionOneScoreBox = new Rect(0f, yPositionBar,
            position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ScoreForFaction(factionOne),
            barHeight);
        GUI.DrawTexture(leftFactionOneScoreBox, FactionOneColorTexture(factionOne));
        var rightFactionTwoScoreBox = new Rect(
            position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ScoreForFaction(factionOne),
            yPositionBar,
            position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>()
                .ScoreForFaction(factionInstigator), barHeight);
        GUI.DrawTexture(rightFactionTwoScoreBox, FactionTwoColorTexture(factionInstigator));

        //stuff that fills up and does the faction name and call label boxes.
        Text.Font = GameFont.Medium;
        Widgets.Label(leftFactionLabel, factionOne.GetCallLabel());
        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(rightFactionLabel, factionInstigator.GetCallLabel());
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        Widgets.Label(leftBox, factionOne.GetInfoText());

        var factionOnePlayerRelationKind = factionOne.PlayerRelationKind;
        GUI.color = factionOnePlayerRelationKind.GetColor();
        var factionOneRelactionBox = new Rect(leftBox.x,
            leftBox.y + Text.CalcHeight(factionOne.GetInfoText(), leftBox.width) + Text.SpaceBetweenLines,
            leftBox.width, 30f);
        Widgets.Label(factionOneRelactionBox, factionOnePlayerRelationKind.GetLabel());

        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(rightBox, factionInstigator.GetInfoText());

        var factionInstigatorPlayerRelationKind = factionInstigator.PlayerRelationKind;
        GUI.color = factionInstigatorPlayerRelationKind.GetColor();
        var factionInstigatorRelactionBox = new Rect(rightBox.x,
            rightBox.y + Text.CalcHeight(factionInstigator.GetInfoText(), rightBox.width) +
            Text.SpaceBetweenLines, rightBox.width, 30f);
        Widgets.Label(factionInstigatorRelactionBox, factionInstigatorPlayerRelationKind.GetLabel());

        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        GUI.EndGroup();
    }
}
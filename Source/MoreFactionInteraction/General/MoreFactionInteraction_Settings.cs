﻿using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

public class MoreFactionInteraction_Settings : ModSettings
{
    public static int ticksToUpgrade = 3 * GenDate.DaysPerQuadrum * GenDate.TicksPerDay; //3 * 15 * 60000 = 2700000
    public static float timeModifierBetweenFactionInteraction = 1f;
    public static float traderWealthOffsetFromTimesTraded = 0.7f;
    public static float pirateBaseUpgraderModifier = 0.8f;
    public static bool enableAnnualExpo = true;
    public static bool doNotAffectTraders;

    public void DoWindowContents(Rect rect)
    {
        var options = new Listing_Standard();
        options.Begin(rect);
        options.Gap();
        options.SliderLabeled("MFI_ticksToUpgrade".Translate(), ref ticksToUpgrade,
            ticksToUpgrade.ToStringTicksToPeriodVague(false), 0, GenDate.TicksPerYear,
            "MFI_ticksToUpgradeDesc".Translate());
        options.Gap();
        options.SliderLabeled("MFI_pirateBaseUpgraderModifier".Translate(), ref pirateBaseUpgraderModifier,
            pirateBaseUpgraderModifier.ToStringByStyle(ToStringStyle.FloatOne), 0.1f, 2f,
            "MFI_pirateBaseUpgraderModifierDesc".Translate());
        options.GapLine();
        options.SliderLabeled("MFI_timeModifierBetweenFactionInteraction".Translate(),
            ref timeModifierBetweenFactionInteraction,
            timeModifierBetweenFactionInteraction.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 20f,
            "MFI_timeModifierBetweenFactionInteractionDesc".Translate());
        options.CheckboxLabeled("MFI_doNotAffectTraders".Translate(), ref doNotAffectTraders,
            "MFI_doNotAffectTradersDesc".Translate());
        options.GapLine();
        options.SliderLabeled("MFI_traderWealthOffsetFromTimesTraded".Translate(),
            ref traderWealthOffsetFromTimesTraded,
            traderWealthOffsetFromTimesTraded.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 3f);
        options.CheckboxLabeled("MFI_enableannualexpo".Translate(), ref enableAnnualExpo,
            "MFI_enableannualexpotooltip".Translate());
        if (MoreFactionInteractionMod.currentVersion != null)
        {
            options.Gap();
            GUI.contentColor = Color.gray;
            options.Label("MFI_CurrentModVersion".Translate(MoreFactionInteractionMod.currentVersion));
            GUI.contentColor = Color.white;
        }

        options.End();
        Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref ticksToUpgrade, "ticksToUpgrade", 2700000);
        Scribe_Values.Look(ref enableAnnualExpo, "enableAnnualExpo", true);
        Scribe_Values.Look(ref doNotAffectTraders, "doNotAffectTraders");
        Scribe_Values.Look(ref timeModifierBetweenFactionInteraction, "timeModifierBetweenFactionInteraction", 1f);
        Scribe_Values.Look(ref traderWealthOffsetFromTimesTraded, "traderWealthOffsetFromTimesTraded", 0.8f);
        Scribe_Values.Look(ref pirateBaseUpgraderModifier, "MFI_pirateBaseUpgraderModifier", 0.8f);
    }
}
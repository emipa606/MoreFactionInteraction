﻿using RimWorld;
using UnityEngine;
using Verse;
using JetBrains.Annotations;


namespace MoreFactionInteraction
{
    public class MoreFactionInteraction_Settings : ModSettings
    {
        public static int ticksToUpgrade = 3 * GenDate.DaysPerQuadrum * GenDate.TicksPerDay; //3 * 15 * 60000 = 2700000
        public static float timeModifierBetweenFactionInteraction = 1f;
        public static float traderWealthOffsetFromTimesTraded = 0.7f;
        public static float pirateBaseUpgraderModifier = 0.8f;

        public void DoWindowContents(Rect rect)
        {
            var options = new Listing_Standard();
            options.Begin(rect: rect);
            options.Gap();
            options.SliderLabeled(label: "MFI_ticksToUpgrade".Translate(), val: ref ticksToUpgrade, format: ticksToUpgrade.ToStringTicksToPeriodVague(vagueMin: false), min: 0, max: GenDate.TicksPerYear, "MFI_ticksToUpgradeDesc".Translate());
            options.Gap();
            options.SliderLabeled(label: "MFI_pirateBaseUpgraderModifier".Translate(), val: ref pirateBaseUpgraderModifier, format: pirateBaseUpgraderModifier.ToStringByStyle(style: ToStringStyle.FloatOne), min: 0.1f, max: 2f, "MFI_pirateBaseUpgraderModifierDesc".Translate());
            options.GapLine();
            options.SliderLabeled(label: "MFI_timeModifierBetweenFactionInteraction".Translate(), val: ref timeModifierBetweenFactionInteraction, format: timeModifierBetweenFactionInteraction.ToStringByStyle(style: ToStringStyle.FloatOne), min: 0.5f, max: 3f, "MFI_timeModifierBetweenFactionInteractionDesc".Translate());
            options.Gap();
            options.SliderLabeled(label: "MFI_traderWealthOffsetFromTimesTraded".Translate(), val: ref traderWealthOffsetFromTimesTraded, format: traderWealthOffsetFromTimesTraded.ToStringByStyle(style: ToStringStyle.FloatOne), min: 0.5f, max: 3f);
            options.End();
            Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(value: ref ticksToUpgrade, label: "ticksToUpgrade", defaultValue: 2700000);
            Scribe_Values.Look(value: ref timeModifierBetweenFactionInteraction, label: "timeModifierBetweenFactionInteraction", defaultValue: 1f);
            Scribe_Values.Look(value: ref traderWealthOffsetFromTimesTraded, label: "traderWealthOffsetFromTimesTraded", defaultValue: 0.8f);
            Scribe_Values.Look(value: ref pirateBaseUpgraderModifier, label: "MFI_pirateBaseUpgraderModifier", defaultValue: 0.8f);
        }
    }

    [UsedImplicitly]
    public class MoreFactionInteractionMod : Mod
    {
        public MoreFactionInteractionMod(ModContentPack content) : base(content: content)
        {
            GetSettings<MoreFactionInteraction_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect: inRect);
            GetSettings<MoreFactionInteraction_Settings>().DoWindowContents(rect: inRect);
        }

        public override string SettingsCategory()
        {
            return "More Faction Interaction";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}

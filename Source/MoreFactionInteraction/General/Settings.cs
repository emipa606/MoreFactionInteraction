using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

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
                timeModifierBetweenFactionInteraction.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 10f,
                "MFI_timeModifierBetweenFactionInteractionDesc".Translate());
            options.Gap();
            options.SliderLabeled("MFI_traderWealthOffsetFromTimesTraded".Translate(),
                ref traderWealthOffsetFromTimesTraded,
                traderWealthOffsetFromTimesTraded.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 3f);
            options.End();
            Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToUpgrade, "ticksToUpgrade", 2700000);
            Scribe_Values.Look(ref timeModifierBetweenFactionInteraction, "timeModifierBetweenFactionInteraction", 1f);
            Scribe_Values.Look(ref traderWealthOffsetFromTimesTraded, "traderWealthOffsetFromTimesTraded", 0.8f);
            Scribe_Values.Look(ref pirateBaseUpgraderModifier, "MFI_pirateBaseUpgraderModifier", 0.8f);
        }
    }

    [UsedImplicitly]
    public class MoreFactionInteractionMod : Mod
    {
        public MoreFactionInteractionMod(ModContentPack content) : base(content)
        {
            GetSettings<MoreFactionInteraction_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<MoreFactionInteraction_Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "More Faction Interaction";
        }
    }
}
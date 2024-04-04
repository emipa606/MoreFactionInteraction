using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

//thanks to AlexTD for the below
internal static class SettingsHelper
{
    //private static float gap = 12f;

    public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format,
        float min = 0f, float max = 100f, string tooltip = null)
    {
        float fVal = val;
        ls.SliderLabeled(label, ref fVal, format, min, max);
        val = (int)fVal;
    }

    public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format,
        float min = 0f, float max = 1f, string tooltip = null)
    {
        var rect = ls.GetRect(Text.LineHeight);
        var rect2 = rect.LeftPart(.70f).Rounded();
        var rect3 = rect.RightPart(.30f).Rounded().LeftPart(.67f).Rounded();
        var rect4 = rect.RightPart(.10f).Rounded();

        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect2, label);

        var result = Widgets.HorizontalSlider(rect3, val, min, max, true);
        val = result;
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect4, string.Format(format, val));
        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        Text.Anchor = anchor;
        ls.Gap(ls.verticalSpacing);
    }
}
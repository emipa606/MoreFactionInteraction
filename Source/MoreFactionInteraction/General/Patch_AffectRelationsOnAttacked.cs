using HarmonyLib;
using RimWorld.Planet;

namespace MoreFactionInteraction;

[HarmonyPatch(typeof(SettlementUtility), nameof(SettlementUtility.AffectRelationsOnAttacked))]
internal static class Patch_AffectRelationsOnAttacked
{
    private static bool Prefix(MapParent mapParent)
    {
        if (mapParent is not Site site || site.parts == null)
        {
            return true;
        }

        foreach (var part in site.parts)
        {
            if (part.def == MFI_DefOf.MFI_HuntersLodgePart)
            {
                return false;
            }
        }

        return true;
    }
}
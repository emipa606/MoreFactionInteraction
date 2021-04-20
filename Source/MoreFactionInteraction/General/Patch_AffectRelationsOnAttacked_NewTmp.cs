using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    [HarmonyPatch(typeof(SettlementUtility), "AffectRelationsOnAttacked_NewTmp")]
    internal static class Patch_AffectRelationsOnAttacked_NewTmp
    {
        private static bool Prefix(MapParent mapParent, ref TaggedString letterText)
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
}
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    internal class MFI_SymbolResolver_BasePart_Indoors : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width > 13 || rp.rect.Height > 13 ||
                (rp.rect.Width >= 9 || rp.rect.Height >= 9) && Rand.Chance(0.3f))
            {
                BaseGen.symbolStack.Push("MFI_basePart_indoors_division", rp);
            }
            else
            {
                BaseGen.symbolStack.Push("MFI_basePart_indoors_leaf", rp);
            }
        }
    }
}
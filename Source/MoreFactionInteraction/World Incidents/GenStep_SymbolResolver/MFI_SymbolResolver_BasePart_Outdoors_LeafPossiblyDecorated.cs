using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    internal class MFI_SymbolResolver_BasePart_Outdoors_LeafPossiblyDecorated : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width >= 10 && rp.rect.Height >= 10 && Rand.Chance(0.25f))
            {
                BaseGen.symbolStack.Push("MFI_basePart_outdoors_leafDecorated", rp);
            }
            else
            {
                BaseGen.symbolStack.Push("MFI_basePart_outdoors_leaf", rp);
            }
        }
    }
}
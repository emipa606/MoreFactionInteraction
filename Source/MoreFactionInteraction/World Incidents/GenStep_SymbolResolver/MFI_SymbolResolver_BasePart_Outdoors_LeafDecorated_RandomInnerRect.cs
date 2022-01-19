using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver;

internal class MFI_SymbolResolver_BasePart_Outdoors_LeafDecorated_RandomInnerRect : SymbolResolver
{
    private const int MinLength = 5;

    private const int MaxRectSize = 15;

    public override bool CanResolve(ResolveParams rp)
    {
        return base.CanResolve(rp) && rp.rect.Width <= MaxRectSize
                                   && rp.rect.Height <= MaxRectSize
                                   && rp.rect.Width > MinLength
                                   && rp.rect.Height > MinLength;
    }

    public override void Resolve(ResolveParams rp)
    {
        var num = Rand.RangeInclusive(MinLength, rp.rect.Width - 1);
        var num2 = Rand.RangeInclusive(MinLength, rp.rect.Height - 1);
        var num3 = Rand.RangeInclusive(0, rp.rect.Width - num);
        var num4 = Rand.RangeInclusive(0, rp.rect.Height - num2);
        var resolveParams = rp;
        resolveParams.rect = new CellRect(rp.rect.minX + num3, rp.rect.minZ + num4, num, num2);
        BaseGen.symbolStack.Push("MFI_basePart_outdoors_leaf", resolveParams);
    }
}
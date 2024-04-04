using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver;

internal class MFI_SymbolResolver_BasePart_Indoors_Division_Split : SymbolResolver
{
    private const int MinLengthAfterSplit = 5;

    private const int MinWidthOrHeight = 9;

    public override bool CanResolve(ResolveParams rp)
    {
        return base.CanResolve(rp) && (rp.rect.Width >= MinWidthOrHeight || rp.rect.Height >= MinWidthOrHeight);
    }

    public override void Resolve(ResolveParams rp)
    {
        if (rp.rect is { Width: < MinWidthOrHeight, Height: < MinWidthOrHeight })
        {
            Log.Warning($"Too small rect. params={rp}");
        }
        else
        {
            if (Rand.Bool && rp.rect.Height >= MinWidthOrHeight || rp.rect.Width < MinWidthOrHeight)
            {
                var num = Rand.RangeInclusive(4, rp.rect.Height - MinLengthAfterSplit);
                var resolveParams = rp;
                resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, num + 1);
                BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams);
                var resolveParams2 = rp;
                resolveParams2.rect =
                    new CellRect(rp.rect.minX, rp.rect.minZ + num, rp.rect.Width, rp.rect.Height - num);
                BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams2);
            }
            else
            {
                var num2 = Rand.RangeInclusive(4, rp.rect.Width - MinLengthAfterSplit);
                var resolveParams3 = rp;
                resolveParams3.rect = new CellRect(rp.rect.minX, rp.rect.minZ, num2 + 1, rp.rect.Height);
                BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams3);
                var resolveParams4 = rp;
                resolveParams4.rect =
                    new CellRect(rp.rect.minX + num2, rp.rect.minZ, rp.rect.Width - num2, rp.rect.Height);
                BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams4);
            }
        }
    }
}
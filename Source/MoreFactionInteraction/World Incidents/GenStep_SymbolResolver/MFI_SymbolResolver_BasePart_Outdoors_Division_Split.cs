using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver;

// yeah, this is mostly taken from vanilla.
internal class MFI_SymbolResolver_BasePart_Outdoors_Division_Split : SymbolResolver
{
    private const int MinLengthAfterSplit = 5;

    private static readonly IntRange SpaceBetweenRange = new IntRange(1, 2);

    public override bool CanResolve(ResolveParams rp)
    {
        return base.CanResolve(rp) && (TryFindSplitPoint(false, rp.rect, out _, out _)
                                       || TryFindSplitPoint(true, rp.rect, out _, out _));
    }

    public override void Resolve(ResolveParams rp)
    {
        var coinFlip = Rand.Bool;
        bool flipACoin;
        if (TryFindSplitPoint(coinFlip, rp.rect, out var splitPoint, out var spaceBetween))
        {
            flipACoin = coinFlip;
        }
        else
        {
            if (!TryFindSplitPoint(!coinFlip, rp.rect, out splitPoint, out spaceBetween))
            {
                Log.Warning("Could not find split point.");
                return;
            }

            flipACoin = !coinFlip;
        }

        var floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
        ResolveParams resolveVariantA;
        ResolveParams resolveVariantB;
        if (flipACoin)
        {
            var resolveParams = rp;
            resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.minZ + splitPoint, rp.rect.Width, spaceBetween);
            resolveParams.floorDef = floorDef;
            resolveParams.streetHorizontal = true;
            BaseGen.symbolStack.Push("street", resolveParams);
            var resolveParams2 = rp;
            resolveParams2.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, splitPoint);
            resolveVariantA = resolveParams2;
            var resolveParams4 = rp;
            resolveParams4.rect = new CellRect(rp.rect.minX, rp.rect.minZ + splitPoint + spaceBetween,
                rp.rect.Width, rp.rect.Height - splitPoint - spaceBetween);
            resolveVariantB = resolveParams4;
        }
        else
        {
            var resolveParams6 = rp;
            resolveParams6.rect =
                new CellRect(rp.rect.minX + splitPoint, rp.rect.minZ, spaceBetween, rp.rect.Height);
            resolveParams6.floorDef = floorDef;
            resolveParams6.streetHorizontal = false;
            BaseGen.symbolStack.Push("street", resolveParams6);
            var resolveParams7 = rp;
            resolveParams7.rect = new CellRect(rp.rect.minX, rp.rect.minZ, splitPoint, rp.rect.Height);
            resolveVariantA = resolveParams7;
            var resolveParams8 = rp;
            resolveParams8.rect = new CellRect(rp.rect.minX + splitPoint + spaceBetween, rp.rect.minZ,
                rp.rect.Width - splitPoint - spaceBetween, rp.rect.Height);
            resolveVariantB = resolveParams8;
        }

        if (Rand.Bool)
        {
            BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantA);
            BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantB);
        }
        else
        {
            BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantB);
            BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantA);
        }
    }

    private bool TryFindSplitPoint(bool horizontal, CellRect rect, out int splitPoint, out int spaceBetween)
    {
        var num = !horizontal ? rect.Width : rect.Height;
        spaceBetween = SpaceBetweenRange.RandomInRange;
        spaceBetween = Mathf.Min(spaceBetween, num - 10);
        if (spaceBetween < SpaceBetweenRange.min)
        {
            splitPoint = -1;
            return false;
        }

        splitPoint = Rand.RangeInclusive(MinLengthAfterSplit, num - MinLengthAfterSplit - spaceBetween);
        return true;
    }
}
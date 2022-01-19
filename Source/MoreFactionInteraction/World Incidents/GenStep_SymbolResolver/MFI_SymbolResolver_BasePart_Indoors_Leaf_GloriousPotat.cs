using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver;

internal class MFI_SymbolResolver_BasePart_Indoors_Leaf_GloriousPotat : SymbolResolver
{
    public override void Resolve(ResolveParams rp)
    {
        var rect = new CellRect(rp.rect.maxX - 3, rp.rect.maxZ - 3, 4, 4);
        var gloriousPotat = ThingDefOf.RawPotatoes;
        var num = Rand.RangeInclusive(2, 3);
        for (var i = 0; i < num; i++)
        {
            var resolveParams = rp;
            resolveParams.rect = rect.ContractedBy(1);
            resolveParams.singleThingDef = gloriousPotat;
            resolveParams.singleThingStackCount = Rand.RangeInclusive(Mathf.Min(10, gloriousPotat.stackLimit),
                Mathf.Min(50, gloriousPotat.stackLimit));
            BaseGen.symbolStack.Push("thing", resolveParams);
        }

        var resolveParams2 = rp;
        resolveParams2.rect = rect;
        BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams2);
        var resolveParams3 = rp;
        resolveParams3.rect = rect;
        BaseGen.symbolStack.Push("emptyRoom", resolveParams3);
    }
}
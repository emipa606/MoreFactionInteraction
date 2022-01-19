using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver;

internal class MFI_SymbolResolver_BasePart_Outdoors_LeafDecorated_EdgeStreet : SymbolResolver
{
    public override void Resolve(ResolveParams rp)
    {
        var resolveParams = rp;
        resolveParams.floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
        BaseGen.symbolStack.Push("edgeStreet", resolveParams);
        var resolveParams2 = rp;
        resolveParams2.rect = rp.rect.ContractedBy(1);
        BaseGen.symbolStack.Push("MFI_basePart_outdoors_leaf", resolveParams2);
    }
}
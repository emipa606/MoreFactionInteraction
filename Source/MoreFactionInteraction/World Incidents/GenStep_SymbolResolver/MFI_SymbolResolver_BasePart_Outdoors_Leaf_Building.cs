using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    internal class MFI_SymbolResolver_BasePart_Outdoors_Leaf_Building : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) && (BaseGen.globalSettings.basePart_emptyNodesResolved >=
                                           BaseGen.globalSettings.minEmptyNodes
                                           || BaseGen.globalSettings.basePart_buildingsResolved <
                                           BaseGen.globalSettings.minBuildings);
        }

        public override void Resolve(ResolveParams rp)
        {
            var resolveParams = rp;
            resolveParams.wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
            resolveParams.floorDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, true);
            BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams);
            BaseGen.globalSettings.basePart_buildingsResolved++;
        }
    }
}
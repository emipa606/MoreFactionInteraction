using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    public class SymbolResolver_BasePart_Indoors_Leaf_Kitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("kitchen", rp);
        }
    }
}
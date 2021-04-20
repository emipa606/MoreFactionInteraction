using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    public class SymbolResolver_Interior_Kitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(rp);
            BaseGen.symbolStack.Push("fillWithKitchen", rp);
        }
    }
}
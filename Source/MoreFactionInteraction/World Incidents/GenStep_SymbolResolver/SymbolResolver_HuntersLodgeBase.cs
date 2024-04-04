using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents;

public class SymbolResolver_HuntersLodgeBase : SymbolResolver
{
    public override void Resolve(ResolveParams rp)
    {
        //Map map = BaseGen.globalSettings.map;
        var faction = rp.faction ?? Find.FactionManager.RandomAlliedFaction();
        var num = 0;

        if (rp.rect is { Width: >= 20, Height: >= 20 } &&
            (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
        {
            num = !Rand.Bool ? 4 : 2;
        }

        var num2 = rp.rect.Area / 144f * 0.17f;
        BaseGen.globalSettings.minEmptyNodes = num2 >= 1f ? GenMath.RoundRandom(num2) : 0;

        BaseGen.symbolStack.Push("outdoorLighting", rp);
        if (faction.def.techLevel >= TechLevel.Industrial)
        {
            var num4 = !Rand.Chance(0.75f) ? 0 : GenMath.RoundRandom(rp.rect.Area / 400f);
            for (var i = 0; i < num4; i++)
            {
                var resolveParams2 = rp;
                resolveParams2.faction = faction;
                BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
            }
        }

        var resolveParams4 = rp;
        resolveParams4.rect = rp.rect.ContractedBy(num);
        resolveParams4.faction = faction;
        BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);

        var mainBasePart = rp;
        mainBasePart.faction = faction;
        BaseGen.symbolStack.Push("MFI_basePart_outdoors_division", mainBasePart);
    }
}
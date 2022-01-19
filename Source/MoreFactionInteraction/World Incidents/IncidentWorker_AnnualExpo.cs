using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class IncidentWorker_AnnualExpo : IncidentWorker
{
    private const int MinDistance = 12;
    private const int MaxDistance = 26;
    private static readonly IntRange TimeoutDaysRange = new IntRange(15, 21);

    public override float BaseChanceThisGame => 0f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!MoreFactionInteraction_Settings.enableAnnualExpo)
        {
            return false;
        }

        return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null
                                         && TryGetRandomAvailableTargetMap(out _)
                                         && TryFindTile(out _)
                                         && TryGetFactionHost(out _)
                                         && !Find.World.worldObjects.AllWorldObjects.Any(x => x is AnnualExpo);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!MoreFactionInteraction_Settings.enableAnnualExpo)
        {
            return false;
        }

        var worldComp = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>();


        if (worldComp == null)
        {
            return false;
        }

        if (!TryGetRandomAvailableTargetMap(out var map))
        {
            return false;
        }

        if (map == null)
        {
            return false;
        }

        if (!TryFindTile(out var tile))
        {
            return false;
        }

        if (!TryGetFactionHost(out var faction))
        {
            return false;
        }

        var annualExpo = (AnnualExpo)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_AnnualExpoObject);
        annualExpo.Tile = tile;
        annualExpo.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * GenDate.TicksPerDay);
        worldComp.events.InRandomOrder().TryMinBy(kvp => kvp.Value, out var result);
        annualExpo.eventDef = result.Key;
        annualExpo.host = faction;
        annualExpo.SetFaction(faction);

        worldComp.timesHeld++;
        worldComp.events[result.Key]++;

        Find.WorldObjects.Add(annualExpo);
        Find.LetterStack.ReceiveLetter(def.letterLabel,
            "MFI_AnnualExpoLetterText".Translate(
                Find.ActiveLanguageWorker.OrdinalNumber(worldComp.TimesHeld),
                Find.World.info.name,
                annualExpo.host.Name,
                annualExpo.eventDef.theme,
                annualExpo.eventDef.themeDesc),
            def.letterDef,
            annualExpo);

        return true;
    }

    private static bool TryGetFactionHost(out Faction faction)
    {
        return Find.FactionManager.AllFactionsVisible
            .Where(x => !x.defeated && !x.def.permanentEnemy && !x.IsPlayer && !x.temporary)
            .TryRandomElement(out faction);
    }

    private static bool TryGetRandomAvailableTargetMap(out Map map)
    {
        return Find.Maps.Where(x => x.IsPlayerHome).TryRandomElement(out map);
    }

    private static bool TryFindTile(out int tile)
    {
        return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using MoreFactionInteraction.General;


namespace MoreFactionInteraction.More_Flavour
{

    public class IncidentWorker_AnnualExpo : IncidentWorker
    {
        private const int MinDistance = 12;
        private const int MaxDistance = 26;
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 15, max: 21);

        public override float BaseChanceThisGame => 0f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null
                                             && TryGetRandomAvailableTargetMap(out Map map)
                                             && TryFindTile(tile: out var num)
                                             && TryGetFactionHost(out Faction faction)
                                             && !Find.World.worldObjects.AllWorldObjects.Any(x => x is AnnualExpo);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            WorldComponent_MFI_AnnualExpo worldComp = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>();

            if (worldComp == null)
            {
                return false;
            }

            if (!TryGetRandomAvailableTargetMap(map: out Map map))
            {
                return false;
            }

            if (map == null)
            {
                return false;
            }

            if (!TryFindTile(tile: out var tile))
            {
                return false;
            }

            if (!TryGetFactionHost(out Faction faction))
            {
                return false;
            }

            var annualExpo = (AnnualExpo)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_AnnualExpoObject);
            annualExpo.Tile = tile;
            annualExpo.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * GenDate.TicksPerDay);
            worldComp.events.InRandomOrder().TryMinBy(kvp => kvp.Value, out KeyValuePair<EventDef, int> result);
            annualExpo.eventDef = result.Key;
            annualExpo.host = faction;
            annualExpo.SetFaction(faction);

            worldComp.timesHeld++;
            worldComp.events[result.Key]++;

            Find.WorldObjects.Add(o: annualExpo);
            Find.LetterStack.ReceiveLetter(label: def.letterLabel,
                                            text: "MFI_AnnualExpoLetterText".Translate(
                                                Find.ActiveLanguageWorker.OrdinalNumber(worldComp.TimesHeld),
                                                Find.World.info.name,
                                                annualExpo.host.Name,
                                                annualExpo.eventDef.theme,
                                                annualExpo.eventDef.themeDesc),
                                            textLetterDef: def.letterDef,
                                            lookTargets: annualExpo);

            return true;
        }

        private static bool TryGetFactionHost(out Faction faction)
        {
            return Find.FactionManager.AllFactionsVisible.Where(x => !x.defeated && !x.def.permanentEnemy && !x.IsPlayer).TryRandomElement(out faction);
        }

        private static bool TryGetRandomAvailableTargetMap(out Map map)
        {
            return Find.Maps.Where(x => x.IsPlayerHome).TryRandomElement(out map);
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }
    }
}

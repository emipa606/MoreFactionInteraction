using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_HuntersLodge : IncidentWorker
    {
        private const int MinDistance = 2;
        private const int MaxDistance = 15;

        private static readonly IntRange TimeoutDaysRange = new(15, 25);


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null
                                             && Find.FactionManager.RandomAlliedFaction(false, false, false) != null
                                             && TryFindTile(out _);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var faction = parms.faction ?? Find.FactionManager.RandomAlliedFaction(false, false, false);

            if (faction == null)
            {
                Log.ErrorOnce("MFI: No allied faction found, but event was forced. Using random faction.", 40830425);
                faction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
            }

            if (!TryFindTile(out var tile))
            {
                return false;
            }

            var site = SiteMaker.MakeSite(MFI_DefOf.MFI_HuntersLodgePart,
                tile, faction, false);

            if (site == null)
            {
                return false;
            }

            var randomInRange = TimeoutDaysRange.RandomInRange;

            site.Tile = tile;
            site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
            site.SetFaction(faction);
            site.customLabel = site.def.LabelCap + site.parts.First(x => x.def == MFI_DefOf.MFI_HuntersLodgePart).def
                .Worker.GetPostProcessedThreatLabel(site, site.parts.FirstOrDefault());

            Find.WorldObjects.Add(site);

            var text = string.Format(def.letterText,
                    faction,
                    faction.def.leaderTitle,
                    GetDescriptionDialogue(site, site.parts.FirstOrDefault()),
                    randomInRange)
                .CapitalizeFirst();

            Find.LetterStack.ReceiveLetter(def.letterLabel, text, def.letterDef, site);
            return true;
        }

        public static string GetDescriptionDialogue(Site site, SitePart sitePart)
        {
            if (sitePart != null && !sitePart.def.defaultHidden)
            {
                return sitePart.def.description;
            }

            return "HiddenOrNoSitePartDescription".Translate();
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true, false);
        }
    }
}
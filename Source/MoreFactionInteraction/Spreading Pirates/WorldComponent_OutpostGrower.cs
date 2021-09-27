using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction
{
    internal class WorldComponent_OutpostGrower : WorldComponent
    {
        private List<ChoiceLetter> choiceLetters = new List<ChoiceLetter>();

        public WorldComponent_OutpostGrower(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % 350 != 0)
            {
                return;
            }

            TickLetters();
            MakeHomeForSquatters();
            FindSitesToUpgrade();
        }

        private static void FindSitesToUpgrade()
        {
            //get settlements to upgrade. These shouldn't include temp generated or event maps -- preferably only the outposts this spawned by this mod
            //ideally I'd add some specific Component to each outpost (as a unique identifier and maybe even as the thing that makes em upgrade), but for the moment that's not needed.

            var sites = from site in Find.WorldObjects.Sites
                where site.Faction != null
                      && site.Faction.HostileTo(Faction.OfPlayer)
                      && site.Faction.def.permanentEnemy && !site.Faction.def.hidden
                      && !site.Faction.defeated
                      && (!site.HasMap || site.ShouldRemoveMapNow(out _))
                      && site.parts.Any(x => x.def == SitePartDefOf.Outpost)
                      && !site.GetComponent<TimeoutComp>().Active
                select site;

            foreach (var current in sites)
            {
                if (current.creationGameTicks + MoreFactionInteraction_Settings.ticksToUpgrade >
                    Find.TickManager.TicksGame)
                {
                    continue;
                }

                UpgradeSiteToSettlement(current);
                break;
            }
        }

        private static void MakeHomeForSquatters()
        {
            var abandoned = Find.WorldObjects.AllWorldObjects
                .Where(wObject => wObject.def == WorldObjectDefOf.AbandonedSettlement ||
                                  wObject.def == WorldObjectDefOf.DestroyedSettlement);

            foreach (var wObject in abandoned)
            {
                if (wObject.Tile - (wObject.ID % 10) == 0)
                {
                    continue;
                }

                if (!(Find.TickManager.TicksGame > wObject.creationGameTicks +
                    (GenDate.TicksPerYear * (1 + Mathf.Clamp01((float)wObject.ID / 10)))))
                {
                    continue;
                }

                if (!Rand.Chance(0.000175f))
                {
                    continue;
                }

                var settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(Find.FactionManager.AllFactionsVisible
                    .Where(x => x.def.settlementGenerationWeight > 0f).RandomElement());
                settlement.Tile = wObject.Tile;
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                Find.WorldObjects.Remove(wObject);
                Find.WorldObjects.Add(settlement);
                Find.LetterStack.ReceiveLetter("MFI_LetterLabelSquatters".Translate(),
                    "MFI_LetterSquatters".Translate(settlement.Faction, settlement.Name),
                    LetterDefOf.NeutralEvent, settlement);
            }
        }

        private static void UpgradeSiteToSettlement(Site toUpgrade)
        {
            var factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            factionBase.SetFaction(toUpgrade.Faction);
            factionBase.Tile = toUpgrade.Tile;
            factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
            Find.WorldObjects.Remove(toUpgrade);
            Find.WorldObjects.Add(factionBase);
            Find.LetterStack.ReceiveLetter(
                "MFI_LetterLabelBanditOutpostUpgraded".Translate(),
                "MFI_LetterBanditOutpostUpgraded".Translate(factionBase.Faction.Name),
                LetterDefOf.NeutralEvent,
                factionBase,
                toUpgrade.Faction);
        }

        private void TickLetters()
        {
            foreach (var letter in choiceLetters)
            {
                if (letter == null)
                {
                    choiceLetters.Remove(null);
                    break;
                }

                if (Find.TickManager.TicksGame <= letter.disappearAtTick)
                {
                    continue;
                }

                if (letter is ChoiceLetter_ExtortionDemand { completed: false })
                {
                    Find.LetterStack.ReceiveLetter(letter);
                    letter.OpenLetter();
                }

                choiceLetters.Remove(letter);
                break;
            }
        }

        public void Registerletter(ChoiceLetter choiceLetter)
        {
            choiceLetters.Add(choiceLetter);
        }

        public override void ExposeData()
        {
            //this is where I store letters, because RimWorld just goes and deletes them.
            base.ExposeData();
            Scribe_Collections.Look(ref choiceLetters, "MFI_ChoiceLetters", LookMode.Reference);
        }
    }
}
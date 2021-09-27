using System;
using System.Collections.Generic;
using MoreFactionInteraction.More_Flavour;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    [Obsolete]
    public class ChoiceLetter_MysticalShaman : ChoiceLetter
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(5, 15);
        public Faction faction;
        public int fee;
        public Map map;
        public int tile;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                }
                else
                {
                    var accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            var mysticalShaman =
                                (MysticalShaman)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_MysticalShaman);
                            mysticalShaman.Tile = tile;
                            mysticalShaman.SetFaction(faction);
                            var randomInRange = TimeoutDaysRange.RandomInRange;
                            mysticalShaman.GetComponent<TimeoutComp>()
                                .StartTimeout(randomInRange * GenDate.TicksPerDay);
                            Find.WorldObjects.Add(mysticalShaman);

                            TradeUtility.LaunchSilver(map, fee);
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    if (!TradeUtility.ColonyHasEnoughSilver(map, fee))
                    {
                        accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
                    }

                    yield return accept;

                    var reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () => Find.LetterStack.RemoveLetter(this),
                        resolveTree = true
                    };
                    yield return reject;
                    yield return Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(map);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "MFI_Shaman_Map");
            Scribe_References.Look(ref faction, "MFI_Shaman_Faction");
            Scribe_Values.Look(ref tile, "MFI_ShamanTile");
            Scribe_Values.Look(ref fee, "MFI_ShamanFee");
        }
    }
}
using Verse;

namespace MoreFactionInteraction;

internal class Dialog_MFIDebugActionMenu : Dialog_DebugActionsMenu
{
    protected override void DoListingItems()
    {
        base.DoListingItems();
#if DEBUG
            if (!WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }

            DoGap();
            DoLabel("Tools - MFI");

            DebugToolWorld_NewTmp("Spawn pirate base", () =>
                {
                    var tile = GenWorld.MouseTile();

                    if (tile < 0 || Find.World.Impassable(tile))
                    {
                        Messages.Message("Impassable", MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        var faction = (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden
                                  && !x.defeated
                                  && !x.IsPlayer
                                  && x.HostileTo(Faction.OfPlayer)
                                  && x.def.permanentEnemy
                            select x).First();

                        var factionBase =
                            (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        factionBase.SetFaction(faction);
                        factionBase.Tile = tile;
                        factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
                        Find.WorldObjects.Add(factionBase);
                    }
                },
                false
            );

            DebugToolWorld_NewTmp("Test annual Expo",
                new AnnualExpoDialogue(null, null, null, Find.FactionManager.RandomAlliedFaction())
                    .DebugLogChances, false);
#endif
    }
}
using System.Collections.Generic;
using System.Linq;
using MoreFactionInteraction.General;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar;

public class IncidentWorker_WoundedCombatants : IncidentWorker
{
    private readonly IntRange pawnstoSpawn = new(4, 6);

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
                                         && FindAlliedWarringFaction(out _)
                                         && CommsConsoleUtility.PlayerHasPoweredCommsConsole((Map)parms.target)
                                         && DropCellFinder.TryFindRaidDropCenterClose(out _,
                                             (Map)parms.target);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!DropCellFinder.TryFindRaidDropCenterClose(out var dropSpot, (Map)parms.target))
        {
            return false;
        }

        if (!FindAlliedWarringFaction(out var faction))
        {
            return false;
        }

        if (faction == null)
        {
            return false;
        }

        var bamboozle = false;
        var arrivalText = string.Empty;
        var factionGoodWillLoss = MFI_DiplomacyTunings
            .GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange / 2;

        var raidParms =
            StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, (Map)parms.target);
        raidParms.forced = true;
        raidParms.faction = faction.EnemyInFactionWar();
        raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        raidParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
        raidParms.spawnCenter = dropSpot;

        if (faction.EnemyInFactionWar().def.techLevel >= TechLevel.Industrial
            && faction.EnemyInFactionWar().RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile)
        {
            bamboozle = Rand.Chance(0.25f);
        }

        if (bamboozle)
        {
            arrivalText = string.Format(raidParms.raidArrivalMode.textEnemy, raidParms.faction.def.pawnsPlural,
                raidParms.faction.Name);
        }

        //get combat-pawns to spawn.
        var defaultPawnGroupMakerParms =
            IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms);
        defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(
            defaultPawnGroupMakerParms.points, raidParms.raidArrivalMode, raidParms.raidStrategy,
            defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, parms.target);
        IEnumerable<PawnKindDef> pawnKinds =
            PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms).ToList();
        var pawnlist = new List<Thing>();

        for (var i = 0; i < pawnstoSpawn.RandomInRange; i++)
        {
            var request = new PawnGenerationRequest(pawnKinds.RandomElement(), faction, allowDowned: true,
                allowDead: true, mustBeCapableOfViolence: true);
            var woundedCombatant = PawnGenerator.GeneratePawn(request);
            woundedCombatant.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
            var weapon = Rand.Bool
                ? DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsWeaponUsingProjectiles).RandomElement()
                : null;

            var usedWeaponDef = weapon;
            var damageDef =
                usedWeaponDef?.Verbs?.First()?.defaultProjectile?.projectile
                    ?.damageDef; //null? check? All? THE? THINGS!!!!?
            if (usedWeaponDef != null && damageDef == null)
            {
                usedWeaponDef = null;
            }

            CustomFaction_HealthUtility.DamageUntilDownedWithSpecialOptions(woundedCombatant,
                true, damageDef, usedWeaponDef);
            //todo: maybe add some story logging.
            pawnlist.Add(woundedCombatant);
        }

        string initialMessage = "MFI_WoundedCombatant".Translate(faction.Name);
        var diaNode = new DiaNode(initialMessage);

        var diaOptionOk = new DiaOption("OK".Translate()) { resolveTree = true };

        var diaOptionAccept = new DiaOption("Accept".Translate())
        {
            action = () =>
            {
                if (bamboozle)
                {
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(raidParms);
                }
                else
                {
                    var intVec = IntVec3.Invalid;

                    var allBuildingsColonist = ((Map)parms.target).listerBuildings.allBuildingsColonist
                        .Where(x => x.def.thingClass == typeof(Building_Bed)).ToList();
                    foreach (var building in allBuildingsColonist)
                    {
                        if (DropCellFinder.TryFindDropSpotNear(building.Position, (Map)parms.target,
                                out intVec, false, false))
                        {
                            break;
                        }
                    }

                    if (intVec == IntVec3.Invalid)
                    {
                        intVec = DropCellFinder.RandomDropSpot((Map)parms.target);
                    }

                    DropPodUtility.DropThingsNear(intVec, (Map)parms.target, pawnlist, 180, leaveSlag: true,
                        canRoofPunch: false);
                    Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(faction);
                }
            }
        };
        string bamboozledAndAmbushed = "MFI_WoundedCombatantAmbush".Translate(faction, arrivalText);
        string commanderGreatful = "MFI_WoundedCombatantGratitude".Translate();
        var acceptDiaNode = new DiaNode(bamboozle ? bamboozledAndAmbushed : commanderGreatful);
        diaOptionAccept.link = acceptDiaNode;
        diaNode.options.Add(diaOptionAccept);
        acceptDiaNode.options.Add(diaOptionOk);

        var diaOptionRejection = new DiaOption("MFI_Reject".Translate())
        {
            action = () =>
            {
                if (bamboozle)
                {
                    Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(faction);
                }
                else
                {
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, factionGoodWillLoss, false);
                }
            }
        };
        string rejectionResponse = "MFI_WoundedCombatantRejected".Translate(faction.Name, factionGoodWillLoss);
        string bamboozlingTheBamboozler = "MFI_WoundedCombatantAmbushAvoided".Translate();
        var rejectionDiaNode = new DiaNode(bamboozle ? bamboozlingTheBamboozler : rejectionResponse);
        diaOptionRejection.link = rejectionDiaNode;
        diaNode.options.Add(diaOptionRejection);
        rejectionDiaNode.options.Add(diaOptionOk);

        string title = "MFI_WoundedCombatantTitle".Translate(((Map)parms.target).Parent.Label);
        Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, faction, true, true, title));
        Find.Archive.Add(new ArchivedDialog(diaNode.text, title, faction));
        return true;
    }

    /// <summary>
    ///     Find warring allied faction that can send drop pods.
    /// </summary>
    /// <param name="faction"></param>
    /// <returns></returns>
    private bool FindAlliedWarringFaction(out Faction faction)
    {
        faction = null;

        if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing)
        {
            return false;
        }

        return Find.World.GetComponent<WorldComponent_MFI_FactionWar>().AllFactionsInVolvedInWar
            .Where(f => f.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Ally
                        && f.def.techLevel >= TechLevel.Industrial)
            .TryRandomElementByWeight(f => f.def.RaidCommonalityFromPoints(600f),
                out faction);
    }
}
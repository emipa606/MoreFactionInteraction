using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MoreFactionInteraction.World_Incidents;

public class IncidentWorker_HerdMigration_Ambush : IncidentWorker_Ambush
{
    private PawnKindDef pawnKindDef = PawnKindDefOf.Thrumbo;

    public override float BaseChanceThisGame => 0f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && Current.Game.Maps.Any(x => x.Tile == parms.target.Tile) && parms.forced;
    }

    protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
    {
        var map = parms.target as Map;
        TryFindEndCell(map, generatedPawns, out var end);
        if (!end.IsValid && CellFinder.TryFindRandomPawnExitCell(generatedPawns[0], out var intVec3))
        {
            end = intVec3;
        }

        return new LordJob_ExitMapNear(end, LocomotionUrgency.Walk);
    }

    protected override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        if (parms.target is not Map map)
        {
            pawnKindDef = PawnKindDefOf.Thrumbo; //something went really wrong. Let's uh.. brush it under the rug.
        }
        else if (Find.WorldObjects.SiteAt(map.Tile) is { } site)
        {
            pawnKindDef = site.parts.First(x => x.def == MFI_DefOf.MFI_HuntersLodgePart).parms?.animalKind ??
                          PawnKindDefOf.Thrumbo;
        }

        var num = new IntRange(30, 50).RandomInRange;

        var list = new List<Pawn>();
        for (var i = 0; i < num; i++)
        {
            var request = new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer,
                parms.target.Tile);
            var item = PawnGenerator.GeneratePawn(request);
            list.Add(item);
        }

        return list;
    }

    protected override string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
    {
        return string.Format(def.letterLabel, pawnKindDef.GetLabelPlural().CapitalizeFirst());
    }

    protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
    {
        return string.Format(def.letterText, pawnKindDef.GetLabelPlural());
    }

    private static void TryFindEndCell(Map map, List<Pawn> generatedPawns, out IntVec3 end)
    {
        end = IntVec3.Invalid;
        for (var i = 0; i < 8; i++)
        {
            var intVec3 = generatedPawns[i].Position;
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    x => map.reachability.CanReach(intVec3, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors,
                        Danger.Deadly), map, CellFinder.EdgeRoadChance_Ignore, out var intVec))
            {
                break;
            }

            if (!end.IsValid || intVec.DistanceToSquared(intVec3) > end.DistanceToSquared(intVec3))
            {
                end = intVec;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    internal class WorstCaravanPawnUtility
    {
        private static readonly Dictionary<Pawn, int> tempPawns = new Dictionary<Pawn, int>();

        private static float HandCoverageAbsWithChildren => ThingDefOf.Human.race.body
            .GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;

        public static Pawn FindSickestPawn(Caravan caravan)
        {
            tempPawns.Clear();
            //Muffalo 1 deserves a chance to get healed too.
            foreach (var pawn in caravan.PawnsListForReading)
            {
                tempPawns.Add(pawn,
                    CalcHealthThreatenedScore(pawn) / (pawn.RaceProps.Humanlike ? 1 : 4));
            }

            tempPawns.RemoveAll(x => x.Value == 0);
            return tempPawns.FirstOrDefault(x => x.Value.Equals(tempPawns.Values.Max())).Key;
        }

        //Taken from CompUseEffect_FixWorstHealthCondition, with a bit of Resharper cleanup to stop my eyes bleeding.
        private static int CalcHealthThreatenedScore(Pawn usedBy)
        {
            var hediff = FindLifeThreateningHediff(usedBy);
            if (hediff != null)
            {
                return 8192;
            }

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(usedBy) < 2500)
            {
                var hediff2 = FindMostBleedingHediff(usedBy);
                if (hediff2 != null)
                {
                    return 4096;
                }
            }

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                var hediffInjury = FindPermanentInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                if (hediffInjury != null)
                {
                    return 2048;
                }
            }

            var bodyPartRecord = FindBiggestMissingBodyPart(usedBy, HandCoverageAbsWithChildren);
            if (bodyPartRecord != null)
            {
                return 1024;
            }

            var hediffInjury2 = FindPermanentInjury(usedBy,
                usedBy.health.hediffSet.GetNotMissingParts()
                    .Where(x => x.def == BodyPartDefOf.Eye));
            if (hediffInjury2 != null)
            {
                return 512;
            }

            var hediff3 = FindImmunizableHediffWhichCanKill(usedBy);
            if (hediff3 != null)
            {
                return 255;
            }

            var hediff4 = FindNonInjuryMiscBadHediff(usedBy, true);
            if (hediff4 != null)
            {
                return 128;
            }

            var hediff5 = FindNonInjuryMiscBadHediff(usedBy, false);
            if (hediff5 != null)
            {
                return 64;
            }

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                var hediffInjury3 = FindInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                if (hediffInjury3 != null)
                {
                    return 32;
                }
            }

            var bodyPartRecord2 = FindBiggestMissingBodyPart(usedBy);
            if (bodyPartRecord2 != null)
            {
                return 16;
            }

            var hediffAddiction = FindAddiction(usedBy);
            if (hediffAddiction != null)
            {
                return 8;
            }

            var hediffInjury4 = FindPermanentInjury(usedBy);
            if (hediffInjury4 != null)
            {
                return 4;
            }

            var hediffInjury5 = FindInjury(usedBy);
            if (hediffInjury5 != null)
            {
                return 2;
            }

            return 0;
        }

        private static Hediff FindLifeThreateningHediff(Pawn pawn)
        {
            Hediff hediff = null;
            var num = -1f;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var current in hediffs)
            {
                if (!current.Visible || !current.def.everCurableByItem)
                {
                    continue;
                }

                if (current.FullyImmune())
                {
                    continue;
                }

                var flag = current.CurStage != null && current.CurStage.lifeThreatening;
                var flag2 = current.def.lethalSeverity >= 0f &&
                            current.Severity / current.def.lethalSeverity >= 0.8f;
                if (!flag && !flag2)
                {
                    continue;
                }

                var num2 = current.Part?.coverageAbsWithChildren ?? 999f;
                if (hediff != null && !(num2 > num))
                {
                    continue;
                }

                hediff = current;
                num = num2;
            }

            return hediff;
        }

        private static Hediff FindMostBleedingHediff(Pawn pawn)
        {
            var num = 0f;
            Hediff hediff = null;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var current in hediffs)
            {
                if (!current.Visible || !current.def.everCurableByItem)
                {
                    continue;
                }

                var bleedRate = current.BleedRate;
                if (!(bleedRate > 0f) || !(bleedRate > num) && hediff != null)
                {
                    continue;
                }

                num = bleedRate;
                hediff = current;
            }

            return hediff;
        }

        private static Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
        {
            Hediff hediff = null;
            var num = -1f;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var current in hediffs)
            {
                if (!current.Visible || !current.def.everCurableByItem)
                {
                    continue;
                }

                if (current.TryGetComp<HediffComp_Immunizable>() == null)
                {
                    continue;
                }

                if (current.FullyImmune())
                {
                    continue;
                }

                if (!CanEverKill(current))
                {
                    continue;
                }

                var severity = current.Severity;
                if (hediff != null && !(severity > num))
                {
                    continue;
                }

                hediff = current;
                num = severity;
            }

            return hediff;
        }

        private static Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
        {
            Hediff hediff = null;
            var num = -1f;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var current in hediffs)
            {
                if (!current.Visible || !current.def.isBad || !current.def.everCurableByItem)
                {
                    continue;
                }

                if (current is Hediff_Injury || current is Hediff_MissingPart || current is Hediff_Addiction ||
                    current is Hediff_AddedPart)
                {
                    continue;
                }

                if (onlyIfCanKill && !CanEverKill(current))
                {
                    continue;
                }

                var num2 = current.Part?.coverageAbsWithChildren ?? 999f;
                if (hediff != null && !(num2 > num))
                {
                    continue;
                }

                hediff = current;
                num = num2;
            }

            return hediff;
        }

        private static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (var current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (!(current.Part.coverageAbsWithChildren >= minCoverage))
                {
                    continue;
                }

                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current.Part))
                {
                    continue;
                }

                if (bodyPartRecord == null || current.Part.coverageAbsWithChildren >
                    bodyPartRecord.coverageAbsWithChildren)
                {
                    bodyPartRecord = current.Part;
                }
            }

            return bodyPartRecord;
        }

        private static Hediff_Addiction FindAddiction(Pawn pawn)
        {
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var current in hediffs)
            {
                if (current is Hediff_Addiction {Visible: true} hediffAddiction &&
                    hediffAddiction.def.everCurableByItem)
                {
                    return hediffAddiction;
                }
            }

            return null;
        }

        private static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediffInjury = null;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var currentHediff in hediffs)
            {
                if (currentHediff is not Hediff_Injury {Visible: true} hediffInjury2 || !hediffInjury2.IsPermanent() ||
                    !hediffInjury2.def.everCurableByItem)
                {
                    continue;
                }

                if (allowedBodyParts != null && !allowedBodyParts.Contains(hediffInjury2.Part))
                {
                    continue;
                }

                if (hediffInjury == null || hediffInjury2.Severity > hediffInjury.Severity)
                {
                    hediffInjury = hediffInjury2;
                }
            }

            return hediffInjury;
        }

        private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediffInjury = null;
            var hediffs = pawn.health.hediffSet.hediffs;
            foreach (var hediff in hediffs)
            {
                if (hediff is not Hediff_Injury {Visible: true} hediffInjury2 || !hediffInjury2.def.everCurableByItem)
                {
                    continue;
                }

                if (allowedBodyParts != null && !allowedBodyParts.Contains(hediffInjury2.Part))
                {
                    continue;
                }

                if (hediffInjury == null || hediffInjury2.Severity > hediffInjury.Severity)
                {
                    hediffInjury = hediffInjury2;
                }
            }

            return hediffInjury;
        }

        private static bool CanEverKill(Hediff hediff)
        {
            if (hediff.def.stages == null)
            {
                return hediff.def.lethalSeverity >= 0f;
            }

            if (Enumerable.Any(hediff.def.stages, t => t.lifeThreatening))
            {
                return true;
            }

            return hediff.def.lethalSeverity >= 0f;
        }
    }
}
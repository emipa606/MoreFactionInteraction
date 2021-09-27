using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class CustomFaction_HealthUtility
    {
        public static void DamageUntilDownedWithSpecialOptions(Pawn p, bool allowBleedingWounds = true,
            DamageDef damageDef = null, ThingDef weapon = null)
        {
            if (p.health.Downed)
            {
                return;
            }

            if (p.apparel != null)
            {
                foreach (var apparel in p.apparel.WornApparel)
                {
                    if (!apparel.def.StatBaseDefined(StatDefOf.SmokepopBeltRadius))
                    {
                        continue;
                    }

                    p.apparel.Remove(apparel);
                    break;
                }
            }

            var hediffSet = p.health.hediffSet;
            p.health.forceIncap = true;
            IEnumerable<BodyPartRecord> source = HittablePartsViolence(hediffSet).Where(x => !p.health.hediffSet
                .hediffs
                .Any(y => y.Part == x && y.CurStage?.partEfficiencyOffset < 0f)).ToList();

            var num = 0;
            while (num < 300 && !p.Downed && source.Any())
            {
                num++;
                var bodyPartRecord = source.RandomElementByWeight(x => x.coverageAbs);
                var num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
                if (num2 < 8)
                {
                    continue;
                }

                if (bodyPartRecord.depth == BodyPartDepth.Outside)
                {
                    if (!allowBleedingWounds && bodyPartRecord.def.bleedRate > 0f)
                    {
                        damageDef = DamageDefOf.Blunt;
                    }
                    else
                    {
                        if (damageDef == null || damageDef == DamageDefOf.Flame)
                        {
                            damageDef = HealthUtility.RandomViolenceDamageType();
                        }
                    }
                }
                else
                {
                    damageDef = DamageDefOf.Blunt;
                }

                var num3 = Rand.RangeInclusive(Mathf.RoundToInt(num2 * 0.65f), num2);
                var hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
                if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, num3))
                {
                    continue;
                }

                var def = damageDef;
                var amount = (float)num3;
                var armorPenetration = 999f;
                var dinfo = new DamageInfo(def, amount, armorPenetration, -1f, null, bodyPartRecord, weapon);
                dinfo.SetAllowDamagePropagation(false);
                p.TakeDamage(dinfo);
            }

            if (p.Dead)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
                foreach (var hediff in p.health.hediffSet.hediffs)
                {
                    stringBuilder.AppendLine("   -" + hediff);
                }

                Log.Error(stringBuilder.ToString());
            }

            p.health.forceIncap = false;
        }

        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return bodyModel.GetNotMissingParts()
                .Where(x => x.depth == BodyPartDepth.Outside
                            || x.depth == BodyPartDepth.Inside
                            && x.def.IsSolid(x, bodyModel.hediffs));
        }
    }
}
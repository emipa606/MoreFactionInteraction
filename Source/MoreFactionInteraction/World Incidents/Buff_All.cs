using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    public abstract class Buff : IExposable, IEquatable<Buff>
    {
        public bool Active;

        public virtual TechLevel MinTechLevel()
        {
            return TechLevel.Undefined;
        }

        public abstract void Apply();
        public abstract void ExposeData();
        public abstract string Description();
        public abstract ThingDef RelevantThingDef();

        public bool Equals(Buff obj)
        {
            return obj != null && obj.GetType() == GetType();
        }

        public override int GetHashCode() //oof.
        {
            return base.GetHashCode();
        }
    }

    public class Buff_Pemmican : Buff
    {
        public override void Apply()
        {
            Active = true;
            ThingDefOf.Pemmican.comps.RemoveAll(x => x is CompProperties_Rottable);
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_Pemmican());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_Pemmican");
        }

        public override ThingDef RelevantThingDef()
        {
            return ThingDefOf.Pemmican;
        }

        public override string Description()
        {
            return "MFI_buffPemmican".Translate();
        }
    }

    public class Buff_Emanator : Buff
    {
        public override void Apply()
        {
            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = 6f;
            ThingDefOf.PsychicEmanator.specialDisplayRadius = 20f;
            var power = (CompProperties_Power)ThingDefOf.PsychicEmanator.comps.FirstOrDefault(x => x is CompProperties_Power);

            if (power != null)
            {
                power.basePowerConsumption = 350f;
            }

            Active = true;
        }

        public override TechLevel MinTechLevel()
        {
            return TechLevel.Industrial;
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_Emanator());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_Emanator");
        }

        public override ThingDef RelevantThingDef()
        {
            return ThingDefOf.PsychicEmanator;
        }

        public override string Description()
        {
            return "MFI_buffEmanator".Translate(ThingDefOf.PsychicEmanator.label);
        }
    }

    public class Buff_PsychTea : Buff
    {
        public override void Apply()
        {
            var giveHediff = (IngestionOutcomeDoer_GiveHediff)ThingDef.Named("PsychiteTea").ingestible.outcomeDoers.FirstOrDefault(x => x is IngestionOutcomeDoer_GiveHediff);
            if (giveHediff != null)
            {
                giveHediff.severity = 1f;
            }

            Active = true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_PsychTea");
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_PsychTea());
        }

        public override ThingDef RelevantThingDef()
        {
            return DefDatabase<ThingDef>.GetNamed("PsychiteTea");
        }

        public override string Description()
        {
            return "MFI_buffPsychite".Translate(ThingDef.Named("PsychiteTea").label);
        }
    }

    public class Buff_Chemfuel : Buff
    {
        public override void Apply()
        {
            var spawner = (CompProperties_Spawner)ThingDefOf.InfiniteChemreactor.comps.FirstOrDefault(x => x is CompProperties_Spawner);

            if (spawner != null)
            {
                spawner.spawnIntervalRange.min = (int)(spawner.spawnIntervalRange.min * 0.9f);
            }

            Active = true;
        }

        public override TechLevel MinTechLevel()
        {
            return TechLevel.Industrial;
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_Chemfuel());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_ChemFuel");
        }

        public override ThingDef RelevantThingDef()
        {
            return ThingDefOf.InfiniteChemreactor;
        }

        public override string Description()
        {
            return "MFI_buffChemfuel".Translate(ThingDefOf.InfiniteChemreactor.label, ThingDefOf.InfiniteChemreactor.GetCompProperties<CompProperties_Spawner>().thingToSpawn.label);
        }
    }
}

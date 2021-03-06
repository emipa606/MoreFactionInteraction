﻿using System.Linq;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    public class Buff_Emanator : Buff
    {
        public override void Apply()
        {
            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = 6f;
            ThingDefOf.PsychicEmanator.specialDisplayRadius = 20f;
            var power = (CompProperties_Power) ThingDefOf.PsychicEmanator.comps.FirstOrDefault(x =>
                x is CompProperties_Power);

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
}
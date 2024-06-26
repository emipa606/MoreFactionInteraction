﻿using System.Linq;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class Buff_Emanator : Buff
{
    public override void Apply()
    {
        try
        {
            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = 6f;
            ThingDefOf.PsychicEmanator.specialDisplayRadius = 20f;
            ThingDefOf.PsychicEmanator.GetCompProperties<CompProperties_Power>().basePowerConsumption = 350f;
            Active = true;
        }
        catch
        {
            // ignored
        }
    }

    public override TechLevel MinTechLevel()
    {
        return TechLevel.Industrial;
    }

    public static void Register()
    {
        if (DefDatabase<ThingDef>.GetNamedSilentFail("PsychicEmanator") == null)
        {
            return;
        }

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
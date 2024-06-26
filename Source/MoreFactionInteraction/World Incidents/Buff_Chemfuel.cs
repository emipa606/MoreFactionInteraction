﻿using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class Buff_Chemfuel : Buff
{
    private static readonly ThingDef infiniteChemreactor = ThingDef.Named("InfiniteChemreactor");

    public override void Apply()
    {
        var spawner =
            (CompProperties_Spawner)infiniteChemreactor.comps.FirstOrDefault(x =>
                x is CompProperties_Spawner);

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
        if (DefDatabase<ThingDef>.GetNamedSilentFail("InfiniteChemreactor") == null)
        {
            return;
        }

        Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_Chemfuel());
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref Active, "MFI_Buff_ChemFuel");
    }

    public override ThingDef RelevantThingDef()
    {
        return infiniteChemreactor;
    }

    public override string Description()
    {
        return "MFI_buffChemfuel".Translate(infiniteChemreactor.label,
            infiniteChemreactor.GetCompProperties<CompProperties_Spawner>().thingToSpawn.label);
    }
}
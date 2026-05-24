using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class Buff_Emanator : Buff
{
    private static readonly FieldInfo basePowerConsumptionFieldInfo =
        AccessTools.Field(typeof(CompProperties_Power), "basePowerConsumption");

    private static readonly float defaultMoodEffect = ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect;
    private static readonly float defaultDisplayRadius = ThingDefOf.PsychicEmanator.specialDisplayRadius;
    private static readonly float defaultPowerConsumption =
        (float)basePowerConsumptionFieldInfo.GetValue(ThingDefOf.PsychicEmanator.GetCompProperties<CompProperties_Power>());

    public static void Reset()
    {
        try
        {
            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = defaultMoodEffect;
            ThingDefOf.PsychicEmanator.specialDisplayRadius = defaultDisplayRadius;
            basePowerConsumptionFieldInfo.SetValue(ThingDefOf.PsychicEmanator.GetCompProperties<CompProperties_Power>(),
                defaultPowerConsumption);
        }
        catch
        {
            // ignored
        }
    }

    public override void Apply()
    {
        try
        {
            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = 6f;
            ThingDefOf.PsychicEmanator.specialDisplayRadius = 20f;
            basePowerConsumptionFieldInfo.SetValue(ThingDefOf.PsychicEmanator.GetCompProperties<CompProperties_Power>(),
                350f);
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
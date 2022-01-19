using System;
using MoreFactionInteraction.General;
using MoreFactionInteraction.More_Flavour;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

public class EventRewardWorker_ScienceFaire : EventRewardWorker
{
    private readonly EventDef eventDef = MFI_DefOf.MFI_ScienceFaire;

    public override Predicate<ThingDef> ValidatorFirstOther => x => x == ThingDefOf.TechprofSubpersonaCore;

    public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator,
        ThingSetMakerDef thingSetMakerDef)
    {
        return GenerateBuff(thingSetMakerDef.root.fixedParams.techLevel.GetValueOrDefault(), pawn, caravan,
            globalValidator, thingSetMakerDef);
    }

    private string GenerateBuff(TechLevel desiredTechLevel, Pawn pawn, Caravan caravan,
        Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
    {
        string reward;

        var buff = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>()
            .ApplyRandomBuff(x => x.MinTechLevel() >= desiredTechLevel && !x.Active);

        if (buff == null)
        {
            buff = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff(x => !x.Active);
        }

        if (buff != null)
        {
            reward = buff.Description() + MaybeTheySuckAndDontHaveItYet(buff, caravan, thingSetMakerDef);
        }
        else
        {
            reward = base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
        }

        return reward;
    }

    private string MaybeTheySuckAndDontHaveItYet(Buff buff, Caravan caravan,
        ThingSetMakerDef thingSetMakerDef)
    {
        if (thingSetMakerDef != eventDef.rewardFirstPlace ||
            MFI_Utilities.CaravanOrRichestColonyHasAnyOf(buff.RelevantThingDef(), caravan, out _))
        {
            return string.Empty;
        }

        var thing = ThingMaker.MakeThing(buff.RelevantThingDef());
        thing.stackCount = Mathf.Min(thing.def.stackLimit, 75); //suck it, stackXXL users.
        CaravanInventoryUtility.GiveThing(caravan, thing);
        var anReward = Find.ActiveLanguageWorker.WithDefiniteArticlePostProcessed(thing.Label);
        return "\n\n" + "MFI_SinceYouSuckAndDidntHaveIt".Translate(anReward);
    }
}
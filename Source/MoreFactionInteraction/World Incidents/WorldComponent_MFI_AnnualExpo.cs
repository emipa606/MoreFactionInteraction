﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour;

public class WorldComponent_MFI_AnnualExpo : WorldComponent
{
    private readonly IncidentDef incident = MFI_DefOf.MFI_AnnualExpo;
    private readonly float intervalDays = 60f;
    private List<Buff> activeBuffList = new List<Buff>();

    public Dictionary<EventDef, int> events = new Dictionary<EventDef, int>
    {
        { MFI_DefOf.MFI_GameOfUrComp, 0 },
        { MFI_DefOf.MFI_ShootingComp, 0 },
        { MFI_DefOf.MFI_CulturalSwap, 0 },
        { MFI_DefOf.MFI_ScienceFaire, 0 },
        { MFI_DefOf.MFI_AcousticShow, 0 }
    };

    private float occuringTick;
    public int timesHeld;

    public WorldComponent_MFI_AnnualExpo(World world) : base(world)
    {
    }

    private List<Buff> ActiveBuffsList => activeBuffList;

    public int TimesHeld => timesHeld + Rand.RangeInclusiveSeeded(
        (int)PawnKindDefOf.Muffalo.race.race.lifeExpectancy,
        (int)PawnKindDefOf.Thrumbo.race.race.lifeExpectancy,
        (int)(Rand.ValueSeeded(Find.World.ConstantRandSeed) * 1000));

    public bool BuffedEmanator => ActiveBuffsList.Find(x => x is Buff_Emanator)?.Active ?? false; //used by patches.

    private float IntervalTicks => GenDate.TicksPerDay * intervalDays;

    public void RegisterBuff(Buff buff)
    {
        if (!ActiveBuffsList.Contains(buff))
        {
            ActiveBuffsList.Add(buff);
        }
    }

    public Buff ApplyRandomBuff(Predicate<Buff> validator)
    {
        if (ActiveBuffsList.Where(x => validator(x)).TryRandomElement(out var result))
        {
            result.Apply();
        }

        return result;
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();

        Buff_Chemfuel.Register();
        Buff_Emanator.Register();
        Buff_Pemmican.Register();
        Buff_PsychTea.Register();

        foreach (var item in ActiveBuffsList.Where(x => x.Active))
        {
            item.Apply();
        }

        if (occuringTick < 4f && timesHeld == 0) // I picked 4 in case of extraordinarily large values of 0.
        {
            occuringTick = GenTicks.TicksAbs +
                           new FloatRange(GenDate.TicksPerDay * 45, GenDate.TicksPerYear).RandomInRange;
        }
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (Find.AnyPlayerHomeMap == null)
        {
            return;
        }

        if (!(Find.TickManager.TicksGame >= occuringTick))
        {
            return;
        }

        var parms = StorytellerUtility.DefaultParmsNow(incident.category,
            Find.Maps.Where(x => x.IsPlayerHome).RandomElement());

        if (incident.Worker.TryExecute(parms))
        {
            occuringTick += IntervalTicks;
        }
        else
        {
            occuringTick += GenDate.TicksPerDay;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref occuringTick, "MFI_occuringTick", 0f, true);
        Scribe_Collections.Look(ref events, "MFI_Events");
        Scribe_Collections.Look(ref activeBuffList, "MFI_buffList");
        Scribe_Values.Look(ref timesHeld, "MFI_AnnualExpoTimesHeld");
    }
}
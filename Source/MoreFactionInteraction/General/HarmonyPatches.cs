using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Fluffy_Relations;
using HarmonyLib;
using MoreFactionInteraction.More_Flavour;
using MoreFactionInteraction.MoreFactionWar;
using MoreFactionInteraction.World_Incidents;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    //resources must be loaded in cctor
    public static readonly Texture2D setPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow");

    private static readonly SimpleCurve WealthSilverIncreaseDeterminationCurve = new SimpleCurve
    {
        new CurvePoint(0, 0.8f),
        new CurvePoint(10000, 1),
        new CurvePoint(75000, 2),
        new CurvePoint(300000, 4),
        new CurvePoint(1000000, 6f),
        new CurvePoint(2000000, 7f)
    };

    private static readonly SimpleCurve WealthQualityDeterminationCurve = new SimpleCurve
    {
        new CurvePoint(0, 1),
        new CurvePoint(10000, 1.5f),
        new CurvePoint(75000, 2.5f),
        new CurvePoint(300000, 3),
        new CurvePoint(1000000, 3.8f),
        new CurvePoint(2000000, 4.3f)
    };

    private static readonly SimpleCurve WealthQualitySpreadDeterminationCurve = new SimpleCurve
    {
        new CurvePoint(0, 4.2f),
        new CurvePoint(10000, 4),
        new CurvePoint(75000, 2.5f),
        new CurvePoint(300000, 2.1f),
        new CurvePoint(1000000, 1.5f),
        new CurvePoint(2000000, 1.2f)
    };

    static HarmonyPatches()
    {
        var harmony = new Harmony("mehni.rimworld.MFI.main");
        //HarmonyInstance.DEBUG = true;

        harmony.Patch(AccessTools.Method(typeof(TraderKindDef), nameof(TraderKindDef.PriceTypeFor)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PriceTypeSetter_PostFix)));

        harmony.Patch(AccessTools.Method(typeof(StoryState), nameof(StoryState.Notify_IncidentFired)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentFired_TradeCounter_Postfix)));

        harmony.Patch(AccessTools.Method(typeof(CompQuality), nameof(CompQuality.PostPostGeneratedForTrader)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(CompQuality_TradeQualityIncreaseDestructivePreFix)));

        harmony.Patch(
            AccessTools.Method(typeof(ThingSetMaker), nameof(ThingSetMaker.Generate),
                new[] { typeof(ThingSetMakerParams) }),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TraderStocker_OverStockerPostFix)));

        harmony.Patch(AccessTools.Method(typeof(Tradeable), "InitPriceDataIfNeeded"),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(ErrorSuppressionSssh)));

        harmony.Patch(
            AccessTools.Method(typeof(WorldReachabilityUtility), nameof(WorldReachabilityUtility.CanReach)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(WorldReachUtility_PostFix)));

#if DEBUG
            harmony.Patch(AccessTools.Method(typeof(DebugWindowsOpener), "ToggleDebugActionsMenu"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches),
                    nameof(DebugWindowsOpener_ToggleDebugActionsMenu_Patch)));
#endif

        harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_PsychicEmanatorSoothe), "CurrentStateInternal"),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PsychicEmanatorSoothe_Transpiler)));

        harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.Notify_RelationKindChanged)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Notify_RelationKindChanged)));
        harmony.PatchAll();
        if (ModsConfig.ActiveModsInLoadOrder.All(m => m.Name != "Relations Tab"))
        {
            return;
        }

        try
        {
            ((Action)(() =>
            {
                static float func(Faction faction, Vector2 pos, float width)
                {
                    if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StuffIsGoingDown)
                    {
                        return 0;
                    }

                    var canvas = new Rect(pos.x, pos.y, width, 125f);
                    MainTabWindow_FactionWar.DrawFactionWarBar(canvas);
                    return 125f;
                }

                MainTabWindow_Relations.ExtraFactionDetailDrawers.Add(func);
            }))();
        }
        catch (TypeLoadException)
        {
        }
    }

    private static void Notify_RelationKindChanged(Faction __instance, Faction other)
    {
        if (other != Faction.OfPlayer || !__instance.HostileTo(other))
        {
            return;
        }

        foreach (var item in Find.WorldObjects.AllWorldObjects.Where(x =>
                     x is WorldObject_RoadConstruction && x.Faction == __instance))
        {
            Find.WorldObjects.Remove(item);
        }

        foreach (var stlmnt in Find.WorldObjects.Settlements.Where(x => x.Faction == __instance))
        {
            stlmnt.GetComponent<WorldObjectComp_SettlementBumperCropComp>()?.Disable();
        }
    }

    private static IEnumerable<CodeInstruction> PsychicEmanatorSoothe_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var helperMethod = AccessTools.Method(typeof(HarmonyPatches), nameof(SootheTranspilerHelperMethod));

        foreach (var codeInstruction in instructions)
        {
            if (codeInstruction.opcode == OpCodes.Ldc_R4)
            {
                codeInstruction.opcode = OpCodes.Call;
                codeInstruction.operand = helperMethod;
            }

            yield return codeInstruction;
        }
    }

    private static float SootheTranspilerHelperMethod()
    {
        return Find.World?.GetComponent<WorldComponent_MFI_AnnualExpo>()?.BuffedEmanator ?? false ? 20f : 15f;
    }

    //thx Brrainz
    private static IEnumerable<CodeInstruction> DebugWindowsOpener_ToggleDebugActionsMenu_Patch(
        IEnumerable<CodeInstruction> instructions)
    {
        var from = AccessTools.Constructor(typeof(Dialog_DebugActionsMenu));
        var to = AccessTools.Constructor(typeof(Dialog_MFIDebugActionMenu));
        return instructions.MethodReplacer(from, to);
    }

    private static void WorldReachUtility_PostFix(ref bool __result, Caravan c)
    {
        var settlement = CaravanVisitUtility.SettlementVisitedNow(c);
        var bumperCropComponent = settlement?.GetComponent<WorldObjectComp_SettlementBumperCropComp>();

        if (bumperCropComponent != null)
        {
            __result = !bumperCropComponent.CaravanIsWorking;
        }
    }

    private static void TraderStocker_OverStockerPostFix(ref List<Thing> __result, ThingSetMakerParams parms)
    {
        if (parms.traderDef == null)
        {
            return;
        }

        Map map = null;

        if (parms.tile != null && parms.tile != -1 &&
            (Current.Game.FindMap(parms.tile.Value)?.IsPlayerHome ?? false))
        {
            map = Current.Game.FindMap(parms.tile.Value);
        }
        else if (Find.AnyPlayerHomeMap != null)
        {
            map = Find.AnyPlayerHomeMap;
        }
        else if (Find.CurrentMap != null)
        {
            map = Find.CurrentMap;
        }

        float? silverCount;
        if (map != null && (parms.traderDef.orbital || parms.traderDef.defName.Contains("Base_")))
        {
            //nullable float because not all traders bring silver
            silverCount = __result.Find(x => x.def == ThingDefOf.Silver)?.stackCount;
            if (!silverCount.HasValue)
            {
                return;
            }

            silverCount *= WealthSilverIncreaseDeterminationCurve.Evaluate(map.PlayerWealthForStoryteller);
            __result.First(x => x.def == ThingDefOf.Silver).stackCount = (int)silverCount;
            return;
        }

        if (map == null || parms.makingFaction == null)
        {
            return;
        }

        //nullable float because not all traders bring silver
        silverCount = __result.Find(x => x.def == ThingDefOf.Silver)?.stackCount;
        if (!silverCount.HasValue)
        {
            return;
        }

        var goodwillFactions = map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded;
        var goodwillFactor = 1f;
        if (goodwillFactions.ContainsKey(parms.makingFaction))
        {
            goodwillFactor = goodwillFactions[parms.makingFaction];
        }

        __result.First(x => x.def == ThingDefOf.Silver).stackCount +=
            (int)(parms.makingFaction.GoodwillWith(Faction.OfPlayer) *
                  (goodwillFactor *
                   MoreFactionInteraction_Settings.traderWealthOffsetFromTimesTraded));
    }

    private static bool CompQuality_TradeQualityIncreaseDestructivePreFix(CompQuality __instance,
        TraderKindDef trader, int forTile, Faction forFaction)
    {
        //forTile is assigned in RimWorld.ThingSetMaker_TraderStock.Generate. It's either a best-effort map, or -1.
        Map map = null;
        if (forTile != -1)
        {
            map = Current.Game.FindMap(forTile);
        }

        __instance.SetQuality(FactionAndGoodWillDependantQuality(forFaction, map, trader),
            ArtGenerationContext.Outsider);
        return false;
    }

    /// <summary>
    ///     Change quality carried by traders depending on Faction/Goodwill/Wealth.
    /// </summary>
    /// <returns>QualityCategory depending on wealth or goodwill. Fallback to vanilla when fails.</returns>
    private static QualityCategory FactionAndGoodWillDependantQuality(Faction faction, Map map,
        TraderKindDef trader)
    {
        float num;
        if ((trader.orbital || trader.defName.Contains("_Base")) && map != null)
        {
            if (Rand.Value < 0.25f)
            {
                return QualityCategory.Normal;
            }

            num = Rand.Gaussian(WealthQualityDeterminationCurve.Evaluate(map.wealthWatcher.WealthTotal),
                WealthQualitySpreadDeterminationCurve.Evaluate(map.wealthWatcher.WealthTotal));
            num = Mathf.Clamp(num, 0f, QualityUtility.AllQualityCategories.Count - 0.5f);
            return (QualityCategory)num;
        }

        if (map == null || faction == null)
        {
            return QualityUtility.GenerateQualityTraderItem();
        }

        var qualityIncreaseFromTimesTradedWithFaction = 0f;
        var qualityIncreaseFactorFromPlayerGoodWill = 0f;
        var goodwillFactions = map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded;
        if (goodwillFactions.ContainsKey(faction))
        {
            qualityIncreaseFromTimesTradedWithFaction = Mathf.Clamp01((float)goodwillFactions[faction] / 100);
            qualityIncreaseFactorFromPlayerGoodWill =
                Mathf.Clamp01((float)faction.GoodwillWith(Faction.OfPlayer) / 100);
        }


        if (Rand.Value < 0.25f)
        {
            return QualityCategory.Normal;
        }

        num = Rand.Gaussian(2.5f + qualityIncreaseFactorFromPlayerGoodWill,
            0.84f + qualityIncreaseFromTimesTradedWithFaction);
        num = Mathf.Clamp(num, 0f, QualityUtility.AllQualityCategories.Count - 0.5f);
        return (QualityCategory)num;
    }

    private static IEnumerable<CodeInstruction> ErrorSuppressionSssh(IEnumerable<CodeInstruction> instructions)
    {
        var instructionList = instructions.ToList();
        for (var i = 0; i < instructionList.Count; i++)
        {
            if (instructionList[i].opcode == OpCodes.Ldstr)
            {
                for (var j = 0; j < 7; j++)
                {
                    instructionList[i + j].opcode = OpCodes.Nop;
                }
            }

            yield return instructionList[i];
        }
    }

    /// <summary>
    ///     Increment TimesTraded count of dictionary by one for this faction.
    /// </summary>
    private static void IncidentFired_TradeCounter_Postfix(ref FiringIncident fi)
    {
        if (fi.parms.target is not Map map || fi.def != IncidentDefOf.TraderCaravanArrival || fi.parms.faction == null)
        {
            return;
        }

        var goodwillFactions = map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded;
        if (goodwillFactions.ContainsKey(fi.parms.faction))
        {
            goodwillFactions[fi.parms.faction]++;
        }
    }

    private static void PriceTypeSetter_PostFix(TraderKindDef __instance, ref PriceType __result,
        TradeAction action)
    {
        //PriceTypeSetter is more finicky than I'd like, part of the reason traders arrive without any sellable inventory.
        // had issues with pricetype undefined, pricetype normal and *all* traders having pricetype expensive for *all* goods. This works.
        var priceType = __result;
        if (priceType == PriceType.Undefined)
        {
            return;
        }

        //if (__instance.stockGenerators[i] is StockGenerator_BuyCategory && action == TradeAction.PlayerSells)
        if (__instance.stockGenerators.Any(x => x is StockGenerator_BuyCategory) &&
            action == TradeAction.PlayerSells)
        {
            __result = PriceType.Expensive;
        }
        else
        {
            __result = priceType;
        }
    }
}
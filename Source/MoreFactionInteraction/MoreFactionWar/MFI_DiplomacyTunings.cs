using Verse;

namespace MoreFactionInteraction.MoreFactionWar;

public static class MFI_DiplomacyTunings
{
    public static readonly SimpleCurve BadOutcomeFactorAtStatPower =
    [
        new CurvePoint(0f, 4f),
        new CurvePoint(1f, 1f),
        new CurvePoint(1.5f, 0.4f)
    ];

    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactHuge = new(50, 80);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactBig = new(30, 70);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactMedium = new(20, 50);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactSmall = new(10, 30);

    public static readonly IntRange GoodWill_DeclinedMarriage_Impact = new(-5, -15);

    public static readonly SimpleCurve PawnValueInGoodWillAmountOut =
    [
        new CurvePoint(0f, 0),
        new CurvePoint(500f, 20),
        new CurvePoint(1000f, 40),
        new CurvePoint(2000f, 60),
        new CurvePoint(4000f, 80)
    ];

    public static Outcome FavourDisaster => new()
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome FavourBackfire => new()
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome FavourFlounder => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange
    };

    public static Outcome FavourSuccess => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange
    };

    public static Outcome FavourTriumph => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange
    };

    public static Outcome SabotageDisaster => new()
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome SabotageBackfire => new()
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange
    };

    public static Outcome SabotageFlounder => new();

    public static Outcome SabotageSuccess => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        startWar = true
    };

    public static Outcome SabotageTriumph => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        startWar = true
    };

    public static Outcome BrokerPeaceDisaster => new()
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        startWar = true
    };

    //"rescheduled for later"
    public static Outcome BrokerPeaceBackfire => new();

    public static Outcome BrokerPeaceFlounder => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange
    };

    public static Outcome BrokerPeaceSuccess => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange
    };


    public static Outcome BrokerPeaceTriumph => new()
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange
    };
}
using Verse;

namespace MoreFactionInteraction.MoreFactionWar;

public static class MFI_DiplomacyTunings
{
    public static readonly SimpleCurve BadOutcomeFactorAtStatPower = new SimpleCurve
    {
        new CurvePoint(0f, 4f),
        new CurvePoint(1f, 1f),
        new CurvePoint(1.5f, 0.4f)
    };

    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactHuge = new IntRange(50, 80);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactBig = new IntRange(30, 70);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactMedium = new IntRange(20, 50);
    public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactSmall = new IntRange(10, 30);

    public static readonly IntRange GoodWill_DeclinedMarriage_Impact = new IntRange(-5, -15);

    public static readonly SimpleCurve PawnValueInGoodWillAmountOut = new SimpleCurve
    {
        new CurvePoint(0f, 0),
        new CurvePoint(500f, 20),
        new CurvePoint(1000f, 40),
        new CurvePoint(2000f, 60),
        new CurvePoint(4000f, 80)
    };

    public static Outcome FavourDisaster => new Outcome
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome FavourBackfire => new Outcome
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome FavourFlounder => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange
    };

    public static Outcome FavourSuccess => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange
    };

    public static Outcome FavourTriumph => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange
    };

    public static Outcome SabotageDisaster => new Outcome
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        setHostile = true,
        startWar = true
    };

    public static Outcome SabotageBackfire => new Outcome
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange
    };

    public static Outcome SabotageFlounder => new Outcome();

    public static Outcome SabotageSuccess => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        startWar = true
    };

    public static Outcome SabotageTriumph => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        startWar = true
    };

    public static Outcome BrokerPeaceDisaster => new Outcome
    {
        goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        startWar = true
    };

    //"rescheduled for later"
    public static Outcome BrokerPeaceBackfire => new Outcome();

    public static Outcome BrokerPeaceFlounder => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange
    };

    public static Outcome BrokerPeaceSuccess => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange
    };


    public static Outcome BrokerPeaceTriumph => new Outcome
    {
        goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange
    };
}
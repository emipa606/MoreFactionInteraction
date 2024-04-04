using System;
using System.Collections.Generic;
using System.Linq;
using MoreFactionInteraction.General;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace MoreFactionInteraction.More_Flavour;

public class AnnualExpoDialogue(Pawn participant, Caravan caravan, EventDef activity, Faction host)
{
    public DiaNode AnnualExpoDialogueNode()
    {
        GrammarRequest request = default;
        request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);

        var flavourText = GrammarResolver.Resolve("artextra_clause", request);

        var dialogueGreeting =
            new DiaNode(
                "MFI_AnnualExpoDialogueIntroduction".Translate(activity.theme, FirstCharacterToLower(flavourText)));

        foreach (var option in DialogueOptions(participant))
        {
            dialogueGreeting.options.Add(option);
        }

        return dialogueGreeting;
    }

    private IEnumerable<DiaOption> DialogueOptions(Pawn participatingPawn)
    {
        var annualExpoDialogueOutcome =
            $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL + F12 now), you get a cookie. P: {participatingPawn} C: {caravan} E: {activity} H: {host}";

        var broughtArt = (activity == MFI_DefOf.MFI_CulturalSwap) &
                         MFI_Utilities.TryGetBestArt(caravan, out var art, out _);

        yield return new DiaOption("MFI_AnnualExpoFirstOption".Translate())
        {
            action = () => DetermineOutcome(out annualExpoDialogueOutcome),
            linkLateBind = () =>
            {
                var endpoint = DialogueResolver(annualExpoDialogueOutcome, broughtArt);

                if (broughtArt)
                {
                    endpoint.options[0].linkLateBind = () =>
                        EventRewardWorker_CulturalSwap.DialogueResolverArtOffer(
                            "MFI_culturalSwapOutcomeWhoaYouActuallyBroughtArt", art, caravan);
                }

                return endpoint;
            }
        };

#if DEBUG
            var devModeTest = new DiaOption("DevMode: Test chances and outcomes")
                {action = DebugLogChances};

            if (!Prefs.DevMode)
            {
                yield break;
            }

            yield return devModeTest;
            yield return new DiaOption("restart")
            {
                action = GenCommandLine.Restart
            };
#endif
    }

#if DEBUG
        internal void DebugLogChances()
        {
            var sb = new StringBuilder();
            foreach (var defEvent in DefDatabase<EventDef>.AllDefsListForReading)
            {
                var outComeOne = 0;
                var outComeTwo = 0;
                var outComeThree = 0;
                const float pawnsToTest = 1000;
                float skill = 0;

                double mean = 0;
                double variance = 0;
                double stdDev = 0;
                double min = 0;
                double max = 0;

                sb.AppendLine(defEvent.LabelCap);

                for (var i = 0; i < pawnsToTest; i++)
                {
                    var bestpawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

                    while (defEvent.relevantStat.Worker.IsDisabledFor(bestpawn))
                    {
                        bestpawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                    }

                    skill += bestpawn.GetStatValue(defEvent.relevantStat);

                    var placement = DeterminePlacementFor(bestpawn, defEvent, out mean, out variance, out stdDev,
                        out max, out min);
                    switch (placement)
                    {
                        case Placement.First:
                            outComeOne++;
                            break;
                        case Placement.Second:
                            outComeTwo++;
                            break;
                        case Placement.Third:
                            outComeThree++;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                sb.AppendLine(
                    $"Chances for {pawnsToTest} pawns with stat {defEvent.relevantStat} @ {skill / pawnsToTest}:" +
                    $" first: {(outComeOne / pawnsToTest).ToStringPercent()}, " +
                    $" second: {(outComeTwo / pawnsToTest).ToStringPercent()}, " +
                    $" third: {(outComeThree / pawnsToTest).ToStringPercent()} " +
                    $" mean: {mean}, variance: {variance}, stdDev: {stdDev}, min: {min}, max: {max}");
            }

            Log.Error(sb.ToString(), true);
        }
#endif

    private void DetermineOutcome(out string annualExpoDialogueOutcome)
    {
        string rewards;
        var thisYearsRelevantSkill = activity.learnedSkills.RandomElement();

        if (participant.skills.GetSkill(thisYearsRelevantSkill).TotallyDisabled)
        {
            thisYearsRelevantSkill = participant.skills.skills.Where(x => !x.TotallyDisabled)
                .RandomElementByWeight(x => (int)x.passion).def;
        }

        switch (DeterminePlacementFor(participant, activity, out _, out _, out _,
                    out _, out _))
        {
            case Placement.First:
                rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstPlace,
                    activity.rewardFirstPlace);
                participant.skills.Learn(thisYearsRelevantSkill, activity.xPGainFirstPlace, true);
                TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstPlace);
                annualExpoDialogueOutcome = activity.outComeFirstPlace.Formatted(rewards).AdjustedFor(participant);
                break;

            case Placement.Second:
                rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstLoser,
                    activity.rewardFirstLoser);
                participant.skills.Learn(thisYearsRelevantSkill, activity.xPGainFirstLoser, true);
                TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstLoser);
                annualExpoDialogueOutcome = activity.outcomeFirstLoser.Formatted(rewards).AdjustedFor(participant);
                break;

            case Placement.Third:
                rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstOther,
                    activity.rewardFirstOther);
                participant.skills.Learn(thisYearsRelevantSkill, activity.xPGainFirstOther, true);
                TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstOther);
                annualExpoDialogueOutcome = activity.outComeFirstOther.Formatted(rewards).AdjustedFor(participant);
                break;

            default:
                Log.Error($"P: {participant}, C: {caravan}, E: {activity}");
                throw new Exception(
                    $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL+F12 now), you get a cookie. P: {participant} C: {caravan} E: {activity} H: {host}. C: default.");
        }
    }

    private Placement DeterminePlacementFor(Pawn rep, EventDef eventDef, out double mean, out double variance,
        out double stdDev, out double max, out double min)
    {
        var difficultyModifier =
            1.05f + (0.01f * Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().timesHeld);

        difficultyModifier = Mathf.Clamp(difficultyModifier, 1.05f, 1.1f);

        var leaders = Find.FactionManager.AllFactionsVisible
            .Select(faction => faction.leader)
            .Where(leader => leader != null && !eventDef.relevantStat.Worker.IsDisabledFor(leader))
            .Concat(new[] { rep })
            .Concat(Find.WorldPawns.AllPawnsAlive
                .Where(x => x.Faction == host && !eventDef.relevantStat.Worker.IsDisabledFor(x)).Take(25))
            .Select(pawn => new
            {
                pawn,
                score = pawn.Faction.leader == pawn
                    ? pawn.GetStatValue(eventDef.relevantStat) * difficultyModifier
                    : pawn.GetStatValue(eventDef.relevantStat)
            })
            .OrderBy(x => x.score)
            .ToArray();

        var repSkill = rep.GetStatValue(eventDef.relevantStat);

        max = leaders.Max(x => x.score);
        min = leaders.Min(x => x.score);
        mean = leaders.Average(x => x.score);
        variance = (((max - min + 1) * (max - min + 1)) - 1.0) / 12;
        stdDev = Math.Sqrt(variance);

        var averageSkillRange = new FloatRange((float)(mean - (stdDev * 0.3)), (float)(mean + (stdDev * 0.3)));

        if (leaders[0].pawn == rep)
        {
            return Placement.First;
        }

        if (averageSkillRange.Includes(repSkill))
        {
            return Placement.Second;
        }

        return repSkill > mean ? Placement.First : Placement.Third;
    }

    private static void TryAppendExpGainInfo(ref string rewardsOutcome, Pawn pawn, SkillDef skill, float amount)
    {
        if (amount > 0)
        {
            rewardsOutcome = $"{rewardsOutcome}\n\n" +
                             "MFI_AnnualExpoXPGain".Translate(pawn.LabelShort, amount.ToString("F0"), skill.label);
        }
    }

    private static DiaNode DialogueResolver(string textResult, bool broughtArt)
    {
        var resolver = new DiaNode(textResult);
        var diaOption = new DiaOption("OK".Translate())
        {
            resolveTree = !broughtArt
        };
        resolver.options.Add(diaOption);
        return resolver;
    }

    private static string FirstCharacterToLower(string str)
    {
        if (str.NullOrEmpty() || char.IsLower(str[0]))
        {
            return str;
        }

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private enum Placement
    {
        First,
        Second,
        Third
    }
}
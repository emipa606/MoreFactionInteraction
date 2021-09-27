using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class FactionWarDialogue
    {
        private const float BaseWeight_Disaster = 0.05f;
        private const float BaseWeight_Backfire = 0.1f;
        private const float BaseWeight_TalksFlounder = 0.2f;
        private const float BaseWeight_Success = 0.55f;
        private const float BaseWeight_Triumph = 0.1f;
        private readonly IIncidentTarget _incidentTarget;

        private readonly Pawn _pawn;
        private Faction _burdenedFaction;
        private Faction _favouredFaction;
        private List<(Action Outcome_Talks, float weight, string dialogueResolverText)> outComes;

        public FactionWarDialogue(Pawn pawn, Faction factionOne, Faction factionInstigator,
            IIncidentTarget incidentTarget)
        {
            _pawn = pawn;
            _favouredFaction = factionOne;
            _burdenedFaction = factionInstigator;
            _incidentTarget = incidentTarget;
        }

        public DiaNode FactionWarPeaceTalks()
        {
            var factionInstigatorLeaderName = _burdenedFaction.leader != null
                ? _burdenedFaction.leader.Name.ToStringFull
                : _burdenedFaction.Name;

            var factionOneLeaderName =
                _favouredFaction.leader != null ? _favouredFaction.leader.Name.ToStringFull : _favouredFaction.Name;

            var dialogueGreeting =
                new DiaNode("MFI_FactionWarPeaceTalksIntroduction".Translate(factionOneLeaderName,
                    factionInstigatorLeaderName, _pawn.Label));

            foreach (var option in DialogueOptions())
            {
                dialogueGreeting.options.Add(option);
            }

            if (Prefs.DevMode)
            {
                dialogueGreeting.options.Add(new DiaOption("(Dev: start war)")
                {
                    action = () =>
                        Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(_favouredFaction,
                            _burdenedFaction, false),
                    linkLateBind = () =>
                        DialogueResolver(
                            "Alrighty. War started. Sorry about the lack of fancy flavour text for this dev mode only option.")
                });
            }

            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions()
        {
            var factionWarNegotiationsOutcome =
                "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption("MFI_FactionWarPeaceTalksCurryFavour".Translate(_favouredFaction.Name))
            {
                action = () =>
                    DetermineOutcome(DesiredOutcome.CURRY_FAVOUR_FACTION_ONE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome)
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksCurryFavour".Translate(_burdenedFaction.Name))
            {
                action = () =>
                {
                    SwapFavouredFaction();
                    DetermineOutcome(DesiredOutcome.CURRY_FAVOUR_FACTION_TWO, out factionWarNegotiationsOutcome);
                },
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome)
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksSabotage".Translate())
            {
                action = () => DetermineOutcome(DesiredOutcome.SABOTAGE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome)
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksBrokerPeace".Translate())
            {
                action = () => DetermineOutcome(DesiredOutcome.BROKER_PEACE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome)
            };
        }

        public void DetermineOutcome(DesiredOutcome desiredOutcome, out string factionWarNegotiationsOutcome)
        {
            var badOutcomeWeightFactor = BaseWeight_Disaster * GetBadOutcomeWeightFactor(_pawn);
            var goodOutcomeWeightFactor = 1f / badOutcomeWeightFactor;
            factionWarNegotiationsOutcome =
                "Something went wrong with More Faction Interaction. Please contact mod author.";

            if (desiredOutcome == DesiredOutcome.CURRY_FAVOUR_FACTION_ONE ||
                desiredOutcome == DesiredOutcome.CURRY_FAVOUR_FACTION_TWO)
            {
                factionWarNegotiationsOutcome = CurryFavour(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
            else if (desiredOutcome == DesiredOutcome.SABOTAGE)
            {
                factionWarNegotiationsOutcome = Sabotage(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
            else if (desiredOutcome == DesiredOutcome.BROKER_PEACE)
            {
                factionWarNegotiationsOutcome = BrokerPeace(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
        }

        private string CurryFavour(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>
            {
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.FavourDisaster),
                    BaseWeight_Disaster * GetBadOutcomeWeightFactor(_pawn),
                    "MFI_FactionWarFavourFactionDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.FavourBackfire),
                    BaseWeight_Backfire * badOutcomeWeightFactor,
                    "MFI_FactionWarFavourFactionBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.FavourFlounder),
                    BaseWeight_TalksFlounder,
                    "MFI_FactionWarFavourFactionFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.FavourSuccess),
                    BaseWeight_Success * goodOutcomeWeightFactor,
                    "MFI_FactionWarFavourFactionSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.FavourTriumph),
                    BaseWeight_Triumph * goodOutcomeWeightFactor,
                    "MFI_FactionWarFavourFactionTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name))
            };


            var factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string Sabotage(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>
            {
                (
                    () =>
                    {
                        HandleOutcome(MFI_DiplomacyTunings.SabotageDisaster);
                        Outcome_TalksSabotageDisaster(_favouredFaction, _burdenedFaction, _pawn, _incidentTarget);
                    },
                    BaseWeight_Disaster * badOutcomeWeightFactor,
                    "MFI_FactionWarSabotageDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.SabotageBackfire),
                    BaseWeight_Backfire * badOutcomeWeightFactor,
                    "MFI_FactionWarSabotageBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.SabotageFlounder),
                    BaseWeight_TalksFlounder,
                    "MFI_FactionWarSabotageFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.SabotageSuccess),
                    BaseWeight_Success * goodOutcomeWeightFactor,
                    "MFI_FactionWarSabotageSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.SabotageTriumph),
                    BaseWeight_Triumph * goodOutcomeWeightFactor,
                    "MFI_FactionWarSabotageTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name))
            };


            var factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string BrokerPeace(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>
            {
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceDisaster),
                    BaseWeight_Disaster * badOutcomeWeightFactor,
                    "MFI_FactionWarBrokerPeaceDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceBackfire),
                    BaseWeight_Backfire * badOutcomeWeightFactor,
                    "MFI_FactionWarBrokerPeaceBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceFlounder),
                    BaseWeight_TalksFlounder,
                    "MFI_FactionWarBrokerPeaceFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceSuccess),
                    BaseWeight_Success * goodOutcomeWeightFactor,
                    "MFI_FactionWarBrokerPeaceSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)),
                (
                    () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceTriumph),
                    BaseWeight_Triumph * goodOutcomeWeightFactor,
                    "MFI_FactionWarBrokerPeaceTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name))
            };


            var factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string TriggerOutcome()
        {
            _pawn.skills.Learn(SkillDefOf.Social, 6000f, true);

            var (chosenOutcome, _, flavor) = outComes.RandomElementByWeight(x => x.weight);
            chosenOutcome();
            return flavor;
        }

        private void SwapFavouredFaction()
        {
            (_favouredFaction, _burdenedFaction) = (_burdenedFaction, _favouredFaction);
        }

        private static DiaNode DialogueResolver(string textResult)
        {
            var resolver = new DiaNode(textResult);
            var diaOption = new DiaOption("OK".Translate())
            {
                resolveTree = true
            };
            resolver.options.Add(diaOption);
            return resolver;
        }

        private void HandleOutcome(Outcome result)
        {
            _favouredFaction.TryAffectGoodwillWith(_pawn.Faction, result.goodwillChangeFavouredFaction);
            _burdenedFaction.TryAffectGoodwillWith(_pawn.Faction, result.goodwillChangeBurdenedFaction);

            if (result.setHostile)
            {
                _burdenedFaction.SetRelationDirect(_pawn.Faction, FactionRelationKind.Hostile, false);
            }

            if (result.startWar)
            {
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(_favouredFaction,
                    _burdenedFaction, _favouredFaction.leader == _pawn);
            }
        }

        private static void Outcome_TalksSabotageDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn,
            IIncidentTarget incidentTarget)
        {
            favouredFaction.SetRelationDirect(pawn.Faction, FactionRelationKind.Hostile, false);
            LongEventHandler.QueueLongEvent(delegate
            {
                var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, incidentTarget);
                incidentParms.faction = favouredFaction;
                var defaultPawnGroupMakerParms =
                    IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                var list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();

                incidentParms.faction = burdenedFaction;
                var burdenedFactionPawnGroupMakerParms =
                    IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                burdenedFactionPawnGroupMakerParms.generateFightersOnly = true;
                var burdenedFactionWarriors =
                    PawnGroupMakerUtility.GeneratePawns(burdenedFactionPawnGroupMakerParms).ToList();

                var combinedList = new List<Pawn>();
                combinedList.AddRange(list);
                combinedList.AddRange(burdenedFactionWarriors);

                var map = CaravanIncidentUtility.SetupCaravanAttackMap(incidentTarget as Caravan, combinedList, true);

                if (list.Any())
                {
                    LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(favouredFaction), map, list);
                }

                if (burdenedFactionWarriors.Any())
                {
                    LordMaker.MakeNewLord(incidentParms.faction,
                        new LordJob_AssaultColony(burdenedFaction), map, burdenedFactionWarriors);
                }

                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }, "GeneratingMapForNewEncounter", false, null);
        }

        private static float GetBadOutcomeWeightFactor(Pawn _pawn)
        {
            return MFI_DiplomacyTunings.BadOutcomeFactorAtStatPower.Evaluate(
                _pawn.GetStatValue(StatDefOf.NegotiationAbility));
        }
    }
}
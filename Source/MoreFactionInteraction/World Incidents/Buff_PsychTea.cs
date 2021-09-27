using System.Linq;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    public class Buff_PsychTea : Buff
    {
        public override void Apply()
        {
            var giveHediff = (IngestionOutcomeDoer_GiveHediff)ThingDef.Named("PsychiteTea").ingestible.outcomeDoers
                .FirstOrDefault(x => x is IngestionOutcomeDoer_GiveHediff);
            if (giveHediff != null)
            {
                giveHediff.severity = 1f;
            }

            Active = true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_PsychTea");
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_PsychTea());
        }

        public override ThingDef RelevantThingDef()
        {
            return DefDatabase<ThingDef>.GetNamed("PsychiteTea");
        }

        public override string Description()
        {
            return "MFI_buffPsychite".Translate(ThingDef.Named("PsychiteTea").label);
        }
    }
}
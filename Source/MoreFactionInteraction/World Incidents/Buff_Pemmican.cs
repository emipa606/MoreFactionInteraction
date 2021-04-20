using RimWorld;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    public class Buff_Pemmican : Buff
    {
        public override void Apply()
        {
            Active = true;
            ThingDefOf.Pemmican.comps.RemoveAll(x => x is CompProperties_Rottable);
        }

        public static void Register()
        {
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().RegisterBuff(new Buff_Pemmican());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Active, "MFI_Buff_Pemmican");
        }

        public override ThingDef RelevantThingDef()
        {
            return ThingDefOf.Pemmican;
        }

        public override string Description()
        {
            return "MFI_buffPemmican".Translate();
        }
    }
}
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction;

[HarmonyPatch(typeof(TransportPodsArrivalAction_VisitSite), "Arrived")]
internal static class Patch_Arrived
{
    private static bool Prefix(Site ___site, PawnsArrivalModeDef ___arrivalMode, List<ActiveDropPodInfo> pods)
    {
        if (___site.parts == null)
        {
            return true;
        }

        foreach (var part in ___site.parts)
        {
            if (part.def != MFI_DefOf.MFI_HuntersLodgePart)
            {
                continue;
            }

            var lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
            var num = !___site.HasMap;
            var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile,
                ___site.PreferredMapSize, null);
            if (num)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns,
                    "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural),
                    LetterDefOf.NeutralEvent, true);
            }

            Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget,
                MessageTypeDefOf.TaskCompletion);
            ___arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
            return false;
        }

        return true;
    }
}
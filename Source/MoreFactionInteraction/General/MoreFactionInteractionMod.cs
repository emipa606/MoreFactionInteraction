using JetBrains.Annotations;
using Mlie;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

[UsedImplicitly]
public class MoreFactionInteractionMod : Mod
{
    public static string currentVersion;

    public MoreFactionInteractionMod(ModContentPack content) : base(content)
    {
        GetSettings<MoreFactionInteraction_Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        GetSettings<MoreFactionInteraction_Settings>().DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "More Faction Interaction";
    }
}
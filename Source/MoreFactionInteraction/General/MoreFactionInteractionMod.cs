using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction;

[UsedImplicitly]
public class MoreFactionInteractionMod : Mod
{
    public MoreFactionInteractionMod(ModContentPack content) : base(content)
    {
        GetSettings<MoreFactionInteraction_Settings>();
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
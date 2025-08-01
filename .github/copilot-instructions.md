# GitHub Copilot Instructions for More Faction Interaction (Continued)

## Mod Overview and Purpose
The "More Faction Interaction (Continued)" mod enhances the interaction between the player and AI factions within the game RimWorld. This update, originally developed by Mehnis and continued by Taranchuk in collaboration with Spark7979, aims to bring more depth to the world through intricate faction dynamics, trade improvements, and new event opportunities.

## Key Features and Systems

### Meaningful Faction Relations
- **Trust Building and Rewards**: Establish positive relations to gain rewards from factions.
- **Inter-faction Marriages**: Strengthen alliances through diplomatic marriages.
- **Labor Requests**: Provide assistance in faction recovery efforts.
- **Favor Exchanges**: Participate in mutual beneficial activities.
- **Increased World Events**: Experience a more eventful world.

### Trade Improvements
- **Specialized Traders**: Engage with traders offering specific goods based on market demands.
- **Frequent Trading Opportunities**: Boost trade frequency by maintaining good relations.
- **Wealthier Traders**: Benefit from improved trader inventories with increased trust.

### A Living World
-**Pirate Expansion**: Pirates establish outposts and may demand tribute.
- **Annual Expo**: Showcase skills, possibly earning hosting invitations.
- **Roadworks by Allies**: Utilize roads as a productive silver sink.
- **Squatters and Settlements**: Explore new dynamics with settlements.

### Warring Factions
- Choose sides or stay neutral as faction tensions rise.
- Participate or benefit in faction conflicts and resolutions.

## Coding Patterns and Conventions

### General Coding Guidelines
- **Class Naming**: Use PascalCase for class names.
- **Method Naming**: Use PascalCase for method names.
- **Visibility**: Prefer internal for classes and methods unless access is needed elsewhere.
- **Structuring**: Group related methods within classes for clarity and organization.

csharp
public class ExampleClass
{
    public void ExampleMethod()
    {
        // Method implementation
    }
}


## XML Integration
- XML files define event details, trader preferences, and item categories.
- Ensure XML tags are accurate and consistent with class definitions in C#.
- Use XML for localization, supporting multiple languages as specified in the mod description.

## Harmony Patching
- Employ **Harmony** for non-intrusive patches on the original game methods.
- Structure patches as separate classes (e.g., `Patch_AffectRelationsOnAttacked`).
- Follow the pattern of pre and post-fix patches to safely modify game behavior.

csharp
[HarmonyPatch(typeof(TargetClass), "MethodToPatch")]
public static class Patch_ClassName
{
    public static void Prefix()
    {
        // Prefix logic
    }

    public static void Postfix()
    {
        // Postfix logic
    }
}


## Suggestions for Copilot
- Implement Helper Methods: Leverage utility classes for repeated logic (e.g., `MFI_Utilities`).
- Harmony Patch Generation: Create new patches systematically using premade templates.
- Event Creation: Use examples from existing incidents to create new event types.
- User Interface: Enhance `DoWindowContents` for settings configuration interfaces.

## Conclusion
These instructions are aimed to guide developers working with "More Faction Interaction (Continued)" using GitHub Copilot efficiently. By adhering to outlined guidelines, you can maintain consistency and contribute significantly to expanding mod functionalities.

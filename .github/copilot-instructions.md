# GitHub Copilot Instructions for "More Faction Interaction (Continued)"

## Mod Overview
The "More Faction Interaction (Continued)" mod enhances the interaction dynamics within the game RimWorld by expanding the depth and breadth of interactions between factions and the player. The mod aims to make the game world feel more alive and interconnected, offering players meaningful decisions and consequences tied to faction relationships.

## Key Features and Systems
- **Meaningful Faction Relations**: Develop trust with factions to gain rewards, engage in diplomatic marriages, and assist in faction recoveries. Requests for aid and favors from factions become commonplace.
- **Trade Improvements**: Specialized and richer traders visit more frequently with higher quality goods as relations improve.
- **Living World Dynamics**: Pirates establish outposts; yearly expositions display colony skills; allied road constructions act as a silver sink. Players also contend with squatters and engage in faction wars.
- **Event Variety**: The mod includes diplomatic marriages, trade requests, pirate extortion threats, mystical shaman events, and more, significantly increasing the rate and variety of world events.

## Coding Patterns and Conventions
- **Class Structure**: Mostly utilizes static classes for utilities and patching (`HarmonyPatches.cs`, `MFI_DefOf.cs`). Game mechanics and settings use class inheritance, e.g., `MoreFactionInteraction_Settings` for mod settings.
- **Method Access Modifiers**: Classes are defined with public or internal access, ensuring internal helpers are only accessible within the assembly.
- **Mod Settings**: Custom settings are managed via `MoreFactionInteraction_Settings.cs`, including options to toggle specific features or adjust gameplay parameters (e.g., disabling Annual Expo).

## XML Integration
- **Translation and Localization**: Multi-language support is included via XML files housed in the Languages folder. The mod supports multiple languages like German, Czech, Japanese, Russian, and more.
- **Game Definitions**: XML files define incidents, letters, and site parts for streamlined integration. Developers should ensure definitions are tagged correctly for adaptability with other mods.

## Harmony Patching
- **Patching Setup**: `HarmonyPatches.cs` consists of static methods designed to inject behavior modifications into the base game's code. It leverages Harmony 2 for runtime method patching.
- **Examples of Patches**: Enhancements to faction interaction often involve patched methods related to trade caravans or faction relation events.

## Suggestions for Copilot
1. **Suggest Patch Points**: Automatically identify and suggest potential patch points within RimWorld methods that could benefit from Harmony, indicating common extension opportunities.
2. **XML Snippet Integration**: Provide snippet templates for XML structure when adding new incidents or modifying existing game definitions for smoother translation adjustments.
3. **Method Stubs**: Generate stub methods for new incident worker classes, ensuring developers insert required logic without designing the boilerplate from scratch.
4. **Event Handling**: Recommend robust design patterns for handling asynchronous world events to prevent potential data racing issues or event stacking bugs.
5. **Settings Extensions**: Propose auto-completions for extending the mod settings UI, ensuring customizable features are easily adjustable by users.
6. **Performance Tips**: Analyze common performance pitfalls in modding scenarios (e.g., excessive tick operations) and suggest optimizations.

Always ensure changes maintain compatibility with supported mods such as Dynamic Diplomacy and the "Vanilla Expanded" series. Developers are encouraged to test the mod in varied load orders, ideally towards the bottom of their mod list for optimal results.


This markdown file provides an extensive overview, clear instructions, and suggestions for mod developers utilizing GitHub Copilot for the "More Faction Interaction (Continued)" RimWorld mod. It follows the typical workflow and expectations within mod development in the RimWorld ecosystem.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.

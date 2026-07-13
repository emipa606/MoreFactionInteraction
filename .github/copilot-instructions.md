# Copilot Instructions for More Faction Interaction (Continued)

## Mod Overview and Purpose
More Faction Interaction (Continued) is an ambitious expansion mod for RimWorld designed to enhance your gameplay by introducing intricate and meaningful interactions with in-game factions. The mod, originally created by Mehni and updated by Taranchuk, integrates a wide variety of events and features to make the game's world dynamic and alive. The goal of the mod is to provide a more immersive experience by enriching the diplomatic, trade, and societal interactions you have with the various factions that occupy the game world.

## Key Features and Systems

### Meaningful Faction Relations
- **Trust and Rewards**: Building trust with factions will lead to rewards such as better trade deals or alliances.
- **Inter-Faction Marriages**: Factions can intermarry, fostering stronger alliances and relations between them.
- **Faction Assistance Requests**: Factions may call upon your colony for labor or other forms of assistance.
- **Favor Exchange**: Gain favors through meaningful actions and redeem them with factions for various advantages.
- **Increased World Events**: Experience a higher frequency of world-altering events as you build relationships with factions.

### Trade Improvements
- **Specialized Traders**: Encounter traders desperate for specific goods, creating unique selling opportunities.
- **Increased Trade Frequency**: Build relationships to increase the frequency of visiting traders.
- **Richer Traders**: As trust grows, traders arrive with more resources and better offers.
- **Higher Quality Goods**: Stronger faction relations lead to traders offering higher quality goods.

### Living World
- **Pirate Outposts**: Pirates expand their territories, leading to new challenges and events.
- **Annual Exposition**: Show off your skills at a yearly expo; success might lead to hosting opportunities.
- **Roadworks and Infrastructure**: Allies can build roads, improving travel and trade, while functioning as a silver expenditure.

### Warring Factions
- **Faction Conflicts**: Navigate brewing unrest between factions, with opportunities to support, remain neutral, or profit from the chaos.

### Event Highlights
- **Diplomatic Marriages**
- **Reverse Trade Requests**
- **Bumper Crops**
- **Hunter's Lodge**
- **Pirate Extortion**
- **Mystical Shaman**
- **Annual Expo**
- **Squatters**
- **Roadworks**
- **Faction WAR**

## Coding Patterns and Conventions

### C# Coding Practices
- **Namespace Organization**: Ensure logical grouping of classes by functionality.
- **Commenting and Documentation**: Use XML comments for all public methods and classes to enhance code readability and maintainability.
- **Consistent Naming Conventions**: Utilize PascalCase for class names and methods, camelCase for variables, and ALL_UPPERCASE for constants.
- **Encapsulation**: Favor `private` or `protected` accessors for member variables, exposing them via properties when necessary.

### XML Integration
The mod utilizes XML for event definitions and responses:
- **Event Definitions**: Structured in XML files such as `AnnualExpoEvents.xml`, defining the parameters and logic for each world event.
- **Choice Letters**: Custom choice letters are defined in XML to facilitate user interaction with faction events.

### Harmony Patching
Harmony is essential for modifying base game methods to introduce new behaviors:
- **Prefix and Postfix Methods**: Use patches judiciously to modify or extend existing methods without breaking core functionalities.
- **Maintain Compatibility**: Ensure patches are reversible and maintain game stability when the mod is not active.

## Suggestions for Copilot
- **Autocomplete for Modding API**: Enable Copilot to suggest completes for frequently used RimWorld modding APIs and Harmony patching templates.
- **XML Def Autocompletion**: Provide intelligent suggestions for defining new `IncidentDef`, `LetterDef`, and other XML nodes relevant to RimWorld modding.
- **Code Snippets for Common Patterns**: Offer snippets for recurring tasks, such as generating new trade caravans or handling custom event logic.
- **Error Handling Practices**: Suggest robust error handling and logging practices throughout the mod's codebase.

## Additional Notes
To improve compatibility and mod behavior, it's recommended to load More Faction Interaction at the bottom of your mod list. For comprehensive bug reporting and support, use the designated Discord channel or upload logs directly using the Log Uploader.

If you have any solutions or modifications, consider contributing back to the GitHub repository to help improve the mod further.

Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).


# Project Horizons C#

A Godot 4 C# version of the project focused on top-down combat, procedural generation, system-oriented organization, and fast gameplay iteration.

## Current status

The project already includes:

- melee and ranged combat
- weapons and items defined through `.tres`
- enemies built on a shared base with specific variants
- multiple world generation approaches
- HUD, main menu, saves, settings, and pause menu
- world and progression persistence through `DataManager`

Some areas are still in transition, especially balancing, large-script cleanup, and UX refinement.

## Stack

- Godot 4.7 with C#
- .NET / Mono
- Godot resources (`.tscn`, `.tres`) for scenes and data

## Main structure

- `Autoload/` global game systems, saves, achievements, settings, and enemy management
- `Player/` player logic, stats, hand, class, and animation
- `Items/` item resources
- `Weapons/` weapon scenes and base scripts
- `Projectiles/` player and enemy projectiles
- `Enemies/` enemy implementations and enemy-system utilities
- `Chests/` chest logic and chest-state tracking
- `Levels/` level scenes
- `LevelGenerators/` world generators
- `Interface/` menus, HUD, and overlays
- `Docs/` project documentation

## Current game flow

1. `MainMenu` opens saves or settings.
2. The selected save loads a procedural level.
3. The HUD is instantiated together with the level.
4. During a run, the game uses `DataManager` for world state, equipment, and progression.
5. The pause menu allows continuing, saving, opening settings, or returning to the main menu.

## Default controls

- `WASD`: move
- `Left mouse button`: attack / use weapon
- `Space`: dash
- `E`: collect / interact
- `G`: consume
- `Esc`: pause

## Local build

```bash
dotnet build project-horizons-cs.csproj
```

## Documentation

- [Docs/overview.md](Docs/overview.md)
- [Docs/architecture.md](Docs/architecture.md)
- [Docs/development.md](Docs/development.md)
- [Docs/roadmap.md](Docs/roadmap.md)
- [Docs/systems/items.md](Docs/systems/items.md)
- [Docs/systems/enemies.md](Docs/systems/enemies.md)
- [Docs/systems/projectiles.md](Docs/systems/projectiles.md)
- [Docs/systems/world.md](Docs/systems/world.md)

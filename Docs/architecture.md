# Architecture

## General view

The project is built around three main layers:

- Godot scenes for visual composition and hierarchy
- C# scripts for behavior
- `.tres` resources for data and content configuration

This is not an ECS architecture. At the moment it follows an object-and-scene model with a few autoloads acting as global services.

## Autoloads

### `Globals`

Stores runtime references to the player, HUD, local level generator, and global flags such as `InMenu`.

### `DataManager`

Responsible for:

- saving global settings into `user://save.txt`
- saving the current world into `user://<slot>.txt`
- serializing equipped items
- tracking opened chests and player progression

### `EnemiesManager`

Centralizes:

- spawn tables per biome
- enemy instantiation by path
- simple LOD grouping through `EnemyGroup`

### `AchievementsManager`

Tracks progression and unlocks tied to exploration, deaths, bosses, and items.

### `SettingsApplier`

Reads settings from `DataManager` and applies window, rendering, and quality options.

## Main runtime

### Player

Player logic is separated into:

- input and state handling in `Player.cs`
- persistent stats in `PlayerStats.cs`
- equipped weapon handling in `PlayerHand.cs`

At the moment, gameplay differentiation is much stronger between melee and ranged than between full weapon subclasses.

### Items and Weapons

- `Items/*.tres`: item data definitions
- `Weapons/*.tscn`: concrete equipped weapon scenes
- `Weapon.cs`, `PhysicsWeapon.cs`, `DistanceWeapon.cs`: weapon-system bases

### Projectiles

`Projectile.cs` handles movement, lifetime, and damage. Variants swap sprite, shape, speed, and damage through scenes.

### Enemies

`EnemyTemplate.cs` contains health, state, damage reception, and the base runtime flow. Concrete enemies specialize behavior and presentation.

### World

The project currently contains more than one generator and more than one level type:

- `ProceduralLevel`
- `RubyProceduralLevel`
- `DeltaProceduralLevel`
- `ChunkedProceduralLevel`
- `EmeraldLevel`

Not all of them are equally mature.

## Persistence

World saves currently store:

- current level
- played time
- seed and difficulty
- player position
- health, XP, level, and kills
- equipment
- opened chests

## Interface

`Interface/` contains:

- main menu
- save manager
- settings
- HUD
- pause menu
- achievement overlays

`PauseMenu` is embedded into the `HUD`; it does not switch scenes.

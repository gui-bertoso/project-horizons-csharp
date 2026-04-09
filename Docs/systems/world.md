# World System

## Overview

The project contains more than one generation path and more than one level type at the same time.

That is useful for experimentation, but it also means not everything represents the final main gameplay path.

## Main level scenes

- `Levels/ProceduralLevel.tscn`
- `Levels/ruby_procedural_level.tscn`
- `Levels/delta_procedural_level.tscn`
- `Levels/ChunkedProceduralLevel.tscn`
- `Levels/EmeraldLevel.tscn`

## Generators

- `LevelGenerators/BruteGen/`
- `LevelGenerators/RubyGen/`
- `LevelGenerators/DeltaGen/`
- `LevelGenerators/FiberGen/`
- `LevelGenerators/EmeraldGen/`

## World-related persistence

The world save currently stores:

- current level
- played time
- seed
- difficulty
- opened chests
- equipment
- player stats

## Chests

`ChestTemplate` uses `DataManager` and `ChestStateRegistry` to control opening state and drops.

In `DeltaGen`, dynamically generated chests can implement `IGeneratedChest`.

## Environments

Biome and visual profiles are spread across:

- `BiomeEnvironments/`
- `Environment/`
- generator-specific configuration

## Notes

- `FiberGen` still uses its own `ChunkData`
- `RubyGen` and `SaveCreationLoading` still concentrate several internal classes in very large files
- the project still mixes experimental paths with the current playable path

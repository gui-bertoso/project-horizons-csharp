# Overview

## What the project is

Project Horizons C# is a top-down Godot game project built around:

- simple but expandable combat
- data-driven content
- multiple procedural generation approaches
- fast system iteration

The project is not a finished product yet. Right now it works as both a playable base and a structure playground.

## Current pillars

- items and weapons defined through resources
- enemies built on a shared template
- projectiles decoupled from weapons
- persistent save slots
- global settings stored separately from world saves
- multiple level generators living in the same project

## Current loop

1. choose or create a save
2. load a procedural level
3. explore, fight, open chests, and equip items
4. save progress
5. repeat through later levels

## Most important systems

- `DataManager`: persists saves, progression, equipment, and settings
- `Globals`: runtime global references
- `EnemiesManager`: enemy spawning and simple LOD grouping
- `SettingsApplier`: applies visual and technical settings
- `HUD` and `PauseMenu`: main runtime interface

## Areas that still need attention

- balancing is still being refined
- several classes still use placeholder names and text
- some scripts are still too large
- not every exported field has real gameplay impact yet

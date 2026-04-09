# Enemy System

## Overview

Enemies inherit from `EnemyTemplate` and specialize behavior when needed.

## Main files

- `Enemies/EnemyTemplate/EnemyTemplate.cs`
- `Autoload/EnemiesManager.cs`
- `Enemies/EnemyGroup.cs`
- enemy subfolders inside `Enemies/`

## What the base template does

- health, damage, and XP reward
- simple state machine
- player distance checks
- incoming damage application
- invulnerability frames after hits
- kill rewards

## Spawning and management

`EnemiesManager` centralizes:

- spawn tables by biome
- enemy instantiation by path
- LOD groups

`EnemyGroup` applies `UpdateCooldown` to grouped enemies to reduce update cost.

## Damage flow

- player melee damage comes through `PhysicsWeapon`
- player projectile damage comes through `Projectile`
- the enemy calls `ApplyDamage(int value)` and spawns floating text

## Current state

The system works, but it is still at baseline gameplay maturity.

Main areas to improve:

- richer attack behavior per enemy
- more consistent hitbox and attack-area patterns
- better scaling by biome and level

# Projectile System

## Overview

Projectiles are scenes derived from `Projectile.tscn` with shared behavior in `Projectile.cs`.

## Common base

`Projectile.cs` controls:

- speed
- direction through `Rotation`
- lifetime
- damage

Each variant changes:

- sprite
- collision shape
- `Damage`
- `Speed`

## Main files

- `Projectiles/Projectile.cs`
- `Projectiles/Projectile.tscn`
- `Weapons/DistanceWeapon.cs`

## Flow

1. a ranged weapon points to a projectile `PackedScene`
2. `DistanceWeapon` instantiates the projectile
3. the projectile spawns at `ProjectileSpawn`
4. collision on a valid target applies `Damage`

## Current variants

There are projectile variants for:

- debug book / magic weapons
- bows
- guns
- rod
- spell
- boomerang
- enemy attacks such as `SeedProjectile` and `greenSeed`

## Important notes

- the collision shape must be configured in the scene
- the real gameplay damage currently lives on the projectile itself
- if two weapons share the same projectile scene, they also share balancing

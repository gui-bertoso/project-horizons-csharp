# Development Guide

## Requirements

- Godot 4.7 with C# support
- a compatible .NET SDK

## Quick command

```bash
dotnet build project-horizons-cs.csproj
```

## Recommended structure

### Prefer data-first when the system supports it

When adding item, weapon, enemy, or chest content:

1. create or reuse a `.tres` resource
2. create the concrete scene if the item needs real visual behavior or its own hitbox
3. wire the resource into the existing system
4. test it in the in-game flow

### Practical rules

- prefer reusing `Weapon`, `PhysicsWeapon`, and `DistanceWeapon`
- if behavior can change through configuration, try solving it with `.tscn` or `.tres`
- if the difference is real logic, create a new script
- keep utility classes close to the system that owns them

## When changing saves

Always review `Autoload/DataManager.cs` when adding:

- a new persistent player field
- a new equipment slot or equipment field
- a new world-state field

If you forget serialization, the data will disappear across loads.

## When changing UI

- full-screen menus belong in `Interface/`
- runtime UI should prefer overlays inside the `HUD`
- if the UI must work while the game is paused, configure `ProcessMode` correctly

## When changing combat

At the moment, the most relevant gameplay values are:

- `Damage` on melee weapons
- `Damage`, `Speed`, and collision shape on projectiles

`Cooldown` and `UseSpeed` exist on the weapon base, but they do not yet drive all combat behavior.

## Areas that deserve future refactoring

- very large generator scripts
- placeholder names and texts
- real class separation inside player combat logic
- save-format documentation

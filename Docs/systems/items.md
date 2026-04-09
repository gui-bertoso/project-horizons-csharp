# Item System

## Overview

The item system is built around `Item.cs` and `.tres` resources.

Each item can represent:

- a weapon
- armor
- a consumable
- an accessory

## Main files

- `Items/Item.cs`
- `Items/*.tres`
- `Interface/ItemsDisplay/ItemsDisplay.cs`
- `Player/PlayerHand.cs`

## Important fields

### `ItemType`

Defines the general category of the item.

### `ItemClass`

Defines the functional class. Right now gameplay mainly uses it to distinguish melee from ranged.

### `ItemTexture`

Sprite used in the inventory and as a fallback visual when there is no concrete scene.

### `ItemScene`

Scene instantiated when the item is equipped and needs real runtime behavior.

## Current flow

1. the item is loaded from a `.tres`
2. when equipped, `PlayerHand` tries to instantiate `ItemScene`
3. if there is no scene, it falls back to a plain sprite
4. the UI stores the equipped item in `DataManager.CurrentWorldData`

## Notes

- important weapons should have `ItemScene`
- items without `ItemScene` still work visually, but with less behavior
- placeholders still exist in `ItemName` and descriptions

## Desired improvements

- clean up names and descriptions
- better separate classes such as mage, wizard, archer, and bommet at runtime
- add clearer progression and drop metadata

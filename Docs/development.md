# Development Guide

## Setup

- engine: Godot (C# / Mono)
- language: C#

## Project Structure

- Autoload → global systems
- Items → item logic
- Enemys → enemy logic
- Projectiles → projectile system
- Assets → resources and visuals

## Guidelines

- keep systems isolated
- prefer data-driven design
- avoid hardcoding values
- reuse components when possible

## Workflow

1. define data (.tres)
2. implement logic (C#)
3. integrate with systems
4. test in isolated scenarios
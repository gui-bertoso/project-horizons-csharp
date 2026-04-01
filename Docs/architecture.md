# Architecture

## Overview

The project follows a modular and system-oriented architecture.

Instead of tightly coupling logic, systems are separated into distinct responsibilities and interact through shared state and global managers.

## Core Layers

### Autoload Systems

Global managers responsible for persistent logic:

- AchievementsManager
- EnemysManager
- SettingsApplyer

These act as service layers accessible from anywhere.

### Game Systems

- Items
- Enemies
- Projectiles

Each system is responsible for its own behavior and data.

### Data Layer

Uses Godot resources (.tres) for defining:

- items
- environments
- configurations

This allows content to be modified without changing code.

### Rendering Layer

- shaders (hitflash, etc.)
- sprites and UI assets

## Design Goals

- modularity
- scalability
- maintainability
- performance readiness

## Future Direction

- chunk-based world system
- ECS-like architecture
- multithreading / job systems
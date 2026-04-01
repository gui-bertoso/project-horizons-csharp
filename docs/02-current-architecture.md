# arquitetura atual

## autoloads (singletons)

registrados em `project.godot`:

- `Globals` — estado global e referências runtime
- `DataManager` — save global + world data
- `AchievementsManager` — tracking de progresso
- `SettingsApplyer` — config
- `EnemysManager` — (provável coordenação de inimigos)

### problema atual

`Globals` virou ponto central de tudo:

- player
- stats container
- level generator
- ui

isso é útil, mas gera alto acoplamento.

## camadas (implícitas)

### gameplay

- Player
- Enemys
- Weapons
- Items

### world

- LevelGenerator
- ProceduralLevel
- Environment

### persistence

- DataManager

### meta systems

- AchievementsManager

### ui

- Interface/*

## observação importante

não existe separação clara entre:

- domain (regras puras)
- gameplay (execução)
- presentation (ui)

isso ainda tá tudo meio misturado.

👉 isso não é errado agora, mas vai virar problema quando o projeto crescer.

# overview

## o que esse projeto é hoje

um jogo em godot + c# com foco em:

- sistemas (player, inimigos, armas)
- geração procedural de níveis
- progressão por runs (loop de níveis + portal)

## estrutura geral

- player (`Player/Player.cs`) controla movimento, dash, ataque e coleta
- stats (`PlayerStats.cs`) centraliza atributos e dano
- inimigos (`Enemys/*`) herdam comportamento base
- geração procedural (`LevelGenerators/BruteGen/LevelGenerator.cs`)
- save (`Autoload/DataManager.cs`)
- achievements (`Autoload/AchievementsManager.cs`)

## loop atual

1. spawn no nível procedural
2. explorar e lutar
3. coletar itens
4. encontrar portal
5. ir para próximo nível

## identidade técnica

isso aqui já é claramente um projeto orientado a sistemas (bom sinal), mas com forte uso de singletons globais (`Globals`, `DataManager`, etc).

isso facilita desenvolvimento rápido, mas pode virar gargalo de manutenção depois.

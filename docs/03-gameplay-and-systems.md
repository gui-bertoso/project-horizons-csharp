# gameplay systems

## player

arquivo: `Player/Player.cs`

features:

- movimento com input vector
- dash com cooldown e charges
- ataque melee (sword)
- ataque ranged com state machine
- coleta de itens

### ponto forte

boa separação interna (funções por comportamento).

### ponto fraco

muita responsabilidade numa classe só.

👉 futuro: dividir em componentes (movement, combat, dash)

## stats

arquivo: `PlayerStats.cs`

- controla vida, velocidade, dash
- aplica dano
- spawn de feedback visual

problema:

usa string (`"Decrease"`) → frágil

## inimigos

pasta: `Enemys/`

- múltiplos tipos
- bosses
- provavelmente herdam `EnemyTemplate`

sistema já escalável 👍

## armas

- `PhysicWeapon`
- `DistanceWeapon`

ligadas ao `PlayerHand`

## itens

- `Items/Item.cs`
- `PhysicItem`

integra com coleta do player

## achievements

arquivo: `AchievementsManager.cs`

- baseado em eventos
- check por trigger string

boa base pra expandir

## conclusão

os sistemas existem e já funcionam bem juntos.

isso aqui já é um core de jogo real, não só experimento.

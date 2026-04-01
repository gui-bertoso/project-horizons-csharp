# sistemas de gameplay

## lista principal de sistemas

> ajustar conforme o projeto real

- movimentação do jogador
- câmera
- interação com mundo
- inventário
- itens e recursos
- crafting
- combate
- atributos/status
- construção
- exploração
- clima / tempo / ciclo dia-noite
- progressão
- quests ou objetivos
- economia / loot / drops

## movimentação

responsável por:
- deslocamento do jogador
- estados de movimento
- velocidade base e modificadores
- interação com terreno e colisão

decisões importantes:
- física ou character controller?
- aceleração arcade ou mais simulada?
- stamina afeta locomoção?
- terreno afeta velocidade?

## combate

responsável por:
- dano
- defesa
- hit detection
- cooldowns
- estados de ataque
- stagger / knockback / i-frames

perguntas:
- combate melee, ranged ou híbrido?
- o dano é instantâneo, por projétil ou por área?
- existe lock-on?
- como o feedback visual/sonoro comunica impacto?

## inventário

precisa definir:
- limite por slot, peso ou volume
- stack de itens
- hotbar
- quick use
- drop no mundo
- sincronização com save

## crafting

deve suportar:
- recipes
- validação de ingredientes
- crafting instantâneo ou com tempo
- estações de trabalho
- tiers de progressão

## progressão

formas possíveis:
- xp e níveis
- desbloqueio de recipe
- árvore de talentos
- upgrades de equipamento
- progressão por descoberta

## sistema de estados

muitos sistemas devem compartilhar estados como:
- idle
- move
- jump
- attack
- interact
- dead
- stunned
- crafting
- menu_open

idealmente, estados globais não devem virar bagunça acoplada.

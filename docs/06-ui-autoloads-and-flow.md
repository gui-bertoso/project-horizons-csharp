# ui, autoloads e fluxo

## ui

pasta: `Interface/`

- main menu
- saves manager
- debug panel
- splash screen
- items display

## fluxo principal

menu → create/load save → level → portal → next level

## autoloads importantes

- `Globals` → estado runtime
- `DataManager` → dados
- `AchievementsManager` → progresso

## problema

ui acessa diretamente dados globais

isso acopla muito:

ui → data → gameplay

## sugestão

- criar camada de service
- ou eventos (signals)

exemplo:

ui não deveria alterar `CurrentWorldData` direto

# project horizons c# docs

esta pasta documenta a versão atual do projeto com base na estrutura real do repositório.

## objetivo

registrar como o jogo está organizado hoje, quais sistemas já existem e onde faz sentido evoluir sem bagunçar a base.

## docs disponíveis

- `01-overview.md` — visão geral do projeto e estado atual
- `02-current-architecture.md` — arquitetura atual baseada nas pastas e autoloads reais
- `03-gameplay-and-systems.md` — player, armas, inimigos, itens, achievements
- `04-procedural-generation.md` — geração procedural, biomas, portais e fluxo de nível
- `05-save-and-persistence.md` — save global, save de mundo e fluxo dos menus de save
- `06-ui-autoloads-and-flow.md` — menus, debug, overlays e singletons
- `07-roadmap-and-refactors.md` — próximos passos e refactors recomendados

## resumo honesto

hoje o projeto já tem uma base jogável de:

- player com movimento, dash, coleta e combate
- armas melee e ranged
- geração procedural por tilemap
- múltiplos inimigos e bosses
- saves de mundo
- achievements
- menus e debug tools

mas a arquitetura ainda está em fase de crescimento. ela funciona, porém já mostra sinais de acoplamento por singleton global, uso forte de `CurrentWorldData` como dicionário genérico e alguma mistura entre regra de jogo, ui e persistência.

traduzindo: está promissor, mas é a hora perfeita de documentar antes do projeto virar um monstro radioativo kkk.

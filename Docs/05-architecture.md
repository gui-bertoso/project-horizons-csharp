# arquitetura

## princípios

- single responsibility
- baixo acoplamento
- alta coesão
- composição > herança quando possível
- dados separados de comportamento quando fizer sentido

## camadas sugeridas

### domain
regras centrais do jogo.

### gameplay
sistemas ativos do runtime.

### data
configs, definitions, tables, seeds, recipes, stats.

### infrastructure
save, loading, input, asset management, logging, integração externa.

### presentation
ui, áudio, vfx, animação, feedback.

## padrões úteis

- event bus / signals para comunicação desacoplada
- state machine para entidades complexas
- strategy pattern para comportamentos variáveis
- factory para criação consistente de entidades
- service locator com moderação, sem virar deus ex gambiarra
- dependency injection se o projeto realmente se beneficiar disso

## convenções

- classes com uma responsabilidade clara
- managers só quando realmente coordenarem subsistemas
- evitar classes gigantes com 900 funções e depressão embutida
- nomes explícitos
- métodos curtos e previsíveis

## exemplo de módulos

- `Player`
- `World`
- `Combat`
- `Inventory`
- `Crafting`
- `SaveSystem`
- `AI`
- `UI`
- `Audio`
- `DebugTools`

## fluxo de dependências ideal

presentation -> gameplay -> domain/data -> infrastructure

evitar dependências invertidas bizarras, tipo ui decidindo regra de dano. aí já virou crime.

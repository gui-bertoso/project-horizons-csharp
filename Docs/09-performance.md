# performance

## objetivo

manter frame time estável e uso de memória previsível.

## métricas principais

- frame time
- cpu time por sistema
- gpu time
- gc alloc
- uso de memória
- tempo de loading
- número de draw calls
- número de entidades ativas

## fontes comuns de custo

- alocações por frame
- buscas caras em update
- instantiate/destroy excessivo
- física demais
- navegação demais
- ui rebuild excessivo
- serialização pesada
- lógica de geração sem budget

## estratégias

- pooling
- cache
- spatial partitioning
- update em intervalos
- async/job system quando fizer sentido
- batching
- culling
- lod
- chunking / streaming

## metodologia

1. medir
2. localizar gargalo
3. corrigir o gargalo real
4. medir de novo

sem isso tu só pratica placebo de performance, que é o crossfit do profiling.

## orçamento sugerido

> ajustar ao target

- 60 fps = 16.67 ms/frame
- 120 fps = 8.33 ms/frame

## checklist de revisão

- esse sistema aloca em runtime?
- isso roda todo frame ou poderia rodar menos?
- isso precisa existir longe do jogador?
- isso pode ser pré-calculado?
- essa estrutura de dados é adequada?

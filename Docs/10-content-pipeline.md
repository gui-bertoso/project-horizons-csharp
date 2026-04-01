# pipeline de conteúdo

## objetivo

facilitar adição e manutenção de conteúdo sem quebrar código central.

## tipos de conteúdo

- itens
- recipes
- inimigos
- biomas
- estruturas
- quests
- diálogos
- efeitos
- sons
- ui data

## abordagem recomendada

usar data-driven design sempre que possível.

isso significa:
- configs em arquivos ou assets próprios
- comportamento ligado a definições
- menos hardcode espalhado em gameplay

## vantagens

- iteração rápida
- menos recompilação
- melhor separação entre sistema e conteúdo
- balanceamento mais fácil

## riscos

- dados inconsistentes
- ids quebrados
- references órfãs
- explosão de campos sem validação

## mitigação

- validadores
- defaults seguros
- schemas bem definidos
- tooling interna de debug

# save e load

## objetivo

garantir persistência confiável, rápida e compatível com evolução do projeto.

## o que salvar

- progresso do jogador
- inventário
- equipamentos
- posição atual
- estado do mundo alterado
- construções
- entidades persistentes
- flags de progressão
- configurações relevantes

## o que não salvar diretamente

- dados totalmente regeneráveis pela seed
- caches transitórios
- referências temporárias de runtime
- objetos derivados que podem ser reconstruídos

## estrutura sugerida

- `SaveMetadata`
- `PlayerSaveData`
- `WorldSaveData`
- `ChunkModificationData`
- `QuestSaveData`
- `SettingsSaveData`

## versionamento

todo save deve conter:
- versão do save
- versão do build
- timestamp
- seed do mundo

isso ajuda a:
- migrar saves antigos
- detectar incompatibilidades
- debugar corrupção

## fluxo de salvamento

1. coletar snapshots de sistemas
2. serializar para estrutura intermediária
3. validar
4. escrever arquivo temporário
5. substituir save antigo de forma segura

## fluxo de carregamento

1. ler metadata
2. validar compatibilidade
3. desserializar
4. reconstruir estado base
5. aplicar dados persistidos
6. validar runtime final

## riscos

- save quebrar após refactor
- duplicação de entidades
- ordem incorreta de restauração
- dependências entre sistemas mal resolvidas

## recomendação

criar testes simples para:
- save/load do jogador
- save/load de chunk alterada
- retrocompatibilidade de versão

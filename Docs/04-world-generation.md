# geração de mundo e streaming

## objetivo

o sistema de mundo deve permitir expansão controlada, geração procedural e carregamento eficiente.

## componentes esperados

- seed global
- gerador de terreno
- gerador de biomas
- distribuição de recursos
- spawn de entidades
- chunks / regions / sectors
- sistema de loading e unloading

## estrutura do mundo

> exemplo

- mundo dividido em chunks
- chunks agrupados em regiões
- cada chunk possui dados de terreno, objetos, entidades e estado salvo

## requisitos

- geração determinística por seed quando aplicável
- persistência de alterações do jogador
- carregamento incremental
- descarregamento seguro
- baixo custo por frame

## pipeline de geração

1. determinar coordenadas da chunk
2. calcular parâmetros base
3. gerar terreno
4. aplicar bioma
5. distribuir recursos
6. spawnar props e entidades
7. aplicar modificações persistidas
8. ativar chunk

## streaming

o streaming deve considerar:
- posição do jogador
- direção / velocidade
- distância de interesse
- orçamento por frame
- prioridade de carregamento

## persistência procedural

regra importante:
- conteúdo derivado de seed pode ser recalculado
- conteúdo alterado pelo jogador deve ser salvo

isso reduz custo de save e evita salvar meio universo à toa.

## riscos técnicos

- hitching ao gerar chunks
- garbage collection por alocações temporárias
- custo excessivo com instantiation/destruction
- duplicação de entidades após load
- inconsistência entre geração e persistência

## mitigação

- pooling
- jobs / async quando fizer sentido
- budgets por frame
- cache de dados
- serialização compacta
- separação entre dados estáticos e dados mutáveis

# geração procedural

arquivo principal: `LevelGenerators/BruteGen/LevelGenerator.cs`

## como funciona

- usa `FastNoiseLite`
- gera mapa baseado em radius + noise
- aplica bioma (id 0–5)
- gera detalhes em camadas

## biomas

comentados no código:

- forest
- dark forest
- dry forest
- snow
- ice
- desert

## pipeline

1. define seed
2. gera terrain base
3. aplica variação de costa
4. aplica detalhes
5. spawn portais

## portais

- initial portal → perto do centro
- exit portal → mais distante

isso cria fluxo de progressão natural

## ponto forte

algoritmo já robusto e com variação boa

## pontos de melhoria

- separar geração em classes menores
- desacoplar lógica de tilemap
- permitir seed fixa (debug/replay)

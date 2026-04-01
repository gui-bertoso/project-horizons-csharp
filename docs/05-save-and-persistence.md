# save system

arquivo principal: `Autoload/DataManager.cs`

## dois tipos de dados

### game data

- settings
- lista de saves

salvo em:

user://save.txt

### world data

- player stats
- posição
- seed
- dificuldade

salvo em:

user://<save_name>.txt

## estrutura

usa `Dictionary<string, Variant>`

isso é flexível mas perigoso:

- sem type safety
- fácil quebrar chave

## fluxo

- create save → salva world + adiciona lista
- load save → carrega dictionary
- quick save → usa nome atual

## integração com ui

`SavesManager.cs`:

- cria save
- lista saves
- carrega dados

## problema principal

string-based data ("PlayerHealth", etc)

👉 solução futura:

- criar classes fortemente tipadas
- ou wrapper para acesso seguro

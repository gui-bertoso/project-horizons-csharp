# padrões de código

## princípios

- nomes claros
- métodos pequenos
- responsabilidades separadas
- evitar side effects ocultos
- preferir previsibilidade a “esperteza”

## nomenclatura

- classes: `PascalCase`
- métodos: `PascalCase`
- campos privados: `_camelCase`
- parâmetros/variáveis locais: `camelCase`
- constantes: `UPPER_CASE` ou padrão definido pelo projeto

## boas práticas

- validar entradas públicas
- logar falhas relevantes
- documentar decisões incomuns
- evitar magia numérica
- preferir enums/structs/objetos semânticos a ints soltos

## evitar

- classes deus
- heranças profundas demais
- utilitários globais para tudo
- comentários redundantes
- lógica de negócio misturada com ui

## comentário bom

explica **por que** algo existe ou **por que** foi feito daquele jeito.

## comentário ruim

explica o óbvio e ocupa oxigênio.

exemplo ruim:
```csharp
health = 10; // seta a vida para 10
```

exemplo útil:
```csharp
// valor inicial reduzido para acelerar testes do loop de combate no protótipo
health = 10;
```

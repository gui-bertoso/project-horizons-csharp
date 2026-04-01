# organização de pastas

## sugestão

```text
project-horizons/
├─ docs/
├─ src/
│  ├─ core/
│  ├─ gameplay/
│  ├─ world/
│  ├─ ai/
│  ├─ ui/
│  ├─ data/
│  ├─ save/
│  ├─ tools/
│  └─ debug/
├─ assets/
├─ tests/
└─ builds/
```

## regras

- `core/` para bootstrap, base abstractions e utilitários centrais
- `gameplay/` para regras e sistemas jogáveis
- `world/` para chunking, geração, streaming e entidades ambientais
- `ai/` para percepção, decisão e navegação
- `ui/` para interface e apresentação
- `data/` para definições, configs e tabelas
- `save/` para serialização, snapshots e migração
- `tools/` para utilitários de editor/dev
- `debug/` para gizmos, métricas e inspeção

## regra prática

se uma pasta vira “misc”, “stuff”, “newfolder2” ou “finalfinal_agoraVai”, algo deu ruim.

# Câmara segura HD modular v6 — 2026-11-06

A nova arte do refúgio está integrada na Unity com um fundo 1024×1536 (`Assets/Resources/Backgrounds/safe_chamber_v3.jpg`) e oito sprites RGBA independentes (`SanctuaryProps/`: `bed`, `wardrobe`, `fountain`, `meditation_altar`, `mount_nest`, `entrance_arch`, `banner`, `lantern`). O tamanho dos PNGs finais vai até aproximadamente 1350 pixels no maior eixo. Os arquivos são PNGs de paleta otimizada, com transparência, não miniaturas para esticar.

A passagem de pedra já está pintada no fundo. Por isso o `entrance_arch` é guardado como asset separado, mas marcado como `disabled` no JSON para evitar um arco duplicado. Estão ativos cama à esquerda ao norte, fonte à direita, armário à esquerda, altar à direita e ninho no canto inferior esquerdo, com centro livre. As posições são editáveis em `sanctuary_props.json`.

O novo `Assets/Editor/NecroSanctuaryImportSettings.cs` aplica bilinear com `Clamp`, sem mipmaps, sem compressão de textura no Android, sRGB e limite de 2048 px em Android. Texturas sem compressão usam mais memória que ASTC; testar desempenho em aparelhos mais modestos.

A correção v5 de proporções, ordem de sprites por pés e colisões independentes foi preservada. Compilação e teste Android pendentes: validar nitidez, ausência de arco duplicado, colisões ao redor dos móveis, movimentação até a saída e retorno do zoom em combate.

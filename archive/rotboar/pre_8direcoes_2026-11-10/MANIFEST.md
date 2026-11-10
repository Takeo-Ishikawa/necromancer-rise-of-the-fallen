# Rotboar — cópia de segurança antes das animações de 8 direções

Esta pasta contém cópias binárias **exatas** dos sprites que estavam em uso antes da substituição, além do código responsável por renderizar e reanimar o Rotboar.

Commit de origem: `a495aa6c738d7a852968e5a74b20a27761ad5cd9` (branch `main`).

Os PNG abaixo usam os mesmos blobs do GitHub, sem reconversão, recompressão ou alteração de bytes:

- `rotboar.png` — SHA d3b012ebbead84c481b14e9330579e356474faf3 — 150737 bytes
- `rotboar_ally.png` — SHA 688f968e7db94dd4aecb47b882f28ac6ae38d85e — 1187494 bytes
- `rotboar_fallen.png` — SHA cee50cbb77adde10f5512e54fbc0d8d5c6b0d5a7 — 155685 bytes

Restaurar: copie os PNG e os arquivos C# arquivados de volta aos caminhos originais, reverta as chamadas novas e execute os workflows manuais. A pasta `archive/` não é carregada pelo Unity em tempo de execução.

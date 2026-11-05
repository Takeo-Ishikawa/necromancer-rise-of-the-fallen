# Câmara inicial vazia

A partida abre no refúgio antes da Caverna Esquecida. Siga ao norte e toque em
ENTRAR NA CAVERNA. Na caverna, aproxime-se do ponto de chegada para usar
VOLTAR AO REFÚGIO. A transição exige toque no botão, não acontece ao caminhar.

A câmara contém somente a imagem aprovada: piso, paredes e passagem natural.
Não há móveis, fonte, bônus, altar ou montaria nesta etapa. Não há inimigos,
respawn nem dano no refúgio. Servos ativos acompanham a personagem e são
reposicionados procurando espaço livre. Projéteis e ordens de ataque são
limpos na passagem; o estado dos inimigos da caverna permanece na sessão.

## Arquivos para ajustes

- `unity/Assets/Resources/Backgrounds/safe_chamber.png`: arte original, 1024×1536.
- `unity/Assets/Resources/Data/sanctuary.json`: dimensão 16×24 e posição inicial.
- `unity/Assets/Scripts/NecroSanctuary.cs`: limites do piso, ponto de saída,
  navegação e posicionamento de servos.

A sala é separada do catálogo numerado de combate, preservando os índices das
regiões existentes. O armazenamento de companheiros continua no formato atual.
O refúgio não grava suas coordenadas sobre o save da região de combate.
Ao reabrir o jogo, a personagem começa no refúgio e carrega os companheiros.

Compilação, testes e validação no aparelho não executados, conforme solicitado.
Commit marcado com [skip ci] para não disparar builds automáticos.

# Necromante Física

Terceira opção disponível na seleção inicial. O progresso e os servos já salvos continuam compartilhados entre personagens.

## Arte

`Characters/Unarmed/combat.png`: atlas RGBA 1024×2048, 32 células de 256×256. Linhas: sul, sudoeste, oeste, noroeste, norte, nordeste, leste, sudeste. Colunas: espera, passo A, passo B, soco. São 8 poses de espera, 16 passos e 8 ataques. A caminhada alterna passo A, espera, passo B, espera, avançando pela distância efetivamente percorrida.

Fonte em `tools/unarmed-source.png`; empacotamento reproduzível com `python tools/prepare_unarmed.py` (Pillow). Não é animação por ossos nem interpolação.

## Combate

Ataque manual corpo a corpo, alcance de 1,35 unidades. A propriedade `meleeOnly` bloqueia projéteis, Pulso Necrótico, Ordem dos Mortos, reanimação e aquisição de habilidades mágicas, independentemente das habilidades do save. Atalhos e botões usam os mesmos bloqueios. Atributos e administração dos servos existentes permanecem disponíveis.

## Validação

Verificações estáticas do projeto e dos 32 recortes. Compilação Unity e teste de fluidez no aparelho ainda necessários. Para testar, compilar a main, selecionar Necromante Física, andar nas oito direções e atacar um inimigo próximo. Testar também Q/E/R com save que já tenha magia desbloqueada.

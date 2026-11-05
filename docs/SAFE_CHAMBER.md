# Câmara segura / refúgio

O jogo começa na câmara segura antes da Caverna Esquecida. Ao norte, a
passagem oferece **ENTRAR NA CAVERNA**; no início da primeira região de combate
há **VOLTAR AO REFÚGIO**. As duas transições exigem toque no botão.

Não há inimigos, respawn ou dano no refúgio. Servos ativos acompanham a
necromante e são posicionados em áreas livres. Projéteis e ordens de combate
são limpos ao mudar de cenário. Nenhum índice de fase nem formato de save é
modificado.

## Arte separada em oito objetos

O fundo original permanece em
`unity/Assets/Resources/Backgrounds/safe_chamber.png`.

A decoração é definida em
`unity/Assets/Resources/Data/sanctuary_props.json`. Cada objeto recebe seu
próprio GameObject, SpriteRenderer, tamanho, posição e (quando necessário)
footprint de colisão. A arte deve existir exatamente neste local:

| Recurso (Resources/SanctuaryProps) | Elemento | Função futura |
| --- | --- | --- |
| `entrance_arch.png` | Arco ornamentado de entrada | Transição existente |
| `protective_sigil.png` | Sigilo turquesa no chão | Proteção estética |
| `wardrobe.png` | Guarda-roupa | Equipamentos |
| `bed.png` | Cama | Por enquanto decoração |
| `meditation_altar.png` | Altar | Árvore de habilidades/atributos |
| `meditation_mat.png` | Esteira | Meditação |
| `fountain.png` | Fonte | Bônus temporário de dano/velocidade |
| `mount_nest.png` | Ninho | Descanso de montaria/pet |

**IMPORTANTE:** os PNGs das oito decorações devem ser adicionados separadamente
ao repositório. O código verifica cada recurso em runtime: se um PNG estiver
ausente, o objeto não é renderizado **nem bloqueia a movimentação**.

A pasta pode ser criada arrastando os oito arquivos, com exatamente esses
nomes, para `unity/Assets/Resources/SanctuaryProps/` dentro do projeto Git.
Não é preciso alterar o JSON para que as imagens apareçam ao carregar a
câmara. As imagens devem ter transparência; o importador de texturas da Unity
preserva o canal alpha e usa compressão com mipmaps desativados.

## Ajustes de layout

O mundo tem 16 × 24 unidades. Em `sanctuary_props.json`, cada objeto contém:

- `x`, `y`: ponto de inserção (base inferior para móveis; centro para
  camadas `floor`).
- `width`, `height`: dimensões visuais em unidades do mundo.
- `layer`: `wall` atrás da personagem, `floor` no chão ou `object`
  com profundidade baseada em Y.
- `blockOffsetX`, `blockOffsetY`, `blockWidth`, `blockHeight`:
  área de colisão aproximada, independente da arte. Campos omitidos significam
  decoração atravessável.

O caminho central até o arco permanece livre. No refúgio, a personagem e os
servos usam SortingGroup por coordenada Y; ao voltar ao combate esse grupo é
desabilitado, restaurando a lógica de renderização original.

## Escopo

Somente ambientação, circulação e organização técnica. O guarda-roupa, a
fonte, o altar e o ninho **ainda não concedem funcionalidades novas**.

Arquivos principais:
- `unity/Assets/Scripts/NecroSanctuary.cs`: refúgio, limites, transição.
- `unity/Assets/Scripts/NecroSanctuaryDecor.cs`: importação, objetos,
  footprints e ordenação de profundidade.
- `unity/Assets/Resources/Data/sanctuary_props.json`: composição editável.

Compilação Android e validação no aparelho não executadas nesta alteração.

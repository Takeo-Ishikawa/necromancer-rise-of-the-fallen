# Câmara segura — cenário modular

A câmara do refúgio começa antes da Caverna Esquecida. A necromante entra na
caverna pela passagem ornamentada ao norte (botão **ENTRAR NA CAVERNA**) e
retorna pela primeira região (botão **VOLTAR AO REFÚGIO**).

## Layout aprovado — arte independente

O plano corresponde à composição visual aprovada: arco na parede norte;
cama no canto superior esquerdo; fonte no superior direito; guarda-roupa na
parede esquerda; altar à direita; ninho da montaria no canto inferior esquerdo.
O centro fica intencionalmente desobstruído para personagem e servos. O sigilo
turquesa fica sobreposto à passagem, e a esteira fica ao lado do altar.

O fundo continua em `unity/Assets/Resources/Backgrounds/safe_chamber.png`.
As oito imagens decorativas **não são incorporadas ao fundo**: o jogo instancia
um GameObject independente por decoração (um contêiner sem escala, um SpriteRenderer
de arte e, para os móveis, uma sombra de contato separada).

| PNG em `unity/Assets/Resources/SanctuaryProps/` | Posição |
| --- | --- |
| `entrance_arch.png` | Arco e estandartes ao norte |
| `protective_sigil.png` | Emblema sobre a passagem |
| `bed.png` | Cama no superior esquerdo |
| `fountain.png` | Fonte no superior direito |
| `wardrobe.png` | Guarda-roupa na lateral esquerda |
| `meditation_altar.png` | Altar e almofada na lateral direita |
| `meditation_mat.png` | Esteira auxiliar junto ao altar |
| `mount_nest.png` | Descanso da montaria no inferior esquerdo |

Os PNGs enviados pelo usuário devem ser importados com esses nomes exatos.
Se faltar alguma imagem, a decoração correspondente não aparece e sua colisão
não é ativada. **Os arquivos binários ainda precisam ser enviados ao GitHub**;
isso não impede a configuração da cena, mas impede vê-la completa na Unity
até que as imagens sejam adicionadas.

## Correção do contato com o piso

As sombras de contato são elipses suaves geradas pela Unity e associadas
individualmente ao GameObject de cada móvel. No layout v3, `shadowOffsetX`
e `shadowOffsetY` são calibrados em relação ao **contato visível** de cada
PNG, não à borda do arquivo (que pode conter pixels transparentes). Isso
corrige o deslocamento observado na prévia anterior, especialmente em
guarda-roupa, fonte e ninho.

As sombras não alteram posição, escala ou hitbox dos sprites originais.
Se um móvel mudar de arte, revise sua sombra no JSON junto com o
respectivo pivô. A esteira, o sigilo e o arco não recebem sombra artificial.

## Como ajustar um elemento

Edite `unity/Assets/Resources/Data/sanctuary_props.json`. O arquivo contém:

- `x`, `y`: posição em unidades de mundo (largura 16, altura 24).
- `width`, `height`: tamanho independente de cada arte.
- `layer`: `wall` atrás de personagens, `wall_overlay` sobre o arco,
  `floor` para os decalques, `object` com ordenação por Y.
- `blockOffsetX`, `blockOffsetY`, `blockWidth`, `blockHeight`:
  footprint de colisão próximo à base, não a figura inteira.
- `shadowWidth`, `shadowHeight`, `shadowOffsetX`, `shadowOffsetY`:
  sombra suave opcional, independente da textura original.

O código correspondente está em
`unity/Assets/Scripts/NecroSanctuaryDecor.cs`. Não altere a imagem-base
para mover móveis. Um ajuste deve envolver apenas o item no JSON.

## Gameplay e compatibilidade

A personagem e os servos usam ordenação de sprites por Y no refúgio.
Ao retornar ao combate, o sorting original é restaurado.
A circulação central (x aproximadamente 6 a 10 unidades) continua aberta.
A sala não possui inimigos, dano ou respawn. Transições limpam projéteis e
ordens de combate, sem modificar saves nem índices das regiões existentes.

As mecânicas do guarda-roupa, da fonte, do altar e da montaria ficam para
etapas futuras. Nenhuma funcionalidade foi ativada pela montagem decorativa.

A compilação Unity/Android e os testes no aparelho não foram executados;
recomenda-se validar a renderização, colisões e importação dos PNGs localmente.

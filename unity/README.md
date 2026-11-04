# Necromancer — 0.6.5a · Protagonista animada

A versão 0.6.5a inclui arte de poses ilustradas integrada ao Unity, um novo atlas de quatro direções e seis conjuntos de animação. A base de combate, a Cripta, o respawn, os 12 pontos de comando e o formato do save permanecem inalterados.

## Direção de arte aplicada

A necromante mantém chapéu violeta com caveira, cabelos prateados, traje escuro, botas e cajado com brilho espectral. A arte-base foi extraída e tratada a partir de nossas próprias ilustrações aprovadas.

O arquivo integrado `Assets/Resources/AnimationSheets/necromante_source.png` contém **16 poses/vistas em PNG transparente**. São quatro vistas realmente diferentes (frente, costas, esquerda e direita) e 12 estudos de poses: repouso, caminhada, ataque, conjuração, ressuscitar e dano, duas variantes de cada. Este arquivo é usado no build e não é apenas imagem de referência.

## Quadro a quadro dentro do Unity

O `NecroHeroSheetBuilder` compõe os quadros a partir dessas artes, com deslocamentos rígidos por região do corpo e efeitos desenhados sobre o frame — em vez de esticar a mesma figura frontal em todas as direções.

| Ação | Quadros por direção | FPS | Estado |
| --- | ---: | ---: | --- |
| Repouso | 4 | 4 | Parado |
| Caminhada | 8 | 11 | Movimento |
| Ataque físico | 6 | 12 | Golpe básico |
| Conjuração | 6 | 10 | Ataque automático mágico |
| Ressurreição | 8 | 9 | Botão Reviver |
| Reação a dano | 3 | 12 | Quando atingida |

Total: **140 sprites utilizados em 24 clipes** (6 ações × 4 direções), criados e armazenados em cache uma única vez na primeira execução. Os quadros são sprites distintos, embora **nem todos tenham sido desenhados manualmente**; os intermediários são composições da arte-base. A forma frontal já possui variantes próprias de cada ação; costas e perfis mantêm os desenhos próprios com poses/effects derivados.

A biblioteca `NecroFrameLibrary.cs` continua responsável por todas as sequências e aceita arquivos de atlas completos importados de artistas no futuro. `NecroCharacterAnimation.cs` mantém a máquina de estados e as prioridades de animação: dano, ressurreição, conjuração, ataque, caminhada e repouso.

## Efeitos visuais sincronizados

- Círculo necromântico sob os pés durante a ressurreição, com partículas esverdeadas ascendentes
- Esfera espectral durante a conjuração do ataque básico gratuito
- Arco violeta nas animações de ataque físico, sem mudar o dano, hitbox ou custo
- Brilho do corpo ao sofrer dano, com duração alinhada aos três quadros

A direção do personagem muda para o alvo ao atacar parado. A animação não mexe nos componentes de movimentação, colisão, inteligência artificial, salvamento ou condições de vitória.

## Performance, qualidade e limitações

A folha de origem é compacta, em estilo pixel art ilustrado, e os quadros de 96×96 são construídos durante a inicialização e reutilizados. Nenhuma rasterização acontece a cada quadro do jogo. O atlas ocupa aproximadamente 7 MB de textura RGBA32 em memória gráfica, além de sobrecarga de sprites; o processamento inicial pode variar conforme o Android. O próximo refinamento seria desenhar cada um dos 140 quadros individualmente, melhorando a fluidez e reduzindo os intermediários compostos. **Não se trata ainda de animação manual final de estúdio.**

## Instalação Android

Esta prévia usa `com.takeo.necromancer.unity.preview065` (nome *Necromancer Preview 0.6.5a*) e armazenamento próprio, sem sobrescrever as instalações anteriores. Não desinstale a versão 0.5 nem outras prévias que queira manter. Uma compilação de produção com assinatura permanente só será possível após cadastrar a chave privada conforme [ANDROID_SIGNING.md](../docs/ANDROID_SIGNING.md).

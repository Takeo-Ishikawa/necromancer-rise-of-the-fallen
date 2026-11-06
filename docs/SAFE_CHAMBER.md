# Câmara segura — ajuste de proporções, oclusão e colisões (v5)

Esta revisão corrige a diferença de escala entre a protagonista e os móveis e evita que ela atravesse visualmente os obstáculos. A câmara continua modular e todas as artes anteriores permanecem independentes.

## Alterações

- A protagonista aparece a **1,9×** do tamanho usado em combate **apenas dentro do refúgio**. Não altera velocidade, raio de colisão, atributos, animações ou progressão. Ao entrar na caverna, a escala original é restaurada.
- A ordenação por profundidade da protagonista considera o **ponto visual dos pés**, 0,52 unidade abaixo da posição lógica do ator. Os servos usam correção menor, de 0,30 unidade.
- Colisões agora bloqueiam a área ocupada **pelos móveis e rochas representadas**, não somente uma faixa fina no rodapé do desenho: armário alto, cama e altar têm volumes retangulares; fonte e ninho têm volumes elípticos. O tapete do altar permanece acessível pela frente.
- A posição, escala, colisor e ordenação de cada móvel continuam configuráveis em `unity/Assets/Resources/Data/sanctuary_props.json`. O campo opcional `blockShape` aceita `box` (padrão) ou `ellipse`.
- Fundo, entrada norte, servos, combate nas cavernas e salvamentos não foram modificados.

## Geometria e enquadramento

O mapa segue **16 × 24 unidades** com início em (8, 5.55). As zonas de bloqueio foram revisadas contra o PNG efetivamente desenhado e verificados, por simulação de ocupação, os caminhos entre o centro inicial e a saída em (8, 18.6).

A câmera permanece mais aberta no refúgio (ortographicSize de até 11.7); a escala adicional da protagonista compensa a distância sem mudar o zoom de combate.

## Conferência no aparelho

1. Em cada canto do mapa, aproximar-se da cama, fonte, guarda-roupa, altar e ninho por frente e pelas laterais: a protagonista deve parar antes das áreas sólidas e não atravessar os objetos.
2. Observar os pés e o ponto de contato com o chão. A personagem deve ficar à frente dos móveis quando caminha em primeiro plano e atrás somente quando fisicamente mais distante, fora da área bloqueada.
3. Confirmar que ainda é possível caminhar pelo centro até a saída norte.
4. Entrar na caverna e conferir que a protagonista volta à **escala original**.
5. Verificar servos e retorno ao refúgio.

**Validação:** a geometria e os caminhos foram checados por simulação, mas ainda é necessário compilar e verificar os sprites e movimento na Unity/Android.

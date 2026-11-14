# BONEHOLLOW — Bíblia visual v0.1 (Etapa 01)

**Status:** proposta técnica de direção de arte, pronta para avaliação; **não validada no APK**.  
**Referência técnica:** [auditoria 00](BONEHOLLOW_MAPA_01_AUDITORIA_00.md) e [planta](BONEHOLLOW_MAPA_01_PLANTA_AUDITORIA.svg).  
**Amostras vetoriais autorais:** [folha de materiais](BONEHOLLOW_MAPA_01_AMOSTRAS_VISUAIS.svg).

## 1. Direção visual

Fantasia sombria subterrânea: rocha fria cinza-violeta, desgaste mineral irregular, pequenos pontos luminosos azulados e verde-água, ossos marfim envelhecido. Não usar brilho uniforme ou névoa opaca em áreas de combate. A cor do cenário deve apoiar a leitura de silhuetas, não competir com os personagens.

**Perspectiva:** piso visto de cima, com laterais/volumes de props em três-quartos suave, compatível com os sprites 2D atuais. Não simular isometria rígida nem criar paredes que pareçam atravessáveis onde há colisão.

**Orientação de luz:** preenchimento ambiental frio vindo do alto/esquerda da composição; fontes locais quentes (lanternas) e frias (cristais, fungos) em manchas pequenas. Sombras de contato curtas, deslocadas discretamente para baixo/direita, sem descolar da base do objeto.

## 2. Paleta-base (sRGB / HEX)

| Uso | HEX | Aplicação |
|---|---|---|
| Escuridão de fundo | `#181720` | Cavidades e profundidade, nunca piso inteiro |
| Piso-base | `#302C3B` | Base mineral neutra |
| Piso elevado | `#494354` | Planos secundários |
| Rocha estrutural | `#676171` | Paredes e rochas |
| Aresta iluminada | `#92899B` | Acentos em contornos |
| Osso antigo | `#C7BDA7` | Ossadas e detritos |
| Osso iluminado | `#E4D8C0` | Pequenos realces |
| Fungo | `#6BAE99` | Núcleos bioluminescentes |
| Cristal | `#80C9D9` | Brilho principal |
| Cristal profundo | `#666FBD` | Sombra/refração |
| Marca ritual | `#B48BCE` | Selos e poço |
| Alerta de covil | `#B76D73` | Acento de perigo |
| Lanterna | `#D9AC70` | Luz pontual quente |

Ajustar tons somente depois de comparação com sprites reais no telefone. Evitar usar a paleta inteira em um único recorte.

## 3. Unidade, escala e pivôs

- Mundo: `levels.json` em unidades Unity, **56 × 34**; coordenada Y do JSON é invertida por `NecroLevels.At`.
- `NecroArt.Environment` cria sprites de texturas com **100 pixels por unidade** quando não importados como Sprite. Manter o contrato do código; sprites importados devem ser revisados para equivalência visual.
- Piso atual `entrada_da_caverna_bonehollow` é repetido por `SpriteDrawMode.Tiled`. Novas variações devem ser tileáveis e sem costura perceptível.
- Pivô para objetos apoiados no chão: `(0.5, 0.37)` é a referência atual para `rock_cluster` via fallback de textura; preferir contato real do sprite, sem generalizar o valor a personagens. Props com sombra independente: sombra ancorada no mesmo ponto de contato.
- Tamanho atual de props procedurais: cerca de **0,37–0,80 unidade** (poças multiplicadas por 1,7); rochas de bloqueio são renderizadas em diâmetro **2 × raio** do JSON. Preservar esses tamanhos até o QA visual.
- Camadas atuais: chão `-50`, manchas `-49`, marcos `-5`, props `-4`, rochas `-3`, selo `5`, inimigos `12` e protagonista/servos `15` no código consultado. Não alterar globalmente o sorting: avaliar ordenação por Y só com testes de oclusão.
- Resolução de origem é decisão por asset, não meta fixa: piso tileável em potências de dois quando útil; atlas e compressão ETC2 conforme `docs/PADRAO_OTIMIZACAO_ANDROID.md`. Validar nitidez em resolução de tela real antes de produzir arquivos maiores.

## 4. Gramática de materiais e contraste

**Piso:** placas rochosas de tamanho variado, fissuras discretas e desgaste sem quadrícula visível; contraste baixo no centro das passagens. Textura repetida deve ter ao menos duas variações futuras para quebrar repetição.

**Parede:** topo irregular mais claro, face lateral mais escura; a face não pode mascarar um corredor acessível. Qualquer massa de parede deve ser compatível com os círculos de colisão atuais ou vir acompanhada de revisão geométrica explícita.

**Rocha:** massa assimétrica com borda superior mais clara e sombra curta; manter base alinhada ao círculo correspondente. Não transformar pedra visualmente pequena em obstáculo invisível.

**Cristal:** núcleo ciano claro com facetas violeta, brilho localizado e halo muito fraco; preservar leitura do cristal coletável (não confundir com adereços de fundo).

**Fungo:** verde-azulado menos brilhante que cristais, agrupamentos de silhueta arredondada e halo contido.

**Ossadas:** marfim dessaturado, sombras acinzentadas; densidade maior nas bordas e nichos, não no centro do combate.

**Regra de contraste:** personagens e efeitos de ataque devem ser identificáveis em 1 segundo na tela do celular. Se o fundo e a silhueta se fundirem, reduzir contraste da decoração antes de aumentar a saturação do personagem.

## 5. Regiões: distribuição, não sete cenários desconectados

| Região | Ênfase | Controle de legibilidade |
|---|---|---|
| Entrada | Rocha fria, lanterna, caminho amplo | Primeira visão limpa da protagonista |
| Ossadas | Marfim, marcas de arrasto | Evitar pilhas sobre pontos de interação |
| Fungos | Verde-água e poças discretas | Não confundir brilho com loot |
| Poço | Roxo e reflexos suaves | Não esconder a borda navegável |
| Cristais | Ciano/violeta, facetas pontuais | Coletáveis devem se distinguir dos cenográficos |
| Covil | Rocha escura, rubor contido | Silhueta e investida do Rotboar sempre legíveis |
| Selo | Verde-frio ritual, geometria mais organizada | Interação reconhecível mesmo com servos |

## 6. Plano de integração controlada

1. Usar esta folha como **referência**, não como textura de runtime; os desenhos são esquemáticos, não sprites prontos.
2. Produzir primeiro **2 pisos, 1 parede, 2 rochas, 1 cristal, 1 fungo e 1 conjunto de ossadas** para o recorte da entrada, em arquivos separados com transparência adequada.
3. Montar os objetos como filhos do `StageInstance.root`, mantendo os caminhos de `Resources.Load` atuais como fallback. Não trocar `levels.json` nem `Blocked`.
4. Capturar entrada, combate com Grave Pup/rato, grupo de servos e retorno ao refúgio. Comparar a imagem no aparelho.
5. Registrar FPS, memória e tamanho do APK. Não aprovar a ETAPA 01 por uma imagem isolada: conferir a mesma luz, escala e contato de sombra em movimento.

## 7. Evidências pendentes para aceite

- [x] Paleta e regras de perspectiva, escala, pivôs, luz, sombras e legibilidade documentadas.
- [x] Folha **esquemática** de amostras autorais produzida.
- [ ] Capturas de personagens reais sobre os materiais (não disponíveis nesta sessão).
- [ ] Comparação em dispositivo Android e correções de nitidez/oclusão.
- [ ] Conjunto de referências fotográficas próprias/licenciadas selecionado.
- [ ] Teste de aceitação de gameplay e performance.

**Status da ETAPA 01:** em andamento. A direção de arte está especificada, mas as amostras não são artes finais nem estão integradas ao jogo.

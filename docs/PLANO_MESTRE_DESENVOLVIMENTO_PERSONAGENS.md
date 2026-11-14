# PLANO-MESTRE REUTILIZÁVEL — PRODUÇÃO DE PERSONAGENS E CRIATURAS
## Necromancer: Rise of the Fallen | arte 2D, animação, implementação e QA

**Versão:** 1.0  
**Criado em:** 13/11/2026  
**Aplicação:** qualquer novo protagonista, aparência alternativa, monstro, chefe, NPC, animal, servo reanimado ou atualização de personagem já existente.  
**Repositório:** Takeo-Ishikawa/necromancer-rise-of-the-fallen  
**Engine de referência:** Unity 2022.3.62f2 LTS  
**Documento complementar:** docs/MODELO_FICHA_PRODUCAO_PERSONAGEM.md

> ESTE DOCUMENTO É UM PROCESSO PERMANENTE, NÃO UMA TAREFA DE PERSONAGEM ESPECÍFICO. A cada nova encomenda, copiar o modelo de ficha e abrir uma produção independente em docs/personagens/<id>/FICHA_<id>.md (caminho a criar quando necessário). Usar os checkpoints de aprovação, manter registro de evidências e atualizar o andamento ao concluir cada sessão. Não inventar que uma arte está integrada, que um PNG está no GitHub ou que um APK compilou.

---

## A. Resultado esperado e princípios

**Entrada:** o usuário fornece uma imagem, referência visual, conceito escrito ou combinação desses materiais.  
**Saída concluída:** personagem visualmente fiel à proposta, com sprites reais e consistentes, atlas organizados, ciclos de animação fluidos, código integrado ao jogo, comportamento funcional e testes registrados. Se a produção exigir novas regras de gameplay ou novos controles, isso será documentado e aprovado separadamente.

**Compromissos inegociáveis:**

1. Preservar o conceito aprovado: silhueta, proporções, rosto, anatomia, roupa, acessórios, paleta, textura, marcas exclusivas, perspectiva e identidade. Não trocar detalhes arbitrariamente entre quadros.
2. **Cada quadro representa uma pose válida**, não o mesmo recorte deslocado, esticado ou apenas espelhado. Espelhar é aceitável como fallback declarado, jamais como substituto silencioso de 8 direções ilustradas quando estas foram solicitadas.
3. O personagem deve parecer **um ser vivo em movimento**: apoio do pé/pata, distribuição de peso, antecipação, impulso, impacto, recuperação, respiração, cauda, roupa, cabelo e movimento secundário.
4. O cenário mantém o ponto real de contato com o solo; a arte move-se dentro da sua caixa de animação, não altera o Transform do ator para corrigir um quadro mal posicionado.
5. **Descanso (rest/sleep) não é ocioso (idle)**; morte não é apenas tombamento por rotação; cadáver recuperável e servo reanimado são estados de gameplay distintos.
6. Usar PNG transparente de verdade, células regulares, pivôs e volumes constantes. Não empacotar fundo preto/branco pintado como se fosse transparência.
7. Não sobrescrever personagens, sprites, chaves Android, progressão nem saves existentes sem autorização. Preservar arte antiga e caminho de retorno.
8. A qualidade é aferida no jogo e em movimento, não só por uma folha de imagens bonita.
9. Não adicionar automaticamente magia, classes, estatísticas ou poderes a um personagem inicialmente sem magia. Seu comportamento depende da ficha aprovada.
10. Otimizar texturas e memória sem degradar o nível de detalhe validado pelo usuário.

**Definição de concluído:** arte final e integração confirmadas, regressões estruturais aprovadas, builds selecionados concluídos e teste visual/jogável registrado. Quando o teste em aparelho não for realizado, o resultado fica como **implementado, pendente de QA**, não como funcional confirmado.

---

## B. O que existe no projeto: referências reais, não contratos universais

O jogo hoje combina sistemas de animação diferentes:

| Referência existente | Contrato atualmente usado | Observação de compatibilidade |
| --- | --- | --- |
| Rato corrompido | rat_move: 4 colunas × 8 linhas, células 128px; ataque 8 × 1; morte 10 × 1 | Movimento em 8 direções; ataque/morte atuais são laterais. Código NecroRatAtlas e NecroCreatureView |
| Rotboar | carregador preparado para 4 × 8, células 192px, por ação e por facção | Há fallback; conferir se os PNGs de cada ação realmente estão no repositório antes de afirmar que estão ativos |
| Grave Pup | sequência própria de imagens para vivo, aliado e caído | Mecânica e sprites diferentes de rat/rotboar; não forçar o mesmo loader |
| Arlequina | idle 1024×512, walk 4×8 com células 256px, spectral para outras ações | A direção segue o contrato do seu próprio carregador |
| Necromante Sombria | walk6 6×8, total 48 quadros, dimensões 768×1024 | O importador trata proporção NPOT; o nome do campo é walkSixResource |
| Necromante Física | atlas combat 4×8 e idle próprio | Sem magia; ataques e locomação compartilham outro formato |
| Criaturas genéricas | NecroFrameLibrary pode gerar ciclos derivados de uma fonte | Adequado como fallback/protótipo, não substitui arte final pintada de múltiplos ângulos |

**ATENÇÃO SOBRE A ORDEM DAS DIREÇÕES:** os carregadores de personagens existentes **não são uniformes**. Rat/Rotboar usam S, SE, E, NE, N, NW, W, SW; alguns personagens selecionáveis usam S, SW, W, NW, N, NE, E, SE. Sempre ler o parser do tipo de personagem antes de exportar. A ficha deve registrar mapeamento de linhas e índice, não apenas escrever “8 direções”.

**Arquivos-chave que cada nova sessão deve examinar conforme o papel do personagem:**

- Criaturas: unity/Assets/Scripts/NecroCreatureView.cs, NecroFrameLibrary.cs, NecroCreatureArt.cs, NecroRatAtlas.cs, NecroRotboarAtlas.cs.
- Spawn, combate, morte e servos: NecroPrototype.cs, NecroGameplay.cs, NecroCorpseRitual.cs, NecroRotboar.cs, NecroRespawn.cs, NecroSaveData.cs.
- Protagonistas: NecroCharacterAnimation.cs, NecroCharacterCatalog.cs, NecroCharacterSelection.cs, NecroCharacterAbilities.cs, Resources/Data/characters.json.
- Interface/troca de personagens: NecroUI.cs e docs/MENU_PERSONAGENS_0_9_22.md.
- Regras de sprites: docs/RATO_CORROMPIDO_SPRITES.md, docs/ROTBOAR_8_DIRECOES.md, docs/SELECAO_PERSONAGENS.md.
- Otimização/build: docs/PADRAO_OTIMIZACAO_ANDROID.md, docs/COMPILACAO_ANDROID_WINDOWS.md.
- Verificações: tests/check_unity_project.py e ferramentas/tests específicos do personagem.

Este documento propõe um contrato para **novas produções**, mas **não autoriza uma refatoração geral** dos personagens antigos.

---

## C. Operação a cada novo personagem — sem voltar ao zero

1. Receber a referência fornecida; reconhecer automaticamente o papel provável (protagonista, inimigo, servo, chefe, NPC) e a ação pretendida. Se uma decisão de gameplay essencial faltar, registrar hipótese conservadora na ficha em vez de inventar habilidades.
2. Conferir o código e a arte atuais no GitHub, bem como commits recentes que possam ter alterado renderização, direção, estados, atlas ou menu.
3. Escolher ID técnico estável em snake_case, nome exibido, categoria e escopo de implementação. Se for uma skin, reaproveitar lógica existente. Se for espécie nova, verificar fábrica de atores, spawns, IA, cadáver, reanimação e salvamento.
4. Criar uma ficha individual pelo modelo docs/MODELO_FICHA_PRODUCAO_PERSONAGEM.md, com todos os campos necessários, arte de origem, riscos, etapas e data. Essa ficha é o único registro de progresso daquela produção.
5. Seguir **ETAPAS 00–14** abaixo. Ao passar um gate, registrar explicitamente “aprovado”, “pendente” ou “bloqueado”. Não anunciar a fase seguinte como concluída antes da verificação real.
6. Ao concluir sessão, atualizar somente a ficha individual; alterar este plano-mestre apenas quando o processo geral evoluir.

**Os arquivos gerados serão versionados separadamente da lógica do jogo.** Mesmo quando houver produção artística em uma ferramenta de imagens, ainda será preciso converter para PNGs, validar transparência, empacotar e integrar.

---

## D. Contrato artístico e técnico proposto

### D.1. Estados de animação

| Estado | Obrigatoriedade | Conteúdo e comportamento esperado |
| --- | --- | --- |
| idle — ocioso | Obrigatório | Respirar, piscar, peso corporal, cabelo/pelo/cauda; loop discreto e fluido |
| walk — caminhar | Obrigatório | Passadas reais, alternância de apoio, corpo compensando movimento e deslocamento coerente |
| attack — atacar | Obrigatório para combatentes | Preparação, aceleração, impacto, follow-through e recuperação; temporalidade compatível com hit |
| hurt — receber dano | Recomendado; obrigatório para maior expressividade | Reação breve, recuo e recuperação; não substituir mecanicamente invulnerabilidade |
| death — morrer | Obrigatório para atores mortais | Perda de equilíbrio, queda, acomodação no chão e último frame persistente quando há corpo |
| rest — descansar | Obrigatório se o conceito prevê descanso; recomendado para os demais | Sentar, deitar ou repousar; transição entrar/sair e loop contínuo de respiração |
| reanimate / rise — reerguer | Obrigatório para criaturas reanimáveis | Corpo responde ao ritual, levanta, assume pose de servo; integração com efeitos existentes |
| ally_idle / ally_walk / ally_attack | Obrigatório quando houver variante visual própria de servo | Mesmo personagem reconhecível, com diferenças visuais de corrupção/energia; não apenas um filtro de cor se foi solicitada arte própria |
| cast / special / skill | Somente se a habilidade existir e for aprovada | O ato do golpe ou ritual corresponde a uma mecânica real, nunca a botão sem efeito |
| interact / harvest / capture | Condicional | Animação específica quando exigida; caso contrário, conservar sistema atual |

**Diferença obrigatória entre idle e rest:** idle representa personagem em pé/pronto, podendo ter movimentos leves; rest representa repouso explícito (deitado, encolhido, sentado, dormindo ou repousando contra objeto). Se rest não for acionado por gameplay atual, preparar os sprites e a transição, deixando sua ativação como pendência claramente registrada; não criar mecânica de sono/recuperação sem pedido.

**Cadáver e revive:** no jogo, inimigos comuns podem manter um corpo recuperável por aproximadamente 14 s e aliados caídos têm regras próprias. A apresentação visual não pode encurtar, renovar ou duplicar esse tempo. O último frame de death precisa poder permanecer parado até coleta/reanimação/desaparecimento.

### D.2. Quantidades sugeridas de quadros por ciclo

Estes são **orçamentos de direção artística**, não regras impostas pelo carregador; ajustar por tamanho, complexidade e gameplay na ficha.

| Ciclo | Referência de quadros por direção | Requisitos |
| --- | --- | --- |
| idle | 4–6 | Poses discretas, timing de pausa, loop sem tranco |
| walk | 6–8 (mínimo artístico aceitável 4 com aprovação) | Apoio esquerdo/direito distinto; marcha repetível sem salto visual |
| attack | 6–10 | Antecipação, contato e recuperação legíveis |
| hurt | 2–4 | Reação com silhueta perceptível |
| death | 8–12 | Queda e conclusão em pose estável |
| rest_enter | 3–6 | Transição da postura ativa para descanso |
| rest_loop | 4–8 | Repouso/respiração suave, loop fechado |
| rest_exit | 3–6 | Retomada coerente da pose idle |
| reanimate | 6–10 | Passagem de cadáver a ativo/servo |
| special | Conforme mecânica | Definir quadro de evento e de cancelamento |

**Cobertura direcional padrão para jogos top-down/3⁄4:** 8 direções ilustradas para idle, walk e attack dos combatentes, salvo escolha distinta documentada. Para death/rest/reanimate, produzir as direções realmente visíveis/necessárias; vistas compartilhadas ou espelhadas precisam ser declaradas e testadas. Nunca atribuir 8 direções a um atlas com imagens laterais iguais.

**Metas de ciclo:** começar com 8–12 FPS de desenho para walk, 10–16 FPS para ataque, 3–6 FPS para idle/rest, ajustando à sensação da criatura; duração total, deslocamento por passo e janela de hit são os verdadeiros critérios. Quadros podem ter durações diferentes quando um estado precisa de hold. A movimentação deve acompanhar distância/speed quando possível; não fazer caminhada no lugar ao empurrar uma parede.

### D.3. Orientações, pivô e formatos

- Referencial visual geral: top-down/3⁄4 no mesmo nível de câmera do jogo.
- Especificar na ficha a sequência exata de direções **por atlas**. Para novas criaturas, preferência S, SE, E, NE, N, NW, W, SW; para personagem jogável integrado a carregador antigo, respeitar a ordem do parser existente ou criar adaptador.
- Todos os quadros de um ciclo usam células do mesmo tamanho e canvas com padding interno suficiente para não cortar orelhas, caudas, armas, poeira ou golpes.
- Pivô baseado no ponto de contato dos pés/patas com o chão (ou ancoragem definida para criatura voadora); não centralizar cada frame segundo o bounding box variável.
- Manter escala anatômica constante: cabeça, tronco, corpo, roupa, acessórios, cauda e proporção de membros não crescem/encolhem entre frames sem intenção animada.
- PNG RGBA com fundo realmente transparente; não há cenário, grade, texto, numeração, sombras retangulares ou rótulos dentro dos quadros finais.
- Manter efeito de sombra de contato como camada separada quando possível, para evitar sombra “pulando” junto com a animação.
- Recomendar células de 128px para criaturas pequenas, 192px para médias, 256px para chefes/protagonistas ilustrados, **apenas como ponto de partida**; resolução final decorre do tamanho do personagem em tela e de seu detalhamento.
- Exportar versões-fonte de alta qualidade e versão runtime comprimida via Unity; não degradar o arquivo mestre para reduzir APK.
- Testar importação NPOT, Read/Write quando necessário, filtro (bilinear vs point por estilo), alpha is transparency, mipmaps e TextureImporterType de acordo com Resources.Load<Texture2D>()/Sprite.Create() ou sprites pré-importados.
- Metadados obrigatórios: ID, estado, direções, frameCount, ordem de quadros, colunas, linhas, largura/altura da célula, FPS/duração, pivô normalizado, pixels por unidade, ponto de impacto, estado de origem e de destino, variante hostil/servo e caminho Resources.
- **Não existe spritesheet universal**: rat_move 4×8, rat_attack 8×1 e hero walk6 6×8 coexistem; o loader e o manifest são a fonte contratual.

### D.4. Organização de arquivos por personagem

Estrutura sugerida, adaptável ao sistema atual:

- tools/character_pipeline/<id>/ — script de montagem reprodutível e configuração, quando útil.
- art_source/characters/<id>/ — master art, estudos e frames-fonte, fora de Resources (ou em pasta de trabalho separada conforme o projeto permitir).
- unity/Assets/Resources/Creatures/<id>_*.png — se for criatura compatível com carregadores de criaturas.
- unity/Assets/Resources/Characters/<Id>/ — se for personagem selecionável; caminhos serão registrados em characters.json.
- docs/personagens/<id>/FICHA_<id>.md — progresso, links e checklist.
- docs/personagens/<id>/EVIDENCIAS.md — se houver muitos screenshots, medidas e correções.

**Arquivos sugeridos, sujeitos ao manifest:** <id>_idle.png, <id>_walk.png, <id>_attack.png, <id>_hurt.png, <id>_death.png, <id>_rest_enter.png, <id>_rest_loop.png, <id>_rest_exit.png, <id>_reanimate.png, <id>_ally_*.png, <id>_portrait.png. Não criar pastas vazias nem PNGs falsos quando um estado não for aplicável.

---

## E. Processo passo a passo (todas as novas sessões seguem esta ordem)

### ETAPA 00 — Recebimento e auditoria do personagem
**Estado inicial:** pendente.

- [ ] Analisar visualmente referência ou descrição do usuário. Se for imagem, descrever apenas características visíveis relevantes à modelagem; preservar singularidades.
- [ ] Identificar se é criação nova, substituição visual ou skin de personagem existente.
- [ ] Analisar arte e código atual no GitHub para descobrir o caminho de renderização, estado de gameplay e formatos exatos.
- [ ] Registrar classificação: protagonista, hostil, servo, chefe, NPC, mascote, etc.
- [ ] Definir ID, escala aproximada em tela e as ações efetivamente exigidas.
- [ ] Registrar o que é conhecido, o que é hipótese de design e o que precisa de decisão pontual.
- [ ] Criar ficha individual a partir do modelo; não alterar personagem antigo ainda.

**Entregável:** ficha preenchida, estado atual documentado e commit-base.  
**Critério de aceite:** personagem identificado, riscos de substituição e dependências mapeados.

### ETAPA 01 — Conceito artístico (imagem-mestre)
- [ ] Produzir ilustração-mestre consistente com a referência, sem misturar fundo que depois será difícil remover.
- [ ] Conferir anatomia, roupa, cor, rosto, marcas, acessórios, dedos/patas, cauda, cabelo e volumes.
- [ ] Garantir que o desenho combina com Rotboar/rato/protagonistas sem copiar literalmente todo personagem.
- [ ] Criar versão de escala in-game e comparação com protagonista e criaturas vizinhas.
- [ ] Mostrar prévia da arte-mestre e anotar o que foi aprovado.

**Entregável:** arte-mestre com ID e versão, prévia proporcional.  
**GATE A — identidade visual:** somente após aprovação avançar para animações de produção. Não modificar a identidade ao criar sprites.

### ETAPA 02 — Turnaround direcional
- [ ] Criar oito poses base com o mesmo personagem visto de S, SE, E, NE, N, NW, W, SW (ou direção do loader de destino).
- [ ] Comparar contornos, altura aparente, proporções, cores e posição de membros.
- [ ] Verificar coerência de rosto/olhos, orelhas, cabelos, mãos, patas, feridas, cauda e trajes ao virar.
- [ ] Verificar o que aparece/desaparece naturalmente por perspectiva.
- [ ] Conferir se nenhuma vista corresponde apenas a um espelho incorreto com marcas assimétricas trocadas.
- [ ] Congelar pivô, escala, distância base dos pés e área de sombra.

**Entregável:** folha de turnaround e oito bases limpas.  
**GATE B — consistência espacial:** lado, costas, frente e diagonais correspondem ao mesmo corpo.

### ETAPA 03 — Rig de referência, poses e guia de timing
- [ ] Definir articulações, massa, centro de gravidade, número de patas, equipamento e amplitude de movimento.
- [ ] Criar storyboard de silhuetas para os estados idle, walk, attack, hurt, death, rest e reanimate conforme aplicabilidade.
- [ ] Separar primeiro contato, preparação, impacto e recuperação para ataque.
- [ ] Definir quais partes permanecem ancoradas durante descanso, queda e revive.
- [ ] Registrar loop, FPS/durações, direção, número de quadros e marcadores de evento na ficha.
- [ ] Decidir entre desenho quadro a quadro, deformação/rig 2D, ou técnica híbrida, sem sacrificar fluidez nem depender de transformações que distorçam anatomia.

**Entregável:** storyboard, tabela temporal e manifest preliminar.

### ETAPA 04 — Idle e descanso
- [ ] Desenhar idle com respiração real, postura pronta, movimento secundário discreto e loop suave.
- [ ] Desenhar rest_enter, rest_loop e rest_exit quando previsto, diferenciando nitidamente repouso de idle.
- [ ] Preservar pontos de contato no chão e não deslocar a cabeça para cima/baixo de forma artificial.
- [ ] Testar transição idle→walk, walk→idle e idle→rest→idle.
- [ ] Se o jogo não tem comando ou IA de descanso para aquele ator, registrar implementação visual do estado e pendência de acionamento; não ativar cura automática ou funcionalidade não aprovada.

**Entregável:** atlases e prévias animadas de idle e descanso.  
**Critério de aceite:** loop natural sem piscar/fazer teleport nos membros ou mudar o volume do personagem.

### ETAPA 05 — Caminhada em 8 direções
- [ ] Produzir 4–8+ poses distintas por direção conforme aprovado, preferencialmente 6–8 quando a fluidez justificar.
- [ ] Desenhar contatos, apoio, passagem, impulsão e recuperação; alternar passos ou patas corretamente.
- [ ] Adicionar resposta do tronco, mãos, orelhas, cauda e roupa, com atraso natural.
- [ ] Conferir continuidade do primeiro/último frame, simetria do ciclo e mudança de direção.
- [ ] Testar parada durante animação e caminhada contra obstáculos. Não deixar o ator deslizar parado, rodar sem deslocamento nem mudar de tamanho.
- [ ] Fazer teste de reprodução em loop nas oito direções, em escala real de tela.
- [ ] Se couber, sincronizar índice de walk com distância real percorrida, evitando animações aceleradas pela taxa de FPS.

**Entregável:** walk completo, atlas validado e prévia GIF/vídeo.  
**GATE C — locomação:** não avançar para produção massiva de outras animações enquanto a caminhada não parecer fluida.

### ETAPA 06 — Ataques e eventos de combate
- [ ] Produzir ataque coerente com anatomia, direção e arma/patas/mordida/soco/chute reais da criatura.
- [ ] Separar antecipação, avanço, contato, seguimento e recuperação; permitir ação distinta quando houver múltiplos golpes previstos.
- [ ] Registrar o momento do impacto (hit frame/time) para sincronizar com a lógica real, sem duplicar dano.
- [ ] Evitar que ataque visual fique em loop enquanto gameplay executa só um golpe.
- [ ] Confirmar que sprite não atravessa paredes nem apaga o contato com o chão.
- [ ] Testar troca de alvo à esquerda/direita e diagonais; arte lateral espelhada só com fallback explicitado.
- [ ] Preservar cooldown, hitbox, alcance e priorização de alvo, salvo mudança de gameplay aprovada.
- [ ] Definir cancelamentos e precedência (morte > hit/ataque quando a saúde zera; ataque > walk; idle/rest apenas fora de ações).

**Entregável:** ataque com timing de impacto documentado e prévia.  
**GATE D — combate:** golpe inteligível em movimento e sincronizado com a mecânica.

### ETAPA 07 — Hurt, morte e estado de cadáver
- [ ] Preparar hit/hurt breve com resposta visual sem alterar posição real da unidade incorretamente.
- [ ] Desenhar sequência de morte com inércia, queda e acomodação críveis; último quadro permanece estático se o corpo deve existir.
- [ ] Respeitar rotação, profundidade e direção do corpo sem atravessar outras camadas.
- [ ] Se o ator deixa cadáver recuperável, manter estado e janela atual do jogo e checar salvar/restaurar cadáver.
- [ ] Se o ator desaparece sem corpo, definir fade/dissolve separado, sem usá-lo no lugar de morte completa quando foi pedida.
- [ ] Não deixar cadáver “respirar” como se estivesse vivo, a menos que o estado seja propositalmente atordoado e isso esteja definido.
- [ ] Garantir que mortos não sejam alvos válidos de novo ataque e não executem loops de caminhada.

**Entregável:** hurt/death/corpse (conforme aplicável), transição validada.

### ETAPA 08 — Reanimação, aliado e variantes visuais
- [ ] Para criaturas reanimáveis, criar sequência corpse→rise→ally_idle; não trocar instantaneamente por um sprite sem evento.
- [ ] Criar variante de servo reconhecível visualmente se prevista: sinais de energia necromântica, coloração, marcas, pupilas e/ou detalhes próprios.
- [ ] Preservar anatomia e silhueta da mesma espécie ao mudar de facção; não mudar o tamanho de hitbox visualmente sem justificativa.
- [ ] Cobrir idle, walk e attack da variante aliada quando solicitada.
- [ ] Integrar aos sistemas existentes de necromancia, inventário de servos, capacidade, captura e efeitos, sem reimplementar regras.
- [ ] Confirmar que morte de servo tem comportamento distinto da morte de inimigo quando o jogo assim define.

**Entregável:** reanimation e atlas aliado; fluxos de combate/servo coerentes.

### ETAPA 09 — Preparação dos sprites e empacotamento reprodutível
- [ ] Guardar quadros-fonte separados e ordenados com nomes determinísticos, por exemplo <id>__walk__S__000.png.
- [ ] Cortar/corrigir excesso de margem apenas com regra de pivô comum; não centralizar cada pose independentemente.
- [ ] Garantir dimensão exata de todas as células, canal alpha e recorte sem vazamento para a célula vizinha.
- [ ] Compor atlas conforme o manifest, não conforme uma grade decorativa gerada pela ferramenta de imagem.
- [ ] Criar script de empacotamento reproduzível quando houver muitos quadros. Documentar dependências e execução.
- [ ] Validar alpha verdadeiro, tamanho, ordem, número de células, limites do desenho, alinhamento dos pés/patas e ocorrência de quadros vazios/duplicados.
- [ ] Validar consistência temporal visualmente: evitar borrões, faces distorcidas, caudas que surgem/desaparecem e membros duplicados.
- [ ] Gerar prévia GIF/vídeo a partir dos mesmos PNGs finais. Nunca dizer “32 sprites prontos para Unity” se ainda só existe uma imagem conceitual montada.
- [ ] Preservar versão anterior em branch, commit ou backup recuperável antes de substituir a ativa.

**Entregável:** spritesheets PNG RGBA finais, script/fonte, manifest, prévias e relatório automático de validação.  
**GATE E — arquivo técnico:** não integrar se os PNGs não passaram na inspeção dimensional e visual.

### ETAPA 10 — Importação no Unity e controle de memória
- [ ] Definir recursos a carregar via Resources.Load<Texture2D>() ou Sprite importado e configurar o importador correspondente.
- [ ] Para atlas NPOT, verificar se a Unity altera tamanho/proporção no import Android/Windows e aplicar importador específico se necessário.
- [ ] Definir pivôs, PPU, filtro, wrap, mipmaps e textura de alpha. Cuidado com transparência preta produzindo halos quando comprimida.
- [ ] Utilizar compressão Android adequada, começando por ETC2 RGBA para sprites com alpha quando compatível com a arte e o aparelho.
- [ ] Evitar texturas runtime geradas por inimigo individual; cortar Sprite uma vez, guardar em cache e compartilhar entre instâncias.
- [ ] Evitar Unity Resources com variantes de PNG inutilizadas que aumentam o APK.
- [ ] Medir tamanho real de texturas, quantidade de atlas, custo de memória e impacto no APK.
- [ ] Evitar alterar importação de outros personagens para “consertar” a textura nova.

**Entregável:** recursos no caminho correto e carregador com validação e fallback quando tecnicamente possível.

### ETAPA 11 — Implementação do personagem no jogo
- [ ] Decidir caminho: **skin selecionável** (catálogo e animação), **nova espécie** (fábrica, IA, spawn, stats, sprites, morte/servos) ou **substituição artística** (mesma entidade e regras).
- [ ] Criar/adaptar loader da animação, compatibilizar direções, pivô, escala, flip e prioridades de estados.
- [ ] Integrar idle, walk, attack, hurt, death, rest (quando acionável), corpse, reanimate e ally sem quebrar callbacks.
- [ ] Garantir que o ataque parte do evento real de gameplay, e não apenas do movimento do SpriteRenderer.
- [ ] Para protagonista, cadastrar ID único e caminhos corretos em Resources/Data/characters.json, deixar disponível apenas quando todos os recursos exigidos existirem e preservar a troca em jogo.
- [ ] Para inimigo novo, configurar espécie no sistema de criação e spawn com parâmetros de vida/dano/velocidade/alvos aprovados ou conservadores; respeitar território, colisão, respawn, drop/corpo.
- [ ] Para criatura reanimável, registrar no fluxo de corpse e recuperação/arsenal de servos; manter persistência sem invalidar saves anteriores.
- [ ] Fixar sombra no chão, animação visual em filho do ator e sorting coerente, sem alterar root de movimentação para corrigir a ilustração.
- [ ] Proteger o fallback e os assets antigos até o novo personagem ser validado.
- [ ] Documentar arquivos modificados, motivo, escopo e rollback.

**Entregável:** personagem encontrado e executando suas animações no fluxo real do jogo.  
**GATE F — integração:** o ator entra no jogo, reage a eventos e não gera erro de referência, dimensão ou estado.

### ETAPA 12 — QA automatizado e prévia de animação
- [ ] Executar validação de estrutura, imagens, JSON e scripts relevantes.
- [ ] Conferir que não existem células invertidas (origem superior da folha vs coordenadas inferiores do Unity).
- [ ] Conferir oito direções conforme tabela do loader, especialmente diagonal, costas e espelhamento.
- [ ] Testar sequência idle→walk→idle→attack→idle, hurt→death→corpse e corpse→reanimate→ally quando aplicável.
- [ ] Verificar interrupção de ações, ausência de duplicações de dano e sincronização do hit frame.
- [ ] Verificar que a introdução da espécie não altera desempenho/AI dos demais personagens.
- [ ] Exibir gif/vídeo com loops curtos a partir dos sprites finais, preferencialmente em fundo neutro e cenário do jogo.
- [ ] Registrar problemas visuais e **corrigir atlas antes de compensar com distorções de código**.

**Entregável:** relatório de testes e galeria de prévias finais.

### ETAPA 13 — Build selecionado e teste jogável
- [ ] Confirmar com o contexto do pedido se build Android ou Windows é necessário; por padrão, priorizar Android preview para teste móvel, **mas não compilar sem etapa pronta**.
- [ ] Usar .github/workflows/unity-android.yml **manualmente** para Android; unity-windows.yml somente se houver pedido/teste Windows.
- [ ] Não modificar assinatura estável, package ID ou versionCode da preview além do fluxo já adotado.
- [ ] Checar o resultado real do workflow, logs, existência do artefato e tamanho do APK, sem confundir build iniciado com build bem-sucedido.
- [ ] Testar personagem no aparelho/computador: 8 direções, idle, descanso, ataque, dano recebido, morte, cadáver, reanimação, troca de personagem, salvamento e retorno ao jogo conforme aplicabilidade.
- [ ] Verificar fluidez, proporção, pivôs, transparência, oclusão, FPS, consumo de memória e tamanho do aplicativo com múltiplos servos presentes.
- [ ] Repetir o teste após correções significativas. Não declarar aprovação apenas pelo script Python ou pela compilação Unity.

**Entregável:** execução de CI identificada, artefato quando gerado, QA em jogo e pendências conhecidas.

### ETAPA 14 — Entrega, documentação e liberação
- [ ] Registrar commits, caminhos, ID e nomes de todas as animações realmente adicionadas ao repositório.
- [ ] Atualizar ficha individual com estados [x]/[~]/[!], evidências e próximos trabalhos.
- [ ] Registrar limitações artísticas conhecidas, inclusive ações com vistas compartilhadas ou espelhadas.
- [ ] Documentar funcionamento no jogo e procedimentos de rollback.
- [ ] Se comprovado, marcar “implementado e testado” com plataforma e versão. Caso contrário, “implementado; testes pendentes”.
- [ ] Preservar o material original e os sprites antigos para recuperação.
- [ ] Não alterar o plano-mestre a cada criatura, salvo aperfeiçoamento global justificado.

**GATE FINAL — entrega:** personalidade visual preservada, animações fluidas e identificáveis, integração comprovada, save e gameplay sem regressão, tamanho/desempenho aceitáveis e status transparente.

---

## F. Critérios de qualidade visual (checklist transversal)

- [ ] **Identidade consistente:** mesma cabeça, olhos, roupa, manchas, membros, acessórios e silhueta entre frames/direções.
- [ ] **Anatomia:** articulações plausíveis, sem patas sobrando, braços duplicados ou variação inexplicada de comprimento.
- [ ] **Perspectiva:** frente, costas, perfil e diagonais realmente distintas; linhas de base compatíveis com câmera.
- [ ] **Ciclo:** início/fim sem tranco, contato com solo coerente e balanço sem deslizamento.
- [ ] **Descanso:** claramente diferente de idle; transições coerentes.
- [ ] **Combate:** golpe segue a trajetória certa e o frame de impacto corresponde à ação real.
- [ ] **Morte:** conclusão crível, pose caída persistente e sem animações indevidas depois da morte.
- [ ] **Reanimação:** cadáver e servo reconhecíveis como o mesmo indivíduo/espécie.
- [ ] **Transparência:** sem fundo pintado, halos, quadrículas, pixels soltos ou recorte de extremidades.
- [ ] **Integridade:** dimensões/células/pivô validados; nomes e ordem do atlas correspondem ao código.
- [ ] **Leitura em tela:** legível em celular, com fundo real do jogo e diversos servos próximos.
- [ ] **Estabilidade:** sem mudanças indesejadas de hitbox, posicionamento, progresso ou personagem selecionado.

---

## G. Gate de aprovação e política de retrabalho

| Gate | Marco | Prova requerida | Se falhar |
| --- | --- | --- | --- |
| A | Conceito | Ilustração fiel e aprovada | Revisar design, não desenhar centenas de quadros |
| B | Turnaround | Oito ângulos consistentes | Corrigir modelos-base |
| C | Movimento | Loop fluido in-game size em 8 direções | Refazer poses/timing e só depois prosseguir |
| D | Ataque | Golpe completo e hit correto | Corrigir timing, desenho e evento |
| E | Arquivos | PNG alpha, grade, células, pivô e import verificados | Corrigir exportação/empacotamento |
| F | Integração | Ator vivo usa clips reais, cadáver/servo funcionam | Corrigir loader e transições |
| Final | QA | Build e execução documentados no ambiente escolhido | Manter status pendente, corrigir e retestar |

**Política:** não declarar uma etapa “feita” se só existe conceito. Se o usuário aprova uma pose/estilo, congelar essa versão para não rediscutir a mesma decisão a cada sessão. Modificações novas seguem controle de mudanças.

---

## H. Procedimento de continuidade em outras sessões

A cada novo personagem:

- Criar ou recuperar docs/personagens/<id>/FICHA_<id>.md.
- Ler último commit e a seção “ETAPA ATIVA” da ficha individual.
- Verificar se a referência original está acessível; se não estiver, pesquisar arquivos disponíveis e trabalhar apenas com informações comprovadas. Não fingir fidelidade a uma imagem inacessível.
- Reutilizar o material já aprovado, evitando gerar novamente sprites finais sem necessidade.
- Executar o primeiro item pendente, registrar saída real e evidências.
- Ao fim de cada sessão, atualizar a ficha no GitHub e informar quais estados estão prontos, quais estão integrados, quais foram testados e o próximo passo.

**Mensagem-modelo para nova sessão:**

“Vamos criar ou continuar o personagem [NOME / ID] no Necromancer: Rise of the Fallen. Leia docs/PLANO_MESTRE_DESENVOLVIMENTO_PERSONAGENS.md, localize ou crie docs/personagens/[ID]/FICHA_[ID].md a partir de docs/MODELO_FICHA_PRODUCAO_PERSONAGEM.md e use a imagem/conceito anexado como referência. Continue da primeira etapa pendente, com prioridade para fidelidade artística e movimentação fluida. Preserve os personagens existentes, seus scripts, salvamentos e builds separados. Mostre a prévia dos marcos de aprovação e atualize a ficha após cada entrega. Só considere integrado o que foi gravado e verificado no GitHub; só considere testado o que tiver resultados reais.”

**Quando o pedido for curto (“continue”, “faça o ataque”, “coloque no jogo”)**: identificar o personagem ativo pela conversa/ficha, executar a etapa correspondente sem reiniciar o processo nem pedir novamente dados já fornecidos. Preservar os demais requisitos definidos na ficha.

---

## I. Termos de estado e diário de produção

Para cada ação/clip na ficha, classificar o status separadamente:

- NÃO INICIADO — nenhum recurso foi produzido.
- CONCEITO — imagem de referência/arte-mestre apenas.
- QUADROS PRONTOS — poses individuais aprovadas, ainda não empacotadas.
- ATLAS VALIDADO — PNG final, grade, pivô, transparência e prévia aprovados.
- INTEGRADO — código e assets presentes e verificados no GitHub.
- CI APROVADO — build da plataforma concluído com resultado real e artefato.
- TESTADO EM JOGO — funcionamento validado durante a partida no ambiente especificado.
- BLOQUEADO — razão documentada, sem fingir conclusão.

**Registro mínimo no fim da sessão:** data, personagem, etapa, arte de referência utilizada, arquivos/commits, estados produzidos, testes (com links), riscos, pendências e primeira tarefa exata para a próxima sessão.

Não atribuir CI APROVADO se a execução apenas foi iniciada, nem TESTADO EM JOGO porque um GIF reproduziu corretamente.

---

## J. Referências internas

- docs/MODELO_FICHA_PRODUCAO_PERSONAGEM.md — ficha reutilizável por personagem.
- docs/RATO_CORROMPIDO_SPRITES.md — exemplo de atlas e diferença entre direção de marcha e ataque lateral.
- docs/ROTBOAR_8_DIRECOES.md — variações hostil/aliado, morte e reanimação.
- docs/SELECAO_PERSONAGENS.md e docs/MENU_PERSONAGENS_0_9_22.md — catálogo e troca de protagonistas.
- docs/PADRAO_OTIMIZACAO_ANDROID.md — arte-mestre e compactação.
- docs/COMPILACAO_ANDROID_WINDOWS.md — workflows independentes.
- docs/PLANO_DESENVOLVIMENTO_BONEHOLLOW_MAPA_01.md — coerência de escala com o cenário inicial.
- unity/Assets/Scripts/NecroCreatureView.cs e NecroCharacterAnimation.cs — fluxos existentes, a serem consultados antes de adaptar loaders.

**Nota final:** a direção artística é comum; os contratos exatos de código serão definidos por tipo de ator. A ficha torna essas escolhas verificáveis e reutilizáveis sem sacrificar a identidade de cada personagem.

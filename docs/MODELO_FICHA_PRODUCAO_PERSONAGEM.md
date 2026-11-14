# MODELO DE FICHA — [NOME DO PERSONAGEM]
## Registro individual de produção, arte e integração Unity

> **USO:** copiar este arquivo para docs/personagens/<id>/FICHA_<id>.md, substituindo os marcadores [..]. NÃO editar o modelo original para acompanhar um personagem; o plano-mestre é docs/PLANO_MESTRE_DESENVOLVIMENTO_PERSONAGENS.md. Criar a pasta apenas quando houver um personagem real.
>
> **IMPORTANTE:** nenhum campo “a definir” significa aprovado. Marque apenas ações comprovadas por arquivos, imagens, testes ou commits.

### 1. Identificação

| Campo | Valor |
| --- | --- |
| Nome exibido | [NOME] |
| ID técnico estável | [id_em_snake_case] |
| Papel | [protagonista / skin / inimigo / servo / chefe / NPC / outro] |
| Espécie / classe | [DESCRIÇÃO] |
| Variante(s) | [hostil / aliada / viva / corrompida / outras] |
| Gameplay existente a reutilizar | [SISTEMA / ARQUIVOS] |
| Habilidades autorizadas | [HABILIDADES; “nenhuma” se inicialmente sem magia] |
| Referência original | [link GitHub, imagem anexa acessível, descrição ou ID de arquivo] |
| Data de início | [AAAA-MM-DD] |
| Responsável pela aprovação da arte | [USUÁRIO] |
| Commit-base | [SHA] |
| Branch de trabalho | [branch ou main se explicitamente usado] |
| Status geral | [NÃO INICIADO / CONCEITO / QUADROS PRONTOS / ATLAS VALIDADO / INTEGRADO / CI APROVADO / TESTADO EM JOGO / BLOQUEADO] |
| ETAPA ATIVA | [00–14] |
| Primeira tarefa pendente | [descrever ação executável] |

### 2. Descrição da imagem-mestre

**Resumo fiel da referência:** [DESCRIÇÃO]

**Características invariáveis:** [cabeça, olhos, cabelo, cor, pelagem, pele, corpo, proporções, roupa, marcas, acessórios, garras/pés, cauda, expressão, silhueta].

**Características que podem variar durante a animação:** [movimento secundário, deformação de impacto, respiração, roupa, cabelos, etc.].

**Proibições específicas:** [não usar magia, sem armas, sem chapéu, não trocar traje, sem sapatos, etc.].

**Tamanho esperado em jogo:** [unidades de altura/largura, comparado ao protagonista e monstros].

**Perspectiva:** [top-down 3/4 ou outra explicitamente aprovada].

**Design aprovado?** [ ] Sim [ ] Não [ ] Aguardando comparação visual.

**Link e versão da arte-mestre:** [CAMINHO].

### 3. Contrato de direção, células e renderização

| Parâmetro | Definição |
| --- | --- |
| Loader escolhido | [NecroCreatureView / adapter próprio / NecroCharacterAnimation / outro] |
| Ordem de direções no atlas | [S, SE, E, NE, N, NW, W, SW ou ordem do loader real] |
| Número de direções exigidas | [8 padrão / justificativa para menos] |
| Nome / pasta Resources | [CAMINHO SEM EXTENSÃO] |
| Resolução da célula (px) | [128 / 192 / 256 / personalizado] |
| Dimensões da folha | [COLUNAS × LINHAS × TAMANHO_CÉLULA] |
| Pivô dos pés/patas normalizado | [x,y] |
| Pixels per unit | [PPU] |
| Visual sorting / sombra | [ESPECIFICAÇÃO] |
| Corpo ou hitbox gameplay | [PRESERVAR / ESPECIFICAR] |
| FPS/duração por estado | [TABELA ABAIXO] |
| Importador Android | [Default + Texture2D ou Sprite; ETC2; alpha; NPOT] |
| Mapeamento de linha → direção | [ESPECIFICAR TODAS AS LINHAS] |

### 4. Matriz de animação (preencher por ação real)

**Regra:** distinguir o que foi apenas ilustrado do que está em PNG válido, integrado e testado.

| Estado | Direções | Quadros por direção / total | FPS/duração | Hit ou evento | Arquivo Resources | Status / evidência |
| --- | --- | --- | --- | --- | --- | --- |
| idle | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| walk | [ ] | [ ] | [ ] | passo [se aplicável] | [ ] | NÃO INICIADO |
| attack | [ ] | [ ] | [ ] | impacto no quadro [ ] | [ ] | NÃO INICIADO |
| hurt | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| death | [ ] | [ ] | [ ] | cadáver no último quadro | [ ] | NÃO INICIADO |
| rest_enter | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| rest_loop | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| rest_exit | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| reanimate | [ ] | [ ] | [ ] | servo ativo no final | [ ] | NÃO INICIADO |
| ally_idle | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| ally_walk | [ ] | [ ] | [ ] | — | [ ] | NÃO INICIADO |
| ally_attack | [ ] | [ ] | [ ] | impacto no quadro [ ] | [ ] | NÃO INICIADO |
| special/skill | [se aplicável] | [ ] | [ ] | [ ] | [ ] | NÃO INICIADO |
| portrait | [1 vista] | 1 | estático | — | [ ] | NÃO INICIADO |

**Notas de cobertura:** [Direções ilustradas separadamente ou reutilização explícita, espelhos, variações, justificativas].  
**Ações não aplicáveis:** [por que não serão produzidas].  
**Descanso acionável no jogo?** [Sim: gatilho/evento / Não: preparar clips; ligação pendente].

### 5. Etapas de produção — checklist de execução

- [ ] **00 — Recepção e auditoria:** verificar código/foto/referência, definir ID e manter arquivos atuais.
- [ ] **01 — Conceito visual:** imagem-mestre aprovada e escala comparada em gameplay.
- [ ] **02 — Turnaround:** vistas e diagonais coerentes.
- [ ] **03 — Storyboard/rig:** articulações, ciclos, eventos e pivôs definidos.
- [ ] **04 — Ocioso e descanso:** idle + rest conforme matriz, transições verificadas.
- [ ] **05 — Caminhada:** quadros distintos, 8 direções, loop fluido aprovado.
- [ ] **06 — Ataque:** preparação, impacto e recuperação; hit sincronizado.
- [ ] **07 — Dano e morte:** hurt/death/corpse sem falhas.
- [ ] **08 — Reanimação/aliado:** quando aplicável, servo visualmente coerente.
- [ ] **09 — Empacotamento:** spritesheets RGBA válidas, scripts de conversão e prévias.
- [ ] **10 — Importação:** loader, pivot, escala, alpha e compressão verificados.
- [ ] **11 — Integração:** novo ator/skin realmente disponível na partida.
- [ ] **12 — QA automatizado:** tests e inspeção de clips/transições.
- [ ] **13 — Build e QA jogável:** plataforma escolhida, logs e teste real.
- [ ] **14 — Documentação e entrega:** evidências, commits, rollback e status.

**Gate A — conceito:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate B — vistas:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate C — caminhada:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate D — ataque:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate E — arquivos Unity:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate F — integração:** [ ] Aprovado [ ] Pendente [ ] Reprovado.  
**Gate final — jogável:** [ ] Aprovado [ ] Pendente [ ] Reprovado.

### 6. Plano funcional e limites de gameplay

**Comportamentos que já existem e devem ser preservados:** [MOVIMENTO, ATAQUE, REANIMAÇÃO, INVENTÁRIO, TROCA, AI, RESPAWN, SONS].

**Ações ou IA novas expressamente solicitadas:** [DESCRIÇÃO OU NENHUMA].

**Regras de morte e corpo:** [Cadáver recuperável / dissipação / chefe / servo / outra].

**Física e colisão:** [dimensões existentes, mudanças autorizadas, teste].

**Interações com o jogador:** [ataque manual, captura, coleta, guarda/solta, seleção].

**Persistência/salvamento:** [chave da seleção, IDs, catálogo de inimigos, risco de compatibilidade].

**Compatibilidade obrigatória:** não modificar package ID, assinatura, saves antigos ou personagens existentes sem autorização.

### 7. Checklist técnico de qualidade dos PNGs

- [ ] As imagens finais são PNG com alpha real, sem fundo preto/branco.
- [ ] Todas as células têm dimensões uniformes e borda de segurança.
- [ ] O número de frames confere com o manifest.
- [ ] A ordem das direções confere com o parser Unity.
- [ ] O pivô dos pés/patas é constante; não centralizar pela silhueta variável.
- [ ] A anatomia/roupa/cauda é idêntica em todas as vistas.
- [ ] O primeiro e o último frame de loops se encontram de modo natural.
- [ ] Há caminhada real (passos diferentes), sem duplicação de membros.
- [ ] Não existem quadros indevidamente vazios, cortados ou copiados.
- [ ] A morte termina em quadro estável; o corpo permanece se a mecânica pede.
- [ ] O idle e o descanso são visualmente diferentes.
- [ ] As texturas NPOT, alpha e a compressão foram checadas no importador.
- [ ] A arte antiga/fonte está preservada de forma recuperável.
- [ ] Uma prévia GIF/vídeo foi gerada dos mesmos arquivos finais.

### 8. Integração Unity — mapa de arquivos e testes

**Recursos novos:** [CAMINHO, TAMANHO, SPRITESHEETS].

**Código modificado:** [ARQUIVO, COMMIT, OBJETIVO].

**Loader / cache:** [CLASSE E API].

**Estados/triggers utilizados:** [EVENTOS REAIS E PRECEDÊNCIA].

**Spawns/catálogo:** [ID, REGIÃO, SELEÇÃO, CAMINHOS RESOURCES].

**Corpse/reanimation/faction:** [COMPORTAMENTO E TESTES].

**Compatibilidade/rollback:** [COMMIT-BASE + ARQUIVOS ANTIGOS].

**Testes automatizados executados:**

| Teste | Execução / link | Resultado | Observação |
| --- | --- | --- | --- |
| Verificação estrutural | [ ] | Não executado | [ ] |
| Verificação das spritesheets | [ ] | Não executado | [ ] |
| Importação Unity | [ ] | Não executado | [ ] |
| Build Android preview | [ ] | Não executado | [ ] |
| Build Windows (se solicitado) | [ ] | Não executado | [ ] |

**Testes em jogo (marcar apenas quando realizados):**

- [ ] Personagem renderiza na escala correta.
- [ ] Parado: idle funciona.
- [ ] Repouso: enter/loop/exit funciona ou está conscientemente não acionável.
- [ ] Caminhada: oito direções sem distorção nem deslizar.
- [ ] Ataque: animação e evento de dano sincronizados.
- [ ] Hurt e morte: transição correta; nenhuma ação indevida após morrer.
- [ ] Cadáver: permanece pelo tempo existente, pode ser colhido/capturado.
- [ ] Reanimação: criatura ergue e segue como servo, se aplicável.
- [ ] Personagem selecionável: trocar sem perder nível, equipamentos ou servos, se aplicável.
- [ ] Save/restart: estado do mundo preservado.
- [ ] APK atualiza versão anterior sem desinstalar (mesma assinatura/ID).
- [ ] FPS, memória e APK sem regressão relevante.

### 9. Registro de artefatos e aprovação

| Artefato | Caminho/URL verificável | Versão | Estado |
| --- | --- | --- | --- |
| Referência original | [ ] | [ ] | [ ] |
| Arte-mestre | [ ] | [ ] | [ ] |
| Turnaround | [ ] | [ ] | [ ] |
| Fontes por quadro | [ ] | [ ] | [ ] |
| Atlas idle | [ ] | [ ] | [ ] |
| Atlas walk | [ ] | [ ] | [ ] |
| Atlas attack | [ ] | [ ] | [ ] |
| Atlas death | [ ] | [ ] | [ ] |
| Atlas rest | [ ] | [ ] | [ ] |
| Atlas ally/reanimate | [ ] | [ ] | [ ] |
| Prévia animada | [ ] | [ ] | [ ] |
| Código integrado | [ ] | [ ] | [ ] |
| Execução CI | [ ] | [ ] | [ ] |
| APK/Windows | [ ] | [ ] | [ ] |

**Limitações assumidas e aprovadas:** [DESCRIÇÃO].

### 10. Diário de execução

**[DATA] — [ETAPA]**

- Entrada e referência efetivamente analisadas:
- Trabalho executado nesta sessão:
- Imagens/atlas criados:
- Arquivos do GitHub criados/alterados:
- Commit(s) e branch:
- Verificações concluídas e resultados:
- Comportamentos testados em jogo:
- Erros, limitações e dependências:
- O que ainda NÃO está integrado ou testado:
- GATE/Status atualizado:
- **PRIMEIRA PRÓXIMA AÇÃO:**

### 11. Mensagem de retomada para outra sessão

“Continuar o personagem [NOME/ID] no repositório Takeo-Ishikawa/necromancer-rise-of-the-fallen. Leia docs/PLANO_MESTRE_DESENVOLVIMENTO_PERSONAGENS.md e docs/personagens/[ID]/FICHA_[ID].md. Inspecione os arquivos/commits reais e a última referência visual aprovada. Retome exatamente a ETAPA ATIVA e a PRIMEIRA PRÓXIMA AÇÃO, sem apagar os sprites existentes, reconstruir um personagem aprovado do zero ou mudar seu conceito. Preserve colisão, saves, servos, seleção e workflows independentes. Registre cada arquivo/commit, declare claramente o que foi de fato testado e atualize a ficha ao final.”

**Lembrete:** uma ficha é obrigatória para CADA personagem; o plano-mestre permanece genérico.

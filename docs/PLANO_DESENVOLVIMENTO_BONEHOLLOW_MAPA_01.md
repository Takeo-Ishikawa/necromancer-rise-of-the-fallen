# PLANO DE PRODUÇÃO — BONEHOLLOW / MAPA 01
## Necromancer: Rise of the Fallen — cenário 2D de fantasia sombria

**Versão do plano:** 1.0  
**Data de referência:** 13/11/2026  
**Repositório:** Takeo-Ishikawa/necromancer-rise-of-the-fallen  
**Engine:** Unity 2022.3.62f2 LTS  
**Região:** bonehollow — “A Caverna Esquecida”  
**Situação do documento:** planejamento aprovado como direção de trabalho; etapas de produção visual ainda não executadas.  
**Objetivo principal:** elevar Bonehollow a um cenário ilustrado, coerente, explorável e performático no Android, sem comprometer combate, servos, salvamentos ou progressão.

> REGRA PARA NOVAS SESSÕES: ler este plano e inspecionar a branch atual antes de editar. Não interpretar uma etapa planejada como implementada. Registrar o resultado de cada etapa na seção “Diário de execução”, com commits, testes e pendências reais. Não compilar Android e Windows automaticamente a cada alteração.

---

## 1. Visão do resultado final

Bonehollow será uma caverna natural de fantasia sombria, com terreno orgânico, paredes volumosas, rochas, ossadas, fungos bioluminescentes, cristais e ruínas discretas. A imagem não deve parecer uma colagem nem uma grade repetitiva. O cenário deve ter uma identidade visual à altura dos sprites do Rotboar, do rato corrompido e da protagonista, mantendo-os legíveis mesmo com vários servos na tela.

**Pilares visuais e de jogabilidade:**

1. **Leitura imediata:** os contornos e as animações do jogador, dos monstros e dos corpos derrotados são prioritários; efeitos e decorações não devem esconder informação importante.
2. **Profundidade crível:** piso, rocha, sombra, contato com o chão, objetos altos e oclusão precisam parecer parte de um mesmo ambiente.
3. **Exploração real:** entrada, ramificações, atalhos, marcos memoráveis e transições entre regiões, sem corredores estreitos demais para o conjunto de servos.
4. **Modularidade:** cada elemento pode ser movido, substituído, redimensionado ou reaproveitado individualmente, sem redesenhar o mapa inteiro.
5. **Desempenho verificável:** nenhum aumento de detalhamento deve ser aprovado apenas por parecer bonito em uma imagem estática; exige teste no aparelho.
6. **Compatibilidade:** não alterar regras de inimigos, colisões, câmera, fases, salvamentos nem a assinatura do APK sem decisão expressa e validação.

**Decisão técnica principal:** desenvolvimento incremental sobre o sistema existente de cenário criado por código. A adoção de Tilemap/RuleTile, URP 2D e iluminação dinâmica é uma decisão técnica a ser testada em protótipo, não uma migração já autorizada nem uma dependência obrigatória.

---

## 2. Estado real do projeto e pontos que não podemos perder

Dados verificados na branch main em 13/11/2026:

| Elemento | Estado de referência |
| --- | --- |
| Mapa atual | 56 × 34 unidades Unity |
| ID da fase | bonehollow |
| Ponto inicial no JSON | (7,8; 29,3) |
| Selo / saída no JSON | (49,1; 5,8) |
| Posição do Rotboar no JSON | (45,4; 10,4) |
| Obstáculos | 80 entradas no JSON |
| Recursos | 12 pontos de cristais |
| Inimigos comuns inicialmente posicionados | 4 Grave Pups, 3 ratos, 2 morcegos, 2 slimes e 1 aranha |
| Criatura especial | 1 Rotboar criado separadamente |
| Áreas visuais já nomeadas | entrada, ossadas, fungos, poço central, cristais, covil e selo |
| Fundo ativo | entrada_da_caverna_bonehollow.png |
| Cenografia | NecroScenery cria props procedurais; NecroCaveZones cria manchas ambientais |
| Minimap | Código de minimapa, névoa e salvamento já adicionado; teste visual em APK ainda precisa ser confirmado |
| Scripts de combate | Devem continuar funcionando sem mudança funcional nesta produção visual |

**Fonte de verdade das coordenadas:** unity/Assets/Resources/Data/levels.json. Os dados usam origem no canto superior esquerdo; NecroLevels.At converte para a origem inferior esquerda do Unity. Não misturar os dois sistemas; os obstáculos, spawns, recursos, desenho e minimapa precisam obedecer à mesma transformação.

**Importante:** docs/CAVERNA_INICIAL.md descreve também uma implementação histórica Java/Canvas. Não usar esse mapa antigo como fonte da geometria Unity atual.

**Arquivos existentes relevantes:**

- unity/Assets/Scripts/NecroPrototype.cs — ciclo de vida, criação da região, entidades e câmera.
- unity/Assets/Scripts/NecroLevels.cs — leitura dos dados e transformação de coordenadas.
- unity/Assets/Scripts/NecroScenery.cs — decoração procedural e sprites de cenografia.
- unity/Assets/Scripts/NecroCaveZones.cs — zonas ambientais coloridas.
- unity/Assets/Scripts/NecroGameplay.cs — movimentação, obstáculos, interação e combate.
- unity/Assets/Scripts/NecroRespawn.cs — ressurgimento de inimigos.
- unity/Assets/Scripts/NecroMiniMap.cs e NecroUI.cs — mapa, interface ativa e controles.
- unity/Assets/Scripts/NecroSanctuary.cs — câmara segura e transição.
- unity/Assets/Scripts/NecroCrypt.cs e NecroSaveData.cs — salvamento.
- unity/Assets/Resources/Backgrounds/entrada_da_caverna_bonehollow.png — fundo atual, que deve ser preservado como fallback.
- unity/Assets/Resources/Scenery/ — recursos e substituições artísticas disponíveis.
- unity/Assets/Editor/NecroSanctuaryImportSettings.cs — referência de política de texturas Android.
- tests/check_unity_project.py — verificações estruturais obrigatórias.
- docs/PADRAO_OTIMIZACAO_ANDROID.md — padrão existente de qualidade e compactação.
- docs/COMPILACAO_ANDROID_WINDOWS.md — builds manuais e independentes.

**Status inicial dos marcos:** [x] protótipo de mapa e gameplay existentes; [x] dados de encontros e mapa cadastrados; [x] código inicial de minimapa; [ ] direção visual definitiva; [ ] kit modular de ambiente; [ ] vertical slice aprovada; [ ] mapa ilustrado completo; [ ] iluminação/áudio finais; [ ] QA Android do novo cenário.

---

## 3. Organização de trabalho e regras de segurança

1. Antes de começar uma etapa, comparar main com os últimos commits e registrar o commit-base. Trabalhar preferencialmente em uma branch temática, por exemplo feature/bonehollow-art-v2, e integrar apenas após a verificação da etapa.
2. Preservar o terreno atual, o JSON e os scripts originais até o substituto ser comprovadamente funcional. Não apagar assets apenas por terem sido visualmente substituídos; remover da árvore ativa só depois de confirmar ausência de referências, usando o histórico Git como recuperação.
3. Separar **arte-mestre** e **arte para runtime**. Arquivos-fonte grandes, estudos, referências e versões descartadas ficam fora de Assets/Resources. Apenas texturas realmente carregadas pelo jogo entram em Resources.
4. Separar cenário visual de colisão. A aparência de uma rocha não define sozinha a área bloqueada. Manter a geometria do JSON como autoridade até haver migração deliberada.
5. Não alterar os sprites existentes dos personagens e criaturas durante a produção do cenário. Ajustar contraste do ambiente primeiro; qualquer mudança de tamanho/pivô dos personagens exige etapa própria.
6. Cada melhoria deve permitir rollback isolado: um kit de piso, uma zona, um importador ou um efeito pode ser revertido sem desfazer o restante.
7. Executar testes estáticos a cada entrega de código; compilar somente a plataforma selecionada quando houver uma etapa pronta para teste jogável.
8. Registrar ao fim de cada sessão: arquivos modificados, commit, verificações concluídas, falhas e **o primeiro próximo passo exato**.

**Estrutura de arquivos proposta (não presumir que já existe):**

- unity/Assets/ArtSource/Bonehollow/ — estudos e fontes de trabalho opcionais; não carregados em runtime.
- unity/Assets/Resources/Environment/Bonehollow/Ground/ — partes do piso usadas por carregamento dinâmico.
- unity/Assets/Resources/Environment/Bonehollow/Walls/ — paredes, bordas e entradas.
- unity/Assets/Resources/Environment/Bonehollow/Props/ — rochas, ossos, fungos, cristais, raízes e ruínas.
- unity/Assets/Resources/Environment/Bonehollow/Effects/ — máscaras, texturas discretas e partículas indispensáveis.
- unity/Assets/Scripts/Bonehollow/ — novos módulos somente quando a separação melhorar manutenção.
- docs/bonehollow/ — inventário, matriz de materiais, capturas de QA e diário de benchmark, quando necessários.

*Observação:* usar os diretórios acima apenas se forem compatíveis com a forma como a cena é criada em runtime. Evitar duplicar imagens já acessíveis em Resources/Scenery. Se o objeto virar referência serializada na cena, ele não precisa obrigatoriamente estar em Resources.

---

## 4. Etapas e critérios de conclusão

### ETAPA 00 — Auditoria, baseline e proteção do mapa atual
**Prioridade:** P0. **Dependências:** nenhuma. **Estado:** pendente.

**Tarefas:**
- [ ] Registrar commit-base, versão do APK, tamanho real do APK e plataforma de teste.
- [x] Documentar posições de entrada, selo, Rotboar, mobs, recursos e obstáculos a partir do JSON (ver auditoria 00).
- [ ] Registrar screenshots de entrada, poço, cristais, fungos, ossadas, covil e câmara segura, com resolução do aparelho.
- [x] Verificar os caminhos realmente utilizados por Resources.Load e os scripts responsáveis por criar rochas e decorações (ver auditoria 00).
- [ ] Confirmar a localização de cada zona e conferir se as regiões visuais coincidem com a navegação real.
- [ ] Medir, em um aparelho disponível, FPS, tempo de entrada, uso de memória quando possível e tamanho do APK antes da mudança.
- [x] Preparar rollback isolado do conjunto de arquivos de cenário (branch temática criada; main e Assets intactos).

**Entregável:** documento de auditoria com baseline e lista de riscos; referência ao commit-base.

**Aceite:** mapa atual ainda compila; colisões, travessias, respawn, servos, minimapa e save mantêm o comportamento anterior. Nenhum asset foi substituído nesta etapa.

### ETAPA 01 — Bíblia visual e padrão de escala
**Prioridade:** P0. **Dependências:** 00. **Estado:** em andamento (direção de arte e estudos esquemáticos produzidos; testes e referências pendentes).

**Tarefas:**
- [ ] Definir três a cinco imagens de referência próprias ou licenciadas para consulta, sem importar conteúdo de terceiros sem autorização.
- [x] Fixar perspectiva top-down/3⁄4 compatível com personagens, escala de piso, direção da luz-base e tamanho das sombras.
- [x] Definir paleta de rochas cinza-violeta, ossos marfim envelhecido, fungos verde-azulados e cristais azuis/violetas.
- [x] Criar guia de contraste: centro do caminho mais limpo; bordas mais detalhadas; silhuetas do jogador e inimigos destacadas.
- [x] Definir regras de espessura das paredes, densidade de detritos, transparência e oclusão.
- [x] Produzir uma folha de amostras com chão, parede, rocha, cristal, fungo e sombra alinhados à mesma perspectiva. (amostras esquemáticas, não sprites finais)
- [x] Definir pivô, pixels por unidade e faixa de dimensões dos props; medir no tamanho realmente exibido em dispositivo, em vez de adotar resolução máxima por hábito. (regras documentadas; medição no aparelho pendente)

**Entregável:** guia de estilo com paleta, proporções, pivôs, exemplos de luz/sombra e regras de legibilidade.

**Aceite:** as amostras parecem um único cenário, não elementos montados; personagem, rato e Rotboar permanecem nítidos contra o fundo.

### ETAPA 02 — Layout navegável e planta de zonas
**Prioridade:** P0. **Dependências:** 00 e 01. **Estado:** pendente.

**Tarefas:**
- [ ] Sobrepor em uma planta de 56 × 34 as áreas existentes, os 80 obstáculos, todos os spawns, os 12 cristais, o início e o selo.
- [ ] Marcar caminhos principais, rotas alternativas, pontos de combate, gargalos e marcos visuais.
- [ ] Verificar alcance real dos servos nas bifurcações e circulação do Rotboar durante investidas.
- [ ] Preservar a navegação entre entrada, regiões intermediárias, covil e selo. Não mover spawns ou rochas só para melhorar uma composição sem validação.
- [ ] Anotar espaços de segurança para ataques, cadáveres, captura/colheita e respawn fora da visão.
- [ ] Planejar onde a atmosfera fica mais intensa sem ocultar área de combate.
- [ ] Propor, **sem aplicar automaticamente**, eventuais ajustes de geometria que melhorem a exploração.

**Entregável:** planta anotada com coordenadas no padrão do JSON, mapa de acessibilidade e lista de mudanças opcionais.

**Aceite:** há pelo menos um trajeto livre entre os objetivos importantes com a folga necessária para jogador e servos; não existe passagem obrigatória bloqueada por decoração.

### ETAPA 03 — Vertical slice da entrada e transição para ossadas
**Prioridade:** P0 / primeiro marco visual. **Dependências:** 01 e 02. **Estado:** pendente.

Construir somente uma área contígua próxima à entrada existente, incluindo um pequeno trecho de transição para ossadas. O recorte exato será definido após a etapa 02; evitar impor limites arbitrários que atravessem obstáculos ou spawns.

**Tarefas:**
- [ ] Criar duas ou três variações de piso, uma parede, um contorno de parede, duas rochas e dois objetos de ossada.
- [ ] Montar esses elementos por camadas, como objetos reutilizáveis, preservando a geometria atual.
- [ ] Aplicar sombras de contato consistentes e ordenar objetos pela profundidade sem ocultar a protagonista.
- [ ] Posicionar os primeiros ratos/Grave Pups existentes sem modificar suas regras.
- [ ] Testar movimentação em oito direções, diagonal, empurrão/contato, ataque manual e acompanhamento de servos.
- [ ] Produzir capturas durante movimento e combate, não apenas uma arte estática.
- [ ] Comparar a integração com o cenário antigo, mantendo uma opção de fallback.

**Entregável:** entrada demonstrável em execução real, com checklist de arte, colisão e desempenho.

**Aceite (GATE 1):** qualidade visual aprovada em screenshot e em movimento; nenhuma oclusão injusta; desempenho aceitável no Android. **Não expandir o restante da caverna antes da aprovação deste marco.**

### ETAPA 04 — Sistema de chão modular e transições orgânicas
**Prioridade:** P1. **Dependências:** GATE 1. **Estado:** pendente.

**Tarefas:**
- [ ] Criar conjunto limitado de piso-base, fissuras, terra, umidade e erosão, com continuidade de iluminação e escala.
- [ ] Decidir entre mosaico de sprites, Tilemap/RuleTile ou solução híbrida, após comparar aparência, facilidade de manutenção e custo de renderização.
- [ ] Remover padrões óbvios de repetição por variação de decalques e composição, não por dezenas de texturas duplicadas.
- [ ] Concentrar detalhes junto a bordas e marcos; manter centro do caminho visualmente legível.
- [ ] Reutilizar materiais/texturas e controlar transparências sobrepostas.
- [ ] Garantir costuras invisíveis entre módulos na resolução do celular.
- [ ] Validar o desenho contra a câmera ortográfica atual e a escala real dos inimigos.

**Entregável:** biblioteca de piso e transições aplicável a toda Bonehollow.

**Aceite:** ao percorrer o ambiente, não surgem cortes, quadrículas evidentes, mudanças bruscas de perspectiva nem padrões excessivamente repetidos.

### ETAPA 05 — Paredes, rochas altas, oclusão e colisão
**Prioridade:** P1. **Dependências:** 04. **Estado:** pendente.

**Tarefas:**
- [ ] Produzir peças de parede reta, curvas/cantos, boca de passagem, rocha isolada, blocos grandes e variações de borda.
- [ ] Diferenciar o piso caminhável, parede inacessível e decoração atravessável.
- [ ] Aplicar sombra projetada e contato no piso, compatíveis com a direção da luz-base.
- [ ] Definir ordenação por profundidade (Y ou pontos de ancoragem) para props altos; evitar que objetos atravessem ou cubram o jogador de forma incorreta.
- [ ] Mapear a geometria desenhada aos obstáculos já existentes; criar ou alterar colisores **somente** com uma migração explícita.
- [ ] Testar bordas da caverna, diagonal, servos numerosos, Rotboar e recuperação de cadáveres junto às rochas.
- [ ] Eliminar sobreposições de sombra e elementos soltos que pareçam colagem.

**Entregável:** kit de paredes e rochas com pivôs consistentes, separação de elementos e plano de oclusão.

**Aceite:** silhuetas, sombras e colisões condizem; não há pontos onde pareça possível passar mas a geometria impeça, ou vice-versa.

### ETAPA 06 — Construção dos biomas internos e marcos memoráveis
**Prioridade:** P1. **Dependências:** 04 e 05. **Estado:** pendente.

**Sequência recomendada:**

1. **Entrada:** piso mais aberto, luz controlada e indícios de perigo. É a primeira leitura de movimentação e combate.
2. **Galeria das Ossadas:** ossos, restos de criaturas, pilares quebrados e tons quentes apagados; não saturar o centro do combate.
3. **Setor de Fungos:** fungos volumétricos, esporos discretos, manchas úmidas e pontos de brilho verde-azulado; transição gradual.
4. **Poço Central:** marco de orientação que conecte rotas, com volume e profundidade sugeridos por pintura e sombra; confirmar se terá ou não colisão antes de criar.
5. **Galeria de Cristais:** cristais de formatos distintos, luz azul/violeta e reflexos leves; utilizar menos fontes luminosas do que a imagem sugere.
6. **Covil do Rotboar:** arena ampla, ossadas maiores, sulcos e sinais de passagem da criatura; preservar espaço para investida e desvio.
7. **Selo e passagem:** iluminação ritual discreta, visual reconhecível e caminho legível até o objetivo.

**Para cada setor:** produzir um pacote de elementos separado; registrar coordenadas, mudanças de cenografia, screenshot antes/depois e teste de acesso. Evitar transformar cada setor em uma nova cena independente se isso desorganizar respawn, saves e transições existentes.

**Entregável:** sete setores visualmente relacionados, com identidade própria e transições naturais.

**Aceite (GATE 2):** mapa inteiro pode ser percorrido; cada setor é reconhecível, mas a caverna parece um ambiente contínuo. Spawns e recursos continuam acessíveis.

### ETAPA 07 — Decoração final, escala e sombreamento de contato
**Prioridade:** P1. **Dependências:** 06. **Estado:** pendente.

**Tarefas:**
- [ ] Produzir assets reutilizáveis de pedregulho, raízes, ossadas, fragmentos de cristal, fungos, entulho e detalhes ambientais.
- [ ] Evitar dezenas de variações quase iguais; a diversidade virá de rotação/espelhamento apropriados, escala moderada e distribuição dirigida.
- [ ] Definir três densidades visuais: caminho limpo, borda decorada e ponto focal denso.
- [ ] Aplicar sombra de contato separada e estável, com pivôs alinhados ao chão.
- [ ] Reduzir excesso de elementos perto de botões, portais e locais de coleta.
- [ ] Separar objetos puramente decorativos de objetos interativos, sem inventar colisão.
- [ ] Inspecionar o cenário em diferentes proporções de tela e com muitos servos.

**Entregável:** biblioteca final de props, prefabs/padrões de instância e lista de assets ativos.

**Aceite:** decoração legível, sem aparência de colagem, sem engasgos ao atravessar setores e sem confusão entre objeto coletável e cenário.

### ETAPA 08 — Luz, atmosfera e efeitos ambientais
**Prioridade:** P1, experimental. **Dependências:** 03 e 06. **Estado:** pendente.

**Tarefas:**
- [ ] Começar com iluminação pintada nos próprios sprites e manchas ambientais já existentes.
- [ ] Testar discretamente luz de cristais/fungos, névoa de caverna e partículas de poeira com limites de overdraw.
- [ ] Fazer um protótipo isolado para comparar o pipeline atual com URP 2D antes de migrar; o manifest atual contém o pacote 2D Sprite, não uma configuração URP pronta.
- [ ] Se a migração for aprovada, criar branch própria para Render Pipeline, materiais e shaders; verificar compatibilidade com todos os personagens e sprites do refúgio.
- [ ] Não deixar névoa cobrir ataques, cadáveres, itens ou indicadores.
- [ ] Medir memória, draw calls, FPS e artefatos da compressão no aparelho real.
- [ ] Garantir versão simplificada para dispositivos menos potentes.

**Entregável:** perfil de atmosfera por setor, com implementação otimizada e fallback simples.

**Aceite:** o jogador entende espaço e ameaça, os efeitos não atrapalham o combate e a melhoria visual justifica seu custo.

### ETAPA 09 — Integração dos inimigos, recursos e objetivos existentes
**Prioridade:** P0 para jogabilidade. **Dependências:** 05–08. **Estado:** pendente.

**Tarefas:**
- [ ] Confirmar os 4 Grave Pups, 3 ratos, 2 morcegos, 2 slimes e 1 aranha cadastrados como encontros comuns iniciais.
- [ ] Validar o Rotboar como encontro especial separado, no covil correspondente.
- [ ] Conferir o atlas dos ratos e do Rotboar, incluindo movimento, ataque, morte e comportamento dos cadáveres.
- [ ] Validar o respawn fora da câmera e longe de recursos; não fazer inimigos surgirem visualmente dentro de paredes.
- [ ] Posicionar claramente os 12 recursos de cristal e conferir alcance de coleta.
- [ ] Confirmar captura, coleta de corpos caídos, reanimação, formação e seguimento de vários servos.
- [ ] Confirmar acesso a selo/portal, progressão de regiões e retorno à câmara segura.
- [ ] Não alterar estatísticas ou IA para consertar problema visual de cenário; registrar separadamente eventuais problemas de balanceamento.

**Entregável:** checklist funcional por setor, com os encontros existentes dentro do novo cenário.

**Aceite:** nenhum personagem nasce fora do caminho, atravessa obstáculos por erro de apresentação, desaparece atrás do cenário ou bloqueia permanentemente a progressão.

### ETAPA 10 — Minimapa e orientação espacial
**Prioridade:** P0 para usabilidade. **Dependências:** 02 e 06. **Estado:** código inicial já existe; QA pendente.

**Tarefas:**
- [ ] Confirmar se o minimapa ativo coincide com a geometria efetiva após a finalização do cenário.
- [ ] Validar jogador, servos, inimigos visíveis/próximos, saída e Rotboar sem sobreposição desnecessária.
- [ ] Testar névoa, revelação em novas áreas, retorno a regiões e restauração após fechar/reabrir o jogo.
- [ ] Testar o modo ampliado e zoom na tela real, incluindo botão de fechar e toque simultâneo com controles.
- [ ] Não afirmar que as paredes do cenário são corredores se o minimapa só conhece obstáculos pontuais; enriquecer os dados de geometria se isso for necessário.
- [ ] Conferir compatibilidade com HUD, retrato, habilidades, botões de ataque e navegação da câmara segura.

**Entregável:** minimapa funcional, legível e correspondente ao Bonehollow final.

**Aceite:** orientação confiável, toque sem conflito e exploração preservada após salvar/carregar; teste jogável obrigatório.

### ETAPA 11 — Áudio, feedback ambiental e pequenas interações
**Prioridade:** P2. **Dependências:** 06–09. **Estado:** pendente.

**Tarefas:**
- [ ] Criar camadas suaves de eco, gotejamento, pedras e vento subterrâneo, sem encobrir áudio de combate.
- [ ] Usar loops curtos e sons ambientes posicionados apenas quando agregarem orientação.
- [ ] Manter variação leve por setor: fungos úmidos, cristais vibrando, covil mais grave e silencioso.
- [ ] Produzir feedback visual/sonoro modesto para passagem, coleta e aproximação de marcos já existentes.
- [ ] Não acrescentar quebra de rochas, armadilhas ou coleta extra sem especificação e aprovação de gameplay.

**Entregável:** mix básico por setor e matriz de fontes sonoras.

**Aceite:** som atmosférico, não repetitivo em excesso, inteligível com combate ativo e controlável pelo sistema de áudio existente.

### ETAPA 12 — Otimização e orçamento de produção
**Prioridade:** P0 antes da aprovação final. **Dependências:** 03–11. **Estado:** pendente.

**Metas iniciais propostas — medir e revisar, não tratar como resultados já obtidos:**

| Indicador | Critério de trabalho |
| --- | --- |
| Fluidez | Buscar 60 FPS no aparelho de referência; aceitar 30 FPS estáveis em dispositivo mais modesto, medindo percentis e oscilações |
| Importação | ETC2 como ponto de partida para Android com alpha quando preciso; perfis específicos por categoria de arte |
| Tamanho de texturas | Dimensionar pelo uso real; evitar 4K para objetos pequenos e manter arte-fonte fora de Resources |
| Reuso | Reutilizar materiais, sprites e partículas; evitar cópias de textura para cada prop |
| Iluminação | Preferir luz pintada ou poucas luzes em movimento; limitar camadas transparentes |
| Memória | Registrar pico antes/depois de cada pacote de assets e comparar sob condições iguais |
| Build | Comparar bytes do APK final no resumo do workflow, não apenas o tamanho do artefato |
| Qualidade | Nenhuma economia deve gerar borrões, halos em transparências, sombras deslocadas ou sprites cortados |

**Tarefas:**
- [ ] Criar importador específico para a arte de Bonehollow, inspirado no padrão já usado para o refúgio.
- [ ] Confirmar TextureImporterType coerente com a forma de carregamento; hoje alguns elementos usam Resources.Load<Texture2D>() e Sprite.Create().
- [ ] Verificar atlas, compressão, textura repetida, animações, culling e objetos instanciados desnecessariamente.
- [ ] Medir o pior caso: covil com Rotboar, vários servos, efeitos e inimigos.
- [ ] Anotar tamanho real do APK, memória, FPS e eventuais artefatos por build.
- [ ] Se o custo for alto, reduzir primeiro densidade de props, transparências, resolução excessiva e efeitos; preservar legibilidade e arte principal.

**Entregável:** relatório comparativo com valores medidos e decisões técnicas, incluindo ajustes revertidos por perda de qualidade.

**Aceite (GATE 3):** qualidade visual preservada e desempenho verificado no Android de teste; não declarar “otimizado” apenas porque a compilação teve sucesso.

### ETAPA 13 — QA integral, regressões e publicação de preview
**Prioridade:** P0. **Dependências:** 00–12. **Estado:** pendente.

**Fluxo obrigatório de testes:**

- [ ] Rodar python tests/check_unity_project.py após alterações relevantes.
- [ ] Confirmar que as verificações Unity/Editor relacionadas a posicionamento, estrutura e cenas continuam aprovadas.
- [ ] Construir **somente Android**, usando o workflow manual Unity Android Preview APK.
- [ ] Instalar o APK sobre a versão anterior, preservando package ID e chave de assinatura do preview; testar se a atualização mantém os saves.
- [ ] Começar pela câmara segura, entrar na caverna e verificar retorno ao refúgio.
- [ ] Percorrer o caminho principal e as rotas alternativas; observar paredes e colisões.
- [ ] Combater ratos, Grave Pups, morcegos, slimes, aranha e Rotboar.
- [ ] Testar cadáver, coleta, captura/reanimação e movimentação com muitos servos.
- [ ] Testar selo, avanço, minimapa ampliado, fog of war e retomada do progresso.
- [ ] Validar telas com proporções/resoluções distintas, com foco em Android.
- [ ] Gerar screenshots e vídeo curto de cada setor, anotando defeitos e correções.
- [ ] Rodar build Windows **se solicitado**; não compilar automaticamente ambas as plataformas.
- [ ] Atualizar documentação com commit final, versão, APK produzido e status real dos testes.

**Entregável:** APK preview Android, tabela de QA, evidências e lista de correções priorizadas.

**Aceite (GATE 4):** a fase completa funciona sem regressões críticas; arte e colisão são coerentes; métricas e limitações estão registradas.

---

## 5. Critérios de aprovação do mapa completo

O mapa só recebe o estado **APROVADO** quando:

- [ ] Todas as etapas P0 concluídas e documentadas.
- [ ] Pelo menos a vertical slice e o mapa integral aprovados em execução real.
- [ ] Estética homogênea: escala, luz, cores, sombra e perspectiva coerentes.
- [ ] Deslocamento claro e sem colisões enganosas; servos acompanham sem travamentos recorrentes.
- [ ] Encontros e recursos existentes mantidos e acessíveis.
- [ ] Rotboar enfrenta o jogador em uma arena adequada para a mecânica de investida.
- [ ] Minimapa corresponde ao cenário e mantém progressão exploratória entre sessões de jogo.
- [ ] Save existente continua válido; Android preview instala como atualização.
- [ ] Métricas e screenshots de Android registrados.
- [ ] Arquivos antigos e arquivos-fonte preservados de forma recuperável.
- [ ] Nenhuma alteração indevida na câmara segura ou nas outras três regiões.

Se um item falhar, registrar o defeito e não marcar a etapa como concluída.

---

## 6. Ordem de execução recomendada em sessões curtas

| Sessão de trabalho | Entrega prática | Encerramento esperado |
| --- | --- | --- |
| Sessão A | Auditoria e planta técnica (00 + 02 inicial) | Baseline, coordenadas, screenshots e perigos |
| Sessão B | Bíblia visual (01) | Paleta, perspectiva, escala e kit de amostra |
| Sessão C | Vertical slice da entrada (03) | Primeira área jogável e avaliada |
| Sessão D | Kit de chão e paredes (04 + 05) | Módulos reutilizáveis, sombras e colisões coerentes |
| Sessões E–G | Biomas e landmarks (06) | Entrada, ossadas, fungos, poço, cristais, covil e selo |
| Sessão H | Props e arte final (07) | Composição orgânica, sem poluição visual |
| Sessão I | Atmosfera experimental (08) | Efeitos controlados e decisão URP fundamentada |
| Sessão J | Encontros, recursos e minimapa (09 + 10) | Fase funcional e navegação verificada |
| Sessão K | Áudio e otimização (11 + 12) | Ambiente e métricas comparativas |
| Sessão L | QA e preview (13) | APK testado, evidências e pendências registradas |

A tabela é uma sequência lógica, **não uma estimativa de horas ou promessa de prazo**. Uma sessão pode abranger menos tarefas; dividir fases complexas conforme a validação real.

---

## 7. Registro de tarefas em andamento — atualizar sempre

**ETAPA ATIVA:** 00 — Auditoria em andamento. Commit-base, inventário e planta preliminar registrados em branch temática. Faltam evidências do APK/aparelho e QA de navegação.  
**PRIMEIRA AÇÃO CONCRETA NA PRÓXIMA SESSÃO:** obter capturas e métricas do APK atual no aparelho; depois verificar em execução o recorte preliminar X=4–22, Y=19–33, especialmente passagens com servos, e desenvolver a bíblia visual da ETAPA 01. O inventário e a planta estática já foram produzidos na branch `feature/bonehollow-baseline-mapa-01`. **Não começar ilustrando o mapa inteiro.**

| Etapa | Status inicial | Evidência necessária |
| --- | --- | --- |
| 00 — Auditoria | [~] Em andamento; auditoria documental e planta criadas, APK/QA pendentes | Commit-base + baseline |
| 01 — Bíblia visual | [~] Em andamento; guia e amostras esquemáticas criados, QA pendente | Guia + amostras |
| 02 — Planta | [ ] Pendente; mapa geométrico preliminar registrado, jogabilidade não validada | Mapa técnico navegável |
| 03 — Vertical slice | [ ] Pendente | Captura jogável + validação |
| 04 — Piso | [ ] Pendente | Kit modular aprovado |
| 05 — Paredes | [ ] Pendente | Kit e colisões validados |
| 06 — Setores | [ ] Pendente | Sete setores revisados |
| 07 — Props | [ ] Pendente | Biblioteca e screenshots |
| 08 — Atmosfera | [ ] Pendente | Comparação de desempenho |
| 09 — Encontros | [ ] Pendente | Checklist funcional |
| 10 — Minimapa | [ ] Código inicial; QA pendente | Teste em APK + save |
| 11 — Áudio | [ ] Pendente | Mix e teste |
| 12 — Otimização | [ ] Pendente | Métricas antes/depois |
| 13 — QA e preview | [ ] Pendente | Execução CI + teste real |

**Convenção:** [ ] Pendente; [~] Em andamento; [x] Concluído e verificado; [!] Bloqueado. Em Markdown, usar texto de status na tabela; checklists podem ser marcados ao concluir.

### Diário de execução

**13/11/2026 — Planejamento**
- Plano detalhado criado a partir dos dados atuais do projeto.
- Nenhuma mudança de terreno, colisões, posicionamento, iluminação ou inimigos foi efetuada por este documento.
- Não foi executada compilação Android ou Windows para criar o plano.
- Próxima tarefa: ETAPA 00, seguida de ETAPA 01 e da planta da ETAPA 02.

**13/11/2026 — Início efetivo da ETAPA 00 (auditoria documental)**
- Commit-base de `main`: `9aa2ac673c781cd76825f8ec9c3a5a5243069021` (último commit do plano antes desta execução).
- Branch isolada: `feature/bonehollow-baseline-mapa-01`. Nenhum asset, JSON de fase, script ou workflow foi alterado; compilação não solicitada.
- Auditoria criada: [docs/bonehollow/BONEHOLLOW_MAPA_01_AUDITORIA_00.md](bonehollow/BONEHOLLOW_MAPA_01_AUDITORIA_00.md). Inclui coordenadas, 80 obstáculos (54 de contorno e 26 internos), 12 inimigos comuns, 12 cristais, Rotboar, zonas, caminhos `Resources.Load`, riscos e indicadores pendentes.
- Planta técnica inicial: [docs/bonehollow/BONEHOLLOW_MAPA_01_PLANTA_AUDITORIA.svg](bonehollow/BONEHOLLOW_MAPA_01_PLANTA_AUDITORIA.svg), gerada a partir do JSON; recorte provisório de entrada X=4–22, Y=19–33. Não é prévia de arte final nem prova de passagem com servos.
- Verificação geométrica preliminar: caminho em grade com raio de segurança 0,40 e 0,65 entre entrada e selo; nenhuma alteração de obstáculos ou spawns. Centro da zona de ossadas coincide com rocha bloqueada, exigindo waypoints reais.
- Testes executados: conferência estática das definições de JSON e rotinas de cenário, checagem geométrica amostrada. **Não** houve execução do Unity, do `tests/check_unity_project.py`, de build Android/Windows ou teste em celular.
- Métricas em aberto: FPS, memória, tempo de entrada, versão e tamanho real do APK, resolução do aparelho, screenshots e QA da movimentação.
- **Status:** ETAPA 00 em andamento; ainda sem aceite. Próxima ação: coletar baseline no aparelho / no último workflow e validar passagens do recorte; elaborar a ETAPA 01 sem substituir o cenário ativo.

**13/11/2026 — ETAPA 01: direção de arte especificada**
- [Bíblia visual v0.1](bonehollow/BONEHOLLOW_MAPA_01_BIBLIA_VISUAL_01.md): paleta sRGB, perspectiva, luz, pivôs, sorting, densidade, contraste, escala e protocolo de integração sem modificar o gameplay.
- [Amostras esquemáticas](bonehollow/BONEHOLLOW_MAPA_01_AMOSTRAS_VISUAIS.svg): seis estudos autorais (piso, parede, rocha, cristal, fungo e ossadas), ainda **não integrados** como sprites de produção.
- Etapa 01 permanece **em andamento** até referências próprias/licenciadas, capturas dos personagens e verificação de nitidez/oclusão no aparelho.
- Build Android: solicitado ao final desta rodada; `.github/workflows/unity-android.yml` exige `workflow_dispatch`. Se a integração conectada não expuser ação de disparo, registrar impedimento em vez de afirmar que compilou.

**Modelo de próxima anotação (copiar ao finalizar uma sessão):**

- Data / etapa:
- Commit-base e commit de entrega:
- Arquivos criados ou alterados:
- Resultado visual e funcional:
- Testes executados (estáticos / Android / Windows):
- FPS, memória e tamanho real do APK, se medidos:
- O que **não** foi testado:
- Problemas encontrados:
- Decisões aprovadas:
- Próxima ação concreta:
- Status do marco correspondente:

---

## 8. Instruções prontas para iniciar um novo chat

**Mensagem recomendada para continuidade:**

“Vamos continuar o desenvolvimento de Bonehollow no repositório Takeo-Ishikawa/necromancer-rise-of-the-fallen. Leia o arquivo docs/PLANO_DESENVOLVIMENTO_BONEHOLLOW_MAPA_01.md na branch main e confirme os commits posteriores ao plano. Identifique a ETAPA ATIVA e o diário de execução. Continue exatamente do primeiro item pendente, preservando a cena atual, os assets existentes, o JSON de geometria, colisões, inimigos, servos, minimapa, salvamento e workflows manuais de Android e Windows. Ao concluir uma etapa, teste o que for possível, registre o resultado verdadeiro e atualize o próprio plano no GitHub. Não faça build de outra plataforma sem necessidade.”

**Diretrizes para o assistente de uma sessão futura:**

- Não recomeçar planejamento do zero quando o documento e a branch estiverem acessíveis.
- Confirmar cada afirmação de “implementado” no código e cada afirmação de “testado” no respectivo resultado de CI/aparelho.
- Priorizar uma mudança incremental, legível e reversível por vez.
- Mostrar prévia visual antes de substituir permanentemente grandes regiões quando a decisão artística estiver em aberto.
- Preservar o vínculo da arte com a geometria: qualquer mudança na planta exige checagem de movimento, minimapa, respawn e progressão.
- Não criar assets provisórios dentro de Resources que ficarão para sempre sem uso.
- Registrar a próxima ação concreta e o commit ao final de cada sessão.

---

## 9. Referências internas e recuperação

- docs/CAVERNA_EXPANDIDA.md — configuração original da caverna ampliada.
- docs/PADRAO_OTIMIZACAO_ANDROID.md — política oficial já adotada para assets e compressão.
- docs/COMPILACAO_ANDROID_WINDOWS.md — configuração dos workflows manuais.
- docs/ROTBOAR_8_DIRECOES.md — animação e backup do Rotboar.
- docs/RATO_CORROMPIDO_SPRITES.md — sprites do rato e limites atuais.
- docs/ENCONTROS_E_EXPLORACAO_0_9_17.md — IA, território e respawn.
- docs/SAFE_CHAMBER.md — refúgio, distinto do mapa de combate.
- tests/check_unity_project.py — verificação estrutural, complementar a testes de jogabilidade.

**Princípio de recuperação:** em caso de erro, voltar apenas o módulo ou asset introduzido pela etapa; não reverter avanços independentes de combate, personagens, interface ou minimapa. Se uma refatoração maior da renderização for necessária, isolar em branch e exigir aprovação de vertical slice e benchmark antes de fundir à main.

**Fim do plano v1.0 — manter atualizado como documento vivo de produção.**

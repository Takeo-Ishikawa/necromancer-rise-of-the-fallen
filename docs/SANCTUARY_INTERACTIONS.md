# Necromancer 0.8.0 — Sistema de interações do refúgio

Este documento descreve a implementação da fonte mágica e define um padrão para futuras estações. Respeitar também docs/PADRAO_OTIMIZACAO_ANDROID.md.

## Funcionamento

Aproxime-se da frente da fonte, à direita da câmara. O botão RECEBER BENCAO aparece apenas dentro do alcance; no Editor, a tecla E também ativa a fonte. A bênção dura 45 segundos, dá +25% de dano ao ataque direto e ao projétil da necromante e +20% de velocidade de caminhada, inclusive após a entrada na caverna. A recarga é de 120 segundos desde a ativação e não permite empilhar bônus. A interface mostra a duração restante e o cooldown.

O bônus é transitório, calculado a partir de Time.time e não modifica atributos nem arquivos de salvamento. Pausar o jogo pausa o cronômetro. Reiniciar o aplicativo cancela bênção e cooldown nesta primeira versão.

## Arquitetura

- unity/Assets/Resources/Data/sanctuary_interactions.json: id, kind, label, ponto acessível, raio, duração, recarga, multiplicadores e posição do halo. Não usar o centro da hitbox sólida como ponto de interação.
- unity/Assets/Scripts/NecroSanctuaryInteractions.cs: registro, validação da distância, cooldown por id, dispatch de ações, UI, tecla E, halo e estado do buff.
- NecroSanctuary.cs: inicialização junto aos objetos modulares.
- NecroPrototype.cs: multiplicador de velocidade calculado no momento da caminhada.
- NecroGameplay.cs: multiplicador de dano somente para a necromante, não para os servos.
- NecroUI.cs: bloqueio da área do botão contra captura pelo joystick.

O halo e o efeito de ativação reutilizam NecroArt.RitualCircle e NecroCombatFx; nenhum PNG adicional é carregado. O mapa, o cenário, as colisões e a escala do personagem não foram alterados.

## Padrão de evolução

Para adicionar armário, altar ou outra estação: incluir registro com ID único e ponto alcançável no JSON, implementar somente a nova ação no dispatcher e criar painel modal se a estação exigir uma janela. Não duplicar o joystick, sistema de cooldown ou arte de alta resolução. Adicionar verificações ao arquivo tests/check_unity_project.py.

## Testes em aparelho

1. Chegar à frente da fonte; o botão só deve aparecer próximo à área acessível.
2. Ativar uma vez e observar halo, duração de 45 s e recarga de 120 s.
3. Tocar repetidamente e confirmar que os bônus não se acumulam.
4. Entrar na caverna antes do término; verificar velocidade, dano e retorno ao normal após expirar.
5. Voltar à fonte e tentar ativar antes e após a recarga.
6. Validar que movimentos, combate, servos, câmera, colisões e imagens HD continuam corretos.

A preview 0.8.0 usa o package com.takeo.necromancer.unity.preview080, separado da preview 0.7.5. A instalação e os efeitos exigem teste no Android após a compilação.


## 0.8.1 — Guarda-roupa interativo

Perto do guarda-roupa no lado esquerdo, o botão **ABRIR ARMARIO** abre uma janela modal com o foco arcano e o manto atuais. O painel mostra os bônus reais já utilizados pelo sistema de combate (foco: +5 dano corpo a corpo e +8 por projétil para cada grau; manto: +18 vida máxima por grau) e permite aprimorar cada item com fragmentos. Custo do foco: 8/14/20 fragmentos para graus 1/2/3; custo do manto: 10/16/22 fragmentos. Os níveis continuam limitados a 3 e continuam compatíveis com os equipamentos ganhos como drops. A melhoria do manto aumenta a vida máxima em 18 e restaura até 18 pontos de vida; não reseta a vida atual. O botão FECHAR encerra o painel sem perder os dados.

O armário usa `NecroSanctuaryWardrobe.cs`, com interface desenhada em GUI e nenhuma imagem nova. O registro `wardrobe` vive em `sanctuary_interactions.json`, fora da hitbox do armário. Melhorias são registradas por `SaveProgress` mesmo quando a necromante está no refúgio; nessa situação o save usa o início do estágio de combate como checkpoint para não registrar posição inválida no mapa. A assinatura Android, o ID estável da preview e o incremento automático de versionCode são preservados.

Testar: aproximar pela direita do guarda-roupa; abrir e fechar sem mover o personagem, custos e limites, bloqueio de upgrade sem cristais, retorno à caverna e reinício preservando os aprimoramentos. O visual de equipamento no personagem continua inalterado nesta etapa; troca visual de trajes exige sprites específicos posteriormente.


## 0.8.2 — Polimento da interface do guarda-roupa

Após testes em um aparelho Android, o painel estava transparente demais: o antigo `GUI.Box` utilizava o fundo translúcido do tema IMGUI padrão e não separava a interface do cenário da câmara. A versão 0.8.2 substitui esse fundo por um quadro roxo-acinzentado escuro de alpha 0,97, um véu discreto sobre o mundo, sombra externa e bordas claras. Cada equipamento agora ocupa um cartão independente de alpha 0,975; títulos, valores, descrição e botões têm contraste e hierarquia de leitura reforçados. Botões habilitados e desabilitados possuem superfícies próprias; o botão FECHAR é destacado e as áreas clicáveis continuam no mesmo painel modal. Toda a composição usa `FillUi`/`Texture2D.whiteTexture`, já existentes no projeto: nenhuma textura nova, animação ou efeito pesado foi introduzido. Custos, níveis, colisões, saves e identificação/assinatura de APK são preservados.

**Validação recomendada:** abrir o guarda-roupa na câmara, conferir leitura com o cenário brilhante, verificar botões desativados com 0 fragmentos, aprimorar com fragmentos, fechar, caminhar e reabrir. O número visual muda para 0.8.2; o `versionCode` continua crescente pelo número da execução para atualizar a instalação anterior.


## 0.9.0 — Altar de meditação e progressão de nível

O altar de meditação, no canto inferior direito, possui agora a estação `meditation` com ponto alcançável **pela esquerda** (x=10,72; y=8,90; raio=1,62). Aproxime-se e pressione **MEDITAR** ou **E** no Windows. O painel modal usa os mesmos componentes opacos do guarda-roupa, sem sprites adicionais.

Matar criaturas hostis concede XP uma única vez por morte, inclusive quando um servo aplica o golpe final. O limiar de experiência para cada nível é `28 + nível × 18`; cada nível adquirido (até 100) concede um ponto para distribuir. Os pontos disponíveis são derivados do nível e do total investido, impedindo valores duplicados no salvamento.

Três atributos: **Vitalidade** concede +12 de vida máxima e recupera até 12 de vida; **Poder** concede +2 de dano corpo a corpo e +4 de dano do projétil por ponto (não afeta os servos); **Agilidade** concede +6% de velocidade de movimento por ponto, combinado multiplicativamente com o bônus temporário da fonte. Cada um comporta até 12 pontos; pontos excedentes ficam reservados para futuras habilidades. Não é possível gastar pontos que não existem, o botão FECHAR não consome recursos e a tecla Esc fecha o painel no Windows.

### Compatibilidade e persistência

O arquivo `necro-save-v1.json` continua na mesma versão, incluindo três campos **opcionais** `vitalityPoints`, `powerPoints` e `agilityPoints` (zerados automaticamente em saves antigos). Ao iniciar no refúgio, o jogo lê agora a experiência, o nível, os equipamentos, os atributos, a vida, a ânima, os rituais e os fragmentos do arquivo existente; o sistema de companheiros permanece em PlayerPrefs. O preview ainda começa no refúgio, sem recriar automaticamente os inimigos e as regiões guardadas no snapshot anterior. Essa restauração integral será uma evolução posterior.

Testes: (1) matar criaturas e atingir o próximo nível; (2) voltar ao refúgio e abrir o altar; (3) conferir pontos restantes e bônus; (4) reiniciar e confirmar persistência dos investimentos; (5) testar cap de atributo, falta de pontos, voltar do altar, e interação com fonte/guarda-roupa. Android e Windows compartilham o sistema mas mantêm salvamentos locais separados. Os workflows de compilação seguem **manuais** e independentes.


### Proteção dos checkpoints anteriores

Enquanto o preview inicia na câmara segura, o novo carregamento de atributos mantém também o checkpoint de combate, registros de áreas ainda não abertas e os servos guardados na Cripta. Um investimento no altar ou armário não deve apagar essas informações. Os registros de estágio só são substituídos quando o respectivo estágio realmente está carregado. O teste automatizado verifica esta proteção.


## 0.9.1 — Primeira árvore de habilidades

A estação **MEDITAR** agora possui duas abas: **ATRIBUTOS** e **HABILIDADES**. A árvore é visualizada em três caminhos paralelos, cada um com duas habilidades conectadas por uma linha que indica a dependência. A segunda habilidade exige a primeira desbloqueada. Os cartões mostram o efeito real, estado aprendido/bloqueado e botão **APRENDER**. O painel é rolável e funciona em retrato Android e em janela Windows; utiliza apenas as mesmas cores e superfícies da interface do guarda-roupa, sem texturas extras.

O orçamento é **independente** dos pontos de atributos: ganha-se 1 ponto de habilidade a cada 2 níveis, começando no **nível 3** (níveis 3, 5, 7, 9, 11 e 13). Os pontos não usados permanecem livres para escolher qualquer ramo e não podem ser gastos duas vezes. Total inicial da árvore: 6 nós.

| Ramo | Primeira habilidade | Segunda habilidade (requer a primeira) |
|---|---|---|
| Necromancia | **Laço Sombrio:** ressuscitar criaturas custa 3 de ânima a menos, mínimo 1 | **Servo Voraz:** servos causam +15% de dano, incluindo grave pups e rotboar aliados |
| Essência | **Fluxo de Ânima:** +1,5 ânima por segundo de regeneração | **Ceifa de Almas:** recupera 4 de ânima ao derrotar hostis, com o limite da reserva máxima |
| Magia Espectral | **Eco Espectral:** +12% de dano do projétil da necromante | **Ritmo Profano:** reduz a recarga do lançamento espectral de 0,72 para 0,60 segundo |

A recuperação na Ceifa acontece também quando o golpe final é de um servo. O desconto de Laço Sombrio afeta a ressurreição convencional por cadáver; as interações de vínculo livre do grave pup e rotboar não cobram ânima e permanecem assim. Servo Voraz é aplicado **no cálculo do golpe**, inclusive para servos já presentes, sem alterar permanentemente os valores de ataque serializados e sem aplicar bônus aos inimigos. O ganho de Eco Espectral é exclusivo do projétil da protagonista; não afeta golpe direto nem servos.

### Persistência e compatibilidade

O salvamento permanece **`necro-save-v1.json` na versão 1** e recebe somente o campo opcional `learnedSkills` (máscara inteira de seis bits). Saves anteriores têm máscara zero e seguem válidos. A carga rejeita dependências inválidas e reduz pontos aprendidos acima do orçamento legítimo do nível. Os 3 atributos da versão 0.9.0 continuam intactos. A assinatura permanente, o identificador da prévia Android e os workflows **manuais e independentes** não foram modificados.

Teste sugerido: derrotar criaturas até nível 3, abrir o altar, escolher HABILIDADES, aprender uma habilidade inicial, verificar o efeito no combate; avançar ao nível 5, desbloquear a segunda da mesma trilha; voltar a atributos e confirmar que seus pontos não foram consumidos; reiniciar e verificar as habilidades aprendidas. Não foi executada compilação Unity nesta etapa: a validação de runtime no Windows e Android requer disparar o workflow correspondente manualmente.


**Correção adicional de combate (0.9.1):** a função de regeneração já estava definida mas não era chamada por `Update`. Agora `Regenerate(dt)` roda regularmente; ao atingir a ânima máxima, limpa o acumulador fracionário para impedir que ela fique estocada invisivelmente e se recupere instantaneamente depois de um gasto. Fluxo de Ânima amplia a taxa de regeneração já ativa.


## 0.9.2 — Progresso legível e redistribuição de habilidades

O altar exibe agora o **nível, pontos disponíveis de atributos e habilidades, XP atual e barra de progresso**. A aba selecionada recebe um destaque discreto e as habilidades aprendidas aparecem com linha superior esverdeada; habilidades disponíveis e bloqueadas possuem estados visuais diferentes. O conteúdo continua rolável e não usa imagens extras.

No final da aba HABILIDADES existe **REORGANIZAR PONTOS**, disponível após aprender pelo menos uma habilidade. A operação custa **8 + 4 fragmentos por habilidade aprendida**, exige uma confirmação explícita e devolve todos os pontos de habilidade, preservando os pontos e os graus de Vitalidade, Poder e Agilidade. O custo e o estado da árvore são gravados juntos; se o salvamento falhar, os recursos e habilidades originais são restaurados. O comando só funciona dentro da câmara segura.

A interface do jogo também passa a mostrar uma barra compacta de XP na caverna, o total de pontos pendentes para o altar e os avisos temporários de combate/subida de nível. Antes, o código gerava alguns avisos que não apareciam quando o HUD antigo não era desenhado. A implementação usa exclusivamente a interface IMGUI e as cores já presentes no jogo.

**Compatibilidade:** o save v1 e os seis bits da árvore permanecem iguais. A versão de exibição é 0.9.2; o identificador/assinatura Android e os workflows manuais de Windows e Android não mudaram. Para testar, subir de nível, verificar XP e avisos, comprar duas habilidades, reorganizar com confirmação e reiniciar para conferir o save. Nenhuma compilação foi executada automaticamente.


## 0.9.3 — Primeira habilidade ativa: Pulso Necrótico

O combate recebeu **Pulso Necrótico**, uma habilidade ativa básica da necromante. É usada pelo botão **PULSO** (à esquerda de ATACAR) no Android ou pela tecla **Q** no Windows; clicar no botão com o mouse no Windows também funciona. O botão mostra o custo de ânima e o tempo de recarga. O movimento por joystick não captura o toque usado no botão, e os controles básicos de ataque/ressurreição permanecem independentes.

**Mecânica:** consome **18 de ânima**, causa dano imediato em área de **raio 2,7 unidades** ao redor da necromante (ajustado pelo raio físico do inimigo) e entra em **recarga de 9 segundos**. O dano base é `25 + 3 × (nível−1) + 3 × Poder + 4 × grau do foco`, multiplicado pelo bônus ativo da fonte. Afeta somente hostis vivos com linha de visão livre; o próprio personagem e os servos não sofrem dano. Não pode ser usado no refúgio ou com painéis modais abertos. Se não houver alvos alcançáveis ou a ânima for insuficiente, a habilidade não consome ânima nem inicia recarga. A ânima se regenera normalmente depois do uso. O ataque básico continua sem custo de ânima.

**Visual e desempenho:** reutiliza o círculo ritual pré-existente (`NecroArt.RitualCircle`) e o objeto `NecroCombatFx`, agora com tipo 3 (pulso roxo temporário abaixo dos personagens). Não adiciona PNGs, shaders ou efeitos pesados. Ao derrotar vários inimigos ao mesmo tempo, o ganho de XP funciona por criatura, mas o salvamento é realizado uma única vez após a sequência de dano; o aviso de subida de nível tem prioridade sobre a mensagem do pulso.

**Persistência e compatibilidade:** nenhum novo campo foi necessário no `necro-save-v1.json`. Ânima atual, XP, mortes e demais progressos são persistidos com os dados existentes. A recarga não é salva: o efeito é instantâneo e limitado por sessão, com a recarga mantida ao entrar e sair da câmara durante a partida. As builds Android e Windows seguem independentes, manuais e com o mesmo ID/assinatura da preview Android. Versão de exibição 0.9.3.

**Roteiro de teste:** (1) Em combate, pressionar Q ou PULSO longe dos monstros, sem gastar ânima; (2) entrar no alcance e ativar, conferir vários alvos, impacto e recarga; (3) tentar novamente antes de 9s; (4) usar ao lado de servos, conferir que eles não recebem dano; (5) testar com menos de 18 de ânima; (6) conferir XP e nível ao abater vários inimigos; (7) voltar à câmara, onde não existe botão de pulso. Verificar a versão compilada pela Unity antes de distribuir a testers.

Nota visual: o círculo ritual começa menor e expande até cerca de 5,36 unidades de diâmetro, acompanhando o raio efetivo de 2,7 unidades sem parecer que o golpe alcança inimigos mais distantes.

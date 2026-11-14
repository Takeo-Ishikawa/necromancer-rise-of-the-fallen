# Bonehollow — Caverna Unificada (mapa 01, revisão 2)

**Decisão de escopo:** substituir as três regiões `lower_tunnels`, `necropolis` e `marsh` por uma única grande região contínua, `bonehollow`. A **câmara segura (santuário)** permanece separada e não foi apagada.

## Mundo implementado

- Dimensão: **160 × 104 unidades Unity**, aproximadamente 8,7 vezes a área do Bonehollow anterior (56 × 34).
- Geometria: matriz determinística de 160 colunas × 104 linhas em `unity/Assets/Resources/Data/levels.json`. `#` é rocha sólida; `.` é piso transitável; a origem JSON é no canto superior esquerdo.
- Topologia: **29 ligações autorais** entre os grandes salões, desvios alternativos, atalhos e ramificações sem saída. Câmaras com contornos irregulares, corredores sinuosos, regiões amplas e túneis menores.
- Entrada: **(14, 11)**, no alto do mapa, representando a passagem da superfície para o subsolo. Destino final: **(145, 94)**, no Santuário do Abismo; **Rotboar (132, 77)** possui arena ampla.
- Nove regiões interligadas: Entrada da Superfície, Salões das Ossadas, Jardins de Fungos, Abismo do Poço, Floresta de Cristais, Ossuário Profundo, Lago Subterrâneo, Covil das Presas e Santuário do Abismo.
- **43 pontos de inimigos** autorais com Grave Pups, ratos, morcegos, slimes, aranhas, cobras, esqueletos e wisps; o Rotboar é criado separadamente pelo gameplay.
- **28 recursos** distribuídos entre cristais, ossos e fungos.

Planta: [BONEHOLLOW_CAVERNA_UNIFICADA_PLANTA.svg](BONEHOLLOW_CAVERNA_UNIFICADA_PLANTA.svg). É um desenho técnico derivado do JSON, **não** screenshot do APK nem ilustração final.

## Arquitetura aplicada

`NecroCaveNetwork.cs` é a fonte única para paredes sólidas, cores de terreno e compatibilidade de piso com os biomas:

1. **Colisão real:** `NecroGameplay.Blocked` usa círculo contra células sólidas (sem milhares de `GameObjects` individuais). Limites exteriores continuam fechados.
2. **Imagem de terreno:** o mapa é pintado em **uma textura RGB24** sem mipmaps, 16 pixels por unidade, com cor por bioma, ruído mineral leve e bordas rochosas. Isso é um protótipo de linguagem visual, não uma textura artística final.
3. **Minimapa:** extrai a forma das mesmas células da colisão. Névoa de exploração verifica linha de visão para não descobrir corredores atrás de paredes.
4. **Ambientação:** `NecroCaveZones` posiciona atmosferas sobre os nove salões; `NecroScenery` distribui até 250 props pequenos, apenas em piso livre e com vocabulário por bioma.
5. **Encontrar monstros:** 43 encontros de partida e limite de população pós-derrota ampliado para a área; respawn não ocorre dentro de rochas nem sobre os personagens. A estrutura de combate e os sprites de criaturas não foram alterados.
6. **Progressão:** não existe mais `AVANÇAR` para mapas 2, 3 e 4; o objetivo é percorrer um único mundo contínuo. O refúgio permanece acessível pela mecânica existente.

## Compatibilidade de salvamentos

Não foi alterado o arquivo-base `necro-save-v1.json`, a versão serializada de save nem o identificador do aplicativo Android. Foi acrescentado o campo **opcional** `worldLayoutRevision=2`. Em saves anteriores, o valor ausente é tratado como 0: os registros de posicionamento/exploração da geometria antiga são descartados, mas **nível, atributos, equipamentos, fragmentos, servos e cofre** são preservados. Isso evita ressuscitar mobs sob paredes ou carregar fog-of-war de um mapa com dimensões diferentes.

## Validação e riscos

- [x] O JSON contém exatamente um mapa e nove biomas.
- [x] Colisão, renderização e minimapa compartilham a matriz de paredes.
- [x] Grafo acessível com folga de 0,60 unidade para entrada, selo, Rotboar, 43 inimigos, 28 recursos e centros dos nove biomas, pelo verificador estático.
- [x] Teste estrutural automatizado do Unity passou após atualização das assertivas de regiões e dimensões.
- [ ] Visual de entradas, rochas e transição superfície/subsolo em gameplay real.
- [ ] Passagem de múltiplos servos por gargalos, navegação e oito direções em APK.
- [ ] Comparação de desempenho Android (geração da textura RGB24, memória, FPS, tempo de carga).
- [ ] Capturas do jogo com efeitos, iluminação e UI reais.
- [ ] Qualidade artística final dos biomas: a matriz e o material procedural são **primeira versão funcional**.
- [x] Deslocamento dos quatro spawns mais próximos de paredes para distância de colisão de pelo menos 1,0 unidade.
- [x] Diferença de relevo entre parede e piso, estratos minerais discretos, iluminação vinda do alto/esquerda.
- [x] Luz de entrada da superfície com diminuição gradual e atmosferas restritas ao piso navegável (não atravessam rochas).
- [x] Checagem pelo Editor da leitura real do JSON, dos nove biomas e das folgas de colisão.
- [ ] Revisão em dispositivo do contraste, FPS, memória e iluminação em movimento.

**Política de recuperação:** as outras regiões continuam recuperáveis na branch `main` / histórico do Git. Nenhum PNG, rig, sprite, save ou componente do santuário foi removido. O trabalho só pode ser mesclado à main após revisão do PR e testes Android.

## Diário de execução — 14/11/2026

**Baseline do novo mapa:** o [Android Preview #119](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/runs/43539188110) terminou **com sucesso**, incluindo o Unity Builder e a publicação do APK direto. Artifact `necromancer-unity.apk`: **29.612.388 bytes** (~29,6 MB), armazenado no GitHub Actions. O sucesso de build não prova jogabilidade, FPS ou uso de memória no aparelho.

**Continuidade — ambientação e passagem segura:**
- Quatro pontos de spawn de inimigos foram deslocados localmente para garantir pelo menos **1,0 unidade** de espaço diante de paredes: rato nos Jardins de Fungos, wisp do Ossuário, slime do Lago e Grave Pup do Lago. Não foram movidos os biomas, recursos, pontos principais nem os muros.
- O piso procedural agora usa faixas minerais suaves e pequenas variações de sedimento. As bordas das paredes distinguem arestas iluminadas de faces voltadas para o interior.
- A entrada tem uma iluminação mais quente que **enfraquece ao avançar para o subsolo**. A névoa colorida e a abertura de luz foram recortadas pela matriz de colisão, evitando que luz de bioma apareça dentro de rocha sólida.
- Os novos testes Python protegem o espaço de surgimento; o `NecroRegionEditorCheck.Verify()` testa a importação real do JSON no Unity, colisor de parede, entradas, boss e spawns.
- Alterações mantidas em `feature/bonehollow-caverna-unificada`; refúgio, arte de criaturas, UI de inventário e salvamentos de atributos preservados.
- **O que permanece pendente:** teste do APK em aparelho, circulação de servos em túneis curvos, performance e uma etapa posterior de ilustrações finais de materiais separados por bioma.
- **Build de validação desta revisão:** Android Preview [#120](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/runs/43594586540), solicitado no commit `5e288328118f5a1b68f39384df4653c3c9bb404c` após alterações de código e testes. Estado na abertura do registro: **em andamento**, com verificação inicial concluída e Unity Builder pendente. Não confundir com o APK #119, anterior ao polimento. O disparador auxiliar foi excluído; workflow Android e Windows voltaram a ser manuais.

### Próximos marcos de qualidade

1. **QA físico da entrada:** testar a personagem andando pelas primeiras bifurcações com 0, 2 e 6 servos; observar alinhamento de sombra e oclusão.
2. **Teste de navegação:** alcançar Cristais, Poço, Lago, Rotboar e selo sem teletransporte involuntário, verificar respawn fora de paredes, coleta e minimapa.
3. **Revisão do solo e paredes:** comparar contraste dos nove biomas em capturas reais e só então trocar materiais procedurais por sprites de produção aprovados.
4. **Otimização Android:** medir tempo de carregamento, memória de pico, estabilidade de 60 FPS, tamanho real do APK e política de compactação.
5. **Aprovação:** somente depois desses testes, preparar a mesclagem do PR #2 à `main`.

## Próximas melhorias visuais

Aumentar a variedade de piso e face de parede, colocar transições naturais de umidade e minerais, sombra de contato das rochas, pontos de entrada de luz da superfície e texturas modulares específicas por bioma. Usar a planta para avaliar se os túneis parecem uma caverna orgânica **na tela do celular**. Não introduzir obstáculos invisíveis: qualquer mudança no piso sólido deve atualizar a máscara e os testes.

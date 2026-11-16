# Bonehollow — avaliação visual e kit modular da entrada (vertical slice v1)

**Data:** 16/11/2026. **Branch:** `feature/bonehollow-caverna-unificada`.  
**Limite de escopo:** somente a boca natural da caverna e sua transição inicial. **Não** foi reformado o restante dos nove biomas nem a câmara segura. Esta é a primeira implementação editável, não a aprovação de arte final.

## 1. Estudos visuais encontrados e avaliados

| Fonte do próprio projeto | Avaliação | Decisão |
| --- | --- | --- |
| `Resources/Backgrounds/entrada_da_caverna_bonehollow.png` | Fundo violeta com ruído sutil, pouco relevo, contraste insuficiente para diferenciar faces de rocha e cavidades | **Preservar como legado/fallback**; não usar como referência principal da entrada final |
| `Resources/SanctuaryProps/entrance_arch.png` | Arco ornamentado bem ilustrado, com pedra clara, folhagens e simbolismo arquitetônico | **Manter apenas no refúgio**; o portal artificial não combina com a boca de uma caverna natural |
| `Resources/Scenery/rock_cluster.png` | Silhueta geométrica escura e legível, com facetas simples, mas sem a textura/volume desejados para um mapa extenso | Utilizar **somente como referência de silhueta**, sem repetir o mesmo sprite em massa |
| `NecroCaveNetwork.Ground` | Piso e parede compartilham a colisão verdadeira; biomas têm cores próprias, mas o material ainda é esquemático | Preservar como **base estável**, sobrepor materiais locais sem mexer na máscara |
| `NecroCaveZones` | Iluminação suave com recorte por células, já diferencia superfície e subsolo | Preservar e aumentar detalhes locais com cautela para não lavar as silhuetas |

Não foram identificadas no repositório pranchas de estudos recentes contendo alternativas finais de rochas/ossadas para a entrada. Não seria correto afirmar que existe um conceito visual final aprovado.

## 2. Bíblia visual da entrada, versão de produção inicial

- **Perspectiva:** topo inclinado / 3⁄4 sutil. Faces de pedra com destaque de borda e base sombreada; não introduzir arquitetura grandiosa artificial.
- **Piso de fundo:** cinza-violeta suave, próximo ao `#544D5B` já usado na superfície; fissuras de baixa saturação e sedimentos aquecidos pela luz externa.
- **Faces rochosas:** bases escuras (`#24202F` aproximado), planos intermediários cinza-ameixa e realces cinza-rosados. Não usar preto puro em grandes manchas.
- **Ossos:** marfim envelhecido, menos saturado que as interações/ícones. Devem ser um sinal de ameaça, não camuflar itens recolhíveis.
- **Luz principal:** mais quente e ligeiramente vinda do quadrante noroeste. Perda gradual de luminosidade em direção a Ossadas; não usar holofotes que tapem a borda dos caminhos.
- **Contraste:** centro dos corredores limpo; detalhes deslocados para bordas e câmaras mais amplas. Necromante, rato, Grave Pup e texto da UI continuam acima do fundo.
- **Profundidade:** `Ground` (-50), névoa (-49), decalques de solo (-47), relevos de parede (-45), detritos de superfície (-4), criaturas nas camadas atuais. As peças **não têm Collider nem Rigidbody**.
- **Escala:** cada módulo é uma `Sprite` RGBA 96×96 de origem procedural, com pivot no centro, `100 pixelsPerUnit` nativos e escala final dada por `width/height` do JSON; não são 22 texturas diferentes.
- **Sombra:** sombras discretas são incorporadas às peças de rocha e ossos, sob a base pintada. Não reposicionar sombras independentemente da origem da peça.

## 3. Kit implementado e locais editáveis

A lista de posições está em `unity/Assets/Resources/Data/bonehollow_entrance_modules.json`. Todos os itens são independentes, com `id`, `kind`, `surface`, `x/y`, `width/height`, `rotation` e `alpha`, e podem ser desligados ou remanejados sem alterar o `levels.json`.

| Tipo | Função | Superfície | Comportamento |
| --- | --- | --- | --- |
| `floor_silt` | Sedimentos aquecidos e terra mineral | Piso | Mancha translúcida, sem bloqueio |
| `floor_fracture` | Fissuras ramificadas | Piso | Traço escuro com realce lateral |
| `floor_grit` | Gravilha e partículas | Piso | Baixa densidade visual |
| `wall_facet` | Faces volumétricas | Rocha sólida | Facetas opacas de até 1 célula |
| `wall_rim` | Bordas e contornos | Rocha sólida | Limiar entre planos da parede |
| `rock_chip` | Pequenas pedras soltas | Piso | Apenas detrito baixo, atravessável |
| `rock_slate` | Laje maior | Rocha sólida | Elemento alto somente em superfície bloqueada |
| `bone_shards` | Fragmentos de osso | Piso | Detalhe de ameaça, sem colisão |
| `bone_rib` | Costelas antigas | Piso | Ossada baixa, atravessável |

**Montagem:** `NecroEntranceKit.Populate` é chamada em `NecroPrototype.LoadStage` apenas na primeira criação de Bonehollow. Todos os elementos ficam sob `Bonehollow / Entrance modular kit v1` como GameObjects individuais. Os nove sprites são gerados sob demanda e ficam em cache. O JSON é consultado pelo carregamento de `Resources/Data`. A entrada é totalmente removível sem impacto na câmara segura.

**Posições:** 22 peças na área inicial, entre a abertura de superfície e a primeira transição. O verificador rejeita elementos de parede posicionados no piso, elementos de solo colocados dentro de rochas, itens próximos da personagem ao chegar e de pontos de combate/recursos. Os artefatos `levels.json`, `caveRows`, inimigos, sprites de personagens, salvamento, pacote de assinatura e controles **não** foram alterados.

**Planta exata:** [BONEHOLLOW_ENTRADA_KIT_PLANTA.svg](BONEHOLLOW_ENTRADA_KIT_PLANTA.svg) mostra a geometria efetiva e a posição de cada módulo, dos primeiros encontros, recursos e ponto inicial. É um **esquema técnico**, não screenshot do Android.

## 4. Estratégia de substituição futura

Este kit é procedural e **provisório**. Não é uma coleção de PNGs pintados à mão nem deve ser confundido com um visual de produção final.

Para a próxima iteração, cada `kind` poderá ser substituído por `Resources/Environment/Bonehollow/{Ground,Walls,Props}` mantendo exatamente os mesmos identificadores, dimensões, coordenadas e regras de superfície. Priorizar pedra natural, erosão orgânica, sombras de contato e evitar materiais com grau de detalhe incompatível com os personagens.

O arco ornamental já usado no santuário não será removido, duplicado nem deslocado.

## 5. Validação e critérios de aceite

- [x] As três referências reais de arte foram inspecionadas e classificadas.
- [x] Nove tipos de módulos independentes construídos e 22 instâncias cadastradas.
- [x] Dados e posicionamento validados em comparação com a geometria de Bonehollow.
- [x] Integração isolada na cena de carregamento; `levels.json` e colisões não foram modificados.
- [x] Teste estático de estrutura e integridade do kit no GitHub Actions.
- [ ] Unity Editor carregar o layout e instanciar todos os sprites, validado no Android build desta revisão.
- [ ] Capturar screenshots reais no celular perto do início, da primeira curva e dos primeiros ratos/Grave Pups.
- [ ] Conferir FPS, memória, escala, alinhamento de sombras, silhuetas e legibilidade do minimapa.
- [ ] Revisão artística/aprovação da vertical slice por screenshot e vídeo.
- [ ] Expandir os materiais modulados aos demais setores **somente depois do GATE 1**.

**Próximo passo exato:** compilar Android com `NecroEntranceEditorCheck`, confirmar a publicação do APK e testar a primeira área no celular. A partir das capturas e do vídeo do aparelho, ajustar os materiais do kit sem mexer na geometria ou nos personagens.

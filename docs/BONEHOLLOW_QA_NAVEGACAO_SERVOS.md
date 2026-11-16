# Bonehollow — protocolo de QA de túneis e servos

**Escopo:** caverna unificada v2 (`levels.json`, 160 × 104). Aplicável a builds Android da branch `feature/bonehollow-caverna-unificada`; não substitui os testes de compilação. Nenhum item abaixo equivale a teste em dispositivo até que se registre resultado e evidência.

## Antes de testar

- Confirmar o número exato da build e que a assinatura permite **atualizar o APK sobre o aplicativo já instalado**, preservando `necro-save-v1.json`. Evitar desinstalar e perder o save.
- Testar pelo menos uma vez com perfil que existia nos três mapas antigos, verificando nível, habilidades, equipamento, cofre e criaturas; o histórico de exploração anterior deve ser reiniciado na nova caverna.
- Dispositivo: preencher fabricante/modelo, Android, RAM, resolução, taxa de quadros observada, data e número da compilação.
- Do refúgio entrar na caverna pela passagem habitual. O espaço do refúgio e a seleção de personagens devem continuar funcionando.

## Matriz de rotas e comportamento

| Caso | Procedimento | Critério de aprovação |
| --- | --- | --- |
| N0 | Sem servos, contornar o primeiro corredor sinuoso da entrada e voltar ao refúgio | Parede sólida e minimapa coerente; nenhum atravessamento ou travamento |
| N1 | Dois servos, descer de Entrada até Salões das Ossadas passando pelo primeiro desvio | Ambos acompanham pelas **aberturas reais**, sem cortar rochas |
| N2 | Seis servos, atravessar bifurcações Ossadas → Fungos → Poço; alternar direção e parar duas vezes | Seguimento recupera formação, sem aglomeração permanente |
| N3 | Seis servos, avançar do Poço → Cristais → Ossuário, escolhendo túneis secundários | Servos não desaparecem nem se teleportam para atravessar paredes |
| N4 | Dois ou seis servos, Lago Subterrâneo → Covil do Rotboar → Santuário do Abismo | Criaturas alcançam os três biomas, sem ficarem presas fora da tela |
| N5 | Parar perto de uma parede que separe duas passagens; reunir servos do outro lado do túnel | Servo contorna a rocha por uma rota livre, sem atravessar o obstáculo |
| N6 | Combater perto de curva e de gargalo; derrotar, coletar, reerguer um inimigo e recuar | IA e animações de combate permanecem corretas; servos evitam obstáculo |
| N7 | Fechar e reabrir aplicativo depois de explorar parte do mapa, coletar recursos e armazenar um servo | Posição válida, fog-of-war preservado, criaturas e inventário intactos |
| N8 | Percorrer todo o mapa usando minimapa aberto/fechado e observando névoa atrás das paredes | Nenhum corredor oculto é revelado através de rochas sólidas |
| N9 | Caminhar repetidamente em diagonal em curva estreita com um servo grande e outro pequeno | Sem quinas atravessadas, tremores excessivos ou servo preso indefinidamente |
| N10 | Com seis servos, girar 180° no meio de uma bifurcação e voltar pelo corredor de origem | Servos abandonam o destino antigo, reorganizam a fila e não atravessam rocha |
| N11 | Parar com seis servos diante de uma abertura estreita; dar pequenos passos, parar e acelerar novamente | Sem empilhamento permanente no centro da necromante; intervalos de fila adequados para criaturas grandes e pequenas |
| N12 | Sair para o refúgio e retornar à mesma caverna com servos ativos | Nenhuma trilha antiga é reutilizada e o salvamento dos servos continua íntegro |

## Critérios de desempenho

Medir, se possível, uso de memória e FPS **durante** as rotas N2, N3 e N4 (não apenas parado no refúgio). Registrar se há queda de FPS no primeiro carregamento de Bonehollow, ao abrir o minimapa e durante reanimações. Verificar se o consumo de memória aumenta após entrar/sair do mapa três vezes seguidas. A textura procedural de terreno usa RGB24 em 2560 × 1664 pixels; comparar o uso real de RAM do aplicativo com esse requisito antes de reduzir qualidade visual.

Para quaisquer problemas, registrar: build, bioma, posição aproximada do minimapa, número e tipo de servos, captura de tela ou vídeo e uma sequência curta de reprodução. Não alterar a geometria para mascarar bugs da IA sem primeiro confirmar as colisões reais.

## Portões de aceitação

1. **CI de estrutura** e verificações do Unity Editor aprovadas.
2. APK Android assinado e publicado pela build indicada.
3. Casos N0 a N9 experimentados em aparelho real, com notas de resultado.
4. Servos devem contornar paredes; teletransporte através de rochas não é solução.
5. Recursos, Rotboar, selo, salvamento e refúgio continuam acessíveis.
6. A arte final dos biomas só será substituída após observar o protótipo em dispositivo; o objetivo é manter um cenário harmônico e modular.

**Verificação geométrica adicional (14/11/2026):** comparação independente da matriz `caveRows` com rotas cardinais em grade de 1 unidade, verificando segmentos pelo mesmo critério circular contra paredes: **18/18 rotas válidas** (nove biomas × dois raios, 0,28 e 0,52 unidade). Nenhuma passagem observada através de células sólidas. É um teste matemático da geometria, **não** um teste de IA em tempo real ou no Android.

**Histórico de builds:** Android [#120](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/runs/43594586540) aprovado (iluminação/geração), Android [#121](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/runs/43666214958) aprovado (primeira versão da navegação). Em 15/11/2026 foi integrada a fila por trilha e o recálculo de rotas; essa versão é posterior ao APK #121. Os testes N0–N12 permanecem **pendentes em dispositivo real**.

# BONEHOLLOW / MAPA 01 — Auditoria de baseline e planta técnica inicial

**Data:** 13/11/2026  
**Etapa:** 00 — auditoria (parcial; medições e QA físico pendentes); insumo preliminar da 02.  
**Repositório:** `Takeo-Ishikawa/necromancer-rise-of-the-fallen`  
**Branch de proteção:** `feature/bonehollow-baseline-mapa-01`  
**Commit-base (main, antes da auditoria):** [`9aa2ac673c781cd76825f8ec9c3a5a5243069021`](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/commit/9aa2ac673c781cd76825f8ec9c3a5a5243069021) — inclusão do plano em 13/11/2026.  
**Engine:** Unity `2022.3.62f2`, conforme `unity/ProjectSettings/ProjectVersion.txt`.  
**Planta ilustrativa:** [BONEHOLLOW_MAPA_01_PLANTA_AUDITORIA.svg](BONEHOLLOW_MAPA_01_PLANTA_AUDITORIA.svg).

## 1. Fonte de verdade e inventário confirmado

- `unity/Assets/Resources/Data/levels.json` define a fase `bonehollow`: **56 × 34 unidades**, entrada (7,8; 29,3), selo (49,1; 5,8), Rotboar (45,4; 10,4).
- Coordenadas do JSON têm origem **superior esquerda (Y crescente para baixo)**. `NecroLevels.At(level,x,y)` converte para Unity como `(x, level.height-y)`. Ex.: entrada Unity `(7.8,4.7)`; selo Unity `(49.1,28.2)`; Rotboar Unity `(45.4,23.6)`.
- O JSON contém **80 obstáculos** de colisão circular, separados em **54 de contorno** (18 acima, 18 abaixo, 9 à esquerda, 9 à direita) e **26 internos**; o JSON continua como autoridade da geometria.
- São **12 spawns de inimigos comuns**: 4 Grave Pups, 3 ratos, 2 morcegos, 2 slimes e 1 aranha. O Rotboar é instanciado separadamente em `NecroPrototype.LoadStage`.
- Há **12 recursos do tipo `cristal`** previstos no JSON. A presença exata em saves já iniciados depende do snapshot de recursos salvos.
- A câmara segura e as outras regiões são independentes; **nenhum arquivo de gameplay ou asset foi alterado nesta auditoria**.

## 2. Coordenadas dos encontros e recursos (origem JSON)

### Spawns de inimigos comuns

| # | Tipo | Coordenada X;Y |
|---:|---|---|
| 1 | `grave_pup` | (17; 28,1) |
| 2 | `grave_pup` | (25,5; 25) |
| 3 | `grave_pup` | (38,5; 21,2) |
| 4 | `grave_pup` | (48; 14,5) |
| 5 | `rat` | (12,7; 20,8) |
| 6 | `rat` | (17,8; 16,6) |
| 7 | `rat` | (30; 8) |
| 8 | `bat` | (17; 10,8) |
| 9 | `bat` | (36,4; 9,7) |
| 10 | `slime` | (32; 27) |
| 11 | `slime` | (47; 27) |
| 12 | `spider` | (49,3; 20,1) |

O **Rotboar** não faz parte dos 12 spawns comuns; sua coordenada é (45,4; 10,4).

### Pontos de cristal

| # | Coordenada X;Y |
|---:|---|
| 1 | (9,4; 26,6) |
| 2 | (13,6; 30,2) |
| 3 | (16,5; 21,6) |
| 4 | (24,8; 28,8) |
| 5 | (27; 18) |
| 6 | (35,1; 26,6) |
| 7 | (39; 29,2) |
| 8 | (47,1; 23,4) |
| 9 | (15,6; 8,3) |
| 10 | (26,5; 6,7) |
| 11 | (37; 5,7) |
| 12 | (51,4; 12,7) |

## 3. Obstáculos do cenário

A lista integral de **80 círculos (X, Y, raio)** está no `levels.json` e está representada na planta SVG. Os obstáculos do contorno ficam majoritariamente fora do retângulo jogável para limitar as bordas. Para orientar desenho de passagens, seguem **todos os 26 círculos internos**:

| # | Centro X;Y | Raio |
|---:|---|---:|
| 1 | (11; 24,4) | 1,45 |
| 2 | (15,7; 25) | 1,85 |
| 3 | (18,1; 25) | 1,55 |
| 4 | (22,1; 17,5) | 2,25 |
| 5 | (22,9; 20,2) | 2,3 |
| 6 | (24,1; 22,25) | 1,65 |
| 7 | (28,3; 22,6) | 1,55 |
| 8 | (28; 30) | 1,75 |
| 9 | (33,8; 16,2) | 2,1 |
| 10 | (34,8; 18,85) | 2,1 |
| 11 | (35,3; 21,3) | 1,45 |
| 12 | (42,9; 29,9) | 2,45 |
| 13 | (44,5; 31,65) | 1,72 |
| 14 | (42,8; 18,65) | 2 |
| 15 | (43,3; 20,65) | 1,55 |
| 16 | (11,7; 11) | 2,08 |
| 17 | (13,9; 13,15) | 1,8 |
| 18 | (23,4; 10,05) | 2,1 |
| 19 | (25,9; 12,35) | 1,95 |
| 20 | (28,3; 13,65) | 1,7 |
| 21 | (40,7; 7,45) | 1,23 |
| 22 | (31,3; 5,1) | 1,1 |
| 23 | (8; 16,8) | 1,2 |
| 24 | (47,9; 31,1) | 1 |
| 25 | (18,7; 5,2) | 1,1 |
| 26 | (39,8; 25,2) | 1,4 |

**Atenção:** o piso pode parecer aberto onde existem círculos de bloqueio. Os sprites de rocha atualmente usam `rock.r * 2` de tamanho visual; qualquer futuro muro ou borda desenhada precisa coincidir com a passagem real. Não mover obstáculos nesta etapa.

## 4. Zonas visuais existentes

`NecroCaveZones.Populate` cria manchas translúcidas elípticas **sem colisão** nos centros a seguir. As dimensões são as escalas visuais usadas pelo código, não limites exclusivos de navegação.

| Zona | Centro X;Y | Escala largura × altura |
|---|---|---|
| Entrada | (8; 28,9) | 15 × 11 |
| Ossadas | (18; 24) | 20 × 13 |
| Fungos | (35; 27) | 22 × 14 |
| Poço central | (28; 17) | 19 × 14 |
| Cristais | (17; 10) | 20 × 15 |
| Covil | (46; 12) | 18 × 14 |
| Selo | (49; 5,6) | 15 × 10 |

Marcos decorativos de `NecroCaveZones`: lanterna (5,2; 28,6); ossos (19,1; 23,1) e (20; 28,7); fungos (32,5; 30,8) e (36,8; 24,9); cristais (13,6; 7,5) e (20,4; 9,5); runa (46,3; 7,4); ossos (42,1; 13,5).

**Observação de navegação:** o centro da mancha de **ossadas (18; 24)** está dentro do obstáculo (18,1; 25) (raio 1,55). Portanto, um centro visual não pode ser usado automaticamente como waypoint de travessia.

## 5. Pipeline atual e limites de compatibilidade

| Elemento | Responsável | Comportamento verificado no código |
|---|---|---|
| Fundo | `NecroArt.Environment` + `NecroPrototype.FitGround` | Busca `Backgrounds/entrada_da_caverna_bonehollow` via `Resources.Load<Sprite>` ou `Resources.Load<Texture2D>`; piso repetido por `SpriteDrawMode.Tiled`, ordem -50 |
| Zonas | `NecroCaveZones.Populate` | 7 manchas e 9 marcos visuais; manchas ordem -49; marcos -5; sem colisão |
| Props | `NecroScenery.Populate` | Geração procedural determinística, até 64 props no `bonehollow`; `Scenery/<kind>` usa sprite importado ou desenho fallback; ordem -4; não altera save |
| Rochas | `NecroPrototype.LoadStage` + `NecroArt.Stone` | Renderiza 80 rochas com `Scenery/rock_cluster` (fallback procedural); ordem -3 |
| Colisão | `NecroGameplay.Blocked` | Fronteira do mapa e círculos de `level.obstacles`, usando coordenadas convertidas em Y; independente da arte |
| Minimapa | `NecroMiniMap` | Marca obstáculos circulares e exploração revelada; não tem malha explícita de corredores; validação visual e persistência em aparelho pendentes |
| Salvamento | `NecroPrototype` / `NecroSaveData` | Snapshot de recursos, estado das regiões e descoberta; intocados |
| Android | `.github/workflows/unity-android.yml` | `workflow_dispatch`, compilação sob demanda, checagem estática antes do build e registro do tamanho do APK em bytes |

**Fallback obrigatório:** manter o PNG de fundo atual e os caminhos antigos de carregamento. Não migrar render pipeline, geometrias ou sprites dos personagens durante esta fase.

## 6. Análise geométrica exploratória (não equivale a teste de gameplay)

Foi examinada uma grade de amostragem de **0,4 unidade**, com bloqueio dos círculos do JSON inflados por um raio de segurança, verificando também os movimentos diagonais sem atravessar cantos. Resultado:

- Para raio de segurança **0,40 unidade**, há uma sequência de células livres entre a entrada e o selo.
- Para raio **0,65 unidade**, também há uma sequência de células livres entre entrada e selo.
- Os centros de entrada, selo, Rotboar, todos os 12 spawns comuns e os 12 recursos não estão dentro de círculos do JSON. Isso **não garante** acesso com vários servos, raio específico de cada ator, ações de combate, coleta ou pathfinding real.
- O traçado branco pontilhado do SVG é apenas um **caminho computacional ilustrativo** para raio 0,65, não rota aprovada no jogo.

**Recorte preliminar sugerido para a vertical slice:** retângulo **X = 4–22, Y = 19–33** no JSON. Ele engloba a chegada, a primeira progressão para ossadas, três cristais, o primeiro Grave Pup, o primeiro rato e as rochas internas próximas. Na planta está assinalado com linha amarela. **Não aplicar o recorte à cena ainda:** confirmar rotas e confrontos na ETAPA 02, com margem para sprites e servos.

## 7. Riscos e dependências

1. **P0 / ilusão de passagem:** parede ilustrada, rocha e colisão circular poderão não coincidir. Manter colisão por JSON e revisar transições antes de substituir arte.
2. **P0 / servos e Rotboar:** o trajeto estático não comprova que vários servos atravessam bifurcações nem que o Rotboar consiga executar a investida. Necessário teste em execução real.
3. **P0 / minimapa:** minimapa usa círculos do JSON; não representa corredores ou paredes ilustradas que ainda serão produzidas. Evitar inserir paredes puramente visuais enganosas.
4. **P1 / desempenho Android:** fundo repetido, 80 objetos de rocha, até 64 props e transparências de zonas já geram carga gráfica; registrar FPS, memória e tempo de entrada antes da produção pesada.
5. **P1 / carregamento:** `Sprite` e `Texture2D` seguem caminhos de importação distintos; otimizações em `Resources` ou `TextureImporter` devem manter os dois casos compatíveis.
6. **P1 / saves:** recursos retirados de snapshot não devem reaparecer indevidamente após mexer no visual; não alterar serialização.
7. **P1 / referências do plano:** screenshots da entrada, ossadas, fungos, poço, cristais, covil, selo e refúgio ainda não foram fornecidas / coletadas nesta sessão.

## 8. Baseline ainda pendente de equipamento / CI

| Indicador | Estado real |
|---|---|
| APK de referência / número de build | Não verificado nesta sessão |
| Tamanho real do APK (bytes) | Não medido; consultar o resumo de um workflow já executado, sem acionar novo build automaticamente |
| Aparelho de referência / resolução | Não registrado |
| FPS mínimo, médio e picos | Não medidos |
| Memória / tempo de entrada no mapa | Não medidos |
| Screenshots / gravação em execução | Não produzidos |
| Teste Unity Editor / `tests/check_unity_project.py` | Não executado localmente; nenhum código de runtime foi modificado |
| Build Android / Windows nesta etapa | **Não executada** |

**Situação de aceite da ETAPA 00:** **EM ANDAMENTO**, com inventário, planta, caminho preliminar e riscos documentados. **Não marcar como concluída** enquanto os itens de evidência e validação pendentes não forem avaliados.

## 9. Recuperação e próximo passo

A referência íntegra para recuperação é o commit-base **`9aa2ac6`** na branch `main`. Os novos artefatos documentais ficam em `feature/bonehollow-baseline-mapa-01` e podem ser descartados sem modificar Assets, scripts, JSON, saves ou workflows. Não substituir `Backgrounds/entrada_da_caverna_bonehollow.png`.

**Próxima ação exata:** obter capturas reais e métricas do APK atual no aparelho Android; em paralelo, consolidar a ETAPA 01 (paleta/perspectiva/pivôs) e a ETAPA 02 (verificação de rotas com servos e recorte da vertical slice). Somente depois criar novas artes runtime para o primeiro trecho da entrada.

# Necromancer 0.9.12 — Balanceamento inicial da dificuldade

## Objetivo

Estabelecer uma curva de desafio legível após a introdução de vida e derrota na 0.9.11. A primeira região deve permitir aprender o combate, invocar o primeiro servo e entender o recuo, enquanto regiões posteriores continuam ameaçadoras. O ajuste foi feito apenas por parâmetros de ameaça e ritmo; os danos infligidos pela necromante e pelos servos, os pontos de vida dos monstros, os custos de ânima, os equipamentos e os ganhos de XP não foram alterados.

## Progressão entre regiões

| Região | Dano recebido da IA (multiplicador) | Intervalo entre tentativas de respawn | Limite de inimigos vivos para novos respawns |
|---|---:|---:|---:|
| A Caverna Esquecida | 0,82× | 19 s | 3 |
| Túneis Inferiores | 1,00× | 15 s | 4 |
| Necrópole das Cinzas | 1,08× | 12 s | 5 |
| Pântano dos Sussurros | 1,14× | 11 s | 6 |

O multiplicador incide apenas sobre o dano recebido pela necromante, após os valores básicos dos monstros. Não altera a quantidade de vida dos inimigos nem o dano que eles causam a servos. A proteção de 1,1 segundo após cada golpe continua igual em todas as regiões.

O limite de inimigos vivos é **um teto para acrescentar novos respawns**, não um teto de monstros já presentes ou uma instrução para apagar inimigos da fase. As regiões posteriores podem começar com oito a onze monstros distribuídos no mapa; os respawns só retomam quando o número de sobreviventes cai abaixo do teto correspondente. Um cadáver não conta como inimigo vivo. Os monstros são criados respeitando rochas, entrada, portal, separação entre inimigos e distância dos servos.

Nas regiões iniciais, os novos inimigos surgem mais longe da necromante: entre 4,3 e 5,3 unidades na Caverna Esquecida; entre 3,9 e 5,0 nos Túneis; entre 3,6 e 4,7 nas duas regiões mais avançadas. Uma tentativa sem posição válida é descartada, sem criar um monstro sobre o personagem. O primeiro intervalo de uma região recém-criada usa os valores novos; intervalos remanescentes de saves antigos seguem válidos.

## Reação aos ataques e tutoriais de combate

- **Grave Pups hostis da primeira caverna:** o raio para iniciar perseguição diminuiu de 4,1 para 3,65 unidades; a preparação do salto subiu de 0,42 s para 0,56 s; a recuperação após o ataque subiu de 0,95 s para 1,25 s. Esses valores dão uma janela maior para reagir ao aviso de salto que já existe visualmente. Grave Pups aliados continuam com seu ritmo original.
- **Rotboar hostil da primeira caverna:** preparação da investida aumentada de 0,8 para 1,0 segundo, mantendo velocidade e dano básico. Rotboars aliados mantêm 0,8 segundo.
- **Monstros comuns:** ataque com recarga de 1,40 s na primeira região, 1,20 s na segunda, 1,15 s na terceira e 1,11 s na quarta. Chefes permanecem com recarga de 1,55 s. Os demais valores de animação e ataque foram mantidos.

## Importância dos servos

Monstros hostis passam a considerar companheiros ativos próximos como alvos um pouco mais atraentes. A preferência depende de distância real: aplica-se um peso à distância quadrática até o servo (0,62 na primeira região, 0,73 na segunda, 0,82 na terceira e 0,90 na quarta), competindo com a distância até a necromante. Assim, colocar o exército entre o jogador e os inimigos pode desviar parte da pressão, mas não torna a protagonista invulnerável. Monstros ainda atacam a necromante quando ela é o alvo mais adequado. Um servo morto ou sem corpo válido é ignorado. O bônus Ordem dos Mortos e as posturas Caça/Defesa continuam sem modificações.

## Recuperação após confrontos

Coletar um recurso da caverna passa a recuperar **12% da vida máxima da protagonista, com mínimo de 9 HP**, limitado ao HP que estiver faltando; antes recuperava 9 HP fixos. O mesmo recurso continua concedendo 2 fragmentos e até 12 de ânima. A mensagem exibida informa a vida e a ânima realmente restauradas, inclusive quando os medidores já estão cheios. A cura automática não ocorre durante ataques, apenas ao coletar recursos existentes.

Ao cair abaixo de 25% de vida pela primeira vez em um golpe, a interface avisa para recuar e procurar recursos. Não há aviso repetido em cada quadro: a mensagem é exibida apenas no momento em que a vida cruza o limite. O fluxo de derrota continua sendo o da 0.9.11 (65% de HP ao retornar ao refúgio e perda máxima de 2 fragmentos).

## Compatibilidade e validação

Os valores são calculados pela etapa do mapa no código de combate; não são salvos em campos novos. O arquivo `necro-save-v1.json` permanece com a versão 1 e a assinatura/identificador Android também não mudam. Ambos os workflows Unity continuam separados, exclusivamente manuais. A versão visível nas builds passa a 0.9.12.

**Testes de jogabilidade recomendados para a próxima build Windows:**

1. Entre na Caverna Esquecida sem servos. Confirme que os Grave Pups não saltam de longe e que o aviso da preparação permite se esquivar ou recuar. Observe também a investida mais anunciada do Rotboar.
2. Ressuscite um Grave Pup, posicione-o entre a necromante e outro inimigo, depois observe se parte dos ataques passa a ser dirigida ao servo. Os inimigos ainda devem poder atingir a protagonista.
3. Aguarde 19 segundos na primeira região: o respawn não deve surgir sobre a necromante nem aumentar a população enquanto houver pelo menos três hostis vivos.
4. Cause danos suficientes para entrar abaixo de 25% de HP e confira o aviso de vida crítica. Recolha um recurso e observe a recuperação equivalente a 12% do HP máximo (ou a quantidade faltante, se menor).
5. Avance aos Túneis, à Necrópole e ao Pântano. Compare a pressão dos ataques e a frequência de inimigos sem mudar de nível ou equipamentos entre as comparações, quando possível.
6. Mantenha o personagem parado junto de diferentes inimigos para verificar a proteção de 1,1 segundo entre acertos e a tela de derrota. Confirme que o retorno não apaga níveis, equipamentos nem o exército.
7. Feche e reabra a partida e verifique que o progresso e a população do mundo continuam sendo restaurados, sem criar monstros adicionais por um reset de respawn.
8. Teste no Android posteriormente, especialmente a leitura dos avisos e os comandos de sobrevivência.

Os testes automáticos verificam estrutura, limites e integração, mas uma build Unity e os testes reais de combate ainda são necessários para confirmar o equilíbrio.

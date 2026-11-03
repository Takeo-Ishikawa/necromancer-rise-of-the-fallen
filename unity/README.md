# Necromancer — Unity 2D / Android

**Unity Editor:** 2022.3.62f2 LTS · **Build:** Android APK · **Scripting:** IL2CPP · **CPU:** ARMv7 + ARM64.

O projeto Unity esta em `unity/`. O jogo Android anterior, feito em Java/Canvas, continua preservado em `app/` como referencia para testes de regressao.

## Conteudo recuperado nesta migracao

O codigo foi dividido em quatro modulos:

- `NecroPrototype.cs`: ciclo de vida da partida, camera e estado persistente das regioes.
- `NecroGameplay.cs`: movimento, colisao, esquiva de obstaculos, combate, necromancia, nivelamento, equipamentos, fragmentos e evolucao de servos.
- `NecroUI.cs`: controles por toque, interface de status, menu pausado de evolucao e portais.
- `NecroArt.cs`: sprites de substituicao originais, incluindo a silhueta da bruxa de cabelos prateados, chapeu amplo, manto e lanterna no cajado.

Os dados das regioes ficam em `Assets/Resources/Data/levels.json`. Sao **quatro mapas**: Caverna Esquecida, Tuneis Inferiores, Necropole das Cinzas e Pantano dos Sussurros. Os tuneis foram mantidos para nao descartar o trabalho realizado anteriormente.

Ao transitar, cada regiao apenas se desativa: **inimigos, corpos, itens e recursos coletados mantem seu estado**. A vida, os equipamentos, a experiencia e os servos permanecem com o jogador.

O jogador recebe XP e fragmentos ao derrotar inimigos; itens aparecem a cada duas vitorias, com foco arcano e manto de graus ate 3. O menu EVOLUIR melhora cada especie ate grau 3. Os chefes devem ser derrotados e vinculados para concluir regioes marcadas por guardioes. Os monstros tem perseguição e ataque automatico, e as criaturas invocadas seguem o heroi.

## Como iniciar

1. No Unity Hub, adicione como projeto a pasta **unity/**.
2. Abra com o Editor **2022.3.62f2** (suporte **Android Build Support**, SDK/NDK e OpenJDK instalados).
3. Ao abrir, `NecroSceneBootstrap` cria `Assets/Scenes/NecromancerPrototype.unity` automaticamente.
4. Abra a cena e execute em **Play** para avaliar no editor.
5. Para exportar, utilize o menu *File > Build Settings > Android* ou execute o workflow do GitHub `Unity Android APK`.

## Controles

No celular, use a direcao virtual do lado esquerdo ou arraste o quadrante inferior esquerdo. À direita estao MAGIA/GOLPE, REVIVER, DEFESA/CAÇA e DISPENSAR. A evolucao das especies fica no botao EVOLUIR. Ao conquistar uma regiao, aproxime-se do selo para liberar a passagem; o retorno e acessado perto da entrada. No computador: WASD, Espaco, E, Q, Tab e Enter.

## Arte e limitacoes atuais

O visual atual foi aproximado ao conceito da protagonista **por meio de um sprite procedural original**, nao pela conversao do sheet original. Ele e funcional, mas ainda nao equivale à arte final de producao. Fundos originais mais detalhados e sprites animados podem ser importados como PNGs transparentes, preservando exatamente os nomes `Sprites/<tipo>` e `Backgrounds/<nome-da-fase>` em `Assets/Resources/`.

**Nao foi implementado salvamento persistente entre sessoes**, reservado para o proximo sistema da Cripta. O teste automatizado em `tests/check_unity_project.py` verifica estrutura e dados, mas nao substitui a compilacao real do Unity ou testes num aparelho fisico.

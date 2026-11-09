# Necromancer 0.9.14 — IA tática, formação defensiva e leitura do combate

## Inimigos

As três famílias de comportamento hostil (inimigos comuns, Grave Pups e Rotboars) usam agora `ClosestPartyMember` para selecionar a necromante ou um servo. O peso de ameaça de servos já estabelecido na 0.9.12 permanece ativo em todos os casos, mas a distância real até o alvo deve estar dentro do raio de aquisição de cada espécie: **4,1 unidades** para comuns, **3,65** para Grave Pups na primeira região (4,1 depois) e **5,5** para Rotboars. Os algoritmos de golpes, preparação, recarga, dano, colisões e alcance permanecem inalterados.

Para evitar alternância de alvos a cada quadro, o monstro conserva o alvo atual até que um candidato ofereça uma pontuação pelo menos **20% melhor** em relação à pontuação do novo candidato (limiar de retenção 1,20×). Um alvo morto, sem corpo ou que tenha deixado o grupo ou o raio de aquisição deixa de ser considerado. O estado de foco existe apenas em memória do ator, sem campos adicionais no save. Um Grave Pup cujo alvo desaparece interrompe o salto preparado. Rotboars sem alvo válido não tentam acessar um alvo nulo e mantêm a posição até reencontrar uma ameaça.

## Posturas dos servos

Na postura **CAÇA**, conserva-se a preferência pelo inimigo mais próximo do próprio servo, respeitando alcance, visibilidade, alvo manual e Ordem dos Mortos. Na postura **DEFESA**, os servos consideram tanto a distância ao próprio servo (55%) quanto a distância até a necromante (45%). Uma ameaça a no máximo **1,9 unidade** da protagonista recebe peso adicional de prioridade. Não há teleporte, ataque fora de alcance ou alteração de dano/vida dos servos.

Em DEFESA, os primeiros quatro lugares da formação procuram flanquear ligeiramente à frente da necromante. As posições continuam sujeitas às verificações de bloqueio e ocupação e, quando não disponíveis, usam os mesmos deslocamentos alternativos e formação em fila. Dentro do refúgio e em CAÇA, mantém-se a formação antiga atrás da protagonista.

## Feedback

Por **1,2 segundo** após um golpe válido, a barra de vida fica mais viva e mostra o HP restante, a perda real de vida e, quando relevante, o dano evitado pelo manto. Não há nova textura, número flutuante, efeito permanente ou alocação de objetos de cena por golpe. As mensagens de vida crítica, imunidade breve após receber dano e modal de derrota permanecem inalteradas.

## Compatibilidade e validação

- Continua o formato de save **v1**, com os mesmos campos; `hostileFocus` não é persistido.
- Versão de Windows e Android: **0.9.14**; versionCode de release Android **22**. O versionCode da preview continua sendo calculado pelo número de execução, sem alterar ID ou assinatura.
- Os workflows Android/Windows continuam independentes e iniciados manualmente.
- `tests/check_unity_project.py` verifica integração e preservação de contratos antigos; `NecroTargetEditorCheck.Verify()` exerce os cálculos C# antes da compilação Unity.

### Roteiro de teste manual

1. Na Caverna Esquecida, aproxime a necromante e um servo de um inimigo comum: observe se ele pode escolher o servo como alvo, sem alternar rapidamente a cada passo.
2. Afaste-se com o alvo além da distância de aquisição. O inimigo não deve perseguir indefinidamente nem tentar atacar um ator removido.
3. Observe Grave Pups e Rotboars antes/depois de um servo ser derrotado, inclusive durante preparações de ataque; nenhum erro de referência nula deve ocorrer.
4. Coloque servos em DEFESA, posicione-os perto da necromante e observe se priorizam inimigos que ameaçam a protagonista. Alterne para CAÇA e confirme que passam a privilegiar os inimigos próximos deles.
5. Passe por obstáculos estreitos, troque de postura e entre no refúgio. A formação deve evitar sobreposição, não atravessar paredes e voltar à distribuição padrão no refúgio.
6. Receba ataques com manto grau 0 e superior: confira o valor de HP, a perda real, o bloqueio do manto e a duração breve da indicação.
7. Feche e reabra o jogo; confirme que servos, postura, equipamentos e vida persistem. Inicie o Windows e Android apenas quando quiser testar cada plataforma.

**Estado:** alterações e regressões integradas em código; compilação Unity e validação em jogo permanecem pendentes.

### Correção de compilação Windows (0.9.14)

A primeira compilação Windows da 0.9.14 falhou em `NecroGameplay.cs` por subtrair a posição `Transform.position` (`Vector3`) da posição de referência (`Vector2`), operação ambígua em C#. A seleção de alvo agora converte explicitamente a posição do alvo anterior para `Vector2` antes da subtração; isso não altera as regras de ameaça nem a jogabilidade. O teste estrutural passou a exigir essa conversão. A build Windows precisa ser reexecutada manualmente para validar a compilação de fato.

# Necromancer 0.9.17 — Encontros prolongados e exploração segura

## Problemas abordados

Antes desta versão, inimigos que perseguiam a necromante podiam ficar dispersos por toda a região, pois perder o alvo não implicava retornar ao local de origem. A seleção também podia detectar personagens através de paredes dentro do raio de percepção. O sistema de respawn evitava o contato direto, mas não verificava se a criatura surgiria diante da câmera.

## IA com território e memória curta

- Todo inimigo hostil guarda a posição onde nasceu (ou a posição recuperada no save).
- A perseguição termina quando a criatura ultrapassa **8 unidades** da origem, e, quando não encontra alvo válido, ela **retorna gradualmente** ao ponto inicial; não atravessa obstáculos e mantém as regras de colisão.
- Se a necromante ou um servo sair do campo visual da criatura, esta tem **0,85 segundo de memória**, suficiente para continuar uma perseguição breve atrás de um obstáculo; depois disso, desiste se não recuperar o contato visual.
- A aquisição de novos alvos exige **linha de visão**. A regra de retenção de foco de 20% da versão 0.9.14 continua ativa. Atributos, danos, cooldowns, invocação e postura dos servos não foram modificados.
- Grave Pups e Rotboars compartilham as regras de desistência/retorno; a animação de investida já iniciada termina normalmente.

## Respawn fora da visão e longe de recompensas

- O respawn ainda respeita o intervalo por região, o limite máximo de inimigos, os pontos de entrada, o selo e as colisões.
- Novos inimigos **não devem surgir dentro da área visível da câmera**, incluindo uma margem de 10% nas bordas. O algoritmo continua tentando posições válidas e, se não houver nenhuma, espera até o próximo ciclo: não força um surgimento injusto.
- Recursos próximos têm uma zona de exclusão de **1,85 unidade**, permitindo que o jogador recolha os fragmentos com mais clareza.
- Como antes, somente a região ativa processa respawns; não há spawns de guardiões adicionais.

## Indicador contextual

A cada 0,28 segundo, o painel de exploração procura hostis próximos a até 3,5 unidades **com linha de visão**. Se houver perigo, substitui temporariamente a dica de recursos por um alerta de um ou vários inimigos. Quando o confronto termina ou a personagem se afasta, o painel retoma a dica de coleta. As notificações de recompensas e demais controles não mudam.

## Compatibilidade e testes

Nenhum campo novo é adicionado aos arquivos de save: posição do território, memória visual e foco são transitórios, inicializados pela posição da criatura criada ou restaurada. A versão de save permanece **v1**; Android preview continua usando o mesmo identificador e o versionCode por execução. Android release usa versionCode **25** e ambos os builds apresentam **0.9.17**.

As verificações estruturais checam integração, regressões antigas, preservação das regras de spawn, saves e workflows; o novo `NecroEncounterEditorCheck` executa os cálculos reais de território, retorno, bordas de câmera e exclusão de recursos antes dos builds manuais.

### Roteiro de teste manual

1. Na Caverna Esquecida, atraia um Grave Pup para longe do ponto onde apareceu e recue; ele deve desistir da perseguição após perder o alvo e voltar para sua área.
2. Coloque uma rocha entre a protagonista e um inimigo. Ele não deve iniciar uma perseguição através do obstáculo, mas pode lembrar brevemente de um alvo que acabou de desaparecer de vista.
3. Observe o Rotboar ao perder o alvo após uma investida: a animação deve terminar e o animal deve retornar sem travamentos.
4. Continue lutando por alguns minutos. Novos monstros devem surgir fora do enquadramento e longe dos cristais, sem ultrapassar o limite da população da região.
5. Aproxime-se e afaste-se de inimigos; verifique se a linha de dicas do painel alterna o alerta e as informações dos recursos.
6. Feche e reabra o jogo, visite outras regiões e confirme que os recursos, vida, equipe, equipamento e fragmentos continuam salvos; teste Windows e Android separadamente quando desejar.

**Situação:** mudança integrada e sujeita aos testes estáticos. Compilação real Unity e validação de jogabilidade ainda necessárias.

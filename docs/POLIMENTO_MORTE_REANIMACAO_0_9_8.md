# Necromancer 0.9.8 — Morte e Ressuscitação

## Objetivo

Transformar a derrota de um inimigo em uma ação compreensível e útil: queda visual, corpo temporário e ritual de vínculo que dá origem a um servo. A etapa mantém o visual ilustrado sombrio e reaproveita os sprites e círculos existentes. Não introduz efeitos pesados nem altera atributos de combate.

## Cadáveres das criaturas comuns

Antes desta versão, os inimigos comuns eram destruídos imediatamente. O jogo já possuía o método Raise e a estrutura de salvamento de cadáveres, porém nenhuma morte normal criava registros nessa coleção; além disso, TickCorpses não era chamado no ciclo de jogo. A 0.9.8 fecha esse circuito.

Agora ratos, morcegos, esqueletos, aranhas, wisps, slimes, chefes e outras criaturas comuns com arte animada mantêm o próprio corpo quando derrotados. O desenho se inclina e colapsa brevemente, escurecendo até adquirir a aparência de cadáver. O cadáver fica disponível para vínculo durante 14 segundos, perde opacidade no final e é removido automaticamente. Há um limite de 16 cadáveres comuns por região para evitar acúmulo durante os respawns. Cadáveres não têm colisão física, pois seus atores já foram retirados da lista de inimigos.

Grave Pups mantêm os quadros de queda e o vínculo especial já existentes. Rotboars conservam a animação de três quadros e a janela de aproximadamente 3 segundos para vínculo, com dissolução suave na parte final. Criaturas aliadas derrotadas se dissipam e não criam novos cadáveres ressuscitáveis.

## Ritual e efeitos visuais

A ação ERGUER agora procura o cadáver mais próximo entre criaturas comuns, Grave Pups e Rotboars. O mesmo botão é utilizado no Android, e a tecla E executa a ação no Windows dentro da caverna. A tecla E permanece destinada às estações de interação quando a protagonista está no refúgio.

Ressuscitar um cadáver comum exige ânima conforme a espécie e espaço no limite de comando já existente. O vínculo especial de Grave Pups e Rotboars continua sem custo de ânima, mas também respeita o limite de comando. Quando a criatura é reanimada, um círculo ritual esverdeado aparece no chão e o corpo aliado se materializa suavemente durante 0,7 segundo; nesse curto intervalo o servo não ataca, evitando animações sobrepostas. Um brilho discreto ascende no momento da morte, em violeta nos hostis e verde nos aliados.

O limite já existente de 30 efeitos visuais simultâneos foi preservado. O sistema não cria texturas nem sprites novos e utiliza a ilustração original da própria criatura.

## Salvamento, retorno ao refúgio e compatibilidade

Os cadáveres comuns são registrados na coleção de cadáveres da região, com espécie, posição e tempo restante. O carregamento recria sua aparência caída usando os recursos existentes, sem reiniciar a contagem. Os marcadores de cadáveres de arquivos antigos continuam aceitos.

Os servos de espécies comuns também passam a ser carregados no painel Meus servos, que antes só restaurava Grave Pups e Rotboars. O painel apresenta os retratos e nomes de cada espécie e respeita o limite de comando. Novos vínculos e dispensas atualizam a lista de companheiros e o snapshot de progresso.

O arquivo necro-save-v1.json mantém a versão 1 e o mesmo formato. A assinatura, o identificador Android e os workflows independentes e manuais de Windows e Android não foram alterados; apenas a versão de exibição passa a 0.9.8.

## Roteiro de testes

1. Derrote um rato, esqueleto ou wisp e observe a queda, o cadáver visível e sua remoção ao completar 14 segundos.
2. Aproxime-se e pressione E no Windows ou ERGUER no Android. Verifique o desconto correto de ânima, a transição esverdeada e a participação do novo servo no combate.
3. Tente ressuscitar sem ânima e com o limite de comando completo. A criatura não deve ser criada nem os recursos consumidos.
4. Derrote e reanime Grave Pups e Rotboars e confirme que os três quadros de queda, o ritual e os limites de comando permanecem corretos.
5. Faça muitos combates consecutivos e verifique que a região não acumula mais de 16 cadáveres comuns ao mesmo tempo.
6. Reanime um servo comum, retorne ao refúgio, feche e reabra o jogo e confira se ele reaparece no painel Meus servos e pode ser recolhido/retirado.
7. Volte à caverna, teste Caça/Defesa, Pulso Necrótico e Ordem dos Mortos e verifique que as animações e a progressão não sofreram regressões.
8. Se houver um save anterior com registros de cadáveres, confirme que é carregado sem reiniciar o jogo.

## Estado de validação

Os testes rápidos do GitHub verificam a estrutura do código e os pontos de integração. Ainda é necessário compilar manualmente uma das plataformas e confirmar visualmente os efeitos em execução antes de tratar a 0.9.8 como aprovada para distribuição.

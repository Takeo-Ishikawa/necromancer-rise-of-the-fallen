# Necromancer 0.9.20 — Extração ou captura de criaturas caídas

A interface contextual segue a ideia de duas escolhas por criatura do exemplo de Wild Tamer, usando exclusivamente a arte e os sistemas originais do projeto.

Ao derrotar uma criatura, o corpo permanece no chão por cerca de **14 segundos**. A necromante pode se aproximar e escolher **EXTRAIR**, que concede fragmentos, ou **CAPTURAR**, que cria um servo aliado. As ações são mutuamente exclusivas: a criatura é removida da lista antes de gerar qualquer recompensa.

**Fragmentos:** 2 para espécies comuns; 3 para Grave Pup, Skeleton e Wisp; 5 para Rotboar; 8 para guardiões. **Captura:** respeita o limite de comando; cadáveres comuns custam ânima para personagens mágicos, preservando o custo do antigo ritual, enquanto Grave Pups e Rotboars não custam ânima. Personagens físicos capturam sem usar magia.

O HUD mostra os dois botões na proximidade da personagem e uma contagem do tempo restante. No Windows, **F** extrai e **E** captura; no Android, basta tocar. A ação anterior ERGUER foi substituída pelos botões. Ataque, táticas, portais, refúgio e save v1 são mantidos.

Nota: cadáveres comuns já são persistidos no save v1. Corpos de Grave Pups e Rotboars ainda são transitórios como antes e não persistem após fechar o jogo. A primeira versão da mecânica reutiliza todos os sprites existentes, sem criar arte nova.

## Teste manual

1. Derrote um Grave Pup e aproxime-se antes do prazo. Escolha EXTRAIR e confira que recebe fragmentos apenas uma vez.
2. Derrote outro e escolha CAPTURAR. Confirme que ele vira servo, desaparecendo do solo e sem gerar fragmentos.
3. Teste as opções com Rotboar e outros monstros em regiões posteriores.
4. Confirme que a captura é impedida quando os pontos de comando acabam; para a necromante mágica, cadáveres comuns também exigem ânima.
5. Teste novamente o botão ATACAR, o joystick, as viagens e os dados carregados de um save antigo.

A versão de build é **0.9.20**, Android release versionCode **28**, identificador de preview e formato de save inalterados. A execução de testes C# no editor Unity e a validação prática do Android/Windows continuam necessárias.

# Necromancer 0.9.11 — Vida, derrota e recuperação

## Objetivo

Ativar o risco de combate da protagonista e garantir um ciclo completo de derrota, sem destruir a progressão ou comprometer a experiência nos controles Windows e Android.

## Sistema de dano

Os inimigos passam a reduzir o HP da necromante de acordo com o valor de ataque já definido em cada criatura. **Não são alterados os danos dos monstros nem dos servos.** Após cada acerto, a protagonista fica protegida contra novos danos por **1,1 segundo**; esse estado é indicado por uma breve mudança de cor no sprite e por um efeito de impacto já existente. A proteção não impede movimento nem ataque, apenas novos descontos de vida, evitando a perda instantânea de HP quando vários monstros atacam ao mesmo tempo.

Ao entrar ou trocar de região da caverna, a necromante também recebe 1,1 segundo de proteção contra emboscadas no ponto de carregamento. A câmara segura continua sem dano. A barra de vida do HUD, os bônus de Vitalidade e do Manto, bem como os rituais de HP, utilizam o mesmo valor de vida já salvo e não exigem novos campos.

## Derrota

Quando o HP chega a zero, o jogo marca a derrota e interrompe imediatamente a atualização de combate. A fila de ataque e o lançamento pendente são cancelados. Uma janela escura cobre a tela e informa a recuperação disponível, a penalidade e o botão **RETORNAR AO REFÚGIO**. O painel funciona por toque no Android ou clique no Windows. Também aceita **Enter** e **Espaço** no Windows, mesmo com o `Update()` suspenso.

O jogo não deleta os servos, não diminui níveis, equipamentos nem habilidades. A progressão permanece no mundo existente.

## Recuperação

Ao confirmar a recuperação, a necromante retorna **à câmara segura**, nunca diretamente à caverna. A vida é restaurada para 65% do HP máximo, a ânima para seu valor máximo, e a penalidade é de **até 2 fragmentos de alma**, limitada ao saldo atual. O jogo restaura os controles, interrompe efeitos e projéteis pendentes, reposiciona os servos e grava o resultado em `necro-save-v1.json` e no registro de companheiros.

A recuperação não ressuscita automaticamente servos derrotados anteriormente. Criaturas que ainda estavam ativas acompanham a protagonista. Os estágios visitados e os demais snapshots permanecem preservados, sujeitos às limitações existentes de restauração de regiões ainda não carregadas no início da sessão.

## Roteiro de validação

1. Testar sem servos: sofrer ataques de Grave Pups e confirmar a perda de HP, a proteção breve e a atualização da barra de vida.
2. Permanecer perto de 2–3 monstros para confirmar que vários contatos no mesmo instante não removem a vida inteira durante a imunidade.
3. Aguardar o HP chegar a zero; verificar que movimento, ataque, Pulso Necrótico, Ordem dos Mortos e ERGUER deixam de funcionar enquanto a tela de derrota está aberta.
4. Clicar ou tocar em **RETORNAR AO REFÚGIO**; no Windows, testar também Enter e Espaço.
5. Confirmar retorno à câmara segura, 65% de HP, 100% de ânima e dedução de no máximo 2 fragmentos.
6. Verificar se os servos sobreviventes continuam disponíveis e se o guarda-roupa, o altar e a Cripta funcionam.
7. Fechar e reabrir a sessão e validar a preservação de nível, equipamento, habilidades e demais dados.
8. Retornar à caverna e confirmar a imunidade curta na entrada, depois funcionamento normal dos ataques.
9. Testar com 0, 1 e 2 fragmentos para garantir que o total nunca fique negativo.
10. Repetir os pontos centrais no Android, principalmente a tela de derrota e os botões por toque.

## Compatibilidade

A versão exibida passa a **0.9.11**. O esquema e o nome do salvamento continuam na versão v1, sem novos campos. O pacote Android, a assinatura e os workflows de compilação Windows/Android permanecem inalterados e manuais. Não foram criadas texturas ou animações externas novas: os efeitos usam recursos já disponíveis.

Esta etapa foi desenvolvida no código e possui testes de estrutura/regressão. **A compilação Unity e a avaliação de jogabilidade precisam ser feitas em uma próxima sessão de testes.**

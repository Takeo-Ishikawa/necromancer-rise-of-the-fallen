# Necromancer 0.9.18 — acesso às quatro regiões

## Problema

O projeto continha quatro regiões e as rotinas Advance/Retreat, porém não instanciava o selo de passagem; por isso NearSeal sempre falhava e Claim nunca era acionado. O botão existente no HUD antigo não estava conectado à interface principal. Os guardiões das duas regiões finais também eram excluídos da lista de criaturas.

## Correção

Toda região agora possui um selo visual criado com o sprite já existente. Na Caverna Esquecida e nos Túneis Inferiores, basta alcançar o selo para reivindicar a região, sem necessidade de eliminar todos os inimigos (que reaparecem). Na Necrópole das Cinzas e no Pântano dos Sussurros, o respectivo guardião é criado e sua derrota libera a passagem. Reanimar o guardião continua opcional.

A interface ativa exibe orientação de distância e direção para o selo e oferece o botão AVANÇAR quando a protagonista está perto do selo desbloqueado; ao retornar ao início de uma região posterior, mostra VOLTAR. A navegação entre o refúgio e a primeira caverna continua independente.

Estados claimed, bossDefeated e bossBound de saves anteriores são reaplicados ao criar a região, evitando ressurgimento de chefes já derrotados. A versão do save permanece v1, e não foram adicionados campos novos. Android preview preserva o identificador e a assinatura, Windows e Android mantêm workflows manuais separados.

## Teste recomendado

1. Do refúgio, entre na primeira caverna, siga o indicador SELO DE PASSAGEM e aproxime-se da marca. Deve aparecer AVANÇAR: Túneis Inferiores.
2. Repita nos túneis; confirme que o botão VOLTAR funciona perto do início.
3. Na Necrópole, derrote o Cavaleiro das Cinzas para liberar a passagem ao pântano.
4. No Pântano, derrote o Colosso e confirme o anúncio de conquista.
5. Volte ao refúgio, feche e reabra o jogo e confira a persistência das conquistas e dos equipamentos.

A versão de build passa a 0.9.18, Android release versionCode 26. Compilação real Unity e testes de jogabilidade ainda devem ser realizados.

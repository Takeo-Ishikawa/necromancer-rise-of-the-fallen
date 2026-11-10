# Necromancer 0.9.19 — interface e câmara simplificadas

Removidos da interface ativa os botões **Pulso** e **Ordem**, inclusive as áreas invisíveis de toque/mouse correspondentes. **Q** e **R** continuam funcionando no Windows para personagens com magia, pois a solicitação foi de remover os botões da tela.

A área de **orações/meditação** e o **guarda-roupa** continuam visualmente na câmara, mas não são mais interativos. Apenas a fonte arcana permanece como estação ativa. A validação das estações permite exclusivamente a fonte mesmo com configurações antigas; abertura de painéis e gasto de fragmentos/pontos dos dois sistemas desativados têm travas adicionais.

Mantidos: ataque manual, reviver criaturas, táticas de servos, fonte arcana, Cripta, viagens entre regiões, nível e XP. Salvamentos v1 antigos continuam armazenando o progresso de equipamentos, habilidades e pontos já investidos, com seus efeitos anteriores preservados. Nenhum progresso é zerado.

## Teste

1. No refúgio, aproxime-se do guarda-roupa e do local de orações: nenhuma ação deve aparecer ou responder à tecla E.
2. Teste a fonte arcana; a bênção deve permanecer funcional.
3. Entre na caverna e confirme que os botões de Pulso e Ordem sumiram e que tocar no antigo local não ativa habilidade.
4. Confira ataque, reviver, táticas, exploração e passagem entre regiões.
5. No Windows, Q e R permanecem disponíveis nas personagens mágicas.
6. Reabra um salvamento e confirme os dados de equipamentos, servos, fragmentos e regiões.

Build: 0.9.19; Android release versionCode 27; ID de preview e save v1 inalterados. Não substitui teste real Unity.

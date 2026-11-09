# Necromancer 0.9.15 — Recursos coletáveis e fluxo de progressão inicial

## Defeito corrigido

O projeto possuía `ResourceSpawn` no catálogo de níveis e `CollectNearby()` com recompensas, mas o fluxo normal não chamava a criação inicial dos recursos nem a coleta. Isso deixava os fragmentos inacessíveis, impedindo testar o aprimoramento de equipamentos.

A cada visita a uma região ainda não instanciada, `LoadStage()` agora cria os cristais/ossos/fungos posicionados em `levels.json`. O personagem recolhe automaticamente drops de equipamentos em um raio de 0,64 unidade e recursos em um raio de 0,70 unidade. A cada recurso obtido, recebe **2 fragmentos**, um pouco de vida (12% do HP máximo, mínimo 9, limitado à vida faltante) e até 12 de ânima. Os ganhos são os que já estavam previstos no código; não há menu de coleta nem botão novo.

O mapa inicial recebeu um quinto cristal, próximo à entrada, totalizando **5 cristais × 2 fragmentos = 10 fragmentos**, suficientes para **um aprimoramento inicial do manto** (custo 10), ou para o foco arcano (custo 8). Os preços, os demais mapas e o sistema de combate não foram alterados.

## Salvamentos antigos e segurança contra duplicação

A versão do save continua **1**. A estrutura `NecroStageRecord` ganhou somente o campo opcional `resourcesInitialized`:

- Saves anteriores à 0.9.15 não têm esse campo. Seu valor padrão será `false`; uma região visitada com snapshot antigo recebe os recursos originais quando é recriada.
- Os saves novos têm `resourcesInitialized=true` nos estágios instanciados, e o array `resources` guarda **somente os recursos ainda no chão**.
- Um array vazio com o marcador `true` significa que tudo foi coletado. Ao fechar, abrir ou entrar de novo no mapa, a região não recria cristais já obtidos.
- `PopulateStageResources` é compartilhado pela inicialização de região e pela restauração de snapshots. `RestoreStage` limpa a lista anterior antes de chamar a função, evitando duplicação.
- Todos os recursos coletados no mesmo quadro geram uma única operação `SaveProgress()`, em vez de salvar repetidamente por item.

O mesmo arquivo `necro-save-v1.json`, a mesma identidade da preview Android e a assinatura anterior continuam sendo utilizados. Não é necessário reinstalar nem zerar o progresso.

## Interface e controles

O painel superior da caverna mostra `FRAGMENTOS` e `RECURSOS` restantes na região; após passar perto de um recurso, os valores mudam imediatamente. O combate no Windows ganhou a tecla **Espaço** para usar o ataque manual, além do botão existente; dispositivos Android preservam o toque.

## Validação e próximos passos

Os testes estruturais verificam a chamada de coleta, a criação de recursos, a migração dos snapshots, o não ressurgimento de itens coletados, os 10 fragmentos possíveis na primeira região e os contratos dos builds. O verificador de save Unity testa a nova marca opcional e a leitura de JSON antigo. A compilação real Unity permanece necessária para testar o C# e o runtime.

Roteiro recomendado: iniciar a preview Windows com o save atual, entrar na primeira caverna e encontrar os cristais; aproximar-se para ganhar fragmentos; voltar ao guarda-roupa e testar um grau do manto; fechar o jogo, reabrir e confirmar que os cristais não reaparecem, os fragmentos permanecem e os servos/equipamentos continuam salvos. Verificar também a tecla Espaço no combate e a coleta de recursos nas demais regiões.

**Notas de escopo:** por enquanto, não há respawn de recursos nem alterações nos preços. O balanceamento da economia pode ser decidido depois dos testes.

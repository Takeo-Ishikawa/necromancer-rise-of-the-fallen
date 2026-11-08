# Necromancer 0.9.7 — Polimento das animações e do combate

## Objetivo

Facilitar a leitura das ações dos servos e dos inimigos sem alterar dano, custo de ânima, cadência de golpes, colisões ou ilustrações já aprovadas. Esta etapa utiliza exclusivamente recursos de arte existentes.

## Alterações

**Ataques orientados:** criaturas genéricas passam a olhar na direção efetiva do alvo ao atacar, mesmo quando paradas. O sistema reinicia a sequência de ataque corretamente e evita alterar a orientação durante sua execução. Para criaturas sem atlas, o espelhamento horizontal também recebe o rumo do alvo.

**Grave Pups:** durante a preparação do salto, o corpo comprime e o círculo de chão já existente pulsa com cor diferenciada: rosa-avermelhado para hostis e verde-água para servos. No salto, o corpo se alonga; ao receber dano, ocorre uma compressão visual breve. Não foram adicionadas imagens nem alterados os tempos de preparação, salto e recarga.

**Rotboars:** o início da preparação da investida produz um círculo ritual temporário e discreto, em vermelho para hostis ou verde-água para servos. O animal comprime levemente o desenho na preparação e o alonga na carga, restaurando o tamanho inicial quando volta ao repouso. A coloração reage a impactos, e o estado de morte volta à escala e à cor originais antes dos quadros de queda.

**Impactos:** os efeitos já existentes para acertar o corpo de uma criatura são rosados quando o alvo é hostil e azul-claros quando o alvo é um servo. Os efeitos continuam visuais, sem alterações nas regras de combate.

**Desempenho:** os ângulos usados para tentar contornar obstáculos foram movidos para um array estático compartilhado. Antes havia criação de um novo array para cada tentativa de desvio; agora é reutilizado. O limite de 30 objetos de efeito simultâneos continua em vigor.

## Validação recomendada

1. Aproximar-se de um Grave Pup hostil e observar o aviso antes do salto; ressuscitá-lo e conferir a cor de servo.
2. Enfrentar um Rotboar inimigo, observar o aviso antes da investida e confirmar que a duração e o dano não mudaram.
3. Usar o Rotboar aliado e verificar a cor própria, reação ao dano, recuperação da postura e aparência durante a queda.
4. Testar esqueletos e outras criaturas comuns atacando quando paradas, conferindo que se voltam para o inimigo.
5. Enfrentar grupos com muitos servos e conferir que os efeitos não obscurecem os personagens nem causam quedas perceptíveis de desempenho.
6. Verificar Caça, Defesa, Pulso Necrótico, Ordem dos Mortos, refúgio, persistência do progresso e retorno à caverna.

## Compatibilidade

Versão de exibição: **0.9.7**. O arquivo de salvamento v1 e seus campos não mudaram. Também não mudaram a assinatura, o package ID Android, os controles nem os workflows separados e manuais de compilação. Os testes automatizados são estruturais; a compilação Unity e o teste visual em Windows/Android ainda precisam ser executados manualmente.

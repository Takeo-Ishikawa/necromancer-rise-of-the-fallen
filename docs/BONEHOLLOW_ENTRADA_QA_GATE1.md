# Bonehollow — QA da entrada modular (GATE 1)

**Objetivo:** verificar a primeira composição visual da entrada sem expandir os outros biomas prematuramente. A execução do GitHub Actions prova apenas a integridade do projeto Unity e do APK; não equivale a avaliação no celular.

## Pré-condições

- Abrir a [execução Android #123](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/runs/43797597003), conferir se terminou com sucesso e obter o artefato APK desta **nova** build. A #122 não contém os módulos de entrada.
- Confirmar a atualização do app existente, sem desinstalar: o identificador da preview e a chave de assinatura são mantidos; o `versionCode` varia por execução do workflow.
- Registrar modelo do aparelho, versão do Android, RAM, escala de interface, número da build e data do teste.
- Em `levels.json`, a entrada atual é `(14,11)` nas coordenadas norte-para-sul do JSON; os primeiros encontros no recorte incluem Grave Pup em `(20,19)` e rato em `(24,14)`.

## Casos de teste

| Caso | Experimento | Aprovação esperada |
| --- | --- | --- |
| E01 | Entrar na caverna pela câmara segura; verificar toda a área inicial parado e caminhando | Sem arte ornamental do refúgio transplantada; rochas, chão, ossos e luz com perspectiva harmônica |
| E02 | Andar em oito direções junto à primeira curva, aproximando-se das paredes com a câmera acompanhando | A rocha visível condiz com a parede sólida; nenhuma pedra aparentemente grande ocupa o piso transponível |
| E03 | Aproximar-se do cristal em (18,12) e do rato em (24,14) | Recursos e inimigos continuam distinguíveis dos detalhes baixos do piso |
| E04 | Combater o Grave Pup em (20,19) e recolher recursos, com dois e seis servos quando possível | Personagens e cadáveres não ficam ocultos por ossadas ou contornos rochosos |
| E05 | Seguir até o início do salão de Ossadas e voltar pela entrada | Transição natural de saturação e iluminação; sem linha brusca indicando início/fim dos módulos |
| E06 | Abrir e fechar o minimapa junto a uma parede e a uma bifurcação | Obstáculos visuais continuam coerentes com a forma real do mapa |
| E07 | Caminhar e parar com 0, 2 e 6 servos em corredores sinuosos | Os servos não atravessam paredes nem ficam presos de maneira recorrente |
| E08 | Sair da caverna para o refúgio, voltar e repetir três vezes | Sem duplicação visível das 22 peças e sem crescimento anormal de uso de memória |
| E09 | Atualizar o APK sobre a versão anterior, abrir o jogo e conferir o save existente | Personagens, evolução, cofre e servos preservados, conforme mecanismo atual de save |

## Métricas e evidências

- Capturas: `Entrada parada`, `Entrada em movimento`, `Encontro com rato`, `Curva de túnel com servos`, `Transição para Ossadas`.
- Vídeo curto de caminhada/combate mostrando a legibilidade da área durante movimento.
- Comparar FPS em repouso, durante caminhada com seis servos e combate; registrar valores medidos ou indicar **não medido**.
- Registrar tempo de entrada no mapa e memória/estabilidade somente com dados do aparelho, não deduzir do tamanho do APK.
- Se uma peça causar problema visual, anotar o `id` da planta `BONEHOLLOW_ENTRADA_KIT_PLANTA.svg`, o local e a correção proposta.

## Regra de fechamento

**GATE 1 permanecerá aberto** até o usuário verificar a vertical slice em dispositivo real e aprovar o contraste, a disposição de rochas, iluminação, sombreamento e visibilidade dos personagens. Alterações futuras podem substituir a arte de um `kind` sem mover todas as 22 instâncias, graças ao layout JSON e ao cache de nove sprites.

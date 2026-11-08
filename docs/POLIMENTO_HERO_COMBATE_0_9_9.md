# Necromancer 0.9.9 — Polimento do combate da necromante

## Escopo

Esta versão trabalha a legibilidade da protagonista durante o combate sem substituir a arte ilustrada, trocar a câmera, alterar atributos ou adicionar sons. O mesmo código é utilizado nos builds Windows e Android, que continuam independentes e manuais.

## Animação e resposta dos golpes

O jogo já acionava os temporizadores `heroineStrikePulse` e `heroineRaisePulse`, mas eles eram pouco aproveitados no método de apresentação da protagonista. A 0.9.9 utiliza esses sinais com prioridades claras: **lançamento espectral → invocação → ataque físico → caminhada/repouso**. A protagonista agora orienta o corpo para o alvo do ataque e exibe uma breve preparação, movimento para a frente e recuperação no golpe físico. No ritual, utiliza uma sequência de poses espectrais existente, com balanço discreto. A recuperação visual não altera a colisão, a velocidade do personagem ou o tempo de recarga.

O ataque físico continua com o mesmo valor de dano, o mesmo tempo de recarga de 0,47 segundo e sem gasto de ânima. O lançamento espectral permanece gratuito, com liberação do projétil em 0,28 segundo, animação de 0,62 segundo e recarga de 0,72 segundo (ou 0,60 com o passivo Ritmo Profano). Os estados visuais são interrompidos ao mudar de região para não reaparecerem indevidamente no refúgio.

## Projéteis espectrais

A caveira disparada agora recebe uma pulsação suave em tons violeta e verde-água e uma dissipação curta ao final. Um brilho compacto aparece quando ela sai das mãos da necromante; outro surge quando atinge um monstro ou uma obstrução. O golpe físico também usa um efeito breve de contato. Os três efeitos reutilizam os sprites `NecroArt.Effect` e `NecroArt.RitualCircle` e seguem o limite global de 30 efeitos simultâneos, sem texturas novas.

A verificação de colisão do projétil passou a considerar **todo o segmento percorrido entre dois quadros**, evitando atravessar inimigos pequenos a taxas de atualização mais baixas. O trajeto continua condicionado à linha de visão e às obstruções do cenário. **O alcance, o raio de acerto, a velocidade de 7 unidades/s, a duração de 0,9 segundo e o dano do projétil permanecem iguais.**

## Compatibilidade

O formato `necro-save-v1.json` e o ID do aplicativo Android foram mantidos. Nenhum estado visual novo é serializado; os efeitos não permanecem em transições entre regiões. Os workflows separados de Windows e Android não passam a compilar automaticamente. A versão exibida nas configurações de build passa a **0.9.9**.

## Validação manual futura

1. Quando houver acesso ao Windows, compilar apenas o workflow Unity Windows x64 e iniciar no refúgio. Verificar se a personagem não sofre deslocamento permanente de pés/sombra.
2. Na caverna, testar ataque básico com e sem movimento, com a necromante voltada para diferentes direções, verificando animação e ausência de gastos extras de ânima.
3. Atacar ratos, morcegos e outros inimigos pequenos; verificar que o projétil acerta quando seu caminho cruza o corpo e que não atravessa rochas.
4. Observar o brilho de lançamento e o efeito de impacto, inclusive quando o projétil atinge um obstáculo.
5. Ressuscitar um cadáver comum e um Grave Pup: verificar a pose do ritual, o círculo de invocação e a formação dos servos.
6. Alternar rapidamente entre Pulso Necrótico, Ordem dos Mortos, ataque básico e movimentação. Confirmar que a interface, a recarga e o dano continuam corretos.
7. Voltar ao refúgio após lançar um projétil e verificar que não permanecem efeitos ou poses de combate.
8. Testar mais adiante no Android, verificando visibilidade dos efeitos sem queda perceptível de desempenho.

Os testes estruturais confirmam integrações e invariantes, mas **não substituem uma compilação Unity e a avaliação visual no jogo**.

# Necromancer — Unity 2D / Android Preview 0.6.2

O projeto Unity oficial está em `unity/` (Unity 2022.3.62f2, Android IL2CPP ARM64 + ARMv7).

## Novidades desta etapa

- **10 espécies visualmente diferenciadas:** rato, morcego, limo, aranha, cobra, esqueleto, espírito, Sentinela, Cavaleiro das Cinzas e Colosso do Pântano. Os sprites são originais e gerados uma vez pelo Unity em `NecroCreatureArt.cs`. Sprites PNG importados continuam tendo prioridade.
- **Animação das criaturas:** voos leves de morcegos e espíritos, oscilação dos limos, passos e balanços dos inimigos, movimento mais pesado dos chefes, reação de dano e círculo de aura nos servos.
- **Combate mais legível:** efeitos rápidos de impacto, círculo espectral ao ressuscitar e efeito especial ao derrotar chefes. O número de efeitos simultâneos é limitado para reduzir o custo em celulares.
- **Ambientes com identidade própria:** estalagmites e cristais na caverna, ossos e fungos nos túneis, lápides e lanternas na necrópole, juncos, cogumelos e poças no pântano. A decoração usa posições determinísticas, não bloqueia passagens e evita aparecer sobre inimigos/recursos.
- **Nenhuma mudança nas mecânicas ou no formato de salvamento.** A Cripta, equipamentos, rituais, IA, portais e os dados existentes permanecem disponíveis.
- **Teste de arte no Editor:** antes do APK, `NecroVisualEditorCheck` rasteriza e inspeciona as 10 criaturas e 10 elementos de cenário.

## Segurança ao instalar

Esta compilação é **uma prévia separada**, com pacote `com.takeo.necromancer.unity.preview062`, nome **Necromancer (Preview 0.6.2)** e armazenamento independente. Ela **não substitui** as versões 0.5 nem Preview 0.6.1. O salvamento dessas versões não aparece automaticamente nesta prévia.

Por causa da assinatura instável das compilações antigas, **não desinstale a 0.5**. O processo de produção já exige chave persistente; o procedimento para migrar manualmente o save está em [assinatura e backup](../docs/ANDROID_SIGNING.md).

## Controles e conteúdo preservados

Joystick, GOLPE/MAGIA, REVIVER, DEFESA/CAÇA, EVOLUIR, inventário, Cripta e portais continuam implementados. As quatro regiões do jogo são Caverna Esquecida, Túneis Inferiores, Necrópole das Cinzas e Pântano dos Sussurros.

O sprite da protagonista e a HUD da 0.6 continuam. Animações finais desenhadas em múltiplas direções e cenários completos de alta resolução ainda ficam para uma fase posterior. A implementação desta etapa é uma melhoria concreta no Unity, mas não equivale a spritesheets ilustrados finais.

O workflow [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) produz o APK. A build confirma compilação e arte básica, mas não substitui teste de jogabilidade no dispositivo.

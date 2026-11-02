# Necromancer: Rise of the Fallen — Android 0.2

Jogo nativo para **Android**, em Java com renderização 2D via Canvas. Sem WebView ou navegador.

## Instalação

Baixe o artefato `necromancer-android-v0.2` na execução mais recente em [GitHub Actions](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/android.yml). Extraia `app-debug.apk` e instale no Android 8.0+.

## Jogabilidade

- **Joystick esquerdo:** movimentação livre em oito direções.
- **GOLPE/MAGIA à direita:** ataque manual. O primeiro monstro derrota com os punhos; essa vitória desbloqueia a magia de projétil (9 de ânima).
- **REVIVER:** toque quando próximo a um cadáver destacado. Corpos comuns duram 32 segundos e o da sentinela dura 90 segundos. O destaque mostra o custo de ânima.
- **Segurar REVIVER por 0,65 s:** dispensar o servo mais próximo, liberando capacidade de comando sem devolver ânima.
- **DEFESA / CAÇA:** alterna a IA do exército entre proteger o personagem e perseguir monstros nas proximidades. Servos retornam à formação após a luta.
- **4 pontos de comando:** rato e morcego custam 1, limo custa 2 e sentinela custa 3. As invocações permanecem até morrer ou serem dispensadas. Morcegos atacam de longe e não atravessam rochas com seus ataques.
- **Objetivo final do prólogo:** derrotar e **ressuscitar** a sentinela; gerencie sua capacidade para conseguir vinculá-la.

## Compilação e testes

Com JDK 17, SDK Android 35 e Gradle 8.9:

```sh
gradle :app:assembleDebug
```

O pipeline `.github/workflows/android.yml` executa testes JVM independentes do Android e compila o APK. Fontes: `app/src/main/java/com/takeo/necromancer/`; testes: `tests/com/takeo/necromancer/`.

## Próximas etapas

A versão 0.3 implementará níveis, equipamentos e melhorias de criaturas. As versões seguintes acrescentarão regiões, chefes, cripta, rituais, evolução e polimento visual. Todos os recursos gráficos implementados são originais; Wild Tamer e Necromancer (Prismathunder) são apenas referências conceituais.

# Necromancer: Rise of the Fallen — Android

Protótipo nativo **Android** em Java (Canvas 2D), criado a partir do roteiro/GDD original. **Não é um jogo de navegador**.

## Instalação

Baixe o arquivo `necromancer-prototipo-android` na seção *Actions → Android APK → artefatos* no GitHub. Extraia `app-debug.apk` e instale no Android (pode ser necessário habilitar instalação de APKs do navegador/gerenciador de arquivos).

## Compilação

Requer Android SDK 35, JDK 17 e Gradle 8.9:

```sh
gradle assembleDebug
```

APK gerado em: `app/build/outputs/apk/debug/app-debug.apk`.

O workflow `.github/workflows/android.yml` compila e publica o APK debug automaticamente após cada push em `main`.

## Jogabilidade atual

- Exploração livre por joystick em mapa único de caverna, orientação vertical.
- Necromante começa sem magia: derrota o primeiro inimigo com um golpe corporal.
- Ao derrotar o primeiro monstro, desperta o projétil espiritual (ataque ativo).
- Inimigos perseguem e combatem com comportamento automático.
- Cadáveres permanecem temporariamente e podem ser ressuscitados com gasto de ânima.
- Servos persistem enquanto vivos, seguem o jogador e lutam automaticamente; pontos de comando limitam o exército.
- Ratos, morcegos, limos e sentinela de elite. Vitória ao derrotar a sentinela.
- Gráficos vetoriais estilizados e **originais**, desenhados em Canvas. Sem recursos importados de jogos terceiros.

**Controles:** joystick inferior esquerdo para movimentar; botão grande inferior direito para golpear/lançar magia; botão de caveira acima para ressuscitar cadáver próximo. A interface orienta o início do jogo.

Esta entrega contém as fundações de 0.1 com uma implementação preliminar da necromancia 0.2 para testar cedo o ciclo central do jogo. Próximas etapas: animação/sprites ilustrados autorais, sonorização, melhorias do pathfinding, expansão da cripta/equipamentos, save local e balanceamento.

© Projeto original. Wild Tamer e Necromancer (Prismathunder) foram considerados referências de gênero e atmosfera; nenhum recurso de terceiros foi copiado.
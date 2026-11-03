# Necromancer: Rise of the Fallen — Unity Android 0.5

**Motor principal: Unity 2022.3 LTS (versão 0.6).** O projeto fica em [`unity/`](unity/README.md), com quatro mapas conectados, exército autônomo, progressão, equipamentos, Cripta, rituais e salvamento local entre sessões. O build Unity gera APK com suporte **ARM64 via IL2CPP**.

**Versão 0.5:** a Cripta está acessível no santuário da caverna conquistada, com armazenamento de servos, expansão de nichos e rituais permanentes. O estado da exploração e do exército é salvo no Android com recuperação por backup. A versão Java/Canvas continua preservada; a migração Unity requer validação de jogabilidade e de salvamento em aparelho real.

---

# Prototipo Android legado — 0.4

Jogo Android **nativo** (Java e Canvas), com controles por toque, combate automático do exército, progressão de níveis e equipamentos.

## Instalação

Baixe o artefato **necromancer-android-v0.4** na [execução mais recente do GitHub Actions](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/android.yml). Extraia o ZIP e instale **app-debug.apk** no Android 8.0+.

## Novidades — Exploração

1. **Caverna Esquecida:** mantém o grande mapa e a conquista da base. Após derrotar todos os inimigos e vincular a sentinela, toque em *ENTRAR NA BASE* e aproxime-se do portal no Santuário das Almas (centro da caverna).
2. **Necrópole das Cinzas:** mapa original com lápides, mausoléu, esqueletos e espíritos. Colete ossos rituais. Derrote e ressuscite o Cavaleiro das Cinzas para liberar o portal do pântano.
3. **Pântano dos Sussurros:** novo mapa de lagoas sombrias, fungos espectrais e encontros mais difíceis. Derrote e ressuscite o Colosso para finalizar o capítulo.
4. **Portais bidirecionais:** retornam às regiões já descobertas. Inimigos remanescentes, itens e recursos coletados são preservados entre visitas, assim como o exército e a progressão.
5. **Recursos coletáveis:** cristais, ossos e fungos fornecem fragmentos de alma e restauram um pouco de vida e ânima quando o jogador se aproxima.
6. **Novos servos:** esqueletos (combatentes resistentes, custo de comando 2), espíritos (ataques de alcance, custo 1), Cavaleiro das Cinzas (3) e Colosso (4). Esqueletos e espíritos também podem ser aprimorados no painel EVOLUIR.
7. **Novo visual da protagonista:** arte vetorial original adaptada ao conceito compartilhado, com chapéu largo, cabelo claro, manto, cajado e lanterna. A arte final animada ainda será desenvolvida.

Os controles permanecem: joystick à esquerda; GOLPE/MAGIA, REVIVER e DEFESA/CAÇA à direita; EVOLUIR abre o painel de melhorias. Segure REVIVER para dispensar um servo e liberar capacidade de comando.

### Observações

O progresso continua restrito à expedição; sistema de salvamento e melhorias permanentes ficam previstos para a versão 0.5 (Cripta). A direção de arte usa elementos originais. Não foram copiados recursos do Wild Tamer ou de Necromancer/Prismathunder.

### Verificação

O GitHub Actions compila com JDK 17, Gradle 8.9 e SDK Android 35; roda os testes `WorldSmokeTest`, `WorldProgressionTest` e `WorldExplorationTest`, publicando o APK nativo como artefato.

```sh
gradle :app:assembleDebug
```

### Compatibilidade com a caverna ampliada

A atualização paralela do layout de 3072 × 2048 e o arquivo `assets/caverna_inicial_map.json` foram preservados. O pipeline roda também o `CaveExpansionTest` e valida a conquista da base.

## Atualização visual 0.6

A primeira implantação da arte ilustrada no Unity adiciona sprite transparente da necromante, animação procedural de caminhada e magia, HUD gótica funcional, minimapa e joystick revisado. Consulte [o guia da versão 0.6](unity/README.md). A fase de animações desenhadas quadro a quadro, criaturas e cenários ilustrados ainda será desenvolvida.

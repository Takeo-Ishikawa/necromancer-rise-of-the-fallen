# Necromancer: Rise of the Fallen — Android 0.3

RPG de ação e necromancia **nativo para Android** (Java e Canvas 2D), com controle por toque, combate em tempo real e servos autônomos. Não usa WebView, navegador ou recursos de outros jogos.

## Instalar

Acesse as [compilações no GitHub](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/android.yml), escolha a execução mais recente e baixe o artefato `necromancer-android-v0.3`. Extraia `app-debug.apk` e instale no Android 8.0+.

## Controles e novidades

- **Joystick:** movimento livre. **GOLPE/MAGIA:** ataque ativo; a magia é liberada depois da primeira criatura derrotada.
- **REVIVER:** toque perto de um cadáver para recrutar o monstro, pagando ânima e respeitando o limite de comando 4. Segure REVIVER para dispensar um servo.
- **DEFESA/CAÇA:** alterna o comportamento automático dos servos.
- **EVOLUIR (novo):** abre um painel e pausa o combate. Gaste fragmentos de alma, recebidos ao derrotar inimigos, para evoluir **ratos, limos e morcegos**. Cada espécie tem até **3 graus**. Cada grau acrescenta **24% de vida e 18% de dano** sobre os valores básicos para unidades ativas e futuras invocações; os custos são 2, 4 e 6 fragmentos.
- **Experiência (novo):** cada inimigo derrotado concede XP. Subir de nível aumenta a vida máxima (+12), ânima máxima (+7) e potência das habilidades, além de curar parte da vida.
- **Equipamentos (novo):** a cada duas vitórias, um item cai no cenário. Aproxime-se para coletar automaticamente. **Foco arcano** aumenta o dano do necromante; **manto** acrescenta vida e reduz o dano recebido. Itens superiores substituem os anteriores e duplicados viram fragmentos de alma. Ambos chegam até **grau 3**.
- **HUD (novo):** vida, ânima, XP, nível, foco, manto, fragmentos e capacidade do exército aparecem diretamente durante o jogo.
- **Objetivo do prólogo:** derrotar e ressuscitar a sentinela, liberando espaço de comando se necessário.

### Notas sobre a progressão

Nesta fase, níveis, equipamentos e melhorias duram **somente a expedição atual**. Salvamento e progressão permanente ficarão para o desenvolvimento da cripta, conforme o GDD. A direção visual ainda utiliza desenhos vetoriais originais e provisórios.

## Compilação e testes

JDK 17, Android SDK 35, Gradle 8.9:

```sh
gradle :app:assembleDebug
```

A automação do GitHub executa `WorldSmokeTest` e `WorldProgressionTest` com Java puro, compila o APK e anexa o resultado. Fonte Android: `app/src/main/java/com/takeo/necromancer/`.

Próxima etapa planejada: **0.4, exploração**, com novas regiões, coleta de recursos e chefes. Wild Tamer e Necromancer (Prismathunder) são referências conceituais, não fontes de sprites ou código.


## Caverna inicial ampliada (0.4)

A primeira arena agora é uma câmara 3072 × 2048, com espaços abertos. Ao eliminar os monstros e a sentinela, a caverna se torna a primeira base. Consulte [a documentação da caverna](docs/CAVERNA_INICIAL.md) para configurar o visual e os arquivos de mapa.

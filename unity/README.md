# Necromancer — Unity 2D / Android 0.5

**Unity Editor:** 2022.3.62f2 LTS · **Build:** Android APK · **Scripting:** IL2CPP · **CPU:** ARMv7 + ARM64.

O projeto oficial Unity esta na pasta `unity/`. A implementação Java/Canvas permanece em `app/` como backup e referência de regressão.

## Versão 0.5 — A Cripta

Após derrotar os monstros da Caverna Esquecida e ressuscitar a sentinela, aproxime-se do **Santuário das Almas** (selo central). O **altar espectral** marca a entrada da Cripta; toque em **ENTRAR NA CRIPTA**. No computador, também é possível apertar **C** ao lado do selo.

- **Guardar servo:** seleciona o aliado mais próximo e o remove da equipe ativa, preservando a vida e os atributos, sem criar cadáver.
- **Retirar servo:** escolha um dos servos armazenados. A retirada respeita os **4 pontos de comando** do exército; todos os atributos são restaurados.
- **Nichos:** a Cripta começa com **8 espaços**. Ritual de expansão aumenta em +4 por nível, até 16 espaços.
- **Ritual de vitalidade:** +15 de vida máxima e +5 de ânima máxima por nível, até 5 níveis. Custo inicial de 6 fragmentos.
- **Ritual de fluxo espiritual:** aumenta regeneração de ânima em +0,65/s por nível, até 3 níveis.
- **Fragmentos:** os rituais consomem fragmentos de alma conquistados em combate/exploração. Um ritual realizado é permanente em futuras sessões salvas.
- **Salvar:** botão manual na Cripta. Também existem saves automáticos a cada ~15 segundos, ao subir de área e ao realizar ações importantes, e na pausa/saída do Android.

## Persistência e recuperação

Dados locais em `Application.persistentDataPath/necro-save-v1.json`, com cópia de recuperação `.bak`. O formato JSON guarda:

- mapa atual e visitados, inimigos restantes, corpos, equipamentos caídos e recursos ainda não coletados;
- experiência, nível, habilidades, vida, ânima, equipamentos, fragmentos e melhorias;
- composição do exército, HP e dano dos servos, lista da Cripta, rituais realizados;
- conquista de chefes e acesso às regiões.

Quando o aplicativo é reaberto, **a última partida salva é restaurada automaticamente**. Se for derrotado, o necromante pode ressuscitar (restaura a vida e reduz até dois fragmentos), sem destruir a progressão.

O salvamento é **local, não em nuvem**. Desinstalar o aplicativo ou limpar seus dados apaga o progresso. A versão anterior 0.4 não tinha persistência, então o primeiro jogo na 0.5 começa uma partida nova.

## Organização

- `NecroPrototype.cs`: cena, câmera, mundos e transições.
- `NecroGameplay.cs`: IA, combate, necromancia, progressão e equipamentos.
- `NecroCrypt.cs`: interface lógica da Cripta, rituais e captura/restauração de estados.
- `NecroSaveData.cs`: classes serializáveis e arquivo de save com backup.
- `NecroUI.cs`: controles, menu da Cripta e evolução.
- `NecroArt.cs`: arte 2D procedural autoral, com bruxa, monstros e altar espectral.
- `NecroSaveEditorCheck.cs`: teste de serialização Unity executado durante o build.
- `Assets/Resources/Data/levels.json`: Caverna, Túneis, Necrópole e Pântano.

## Instalação / build

No Unity Hub, abra `unity/` com **Unity 2022.3.62f2** e suporte Android. A cena é montada automaticamente, caso ainda não exista. O workflow [Unity Android APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) compila o projeto com a licença configurada em secrets e publica o artefato `necromancer-unity-android-apk`.

Testes de estrutura em `tests/check_unity_project.py` e verificação de **round-trip JSON no próprio Unity Editor** durante a compilação. A compilação confirma integridade técnica, mas o comportamento visual e a restauração após reiniciar o aparelho ainda precisam de testes manuais no Android.

Todos os gráficos são originais ou provisórios. O conceito da protagonista continua sendo referência para os sprites ilustrados finais.

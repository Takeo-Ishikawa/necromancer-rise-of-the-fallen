# APK Unity Android

Unity 2022.3.62f2 LTS; projeto na pasta `unity/`.

### Compilacao no GitHub

O workflow [Unity Android APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) executa verificacao estrutural, importa o projeto no Unity, compila com IL2CPP para ARMv7 + ARM64 e publica o artefato `necromancer-unity-android-apk`.

A compilacao exige uma **licenca Unity configurada nos secrets do GitHub** (`UNITY_LICENSE` ou `UNITY_SERIAL`). Nunca inclua licencas nem senhas no repositorio.

O sucesso do workflow significa que o APK foi produzido pelo Unity. **Nao equivale a teste de jogabilidade em telefone**. Antes de publicar, validar criacao de partida, combate por toque, necromancia, transicoes, upgrade, derrota e reinicio em aparelhos ARM64.

### Compilacao local

No Unity Hub, instale Unity 2022.3.62f2 com Android Build Support, SDK, NDK e OpenJDK. Abra `unity/`, aguarde o auto-bootstrap da cena `Assets/Scenes/NecromancerPrototype.unity`, clique Play para inspecionar e utilize **Build Settings > Android > Build**. A cena inicial e criada no editor se estiver ausente.

### Arte

Sprites e fundos procedurais originais sao usados quando `Assets/Resources/Sprites/<nome>.png` e `Assets/Resources/Backgrounds/<nome>.png` nao existem. A bruxa foi redesenhada como sprite legivel, mas a ilustracao definitiva precisara de sprites transparentes e animacoes em direcoes.

A implementacao Java/Canvas em `app/` continua disponivel para comparacao.

### Prévia contínua: APK sem ZIP

A partir do novo padrão, o workflow `unity-android.yml` usa `actions/upload-artifact@v7` com `archive: false`, disponibilizando o APK diretamente. A assinatura é persistente e o versionCode cresce com cada execução. **Antes do primeiro build dessa modalidade, configure os dois Secrets de preview** conforme [APK de teste atualizável](APK_PREVIEW_ATUALIZAVEL.md). Não reinstale versões antigas sem antes considerar os salvamentos.

### Fluxo independente por plataforma

O Android Unity agora é compilado **somente pelo Run workflow**. A versão Windows usa um workflow separado e também manual. Consulte [Compilações Android e Windows](COMPILACAO_ANDROID_WINDOWS.md). O APK continua direto e atualizável; a versão Windows é um ZIP com EXE, DLLs e arquivos de dados do Unity.

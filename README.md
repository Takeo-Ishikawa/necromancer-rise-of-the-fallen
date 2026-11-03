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


**Atenção às atualizações Android:** a 0.5 e a 0.6 usaram assinaturas de depuração distintas. Antes de instalar uma versão diferente, veja [assinatura, prévia e preservação do save](docs/ANDROID_SIGNING.md). A prévia é instalada ao lado da 0.5 e os releases exigem uma chave persistente.

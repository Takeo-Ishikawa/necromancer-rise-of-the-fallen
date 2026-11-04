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


### Nova etapa visual 0.6.2

Os dez tipos de criaturas agora têm sprites próprios, animações de combate e diferenciação entre hostis e servos. Cada bioma recebeu decoração 2D original e efeitos de batalha. O APK de teste utiliza o identificador separado `com.takeo.necromancer.unity.preview062` para não afetar o progresso da 0.5. Confira [as instruções da versão](unity/README.md).


### Animação por quadros 0.6.3

Agora a necromante e oito tipos de monstros usam atlases de quadros reais no Unity, com direções e estados de gameplay. Os quadros foram derivados da arte original e ainda não representam ilustrações manuais finais. A prévia usa um identificador de aplicativo separado para preservar instalações anteriores. [Mais detalhes](unity/README.md).

### Atualização de jogabilidade 0.6.4

Inimigos comuns reaparecem em regiões conquistadas (sem reanimar chefes), o exército passou para 12 pontos de comando e o necromante ataca automaticamente. O botão de ataque foi removido; REVIVER, DEFESA/CAÇA e EVOLUIR permanecem. A prévia 0.6.4 tem instalação separada para proteger o salvamento da 0.5. Veja [o guia da versão](unity/README.md).


### Prévia 0.6.5a — Protagonista animada

A protagonista tem uma nova folha PNG de 16 poses/vistas ilustradas (frente/costas/perfis e estados de ação). O Unity gera e utiliza 140 sprites animados em 24 sequências, sem deformar a mesma vista frontal para simular costas. A nova versão preserva as regras e saves e usa identificador separado para teste Android. Consulte [o guia visual 0.6.5a](unity/README.md).


### 0.6.5b — personagem nítida e idle/walk aprimorados

Corrigida a origem minúscula que borrava a necromante na versão anterior: agora são 16 poses de 160×160 pixels, com desenhos distintos para frente, costas, esquerda e direita e quadros de repouso/caminhada. O retrato da HUD também foi atualizado. [Detalhes](unity/README.md).

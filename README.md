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


### 0.6.5c — Caverna ilustrada e interface compacta

A primeira etapa de recursos gráficos estáticos PNG foi adicionada ao Unity: chão em escala nativa, rochas e cristais, HUD mais enxuta e sombras/halo da necromante. A URP ainda não foi ativada. [Implementação e testes](unity/README.md).

### Padrão de otimização mobile (0.7.6)

Antes de adicionar novos cenários, monstros, efeitos ou sprites, consultar o [guia permanente de recursos e otimização Android](docs/PADRAO_OTIMIZACAO_ANDROID.md). O projeto aplica compressão ETC2 no refúgio e impede que fundos antigos voltem à pasta `Resources` por meio dos testes automáticos.

### APK direto e atualização contínua

As próximas previews Unity usam sempre o mesmo identificador Android e uma assinatura de testes permanente. O workflow publica o arquivo APK diretamente, sem ZIP. Para realizar a configuração inicial segura da chave de testes (apenas uma vez), siga [APK de teste atualizável](docs/APK_PREVIEW_ATUALIZAVEL.md). A versão antiga com assinatura temporária pode exigir uma única migração.

### Compilações seletivas Windows/Android

Agora você escolhe a plataforma na aba Actions: [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) ou [Unity Windows x64 Preview](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-windows.yml). **Ambos são manuais**, não executam no mesmo commit. Guia: [Compilação Android e Windows](docs/COMPILACAO_ANDROID_WINDOWS.md).

### 0.9.13 — Defesa e recuperação

O manto reduz 6% do dano recebido por grau, até 18%, além dos bônus de vida existentes. A fonte arcana passa a recuperar 30% da vida máxima por acionamento (mínimo de 18 HP), respeitando sua recarga. Saves, ID da preview e workflows manuais são preservados. [Regras e verificações](docs/DEFESA_E_RECUPERACAO_0_9_13.md).

### 0.9.14 — IA tática e feedback de combate

Inimigos comuns, Grave Pups e Rotboars agora compartilham a seleção de alvos, com alcance físico por espécie e retenção de foco. Servos em DEFESA priorizam ameaças à necromante e adotam uma primeira formação de flanco. A barra de vida exibe brevemente dano recebido e proteção do manto. A preview mantém seu identificador e salvamento v1; builds continuam manuais. [Detalhes e roteiro de testes](docs/COMBATE_TATICO_0_9_14.md).

### 0.9.15 — Coleta de recursos e primeiros fragmentos

Os cristais, ossos e fungos definidos nos mapas agora aparecem e são coletados automaticamente por aproximação. A primeira caverna tem cinco cristais, suficientes para um aprimoramento do manto; o progresso de coleta é salvo e não se duplica ao reiniciar. A HUD mostra fragmentos e recursos restantes, e a tecla Espaço ataca no Windows. O formato de save v1 e o ID da preview permanecem. [Detalhes e roteiro de testes](docs/RECURSOS_COLETA_0_9_15.md).

### 0.9.16 — Exploração e recompensas mais claras

Recursos próximos ganham brilho leve; o indicador de exploração mostra tipo e distância até o recurso visível mais próximo. A coleta apresenta uma notificação breve que acumula fragmentos, vida e ânima realmente recebidos, sem cobrir avisos importantes de combate. Sem mudanças na economia, nos salvamentos v1 nem nas identidades das builds. [Notas e roteiro de teste](docs/EXPLORACAO_RECOMPENSAS_0_9_16.md).

### 0.9.17 — Encontros prolongados e exploração segura

Inimigos retornam ao local de origem após perder o alvo, com memória curta de linha de visão; novos spawns evitam a câmera e as áreas de coleta. O painel contextual avisa quando há ameaças próximas. Economia, arte, formato de save e workflows permanecem inalterados. [Notas e testes recomendados](docs/ENCONTROS_E_EXPLORACAO_0_9_17.md).

### 0.9.18 — Acesso às quatro regiões corrigido

Selos de passagem criados em todas as regiões, conquista acionada por proximidade ou derrota do guardião, chefes restaurados nos estágios avançados e botões reais de avanço/retorno na interface ativa. Saves anteriores compatíveis. [Roteiro de teste](docs/ACESSO_REGIOES_0_9_18.md).

### 0.9.19 — Interface e refúgio simplificados

Botões de Pulso e Ordem removidos. Guarda-roupa e local de orações mantidos apenas como decoração; fonte, ataque, servos e exploração preservados. Saves v1 intactos. [Notas de teste](docs/INTERFACE_SIMPLIFICADA_0_9_19.md).

### 0.9.20 — Extração ou captura após derrotar criaturas

Monstros caídos podem ser **extraídos por fragmentos** ou **capturados como servos**. As ações são exclusivas, aparecem perto da personagem e ficam disponíveis por cerca de 14 segundos. Teclas F/E no Windows e dois botões no Android. [Regras e teste](docs/ESCOLHAS_CRIATURA_CAIDA_0_9_20.md).

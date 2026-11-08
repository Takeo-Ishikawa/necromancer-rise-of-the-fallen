# Compilações Unity independentes — Android e Windows

Este é o padrão de entrega do Necromancer a partir da versão 0.8.2. A base de código, a câmara e os recursos gráficos são os mesmos; **somente a plataforma escolhida pelo desenvolvedor será compilada**.

## A. Android — APK de teste

Workflow: [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml)

1. Abra o link e escolha **Run workflow** → branch `main` → **Run workflow**.
2. Após **Success**, abra a execução e baixe o artefato `necromancer-preview-apk`.
3. O arquivo é um APK direto, sem ZIP. Instale por cima da preview anterior.

A identidade `com.takeo.necromancer.unity.preview080`, a chave `necromancer-preview` armazenada nos Secrets e o incremento de `versionCode` foram preservados. O Android **não** é compilado automaticamente após commits; isso economiza o tempo de build. A assinatura de produção continua no workflow manual separado `unity-release.yml`.

## B. Windows — executável de teste

Workflow: [Unity Windows x64 Preview](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-windows.yml)

1. Abra o link e escolha **Run workflow** → branch `main` → **Run workflow**.
2. Após **Success**, baixe `necromancer-windows-x64` em **Artifacts**.
3. Extraia **todos os arquivos** do ZIP para uma pasta, mantendo o executável e a pasta `necromancer-windows_Data` juntos.
4. Abra `necromancer-windows.exe` no Windows 64 bits.

A Unity não produz um único EXE independente: a pasta `_Data` e eventuais bibliotecas também são necessárias. O GitHub fornece um ZIP para preservar essa estrutura. A versão Windows usa **Mono** e não requer Android SDK, keystore nem os Secrets de assinatura Android; usa somente os mesmos Secrets de licença Unity.

## C. O que acontece em cada commit?

- `unity-structure.yml`: continua executando testes estruturais rápidos, sem compilar o jogo.
- `unity-android.yml`: **manual**, gera só APK Unity, quando solicitado.
- `unity-windows.yml`: **manual**, gera só Windows, quando solicitado.
- `unity-release.yml`: manual, para release Android assinado.
- `android.yml`: histórico aplicativo Java/Canvas. Continua separado e só roda automaticamente quando arquivos do projeto `app/`, `tests/com/` ou Gradle mudarem; edições Unity não o acionam.

## D. Instruções permanentes para outras sessões

Não adicionar gatilho `push` nos workflows de builds Unity sem autorização. Não misturar `StandaloneWindows64` e `Android` num workflow de matriz automática. Não modificar a assinatura, o package ID ou o versionCode do Android ao configurar Windows. Ao acrescentar uma nova plataforma, criar script de build separado em `unity/Assets/Editor` e uma distribuição com os arquivos necessários. Acompanhar falhas na aba **Actions** e manter `tests/check_unity_project.py` atualizado.

O fluxo Windows ainda exige a primeira compilação de validação e teste no computador; compilar com sucesso não garante que a interface, os controles ou o salvamento estejam corretos em todos os dispositivos.

## 0.9.0 e versões seguintes

O painel de meditação e o sistema de atributos são compartilhados pelo build Windows e pelo Android. Ambas as plataformas continuam com workflows **manuais**; modificar scripts Unity não dispara automaticamente o build. O identificador e a assinatura da prévia Android permanecem estáveis, e o versionCode continua sendo incrementado pelo número da execução. Arquivos de save local do Windows e do Android são independentes.


### Skill tree 0.9.1

A árvore de seis habilidades utiliza um único módulo Unity e é incluída automaticamente nos builds **manuais** Android e Windows, sem criar um workflow adicional. Ambas as plataformas usam saves próprios; ambos aceitam arquivos da versão 0.9.0. Para testar, execute somente o workflow da plataforma pretendida.

### 0.9.3 — Pulso Necrótico

A nova habilidade ativa utiliza **Q** no Windows e um botão na interface Android, sem sprites extras. Para verificar o novo recurso, compile somente o workflow da plataforma que você vai testar; nenhuma compilação automática dos workflows Unity foi introduzida.


### 0.9.4 — Ordem dos Mortos

O comando temporário para servos é compartilhado entre Windows (tecla R / clique) e Android (botão ORDEM). Os dois workflows permanecem **manuais e independentes**. A versão de exibição foi atualizada para 0.9.4 e a assinatura/identificador da prévia Android não mudaram. O estado temporário do comando não altera o formato do save.


### 0.9.5 — Controle tático do exército

O modo Caça/Defesa tem tecla **T** no Windows e botão dedicado no Android, usa o mesmo save v1 e os mesmos assets da Unity. A seleção da plataforma permanece manual na aba Actions: nenhuma compilação automática foi adicionada.


### 0.9.6 — Organização dos servos

A lógica de formação e recuperação foi movida para `NecroServantFormation.cs`, compartilhado por Android e Windows. Ela não altera controles, formato do save, assinatura do APK nem os workflows manuais. Compile uma plataforma de cada vez quando for validar a nova movimentação em jogo.

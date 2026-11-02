# Gerar APK Android com Unity

Motor principal: Unity 2022.3.62f2, projeto na pasta `unity/`.

## Situação verificada (2 de novembro de 2026)
Executamos o workflow `Unity Android APK`. A validação da estrutura passou, mas a etapa do motor parou com:

> Missing Unity License File and no Serial was found.

Nenhum APK Unity foi produzido. O APK Android legado (`app/`, Java/Canvas) **não** corresponde a esta compilação.

## Método 1 — Compilar no seu computador (não exige secrets no GitHub)

1. Instale o Unity Hub e o editor **2022.3.62f2**, com **Android Build Support**, **Android SDK & NDK Tools** e **OpenJDK**.
2. Em Unity Hub, abra a pasta `unity/` do repositório.
3. Adicione as imagens do pacote `Necromancer_Unity_Arte_0_1.zip` em `unity/Assets/Resources/Backgrounds/`. Os PNGs têm os nomes exigidos pelo projeto.
4. A montagem de cena no editor cria `Assets/Scenes/NecromancerPrototype.unity` automaticamente; abra-a.
5. Em **File > Build Settings**, selecione **Android**, escolha **Switch Platform** e clique em **Build** para gerar o APK.
6. Esse APK será um protótipo, e o resultado visual ainda depende de converter as folhas de personagem em sprites jogáveis separados.

## Método 2 — Compilar pelo GitHub Actions

1. No seu computador, ative uma licença gratuita no **Unity Hub > Preferences > Licenses** ou use sua licença profissional. Não compartilhe sua licença nem suas senhas no chat.
2. Para Unity Personal, localize `C:\ProgramData\Unity\Unity_lic.ulf` no Windows (ou a localização equivalente no seu sistema).
3. No repositório, acesse **Settings > Secrets and variables > Actions > New repository secret** e cadastre:
   - `UNITY_LICENSE`: conteúdo completo do arquivo `.ulf`.
   - `UNITY_EMAIL`: e-mail da conta Unity.
   - `UNITY_PASSWORD`: senha da conta Unity.
4. Para Unity Pro, em lugar do arquivo pessoal, configure `UNITY_SERIAL`, `UNITY_EMAIL` e `UNITY_PASSWORD`, seguindo os termos da sua licença.
5. Vá até **Actions > Unity Android APK > Run workflow**. Quando a execução estiver marcada `success`, baixe o artefato `necromancer-unity-android-apk`.

Regras: jamais faça commit de arquivos `.ulf`, senha ou serial no repositório; use GitHub Actions Secrets. A licença é requisito externo, não é possível gerar um build Unity sem a ativação do editor.

Referências: https://game.ci/docs/github/activation/ e https://game.ci/docs/github/builder/

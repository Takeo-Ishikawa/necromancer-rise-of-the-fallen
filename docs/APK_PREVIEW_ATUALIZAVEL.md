# APK de teste instalável diretamente e atualizações sem desinstalar

Este procedimento vale para o Necromancer Unity e deve ser preservado em futuras sessões. O workflow .github/workflows/unity-android.yml oferece o APK individual (sem ZIP) e assina todas as versões de teste com **a mesma chave privada**. O identificador Android é fixo: `com.takeo.necromancer.unity.preview080`. Versões futuras (0.9, 1.0 etc.) **não podem modificar esse identificador apenas porque mudou o número da versão**.

## Configuração obrigatória de uma única vez — 2 Secrets

Em seu computador Windows, com o JDK / keytool instalado, crie uma chave de testes separada da chave definitiva usada para publicar o jogo na loja:

```powershell
keytool -genkeypair -v -keystore necromancer-preview.jks -storetype PKCS12 -keyalg RSA -keysize 3072 -validity 10000 -alias necromancer-preview -dname "CN=Necromancer Preview, O=Necromancer"
```

A ferramenta solicitará uma senha: escolha uma senha forte, anote em um gerenciador privado e faça backup do arquivo `necromancer-preview.jks`. **Não compartilhe a senha nem o arquivo da chave no chat ou no repositório.**

No GitHub, acesse o repositório → **Settings → Secrets and variables → Actions → New repository secret**. Crie:

- `PREVIEW_KEYSTORE_PASSWORD`: a senha que você escolheu para o arquivo JKS/PKCS12.
- `PREVIEW_KEYSTORE_BASE64`: a codificação Base64 do arquivo. Para copiá-la ao clipboard (PowerShell, na mesma pasta onde gerou o arquivo):

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes((Resolve-Path ".\necromancer-preview.jks"))) | Set-Clipboard
```

Cole no campo **Secret** de `PREVIEW_KEYSTORE_BASE64`. Guarde um backup da chave e senha fora do GitHub: perdê-las impedirá atualizar a mesma instalação no futuro.

O workflow verifica a assinatura com `keytool` antes de compilar; se os Secrets não estiverem configurados, a compilação falhará claramente, em vez de emitir um APK com assinatura de depuração temporária.

## Como baixar e atualizar

Abra [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml). Na execução mais recente marcada **Success**, abra **Artifacts** e selecione `necromancer-preview-apk`. O upload usa `actions/upload-artifact@v7` com `archive: false`, de forma que o download é um **APK direto** e não um ZIP; em celular, o arquivo pode ser aberto diretamente no instalador Android (autorize a instalação desta fonte, se necessário). O link do artefato exige permissões de acesso normais ao GitHub e expira conforme a retenção configurada.

Cada nova execução recebe automaticamente um **versionCode crescente** (`10000 + github.run_number`). Não mudar o ID do pacote nem recriar a chave de testes. O instalador Android oferece a opção de **atualizar** sobre a instalação existente, mantendo seus dados e progresso. Não requer desinstalação nem atualização automática em segundo plano: o download e a instalação continuam sendo ações manuais no aparelho.

## Por que uma migração pode ser necessária agora?

Os APKs anteriores foram assinados por certificados Android Debug gerados de forma **temporária** em cada runner. Esses certificados não são iguais à chave nova e duradoura. Logo, a primeira versão com assinatura persistente **não poderá atualizar o APK antigo com o mesmo ID de pacote**. **Pode ser necessário fazer uma transição única; não desinstale a versão anterior antes de avaliar/backupear o progresso.** Consulte o histórico e as instruções de backup no início de `docs/ANDROID_SIGNING.md`. Depois de instalar a primeira preview com a nova assinatura, as próximas atualizações usarão a mesma chave e não exigirão reinstalação.

Versões oficiais usam outro ID e outro fluxo, `unity-release.yml`, com credenciais `ANDROID_KEYSTORE_*`. Nunca confundir a chave preview com a chave de produção.

## Instruções para próximas sessões de desenvolvimento

1. Manter `com.takeo.necromancer.unity.preview080` em **todos** os próximos testes Unity; não adicionar sufixo de versão ao package ID.
2. Não alterar a chave ou o alias `necromancer-preview`, nem voltar a `useCustomKeystore=false` na preview.
3. Não sobrescrever `NECRO_PREVIEW_BUILD_NUMBER` com um número fixo; toda execução nova precisa ser instalável por cima.
4. Continuar a publicar o APK com `archive: false` e evitar distribuir somente ZIP.
5. Monitorar `tests/check_unity_project.py` e o status da compilação Unity; confirmar instalação e atualização no aparelho após mudanças de build.


## Correção de compilação (07/11/2026)

A execução GitHub Actions 42335563579 validou a chave de preview, porém falhou dentro da Unity porque a variável `NECRO_PREVIEW_BUILD_NUMBER` não atravessou o contêiner do GameCI v4. O workflow corrigido gera temporariamente um arquivo de configuração JSON privado **fora de `Assets`**, em `unity/.android-build-settings.json`, com modo, número da execução e dados de assinatura. Este arquivo é ignorado pelo Git, tem permissão restrita e é apagado ao final, inclusive se o build falhar. A Unity carrega esse arquivo e utiliza o número crescente para versionCode e a mesma assinatura persistente.

Para builds de produção, o workflow `unity-release.yml` segue o mesmo padrão com modo `release` e credenciais oficiais, sem misturá-las com a preview. **Nunca versionar nem compartilhar o JSON temporário, o keystore ou suas senhas**. A compilação deve preservar o upload do APK direto (sem ZIP) após o sucesso.

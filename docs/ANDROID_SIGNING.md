# Android APK: assinatura, prévia e migração do salvamento

## Causa da falha entre 0.5 e 0.6

Os APKs das versões 0.5 e 0.6 foram compilados com certificados `Android Debug` **diferentes**, porque o runner do GitHub gerou uma chave temporária em cada execução. O Android só permite atualização direta quando a assinatura é compatível.

As impressões SHA-256 dos certificados são:
- 0.5: `FE:CD:D7:6E:29:5B:B1:38:51:6F:6B:3F:A4:0A:F7:8D:1E:05:BB:CF:4C:11:EB:94:BE:C7:BD:76:93:DD:9A:8C`
- 0.6: `CD:95:FD:78:82:AA:B9:F2:4A:0E:C8:43:CB:C0:FC:28:5B:AC:5D:08:A8:75:7E:99:07:2F:75:07:0B:D6:9D:A2`

**A chave privada da versão 0.5 não pode ser derivada de seu APK.** Nunca desinstale a 0.5 antes de copiar seu salvamento.

## Prévia 0.6: instalar lado a lado

O workflow [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) cria o aplicativo `com.takeo.necromancer.unity.preview` com o nome **Necromancer (Preview 0.6)**. Ele pode ser instalado no mesmo telefone em que a versão 0.5 permanece, sem substituí-la. Seus dados são armazenados em um diretório próprio.

**A instalação paralela NÃO transfere automaticamente os dados da 0.5.** A prévia pode começar uma campanha nova ou receber uma cópia de save via ADB (passo abaixo).

## Fazer backup do save 0.5 pelo computador

Com o jogo 0.5 ainda instalado, habilite **Depuração USB** no Android e instale as [Android SDK Platform Tools](https://developer.android.com/tools/releases/platform-tools) no computador.

Na pasta do `adb`, abra PowerShell e conecte o aparelho, autorizando a chave RSA no próprio telefone.

```powershell
.\adb devices
.\adb pull "/sdcard/Android/data/com.takeo.necromancer.unity/files/necro-save-v1.json" ".\necro-save-v1.json"
.\adb pull "/sdcard/Android/data/com.takeo.necromancer.unity/files/necro-save-v1.json.bak" ".\necro-save-v1.json.bak"
```

**Confirme que o primeiro arquivo foi copiado, tem tamanho maior que zero, e guarde outra cópia dele.** O segundo arquivo é opcional. O acesso a `Android/data` pode ser restringido em alguns dispositivos; nesse caso, não desinstale e não tente contornar essas proteções sem um método de backup apropriado.

Para testar seu save na **prévia**, instale e execute a prévia uma vez, feche-a e use:

```powershell
.\adb push ".\necro-save-v1.json" "/sdcard/Android/data/com.takeo.necromancer.unity.preview/files/necro-save-v1.json"
```

Abra a prévia novamente e confira se todos os servos, níveis, inventário e a Cripta foram preservados. **O original 0.5 continuará intacto**.

## Assinatura permanente para as versões de produção

É necessário criar **uma única chave privada de produção** e conservá-la por toda a vida do jogo. O workflow [Unity Signed Android Release](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-release.yml) exige os seguintes Repository Secrets em **Settings > Secrets and variables > Actions > New repository secret**:

- `ANDROID_KEYSTORE_BASE64`: conteúdo base64 de um arquivo de assinatura (.jks)
- `ANDROID_KEYSTORE_PASSWORD`: senha do keystore
- `ANDROID_KEY_ALIAS`: nome do alias
- `ANDROID_KEY_PASSWORD`: senha da chave

A chave deve ficar armazenada em backup privado, **nunca no repositório**. Exemplo de geração no computador com JDK (guarde arquivo e senhas em local seguro):

```powershell
keytool -genkeypair -v -keystore necromancer-release.jks -storetype PKCS12 -keyalg RSA -keysize 3072 -validity 10000 -alias necromancer
[Convert]::ToBase64String([IO.File]::ReadAllBytes(".\necromancer-release.jks")) | Set-Clipboard
```

A saída do segundo comando é colocada na área de transferência e deve ser inserida somente no segredo `ANDROID_KEYSTORE_BASE64`. Não compartilhe a chave ou as senhas no chat.

Depois de cadastrar os secrets, execute manualmente o workflow **Unity Signed Android Release**. Toda versão oficial futura deverá usar **o mesmo** arquivo .jks. O workflow rejeita builds de produção quando os segredos não estão configurados, para impedir uma nova atualização impossível.

### Atenção à transição a partir da 0.5

A nova chave persistente também **não será igual** à assinatura da 0.5; portanto, uma migração única será necessária para a versão oficial. Antes disso, salve o arquivo JSON. Depois de confirmar um backup recuperável, será possível remover a 0.5, instalar uma edição oficial e copiar o JSON de volta à pasta do aplicativo `com.takeo.necromancer.unity` com ADB. As versões oficiais subsequentes poderão ser instaladas como atualização, desde que usem a mesma chave persistente.

## Compilação local

Abra `unity/` no Unity 2022.3.62f2 e importe o Android Build Support. O código de build escolhe `preview` por padrão, e só usa o identificador oficial com `NECRO_ANDROID_MODE=release` e a chave privada válida. O APK da prévia é destinado apenas a testes.

Em ambos os casos, os controles, as quatro regiões, a Cripta e o formato de save permanecem iguais.

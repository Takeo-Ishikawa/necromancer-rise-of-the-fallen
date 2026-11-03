# Necromancer — Unity Android Preview 0.6.4

**Unity Editor:** 2022.3.62f2 LTS · Android IL2CPP · ARM64 + ARMv7.

## Alterações de jogabilidade solicitadas

### Inimigos reaparecem

Depois que uma região é conquistada, **monstros comuns reaparecem continuamente**. O primeiro respawn ocorre após aproximadamente 6 segundos, e os seguintes em intervalos entre 7,5 e 11,5 segundos. Eles surgem a uma distância aproximada de 5 a 8,5 unidades do necromante, sem atravessar obstáculos nem aparecer em cima dos servos. Existe limite de **10 a 16 inimigos simultâneos**, dependendo do mapa. Cada região usa apenas as espécies comuns definidas para ela. Os chefes especiais permanecem únicos; sua morte e conquista não são reiniciadas.

As regiões ainda não conquistadas seguem a progressão original: um mapa deve poder ser limpo para desbloquear o selo. Os novos monstros não impedem essa conquista porque o respawn começa somente **depois** da vitória. Mapas já concluídos em saves antigos voltam a ter monstros quando o jogador retorna ou continua a partida. O estado das criaturas vivas continua sendo salvo normalmente.

### Exército maior

Capacidade aumentada de **4 para 12 pontos de comando**. Isso permite até 12 criaturas de custo 1 simultaneamente ou combinações de unidades maiores: slimes/esqueletos custam 2, guardiões custam 3 e o chefe do pântano custa 4. A interface da Cripta, o salvamento e a retirada dos servos utilizam automaticamente esse novo limite. O menu do exército agora tem páginas para consultar mais aliados.

### Ataque automático

O botão GOLPE/MAGIA foi **removido** da HUD. O necromante ataca automaticamente o inimigo mais próximo dentro do alcance e com linha de visão. Antes do primeiro despertar mágico, usa golpe físico de curta distância. Depois, lança orbes de magia automaticamente quando tem ânima suficiente, respeitando o intervalo dos ataques. Sem ânima, pode golpear fisicamente os inimigos que se aproximarem. Não é necessário tocar na tela para atacar.

A necromancia permanece **ativa**: REVIVER continua sendo um botão e exige aproximar-se de um cadáver. Os controles DEFESA/CAÇA e EVOLUIR também continuam. A parte direita da HUD exibe 'ATAQUE AUTOMÁTICO'.

## Compatibilidade de instalação e salvamento

Esta é outra **prévia independente** com o identificador `com.takeo.necromancer.unity.preview064`, para evitar conflito de assinatura com versões antigas. **Não desinstale a 0.5**, nem as outras prévias que você queira preservar. O progresso da 0.5 não é transferido automaticamente: a instalação usa diretório próprio.

O **formato do JSON de save não mudou**, preservando o layout de níveis, criaturas, equipamentos, Cripta e rituais. A reprodução dos inimigos se recalcula a partir da região e não precisa alterar o formato salvo. O guia de assinatura permanente e migração está em [docs/ANDROID_SIGNING.md](../docs/ANDROID_SIGNING.md).

## Testes

O workflow GitHub [Unity Android Preview APK](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml) executa as verificações estáticas, o teste de quadros no Unity Editor e a compilação do APK.

Ao instalar no celular, confira particularmente os inimigos surgindo novamente em áreas conquistadas, o funcionamento do ataque automático e se consegue invocar mais do que quatro pontos de comando. A validação em aparelho físico permanece necessária.

Os sprites procedurais por quadros, cenários, UI ilustrada e animações da 0.6.3 continuam presentes.

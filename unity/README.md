# Necromancer — Unity 2D / Android 0.6

**Unity:** 2022.3.62f2 LTS · **Android APK:** IL2CPP, ARMv7 + ARM64.

Esta versão **implementa a primeira etapa do conceito de arte 0.6 no jogo real**, não apenas numa imagem.

## Novidades de implementação

- **Sprite real da necromante:** `Assets/Resources/Sprites/necromante.png` é uma arte transparente extraída do conceito aprovado, adaptada e reduzida para leitura em dispositivos móveis. `NecroArt.Get("necromante")` carrega o PNG pelo Unity e faz fallback para a versão procedural se o arquivo faltar.
- **Animação leve:** `NecroCharacterAnimation.cs` separa a imagem do transform usado em colisões. Implementa bob da caminhada, respiração, orientação horizontal, brilho na conjuração e círculo rúnico ao ressuscitar. São efeitos temporais, **não** um sprite sheet completo de múltiplos quadros.
- **Nova HUD de combate:** `NecroPolishedHUD.cs` desenha retrato, vida e ânima, XP, nível, fragmentos, estado dos servos, título da fase e objetivo, minimapa funcional, joystick analógico e quatro botões de ação.
- **Controles com toque:** uma única identificação de dedo acompanha o joystick. Toques no minimapa, barra inferior e habilidades não causam deslocamento acidental.
- **Menus funcionais:** botões de Servos, Cripta, Rituais e Equipamentos abrem a interface apropriada ou indicam que o jogador deve retornar ao santuário. O menu da Cripta e os rituais continuam funcionando.
- **Preservação do progresso:** as estruturas JSON e `Application.persistentDataPath` da versão 0.5 foram mantidos. O **ID do aplicativo permaneceu o mesmo** (`com.takeo.necromancer.unity`), permitindo atualização sobre a instalação anterior quando a assinatura coincidir.

## Como testar

Instale o APK atualizado sem desinstalar o anterior. Verifique se o jogo abre com a personagem ilustrada e sua nova HUD, se o joystick movimenta com um único dedo e se Magia, Reviver, Defesa/Caça e Evoluir continuam funcionando. Teste também os botões inferiores e o acesso à Cripta a partir do santuário. Feche e abra o jogo para confirmar o salvamento.

A arte final dos cenários, dos monstros e o **sprite sheet completo de caminhada em quatro direções** ainda estão reservados para próximas etapas da 0.6. Os desenhos de referência em tamanho integral permanecem separados dos assets finais. Não foi implantado o mockup inteiro como pano de fundo, pois isso impediria a interação e a câmera.

## Compilação

Abra a pasta `unity/` em Unity 2022.3.62f2 com Android Build Support ou utilize o [Unity Android APK workflow](https://github.com/Takeo-Ishikawa/necromancer-rise-of-the-fallen/actions/workflows/unity-android.yml). O pipeline verifica a integridade do PNG e dos scripts e gera o APK para teste. O salvamento local e as mecânicas das versões anteriores são mantidos.

**Limitação de validação:** o pipeline verifica a compilação e integridade dos arquivos, mas as animações, dimensionamento da UI e controles precisam ser avaliados no telefone.

# Necromancer 0.9.22 — troca de personagens durante o jogo

## Correção da Necromante Sombria

A tela de seleção de 0.9.21 rejeitava a nova personagem quando o Unity alterava a resolução importada de sua spritesheet 768 × 1024 (formato NPOT), mesmo com o arquivo presente. O carregamento agora valida a disponibilidade da spritesheet e dimensões mínimas, e o fatiamento 8 × 6 calcula a dimensão real de cada célula. Um AssetPostprocessor dedicado impede o Unity de converter as imagens da **Necromante Sombria** para potência de dois no Windows/Android sem interferir nas outras três personagens. Uma verificação C# de pré-build exige proporção 3:4 e 48 quadros.

O retrato, antes reduzido em um enquadramento estreito com grandes margens, agora usa um recorte proporcional da arte original na área de pré-visualização. Nenhuma imagem dos personagens existentes foi modificada.

## Menu no mundo

Um botão **PERSONAGEM** fica no canto esquerdo do HUD, abaixo da navegação e afastado de ataque, inventário e joystick. Ele abre a mesma tela de seleção durante a partida, com **USAR PERSONAGEM** e **CANCELAR**. A tecla Escape também cancela no Windows.

A seleção é apenas visual até a confirmação: escolher uma carta não altera os poderes no mundo. Ao confirmar, o jogo salva o progresso, substitui os sprites e habilidades do avatar, mantém a posição atual, equipamentos, atributos, pontos, servos, recursos, fases e objetos ativos, sem reconstruir o mundo nem recarregar o cenário. O modelo anterior é desativado/destruído, junto com os sprites fatiados dinamicamente, para evitar personagens duplicados e vazamentos de memória.

A personagem escolhida continua registrada em PlayerPrefs `necro.selected-character.v1` e o estado do mundo continua usando o save **v1**. A troca de personagem física para mágica e vice-versa preserva as limitações do tipo de combate da personagem escolhida.

## Roteiro de teste

1. Na abertura, selecione a Necromante Sombria e pressione CONTINUAR JORNADA; ela deve entrar normalmente no refúgio, sem mensagem de arte indisponível.
2. Confira o retrato ampliado, mantendo proporções naturais, e o movimento nas oito direções com seis quadros cada.
3. Dentro do refúgio ou da caverna, toque em **PERSONAGEM**; escolha Arlequina e pressione **CANCELAR**. O avatar original deve continuar ativo, sem mudar equipamentos/atributos.
4. Abra o menu novamente, escolha Arlequina e pressione **USAR PERSONAGEM**. O sprite deve mudar no mesmo lugar, com os servos e recursos intactos.
5. Volte à Necromante Sombria e teste combate corpo a corpo, captura de criatura e a passagem entre regiões.
6. Saia e reabra o app: o personagem usado por último e o progresso anterior devem continuar disponíveis.

Build **0.9.22**; Android release versionCode **30**; preview mantém identificador e assinatura, workflows Windows e Android continuam separados e manuais.

**Validação restante:** compilar no Unity e testar no aparelho; o CI estrutural não simula a importação real nem a execução do menu.

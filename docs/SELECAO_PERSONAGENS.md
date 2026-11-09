# Tela inicial de seleção de personagens

A abertura do jogo exibe a necromante atual, sua apresentação e dois espaços
“Em breve”. O botão Iniciar jornada (ou Continuar jornada quando há save válido)
abre a câmara segura e restaura o progresso pelo fluxo existente.

## Comportamento

- Clique/toque nos personagens disponíveis; Enter inicia no computador.
- Setas esquerda/direita percorrem apenas personagens disponíveis.
- Lista horizontal rolável para acomodar mais personagens.
- Painel central ajustado à área segura da tela, incluindo janelas horizontais.
- O mundo só é criado após a confirmação: não há combate, joystick ou autosave
  rodando por trás da seleção. Fechar o aplicativo antes de jogar não escreve o save.
- A escolha usa a preferência `necro.selected-character.v1`; o save v1 existente,
  equipamentos, recursos, servos e chaves de assinatura permanecem compatíveis.

## Adicionar personagens

O catálogo fica em `unity/Assets/Resources/Data/characters.json`. Cada entrada tem
ID estável, nome, subtítulo, descrição, disponibilidade e caminhos Resources para
as folhas de animação (sem extensão). IDs devem ser únicos. Catálogo inválido
recorre à necromante padrão; escolha removida ou bloqueada recorre à primeira disponível.

As folhas seguem o contrato atual: idle RGBA 1024×512, oito poses em células 256×256;
spectral RGBA 1024×2048, com a mesma organização de caminhada, ataque e projétil
da necromante atual. A prévia usa a primeira pose frontal da folha idle.
Ao iniciar, folhas ausentes ou com dimensões incorretas impedem a entrada no mundo.

Esta etapa prepara identidades e arte, compartilhando as mecânicas e o progresso
da jornada existente. Antes de adicionar classes com atributos/habilidades próprios
ou campanhas independentes, ampliar a aplicação de atributos e a persistência;
não basta marcar uma entrada como disponível para criar uma classe diferente.

## Validação

`python tests/check_unity_project.py` verifica o projeto e os novos contratos de
catálogo, folhas de animação e bloqueio de inicialização/controles.

Teste manual na próxima compilação Unity Android ou Windows:

1. Abrir com e sem save: verificar título, personagem, espaços bloqueados e botão.
2. Deixar a seleção aberta e fechar/reabrir: o progresso anterior deve permanecer.
3. Iniciar por toque/clique e Enter: entrar uma única vez na câmara, sem movimento residual.
4. Conferir nível, fragmentos, equipamentos e servos do save anterior.
5. Testar movimento, entrada da caverna, ataque, inventário e estações da câmara.
6. Redimensionar a janela e verificar recortes de tela no Android.

Não há nova imagem pesada nem mudança nos workflows de compilação separados.

## Arlequina do Luto — primeira versão jogável

A Arlequina está disponível ao lado da necromante. A seleção é lembrada ao reabrir.
As duas aparências compartilham o progresso, equipamentos, atributos e habilidades;
a seleção não cria nem apaga campanhas.

Os atlas `Characters/Arlequina/idle.png` e `spectral.png` têm células RGBA de
256×256, escala uniforme e base dos pés em y=242. A ordem de direções é
S, SO, O, NO, N, NE, L, SE, conforme `DirectionFrom`.

A arte aprovada está em `tools/arlequina-source.png`; execute
`python tools/prepare_arlequina.py` (Pillow) para reproduzir os atlas. O empacotador
mantém transparência e reutiliza somente a célula do projétil da necromante.

Caminhada corrigida: duas poses de contato para cada uma das oito direções,
intercaladas com a pose neutra em um ciclo de quatro fases. A nova fonte é
`tools/arlequina-walk-source.png`. O empacotador substitui todos os pixels RGBA
nas células ocupadas; compor transparência sobre uma pose anterior causava as
pernas duplicadas. O soco lateral e a pose de invocação continuam simplificados.
A necromante original mantém seu ciclo existente.

Validação adicional: `python tests/check_arlequina_atlas.py` (Pillow) testa
substituição completa de RGBA, isolamento de células, margens e existência de
passos diferentes em todas as direções. A conferência visual dos quadros é feita
fora do Unity; fluidez e escala no aparelho ainda precisam de teste na compilação.

Na próxima compilação, testar cada personagem, reabrir para conferir a escolha,
andar nas oito direções, atacar dos dois lados, erguer um servo e confirmar que
nível, fragmentos e equipamentos permanecem ao trocar de personagem.

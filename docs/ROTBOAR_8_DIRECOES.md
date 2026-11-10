# Rotboar — integração de oito direções

O backup exato dos sprites e scripts antigos já existe em `archive/rotboar/pre_8direcoes_2026-11-10/` (commit `fdd38e8`).

O jogo agora aceita até oito novas folhas de sprites, cada uma com **4 colunas × 8 linhas de 192 × 192**, em RGBA PNG (dimensões: **768 × 1536**), na pasta:

`unity/Assets/Resources/Creatures/`

Arquivos (nomes exatos):

- `rotboar_move.png`
- `rotboar_attack.png`
- `rotboar_death.png`
- `rotboar_reanimate.png`
- `rotboar_ally_move.png`
- `rotboar_ally_attack.png`
- `rotboar_ally_death.png`
- `rotboar_ally_reanimate.png`

Linhas de cima para baixo: sul, sudeste, leste, nordeste, norte, noroeste, oeste, sudoeste. Quatro quadros por linha; os 32 são carregados uma vez e compartilhados por todas as instâncias.

**Proteção:** a falta de qualquer atlas não causa falha de compilação; a lógica retorna aos PNG antigos para aquele estado. A compilação do jogo com os sprites novos só ocorrerá depois que os PNGs estiverem no GitHub. A morte preserva a janela atual de 14 segundos de recuperação do cadáver; o servo reanimado mantém o ataque de investida e o comportamento de seguimento existentes.

A primeira versão das imagens originais geradas foi reorganizada em arquivos próprios para Unity, preservando transparência. O ZIP de entrega contém os oito PNGs, prontos para serem adicionados nesse diretório.

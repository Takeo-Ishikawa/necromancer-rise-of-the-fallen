# Rato corrompido — sprites 2D

A espécie `rat` usa o mesmo conceito visual sombrio do Rotboar, sem alterações nas regras de combate, colisões, coleta ou ressuscitação.

As imagens sob `unity/Assets/Resources/Creatures/` são otimizadas para dispositivos móveis, usam fundo transparente e células quadradas de 128 pixels.

- `rat_move.png`: 512×1024, 32 quadros, 4 colunas × 8 linhas. As linhas representam, de cima para baixo: baixo, baixo-direita, direita, cima-direita, cima, cima-esquerda, esquerda e baixo-esquerda.
- `rat_attack.png`: 1024×128, oito quadros de ataque lateral (espelhados ao atacar à esquerda), duração 0,48 s.
- `rat_death.png`: 1280×128, dez quadros de morte lateral, duração 0,56 s. O último quadro permanece exibido enquanto o cadáver for coletável/ressuscitável.

`NecroRatAtlas` importa as texturas por `Resources.Load<Texture2D>`, recorta e mantém em cache os `Sprite`s. A classe `NecroCreatureView` escolhe a direção pelo deslocamento e usa as animações de ataque e morte nos estados existentes. Se um PNG estiver ausente ou inválido, o jogo mantém o sistema procedimental antigo.

Limite: a movimentação possui oito direções pintadas, mas os sprites de ataque/morte foram produzidos em vista lateral. Testes reais de compilação e de aparelho ainda são necessários.

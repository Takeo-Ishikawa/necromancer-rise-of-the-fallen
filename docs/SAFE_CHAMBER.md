# Câmara segura — arte modular v4 (novembro de 2026)

A câmara inicial usa agora uma pintura vazia original e móveis ilustrados independentes, de acordo com a referência aprovada. Está incorporada ao projeto Unity: **não é necessário importar um ZIP ou baixar assets adicionais para compilar**.

## Recursos incluídos

- `unity/Assets/Resources/Backgrounds/safe_chamber_v2.jpg`: piso, paredes, rochas e passagem natural, sem móveis nem personagem.
- `unity/Assets/Resources/SanctuaryProps/entrance_arch.png`: arco de pedra vazado sobre a passagem.
- `banner.png` (instanciado 2 vezes), `lantern.png` (instanciado 2 vezes), `bed.png`, `fountain.png`, `wardrobe.png`, `meditation_altar.png` e `mount_nest.png`: PNGs RGBA independentes.
- `unity/Assets/Resources/Data/sanctuary.json`: mapa de 16 por 24 unidades, início próximo ao sul (8, 5.55), sem inimigos.
- `unity/Assets/Resources/Data/sanctuary_props.json`: posições, dimensões, camadas, sombras e colisões ajustáveis.

Cada decoração possui um GameObject e SpriteRenderer próprios. As colisões ficam restritas às bases dos móveis. O altar inclui um tapete pintado: `sortY` altera apenas a profundidade de renderização, sem deslocar o sprite nem sua hitbox. As pedras, a cama, a fonte, o guarda-roupa e o ninho não ficam incorporados ao fundo.

A câmera do refúgio utiliza um enquadramento mais amplo (`orthographicSize` até 11.7) sem modificar o zoom usado nas fases de combate.

## Como alterar a composição

Basta editar `sanctuary_props.json`: `x` e `y` são âncoras no mundo; `width`/`height` definem o tamanho do PNG; `layer` distingue piso, parede e móveis; `sortY` é uma âncora opcional para ordenação; `blockOffsetX`, `blockOffsetY`, `blockWidth`, `blockHeight` definem as áreas de colisão; e `shadowWidth`, `shadowHeight`, `shadowOffsetX`, `shadowOffsetY` definem a sombra suave.

## Teste recomendado

1. Atualizar a branch `main` e abrir `unity/` no Unity 2022.3.62f2.
2. Gerar o APK pelo fluxo Android do projeto (ou pelo workflow GitHub Actions já existente).
3. Validar que o fundo novo aparece, que os oito arquivos PNG carregam sem mensagens `Safe chamber sprite missing`, e que os dois estandartes e as duas lanternas estão presentes.
4. Conferir aproximação/oclusão em cada móvel, movimentação dos servos, colisão junto ao altar e ao ninho, entrada ao norte e retorno do estágio zero.
5. Confirmar que combate, saves e fases anteriores permanecem inalterados.

Os sprites são uma primeira versão ilustrada extraída de uma prancha de conceitos; as proporções podem ser refinadas depois do teste em aparelho, sem reescrever o código. A compilação e os testes Android ainda não foram executados nesta sessão.

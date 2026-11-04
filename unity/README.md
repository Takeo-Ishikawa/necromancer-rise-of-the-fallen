# Necromancer Unity — Revisão visual 0.6.5c

Este release inicia o **pipeline de assets raster separados do código**, mantendo o jogo na Unity 2022.3.62f2. É a primeira revisão da Caverna Esquecida em direção ao **2D ilustrado sombrio**, sem alterar a jogabilidade.

## Alterações implementadas

- **Textura PNG de chão:** `Assets/Resources/Backgrounds/entrada_da_caverna_bonehollow.png` (512×512) substitui o antigo piso desenhado durante a execução. É repetida em escala nativa pelo SpriteRenderer Tiled, em vez de esticar um pequeno sprite por toda a caverna. Mantém o caminho amplo e cores frias suaves.
- **Rochas e cristais PNG:** `Assets/Resources/Scenery/rock_cluster.png` e `crystal.png` (256×256, fundo transparente) substituem parte dos objetos procedurais. Os elementos usam imagens de recursos importáveis, com fallback para o artefato antigo se ausentes. Reduzimos o número de decorações espalhadas pela caverna de 25 para 14.
- **HUD compactada:** topo com 89 unidades virtuais (anteriormente 116), retrato e minimapa menores, títulos discretos; a mensagem permanente de região conquistada foi substituída por avisos temporários. Joystick e botões redimensionados com seus pontos de toque atualizados. Não há botão de ataque.
- **Presença no cenário:** sombra de contato suave sob a necromante e uma aura discreta no cajado, que reage a conjuração e ressurreição, sem exigir URP ou um material de renderização novo.

As ilustrações PNG foram preparadas como recursos estáticos independentes do runtime; **ainda não representam a arte final pintada manualmente em alta resolução do conceito**. Esta implementação estabelece a arquitetura e melhora o primeiro bioma. Não se deve dizer que a URP já está ativada: primeiro é necessário confirmar pipeline/renderers/shaders e desempenho no Android.

## Pipeline dos próximos assets

- Fundo e cenário: importar PNG em `Resources/Backgrounds/` e `Resources/Scenery/`; usar transparência em objetos e pivô de contato em torno de 37% de altura.
- Personagens: folhas de sprites ilustradas em `Resources/AnimationSheets/`, com poses compatíveis com `NecroFrameLibrary`. Os protótipos procedurais ficam como fallback.
- UI: substituir progressivamente as molduras/imagens da HUD por PNGs com alta densidade de pixels; o layout compacto já separa esses elementos.
- Iluminação: após a revisão do pipeline, avaliar Universal Render Pipeline 2D com `Sprite-Lit-Default` e `Light2D`, medindo FPS em aparelhos Android. Este release usa halo translúcido de baixo custo.
- Antes de adicionar novos biomas, validar captura da primeira caverna e distância entre personagens, decoração e controles.

## Compatibilidade

A prévia usa `com.takeo.necromancer.unity.preview065c`; instala separada da 0.5 e das outras prévias por causa de suas assinaturas independentes. O salvamento começa isolado. **Não desinstale a 0.5.** Nenhum campo do save ou mecânica do jogo foi modificado.

Os testes verificam integridade dos PNGs, integração no Editor, dimensões e compilação. A aprovação do build não substitui avaliar a legibilidade visual no celular.

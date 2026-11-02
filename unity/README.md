# Necromancer: Rise of the Fallen — Unity 2D (protótipo inicial)

**Motor:** Unity 2022.3.62f2 (Editor 2D). Esta pasta substitui a plataforma de desenvolvimento para as próximas versões. A implementação anterior em Java/Canvas permanece no diretório `app/` como referência e backup.

## Abrir e jogar

1. No Unity Hub, escolha **Add project from disk** e selecione a pasta **unity/**.
2. Abra com Unity 2022.3.62f2 (ou outra versão 2022.3 LTS compatível).
3. Aguarde a compilação dos scripts: o gerador de cena cria automaticamente `Assets/Scenes/NecromancerPrototype.unity`.
4. Abra a cena e clique **Play**. WASD ou setas movimentam; Espaço lança magia/golpe; E ressuscita; Q alterna ordens; Enter abre os túneis após a conquista. Há controles na tela para toque.
5. Para Android, em Build Settings selecione Android e faça Build após instalar o suporte Android no Unity Hub.

## Arte das primeiras fases

Copie as 3 imagens do pacote gráfico para a pasta `Assets/Resources/Backgrounds/`, renomeadas exatamente:

- `entrada_da_caverna_bonehollow.png` (fase 1, área hostil)
- `santuario_necromante_bonehollow.png` (fase 1 conquistada, base)
- `tuneis_inferiores_enredados.png` (fase 2, túneis)

As imagens ficam no chão da cena; inimigos e personagem são desenhados por sprites independentes (provisórios no código). Atenção: as ilustrações de conceito ainda possuem algumas criaturas desenhadas no próprio fundo; a arte definitiva exigirá fundos sem criaturas e sprite sheets originais.

Para trocar os sprites provisórios, adicione PNGs transparentes em `Assets/Resources/Sprites/` com os nomes `necromante.png`, `rat.png`, `bat.png`, `slime.png`, `spider.png`, `snake.png`, `warden.png`.

## Estado

Protótipo inicial em Unity C#: navegação top-down, câmera, inimigos, servos com ataque automático, magia, cadáveres ressuscitáveis, pontuação de comando, mudança da primeira caverna para base e segunda área com cobras/aranhas. Os sistemas mais avançados do Android 0.4 (equipamentos e evolução) ainda NÃO estão migrados. Projeto Unity ainda NÃO compilado no Editor nesta execução; não há APK Unity validado.

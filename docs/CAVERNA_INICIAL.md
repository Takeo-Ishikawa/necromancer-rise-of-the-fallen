# Caverna esquecida — cena 0.4
Cenário inicial ampliado para **3072 × 2048 unidades**, com centro livre e colisões apenas em rochas selecionadas.
O renderizador `CaveScene.java` funciona no Android Canvas nativo: se houver `app/src/main/assets/caverna_inicial.png`, desenha o PNG; se não houver, usa uma arte vetorial de contingência.
O arquivo `assets/caverna_inicial_map.json` descreve as coordenadas e camadas, sem dependências extras.
A imagem PNG pode ser adicionada posteriormente ao caminho indicado (precisa ser copiada ao repositório).
Após derrotar a sentinela e eliminar as criaturas restantes, `baseClaimed` ativa o círculo ritual, as bancadas e a decoração de refúgio.
Ao reviver a sentinela, a conquista é confirmada. A tela `ENTRAR NA BASE` permite voltar ao próprio mapa sem reiniciar.
As rochas coincidem com a simulação `World.rocks`. Decorações não causam colisão e não obstruem servos.
Ratos, limos e morcegos continuam como inimigos iniciais; aranhas e cobras serão acrescentadas em etapa posterior.

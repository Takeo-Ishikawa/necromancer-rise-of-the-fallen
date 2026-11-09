# Necromancer 0.9.16 — Exploração e apresentação das recompensas

Esta atualização melhora a leitura dos recursos liberados na versão 0.9.15, sem mudar receitas, preços, taxas de coleta, danos, economia ou condições de salvamento.

## Identificação dos recursos

- Cristais, ossos e fungos **próximos à necromante** recebem um brilho suave e uma variação de escala de até **7%**. Seus sprites existentes são reutilizados; não há texturas, componentes ou objetos gráficos extras por quadro.
- O painel de progresso mostra, quando não há pontos de altar pendentes, um indicador do tipo de recurso visível mais próximo e a distância aproximada. O alcance de descoberta é de **7 unidades**, condicionado à linha de visão; além disso, o texto sugere explorar.
- Quando não restam recursos na região, o painel indica que todos foram coletados.
- O destaque ocorre apenas em regiões de combate; itens afastados recuperam a escala e as cores normais. Um novo capítulo ou carregamento utiliza as mesmas regras.

## Recompensa sem interromper combate

- Coletar recursos agora exibe uma pequena notificação verde-azulada próxima ao topo da tela por **2,2 segundos**.
- Ela mostra o total de fragmentos ganhos e os valores de vida e ânima **realmente restaurados**. Quando a vida já está cheia, não diz que o personagem foi curado.
- Coletas sucessivas durante a notificação somam os ganhos e a quantidade de recursos. A coleta não substitui mensagens importantes de combate ou progressão.
- Um efeito espectral existente marca a coleta uma vez; não são emitidos efeitos a cada quadro.

## Compatibilidade e desempenho

Os dados `NecroStageRecord` e o arquivo de save v1 são mantidos. A animação usa os `SpriteRenderer` e `Transform` já existentes nos recursos; não cria novos objetos repetidamente. A busca por dicas tem intervalo de **0,28 s**, em vez de refazer linhas de visão em todos os quadros. O sistema de coletas e `SaveProgress()` continua reunindo as mudanças do mesmo quadro em uma única gravação.

A versão de build passa para **0.9.16** e o versionCode de release Android para **24**; a preview mantém seu identificador e seu versionCode automático baseado no número da execução. Compilações Android e Windows permanecem independentes e manuais.

## Como testar

1. Entre na primeira caverna e aproxime-se de um cristal; observe o brilho discreto e a indicação de distância no topo.
2. Recolha o cristal: confirme `+2 FRAGMENTOS` e somente a vida/ânima realmente recuperadas.
3. Recolha outros recursos em sequência e confira a soma na notificação.
4. Durante mensagens de combate, verifique que a coleta não apaga os avisos já exibidos.
5. Afaste-se, volte, atravesse portas de caverna e feche/abra o jogo; confirme que os itens coletados continuam ausentes e que o brilho não altera o posicionamento dos sprites.
6. Abra o guarda-roupa com os fragmentos obtidos, verificando o aprimoramento do manto.
7. Compile manualmente a plataforma desejada; os testes estruturais não equivalem a uma compilação Unity nem a testes visuais.

**Situação ao integrar:** código, testes estáticos e verificações C# de pré-build incluídos; compilação Unity e teste de execução ainda necessários.

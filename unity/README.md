# Necromancer — 0.6.5b · Protagonista legível e repouso/caminhada em quatro direções

Esta entrega implementa um novo padrão de arte da protagonista no Unity, concentrado em **legibilidade no celular** e animações de repouso/caminhada.

## Correção observada na captura Android

A 0.6.5a utilizava uma folha PNG com apenas 124×184 pixels para **16 poses**, ou aproximadamente **31×46 pixels por personagem**. O aumento feito pelo Unity borrava o chapéu, o rosto e o cajado, fazendo a protagonista parecer menos definida que os monstros.

A nova fonte de arte em `Assets/Scripts/NecroHeroPaintedSource.cs` é original, com cores e contornos definidos para a leitura durante a jogabilidade. Agora cada pose começa numa célula desenhada de **160×160 pixels**, mantendo o chapéu violeta e a caveira, o cabelo prateado, o manto escuro e o cajado com núcleo ciano.

São quatro vistas distintas e duas poses de repouso mais duas de caminhada **em cada direção**. A biblioteca usa essas 16 células, sem esticar uma visão frontal para representar costas ou laterais. `NecroHeroSheetBuilder.cs` mantém um atlas de 96×96 pixels por quadro, e organiza os seis estados originais; os intervalos de repouso e caminhada são construídos a partir dos novos desenhos com deslocamentos leves, sem distorção da face.

As animações continuam ligadas ao `NecroCharacterAnimation.cs`. O sprite da personagem ficou aproximadamente 13% maior, sem alterar a física nem as colisões. O retrato circular da HUD passou a usar a própria nova arte do jogo, no lugar da PNG anterior.

## Estado da arte

O novo desenho de origem é rasterizado uma única vez em uma textura transparente de 640×640 pixels pela classe C# instalada no Unity. Não é necessário importar a arte de um site ou anexar um screenshot como plano de fundo.

As sequências de ataque, magia, ressurreição e dano ainda reutilizam as poses de repouso com efeitos específicos; **esta fase cobre especialmente idle e walk**, conforme o escopo definido. A etapa seguinte poderá produzir poses exclusivas também para as outras ações e aprimorar a qualidade visual da caverna/HUD.

## Segurança de instalação

A prévia `com.takeo.necromancer.unity.preview065b` é uma **instalação separada** das versões 0.5, 0.6.4 e 0.6.5a para preservar seus dados. Ela não importa automaticamente o salvamento das versões anteriores. Não desinstale a versão cuja partida queira conservar.

Nenhuma regra de combate, custo de ânima, comando de exército, Cripta ou formato de salvamento foi alterada.

## Verificação

O Editor Unity gera o atlas e verifica as quatro vistas e as 16 poses. O script `tests/check_unity_project.py` valida a presença do gerador e a integração. A compilação Android confirma a integridade técnica, mas a qualidade artística e o conforto de movimento devem ser julgados no telefone.

# Necromancer — Unity 0.6.3: animação por quadros

**Implementado no motor Unity 2022.3 LTS**, para Android ARMv7/ARM64. A versão 0.6.3 substitui a animação por oscilação simples por **quadros distintos armazenados em atlases de sprites**.

## Animação real integrada ao gameplay

A biblioteca `NecroFrameLibrary.cs` constrói atlases de frames a partir das ilustrações originais, mantendo o visual das criaturas e da necromante. Cada quadro é uma `Sprite` independente acessada pela `SpriteRenderer.sprite`, e o atlas é gerado apenas uma vez por espécie.

- **Protagonista:** quatro direções (frente, costas aproximadas, esquerda e direita); três quadros em repouso, seis na caminhada, quatro no ataque, quatro na magia, cinco na ressurreição e dois no dano.
- **Oito categorias de monstros:** rato, morcego, limo, esqueleto, espírito, Sentinela, Cavaleiro das Cinzas e Colosso do Pântano, com estados de repouso, caminhada/voo, ataque e dano em quatro direções. A aranha e a cobra continuam usando animação leve de movimento da etapa 0.6.2.
- **Gatilhos reais:** movimentação, golpes, projéteis, ressurreição, acerto sofrido e IA dos monstros trocam as sequências automaticamente. A aura dos servos e os efeitos visuais de batalha continuam.
- **Sem alterações de regras:** IA, colisões, salvamento da Cripta, upgrades, portais e regiões funcionam sem novas estruturas nos arquivos JSON.

**Limitação artística importante:** estes são *quadros derivados procedimentalmente das ilustrações existentes*, não dezenas de desenhos produzidos manualmente. A direção de costas e os perfis são adaptações da arte frontal; o sprite sheet desenhado à mão em quatro perspectivas ainda é uma melhoria possível. A estrutura já permite substituir os atlases por imagens de produção em `Assets/Resources/AnimationSheets/<especie>.png`, mantendo a convenção de 6 colunas e 4 linhas por estado.

## Testes

`NecroFrameEditorCheck.cs` gera atlases dentro do próprio Editor do Unity antes de compilar Android, inspeciona dimensões, direções e quadros. Ele também verifica cache e disponibilidade das sequências do protagonista, rato e morcego. O teste de estrutura em Python `tests/check_unity_project.py` valida os scripts e seus gatilhos.

## Instalando sem perder a versão 0.5

**Preview 0.6.3** usa o identificador próprio `com.takeo.necromancer.unity.preview063`, que é diferente da 0.5, Preview 0.6.1 e Preview 0.6.2. Este aplicativo aparece como **Necromancer (Preview 0.6.3)** no celular e começa uma campanha independente. Não desinstale a 0.5: a chave antiga não permite uma atualização direta. Consulte [assinatura e backup](../docs/ANDROID_SIGNING.md) antes de migrar dados.

## Próximas etapas

Substituir os quadros derivados por ilustrações 2D definitivas (com direções e roupas consistentes), otimizar memória dos atlases e adicionar partículas/som mais refinados. Esta versão é um protótipo jogável, não os assets finais.

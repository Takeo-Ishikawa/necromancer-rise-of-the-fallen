# Necromancer 0.9.21 — Necromante Sombria

Uma **quarta personagem** foi acrescentada à seleção sem remover, renomear ou modificar recursos de `necromante`, `arlequina` ou `necromante_fisica`.

## Arte nova
- `Assets/Resources/Characters/NecromanteSombria/walk6.png`: novo atlas transparente de 768 × 1024 px, **8 direções × 6 quadros**, com células de 128 × 128 px.
- `Assets/Resources/Characters/NecromanteSombria/portrait.jpg`: retrato aprovado com moldura gótica para a seleção (320 × 427 px).
- As artes foram otimizadas para reduzir peso do APK. Os arquivos de personagens antigos permanecem intactos.

## Carregamento
Os campos opcionais `walkSixResource` e `portraitResource` ativam uma rota de carregamento exclusiva da nova personagem. A lógica existente para os outros três heróis permanece como antes, incluindo quadros, espelhamento e validação de atlas. A reprodução do novo ciclo usa a distância percorrida, em vez de animar os pés quando a personagem encosta nas paredes.

A Necromante Sombria começa como **personagem física**, sem conjuração: usa ataque manual e captura de criaturas caídas. O retrato é mostrado apenas ao selecionar a quarta carta. O ID `necromante_sombria` é salvo na preferência de personagem, sem alterar o formato do progresso.

## Testes recomendados
1. Seleção: encontre a quarta personagem, confira o retrato e selecione-a.
2. Caminhe nas oito direções, verificando os seis passos, a animação parada e as diagonais.
3. Verifique ataque manual, extração/captura de criaturas, refúgio e portais.
4. Volte à seleção e confira os três heróis originais.
5. Confirme que o progresso antigo permanece disponível.

Build 0.9.21, Android release versionCode 29, identificador da preview e save v1 preservados. Os testes estruturais validam o catálogo e os arquivos de arte; a compilação Unity e o teste de jogabilidade ainda são necessários.

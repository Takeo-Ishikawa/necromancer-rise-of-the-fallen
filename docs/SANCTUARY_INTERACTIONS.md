# Necromancer 0.8.0 — Sistema de interações do refúgio

Este documento descreve a implementação da fonte mágica e define um padrão para futuras estações. Respeitar também docs/PADRAO_OTIMIZACAO_ANDROID.md.

## Funcionamento

Aproxime-se da frente da fonte, à direita da câmara. O botão RECEBER BENCAO aparece apenas dentro do alcance; no Editor, a tecla E também ativa a fonte. A bênção dura 45 segundos, dá +25% de dano ao ataque direto e ao projétil da necromante e +20% de velocidade de caminhada, inclusive após a entrada na caverna. A recarga é de 120 segundos desde a ativação e não permite empilhar bônus. A interface mostra a duração restante e o cooldown.

O bônus é transitório, calculado a partir de Time.time e não modifica atributos nem arquivos de salvamento. Pausar o jogo pausa o cronômetro. Reiniciar o aplicativo cancela bênção e cooldown nesta primeira versão.

## Arquitetura

- unity/Assets/Resources/Data/sanctuary_interactions.json: id, kind, label, ponto acessível, raio, duração, recarga, multiplicadores e posição do halo. Não usar o centro da hitbox sólida como ponto de interação.
- unity/Assets/Scripts/NecroSanctuaryInteractions.cs: registro, validação da distância, cooldown por id, dispatch de ações, UI, tecla E, halo e estado do buff.
- NecroSanctuary.cs: inicialização junto aos objetos modulares.
- NecroPrototype.cs: multiplicador de velocidade calculado no momento da caminhada.
- NecroGameplay.cs: multiplicador de dano somente para a necromante, não para os servos.
- NecroUI.cs: bloqueio da área do botão contra captura pelo joystick.

O halo e o efeito de ativação reutilizam NecroArt.RitualCircle e NecroCombatFx; nenhum PNG adicional é carregado. O mapa, o cenário, as colisões e a escala do personagem não foram alterados.

## Padrão de evolução

Para adicionar armário, altar ou outra estação: incluir registro com ID único e ponto alcançável no JSON, implementar somente a nova ação no dispatcher e criar painel modal se a estação exigir uma janela. Não duplicar o joystick, sistema de cooldown ou arte de alta resolução. Adicionar verificações ao arquivo tests/check_unity_project.py.

## Testes em aparelho

1. Chegar à frente da fonte; o botão só deve aparecer próximo à área acessível.
2. Ativar uma vez e observar halo, duração de 45 s e recarga de 120 s.
3. Tocar repetidamente e confirmar que os bônus não se acumulam.
4. Entrar na caverna antes do término; verificar velocidade, dano e retorno ao normal após expirar.
5. Voltar à fonte e tentar ativar antes e após a recarga.
6. Validar que movimentos, combate, servos, câmera, colisões e imagens HD continuam corretos.

A preview 0.8.0 usa o package com.takeo.necromancer.unity.preview080, separado da preview 0.7.5. A instalação e os efeitos exigem teste no Android após a compilação.

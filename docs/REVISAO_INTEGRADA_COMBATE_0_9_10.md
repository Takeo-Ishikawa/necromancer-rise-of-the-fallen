# Necromancer 0.9.10 — Revisão integrada do combate

Data: 8 de novembro de 2026

## Contexto e alcance

Após o usuário confirmar que a build Windows da 0.9.9 compila e funciona na avaliação inicial, esta revisão examinou as interações entre os componentes das versões 0.9.3 a 0.9.9: ataque básico/projétil espectral, Pulso Necrótico, Ordem dos Mortos, posturas Caça/Defesa, formação, cadáveres, reanimação, painel Meus servos, restauração e persistência de regiões.

A revisão **não** promove a 0.9.10 automaticamente a build aprovada: os testes em GitHub Actions são estruturais e ainda será necessário executar a build Unity desta versão quando conveniente.

## Correções aplicadas

### 1. Limite de comando no painel Meus servos

**Antes:** a criação e o carregamento de companheiros respeitavam o limite de 12 pontos de comando, mas o botão **Chamar** no painel Meus servos movia um servo recolhido para a equipe sem verificar esse limite. Isso podia gerar equipes acima do limite, especialmente com esqueletos, chefes e monstros de custo múltiplo.

**Agora:** Chamar verifica `CommandUsed()+CommandCost(a.kind)<=CommandLimit` antes de localizar o espaço e mover a criatura. Em caso de insuficiência, a equipe fica intacta e o painel exibe uma mensagem, sem perder o servo guardado. Recolher, Chamar e Libertar atualizam tanto o registro de companheiros em PlayerPrefs quanto o snapshot JSON de progresso.

### 2. Salvamento periódico e em suspensão

**Antes:** `Autosave(float dt)` existia, mas não era chamado pelo ciclo do jogo. Suspender a aplicação gravava apenas o registro de companheiros, podendo deixar progresso e checkpoint no JSON atrasados.

**Agora:** o Unity agenda o método de salvamento **uma vez a cada 15 segundos** com `InvokeRepeating`, sem adicionar gravações ou varreduras a cada `Update`. O agendamento usa o relógio do jogo, portanto não avança enquanto o painel Meus servos pausa o tempo. Também grava companheiros e progresso ao suspender ou encerrar a aplicação normalmente. Os painéis modais e a restauração impedem gravações periódicas durante operações incompletas.

**Observação:** uma interrupção abrupta do sistema operacional pode impedir `OnApplicationPause` ou `OnApplicationQuit` de executar; nesse caso vale o último checkpoint já gravado. O salvamento mantém a estratégia existente de arquivo temporário e cópia `.bak`.

### 3. Restauração correta do Rotboar

**Antes:** ao restaurar Rotboars de snapshots da Cripta, de uma região ou do exército salvo, os caminhos utilizavam `Create("rotboar")`. Isso gerava o ator comum e não seu conjunto especializado de animações, carregamento e investida. O carregamento de PlayerPrefs já tratava o Rotboar corretamente, mas não os demais caminhos.

**Agora:** todos os caminhos de restauração por snapshot utilizam um único método de fábrica que chama `CreateRotboar` para a espécie Rotboar e `Create` para as outras espécies. Seus atributos personalizados (vida, dano e nível) continuam sendo restaurados do registro original.

## Sistemas revisados sem mudança de balanceamento

- **Combate básico:** soco/chute e projétil permanecem gratuitos, com cadência e dano atuais. A colisão varrida do projétil da 0.9.9 foi preservada.
- **Pulso Necrótico:** custo de 18 ânimas, recarga de 9 segundos, dano apenas contra hostis. O salvamento em lote de mortes foi preservado.
- **Ordem dos Mortos:** bônus temporário de dano e velocidade aos servos, sem alteração de atributos persistidos; recarga mantida entre regiões.
- **Posturas Caça/Defesa:** seleção persistente de alvo e distância máxima de perseguição mantidas.
- **Formação:** posicionamento escalonado, corredor estreito e recuperação segura de servos mantidos.
- **Morte e reanimação:** cadáveres comuns e vínculos especiais permanecem com os tempos e limites estabelecidos na 0.9.8.
- **Interface:** atalhos Windows Q/R/T/E e botões móveis mantidos. Nenhum sprite ou textura foi substituído.

## Ponto de projeto pendente — Vida e derrota da protagonista

A auditoria observou que o ramo `Damage(victim==player)` atualmente dispara apenas o efeito visual `heroineHitPulse` e retorna **sem diminuir HP**. Há também um método `RecoverAfterDeath()`, mas o fluxo de derrota ainda não está conectado à interface. Portanto, a personagem se comporta como invulnerável nessa prévia. **Não modifiquei esse comportamento**, pois ligar o dano sem implementar uma tela de derrota e retorno seguros poderia bloquear o jogo ou mudar radicalmente o equilíbrio de uma versão que acabou de ser testada com sucesso.

Recomendação para uma etapa específica: definir invulnerabilidade de protótipo ou ativar combate real com danos, período de invulnerabilidade após acerto, feedback de HP, tela de derrota, recuperação no refúgio e testes de perda/recuperação de recursos.

## Verificações automatizadas e matriz de testes futuros

Os testes estruturais verificam as três correções acima, incluindo uso da fábrica especializada em todas as rotas de restauração, atualização dos dois registros de companheiros, presença do temporizador e dos callbacks de suspensão e proteção do teto de comando.

Quando houver disponibilidade, testar manualmente os seguintes cenários no Windows:

1. Salvar um jogo, coletar recursos, aguardar mais de 15 segundos, encerrar e reabrir; conferir progresso e checkpoint.
2. Recolher e chamar servos até atingir 12 pontos de comando, inclusive criaturas com custo superior a 1; o 13º ponto deve ser bloqueado sem remover a criatura dos recolhidos.
3. Recolher/libertar criaturas, fechar o jogo e verificar que não retornam indevidamente ao carregar o progresso.
4. Guardar um Rotboar na Cripta, salvar, recuperar, reiniciar e verificar animações/IA de investida (não um sprite genérico).
5. Retornar ao refúgio e testar as posturas Caça/Defesa, formação, Pulso e Ordem, verificando ausência de regressões.
6. Depois, repetir os testes principais no Android, inclusive suspensão do aplicativo e retorno.

## Compatibilidade

Versão de exibição: **0.9.10**. O `necro-save-v1.json` mantém o formato/versionamento, e o identificador/assinatura do APK de prévia são os mesmos. Os workflows de compilação Windows x64 e Android são **manuais e independentes**; nenhuma compilação foi disparada nesta revisão.

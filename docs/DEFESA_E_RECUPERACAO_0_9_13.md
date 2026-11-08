# Necromancer 0.9.13 — Defesa do manto e recuperação no refúgio

## Mudanças

O manto conserva o bônus anterior de 18 HP máximos por grau e agora também reduz o dano recebido **pela protagonista** em 6% por grau: 0%, 6%, 12% e 18% (graus 0–3). A redução multiplica o dano depois da dificuldade regional da 0.9.12 e antes do mínimo de 1 HP por golpe. Não afeta os servos nem modifica danos causados, recompensas ou inimigos. Os valores aparecem no guarda-roupa e no painel de equipamentos.

A fonte arcana recupera **30% da vida máxima por ativação, com mínimo de 18 HP**, limitada ao HP faltante. A recuperação só acontece ao acionar a bênção no refúgio, usando a proximidade e recarga já existentes. Os bônus temporários continuam os mesmos: +25% dano e +20% velocidade por 45 segundos, com recarga de 120 segundos. Com vida cheia, a fonte exibe +0 vida e ainda concede a bênção. A cura na coleta dos recursos da caverna permanece em 12% da vida máxima, mínimo 9 HP. Não foi introduzida regeneração passiva de HP nem cura ao simplesmente entrar no refúgio.

## Compatibilidade

- Sem novos campos de save: armadura deriva de `mantleTier` e cura altera apenas o `hp` já persistido no formato `necro-save-v1.json`. Graus de manto antigos funcionam.
- A ativação da fonte agora chama `SaveProgress()` para persistir o HP recuperado.
- Bônus temporário e recarga da fonte permanecem estados da sessão, sem persistência após reinício, como antes.
- Derrota e retorno ao refúgio continuam restaurando 65% da vida, sem perda de equipamentos nem de servos.
- Versão dos builds Unity 0.9.13; versionCode de release Android 21. ID/assinatura da preview inalterados; seu versionCode continua derivado do número da execução.
- Workflows Android/Windows seguem **manuais e independentes**; nenhuma compilação foi disparada por esta mudança.

## Verificação

O teste estrutural `tests/check_unity_project.py` confere integração e compatibilidade. Antes de compilar Android ou Windows, `NecroDefenseEditorCheck.Verify()` exercita o próprio cálculo C# de armadura e recuperação nos casos de limite. A compilação e os testes reais de jogabilidade ainda precisam ser executados.

**Roteiro da próxima build Windows, quando conveniente:** abra o guarda-roupa e compare redução e vida no manto grau 0–3; receba ataques de um mesmo monstro antes/depois de aumentar o manto; volte ferido ao refúgio e use a fonte, verificando o HP recuperado; tente usá-la novamente durante a recarga; salve e reabra para verificar o HP; confira que os recursos da caverna, o salvamento e a tela de derrota continuam funcionando; no Android, confirme que a preview atualiza a instalação anterior sem trocar o ID.

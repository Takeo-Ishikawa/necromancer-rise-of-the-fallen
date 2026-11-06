# Padrão de otimização Android — Necromancer: Rise of the Fallen

**Versão do padrão:** 1.0 (otimização iniciada na 0.7.6, em 06/11/2026)  
**Engine:** Unity 2022.3.62f2 LTS — projeto em `unity/`  
**Objetivo:** continuar ampliando jogo, monstros, áreas e efeitos, sem crescimento desnecessário do APK ou perda da arte ilustrada.

> **LEITURA OBRIGATÓRIA PARA PRÓXIMAS SESSÕES:** antes de incorporar novos arquivos de arte, verificar as regras abaixo, analisar o código de importação em `unity/Assets/Editor/NecroSanctuaryImportSettings.cs` e comparar o tamanho de um APK Unity real. Não substituir silenciosamente a qualidade das ilustrações por versões borradas.

## 1. Diagnóstico e primeira otimização aplicada

O refúgio v6 adicionou uma nova pintura de fundo e oito PNGs modulares. As versões antigas `safe_chamber.png` (2.343.656 bytes) e `safe_chamber_v2.jpg` (391.804 bytes) ainda estavam em `Assets/Resources/Backgrounds`, embora `sanctuary.json` use **apenas** `safe_chamber_v3`. Como os recursos dentro de `Resources` podem ser empacotados mesmo sem referência explícita, removemos da árvore atual do projeto os dois fundos antigos; eles permanecem recuperáveis pelo histórico do Git. O mapa, a protagonista, o layout JSON e a lógica das colisões não foram alterados.

A versão anterior importava **sem compressão** os PNGs do refúgio (RGBA32) e o JPG de fundo (RGB24) no Android. O importador `NecroSanctuaryImportSettings.cs` agora especifica:
- **Android / mobiliário transparente:** `ETC2_RGBA8` (8 bits por pixel, preserva alpha).
- **Android / fundo opaco da câmara:** `ETC2_RGB4` (4 bits por pixel).
- **Resolução máxima Android:** 2048 px por eixo; resolução original mantida no arquivo-fonte.
- **Filtro:** bilinear; **wrap:** Clamp; **mipmaps:** desligados para estas artes 2D; **sRGB:** ligado.
- **Compression quality:** 100; `CompressedHQ`; não habilitar Crunch automaticamente.
- **Tipo Default** preservado porque o código usa `Resources.Load<Texture2D>()` e `Sprite.Create()`.

ETC2 funciona com GLES 3 e Vulkan e oferece cobertura mais ampla do que exigir ASTC exclusivamente. A qualidade ainda precisa ser comparada visualmente em celulares reais: se pequenos detalhes do ouro e bordas transparentes apresentarem artefatos, ajustar por asset ou testar uma variante ASTC em aparelhos compatíveis. Não converter sprites de forma indiscriminada.

### Métricas de referência

| Indicador | Baseline antes da otimização (v6) |
|---|---:|
| Artefato do GitHub Actions, comprimido (run 42218606276) | **44.096.616 bytes** |
| Artefato anterior, comprimido (run 42161421845) | **26.927.519 bytes** |
| Fundos obsoletos removidos da árvore ativa | **2.735.460 bytes** de arquivos-fonte |
| APK Unity final antes/depois | **medir nos logs**; bytes de artefato ZIP NÃO são tamanho do APK |

O build registra o tamanho do APK em bytes no resumo do workflow `.github/workflows/unity-android.yml`. Compare execuções com **mesma configuração**, mesmo tipo de assinatura, arquitetura e versão do Unity. Não prometa redução exata até verificar a nova compilação.

## 2. Regras permanentes de criação e importação

1. **Arte-mestre vs arte de execução.** Preservar versões originais em local fora de `Assets/Resources` quando não forem usadas no jogo (ou via histórico Git). Exportar apenas uma versão final por recurso ativo. PNG RGBA transparente para objetos, JPG/PNG opaco para fundos.
2. **Proporção e nitidez.** Dimensionar segundo a maior área **realmente exibida na tela**; evitar ampliar sprites minúsculos por transform. Não adotar resolução 4096 automaticamente. Testar no celular; verificar vasos, cabelo, runas, detalhes das mãos e dos olhos.
3. **Importador Android.** Todo novo conjunto de arte pintada precisa de perfil explícito em `Assets/Editor`. Começar com ETC2; testar ASTC 6x6 para grandes fundos/decorativos e ASTC 4x4 para personagens em builds dedicados a GPUs compatíveis. Não aplicar RGBA32 / Uncompressed por padrão em Android.
4. **Alfa/recortes.** Recortar margens transparentes desnecessárias e manter pivô/pés/sombra consistentes. Manter cada objeto com seu próprio GameObject e ordenação vertical. Colisões pertencem aos dados JSON, nunca à área total da imagem.
5. **Reutilização.** Repetir uma mesma lanterna, flor, cristal, rocha ou decalque a partir de um único recurso importado; não duplicar arquivos com nomes diferentes para cada ocorrência. Avaliar SpriteAtlas para muitos ícones pequenos, mas não empacotar imagens gigantes somente por conveniência.
6. **Animações econômicas.** Preferir `ParticleSystem`, gradientes, transformação/UV de materiais e shaders simples para fogo, água, névoa, aura e partículas; evitar sequências longas de PNG quase idênticos. Limitar overdraw e reutilizar partículas com pooling. Medir FPS e memória além de tamanho.
7. **Resources com disciplina.** `Resources` é somente para o necessário em runtime. Apagar da árvore atual (mas preservar no Git) versões de cenários que já foram substituídas. Verificar nomes usados por `Resources.Load`, dados JSON e por scripts de editor antes de mover ou remover.
8. **Sem alterar gameplay durante otimização.** Não modificar combate, salvamentos, colisão, posições, velocidade ou câmera para economizar espaço, salvo necessidade justificada e teste específico.
9. **Build Android.** O build atual utiliza IL2CPP e ARMv7 + ARM64 para compatibilidade. ARM64-only **pode** reduzir o APK, mas não deve virar padrão sem decidir quais aparelhos serão suportados. AssetBundles/Addressables são opção futura quando houver muitos biomas, não requisito agora.

## 3. Checklist por novo elemento (seguir sempre)

- [ ] A arte combina com a paleta do jogo, está nítida no tamanho real e não possui fundo pintado indevido?
- [ ] Foi medida a resolução real necessária, cortadas bordas transparentes e evitadas duplicações?
- [ ] O arquivo está em uma pasta ativa somente se é carregado no jogo?
- [ ] Android importa em formato comprimido adequado, com alpha se necessário?
- [ ] Animação pode ser feita com poucas imagens, material ou partículas reutilizadas?
- [ ] O objeto mantém posição, pivô, sorting, sombra e colisão testados?
- [ ] `python tests/check_unity_project.py` aprovou o novo layout?
- [ ] O workflow **Unity Android Preview APK** concluiu com sucesso e gerou artefato?
- [ ] O tamanho real do APK antes/depois foi anotado, usando o resumo do workflow?
- [ ] No celular: nitidez, artefatos, FPS, memória, interação e entrada/saída da câmara permanecem corretos?

## 4. Próximas rodadas de otimização

**Rodada A (esta):** remover fundos substituídos de `Resources`; importar os gráficos HD do refúgio em ETC2; registrar tamanho do APK; manter testes automatizados.  
**Rodada B:** aplicar política graduada aos sprites da protagonista, criaturas e HUD após verificar efeitos na animação, bordas transparentes e fidelidade dos rostos.  
**Rodada C:** sprite atlases para UI/props repetidos, efeitos de água/fogo/neblina com partículas e materiais reutilizados. Medir FPS/overdraw.  
**Rodada D:** analisar IL2CPP/arquiteturas e, quando houver muitos mapas, conteúdo sob demanda.

## 5. Procedimento de recuperação

O último baseline de refúgio HD antes desta otimização está no commit `b13524f2f685af77d35828643c7ffcd46a2356f9`. Caso apareça qualquer artefato grave ou falha de build, reverta **somente** a compressão/importação ou restaure do Git os PNGs antigos; não desfazer correções de profundidade/colisão sem necessidade. Use o CI da Unity para comprovar compilação e o aparelho para comprovar jogabilidade.

**Regra principal:** primeiro medir, depois otimizar, e só conservar a mudança se o ganho de espaço/memória não comprometer arte, estabilidade ou compatibilidade.

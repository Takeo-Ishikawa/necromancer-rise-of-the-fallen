# Necromancer: Rise of the Fallen

Protótipo 0.1 de um RPG de ação e fantasia sombria para Android (primeiro executado no navegador).

O necromante desperta em uma caverna sem seus poderes. Seu primeiro confronto é com os próprios punhos; ao derrotar uma criatura, desperta uma habilidade espiritual. A habilidade de ressuscitar os inimigos será implementada na etapa 0.2.

## Jogar

Abra [index.html](index.html) por um servidor HTTP local. Por exemplo:

```sh
python -m http.server 8000
```

Visite `http://localhost:8000` no navegador. Para testar no celular, use um endereço acessível na rede local ou publique o repositório no GitHub Pages (raiz da branch `main`).

**Controles:** joystick virtual no lado esquerdo e botão de ataque no direito; no computador, WASD / setas para mover e espaço / J para atacar. Enter inicia e R reinicia após a derrota ou vitória.

## Escopo da versão 0.1

- Mapa único de caverna, câmera que segue o personagem e obstáculos sólidos.
- Movimento em oito direções com joystick virtual e teclado.
- Inimigos de três espécies com perseguição, aproximação, ataque e barra de vida.
- Combate em tempo real e habilidade ofensiva ativa (sem ataque básico automático).
- Primeira derrota por punhos desperta um ataque espiritual à distância.
- Guardião opcional de elite e conclusão curta do prólogo.
- Interface vertical, arte vetorial original desenhada via Canvas e adaptação a telas móveis.
- Lógica de colisão com testes automatizados.

O protótipo usa um micro-motor 2D próprio baseado em Canvas, com atualização em passo de tempo, câmera, colisão e entidades separadas. Não utiliza sprites nem códigos de jogos de terceiros.

## Testes

```sh
npm test
```

Necessário Node.js 20 ou superior. Não há dependências para instalação.

## Roadmap do GDD

| Versão | Entregas |
| --- | --- |
| 0.1 | Movimento, combate, inimigos, habilidade ativa, colisões e câmera |
| 0.2 | Corpos, energia de invocação, ressurreição, IA dos servos, formação e comando |
| 0.3 | Níveis, equipamentos, melhorias e mais criaturas |
| 0.4 | Regiões adicionais, coleta de recursos e chefes |
| 0.5 | Cripta, gerenciamento de servos e rituais |
| 0.6 | Conteúdo avançado, balanceamento e preparação Android |

O roteiro é derivado do GDD fornecido pelo autor. Qualquer referência a Wild Tamer ou Necromancer/Prismathunder serve apenas como inspiração conceitual: os recursos do projeto são originais.

## Organização

- `index.html` e `styles.css`: interface e área de jogo.
- `src/data.js`: configurações, mapa e fichas dos inimigos.
- `src/physics.js`: geometria e resolução de colisões.
- `src/input.js`: joystick, teclado e botão ativo.
- `src/render.js`: câmera e desenho 2D estilizado.
- `src/game.js`: simulação, IA e progressão do prólogo.
- `tests/`: testes de lógica independentes do navegador.

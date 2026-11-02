package com.takeo.necromancer;

/** Regressao da area ampliada e do estado de base. Sem Android SDK. */
public final class CaveExpansionTest {
    private static int tests=0;
    private static void ok(boolean condition,String label) {
        tests++;
        if(!condition)throw new AssertionError(label);
    }
    public static void main(String[] args) {
        World cave=new World(); cave.start();
        ok(World.WIDTH==3072f && World.HEIGHT==2048f,"nova arena ampliada");
        ok(!cave.blocked(cave.x,cave.y,16f),"ponto inicial livre");
        ok(!cave.blocked(1536,1070,16f),"selo central acessivel");
        ok(cave.enemies.size()<=12,"poucos inimigos espacados");
        for(World.Unit enemy:cave.enemies) {
            ok(!cave.blocked(enemy.x,enemy.y,enemy.radius),
                    "inimigo nao nasce dentro de rocha");
        }
        ok(!cave.baseClaimed,"caverna hostil no inicio");
        cave.enemies.clear();
        cave.guardianDefeated=true;
        cave.update(.016f,0,0);
        ok(cave.baseClaimed,"limpeza transforma local em base");
        ok(cave.status==World.PLAY,"base permanece exploravel");

        World bound=new World(); bound.start();
        bound.enemies.clear();
        bound.enemies.add(new World.Unit(World.RAT,850,950,false));
        bound.corpses.add(new World.Corpse(
                new World.Unit(World.WARDEN,bound.x+20,bound.y,false)));
        bound.mana=55;
        ok(bound.raise(),"sentinela ressuscitavel");
        ok(bound.wardenBound && bound.status==World.PLAY,
                "sentinela nao encerra jogo com monstros vivos");
        bound.enemies.clear();
        bound.guardianDefeated=true;
        bound.update(.016f,0,0);
        ok(bound.baseClaimed && bound.status==World.WIN,
                "base finalizada apos ultimo monstro");
        System.out.println("CaveExpansionTest: "+tests+" verificacoes passaram");
    }
}

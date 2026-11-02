package com.takeo.necromancer;

/** Testes independentes da interface Android, usando apenas Java. */
public final class WorldSmokeTest {
    static int checks=0;
    static void check(boolean ok,String what) {
        checks++;
        if(!ok)throw new AssertionError(what);
    }
    public static void main(String[] args) {
        World w=new World();
        check(w.status==World.MENU,"menu inicial");
        check(!w.spellUnlocked,"magia inicialmente bloqueada");
        w.start();
        check(w.status==World.PLAY,"jogo iniciado");
        check(w.blocked(235,785,16),"rocha bloqueia passagem");
        check(w.lineBlocked(135,785,335,785),"rocha bloqueia visada");
        check(!w.lineBlocked(100,100,150,100),"corredor livre");
        World.Unit first=w.enemies.get(0);
        w.x=first.x-35; w.y=first.y; w.dirX=1; w.dirY=0;
        first.hp=20;w.attack();
        check(first.hp<=0,"golpe mata monstro");
        check(w.spellUnlocked,"primeiro monstro desperta magia");
        check(w.kills==1 && w.corpses.size()==1,"cadaver gerado");
        w.x=first.x;w.y=first.y;w.mana=55;
        check(w.raise(),"ressuscitar monstro proximo");
        check(w.servants.size()==1 && w.commandUsed()==1,"servo vinculado");
        check(w.corpses.isEmpty(),"cadaver consumido");
        check(!w.raise(),"sem segundo ritual no mesmo corpo");
        w.toggleOrders();check(w.orders==1,"modo cacar");
        w.toggleOrders();check(w.orders==0,"modo defender");
        check(w.dismiss(),"dispensa servo proximo");
        check(w.commandUsed()==0,"comando liberado");
        check(!w.dismiss(),"sem servo para dispensar");

        World army=new World();army.start();army.enemies.clear();
        for(int i=0;i<5;i++)army.corpses.add(
            new World.Corpse(new World.Unit(World.RAT,army.x+5+i,army.y,false)));
        for(int i=0;i<4;i++) {army.mana=55;check(army.raise(),"respeita comando");}
        check(army.commandUsed()==4,"capacidade maxima");
        army.mana=55;check(!army.raise(),"impede excesso de comando");
        for(int i=0;i<200;i++)army.update(.05f,0,0);
        check(army.servants.size()==4,"servos nao expiram");

        World boss=new World();boss.start();boss.enemies.clear();
        boss.x=400;boss.y=420;boss.mana=55;
        boss.corpses.add(new World.Corpse(new World.Unit(World.WARDEN,405,420,false)));
        check(boss.raise(),"ressuscitar sentinela");
        check(boss.status==World.WIN,"vitoria apos vincular elite");
        check(boss.commandUsed()==3,"custo de comando da elite");
        World decay=new World();decay.start();decay.enemies.clear();
        decay.corpses.add(new World.Corpse(new World.Unit(World.SLIME,decay.x+20,decay.y,false)));
        for(int i=0;i<660;i++)decay.update(.05f,0,0);
        check(decay.corpses.isEmpty(),"cadaver expira");
        System.out.println("WorldSmokeTest: "+checks+" testes passaram");
    }
}

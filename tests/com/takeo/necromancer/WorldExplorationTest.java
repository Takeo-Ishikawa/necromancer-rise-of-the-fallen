package com.takeo.necromancer;

/** Testa viagens, preservação de áreas e os dois chefes da expansão. */
public final class WorldExplorationTest {
    static int passed=0;
    static void check(boolean value,String message) {
        passed++;
        if(!value) throw new AssertionError(message);
    }
    public static void main(String[] args) {
        World w=new World();
        w.start();
        check(w.region==World.CAVE,"inicia na caverna");
        check(w.visited[World.CAVE].prepared,"caverna preparada");
        check(w.resources.size()==4,"caverna contém recursos");
        check(!w.travelTo(World.GRAVEYARD,true),"entrada trancada inicialmente");
        check(!w.forwardPortalOpen(),"santuário inicialmente sem portal");
        w.enemies.clear();w.guardianDefeated=true;w.wardenBound=true;
        w.update(.016f,0,0);
        check(w.baseClaimed,"limpeza da caverna abre base");
        check(w.status==World.WIN,"primeira conquista preservada");
        w.status=World.PLAY;
        check(w.forwardPortalOpen(),"portal da base aberto");
        w.level=3;
        w.soulShards=10;
        check(w.travelTo(World.GRAVEYARD,true),"transição à necrópole");
        check(w.region==World.GRAVEYARD,"região atual necrópole");
        check(w.enemies.size()==11,"encontros da necrópole");
        check(w.resources.size()==5,"ossos rituais no mapa");
        check(w.level==3,"nível preservado");
        check(!w.travelTo(World.MARSH,true),"pântano ainda lacrado");
        check(World.costCommand(World.SKELETON)==2,"esqueleto custa 2 de comando");
        check(World.costCommand(World.WISP)==1,"espírito custa 1");
        check(World.costMana(World.MARSH_LORD)==50,"ritual de chefe mais caro");
        World.Unit skeleton=new World.Unit(World.SKELETON,400,400,true);
        w.applySpeciesBonus(skeleton);
        check(skeleton.hp>0,"novo servo criado");
        w.soulShards=20;w.toggleUpgradeMenu();
        check(w.upgradeSpecies(World.SKELETON),"melhoria de esqueletos");
        check(w.upgradeSpecies(World.WISP),"melhoria de espíritos");
        w.toggleUpgradeMenu();
        w.x=w.resources.get(0).x;w.y=w.resources.get(0).y;
        int gathered=w.gathered;
        int nodes=w.resources.size();
        w.update(.016f,0,0);
        check(w.gathered==gathered+1,"recurso coletado automaticamente");
        check(w.resources.size()==nodes-1,"recurso não pode ser recolhido");

        w.enemies.clear();w.servants.clear();
        w.mana=55;
        w.corpses.add(new World.Corpse(new World.Unit(World.GRAVE_LORD,w.x+10,w.y,false)));
        check(w.raise(),"Cavaleiro pode ser vinculado");
        check(w.regionBossBound[World.GRAVEYARD],"portão da necrópole aberto");
        check(w.status==World.PLAY,"vincular cavaleiro não encerra o jogo");
        check(w.forwardPortalOpen(),"portal do pântano desbloqueado");

        int remaining=w.resources.size();
        check(w.travelTo(World.MARSH,true),"transição ao pântano");
        check(w.region==World.MARSH,"região atual pântano");
        check(w.resources.size()==5,"fungos coletáveis no pântano");
        check(w.enemies.size()==11,"criaturas e chefe do pântano");
        check(!w.forwardPortalOpen(),"sem região posterior ao pântano");
        check(w.travelTo(World.GRAVEYARD,false),"retorno à necrópole");
        check(w.resources.size()==remaining,"recursos persistem entre viagens");
        check(w.enemies.isEmpty(),"inimigos derrotados não renascem ao retornar");
        check(w.travelTo(World.CAVE,false),"retorno à primeira base");
        check(w.baseClaimed,"caverna permanece conquistada");
        check(w.level==3,"nível preservado na volta");
        check(w.travelTo(World.GRAVEYARD,true),"necrópole continua acessível");
        check(w.travelTo(World.MARSH,true),"pântano continua acessível");
        w.servants.clear();w.corpses.clear();
        w.enemies.clear();w.mana=55;
        w.corpses.add(new World.Corpse(new World.Unit(World.MARSH_LORD,w.x+9,w.y,false)));
        check(w.raise(),"Colosso pode ser ressuscitado");
        check(w.status==World.WIN,"Colosso vinculado conclui expansão");
        check(w.commandUsed()==4,"Colosso consome 4 pontos de comando");
        System.out.println("WorldExplorationTest: "+passed+" checks passed");
    }
}

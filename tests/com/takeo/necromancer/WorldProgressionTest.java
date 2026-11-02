package com.takeo.necromancer;

/** Testes de regressao da progressao 0.3, sem SDK nem dependencias externas. */
public final class WorldProgressionTest {
    private static int checks=0;
    private static void ok(boolean success,String label) {
        checks++;
        if(!success) throw new AssertionError(label);
    }
    private static boolean approx(float a,float b) { return Math.abs(a-b)<.001f; }

    public static void main(String[] args) {
        World xp=new World();
        xp.start();
        xp.enemies.clear();
        int startNeed=xp.xpToNext();
        xp.experience=startNeed-1;
        xp.enemies.add(new World.Unit(World.RAT,xp.x+25,xp.y,false));
        World.Unit foe=xp.enemies.get(0);
        foe.hp=5;
        xp.attack();
        ok(xp.level==2,"first kill can level up");
        ok(xp.experience==World.xpValue(World.RAT)-1,"overflow XP carried forward");
        ok(approx(xp.maxHp,127),"level increases max HP");
        ok(approx(xp.maxMana,62),"level increases max mana");
        ok(xp.spellUnlocked,"first kill still unlocks spell");
        ok(xp.soulShards==1,"kill awards soul fragments");
        ok(xp.upgradeCost(World.RAT)==2,"initial species upgrade cost");

        // Guaranteed loot every second kill; test on a simulated second kill.
        World loot=new World();loot.start();loot.enemies.clear();loot.kills=1;
        World.Unit rat=new World.Unit(World.RAT,loot.x+30,loot.y,false);
        rat.hp=5;loot.enemies.add(rat);
        loot.attack();
        ok(loot.drops.size()==1,"second kill generates loot");
        World.Loot chest=loot.drops.get(0);
        ok(chest.slot==1 && chest.tier==1,"first piece is tier-one mantle");
        loot.x=chest.x;loot.y=chest.y;
        loot.enemies.clear();
        loot.update(.016f,0,0);
        ok(loot.drops.isEmpty(),"loot removed after pickup");
        ok(loot.mantleTier==1,"mantle equipped automatically");
        ok(approx(loot.maxHp,133),"mantle increases max HP");
        loot.drops.add(new World.Loot(loot.x,loot.y,0,2));
        loot.update(.016f,0,0);
        ok(loot.staffTier==2,"focus equips at higher tier");
        loot.drops.add(new World.Loot(loot.x,loot.y,0,1));
        int before=loot.soulShards;
        loot.update(.016f,0,0);
        ok(loot.staffTier==2,"weak focus does not replace existing");
        ok(loot.soulShards==before+1,"duplicate converts into shard");

        World army=new World();army.start();army.enemies.clear();
        World.Unit active=new World.Unit(World.RAT,army.x+10,army.y,true);
        army.servants.add(active);
        float baseHp=active.maxHp,baseDamage=active.attack;
        army.soulShards=10;
        army.toggleUpgradeMenu();
        ok(army.upgradeMenu,"progress menu opens");
        float beforeTime=army.time;
        army.update(.04f,1,0);
        ok(army.time==beforeTime,"combat pauses in menu");
        ok(army.upgradeSpecies(World.RAT),"rat upgrade succeeds");
        ok(army.speciesRank[World.RAT]==1,"rat rank increased");
        ok(army.soulShards==8,"upgrade spends shards");
        ok(approx(active.maxHp,baseHp*1.24f),"active minion HP increases");
        ok(approx(active.attack,baseDamage*1.18f),"active minion attack increases");
        army.toggleUpgradeMenu();
        army.mana=55;
        World.Corpse corpse=new World.Corpse(new World.Unit(World.RAT,army.x+5,army.y,false));
        army.corpses.add(corpse);
        ok(army.raise(),"new rat summoned");
        World.Unit revived=army.servants.get(1);
        ok(approx(revived.maxHp,baseHp*1.24f),"new minion inherits upgrades");
        ok(approx(revived.attack,baseDamage*1.18f),"new minion inherits damage");
        army.toggleUpgradeMenu();
        ok(army.upgradeSpecies(World.RAT),"rat rank two upgrade");
        ok(army.speciesRank[World.RAT]==2,"rat rank two applied");
        ok(approx(active.maxHp,baseHp*1.48f),"rank two health scales linearly");
        ok(approx(active.attack,baseDamage*1.36f),"rank two damage scales linearly");
        army.soulShards=0;
        ok(!army.upgradeSpecies(World.RAT),"cannot upgrade without shards");
        ok(army.speciesRank[World.RAT]==2,"failed upgrade leaves rank untouched");
        army.toggleUpgradeMenu();
        army.reset();
        ok(army.level==1 && army.experience==0,"new expedition resets XP");
        ok(army.staffTier==0 && army.mantleTier==0,"new expedition resets gear");
        ok(army.speciesRank[World.RAT]==0 && army.soulShards==0,"new expedition resets ritual progression");
        System.out.println("WorldProgressionTest: "+checks+" checks passed");
    }
}

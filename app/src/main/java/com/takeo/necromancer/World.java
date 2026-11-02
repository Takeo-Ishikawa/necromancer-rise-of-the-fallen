package com.takeo.necromancer;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Random;

/**
 * Simulação do prólogo. Não depende da UI Android.
 * Unidades inimigas e servos usam IA simples com aproximação e combate automático.
 */
final class World {
    static final int MENU = 0, PLAY = 1, DEAD = 2, WIN = 3;
    static final int RAT = 0, SLIME = 1, BAT = 2, WARDEN = 3;
    static final float WIDTH = 1750f, HEIGHT = 1500f;

    static final class Unit {
        final int kind;
        final boolean ally;
        float x, y, hp, maxHp, speed, attack, radius, range, cooldown, attackTimer, flash;
        float targetX, targetY, anim;
        Unit(int type, float px, float py, boolean friendly) {
            kind = type; ally = friendly; x = px; y = py;
            radius = new float[]{15, 20, 15, 28}[kind];
            maxHp = new float[]{34, 54, 30, 230}[kind] * (friendly ? .89f : 1f);
            hp = maxHp;
            speed = new float[]{76, 51, 100, 57}[kind];
            attack = new float[]{8, 10, 6, 15}[kind] * (friendly ? 1.4f : 1f);
            range = new float[]{28, 32, 107, 41}[kind];
            cooldown = new float[]{1.2f, 1.55f, 1.1f, 1.6f}[kind];
        }
    }
    static final class Corpse {
        final int kind;
        final float x, y;
        float time = 32f;
        Corpse(Unit unit) { kind = unit.kind; x = unit.x; y = unit.y; if(kind==WARDEN)time=90f; }
    }
    static final class Projectile {
        float x,y,vx,vy,life = .78f;
        Projectile(float px,float py,float dx,float dy) {
            x=px; y=py; vx=dx*460; vy=dy*460;
        }
    }
    /** Equipamento que surge no chao e e coletado ao passar por perto. */
    static final class Loot {
        final float x,y;
        final int slot; // 0: foco arcano, 1: manto
        final int tier;
        Loot(float px,float py,int itemSlot,int itemTier) {
            x=px; y=py; slot=itemSlot; tier=itemTier;
        }
    }
    static final class Effect {
        float x,y,time,life;
        int type; // 0: golpe, 1: sangue, 2: magia, 3: ressurreição
        Effect(float px,float py,int effect,float duration) {
            x=px; y=py; type=effect; time=duration; life=duration;
        }
    }

    final List<Unit> enemies = new ArrayList<>();
    final List<Unit> servants = new ArrayList<>();
    final List<Corpse> corpses = new ArrayList<>();
    final List<Projectile> projectiles = new ArrayList<>();
    final List<Effect> effects = new ArrayList<>();
    final List<Loot> drops = new ArrayList<>();

    // Progressao limitada a esta expedicao; a cripta persistente virá em 0.5.
    int level=1, experience=0, soulShards=0;
    int staffTier=0, mantleTier=0;
    final int[] speciesRank={0,0,0,0};
    boolean upgradeMenu=false;
    int lastUpgradeKind=-1;

    // Rochas circulares; colisões por eixo permitem escorregar ao longo da borda.
    final float[][] rocks = {
        {235,785,52}, {560,810,43}, {770,1180,61}, {990,1120,59},
        {1120,735,51}, {1240,990,41}, {1360,555,57}, {1600,780,67},
        {540,418,60}, {285,350,80}, {825,370,52}, {1100,250,67},
        {170,1170,43}, {1550,1240,80}, {1090,1340,50}, {875,910,30}
    };

    /** 0: defender o necromante; 1: caçar criaturas nos arredores. */
    int orders = 0;
    boolean guardianDefeated = false;
    int status = MENU, kills = 0;
    float x = 335, y = 1015, dirX = 1, dirY = 0, hp = 115, mana = 55;
    float maxHp = 115, maxMana = 55;
    float attackTimer = 0, attackFlash = 0, hitFlash = 0, time = 0;
    float messageTimer = 0;
    String message = "";
    boolean spellUnlocked = false;
    final Random random = new Random(24);

    World() { reset(); }

    void reset() {
        status = MENU; x=335; y=1015; dirX=1; dirY=0;
        level=1; experience=0; soulShards=0; staffTier=0; mantleTier=0;
        maxHp=115; maxMana=55; upgradeMenu=false; lastUpgradeKind=-1;
        for(int i=0;i<speciesRank.length;i++)speciesRank[i]=0;
        hp=maxHp; mana=maxMana; attackTimer=0; attackFlash=0; hitFlash=0;
        kills=0; time=0; message=""; messageTimer=0; spellUnlocked=false; orders=0; guardianDefeated=false;
        enemies.clear(); servants.clear(); corpses.clear(); projectiles.clear(); effects.clear(); drops.clear();
        spawn(RAT,460,990); spawn(SLIME,615,1105);
        spawn(RAT,755,965); spawn(BAT,830,775);
        spawn(SLIME,970,965); spawn(BAT,705,650);
        spawn(RAT,480,610); spawn(SLIME,1025,610);
        spawn(RAT,1170,820); spawn(BAT,1290,665);
        spawn(SLIME,1375,1045); spawn(RAT,1510,960);
        spawn(BAT,1190,435); spawn(SLIME,1490,460);
        spawn(WARDEN,1510,260);
    }

    void start() { status=PLAY; say("Explore a caverna. Derrote a primeira criatura com os punhos."); }

    private void spawn(int kind,float px,float py) { enemies.add(new Unit(kind,px,py,false)); }

    int commandUsed() {
        int value=0;
        for(Unit u : servants) value += costCommand(u.kind);
        return value;
    }

    int xpToNext() { return 28 + level*18; }
    static int xpValue(int kind) { return kind==WARDEN ? 100 : kind==SLIME ? 20 : kind==BAT ? 17 : 14; }
    static String speciesName(int kind) {
        return kind==RAT ? "RATOS" : kind==SLIME ? "LIMOS" : kind==BAT ? "MORCEGOS" : "SENTINELAS";
    }
    int upgradeCost(int kind) {
        if(kind<0 || kind>=3 || speciesRank[kind]>=3)return -1;
        return 2+2*speciesRank[kind];
    }
    void toggleUpgradeMenu() {
        if(status!=PLAY)return;
        upgradeMenu=!upgradeMenu;
    }
    /** Evolui uma especie em todo o exercito, inclusive futuros ressuscitados. */
    boolean upgradeSpecies(int kind) {
        if(status!=PLAY || !upgradeMenu || kind<0 || kind>=3)return false;
        int cost=upgradeCost(kind);
        if(cost<0) {say("Esta especie ja atingiu o grau maximo.");return false;}
        if(soulShards<cost) {say("Fragmentos de alma insuficientes.");return false;}
        soulShards-=cost;
        speciesRank[kind]++;
        lastUpgradeKind=kind;
        for(Unit ally:servants) {
            if(ally.kind!=kind)continue;
            float gained=ally.maxHp*.24f/(1+.24f*(speciesRank[kind]-1));
            ally.maxHp+=gained;
            ally.hp=Math.min(ally.maxHp,ally.hp+gained);
            ally.attack*= (1+.18f*speciesRank[kind])/(1+.18f*(speciesRank[kind]-1));
        }
        say(speciesName(kind)+" evoluiram para grau "+speciesRank[kind]+"!");
        return true;
    }

    /** Crescimento incremental de 24% vida e 18% dano por grau. */
    void applySpeciesBonus(Unit ally) {
        int rank=speciesRank[ally.kind];
        ally.maxHp*=1+.24f*rank;
        ally.hp=ally.maxHp;
        ally.attack*=1+.18f*rank;
    }

    static int costCommand(int kind) { return kind==SLIME ? 2 : kind==WARDEN ? 3 : 1; }
    static int costMana(int kind) { return kind==SLIME ? 22 : kind==WARDEN ? 37 : 16; }

    void say(String msg) { message=msg; messageTimer=2.6f; }

    /** Seleção espacial usada pelo ritual e pelo indicador no cenário. */
    Corpse nearestCorpse(float radius) {
        Corpse selected = null;
        float nearest = radius * radius;
        for(Corpse corpse:corpses) {
            float d=dist2(x,y,corpse.x,corpse.y);
            if(d<nearest) {nearest=d; selected=corpse;}
        }
        return selected;
    }

    void toggleOrders() {
        if(status!=PLAY || upgradeMenu) return;
        orders=1-orders;
        say(orders==0 ? "Ordem: proteger o necromante." : "Ordem: cacar monstros proximos.");
    }

    /** Retira um servo ativo sem criar novo cadáver ou devolver a ânima gasta. */
    boolean dismiss() {
        if(status!=PLAY || upgradeMenu) return false;
        Unit selected=closest(servants,x,y,165f);
        if(selected==null) {say("Aproxime-se de um servo para dispensa-lo."); return false;}
        servants.remove(selected);
        effects.add(new Effect(selected.x,selected.y,3,.6f));
        say("Servo dispensado. Pontos de comando liberados.");
        return true;
    }

    boolean raise() {
        if(status!=PLAY || upgradeMenu) return false;
        Corpse closest=nearestCorpse(95f);
        if(closest==null) { say("Aproxime-se de um corpo para ressuscitar."); return false; }
        if(commandUsed()+costCommand(closest.kind)>4) {
            say("Limite de comando atingido: 4 pontos."); return false;
        }
        if(mana<costMana(closest.kind)) { say("Ânima insuficiente para o ritual."); return false; }
        mana-=costMana(closest.kind);
        Unit revived=new Unit(closest.kind,closest.x,closest.y,true);
        applySpeciesBonus(revived);
        servants.add(revived);
        corpses.remove(closest);
        effects.add(new Effect(revived.x,revived.y,3,.8f));
        if(revived.kind==WARDEN) {
            status=WIN;
        } else {
            say("Alma vinculada! O servo lutara por voce.");
        }
        return true;
    }

    void attack() {
        if(status!=PLAY || upgradeMenu || attackTimer>0) return;
        if(!spellUnlocked) {
            attackTimer=.46f;
            attackFlash=.20f;
            effects.add(new Effect(x+dirX*30,y+dirY*30,0,.25f));
            Unit nearest=closest(enemies,x,y,66f);
            if(nearest!=null) damage(nearest,26f+staffTier*5f+(level-1)*2f);
        } else {
            if(mana<9) {say("Ânima insuficiente.");return;}
            mana-=9; attackTimer=.72f; attackFlash=.16f;
            Unit target=closest(enemies,x,y,310f);
            float aimX=dirX, aimY=dirY;
            if(target!=null) {
                float deltaX=target.x-x, deltaY=target.y-y;
                float d=(float)Math.hypot(deltaX,deltaY);
                if(d>0){aimX=deltaX/d; aimY=deltaY/d;}
            }
            dirX=aimX; dirY=aimY;
            projectiles.add(new Projectile(x+aimX*20,y+aimY*20,aimX,aimY));
            effects.add(new Effect(x+aimX*22,y+aimY*22,2,.28f));
        }
    }

    void update(float dt, float moveX, float moveY) {
        if(status!=PLAY || upgradeMenu) return;
        dt=Math.max(0,Math.min(.05f,dt));
        time+=dt;
        attackTimer=Math.max(0,attackTimer-dt);
        attackFlash=Math.max(0,attackFlash-dt);
        hitFlash=Math.max(0,hitFlash-dt);
        messageTimer=Math.max(0,messageTimer-dt);
        mana=Math.min(maxMana,mana+(5.5f+(level-1)*.4f)*dt);

        float m=(float)Math.hypot(moveX,moveY);
        if(m>.08f) {
            float dx=moveX/Math.max(1,m),dy=moveY/Math.max(1,m);
            dirX=dx; dirY=dy;
            moveHero(dx*177*dt,dy*177*dt);
        }

        for(Unit enemy:enemies) {
            if(enemy.hp<=0)continue;
            enemy.attackTimer=Math.max(0,enemy.attackTimer-dt);
            enemy.flash=Math.max(0,enemy.flash-dt);
            enemy.anim+=dt;
            float aggression=enemy.kind==WARDEN?325:230;
            float best=dist2(enemy.x,enemy.y,x,y);
            Unit target=null;
            for(Unit ally:servants) {
                float d=dist2(enemy.x,enemy.y,ally.x,ally.y);
                if(d<best){best=d;target=ally;}
            }
            if(best>aggression*aggression) continue;
            float tx=target==null?x:target.x, ty=target==null?y:target.y;
            float dist=(float)Math.sqrt(best);
            if(dist>enemy.range+((target==null)?15:target.radius)-3
                    || lineBlocked(enemy.x,enemy.y,tx,ty)) {
                moveSteered(enemy,tx,ty,enemy.speed*dt);
            } else if(enemy.attackTimer<=0) {
                enemy.attackTimer=enemy.cooldown;
                if(target==null) {
                    hp-=Math.max(1,enemy.attack-mantleTier*1.8f);
                    hitFlash=.25f;
                    effects.add(new Effect(x,y,enemy.kind==BAT?4:1,.3f));
                } else {
                    target.hp-=enemy.attack;
                    target.flash=.23f;
                    effects.add(new Effect(target.x,target.y,enemy.kind==BAT?4:1,.3f));
                }
            }
        }

        for(int i=0;i<servants.size();i++) {
            Unit ally=servants.get(i);
            if(ally.hp<=0)continue;
            ally.flash=Math.max(0,ally.flash-dt);
            ally.attackTimer=Math.max(0,ally.attackTimer-dt);
            ally.anim+=dt;
            // Defesa reativa ou caçada estendida; ninguém se afasta indefinidamente.
            float leash=dist2(ally.x,ally.y,x,y);
            Unit target=leash>390f*390f ? null :
                    closest(enemies,ally.x,ally.y,orders==0?185f:370f);
            if(target!=null) {
                float d=(float)Math.sqrt(dist2(ally.x,ally.y,target.x,target.y));
                if(d>ally.range+target.radius-3f
                        || lineBlocked(ally.x,ally.y,target.x,target.y)) {
                    moveSteered(ally,target.x,target.y,ally.speed*1.19f*dt);
                } else if(ally.attackTimer<=0) {
                    ally.attackTimer=ally.cooldown;
                    damage(target,ally.attack);
                    effects.add(new Effect(target.x,target.y,ally.kind==BAT?4:0,.23f));
                }
            } else {
                float angle=(float)(i*2.3999632+Math.PI/2);
                float tx=x+(float)Math.cos(angle)*55f;
                float ty=y+(float)Math.sin(angle)*55f;
                float d=(float)Math.sqrt(dist2(ally.x,ally.y,tx,ty));
                if(d>15) {
                    float speed=Math.min(ally.speed*1.65f,d*3.7f);
                    moveSteered(ally,tx,ty,speed*dt);
                }
            }
        }

        for(Iterator<Projectile> it=projectiles.iterator();it.hasNext();) {
            Projectile bolt=it.next();
            bolt.x+=bolt.vx*dt;bolt.y+=bolt.vy*dt;bolt.life-=dt;
            boolean impact=false;
            for(Unit target:enemies) {
                if(target.hp>0 && dist2(target.x,target.y,bolt.x,bolt.y)<
                        (target.radius+12)*(target.radius+12)) {
                    damage(target,34f+staffTier*8f+(level-1)*3f);
                    effects.add(new Effect(bolt.x,bolt.y,2,.35f));
                    impact=true;break;
                }
            }
            if(bolt.life<=0 || impact || blocked(bolt.x,bolt.y,5)) it.remove();
        }

        // Passar pelo item o coleta e equipa automaticamente se superar o anterior.
        for(Iterator<Loot> it=drops.iterator();it.hasNext();) {
            Loot item=it.next();
            if(dist2(x,y,item.x,item.y)>43f*43f)continue;
            it.remove();
            if(item.slot==0) {
                if(item.tier>staffTier) {
                    staffTier=item.tier;
                    say("Foco arcano equipado: grau "+staffTier+". Magia fortalecida.");
                } else {
                    soulShards+=1;
                    say("Foco repetido convertido em fragmento de alma.");
                }
            } else {
                if(item.tier>mantleTier) {
                    float increase=18f*(item.tier-mantleTier);
                    mantleTier=item.tier;
                    maxHp+=increase;
                    hp=Math.min(maxHp,hp+increase);
                    say("Manto equipado: grau "+mantleTier+". Defesa fortalecida.");
                } else {
                    soulShards+=1;
                    say("Manto repetido convertido em fragmento de alma.");
                }
            }
            effects.add(new Effect(item.x,item.y,3,.45f));
        }

        enemies.removeIf(unit->unit.hp<=0);
        servants.removeIf(unit->unit.hp<=0);
        for(Iterator<Corpse> it=corpses.iterator();it.hasNext();) {
            Corpse corpse=it.next();corpse.time-=dt;
            if(corpse.time<=0) it.remove();
        }
        for(Iterator<Effect> it=effects.iterator();it.hasNext();) {
            Effect fx=it.next();fx.time-=dt;
            if(fx.time<=0)it.remove();
        }
        if(hp<=0) {hp=0;status=DEAD;}
    }

    private void damage(Unit unit,float amount) {
        if(unit.hp<=0)return;
        unit.hp-=amount;
        unit.flash=.15f;
        effects.add(new Effect(unit.x,unit.y,1,.25f));
        if(unit.hp<=0) {
            kills++;
            corpses.add(new Corpse(unit));
            soulShards+=unit.kind==WARDEN?4:1;
            experience+=xpValue(unit.kind);
            while(experience>=xpToNext()) {
                experience-=xpToNext();
                level++;
                maxHp+=12f;
                maxMana+=7f;
                hp=Math.min(maxHp,hp+20f);
                mana=Math.min(maxMana,mana+13f);
                say("Nivel "+level+"! Vida, anima e magia fortalecidas.");
            }
            // Recompensas visiveis no mapa: uma peca a cada duas vitorias.
            if(kills%2==0 || unit.kind==WARDEN) {
                int slot=(kills/2)%2;
                int tier=Math.min(3,1+Math.max(0,kills-1)/5);
                drops.add(new Loot(unit.x,unit.y,slot,tier));
            }
            mana=Math.min(maxMana,mana+6);
            if(!spellUnlocked) {
                spellUnlocked=true;
                say("Voce despertou o projetil espiritual!");
            }
            if(unit.kind==WARDEN) {
                guardianDefeated=true;
                say("A sentinela caiu! Reviva seu corpo para concluir.");
            }
        }
    }

    Unit closest(List<Unit> units,float px,float py,float maxRange) {
        Unit result=null;
        float best=maxRange*maxRange;
        for(Unit unit:units) {
            if(unit.hp<=0)continue;
            float d=dist2(px,py,unit.x,unit.y);
            if(d<best){best=d;result=unit;}
        }
        return result;
    }

    private static float dist2(float ax,float ay,float bx,float by) {
        float dx=ax-bx,dy=ay-by; return dx*dx+dy*dy;
    }
    boolean blocked(float px,float py,float radius) {
        if(px<radius+10 || py<radius+10 || px>WIDTH-radius-10 || py>HEIGHT-radius-10)return true;
        for(float[] rock:rocks) {
            if(dist2(px,py,rock[0],rock[1])<(radius+rock[2])*(radius+rock[2]))return true;
        }
        return false;
    }
    private void moveHero(float dx,float dy) {
        if(!blocked(x+dx,y,16f))x+=dx;
        if(!blocked(x,y+dy,16f))y+=dy;
    }
    /** Seleção determinística de trajetórias tangentes ao obstáculo imediato. */
    private void moveSteered(Unit unit,float tx,float ty,float step) {
        float distance=(float)Math.hypot(tx-unit.x,ty-unit.y);
        if(distance<1f)return;
        float angle=(float)Math.atan2(ty-unit.y,tx-unit.x);
        // Primeiro o destino; em seguida variações laterais para contornar rochas.
        float[] detours={0f,.37f,-.37f,.75f,-.75f,1.16f,-1.16f,1.57f,-1.57f};
        for(float detour:detours) {
            float a=angle+detour;
            float nx=unit.x+(float)Math.cos(a)*Math.min(step,distance);
            float ny=unit.y+(float)Math.sin(a)*Math.min(step,distance);
            if(!blocked(nx,ny,unit.radius+1f)) {
                unit.x=nx;unit.y=ny;
                return;
            }
        }
    }

    /** Evita que disparos à distância atravessem rochas. */
    boolean lineBlocked(float ax,float ay,float bx,float by) {
        float distance=(float)Math.hypot(bx-ax,by-ay);
        int steps=(int)(distance/18f);
        for(int i=1;i<steps;i++) {
            float t=(float)i/steps;
            for(float[] rock:rocks) {
                float px=ax+(bx-ax)*t,py=ay+(by-ay)*t;
                if(dist2(px,py,rock[0],rock[1])<(rock[2]+3)*(rock[2]+3))return true;
            }
        }
        return false;
    }
}

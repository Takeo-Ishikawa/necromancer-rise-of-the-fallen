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
            attack = new float[]{8, 10, 7, 15}[kind] * (friendly ? 1.4f : 1f);
            range = new float[]{28, 32, 29, 41}[kind];
            cooldown = new float[]{1.2f, 1.55f, 1.1f, 1.6f}[kind];
        }
    }
    static final class Corpse {
        final int kind;
        final float x, y;
        float time = 32f;
        Corpse(Unit unit) { kind = unit.kind; x = unit.x; y = unit.y; }
    }
    static final class Projectile {
        float x,y,vx,vy,life = .78f;
        Projectile(float px,float py,float dx,float dy) {
            x=px; y=py; vx=dx*460; vy=dy*460;
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

    // Rochas circulares; colisões por eixo permitem escorregar ao longo da borda.
    final float[][] rocks = {
        {235,785,52}, {560,810,43}, {770,1180,61}, {990,1120,59},
        {1120,735,51}, {1240,990,41}, {1360,555,57}, {1600,780,67},
        {540,418,60}, {285,350,80}, {825,370,52}, {1100,250,67},
        {170,1170,43}, {1550,1240,80}, {1090,1340,50}, {875,910,30}
    };

    int status = MENU, kills = 0;
    float x = 335, y = 1015, dirX = 1, dirY = 0, hp = 115, mana = 55;
    final float maxHp = 115, maxMana = 55;
    float attackTimer = 0, attackFlash = 0, hitFlash = 0, time = 0;
    float messageTimer = 0;
    String message = "";
    boolean spellUnlocked = false;
    final Random random = new Random(24);

    World() { reset(); }

    void reset() {
        status = MENU; x=335; y=1015; dirX=1; dirY=0;
        hp=maxHp; mana=maxMana; attackTimer=0; attackFlash=0; hitFlash=0;
        kills=0; time=0; message=""; messageTimer=0; spellUnlocked=false;
        enemies.clear(); servants.clear(); corpses.clear(); projectiles.clear(); effects.clear();
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

    static int costCommand(int kind) { return kind==SLIME ? 2 : kind==WARDEN ? 3 : 1; }
    static int costMana(int kind) { return kind==SLIME ? 22 : kind==WARDEN ? 37 : 16; }

    void say(String msg) { message=msg; messageTimer=2.6f; }

    boolean raise() {
        if(status!=PLAY) return false;
        Corpse closest=null;
        float best=85*85;
        for(Corpse corpse:corpses) {
            float d=dist2(x,y,corpse.x,corpse.y);
            if(d<best){best=d; closest=corpse;}
        }
        if(closest==null) { say("Aproxime-se de um corpo para ressuscitar."); return false; }
        if(commandUsed()+costCommand(closest.kind)>4) {
            say("Limite de comando atingido: 4 pontos."); return false;
        }
        if(mana<costMana(closest.kind)) { say("Ânima insuficiente para o ritual."); return false; }
        mana-=costMana(closest.kind);
        Unit revived=new Unit(closest.kind,closest.x,closest.y,true);
        revived.hp=revived.maxHp;
        servants.add(revived);
        corpses.remove(closest);
        effects.add(new Effect(revived.x,revived.y,3,.8f));
        say("Alma vinculada! O servo lutará por você.");
        return true;
    }

    void attack() {
        if(status!=PLAY || attackTimer>0) return;
        if(!spellUnlocked) {
            attackTimer=.46f;
            attackFlash=.20f;
            effects.add(new Effect(x+dirX*30,y+dirY*30,0,.25f));
            Unit nearest=closest(enemies,x,y,66f);
            if(nearest!=null) damage(nearest,26f);
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
        if(status!=PLAY) return;
        dt=Math.max(0,Math.min(.05f,dt));
        time+=dt;
        attackTimer=Math.max(0,attackTimer-dt);
        attackFlash=Math.max(0,attackFlash-dt);
        hitFlash=Math.max(0,hitFlash-dt);
        messageTimer=Math.max(0,messageTimer-dt);
        mana=Math.min(maxMana,mana+5.5f*dt);

        float m=(float)Math.hypot(moveX,moveY);
        if(m>.08f) {
            float dx=moveX/Math.max(1,m),dy=moveY/Math.max(1,m);
            dirX=dx; dirY=dy;
            moveHero(dx*177*dt,dy*177*dt);
        }

        for(Unit enemy:enemies) {
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
            if(dist>enemy.range+((target==null)?15:target.radius)-3) {
                moveUnit(enemy,(tx-enemy.x)/dist*enemy.speed*dt,
                        (ty-enemy.y)/dist*enemy.speed*dt);
            } else if(enemy.attackTimer<=0) {
                enemy.attackTimer=enemy.cooldown;
                if(target==null) {
                    hp-=enemy.attack;
                    hitFlash=.25f;
                    effects.add(new Effect(x,y,1,.3f));
                } else {
                    target.hp-=enemy.attack;
                    target.flash=.23f;
                    effects.add(new Effect(target.x,target.y,1,.3f));
                }
            }
        }

        for(int i=0;i<servants.size();i++) {
            Unit ally=servants.get(i);
            ally.flash=Math.max(0,ally.flash-dt);
            ally.attackTimer=Math.max(0,ally.attackTimer-dt);
            ally.anim+=dt;
            Unit target=closest(enemies,ally.x,ally.y,220f);
            if(target!=null) {
                float d=(float)Math.sqrt(dist2(ally.x,ally.y,target.x,target.y));
                if(d>ally.range+target.radius-3f) {
                    moveUnit(ally,(target.x-ally.x)/Math.max(1,d)*ally.speed*1.19f*dt,
                            (target.y-ally.y)/Math.max(1,d)*ally.speed*1.19f*dt);
                } else if(ally.attackTimer<=0) {
                    ally.attackTimer=ally.cooldown;
                    damage(target,ally.attack);
                    effects.add(new Effect(target.x,target.y,0,.23f));
                }
            } else {
                float angle=(float)(i*2.3999632+Math.PI/2);
                float tx=x+(float)Math.cos(angle)*55f;
                float ty=y+(float)Math.sin(angle)*55f;
                float d=(float)Math.sqrt(dist2(ally.x,ally.y,tx,ty));
                if(d>15) {
                    float speed=Math.min(ally.speed*1.65f,d*3.7f);
                    moveUnit(ally,(tx-ally.x)/Math.max(1,d)*speed*dt,
                            (ty-ally.y)/Math.max(1,d)*speed*dt);
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
                    damage(target,34f); effects.add(new Effect(bolt.x,bolt.y,2,.35f));
                    impact=true;break;
                }
            }
            if(bolt.life<=0 || impact || blocked(bolt.x,bolt.y,5)) it.remove();
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
            mana=Math.min(maxMana,mana+6);
            if(!spellUnlocked) {
                spellUnlocked=true;
                say("Você despertou o projétil espiritual! Toque no ataque.");
            } else if(unit.kind==WARDEN) {
                status=WIN;
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
    private void moveUnit(Unit unit,float dx,float dy) {
        if(!blocked(unit.x+dx,unit.y,unit.radius))unit.x+=dx;
        if(!blocked(unit.x,unit.y+dy,unit.radius))unit.y+=dy;
    }
}

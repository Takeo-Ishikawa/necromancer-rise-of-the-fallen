package com.takeo.necromancer;

import android.app.Activity;
import android.content.Context;
import android.content.pm.ActivityInfo;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.RectF;
import android.graphics.Typeface;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;

import java.util.Random;

/** Activity sem WebView: o jogo é inteiramente desenhado no Canvas do Android. */
public final class MainActivity extends Activity {
    private GameView game;

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        game = new GameView(this);
        setContentView(game);
        immersive();
    }

    @Override public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        if (hasFocus) immersive();
    }

    private void immersive() {
        getWindow().getDecorView().setSystemUiVisibility(
                View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
                        | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_FULLSCREEN
                        | View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                        | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN);
    }

    static final class GameView extends View {
        private final World world = new World();
        private final CaveScene caveScene;
        private final Paint p = new Paint(Paint.ANTI_ALIAS_FLAG);
        private final ExpeditionScene expeditionScene = new ExpeditionScene();
        private final WitchSprite witchSprite = new WitchSprite();
        private final Random decoration = new Random(2026);
        private final float[][] flecks = new float[165][3];
        private float scale=1, sceneHeight=780, cameraX=0, cameraY=0;
        private long previousFrame=0;
        private float joystickX=0,joystickY=0;
        private int joystickPointer=-1;
        private int raisePointer=-1;
        private long raisePressedAt=0;
        private static final int WHITE=Color.rgb(243,234,224);
        private static final int PALE=Color.rgb(204,205,198);
        private static final int INK=Color.rgb(24,26,37);
        private static final int GREEN=Color.rgb(169,217,175);
        private static final int LILAC=Color.rgb(178,164,210);
        private static final int RED=Color.rgb(207,123,123);

        GameView(Context context) {
            super(context);
            caveScene=new CaveScene(context);
            setFocusable(true);
            setKeepScreenOn(true);
            for(int i=0;i<flecks.length;i++) {
                flecks[i][0]=decoration.nextFloat()*World.WIDTH;
                flecks[i][1]=decoration.nextFloat()*World.HEIGHT;
                flecks[i][2]=1+decoration.nextFloat()*4;
            }
        }

        @Override protected void onDraw(Canvas actual) {
            super.onDraw(actual);
            long now=System.nanoTime();
            float dt=previousFrame==0 ? .016f : Math.min(.04f,(now-previousFrame)/1_000_000_000f);
            previousFrame=now;
            if(getWidth()<1 || getHeight()<1)return;
            scale=getWidth()/420f;
            sceneHeight=getHeight()/scale;
            world.update(dt,joystickX,joystickY);
            cameraX=clamp(world.x-210,0,Math.max(0,World.WIDTH-420));
            cameraY=clamp(world.y-sceneHeight*.52f,0,Math.max(0,World.HEIGHT-sceneHeight));

            actual.drawColor(INK);
            actual.save();
            actual.scale(scale,scale);
            actual.save();
            actual.clipRect(0,0,420,sceneHeight);
            actual.translate(-cameraX,-cameraY);
            if(world.region==World.CAVE)caveScene.draw(actual,world,cameraX,cameraY,sceneHeight);
            else expeditionScene.draw(actual,world,cameraX,cameraY,sceneHeight);
            drawPortals(actual);
            drawResources(actual);
            drawCorpses(actual);
            drawLoot(actual);
            drawEntities(actual);
            drawEffects(actual);
            actual.restore();
            drawHud(actual);
            if(world.status==World.PLAY) {
                if(world.upgradeMenu)drawUpgradeMenu(actual);
                else drawControls(actual);
            } else drawOverlay(actual);
            actual.restore();
            postInvalidateOnAnimation();
        }

        private static float clamp(float x,float lo,float hi) {
            return Math.max(lo,Math.min(hi,x));
        }
        private void style(int color) {
            p.reset();p.setAntiAlias(true);p.setColor(color);p.setStyle(Paint.Style.FILL);
        }
        private void rect(Canvas c,float left,float top,float right,float bottom,int color) {
            style(color);c.drawRect(left,top,right,bottom,p);
        }
        private void circle(Canvas c,float x,float y,float radius,int color) {
            style(color);c.drawCircle(x,y,radius,p);
        }
        private void oval(Canvas c,float l,float t,float r,float b,int color) {
            style(color);c.drawOval(l,t,r,b,p);
        }
        private void rounded(Canvas c,float l,float t,float r,float b,float radius,int color) {
            style(color);c.drawRoundRect(l,t,r,b,radius,radius,p);
        }
        private void line(Canvas c,float x,float y,float x2,float y2,float width,int color) {
            style(color);p.setStrokeWidth(width);p.setStrokeCap(Paint.Cap.ROUND);
            c.drawLine(x,y,x2,y2,p);
        }
        private void path(Canvas c,Path path,int color) {
            style(color);c.drawPath(path,p);
        }
        private void text(Canvas c,String message,float x,float y,float size,int color,boolean center,boolean bold) {
            style(color);p.setTypeface(Typeface.create(bold?Typeface.SANS_SERIF:Typeface.SERIF,
                    bold?Typeface.BOLD:Typeface.NORMAL));
            p.setTextSize(size);
            if(center)p.setTextAlign(Paint.Align.CENTER);
            c.drawText(message,x,y,p);
        }

        private void drawCavern(Canvas c) {
            rect(c,0,0,World.WIDTH,World.HEIGHT,Color.rgb(35,39,48));
            for(int gx=0;gx<World.WIDTH;gx+=112) {
                for(int gy=0;gy<World.HEIGHT;gy+=112) {
                    float shift=(gx/112%2)*23;
                    oval(c,gx+shift,gy,gx+shift+92,gy+64,Color.rgb(39,42,51));
                    line(c,gx+12,gy+72,gx+61,gy+72,1,Color.rgb(46,49,59));
                }
            }
            // Faixa de caminho natural que conecta a entrada à sentinela.
            Path road=new Path();
            road.moveTo(285,1480);road.cubicTo(405,1110,875,980,735,740);
            road.cubicTo(600,510,1380,650,1470,170);
            style(Color.rgb(56,53,57));p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(120);p.setStrokeCap(Paint.Cap.ROUND);
            c.drawPath(road,p);
            style(Color.rgb(66,60,65));p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(93);c.drawPath(road,p);

            for(float[] speck:flecks) {
                float x=speck[0],y=speck[1],size=speck[2];
                oval(c,x,y,x+size*2,y+size*.7f,Color.rgb(83,79,87));
                if(size>3.3f)circle(c,x+11,y-9,2.2f,Color.rgb(107,96,106));
            }
            for(float[] rock:world.rocks)drawRock(c,rock[0],rock[1],rock[2]);
            // Portais e brasas do salão final.
            for(int i=-1;i<=1;i+=2) {
                float x=1510+i*115,y=220;
                oval(c,x-22,y-15,x+22,y+12,Color.rgb(17,19,29));
                rounded(c,x-9,y-37,x+9,y-3,3,Color.rgb(85,79,87));
                circle(c,x,y-46,13,Color.rgb(69,118,111));
                circle(c,x,y-47,7,Color.rgb(159,208,176));
                circle(c,x,y-50,3,Color.rgb(224,237,205));
            }
        }

        private void drawRock(Canvas c,float x,float y,float r) {
            oval(c,x-r,y+r*.45f,x+r,y+r*.94f,Color.rgb(15,18,27));
            oval(c,x-r*.92f,y-r*.68f,x+r*.92f,y+r*.75f,Color.rgb(54,55,67));
            oval(c,x-r*.8f,y-r*.66f,x+r*.7f,y+r*.4f,Color.rgb(76,73,84));
            Path facet=new Path();facet.moveTo(x-r*.73f,y-r*.22f);
            facet.lineTo(x-r*.2f,y-r*.66f);facet.lineTo(x+r*.3f,y-r*.4f);
            facet.lineTo(x+r*.1f,y+r*.12f);facet.close();
            path(c,facet,Color.rgb(97,89,96));
            line(c,x-r*.2f,y-r*.65f,x+r*.06f,y-r*.04f,2,Color.rgb(122,110,116));
        }

        private void drawCorpses(Canvas c) {
            World.Corpse selected=world.nearestCorpse(95f);
            for(World.Corpse body:world.corpses) {
                float alpha=clamp(body.time/5,0,1);
                if(alpha<=0)continue;
                c.save();c.translate(body.x,body.y);
                if(body==selected && world.status==World.PLAY) {
                    boolean eligible=world.mana>=World.costMana(body.kind)
                            && world.commandUsed()+World.costCommand(body.kind)<=4;
                    style(eligible?GREEN:RED);
                    p.setStrokeWidth(2.5f);
                    p.setStyle(Paint.Style.STROKE);
                    c.drawCircle(0,0,27+(float)Math.sin(world.time*4)*2,p);
                    text(c,"ANIMA "+World.costMana(body.kind),
                            0,-32,11,eligible?GREEN:RED,true,true);
                }
                oval(c,-19,0,19,10,Color.rgb(20,23,31));
                oval(c,-13,-8,14,3,Color.rgb(119,116,126));
                circle(c,-4,-7,5,Color.rgb(208,197,177));
                circle(c,4,-7,5,Color.rgb(208,197,177));
                circle(c,-4,-7,2,Color.rgb(37,38,49));
                circle(c,4,-7,2,Color.rgb(37,38,49));
                if(body.time<7) {
                    circle(c,0,-13,8,Color.argb((int)(90*alpha),156,206,159));
                }
                c.restore();
            }
        }

        private void drawPortals(Canvas c) {
            if(world.region>World.CAVE) {
                float x=world.returnPortalX(),y=world.returnPortalY();
                ExpeditionScene.portal(c,x,y,Color.rgb(169,180,220),world.time,"BACK");
                text(c,"RETORNAR",x,y-83,13,WHITE,true,true);
            }
            if(world.region<World.MARSH) {
                float x=world.forwardPortalX(),y=world.forwardPortalY();
                if(world.forwardPortalOpen()) {
                    ExpeditionScene.portal(c,x,y,Color.rgb(137,221,190),world.time,"NEXT");
                    text(c,"PRÓXIMA REGIÃO",x,y-83,13,GREEN,true,true);
                } else if(world.region==World.GRAVEYARD) {
                    circle(c,x,y,32,Color.argb(80,22,29,35));
                    text(c,"LACRADO",x,y-49,12,PALE,true,true);
                }
            }
        }

        private void drawResources(Canvas c) {
            for(World.Resource resource:world.resources) {
                c.save();c.translate(resource.x,resource.y);
                float pulse=(float)Math.sin(world.time*3+resource.y)*3;
                oval(c,-22,6,22,12,Color.argb(120,14,18,28));
                circle(c,0,-13,21+pulse,Color.argb(68,146,219,197));
                if(resource.kind==0) {
                    Path gem=new Path();
                    gem.moveTo(0,-40);gem.lineTo(14,-16);gem.lineTo(2,2);
                    gem.lineTo(-14,-15);gem.close();
                    path(c,gem,Color.rgb(165,229,220));
                } else if(resource.kind==1) {
                    line(c,-14,-8,13,-28,5,Color.rgb(222,204,200));
                    line(c,-11,-29,14,-8,4,Color.rgb(216,198,213));
                    circle(c,11,-28,6,Color.rgb(227,211,221));
                } else {
                    line(c,0,1,0,-19,6,Color.rgb(210,218,191));
                    oval(c,-19,-31,19,-12,Color.rgb(153,214,166));
                    circle(c,9,-25,4,Color.rgb(230,241,216));
                }
                c.restore();
            }
        }

        private void drawLoot(Canvas c) {
            for(World.Loot item:world.drops) {
                c.save();c.translate(item.x,item.y);
                float bob=(float)Math.sin(world.time*4+item.x)*4;
                oval(c,-15,9,15,16,Color.argb(160,14,18,29));
                c.translate(0,bob);
                int tint=item.slot==0?Color.rgb(166,190,240):Color.rgb(225,174,116);
                circle(c,0,-5,17,Color.argb(70,190,205,210));
                Path gem=new Path();
                gem.moveTo(0,-22);gem.lineTo(13,-7);gem.lineTo(0,9);
                gem.lineTo(-13,-7);gem.close();
                path(c,gem,tint);
                text(c,item.slot==0?"F":"M",0,-3,12,INK,true,true);
                text(c,"T"+item.tier,0,24,11,WHITE,true,true);
                c.restore();
            }
        }

        private void drawEntities(Canvas c) {
            for(World.Unit enemy:world.enemies)drawUnit(c,enemy);
            for(World.Unit servant:world.servants)drawUnit(c,servant);
            witchSprite.draw(c,world);
        }

        private void drawUnit(Canvas c,World.Unit unit) {
            c.save();c.translate(unit.x,unit.y);
            float bob=(float)Math.sin(world.time*5+unit.x)*2;
            oval(c,-unit.radius*1.05f,unit.radius*.55f,
                    unit.radius*1.1f,unit.radius*1.06f,Color.rgb(20,21,30));
            c.translate(0,bob);
            if(unit.ally) {
                circle(c,0,0,unit.radius+9,Color.argb(45,142,238,181));
            }
            int outline=Color.rgb(41,37,49);
            int base=unit.ally?Color.rgb(163,198,181):
                    unit.kind==World.RAT?Color.rgb(179,125,132):
                    unit.kind==World.SLIME?Color.rgb(154,165,114):
                    unit.kind==World.BAT?Color.rgb(134,116,165):
                    unit.kind==World.SKELETON?Color.rgb(206,194,181):
                    unit.kind==World.WISP?Color.rgb(145,207,210):
                    unit.kind==World.GRAVE_LORD?Color.rgb(166,142,189):
                    unit.kind==World.MARSH_LORD?Color.rgb(124,175,143):
                    Color.rgb(146,123,137);
            if(unit.flash>0)base=WHITE;
            if(unit.kind==World.RAT) {
                line(c,-11,1,-25,-9,4,base);
                oval(c,-18,-9,17,12,outline);
                oval(c,-15,-12,17,11,base);
                circle(c,-9,-12,7,base);circle(c,10,-13,7,base);
                circle(c,-9,-12,3,Color.rgb(222,171,170));
                circle(c,10,-13,3,Color.rgb(222,171,170));
                oval(c,-3,-6,13,4,Color.rgb(208,185,172));
                circle(c,0,-4,2.4f,outline);circle(c,9,-4,2.4f,outline);
                circle(c,5,3,2.2f,Color.rgb(245,165,172));
            } else if(unit.kind==World.SLIME) {
                Path slime=new Path();slime.moveTo(-21,12);
                slime.cubicTo(-28,-1,-16,-14,-7,-17);
                slime.cubicTo(7,-32,22,-8,22,8);
                slime.quadTo(18,16,7,13);slime.quadTo(-7,17,-21,12);slime.close();
                path(c,slime,outline);
                c.save();c.scale(.86f,.88f);path(c,slime,base);c.restore();
                oval(c,-13,-4,9,1,Color.argb(70,232,232,203));
                circle(c,-6,-7,2.6f,outline);circle(c,7,-7,2.6f,outline);
                line(c,-3,2,4,3,2,outline);
            } else if(unit.kind==World.BAT) {
                Path wing=new Path();
                wing.moveTo(-3,-2);wing.quadTo(-26,-29,-30,-12);
                wing.lineTo(-28,4);wing.lineTo(-18,0);wing.lineTo(-11,8);
                wing.lineTo(0,2);wing.lineTo(10,8);wing.lineTo(18,0);
                wing.lineTo(28,4);wing.lineTo(30,-12);
                wing.quadTo(26,-29,3,-2);wing.close();
                path(c,wing,outline);
                c.save();c.scale(.89f,.82f);path(c,wing,base);c.restore();
                oval(c,-10,-15,10,9,base);
                Path ears=new Path();
                ears.moveTo(-9,-9);ears.lineTo(-9,-23);ears.lineTo(-1,-12);
                ears.lineTo(4,-12);ears.lineTo(10,-23);ears.lineTo(9,-9);ears.close();
                path(c,ears,base);
                circle(c,-4,-5,2.4f,Color.rgb(238,198,172));
                circle(c,5,-5,2.4f,Color.rgb(238,198,172));
            } else if(unit.kind==World.SKELETON) {
                oval(c,-18,-20,18,14,outline);
                oval(c,-15,-19,15,12,base);
                oval(c,-11,-18,11,-1,Color.rgb(239,222,197));
                circle(c,-5,-11,3,outline);circle(c,5,-11,3,outline);
                line(c,-9,2,9,2,3,outline);
                line(c,18,-7,29,-34,4,Color.rgb(184,182,184));
                line(c,22,-20,34,-17,3,Color.rgb(184,182,184));
                line(c,-10,11,-10,20,4,base);line(c,10,11,10,20,4,base);
            } else if(unit.kind==World.WISP) {
                circle(c,0,-10,unit.radius+10,Color.argb(70,145,215,220));
                Path spirit=new Path();
                spirit.moveTo(0,-35);
                spirit.cubicTo(23,-33,27,-8,13,9);
                spirit.lineTo(7,19);spirit.lineTo(0,12);
                spirit.lineTo(-9,19);spirit.lineTo(-13,9);
                spirit.cubicTo(-26,-9,-18,-35,0,-35);
                spirit.close();path(c,spirit,base);
                circle(c,-7,-12,3,outline);circle(c,7,-12,3,outline);
                circle(c,-7,-12,1,WHITE);circle(c,7,-12,1,WHITE);
            } else {
                // Guardião de elite: couraça antiga, chifres e arma rachada.
                line(c,-22,0,-35,-35,6,Color.rgb(105,101,101));
                circle(c,-36,-38,7,Color.rgb(153,204,168));
                oval(c,-28,-17,28,30,outline);
                oval(c,-24,-20,24,26,base);
                Path helm=new Path();helm.moveTo(-19,-16);helm.lineTo(-26,-42);
                helm.lineTo(-10,-29);helm.lineTo(0,-36);helm.lineTo(10,-29);
                helm.lineTo(26,-42);helm.lineTo(19,-16);helm.close();
                path(c,helm,Color.rgb(99,95,109));
                oval(c,-15,-24,15,-1,Color.rgb(206,193,181));
                circle(c,-7,-15,3,Color.rgb(92,182,145));
                circle(c,8,-15,3,Color.rgb(92,182,145));
                line(c,-21,5,21,5,3,Color.rgb(200,178,155));
                line(c,-14,13,14,13,3,Color.rgb(200,178,155));
                if(unit.kind==World.GRAVE_LORD) {
                    line(c,-25,-7,26,-7,4,Color.rgb(219,195,214));
                    circle(c,0,-43,12,Color.argb(98,186,152,226));
                } else if(unit.kind==World.MARSH_LORD) {
                    circle(c,-25,5,10,Color.rgb(100,145,119));
                    circle(c,25,5,10,Color.rgb(100,145,119));
                    circle(c,0,-42,13,Color.argb(110,158,219,162));
                }
            }
            if(unit.ally) {
                int grade=world.speciesRank[unit.kind];
                if(grade>0) {
                    circle(c,0,-unit.radius-18,10,Color.argb(130,25,57,48));
                    text(c,""+grade,0,-unit.radius-14,11,WHITE,true,true);
                }
                circle(c,0,-unit.radius-17,4.3f,GREEN);
                circle(c,0,-unit.radius-17,8,Color.argb(50,163,229,170));
            }
            if(unit.hp<unit.maxHp || unit.kind==World.WARDEN ||
                    unit.kind==World.GRAVE_LORD || unit.kind==World.MARSH_LORD) {
                float w=(unit.kind==World.WARDEN || unit.kind==World.GRAVE_LORD ||
                        unit.kind==World.MARSH_LORD)?70:39;
                rounded(c,-w/2,-unit.radius-29,w/2,-unit.radius-24,3,INK);
                rounded(c,-w/2,-unit.radius-29,
                        -w/2+w*clamp(unit.hp/unit.maxHp,0,1),-unit.radius-24,
                        2,unit.ally?GREEN:RED);
            }
            c.restore();
        }

        private void drawHero(Canvas c) {
            c.save();c.translate(world.x,world.y);
            float swing=world.attackFlash>0?12:0;
            oval(c,-22,10,24,20,Color.rgb(14,17,25));
            // Cajado, roupa, capuz e máscara cadavérica: design original.
            line(c,19,-32,29+swing,-54,4,Color.rgb(158,135,113));
            line(c,29+swing,-54,34+swing,-69,4,Color.rgb(158,135,113));
            circle(c,35+swing,-70,9,world.staffTier>0?Color.rgb(110,102,171):Color.rgb(76,110,105));
            circle(c,35+swing,-70,4,world.staffTier>0?Color.rgb(219,202,255):GREEN);
            Path cape=new Path();cape.moveTo(-12,-17);cape.lineTo(12,-18);
            cape.lineTo(25,20);cape.quadTo(0,30,-25,20);cape.close();
            path(c,cape,world.mantleTier>0?Color.rgb(40,36,66):Color.rgb(21,22,33));
            Path inside=new Path();inside.moveTo(-8,-16);inside.lineTo(8,-16);
            inside.lineTo(14,17);inside.lineTo(-15,17);inside.close();
            path(c,inside,Color.rgb(75,66,91));
            line(c,-11,18,11,18,3,Color.rgb(140,122,159));
            oval(c,-17,-34,17,-9,Color.rgb(28,29,42));
            Path hood=new Path();hood.moveTo(-18,-21);hood.quadTo(-20,-43,0,-48);
            hood.quadTo(20,-44,18,-21);hood.lineTo(12,-13);
            hood.lineTo(-13,-13);hood.close();path(c,hood,Color.rgb(40,37,54));
            oval(c,-11,-31,11,-12,Color.rgb(200,200,194));
            oval(c,-10,-28,10,-15,Color.rgb(212,205,190));
            oval(c,-8,-25,-2,-20,Color.rgb(36,42,49));
            oval(c,3,-25,9,-20,Color.rgb(36,42,49));
            circle(c,-5,-23,1.5f,GREEN);circle(c,6,-23,1.5f,GREEN);
            line(c,-2,-17,3,-17,2,Color.rgb(76,62,69));
            oval(c,-16,-37,-9,-26,Color.rgb(64,60,79));
            oval(c,9,-37,16,-26,Color.rgb(64,60,79));
            if(world.attackFlash>0)circle(c,31,-51,12,Color.argb(75,151,237,186));
            if(world.hitFlash>0) {
                circle(c,0,-16,24,Color.argb(85,239,106,107));
            }
            c.restore();
        }

        private void drawEffects(Canvas c) {
            for(World.Projectile b:world.projectiles) {
                circle(c,b.x,b.y,15,Color.argb(62,133,224,177));
                circle(c,b.x,b.y,8,Color.rgb(115,196,162));
                circle(c,b.x,b.y,4,Color.rgb(234,248,216));
            }
            for(World.Effect fx:world.effects) {
                float ratio=1-fx.time/fx.life;
                int alpha=(int)(170*(1-ratio));
                int tint=fx.type==1?Color.argb(alpha,218,123,123):
                        fx.type==4?Color.argb(alpha,195,156,226):
                        fx.type==0?Color.argb(alpha,235,213,178):
                        Color.argb(alpha,154,233,185);
                style(tint);p.setStyle(Paint.Style.STROKE);
                p.setStrokeWidth(fx.type==3?5:3);
                c.drawCircle(fx.x,fx.y,(fx.type==3?12:8)+ratio*(fx.type==3?40:27),p);
                if(fx.type==3) {
                    circle(c,fx.x,fx.y-13*(1-ratio),4,Color.argb(alpha,225,246,215));
                }
            }
        }

        private void drawHud(Canvas c) {
            rect(c,0,0,420,217,Color.argb(208,19,22,32));
            text(c,"✦  NECROMANCER  ✦",210,30,15,WHITE,true,true);
            text(c,world.region==World.CAVE && world.baseClaimed?
                    "SANTUÁRIO DAS ALMAS":World.regionName(world.region),210,47,10,LILAC,true,true);
            bar(c,25,59,280,12,world.hp/world.maxHp,Color.rgb(202,107,113),"VIDA");
            bar(c,25,80,280,12,world.mana/world.maxMana,Color.rgb(147,169,214),"ÂNIMA");
            bar(c,25,101,280,12,(float)world.experience/world.xpToNext(),
                    Color.rgb(163,193,154),"EXP");
            text(c,Math.round(world.hp)+"/"+(int)world.maxHp,380,69,12,WHITE,true,true);
            text(c,Math.round(world.mana)+"/"+(int)world.maxMana,380,90,12,WHITE,true,true);
            text(c,world.experience+"/"+world.xpToNext(),380,111,12,WHITE,true,true);
            text(c,"NECROMANTE · NÍVEL "+world.level,210,130,11,LILAC,true,true);
            if(world.region==World.GRAVEYARD) {
                text(c,world.regionBossBound[World.GRAVEYARD]?"PORTAL ABERTO":"O CAVALEIRO DAS CINZAS",210,149,10,GREEN,true,true);
                text(c,world.regionBossBound[World.GRAVEYARD]?
                        "Use o portal para o pântano":"Derrote e ressuscite o Cavaleiro",210,166,12,WHITE,true,false);
            } else if(world.region==World.MARSH) {
                text(c,"O COLOSSO DO PÂNTANO",210,149,10,GREEN,true,true);
                text(c,"Derrote e ressuscite o Colosso",210,166,12,WHITE,true,false);
            } else if(world.baseClaimed) {
                text(c,"A PRIMEIRA BASE",210,149,10,GREEN,true,true);
                text(c,"Entre no portal do santuário",210,166,12,WHITE,true,false);
            } else if(world.kills==0) {
                text(c,"O DESPERTAR",210,149,10,GREEN,true,true);
                text(c,"Derrote seu primeiro monstro",210,166,12,WHITE,true,false);
            } else if(world.guardianDefeated) {
                text(c,"O RITUAL FINAL",210,149,10,GREEN,true,true);
                text(c,"Derrote os monstros restantes",210,166,12,WHITE,true,false);
            } else if(world.servants.isEmpty()) {
                text(c,"A PRIMEIRA ALMA",210,149,10,GREEN,true,true);
                text(c,"Ressuscite um cadáver próximo",210,166,12,WHITE,true,false);
            } else {
                text(c,"A SENTINELA",210,149,10,GREEN,true,true);
                text(c,"Encontre e derrote o guardião",210,166,12,WHITE,true,false);
            }
            text(c,"FOCO T"+world.staffTier+"   ·   MANTO T"+world.mantleTier+
                     "   ·   ESSÊNCIAS "+world.soulShards,
                    210,188,10,PALE,true,true);
            text(c,"RECURSOS "+world.gathered+"  ·  REGIÃO "+(world.region+1)+"/3",
                    210,207,9,GREEN,true,true);
            rounded(c,103,sceneHeight-53,317,sceneHeight-29,10,Color.argb(187,20,22,31));
            text(c,"SERVOS "+world.servants.size()+"  ·  COMANDO "+world.commandUsed()+"/4",
                    210,sceneHeight-37,11,PALE,true,true);
            if(world.messageTimer>0 && world.status==World.PLAY) {
                rounded(c,20,sceneHeight*.31f,400,sceneHeight*.31f+45,12,Color.argb(204,23,28,37));
                text(c,world.message,210,sceneHeight*.31f+27,11,WHITE,true,true);
            }
        }

        private void bar(Canvas c,float x,float y,float width,float height,
                         float portion,int fill,String caption) {
            text(c,caption,x,y+10,10,PALE,false,true);
            rounded(c,x+51,y,width+x,y+height,5,Color.rgb(67,62,73));
            rounded(c,x+51,y,x+51+(width-51)*clamp(portion,0,1),y+height,5,fill);
        }

        private void drawControls(Canvas c) {
            float jx=91,jy=sceneHeight-111;
            circle(c,jx,jy,62,Color.argb(105,19,20,33));
            style(Color.argb(135,175,183,199));p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(2);c.drawCircle(jx,jy,59,p);
            circle(c,jx+joystickX*40,jy+joystickY*40,27,Color.argb(190,77,73,92));
            circle(c,jx+joystickX*40,jy+joystickY*40,15,Color.argb(118,174,166,185));

            float ax=330,ay=sceneHeight-100;
            circle(c,ax,ay,50,Color.argb(225,46,42,64));
            style(Color.rgb(192,184,203));p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(3);c.drawCircle(ax,ay,48,p);
            text(c,world.spellUnlocked?"✧":"✥",ax,ay+5,32,WHITE,true,true);
            text(c,world.spellUnlocked?"MAGIA":"GOLPE",ax,ay+29,10,WHITE,true,true);
            if(world.attackTimer>0)text(c,String.format(java.util.Locale.US,"%.1f",world.attackTimer),
                    ax,ay+43,10,PALE,true,false);

            // Evolucao de especies em um painel que pausa a batalha.
            float ux=106,uy=sceneHeight-212;
            circle(c,ux,uy,37,Color.argb(222,45,73,63));
            style(GREEN);p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(2);c.drawCircle(ux,uy,35,p);
            text(c,"✦",ux,uy+6,27,GREEN,true,true);
            text(c,"EVOLUIR",ux,uy+24,9,WHITE,true,true);

            // Toque para alternar entre guardar o heroi e perseguir inimigos.
            float ox=238,oy=sceneHeight-212;
            circle(c,ox,oy,36,Color.argb(216,57,52,76));
            style(LILAC);p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(2);c.drawCircle(ox,oy,34,p);
            text(c,world.orders==0?"✥":"⚔",ox,oy+6,27,WHITE,true,true);
            text(c,world.orders==0?"DEFESA":"CAÇA",ox,oy+24,9,WHITE,true,true);

            float rx=336,ry=sceneHeight-212;
            boolean canRaise=world.nearestCorpse(95f)!=null;
            circle(c,rx,ry,37,canRaise?Color.argb(225,38,82,65):Color.argb(216,38,64,59));
            style(Color.rgb(173,208,180));p.setStyle(Paint.Style.STROKE);
            p.setStrokeWidth(2);c.drawCircle(rx,ry,35,p);
            text(c,"☠",rx,ry+7,29,GREEN,true,true);
            text(c,"REVIVER",rx,ry+24,9,WHITE,true,true);
        }

        private void drawUpgradeMenu(Canvas c) {
            rect(c,0,0,420,sceneHeight,Color.argb(220,11,13,22));
            float top=sceneHeight*.17f;
            rounded(c,24,top,396,top+417,15,Color.rgb(35,37,50));
            style(Color.rgb(130,155,143));
            p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(1.5f);
            c.drawRoundRect(24,top,396,top+417,15,15,p);
            text(c,"EVOLUÇÃO DO EXÉRCITO",210,top+37,19,WHITE,true,true);
            text(c,"FRAGMENTOS DE ALMA: "+world.soulShards,
                    210,top+61,12,GREEN,true,true);
            text(c,"Cada grau aumenta vida em 24% e dano em 18%",
                    210,top+80,11,PALE,true,false);
            for(int kind=0;kind<3;kind++) {
                float y=top+94+kind*84;
                int cost=world.upgradeCost(kind);
                boolean can=cost>=0 && world.soulShards>=cost;
                rounded(c,42,y,378,y+72,10,can?Color.rgb(51,79,67):Color.rgb(51,50,61));
                text(c,World.speciesName(kind),59,y+24,15,WHITE,false,true);
                text(c,"GRAU "+world.speciesRank[kind]+"/3",59,y+48,11,LILAC,false,true);
                rounded(c,261,y+15,365,y+57,8,can?Color.rgb(146,187,151):Color.rgb(86,89,95));
                text(c,cost<0?"MÁXIMO":"CUSTO "+cost,313,y+41,12,
                        can?INK:WHITE,true,true);
            }
            rounded(c,79,top+359,341,top+400,9,Color.rgb(115,113,132));
            text(c,"VOLTAR AO COMBATE",210,top+385,13,WHITE,true,true);
            if(world.messageTimer>0) {
                text(c,world.message,210,top+440,11,GREEN,true,true);
            }
        }

        private void drawOverlay(Canvas c) {
            rect(c,0,0,420,sceneHeight,Color.argb(205,11,14,24));
            rounded(c,21,sceneHeight*.22f,399,sceneHeight*.78f,14,
                    Color.rgb(37,37,50));
            style(Color.rgb(122,121,132));p.setStrokeWidth(1.5f);
            p.setStyle(Paint.Style.STROKE);
            c.drawRoundRect(21,sceneHeight*.22f,399,sceneHeight*.78f,14,14,p);
            float top=sceneHeight*.22f;
            text(c,world.region==World.CAVE?"PRÓLOGO · A CAVERNA ESQUECIDA":
                    "EXPLORAÇÃO · "+World.regionName(world.region),
                    210,top+38,12,GREEN,true,true);
            text(c,"☾",210,top+122,85,LILAC,true,false);
            String heading=world.status==World.MENU?"O PRIMEIRO DESPERTAR":
                    world.status==World.DEAD?"O FIM DO RITUAL":
                    world.region==World.MARSH?"O EXÉRCITO DESPERTA":"A PRIMEIRA BASE";
            text(c,heading,210,top+171,22,WHITE,true,true);
            if(world.status==World.MENU) {
                text(c,"Você despertou sem poderes.",210,top+218,16,PALE,true,false);
                text(c,"Derrote inimigos e domine suas almas.",210,top+242,15,PALE,true,false);
                text(c,"Forme um exército e desafie a sentinela.",210,top+266,15,PALE,true,false);
            } else if(world.status==World.DEAD) {
                text(c,"Seus servos foram dispersos.",210,top+226,16,PALE,true,false);
                text(c,"Tente uma nova expedição.",210,top+251,16,PALE,true,false);
            } else if(world.region==World.MARSH) {
                text(c,"O Colosso agora obedece ao exército.",210,top+226,15,PALE,true,false);
                text(c,"Três regiões foram conquistadas.",210,top+251,15,PALE,true,false);
            } else {
                text(c,"A caverna tornou-se sua primeira base.",210,top+226,15,PALE,true,false);
                text(c,"Explore o portal do santuário.",210,top+251,15,PALE,true,false);
            }
            float bottom=sceneHeight*.78f;
            rounded(c,56,bottom-88,364,bottom-34,9,Color.rgb(122,148,130));
            text(c,world.status==World.MENU?"DESPERTAR":world.status==World.WIN?
                    (world.region==World.CAVE?"ENTRAR NA BASE":"CONTINUAR EXPLORANDO"):
                    "JOGAR NOVAMENTE",
                    210,bottom-54,17,INK,true,true);
            text(c,"Mover: joystick · Atacar: magia ou punhos",210,bottom-11,11,PALE,true,false);
            text(c,"REVIVER: toque · Dispensar servo: segure",210,bottom+4,10,PALE,true,false);
        }

        @Override public boolean onTouchEvent(MotionEvent event) {
            int action=event.getActionMasked();
            int index=event.getActionIndex();
            if(action==MotionEvent.ACTION_DOWN ||
                    action==MotionEvent.ACTION_POINTER_DOWN) {
                float px=event.getX(index)/scale,py=event.getY(index)/scale;
                if(world.status!=World.PLAY) {
                    if(world.status==World.WIN) {
                        world.status=World.PLAY;
                        world.say(world.region==World.CAVE?
                                "Santuario conquistado! Procure o portal para explorar.":
                                "O pantano foi conquistado! Explore livremente.");
                    } else {
                        world.reset();world.start();
                    }
                    previousFrame=System.nanoTime();
                    return true;
                }
                if(world.upgradeMenu) {
                    float top=sceneHeight*.17f;
                    boolean tapped=false;
                    if(px>=42 && px<=378) {
                        for(int kind=0;kind<3;kind++) {
                            float row=top+94+kind*84;
                            if(py>=row && py<=row+72) {
                                world.upgradeSpecies(kind);
                                tapped=true;
                                break;
                            }
                        }
                    }
                    if(!tapped)world.toggleUpgradeMenu();
                    return true;
                }
                if(px<175 && py>sceneHeight-205) {
                    if(joystickPointer==-1) {
                        joystickPointer=event.getPointerId(index);
                        updateStick(px,py);
                    }
                } else if(dist(px,py,330,sceneHeight-100)<70) {
                    world.attack();
                } else if(dist(px,py,336,sceneHeight-212)<42) {
                    raisePointer=event.getPointerId(index);
                    raisePressedAt=System.nanoTime();
                } else if(dist(px,py,238,sceneHeight-212)<41) {
                    world.toggleOrders();
                } else if(dist(px,py,106,sceneHeight-212)<41) {
                    joystickPointer=-1;joystickX=0;joystickY=0;
                    world.toggleUpgradeMenu();
                }
            } else if(action==MotionEvent.ACTION_MOVE) {
                int current=event.findPointerIndex(joystickPointer);
                if(current>=0)updateStick(event.getX(current)/scale,event.getY(current)/scale);
            } else if(action==MotionEvent.ACTION_UP ||
                    action==MotionEvent.ACTION_POINTER_UP ||
                    action==MotionEvent.ACTION_CANCEL) {
                if(action==MotionEvent.ACTION_CANCEL) {
                    joystickPointer=-1;joystickX=0;joystickY=0;raisePointer=-1;
                } else {
                    int pointerId=event.getPointerId(index);
                    if(pointerId==raisePointer) {
                        if(System.nanoTime()-raisePressedAt>=650_000_000L)world.dismiss();
                        else world.raise();
                        raisePointer=-1;
                    }
                    if(pointerId==joystickPointer) {
                        joystickPointer=-1;joystickX=0;joystickY=0;
                    }
                }
            }
            return true;
        }

        private static float dist(float ax,float ay,float bx,float by) {
            return (float)Math.hypot(ax-bx,ay-by);
        }
        private void updateStick(float px,float py) {
            float dx=px-91,dy=py-(sceneHeight-111);
            float d=(float)Math.hypot(dx,dy);
            float multiple=Math.min(1,40/Math.max(1,d));
            joystickX=clamp(dx*multiple/40,-1,1);
            joystickY=clamp(dy*multiple/40,-1,1);
        }
    }
}

package com.takeo.necromancer;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.RectF;
import android.graphics.Shader;
import java.io.IOException;
import java.io.InputStream;
import java.util.Random;

/**
 * Caverna inicial: arte em camada de fundo e evolucao visual para a primeira base.
 * O PNG opcional app/src/main/assets/caverna_inicial.png e desenhado em toda a area.
 * Sem o PNG, o jogo continua funcional usando a ilustracao vetorial nativa.
 * A colisao permanece na simulacao World; decoracoes internas sao atravessaveis.
 */
final class CaveScene {
    private final Paint brush = new Paint(Paint.ANTI_ALIAS_FLAG | Paint.FILTER_BITMAP_FLAG);
    private final Bitmap background;
    private final float[][] pebbles = new float[180][3];
    private static final int INK=Color.rgb(31,30,43);
    private static final int STONE=Color.rgb(87,75,92);
    private static final int TEAL=Color.rgb(122,207,205);
    private static final int PALE=Color.rgb(195,177,187);

    CaveScene(Context context) {
        Bitmap img=null;
        try(InputStream stream=context.getAssets().open("caverna_inicial.png")) {
            img=BitmapFactory.decodeStream(stream);
        } catch(IOException ignored) {}
        background=img;
        Random random=new Random(1206);
        for(float[] mark:pebbles) {
            mark[0]=120+random.nextFloat()*(World.WIDTH-240);
            mark[1]=160+random.nextFloat()*(World.HEIGHT-300);
            mark[2]=1+random.nextFloat()*5;
        }
    }

    private void fill(int color) {
        brush.reset();
        brush.setAntiAlias(true);brush.setFilterBitmap(true);
        brush.setColor(color);brush.setStyle(Paint.Style.FILL);
    }
    private void ellipse(Canvas c,float x,float y,float rx,float ry,int color) {
        fill(color);c.drawOval(x-rx,y-ry,x+rx,y+ry,brush);
    }
    private void line(Canvas c,float x,float y,float xx,float yy,int color,float width) {
        fill(color);brush.setStrokeWidth(width);brush.setStrokeCap(Paint.Cap.ROUND);
        c.drawLine(x,y,xx,yy,brush);
    }
    private void ring(Canvas c,float x,float y,float r,int color,float width) {
        fill(color);brush.setStyle(Paint.Style.STROKE);
        brush.setStrokeWidth(width);c.drawCircle(x,y,r,brush);
    }
    private void polygon(Canvas c,float[] coords,int color) {
        Path path=new Path();path.moveTo(coords[0],coords[1]);
        for(int i=2;i<coords.length;i+=2)path.lineTo(coords[i],coords[i+1]);
        path.close();fill(color);c.drawPath(path,brush);
    }

    void draw(Canvas c,World world,float cameraX,float cameraY,float sceneHeight) {
        if(background!=null) {
            fill(Color.WHITE);
            c.drawBitmap(background,null,new RectF(0,0,World.WIDTH,World.HEIGHT),brush);
        } else {
            drawFallback(c,world,cameraX,cameraY,sceneHeight);
        }
        // Sinal ritualico reconhecivel antes da conquista, iluminado depois.
        drawSeal(c,1536,1070,world.baseClaimed,world.time);
        if(world.baseClaimed)drawSanctuary(c,world.time);
    }

    private void drawFallback(Canvas c,World world,float camX,float camY,float sceneH) {
        fill(STONE);
        brush.setShader(new LinearGradient(0,0,World.WIDTH,World.HEIGHT,
                Color.rgb(91,77,95),Color.rgb(76,68,88),Shader.TileMode.CLAMP));
        c.drawRect(0,0,World.WIDTH,World.HEIGHT,brush);brush.setShader(null);
        // Paredes irregulares nas extremidades: a camara central fica vazia.
        polygon(c,new float[]{0,0,World.WIDTH,0,World.WIDTH,205,2850,170,
                2510,232,2290,174,1910,220,1500,185,1170,243,810,181,400,215,0,160},INK);
        polygon(c,new float[]{0,0,180,0,225,380,135,725,210,1060,
                150,1490,220,1760,130,World.HEIGHT,0,World.HEIGHT},INK);
        polygon(c,new float[]{World.WIDTH,0,2850,0,2890,390,2810,760,
                2940,1240,2860,1590,2915,World.HEIGHT,World.WIDTH,World.HEIGHT},INK);
        polygon(c,new float[]{0,World.HEIGHT,World.WIDTH,World.HEIGHT,
                World.WIDTH,1850,2760,1900,2440,1830,2000,1910,1530,1860,
                1070,1910,680,1830,310,1920,0,1810},INK);
        polygon(c,new float[]{210,200,530,160,760,210,800,270,430,298,245,280},Color.rgb(62,55,73));
        polygon(c,new float[]{2380,200,2830,170,2860,350,2580,290,2390,280},Color.rgb(61,55,72));
        // Marmorizacao discreta, concentrada em poucos pontos.
        int begin=(int)Math.max(0,Math.floor(camX/180)-2),end=(int)Math.min(18,Math.ceil((camX+420)/180)+2);
        int yStart=(int)Math.max(0,Math.floor(camY/150)-2),yEnd=(int)Math.min(15,Math.ceil((camY+sceneH)/150)+2);
        for(int ix=begin;ix<=end;ix++)for(int iy=yStart;iy<=yEnd;iy++) {
            int hash=ix*73856093^iy*19349663;
            if((hash&7)!=0)continue;
            float x=ix*180+45+(hash&31);
            float y=iy*150+50+((hash>>>8)&31);
            ellipse(c,x,y,38,15,Color.argb(45,40,37,54));
            line(c,x-12,y-5,x+17,y-9,Color.argb(45,193,175,191),2);
        }
        for(float[] b:pebbles) {
            if(b[0]<camX-12 || b[0]>camX+432 || b[1]<camY-12 || b[1]>camY+sceneH+12)continue;
            ellipse(c,b[0],b[1],b[2]*1.6f,b[2]*.65f,Color.rgb(105,89,108));
        }
        // Grupos isolados e pouco numerosos de cristais.
        crystal(c,390,430,1.3f);crystal(c,2750,1510,1.4f);
        crystal(c,380,1660,1.0f);crystal(c,2570,420,1.0f);
        // Altura e misterio sem sacrificar o caminho.
        ellipse(c,1540,280,205,95,Color.rgb(48,42,60));
        ellipse(c,1540,255,170,70,Color.rgb(69,57,73));
        line(c,1420,255,1655,255,Color.rgb(125,98,112),7);
        // Nicho da entrada.
        ellipse(c,1550,1930,170,115,Color.rgb(46,41,57));
        ellipse(c,1550,1960,125,80,Color.rgb(71,61,79));
    }

    private void crystal(Canvas c,float x,float y,float s) {
        ellipse(c,x,y+18*s,56*s,18*s,Color.argb(100,36,34,52));
        polygon(c,new float[]{x-24*s,y+20*s,x-15*s,y-41*s,x+2*s,y-78*s,
                x+18*s,y-35*s,x+24*s,y+20*s},Color.rgb(134,122,182));
        polygon(c,new float[]{x+5*s,y+20*s,x+22*s,y-18*s,x+35*s,y-47*s,
                x+48*s,y+17*s},Color.rgb(157,142,207));
        polygon(c,new float[]{x-24*s,y+20*s,x-15*s,y-41*s,x+2*s,y-78*s,
                x+3*s,y+20*s},Color.rgb(171,155,215));
    }

    private void drawSeal(Canvas c,float x,float y,boolean owned,float time) {
        int outer=owned?Color.argb(195,122,207,205):Color.argb(75,188,166,184);
        int inner=owned?Color.argb(158,166,239,224):Color.argb(50,205,184,205);
        ellipse(c,x,y+11,191,114,Color.argb(55,29,27,40));
        ring(c,x,y,131,outer,owned?5:3);
        ring(c,x,y,111,inner,2);
        ring(c,x,y,40,inner,3);
        for(int i=0;i<8;i++) {
            double t=2*Math.PI*i/8;
            float ax=x+(float)Math.cos(t)*113,ay=y+(float)Math.sin(t)*113;
            float bx=x+(float)Math.cos(t)*128,by=y+(float)Math.sin(t)*128;
            line(c,ax,ay,bx,by,outer,3);
        }
        if(owned) {
            float pulse=4+(float)Math.sin(time*3)*3;
            ellipse(c,x,y-pulse,17,17,Color.argb(110,115,222,214));
            ellipse(c,x,y-pulse,7,7,Color.rgb(201,251,236));
        }
    }

    private void drawSanctuary(Canvas c,float time) {
        // Estruturas nas bordas da marca: piso de combate continua desimpedido.
        // Bancadas, tocha de alma, bau, bandeirolas e area de repouso.
        ellipse(c,1140,1120,127,55,Color.argb(125,35,31,45));
        polygon(c,new float[]{1050,1105,1230,1105,1220,1060,1058,1060},
                Color.rgb(110,84,88));
        line(c,1070,1105,1070,1140,Color.rgb(59,45,55),11);
        line(c,1204,1105,1204,1140,Color.rgb(59,45,55),11);
        ellipse(c,1110,1048,26,16,Color.rgb(71,119,113));
        ellipse(c,1110,1037,13,13,Color.rgb(157,226,209));
        polygon(c,new float[]{1970,1040,2110,1040,2130,1160,1950,1160},
                Color.rgb(94,77,83));
        polygon(c,new float[]{1970,1040,2110,1040,2100,1080,1980,1080},
                Color.rgb(141,115,112));
        line(c,2020,1080,2020,1158,Color.rgb(178,145,122),7);
        line(c,1540,905,1540,860,Color.rgb(113,86,97),8);
        ellipse(c,1540,843,22,26,Color.argb(105,114,220,211));
        ellipse(c,1540,837-(float)Math.sin(time*2)*3,11,17,TEAL);
        // Bandeiras de tecido penduradas ao redor do altar, decorativas.
        polygon(c,new float[]{1370,876,1410,876,1410,950,1390,937,1370,950},
                Color.rgb(104,70,91));
        polygon(c,new float[]{1660,876,1700,876,1700,950,1680,937,1660,950},
                Color.rgb(104,70,91));
        line(c,1350,875,1430,875,PALE,4);
        line(c,1640,875,1720,875,PALE,4);
    }
}

package com.takeo.necromancer;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;

/** Cenários originais da necrópole e do pântano, desenhados diretamente no Android. */
final class ExpeditionScene {
    private final Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);
    private final float[][] details=new float[110][3];

    ExpeditionScene() {
        java.util.Random rng=new java.util.Random(72319);
        for(float[] d:details) {
            d[0]=170+rng.nextFloat()*(World.WIDTH-340);
            d[1]=180+rng.nextFloat()*(World.HEIGHT-360);
            d[2]=rng.nextInt(4);
        }
    }
    private void fill(int color) {
        p.reset();p.setAntiAlias(true);p.setColor(color);
    }
    private void ellipse(Canvas c,float x,float y,float rx,float ry,int color) {
        fill(color);c.drawOval(x-rx,y-ry,x+rx,y+ry,p);
    }
    private void rect(Canvas c,float x,float y,float width,float height,int color) {
        fill(color);c.drawRect(x,y,x+width,y+height,p);
    }
    private void line(Canvas c,float x,float y,float xx,float yy,float w,int color) {
        fill(color);p.setStrokeCap(Paint.Cap.ROUND);p.setStrokeWidth(w);
        c.drawLine(x,y,xx,yy,p);
    }
    private void quad(Canvas c,float[] coords,int color) {
        Path path=new Path();path.moveTo(coords[0],coords[1]);
        for(int i=2;i<coords.length;i+=2)path.lineTo(coords[i],coords[i+1]);
        path.close();fill(color);c.drawPath(path,p);
    }

    void draw(Canvas c,World world,float cameraX,float cameraY,float viewHeight) {
        boolean grave=world.region==World.GRAVEYARD;
        int ground=grave?Color.rgb(66,64,72):Color.rgb(51,75,74);
        int paths=grave?Color.rgb(93,83,85):Color.rgb(68,93,81);
        int dark=grave?Color.rgb(43,40,53):Color.rgb(32,56,55);
        int light=grave?Color.rgb(124,111,116):Color.rgb(101,135,117);
        rect(c,0,0,World.WIDTH,World.HEIGHT,ground);

        // Grande caminho sinuoso, deixando amplas clareiras para o exército.
        Path track=new Path();
        track.moveTo(480,1740);
        track.cubicTo(850,1350,1050,1080,1590,1110);
        track.cubicTo(2150,1050,2190,490,2640,365);
        fill(paths);p.setStyle(Paint.Style.STROKE);
        p.setStrokeCap(Paint.Cap.ROUND);p.setStrokeWidth(175);
        c.drawPath(track,p);
        fill(grave?Color.rgb(105,94,93):Color.rgb(82,105,89));
        p.setStyle(Paint.Style.STROKE);p.setStrokeCap(Paint.Cap.ROUND);
        p.setStrokeWidth(115);c.drawPath(track,p);

        float right=cameraX+420+150, bottom=cameraY+viewHeight+150;
        for(float[] d:details) {
            float x=d[0],y=d[1];
            if(x<cameraX-150 || x>right || y<cameraY-150 || y>bottom)continue;
            if(grave) {
                if(d[2]<2)graveStone(c,x,y,light,dark);
                else {
                    ellipse(c,x,y,33,8,dark);
                    line(c,x,y,x-14,y-22,3,light);
                    line(c,x,y,x+10,y-16,2,light);
                }
            } else {
                if(d[2]<2) {
                    ellipse(c,x,y,56,24,dark);
                    ellipse(c,x-7,y-4,39,15,Color.rgb(66,104,102));
                    ellipse(c,x+14,y-9,19,7,Color.rgb(74,120,110));
                } else {
                    ellipse(c,x,y,25,8,dark);
                    line(c,x,y,x-6,y-26,5,light);
                    ellipse(c,x-6,y-26,14,8,Color.rgb(129,155,139));
                    ellipse(c,x+14,y-8,10,5,Color.rgb(122,174,149));
                }
            }
        }
        // Rochas correspondentes à lógica de colisão.
        for(float[] rock:world.activeRocks()) {
            float x=rock[0],y=rock[1],r=rock[2];
            if(x+r<cameraX-80 || x-r>right || y+r<cameraY-80 || y-r>bottom)continue;
            ellipse(c,x,y+r*.6f,r*1.1f,r*.56f,dark);
            ellipse(c,x,y,r,r*.78f,light);
            ellipse(c,x-r*.2f,y-r*.27f,r*.5f,r*.26f,
                    grave?Color.rgb(153,134,132):Color.rgb(122,151,131));
        }
        if(grave) {
            // Mausoléu no canto superior direito.
            rect(c,2490,140,280,115,dark);
            quad(c,new float[]{2460,140,2630,75,2800,140},light);
            rect(c,2580,165,85,90,Color.rgb(27,25,38));
            line(c,2623,142,2623,260,7,Color.rgb(135,121,124));
        } else {
            ellipse(c,2590,300,230,124,Color.rgb(37,69,66));
            ellipse(c,2590,290,187,94,Color.rgb(72,116,107));
            ellipse(c,2590,280,125,45,Color.rgb(82,134,118));
            line(c,2570,225,2595,150,11,Color.rgb(61,63,58));
            line(c,2595,150,2632,120,8,Color.rgb(61,63,58));
        }
    }

    private void graveStone(Canvas c,float x,float y,int stone,int shadow) {
        ellipse(c,x,y+9,27,8,shadow);
        rect(c,x-13,y-28,26,36,stone);
        ellipse(c,x,y-28,13,12,stone);
        line(c,x-3,y-23,x-3,y-8,3,shadow);
        line(c,x-10,y-17,x+5,y-17,3,shadow);
    }

    static void portal(Canvas c,float x,float y,int color,float phase,String ignored) {
        Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);
        p.setStyle(Paint.Style.STROKE);
        p.setStrokeWidth(5);
        p.setColor(Color.argb(188,Color.red(color),Color.green(color),Color.blue(color)));
        c.drawOval(x-44,y-63,x+44,y+34,p);
        p.setStrokeWidth(2);
        c.drawOval(x-35,y-55,x+35,y+27,p);
        p.setStyle(Paint.Style.FILL);
        p.setColor(Color.argb(50,Color.red(color),Color.green(color),Color.blue(color)));
        c.drawOval(x-30,y-48,x+30,y+21,p);
        float pulse=(float)Math.sin(phase*3f)*7;
        p.setColor(color);
        c.drawCircle(x,y-16,5+pulse*.25f,p);
    }
}

package com.takeo.necromancer;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;

/**
 * Silhueta original para o protótipo: chapéu largo, cabelos claros,
 * manto castanho-violeta, cajado e lanterna espectral.
 * Adaptada para permanecer legível quando a personagem ocupa poucos pixels.
 */
final class WitchSprite {
    private final Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);
    private static final int OUTLINE=Color.rgb(44,36,47);
    private static final int CAPE=Color.rgb(103,79,96);
    private static final int TRIM=Color.rgb(175,145,142);
    private static final int HAIR=Color.rgb(220,209,211);
    private static final int SKIN=Color.rgb(230,203,187);
    private static final int GLOW=Color.rgb(155,222,225);

    private void color(int fill) {
        p.reset();p.setAntiAlias(true);p.setColor(fill);
    }
    private void ell(Canvas c,float x,float y,float w,float h,int fill) {
        color(fill);c.drawOval(x-w,y-h,x+w,y+h,p);
    }
    private void ln(Canvas c,float x,float y,float xx,float yy,float width,int fill) {
        color(fill);p.setStrokeWidth(width);p.setStrokeCap(Paint.Cap.ROUND);
        c.drawLine(x,y,xx,yy,p);
    }
    private void poly(Canvas c,int fill,float... points) {
        Path path=new Path();path.moveTo(points[0],points[1]);
        for(int i=2;i<points.length;i+=2)path.lineTo(points[i],points[i+1]);
        path.close();color(fill);c.drawPath(path,p);
    }
    private void ring(Canvas c,float x,float y,float radius,float thickness,int tint) {
        color(tint);p.setStyle(Paint.Style.STROKE);
        p.setStrokeWidth(thickness);c.drawCircle(x,y,radius,p);
    }

    void draw(Canvas canvas,World world) {
        canvas.save();canvas.translate(world.x,world.y);
        float sway=(float)Math.sin(world.time*6f)*1.4f;
        ell(canvas,0,16,24,7,Color.rgb(19,20,29));
        // Manto volumoso com bainha rasgada.
        poly(canvas,OUTLINE,-13,-19,12,-20,26,14,12,18,5,14,-3,21,-13,18,-24,12);
        poly(canvas,world.mantleTier>0?Color.rgb(88,65,108):CAPE,
                -11,-20,10,-20,20,12,8,14,-6,17,-19,12);
        poly(canvas,Color.rgb(207,185,173),-10,7,-2,12,5,7,12,13,5,18,-4,17);
        ln(canvas,-11,13,-8,19,5,Color.rgb(80,58,58));
        ln(canvas,12,13,13,19,5,Color.rgb(80,58,58));
        // Cabelos prateados em mechas independentes para leitura na visão superior.
        ln(canvas,-10,-34,-22+sway,-10,10,HAIR);
        ln(canvas,-9,-34,-17+sway,3,6,Color.rgb(198,188,205));
        ln(canvas,10,-35,19+sway,7,9,HAIR);
        ln(canvas,12,-35,25+sway,-4,6,Color.rgb(205,195,209));
        ell(canvas,0,-33,14,17,HAIR);
        ell(canvas,0,-30,10,11,SKIN);
        ell(canvas,-5,-30,1.4f,2.1f,Color.rgb(111,92,110));
        ell(canvas,5,-30,1.4f,2.1f,Color.rgb(111,92,110));
        ell(canvas,-4.6f,-30.5f,.6f,1.1f,GLOW);
        ell(canvas,5.4f,-30.5f,.6f,1.1f,GLOW);
        // Tecido de abas e lacinho na gola.
        poly(canvas,TRIM,-9,-19,0,-14,9,-20,5,-9,0,-13,-5,-10);
        ell(canvas,0,-17,3,3,Color.rgb(171,199,202));
        // Chapéu de abas largas que identifica a personagem à distância.
        ell(canvas,0,-49,31,11,OUTLINE);
        ell(canvas,0,-52,30,8,Color.rgb(72,57,71));
        poly(canvas,OUTLINE,-17,-52,-12,-76,4,-82,17,-72,19,-51);
        poly(canvas,Color.rgb(84,67,79),-15,-53,-8,-73,3,-79,15,-70,16,-52);
        ln(canvas,-17,-54,16,-54,4,TRIM);
        ell(canvas,10,-55,7,5,Color.rgb(201,193,171));
        ell(canvas,7,-55,2.3f,2.4f,OUTLINE);
        ell(canvas,13,-55,2.3f,2.4f,OUTLINE);
        // Cabo de osso e lanterna azul: gesto sutil durante conjuração.
        float shift=world.attackFlash>0?7:0;
        ln(canvas,22,-6,34+shift,-70,4,Color.rgb(173,150,134));
        ln(canvas,34+shift,-70,42+shift,-82,4,Color.rgb(194,179,158));
        ell(canvas,43+shift,-85,7,7,Color.rgb(210,204,184));
        ell(canvas,43+shift,-85,2.2f,2.2f,OUTLINE);
        ln(canvas,32+shift,-56,38+shift,-56,2,Color.rgb(203,181,150));
        ell(canvas,38+shift,-47,8,11,Color.rgb(67,111,124));
        ell(canvas,38+shift,-47,5,7,world.staffTier>0?Color.rgb(171,177,244):GLOW);
        ell(canvas,38+shift,-47,2,4,Color.rgb(219,238,226));
        ring(canvas,38+shift,-47,12,2,Color.argb(60,157,224,221));
        if(world.hitFlash>0) {
            ell(canvas,0,-31,27,36,Color.argb(76,232,113,112));
        }
        canvas.restore();
    }
}

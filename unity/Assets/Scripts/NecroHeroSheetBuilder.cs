using System;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Builds a frame atlas from 16 individually illustrated poses extracted
    /// from the approved original heroine art studies. Four actual directional
    /// base portraits (front/back/left/right) are used, not a mirrored face.
    ///
    /// Source PNG layout: 4 columns x 4 rows, TOP to BOTTOM:
    /// [front,back,left,right]
    /// [idle A,idle B,walk A,walk B]
    /// [attack A,attack B,cast A,cast B]
    /// [raise A,raise B,hurt A,hurt B]
    ///
    /// Unity atlas layout: 8 columns x 24 rows (six states * four directions).
    /// The source sheets are painted. Secondary animation frames add separately
    /// keyed limb / cloak motion and hand-drawn spell accents on top of the poses.
    /// </summary>
    public static class NecroHeroSheetBuilder
    {
        public const int Cell=96;
        public const int Columns=8;
        public static readonly int[] Counts={4,8,6,6,8,3};

        public static Texture2D Build(Texture2D source)
        {
            if(source==null || source.width%4!=0 || source.height%4!=0)
                return null;
            int sourceWidth,sourceHeight;
            Color32[] src=NecroFrameLibrary.ReadablePixels(source,
                out sourceWidth,out sourceHeight);
            int tileW=sourceWidth/4,tileH=sourceHeight/4;
            int outW=Columns*Cell,outH=24*Cell;
            Color32[] pixels=new Color32[outW*outH];
            for(int pose=0;pose<6;pose++)
            for(int direction=0;direction<4;direction++)
            for(int frame=0;frame<Counts[pose];frame++)
                PaintFrame(src,sourceWidth,sourceHeight,tileW,tileH,
                    pixels,outW,pose,direction,frame,Counts[pose]);

            Texture2D atlas=new Texture2D(outW,outH,TextureFormat.RGBA32,false);
            atlas.name="Necromancer - hand painted directions and animation";
            atlas.filterMode=FilterMode.Bilinear;
            atlas.wrapMode=TextureWrapMode.Clamp;
            atlas.SetPixels32(pixels);
            atlas.Apply(false,true);
            return atlas;
        }

        static int SourceTile(int pose,int direction,int frame)
        {
            // Distinct illustrated front-facing poses and distinct illustrated
            // left/right/back viewpoints. No bogus front sprite painted grey.
            if(direction!=0)return direction;
            switch(pose)
            {
                case 0:return 4+(frame%2);
                case 1:return 6+(frame%2);
                case 2:return 8+(frame%2);
                case 3:return 10+(frame%2);
                case 4:return 12+(frame%2);
                default:return 14+(frame%2);
            }
        }

        static void PaintFrame(Color32[] src,int sw,int sh,int tileW,int tileH,
            Color32[] atlas,int aw,int pose,int direction,int frame,int count)
        {
            int tile=SourceTile(pose,direction,frame);
            int tileColumn=tile%4;
            // PNG top-down tile order; Color32[] in Unity starts at BOTTOM.
            int tileRowFromBottom=3-tile/4;
            int startX=tileColumn*tileW;
            int startY=tileRowFromBottom*tileH;
            int baseX=frame*Cell,baseY=(pose*4+direction)*Cell;

            float cycle=frame*Mathf.PI*2f/count;
            float walk=pose==1?Mathf.Sin(cycle):0f;
            float breathe=pose==0?Mathf.Sin(cycle)*1.0f:0f;
            float strike=pose==2?Mathf.Sin((frame+1f)/count*Mathf.PI):0f;
            float cast=pose==3?Mathf.Sin((frame+1f)/count*Mathf.PI):0f;
            float raise=pose==4?Mathf.Sin((frame+1f)/count*Mathf.PI):0f;
            float hurt=pose==5?(frame==0?1f:.35f):0f;

            // Rigid, pixel-addressed layers rather than distorting the face:
            // hat/hair, torso and hem are shifted independently.
            for(int y=0;y<86;y++)
            {
                float ny=(y-4f)/79f;
                int layer=ny>.58f?2:ny>.35f?1:0;
                float shiftX=layer==2?(-strike*3f+hurt*2f):
                    layer==1?walk*1.5f:
                    (walk*3f+raise*1.4f);
                float shiftY=layer==2?breathe+cast*2f:
                    layer==1?walk*.9f:
                    -Mathf.Abs(walk)*1.9f;
                shiftY+=raise*(layer==2?3f:1f);
                int srcY=Mathf.RoundToInt((y-shiftY-5f)*(tileH/82f));
                if(srcY<0||srcY>=tileH)continue;
                int sy=startY+srcY;
                for(int x=4;x<Cell-4;x++)
                {
                    int srcX=Mathf.RoundToInt((x-shiftX-10f)*(tileW/76f));
                    if(srcX<0||srcX>=tileW)continue;
                    Color32 c=src[sy*sw+startX+srcX];
                    if(c.a<12)continue;
                    if(hurt>0f)
                    {
                        c.r=(byte)Mathf.Lerp(c.r,244f,.37f*hurt);
                        c.g=(byte)Mathf.Lerp(c.g,130f,.29f*hurt);
                        c.b=(byte)Mathf.Lerp(c.b,165f,.18f*hurt);
                    }
                    atlas[(baseY+y)*aw+baseX+x]=c;
                }
            }

            int handX=baseX+(direction==2?23:direction==3?72:70);
            int handY=baseY+55;
            if(pose==2)
            {
                // Short, pale-violet slash follows the animation frame.
                float start=-.8f+frame*.17f;
                Arc(atlas,aw,baseX,baseY,handX,handY,17+frame%3,
                    start,start+1.12f,new Color32(190,123,246,218),3);
                Arc(atlas,aw,baseX,baseY,handX,handY,19+frame%3,
                    start,start+.72f,new Color32(238,216,255,175),1);
            }
            if(pose==3)
            {
                // Free automatic projectile: casting VFX never alters mana.
                int radius=3+Mathf.RoundToInt(8f*cast);
                Glow(atlas,aw,baseX,baseY,handX,handY,radius,
                    new Color32(136,244,225,215));
                Glow(atlas,aw,baseX,baseY,handX,handY,Mathf.Max(1,radius/3),
                    new Color32(233,255,251,248));
            }
            if(pose==4)
            {
                int circleX=baseX+48,circleY=baseY+12;
                EllipseRing(atlas,aw,baseX,baseY,circleX,circleY,
                    13+Mathf.RoundToInt(18f*raise),4+Mathf.RoundToInt(3f*raise),
                    new Color32(92,240,211,180));
                for(int i=0;i<5;i++)
                {
                    float d=(i*2.39996f+frame*.47f);
                    int x=circleX+Mathf.RoundToInt(Mathf.Cos(d)*(6f+i*4f));
                    int y=circleY+13+Mathf.RoundToInt(raise*(11+i*2));
                    Glow(atlas,aw,baseX,baseY,x,y,2,
                        new Color32(147,255,219,210));
                }
            }
        }

        static void Pixel(Color32[] arr,int aw,int left,int bottom,
            int x,int y,Color32 color)
        {
            if(x<left || x>=left+Cell || y<bottom || y>=bottom+Cell)return;
            int index=y*aw+x;
            if(index<0 || index>=arr.Length)return;
            arr[index]=color;
        }
        static void Glow(Color32[] arr,int aw,int left,int bottom,
            int cx,int cy,int radius,Color32 color)
        {
            for(int y=-radius;y<=radius;y++)
            for(int x=-radius;x<=radius;x++)
            {
                float d=(x*x+y*y)/(float)Mathf.Max(1,radius*radius);
                if(d>1f)continue;
                Color32 c=color;
                c.a=(byte)(color.a*(1f-d*.60f));
                Pixel(arr,aw,left,bottom,cx+x,cy+y,c);
            }
        }
        static void Arc(Color32[] arr,int aw,int left,int bottom,
            int cx,int cy,int radius,float from,float to,Color32 col,int thickness)
        {
            for(float a=from;a<=to;a+=.045f)
            {
                int x=cx+Mathf.RoundToInt(Mathf.Cos(a)*radius);
                int y=cy+Mathf.RoundToInt(Mathf.Sin(a)*radius);
                for(int t=0;t<thickness;t++)
                    Pixel(arr,aw,left,bottom,x+t,y,col);
            }
        }
        static void EllipseRing(Color32[] arr,int aw,int left,int bottom,
            int cx,int cy,int rx,int ry,Color32 color)
        {
            for(float t=0;t<Mathf.PI*2;t+=.06f)
            {
                int x=cx+Mathf.RoundToInt(Mathf.Cos(t)*rx);
                int y=cy+Mathf.RoundToInt(Mathf.Sin(t)*ry);
                Pixel(arr,aw,left,bottom,x,y,color);
                Pixel(arr,aw,left,bottom,x,y+1,color);
            }
        }
    }
}

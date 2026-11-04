using System;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Original 160px hand-authored vector-like character art rasterized once.
    /// A stable 4-column x 4-row source with four distinct directions:
    /// row 0 idle A, row 1 idle B, row 2 walk A, row 3 walk B.
    /// Rendered at high resolution, with a consistent silhouette and no
    /// low-resolution source stretching or perspective distortion.
    /// </summary>
    public static class NecroHeroPaintedSource
    {
        const int Tile=160;
        const int Canvas=Tile*4;
        static readonly Color32 Clear=new Color32(0,0,0,0);
        static readonly Color32 Ink=Hex(0x252032);
        static readonly Color32 Hat=Hex(0x654768);
        static readonly Color32 HatLight=Hex(0x98709a);
        static readonly Color32 Hair=Hex(0xe6e0ef);
        static readonly Color32 HairShade=Hex(0xb3acc9);
        static readonly Color32 Skin=Hex(0xf6ded9);
        static readonly Color32 Coat=Hex(0x55405c);
        static readonly Color32 CoatLight=Hex(0x8e6d88);
        static readonly Color32 Gold=Hex(0xd3ad8a);
        static readonly Color32 Bone=Hex(0xf1e7d5);
        static readonly Color32 Mana=Hex(0x65dccc);
        static readonly Color32 Core=Hex(0xdbfff2);
        static readonly Color32 Boots=Hex(0x323044);
        static readonly Color32 Eye=Hex(0x44394e);

        static Color32 Hex(int hex)
        {
            return new Color32((byte)(hex>>16),(byte)(hex>>8),(byte)hex,255);
        }

        static void Put(Color32[] pix,int ox,int oy,int x,int y,Color32 c)
        {
            if(x<0||y<0||x>=Tile||y>=Tile)return;
            // The sheet is stored in Unity bottom-up order.
            pix[(oy+y)*Canvas+ox+x]=c;
        }
        static void Oval(Color32[] p,int ox,int oy,int cx,int cy,int rx,int ry,Color32 c)
        {
            if(rx<=0||ry<=0)return;
            for(int y=Mathf.Max(0,cy-ry);y<=Mathf.Min(Tile-1,cy+ry);y++)
            for(int x=Mathf.Max(0,cx-rx);x<=Mathf.Min(Tile-1,cx+rx);x++)
            {
                float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
                if(dx*dx+dy*dy<=1f)Put(p,ox,oy,x,y,c);
            }
        }
        static void Line(Color32[] p,int ox,int oy,int x1,int y1,int x2,int y2,int thick,Color32 color)
        {
            int n=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(
                new Vector2(x1,y1),new Vector2(x2,y2))));
            for(int i=0;i<=n;i++)
            {
                float u=i/(float)n;
                Oval(p,ox,oy,Mathf.RoundToInt(Mathf.Lerp(x1,x2,u)),
                    Mathf.RoundToInt(Mathf.Lerp(y1,y2,u)),
                    Mathf.Max(1,thick/2),Mathf.Max(1,thick/2),color);
            }
        }
        static void Polygon(Color32[] p,int ox,int oy,Color32 color,params int[] points)
        {
            int min=Tile,max=0,count=points.Length/2;
            for(int i=1;i<points.Length;i+=2)
            {
                min=Mathf.Min(min,points[i]);max=Mathf.Max(max,points[i]);
            }
            for(int y=Mathf.Max(0,min);y<=Mathf.Min(Tile-1,max);y++)
            {
                float[] intersections=new float[count];int n=0;
                for(int i=0;i<count;i++)
                {
                    int j=(i+1)%count;
                    int x1=points[2*i],v1=points[2*i+1];
                    int x2=points[2*j],v2=points[2*j+1];
                    if((v1<=y&&v2>y)||(v2<=y&&v1>y))
                        intersections[n++]=x1+(y-v1)*(x2-x1)/(float)(v2-v1);
                }
                Array.Sort(intersections,0,n);
                for(int i=0;i+1<n;i+=2)
                    for(int x=Mathf.Max(0,Mathf.CeilToInt(intersections[i]));
                        x<=Mathf.Min(Tile-1,Mathf.FloorToInt(intersections[i+1]));x++)
                        Put(p,ox,oy,x,y,color);
            }
        }
        static void Paint(Color32[] p,int direction,int variant)
        {
            bool back=direction==1;
            bool side=direction>=2;
            bool walking=variant>=2;
            int phase=(variant%2==0)?-1:1;
            int lFoot=walking?phase*5:0;
            int rFoot=-lFoot;
            int arm=walking?phase*5:phase;
            int ox=direction*Tile,oy=(3-variant)*Tile;

            // Place painting coordinates in the same foot-anchored space
            // for all directions. The left view swaps horizontal coordinates
            // during rasterization after painting the right-side profile.
            if(direction==2)
            {
                Color32[] temp=new Color32[Canvas*Canvas];
                PaintBody(temp,0,0,back,side,phase,lFoot,rFoot,arm);
                for(int y=0;y<Tile;y++)for(int x=0;x<Tile;x++)
                {
                    Color32 color=temp[y*Canvas+x];
                    if(color.a>0)Put(p,ox,oy,Tile-1-x,y,color);
                }
            }
            else PaintBody(p,ox,oy,back,side,phase,lFoot,rFoot,arm);
        }
        static void PaintBody(Color32[] p,int ox,int oy,bool back,bool side,
            int phase,int lFoot,int rFoot,int arm)
        {
            // Foot shadow and cloak: large silhouette keeps character readable.
            Oval(p,ox,oy,80,17,32,8,new Color32(21,20,31,80));
            Polygon(p,ox,oy,Ink,60,66,99,68,112,25,82,18,47,25);
            Polygon(p,ox,oy,CoatLight,64,64,98,65,105,28,79,23,54,28);
            // Independent boots. Pose variants make an actual stride.
            Polygon(p,ox,oy,Ink,63,36,76,34,76+lFoot,20,53+lFoot,20);
            Polygon(p,ox,oy,Boots,65,36,73,34,73+lFoot,23,58+lFoot,22);
            Polygon(p,ox,oy,Ink,85,36,98,35,109+rFoot,20,85+rFoot,20);
            Polygon(p,ox,oy,Boots,88,37,96,34,104+rFoot,23,88+rFoot,23);
            Line(p,ox,oy,57+lFoot,23,72+lFoot,23,2,Gold);
            Line(p,ox,oy,87+rFoot,23,102+rFoot,23,2,Gold);
            // Wide tapered jacket and contrasting front panel.
            Polygon(p,ox,oy,Ink,59,74,99,73,109,31,82,24,49,32);
            Polygon(p,ox,oy,Coat,61,70,98,71,105,34,81,28,55,34);
            Polygon(p,ox,oy,CoatLight,61,64,75,59,78,34,63,33);
            Line(p,ox,oy,56,35,81,27,3,Gold);
            Line(p,ox,oy,81,27,103,35,3,Gold);
            Line(p,ox,oy,81,39,81,70,2,HatLight);
            Oval(p,ox,oy,80,62,5,7,Ink);
            Oval(p,ox,oy,80,62,3,5,Mana);
            // Sleeves animate independently.
            Line(p,ox,oy,61,66,56-arm,47,14,Ink);
            Line(p,ox,oy,61,66,56-arm,47,9,CoatLight);
            Line(p,ox,oy,99,66,103+arm,46,14,Ink);
            Line(p,ox,oy,99,66,103+arm,46,9,CoatLight);
            // Hair is light on dark to carry silhouette at 70 pixels.
            if(back)
            {
                Oval(p,ox,oy,80,89,28,32,Ink);
                Oval(p,ox,oy,80,90,25,30,Hair);
                for(int i=0;i<4;i++)
                    Line(p,ox,oy,62+i*12,92,59+i*12,69,3,HairShade);
            }
            else
            {
                Oval(p,ox,oy,80,92,29,27,Ink);
                Oval(p,ox,oy,80,92,25,23,Hair);
                Oval(p,ox,oy,82,90,21,19,Ink);
                Oval(p,ox,oy,82,91,19,17,Skin);
                if(side)
                {
                    Oval(p,ox,oy,84,90,5,7,Eye);
                    Oval(p,ox,oy,85,92,2,3,Bone);
                    Line(p,ox,oy,77,80,87,80,2,HatLight);
                }
                else
                {
                    foreach(int x in new[]{74,92})
                    {
                        Oval(p,ox,oy,x,91,5,7,Eye);
                        Oval(p,ox,oy,x-1,93,2,3,Bone);
                    }
                    Line(p,ox,oy,79,78,86,77,2,HatLight);
                }
                Line(p,ox,oy,55,96,65,75,7,Hair);
                Line(p,ox,oy,107,96,102,73,7,Hair);
                Line(p,ox,oy,56,77,65,78,3,HairShade);
            }
            // Giant violet witch hat. Strong black rim and cream skull badge.
            Polygon(p,ox,oy,Ink,46,100,52,126,70,150,83,152,98,135,118,103);
            Polygon(p,ox,oy,Hat,51,101,57,124,72,145,82,147,96,132,113,104);
            Polygon(p,ox,oy,HatLight,62,106,72,137,83,139,87,128,92,114);
            Oval(p,ox,oy,80,102,45,11,Ink);
            Oval(p,ox,oy,81,104,41,8,Hat);
            Line(p,ox,oy,53,111,105,111,5,Gold);
            Oval(p,ox,oy,87,112,12,11,Ink);
            Oval(p,ox,oy,87,113,9,8,Bone);
            Oval(p,ox,oy,83,115,3,3,Eye);
            Oval(p,ox,oy,92,115,3,3,Eye);
            Polygon(p,ox,oy,Eye,86,109,89,106,91,109);
            Oval(p,ox,oy,50,106,7,7,HatLight);
            // Staff clearly differentiated from hair; held on the right.
            Line(p,ox,oy,115,35,117,129,8,Ink);
            Line(p,ox,oy,115,35,117,129,5,Gold);
            Line(p,ox,oy,117,129,107,139,4,Gold);
            Line(p,ox,oy,117,129,127,138,4,Gold);
            Oval(p,ox,oy,117,130,15,17,new Color32(62,160,153,95));
            Oval(p,ox,oy,117,130,10,12,Ink);
            Oval(p,ox,oy,117,130,8,10,Mana);
            Oval(p,ox,oy,117,132,4,6,Core);
        }

        public static Texture2D Build()
        {
            var pixels=new Color32[Canvas*Canvas];
            for(int dir=0;dir<4;dir++)
                for(int v=0;v<4;v++)Paint(pixels,dir,v);
            var texture=new Texture2D(Canvas,Canvas,TextureFormat.RGBA32,false);
            texture.name="Necromancer heroine - four views and painted idle/walk";
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(pixels);
            texture.Apply(false,false);
            return texture;
        }
    }
}

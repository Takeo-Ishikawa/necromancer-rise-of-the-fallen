using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Small original 2D artwork generated in Unity, independent from copyrighted
    /// reference art. Transparently prefers separately imported production sprites.
    /// </summary>
    public static class NecroArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        static readonly Color dark=new Color(.16f,.14f,.22f);
        static readonly Color bone=new Color(.91f,.84f,.78f);
        static readonly Color ash=new Color(.83f,.78f,.81f);
        static readonly Color aqua=new Color(.62f,.91f,.89f);
        static readonly Color gold=new Color(.80f,.69f,.53f);
        static readonly Color cape=new Color(.43f,.32f,.42f);
        static readonly Color hood=new Color(.29f,.22f,.32f);

        public static Sprite Get(string kind)
        {
            Sprite imported=Resources.Load<Sprite>("Sprites/"+kind);
            if(imported!=null)return imported;
            if(cache.TryGetValue("sprite/"+kind,out Sprite s))return s;
            Texture2D t=Make(128);
            if(kind=="necromante")DrawWitch(t);
            else if(kind=="crypt_altar")DrawCryptAltar(t);
            else if(kind=="focus"||kind=="mantle"||kind=="cristal"||kind=="ossos"||kind=="fungo")
                DrawItem(t,kind);
            else DrawMonster(t,kind);
            t.Apply();
            return cache["sprite/"+kind]=Sprite.Create(t,new Rect(0,0,128,128),
                new Vector2(.5f,.42f),100f);
        }

        static void DrawWitch(Texture2D t)
        {
            // Oversized skirt and layered hem ensure readability at game scale.
            Oval(t,64,33,30,20,dark);
            Oval(t,64,40,29,32,cape);
            Oval(t,59,32,17,15,new Color(.60f,.47f,.56f));
            Oval(t,64,35,12,13,new Color(.89f,.81f,.75f));
            for(int i=0;i<6;i++)
            {
                int x=39+i*10;
                Line(t,x,24,x+(i%2==0?-4:4),18,3,new Color(.78f,.69f,.67f));
            }
            Oval(t,53,22,7,10,new Color(.25f,.20f,.25f));
            Oval(t,78,22,7,10,new Color(.25f,.20f,.25f));
            Oval(t,64,69,20,24,hood);
            // Pale hair strands below the hat brim.
            Line(t,47,81,35,42,11,ash);
            Line(t,50,78,40,30,7,new Color(.75f,.73f,.81f));
            Line(t,80,80,88,39,10,ash);
            Line(t,83,76,97,32,7,new Color(.77f,.73f,.79f));
            Oval(t,64,73,16,21,ash);
            Oval(t,64,70,12,15,new Color(.93f,.79f,.76f));
            Oval(t,58,73,3,4,new Color(.47f,.39f,.43f));
            Oval(t,72,73,3,4,new Color(.47f,.39f,.43f));
            Dot(t,59,74,aqua);Dot(t,73,74,aqua);
            Line(t,62,65,67,65,2,new Color(.57f,.42f,.45f));
            // Witch hat: extremely broad brim, soft mauve point and bone brooch.
            Oval(t,64,103,58,13,dark);
            Oval(t,64,107,56,10,new Color(.43f,.34f,.43f));
            Poly(t,new Color(.24f,.20f,.29f),
                35,102,42,118,58,127,80,124,93,103);
            Poly(t,new Color(.43f,.32f,.40f),
                39,103,48,116,60,125,80,121,89,104);
            Line(t,36,102,93,102,5,new Color(.71f,.57f,.57f));
            Oval(t,81,106,9,5,bone);
            Oval(t,78,106,2,2,dark);Oval(t,85,106,2,2,dark);
            // Staff with floating lantern and green-blue soul light.
            Line(t,103,30,115,91,6,new Color(.62f,.47f,.35f));
            Line(t,115,91,107,116,4,bone);
            Oval(t,105,116,10,8,bone);
            Oval(t,101,116,3,3,dark);
            Oval(t,110,117,3,3,dark);
            Oval(t,115,85,12,15,new Color(.34f,.51f,.55f,.83f));
            Oval(t,115,85,8,12,aqua);
            Oval(t,116,88,3,5,Color.white);
            Line(t,101,101,119,103,3,gold);
            Oval(t,65,53,5,5,gold);
        }

        static void DrawCryptAltar(Texture2D t)
        {
            // Tomb slab, enchanted crystal, and spectral rising wisps.
            Oval(t,64,21,43,15,dark);
            Poly(t,new Color(.28f,.26f,.34f),
                23,22,35,40,97,40,106,22,95,13,34,13);
            Poly(t,new Color(.53f,.46f,.54f),
                31,42,31,78,96,78,97,42);
            Poly(t,new Color(.35f,.28f,.40f),
                35,46,35,74,92,74,92,46);
            Line(t,49,65,78,65,4,gold);
            Line(t,64,48,64,73,3,bone);
            Line(t,51,58,77,58,3,bone);
            Oval(t,64,87,25,17,new Color(.24f,.55f,.53f,.36f));
            Poly(t,new Color(.58f,.91f,.84f,.86f),
                64,121,86,90,64,72,42,90);
            Poly(t,new Color(.86f,.99f,.91f,.95f),
                64,114,77,92,64,82,52,92);
            Line(t,38,82,31,106,3,aqua);
            Line(t,90,80,100,108,3,aqua);
            Oval(t,29,111,4,5,aqua);
            Oval(t,100,112,4,5,aqua);
        }

        static void DrawMonster(Texture2D t,string kind)
        {
            Color primary=kind=="rat"?new Color(.67f,.45f,.50f):
                kind=="bat"?new Color(.55f,.45f,.70f):
                kind=="slime"?new Color(.52f,.69f,.61f):
                kind=="snake"?new Color(.45f,.67f,.55f):
                kind=="spider"?new Color(.45f,.37f,.58f):
                kind=="skeleton"?bone:
                kind=="wisp"?new Color(.51f,.75f,.82f,.84f):
                kind=="marsh_lord"?new Color(.38f,.59f,.44f):
                new Color(.57f,.48f,.62f);
            if(kind=="wisp")
            {
                Oval(t,64,57,33,41,new Color(.40f,.69f,.75f,.30f));
                Oval(t,64,69,23,30,primary);
                Line(t,51,47,42,18,9,primary);
                Line(t,78,44,89,16,9,primary);
            }
            else if(kind=="slime")
            {
                Oval(t,64,46,40,32,dark);
                Oval(t,64,51,38,31,primary);
                Oval(t,52,65,14,8,new Color(.77f,.88f,.74f,.6f));
            }
            else if(kind=="bat")
            {
                Oval(t,35,67,32,17,dark);Oval(t,93,67,32,17,dark);
                Oval(t,36,70,28,13,primary);Oval(t,92,70,28,13,primary);
                Oval(t,64,68,19,25,primary);
                Poly(t,primary,48,77,52,104,62,86,76,104,80,77);
            }
            else if(kind=="snake")
            {
                Oval(t,60,45,39,17,dark);
                Oval(t,72,53,35,20,primary);
                Oval(t,49,70,24,21,primary);
                Oval(t,52,75,18,14,new Color(.69f,.79f,.64f));
            }
            else if(kind=="spider")
            {
                for(int i=0;i<4;i++)
                {
                    int y=35+i*14;
                    Line(t,50,y,19,y+(i-2)*8,6,primary);
                    Line(t,78,y,109,y+(i-2)*8,6,primary);
                }
                Oval(t,63,61,27,32,dark);
                Oval(t,64,65,24,27,primary);
            }
            else if(kind=="skeleton")
            {
                Oval(t,65,47,26,25,dark);
                Oval(t,64,72,27,25,bone);
                Oval(t,55,76,6,8,dark);Oval(t,73,76,6,8,dark);
                for(int i=0;i<4;i++)Line(t,49,45+i*8,80,45+i*8,3,bone);
                Line(t,94,41,111,103,5,gold);
            }
            else if(kind=="rat")
            {
                Line(t,25,38,11,60,6,primary);
                Oval(t,64,49,38,26,dark);Oval(t,64,55,35,24,primary);
                Oval(t,42,80,14,14,primary);Oval(t,88,80,14,14,primary);
                Oval(t,42,80,7,7,new Color(.88f,.65f,.71f));
                Oval(t,88,80,7,7,new Color(.88f,.65f,.71f));
                Oval(t,67,57,10,8,bone);
            }
            else
            {
                Oval(t,64,46,35,40,dark);
                Oval(t,64,50,32,36,primary);
                Line(t,38,89,27,114,8,bone);
                Line(t,87,89,103,115,8,bone);
                Oval(t,64,84,24,20,bone);
                Oval(t,52,85,7,9,dark);Oval(t,76,85,7,9,dark);
                Line(t,45,46,83,46,4,bone);
                Line(t,45,34,83,34,3,bone);
            }
            Oval(t,54,67,5,6,dark);Oval(t,76,67,5,6,dark);
            Dot(t,54,68,aqua);Dot(t,76,68,aqua);
        }

        static void DrawItem(Texture2D t,string kind)
        {
            Color c=kind=="focus"?new Color(.64f,.66f,.92f):
                kind=="mantle"?new Color(.87f,.61f,.48f):
                kind=="ossos"?bone:kind=="fungo"?new Color(.61f,.85f,.60f):aqua;
            Oval(t,64,57,33,34,new Color(c.r,c.g,c.b,.25f));
            if(kind=="ossos")
            {
                Line(t,34,44,93,79,13,bone);
                Line(t,36,77,93,42,9,bone);
                Oval(t,34,44,10,11,bone);Oval(t,93,79,10,11,bone);
            }
            else if(kind=="fungo")
            {
                Line(t,64,33,64,75,14,bone);
                Oval(t,64,79,37,22,c);
                Oval(t,49,84,5,5,bone);Oval(t,72,79,4,4,bone);
            }
            else if(kind=="mantle")
            {
                Poly(t,c,64,98,39,74,24,31,64,42,105,31,90,74);
                Oval(t,64,77,16,10,bone);
            }
            else
            {
                Poly(t,c,64,113,96,62,64,14,32,62);
                Poly(t,new Color(.85f,.95f,.92f),64,102,83,61,64,31,45,61);
                Oval(t,64,65,10,16,Color.white);
            }
        }

        public static Sprite Environment(string path)
        {
            Sprite imported=Resources.Load<Sprite>("Backgrounds/"+path);
            if(imported!=null)return imported;
            if(cache.TryGetValue("floor/"+path,out Sprite cached))return cached;
            bool swamp=path.Contains("pantano");
            bool grave=path.Contains("necropole");
            bool tunnel=path.Contains("tuneis");
            bool conquered=path.Contains("santuario")||path.Contains("purificada");
            Color baseColor=swamp?new Color(.22f,.35f,.33f):
                grave?new Color(.33f,.32f,.37f):
                tunnel?new Color(.28f,.25f,.33f):
                new Color(.36f,.31f,.39f);
            if(conquered)baseColor+=new Color(.045f,.075f,.058f);
            Texture2D tex=Make(384);
            var random=new System.Random(path.GetHashCode()&0x7fffffff);
            Color[] pixels=new Color[384*384];
            for(int y=0;y<384;y++)for(int x=0;x<384;x++)
            {
                float variation=(float)random.NextDouble()*.055f-.027f;
                float veins=Mathf.Sin(x*.095f+y*.07f)*.018f;
                pixels[y*384+x]=new Color(
                    Mathf.Clamp01(baseColor.r+variation+veins),
                    Mathf.Clamp01(baseColor.g+variation+veins),
                    Mathf.Clamp01(baseColor.b+variation+veins),1);
            }
            tex.SetPixels(pixels);
            Color primary=swamp?new Color(.30f,.47f,.42f,.75f):
                grave?new Color(.49f,.45f,.49f,.82f):
                new Color(.42f,.37f,.44f,.83f);
            for(int i=0;i<42;i++)
            {
                int x=random.Next(20,364),y=random.Next(20,364);
                int rx=random.Next(7,23),ry=random.Next(4,14);
                Oval(tex,x,y,rx,ry,primary);
                if(grave && i%4==0) {
                    Line(tex,x,y,x,y+14,3,bone);
                    Line(tex,x-6,y+8,x+6,y+8,2,bone);
                }
                if(swamp && i%3==0)Oval(tex,x,y,rx/2,ry/2,new Color(.32f,.59f,.50f));
            }
            tex.Apply();
            return cache["floor/"+path]=Sprite.Create(tex,new Rect(0,0,384,384),
                new Vector2(.5f,.5f),100f);
        }
        public static Sprite Effect()
        {
            if(cache.TryGetValue("effect",out Sprite effect))return effect;
            Texture2D t=Make(64);
            Oval(t,32,32,28,28,new Color(.28f,.74f,.71f,.28f));
            Oval(t,32,32,18,18,new Color(.42f,.88f,.84f,.82f));
            Oval(t,32,32,8,8,new Color(.89f,.99f,.91f));
            t.Apply();
            return cache["effect"]=Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100f);
        }
        public static Sprite Stone()
        {
            if(cache.TryGetValue("rock",out Sprite rock))return rock;
            Texture2D t=Make(64);
            Oval(t,32,25,30,22,new Color(.18f,.18f,.25f));
            Oval(t,30,29,26,21,new Color(.36f,.32f,.39f));
            Oval(t,23,34,18,12,new Color(.46f,.41f,.48f));
            t.Apply();
            return cache["rock"]=Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100f);
        }
        static Texture2D Make(int size)
        {
            Texture2D t=new Texture2D(size,size,TextureFormat.RGBA32,false);
            t.SetPixels(new Color[size*size]);
            t.filterMode=FilterMode.Bilinear;
            t.wrapMode=TextureWrapMode.Clamp;
            return t;
        }
        static void Dot(Texture2D t,int x,int y,Color c)
        {
            if(x>=0&&y>=0&&x<t.width&&y<t.height)t.SetPixel(x,y,c);
        }
        static void Oval(Texture2D t,int cx,int cy,int rx,int ry,Color c)
        {
            for(int y=Mathf.Max(0,cy-ry);y<Mathf.Min(t.height,cy+ry+1);y++)
            for(int x=Mathf.Max(0,cx-rx);x<Mathf.Min(t.width,cx+rx+1);x++)
            {
                float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
                if(dx*dx+dy*dy<=1)Dot(t,x,y,c);
            }
        }
        static void Line(Texture2D t,int x1,int y1,int x2,int y2,int thickness,Color c)
        {
            int length=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(new Vector2(x1,y1),new Vector2(x2,y2))));
            for(int i=0;i<=length;i++)
            {
                float a=(float)i/length;
                Oval(t,Mathf.RoundToInt(Mathf.Lerp(x1,x2,a)),Mathf.RoundToInt(Mathf.Lerp(y1,y2,a)),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),c);
            }
        }
        static void Poly(Texture2D t,Color c,params int[] coords)
        {
            int n=coords.Length/2;
            if(n<3)return;
            int yMin=t.height,yMax=0;
            for(int i=1;i<coords.Length;i+=2){yMin=Mathf.Min(yMin,coords[i]);yMax=Mathf.Max(yMax,coords[i]);}
            for(int y=Mathf.Max(0,yMin);y<=Mathf.Min(t.height-1,yMax);y++)
            {
                var intersections=new List<float>();
                for(int i=0;i<n;i++)
                {
                    int j=(i+1)%n;
                    int x1=coords[2*i],yy1=coords[2*i+1],x2=coords[2*j],yy2=coords[2*j+1];
                    if((yy1<=y&&yy2>y)||(yy2<=y&&yy1>y))
                        intersections.Add(x1+(y-yy1)*(x2-x1)/(float)(yy2-yy1));
                }
                intersections.Sort();
                for(int i=0;i+1<intersections.Count;i+=2)
                    for(int x=Mathf.Max(0,Mathf.CeilToInt(intersections[i]));
                        x<=Mathf.Min(t.width-1,Mathf.FloorToInt(intersections[i+1]));x++)Dot(t,x,y,c);
            }
        }
    }
}

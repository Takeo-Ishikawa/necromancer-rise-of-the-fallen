using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    // Sprites gerados em tempo de execucao, substituidos automaticamente por PNGs
    // importados para Assets/Resources/Sprites e Assets/Resources/Backgrounds.
    public static class NecroArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite Get(string name)
        {
            Sprite imported=Resources.Load<Sprite>("Sprites/"+name);
            if(imported!=null)return imported;
            if(cache.TryGetValue(name,out Sprite sprite))return sprite;
            Color main=name=="necromante"?new Color(.34f,.28f,.45f):
                name=="rat"?new Color(.62f,.46f,.52f):
                name=="slime"?new Color(.43f,.68f,.68f):
                name=="bat"?new Color(.52f,.42f,.62f):
                name=="snake"?new Color(.42f,.58f,.53f):
                name=="spider"?new Color(.39f,.34f,.51f):
                new Color(.54f,.48f,.63f);
            var tex=Make(96);
            if(name=="bat"){Oval(tex,24,48,23,14,main);Oval(tex,72,48,23,14,main);}
            if(name=="spider")for(int i=0;i<4;i++){
                Oval(tex,24,28+i*11,18,4,main);
                Oval(tex,72,28+i*11,18,4,main);
            }
            if(name=="snake"){Oval(tex,51,63,28,18,main);Oval(tex,63,42,13,23,main);}
            else if(name=="slime")Oval(tex,48,55,33,27,main);
            else {
                Oval(tex,48,56,name=="warden"?31:24,27,main);
                Oval(tex,30,31,11,16,main);Oval(tex,66,31,11,16,main);
                if(name=="necromante")Oval(tex,48,56,18,18,new Color(.82f,.75f,.78f));
            }
            Oval(tex,39,49,4,5,Color.cyan);Oval(tex,58,49,4,5,Color.cyan);
            tex.Apply();
            return cache[name]=Sprite.Create(tex,new Rect(0,0,96,96),new Vector2(.5f,.5f),100f);
        }
        public static Sprite Environment(string path)
        {
            Sprite s=Resources.Load<Sprite>("Backgrounds/"+path);
            if(s!=null)return s;
            if(cache.TryGetValue("floor",out s))return s;
            Texture2D tex=Make(128);
            var random=new System.Random(2112);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float variation=(float)random.NextDouble()*.055f;
                tex.SetPixel(x,y,new Color(.33f+variation,.29f+variation,.39f+variation,1));
            }
            tex.Apply();
            return cache["floor"]=Sprite.Create(tex,new Rect(0,0,128,128),new Vector2(.5f,.5f),100f);
        }
        public static Sprite Effect()
        {
            if(cache.TryGetValue("effect",out Sprite s))return s;
            Texture2D tex=Make(64);
            Oval(tex,32,32,22,22,new Color(.40f,.85f,.80f,.80f));
            Oval(tex,32,32,10,10,new Color(.80f,.96f,.91f));
            tex.Apply();
            return cache["effect"]=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100f);
        }
        public static Sprite Stone()
        {
            if(cache.TryGetValue("stone",out Sprite s))return s;
            Texture2D tex=Make(64);
            Oval(tex,32,26,27,20,new Color(.27f,.25f,.35f));
            Oval(tex,27,30,19,14,new Color(.41f,.37f,.47f));
            tex.Apply();
            return cache["stone"]=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100f);
        }
        static Texture2D Make(int size)
        {
            Texture2D tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
            tex.SetPixels(new Color[size*size]);
            tex.filterMode=FilterMode.Bilinear;
            tex.wrapMode=TextureWrapMode.Clamp;
            return tex;
        }
        static void Oval(Texture2D t,int cx,int cy,int rx,int ry,Color color)
        {
            for(int y=Mathf.Max(0,cy-ry);y<Mathf.Min(t.height,cy+ry);y++)
            for(int x=Mathf.Max(0,cx-rx);x<Mathf.Min(t.width,cx+rx);x++)
            {
                float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
                if(dx*dx+dy*dy<=1)t.SetPixel(x,y,color);
            }
        }
    }
}

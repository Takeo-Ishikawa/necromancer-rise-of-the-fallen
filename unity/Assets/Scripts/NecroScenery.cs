using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Decorative scenery is generated once per stage and never blocks the
    /// player's path. Every area gets a distinct environmental vocabulary.
    /// No decorations are gameplay state; exploration and save files are unchanged.
    /// </summary>
    public static class NecroScenery
    {
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static readonly Color Ink=new Color(.15f,.14f,.20f);
        static readonly Color Rock=new Color(.46f,.43f,.49f);
        static readonly Color StoneLight=new Color(.66f,.59f,.61f);
        static readonly Color Bone=new Color(.86f,.82f,.75f);
        static readonly Color Leaf=new Color(.38f,.56f,.42f);
        static readonly Color Teal=new Color(.54f,.84f,.80f);

        public static void Populate(LevelDefinition area,Transform root)
        {
            if(area==null || root==null)return;
            string[] variants=area.id=="necropolis" ?
                new[]{"headstone","broken_cross","lantern","bones","headstone"} :
                area.id=="marsh" ?
                new[]{"fungus","reeds","pool","fungus","reeds"} :
                area.id=="lower_tunnels" ?
                new[]{"stalagmite","crystal","bones","stalagmite","fungus"} :
                new[]{"stalagmite","crystal","bones","rune","stalagmite"};
            int seed=8161;
            foreach(char c in area.id)seed=unchecked(seed*31+c);
            System.Random random=new System.Random(seed&0x7fffffff);
            int wanted=area.id=="bonehollow"?64:area.id=="necropolis"?32:area.id=="marsh"?27:25;
            int created=0;
            for(int tries=0;tries<(area.id=="bonehollow"?720:230) && created<wanted;tries++)
            {
                float x=1.0f+(float)random.NextDouble()*(area.width-2.0f);
                float y=1.0f+(float)random.NextDouble()*(area.height-2.0f);
                if(Vector2.Distance(new Vector2(x,y),new Vector2(area.startX,area.startY))<2.5f ||
                   Vector2.Distance(new Vector2(x,y),new Vector2(area.sealX,area.sealY))<2.2f)
                    continue;

                bool tooClose=false;
                if(area.obstacles!=null)foreach(Obstacle obstacle in area.obstacles)
                    if(Vector2.Distance(new Vector2(x,y),new Vector2(obstacle.x,obstacle.y)) <
                        obstacle.r+.85f){tooClose=true;break;}
                if(tooClose)continue;
                if(area.enemies!=null)foreach(Spawn enemy in area.enemies)
                    if(Vector2.Distance(new Vector2(x,y),new Vector2(enemy.x,enemy.y))<.85f)
                        {tooClose=true;break;}
                if(tooClose)continue;
                if(area.resources!=null)foreach(ResourceSpawn resource in area.resources)
                    if(Vector2.Distance(new Vector2(x,y),
                        new Vector2(resource.x,resource.y))<.72f)
                        {tooClose=true;break;}
                if(tooClose)continue;
                if(area.id=="bonehollow" &&
                   Vector2.Distance(new Vector2(x,y),new Vector2(area.rotboarX,area.rotboarY))<2.5f)
                    continue;
                string kind=variants[random.Next(variants.Length)];
                if(area.id=="bonehollow")
                {
                    if(x<26f && y<17f)
                        kind=random.Next(4)==0?"stalagmite":"crystal";
                    else if(x>27f && y>22f)
                        kind=random.Next(4)==0?"pool":"fungus";
                    else if(x>39f && y<22f)
                        kind=random.Next(3)==0?"rune":"bones";
                }
                Sprite sprite=Get(kind);
                GameObject prop=new GameObject("Decor: "+kind);
                prop.transform.SetParent(root,false);
                Vector2 position=NecroLevels.At(area,x,y);
                prop.transform.position=new Vector3(position.x,position.y,0);
                SpriteRenderer renderer=prop.AddComponent<SpriteRenderer>();
                renderer.sprite=sprite;
                renderer.sortingOrder=-4;
                float size=.37f+(float)random.NextDouble()*.43f;
                if(kind=="pool")size*=1.7f;
                if(kind=="headstone")size*=1.10f;
                Vector2 bounds=sprite.bounds.size;
                prop.transform.localScale=new Vector3(size/bounds.x,size/bounds.y,1);
                if(random.Next(2)==0)renderer.flipX=true;
                created++;
            }
        }

        public static Sprite Get(string kind)
        {
            if(sprites.TryGetValue(kind,out Sprite result))return result;
            Sprite imported=Resources.Load<Sprite>("Scenery/"+kind);
            if(imported!=null)return sprites[kind]=imported;
            Texture2D painting=Resources.Load<Texture2D>("Scenery/"+kind);
            if(painting!=null)
            {
                painting.filterMode=FilterMode.Bilinear;
                return sprites[kind]=Sprite.Create(painting,
                    new Rect(0,0,painting.width,painting.height),
                    new Vector2(.5f,.37f),100f,0,SpriteMeshType.FullRect);
            }
            Texture2D t=new Texture2D(128,128,TextureFormat.RGBA32,false);
            t.SetPixels(new Color[128*128]);t.filterMode=FilterMode.Bilinear;
            t.wrapMode=TextureWrapMode.Clamp;
            switch(kind)
            {
                case "headstone": Headstone(t);break;
                case "broken_cross": Cross(t);break;
                case "lantern": Lantern(t);break;
                case "bones": Bones(t);break;
                case "fungus": Fungus(t);break;
                case "reeds": Reeds(t);break;
                case "pool": Pool(t);break;
                case "rune": Rune(t);break;
                case "crystal": Crystal(t);break;
                default: Stalagmite(t);break;
            }
            t.Apply();
            return sprites[kind]=Sprite.Create(t,new Rect(0,0,128,128),
                new Vector2(.5f,.37f),100f);
        }

        static void Headstone(Texture2D t)
        {
            Oval(t,64,18,39,9,new Color(.07f,.07f,.12f,.25f));
            Poly(t,Ink,27,23,26,90,32,105,44,116,83,116,98,105,99,24);
            Poly(t,Rock,34,26,33,89,39,99,51,109,77,109,89,99,91,26);
            Oval(t,62,105,28,14,Rock);
            Line(t,64,92,64,51,6,Ink);
            Line(t,48,76,81,76,6,Ink);
            Line(t,35,33,47,42,3,StoneLight);
            Line(t,40,98,77,98,3,StoneLight);
        }

        static void Cross(Texture2D t)
        {
            Oval(t,64,21,42,10,new Color(.08f,.07f,.1f,.25f));
            Line(t,59,18,77,112,19,Ink);
            Line(t,59,18,77,112,13,Rock);
            Line(t,38,82,99,96,16,Ink);
            Line(t,38,82,99,96,9,Rock);
            Line(t,69,64,76,104,3,StoneLight);
            Line(t,47,86,84,93,3,StoneLight);
        }

        static void Lantern(Texture2D t)
        {
            Oval(t,61,16,32,8,new Color(.07f,.07f,.11f,.20f));
            Line(t,64,21,64,110,9,Ink);
            Line(t,64,21,64,109,5,StoneLight);
            Line(t,46,101,81,103,7,Ink);
            EllipseGlow(t,63,94,16,new Color(.54f,.89f,.79f,.15f));
            Oval(t,63,92,13,17,Ink);
            Oval(t,63,92,9,13,new Color(.45f,.77f,.75f,.66f));
            Oval(t,63,94,5,7,new Color(.83f,.98f,.86f));
            Line(t,48,110,77,110,5,Rock);
            Line(t,47,74,77,74,5,Rock);
        }

        static void Bones(Texture2D t)
        {
            Oval(t,63,21,46,8,new Color(.05f,.07f,.10f,.21f));
            Line(t,30,34,100,51,12,Ink);
            Line(t,30,34,100,51,7,Bone);
            Line(t,38,59,99,28,11,Ink);
            Line(t,38,59,99,28,6,Bone);
            Oval(t,37,34,8,8,Bone);Oval(t,100,51,8,8,Bone);
            Oval(t,96,28,8,8,Bone);Oval(t,38,59,8,8,Bone);
            Oval(t,66,48,15,14,Bone);
            Oval(t,60,49,4,5,Ink);Oval(t,72,49,4,5,Ink);
        }

        static void Fungus(Texture2D t)
        {
            Oval(t,61,21,45,10,new Color(.06f,.1f,.1f,.20f));
            Line(t,50,23,57,67,15,Ink);
            Line(t,50,23,57,68,10,Bone);
            Line(t,81,22,83,52,11,Ink);
            Line(t,81,22,83,52,7,Bone);
            Oval(t,57,71,34,18,Ink);
            Oval(t,57,75,30,14,new Color(.51f,.70f,.59f));
            Oval(t,84,59,24,14,Ink);
            Oval(t,84,63,21,11,new Color(.65f,.76f,.58f));
            Oval(t,45,79,5,4,Bone);Oval(t,64,74,4,3,Bone);
            Oval(t,87,65,4,3,Bone);
            EllipseGlow(t,57,71,16,new Color(.50f,.85f,.74f,.20f));
        }

        static void Reeds(Texture2D t)
        {
            Oval(t,64,19,40,9,new Color(.07f,.12f,.10f,.25f));
            for(int i=0;i<8;i++)
            {
                float offset=(i-4)*7f;
                int x=(int)(64+offset);
                int top=(i%3)*12+70;
                Line(t,x,18,x+(i%2==0?14:-12),top,5,new Color(.36f,.52f,.38f));
                Oval(t,x+(i%2==0?14:-12),top,5,9,
                    new Color(.60f,.64f,.42f));
            }
        }

        static void Pool(Texture2D t)
        {
            Oval(t,64,54,56,39,new Color(.09f,.17f,.19f,.70f));
            Oval(t,63,54,50,33,new Color(.24f,.45f,.48f,.86f));
            Oval(t,63,54,39,23,new Color(.31f,.56f,.54f,.65f));
            Oval(t,49,71,15,4,new Color(.67f,.79f,.70f,.66f));
            Oval(t,82,40,10,4,new Color(.69f,.81f,.72f,.50f));
            Line(t,30,60,42,63,3,new Color(.65f,.75f,.61f,.50f));
            Line(t,91,53,101,56,2,new Color(.65f,.75f,.61f,.50f));
        }

        static void Rune(Texture2D t)
        {
            Oval(t,64,51,47,32,new Color(.12f,.30f,.34f,.18f));
            CircleOutline(t,64,55,35,3,new Color(.45f,.84f,.78f,.78f));
            CircleOutline(t,64,55,25,2,new Color(.53f,.89f,.85f,.68f));
            Line(t,64,20,64,92,3,Teal);
            Line(t,32,54,96,54,3,Teal);
            Poly(t,Teal,64,81,48,38,82,38);
            Oval(t,64,55,5,5,new Color(.83f,.98f,.92f));
        }

        static void Crystal(Texture2D t)
        {
            Oval(t,64,19,41,10,new Color(.12f,.20f,.20f,.29f));
            Poly(t,Ink,43,24,35,70,63,119,86,77,82,27);
            Poly(t,new Color(.34f,.66f,.69f),45,27,41,73,62,111,78,73,78,30);
            Poly(t,new Color(.63f,.92f,.84f),62,112,62,29,79,74);
            Poly(t,new Color(.80f,.98f,.91f),47,70,63,103,62,38);
            Poly(t,new Color(.24f,.47f,.55f),19,22,17,47,34,74,39,36);
            Poly(t,new Color(.56f,.85f,.85f),21,24,25,48,35,64,36,36);
        }

        static void Stalagmite(Texture2D t)
        {
            Oval(t,64,16,46,10,new Color(.07f,.07f,.11f,.29f));
            Poly(t,Ink,24,23,46,85,59,57,72,116,91,54,109,23);
            Poly(t,Rock,29,27,47,74,60,53,71,107,89,51,101,28);
            Poly(t,StoneLight,46,77,59,55,71,109,62,89);
            Poly(t,new Color(.38f,.36f,.44f),70,108,90,52,101,28,82,43);
        }

        static void EllipseGlow(Texture2D t,int cx,int cy,int radius,Color c)
        {
            Oval(t,cx,cy,radius,radius,c);
        }
        static void Dot(Texture2D t,int x,int y,Color color)
        {
            if(x>=0 && y>=0 && x<t.width && y<t.height)t.SetPixel(x,y,color);
        }
        static void Oval(Texture2D t,int cx,int cy,int rx,int ry,Color color)
        {
            for(int y=Mathf.Max(0,cy-ry);y<=Mathf.Min(t.height-1,cy+ry);y++)
            for(int x=Mathf.Max(0,cx-rx);x<=Mathf.Min(t.width-1,cx+rx);x++)
            {
                float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
                if(dx*dx+dy*dy<=1f)Dot(t,x,y,color);
            }
        }
        static void Line(Texture2D t,int x1,int y1,int x2,int y2,int width,Color color)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(
                new Vector2(x1,y1),new Vector2(x2,y2))));
            for(int i=0;i<=steps;i++)
            {
                float u=(float)i/steps;
                Oval(t,Mathf.RoundToInt(Mathf.Lerp(x1,x2,u)),
                    Mathf.RoundToInt(Mathf.Lerp(y1,y2,u)),
                    Mathf.Max(1,width/2),Mathf.Max(1,width/2),color);
            }
        }
        static void CircleOutline(Texture2D t,int cx,int cy,int radius,int width,Color color)
        {
            for(int angle=0;angle<360;angle+=3)
            {
                float theta=angle*Mathf.Deg2Rad;
                Oval(t,cx+Mathf.RoundToInt(Mathf.Cos(theta)*radius),
                    cy+Mathf.RoundToInt(Mathf.Sin(theta)*radius),width,width,color);
            }
        }
        static void Poly(Texture2D t,Color color,params int[] points)
        {
            int n=points.Length/2;
            int min=t.height,max=0;
            for(int i=1;i<points.Length;i+=2)
            {
                min=Mathf.Min(min,points[i]);max=Mathf.Max(max,points[i]);
            }
            for(int y=Mathf.Max(0,min);y<=Mathf.Min(t.height-1,max);y++)
            {
                var cuts=new List<float>();
                for(int i=0;i<n;i++)
                {
                    int j=(i+1)%n;
                    int x1=points[2*i],y1=points[2*i+1];
                    int x2=points[2*j],y2=points[2*j+1];
                    if((y1<=y && y2>y)||(y2<=y && y1>y))
                        cuts.Add(x1+(y-y1)*(x2-x1)/(float)(y2-y1));
                }
                cuts.Sort();
                for(int c=0;c+1<cuts.Count;c+=2)
                    for(int x=Mathf.Max(0,Mathf.CeilToInt(cuts[c]));
                        x<=Mathf.Min(t.width-1,Mathf.FloorToInt(cuts[c+1]));x++)
                        Dot(t,x,y,color);
            }
        }
    }
}

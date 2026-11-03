using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Distinct original 128px illustrated silhouettes for all ten creature types.
    /// These are cached by NecroArt, so texture rasterization happens only once
    /// per species; transparent imported PNG sprites still take precedence.
    /// </summary>
    public static class NecroCreatureArt
    {
        static readonly Color Ink=new Color(.12f,.10f,.17f);
        static readonly Color Bone=new Color(.89f,.84f,.74f);
        static readonly Color BoneShade=new Color(.64f,.59f,.60f);
        static readonly Color Mist=new Color(.60f,.88f,.88f);
        static readonly Color Warm=new Color(.77f,.53f,.59f);
        static readonly Color Gold=new Color(.73f,.61f,.41f);

        public static bool Draw(Texture2D t,string kind)
        {
            switch(kind)
            {
                case "rat": Rat(t);return true;
                case "bat": Bat(t);return true;
                case "slime": Slime(t);return true;
                case "spider": Spider(t);return true;
                case "snake": Snake(t);return true;
                case "skeleton": Skeleton(t);return true;
                case "wisp": Wisp(t);return true;
                case "warden": Guardian(t,0);return true;
                case "grave_lord": Guardian(t,1);return true;
                case "marsh_lord": Guardian(t,2);return true;
                default: return false;
            }
        }

        static void Rat(Texture2D t)
        {
            Color fur=new Color(.58f,.39f,.42f);
            Color light=new Color(.79f,.60f,.57f);
            Color pale=new Color(.91f,.76f,.74f);
            Ellipse(t,65,20,35,8,new Color(.04f,.06f,.10f,.28f));
            Arc(t,32,52,27,30,-110,80,7,fur);
            Ellipse(t,63,46,45,25,Ink);
            Ellipse(t,65,50,41,22,fur);
            Ellipse(t,65,59,34,15,light);
            Ellipse(t,37,80,17,19,Ink);
            Ellipse(t,92,82,18,21,Ink);
            Ellipse(t,38,82,14,16,light);
            Ellipse(t,90,83,15,17,light);
            Ellipse(t,40,83,8,9,pale);
            Ellipse(t,90,85,8,10,pale);
            Ellipse(t,64,59,26,23,fur);
            Ellipse(t,64,62,24,19,light);
            Ellipse(t,54,67,6,7,Ink);
            Ellipse(t,77,68,6,7,Ink);
            Ellipse(t,56,69,2,3,new Color(.89f,.86f,.70f));
            Ellipse(t,79,69,2,3,new Color(.89f,.86f,.70f));
            Ellipse(t,67,51,11,8,pale);
            Ellipse(t,67,55,5,3,Warm);
            Line(t,39,52,13,60,2,BoneShade);Line(t,40,47,12,44,2,BoneShade);
            Line(t,93,53,116,61,2,BoneShade);Line(t,94,48,117,47,2,BoneShade);
            Line(t,42,30,34,22,8,fur);Line(t,86,31,92,21,8,fur);
        }

        static void Bat(Texture2D t)
        {
            Color wing=new Color(.34f,.29f,.48f);
            Color membrane=new Color(.55f,.43f,.59f);
            // Serrated wings are the dominant shape even at phone resolution.
            Poly(t,Ink,63,70,36,102,6,102,18,82,2,73,22,61,10,45,35,43,42,52,62,56);
            Poly(t,Ink,65,70,94,102,122,101,111,82,126,72,104,61,116,45,91,43,85,51,66,56);
            Poly(t,membrane,59,66,31,95,14,97,23,78,11,71,33,60,22,49,41,50);
            Poly(t,membrane,69,66,99,95,115,96,106,79,119,71,96,60,106,49,87,50);
            Line(t,64,64,17,76,2,wing);Line(t,64,64,25,50,2,wing);
            Line(t,64,64,109,75,2,wing);Line(t,64,64,100,50,2,wing);
            Ellipse(t,64,50,20,31,Ink);
            Ellipse(t,64,55,16,26,wing);
            Poly(t,Ink,49,74,50,105,63,87,78,106,79,74);
            Poly(t,membrane,54,79,55,98,63,86,75,99,74,79);
            Ellipse(t,64,59,18,17,new Color(.48f,.39f,.54f));
            Ellipse(t,57,60,5,7,Ink);Ellipse(t,72,60,5,7,Ink);
            Ellipse(t,58,61,2,3,new Color(.92f,.65f,.67f));
            Ellipse(t,73,61,2,3,new Color(.92f,.65f,.67f));
            Poly(t,Bone,59,44,63,39,64,47);
            Poly(t,Bone,68,46,70,39,72,44);
        }

        static void Slime(Texture2D t)
        {
            Color green=new Color(.53f,.71f,.61f,.89f);
            Color darkGreen=new Color(.25f,.49f,.44f);
            Ellipse(t,64,25,43,9,new Color(.06f,.13f,.14f,.24f));
            Ellipse(t,64,45,48,36,Ink);
            Ellipse(t,64,48,45,34,darkGreen);
            Ellipse(t,64,53,40,32,green);
            Ellipse(t,47,73,21,10,new Color(.82f,.95f,.81f,.7f));
            Ellipse(t,38,56,8,20,new Color(.70f,.88f,.77f,.30f));
            for(int i=0;i<4;i++)
            {
                float seed=i*2.1f;
                int x=39+i*19;
                Ellipse(t,x,27+(i%2)*3,5,6,new Color(.59f,.82f,.70f,.75f));
            }
            Ellipse(t,53,55,8,10,Ink);Ellipse(t,79,55,8,10,Ink);
            Ellipse(t,55,57,3,4,new Color(.93f,.98f,.87f));
            Ellipse(t,80,57,3,4,new Color(.93f,.98f,.87f));
            Arc(t,66,42,13,7,205,325,3,darkGreen);
        }

        static void Spider(Texture2D t)
        {
            Color shell=new Color(.47f,.35f,.58f);
            Color shine=new Color(.70f,.53f,.69f);
            for(int i=0;i<4;i++)
            {
                int y=45+i*12;
                int tipY=13+i*16;
                Line(t,55,y,21-(i%2)*7,tipY,8,Ink);
                Line(t,21-(i%2)*7,tipY,7,tipY-5,5,shell);
                Line(t,73,y,108+(i%2)*7,tipY,8,Ink);
                Line(t,108+(i%2)*7,tipY,122,tipY-5,5,shell);
                Line(t,54,y,22,tipY,4,shell);
                Line(t,74,y,108,tipY,4,shell);
            }
            Ellipse(t,64,44,32,29,Ink);
            Ellipse(t,64,48,28,25,shell);
            Ellipse(t,64,69,21,21,Ink);
            Ellipse(t,64,71,18,19,shell);
            Poly(t,shine,49,51,64,65,79,51,64,41);
            Ellipse(t,54,77,5,6,new Color(.98f,.72f,.76f));
            Ellipse(t,73,77,5,6,new Color(.98f,.72f,.76f));
            Ellipse(t,64,85,4,4,new Color(.98f,.72f,.76f));
            Line(t,55,59,48,47,4,BoneShade);Line(t,73,59,80,47,4,BoneShade);
        }

        static void Snake(Texture2D t)
        {
            Color scale=new Color(.38f,.61f,.48f);
            Color belly=new Color(.77f,.79f,.56f);
            Ellipse(t,60,39,45,19,Ink);
            Ellipse(t,60,43,40,16,scale);
            Ellipse(t,54,44,26,10,belly);
            Ellipse(t,77,54,30,20,Ink);
            Ellipse(t,78,57,27,17,scale);
            Arc(t,63,65,30,22,20,195,9,belly);
            Ellipse(t,49,81,26,23,Ink);
            Ellipse(t,50,85,23,19,scale);
            Ellipse(t,50,79,19,13,belly);
            Ellipse(t,41,85,5,6,Ink);Ellipse(t,60,85,5,6,Ink);
            Ellipse(t,41,87,2,2,new Color(.90f,.83f,.58f));
            Ellipse(t,61,87,2,2,new Color(.90f,.83f,.58f));
            Line(t,49,70,44,57,3,new Color(.86f,.53f,.63f));
            Line(t,44,57,36,51,2,new Color(.86f,.53f,.63f));
            Line(t,44,57,50,52,2,new Color(.86f,.53f,.63f));
            for(int i=0;i<4;i++)Line(t,72+i*9,59,78+i*8,65,2,Ink);
        }

        static void Skeleton(Texture2D t)
        {
            Color cloth=new Color(.32f,.33f,.44f);
            Ellipse(t,62,24,29,8,new Color(.06f,.08f,.11f,.28f));
            Line(t,48,42,44,17,9,Ink);Line(t,80,42,86,17,9,Ink);
            Line(t,48,42,44,17,5,Bone);Line(t,80,42,86,17,5,Bone);
            Poly(t,Ink,49,54,40,30,61,24,86,30,78,54);
            Poly(t,cloth,50,51,47,35,64,30,82,36,76,51);
            for(int i=0;i<4;i++)
            {
                int y=39+i*7;
                Line(t,53,y,75,y,4,Bone);
                Line(t,52,y,46,y+4,3,BoneShade);
                Line(t,75,y,81,y+4,3,BoneShade);
            }
            Line(t,43,58,30,43,7,Ink);Line(t,85,58,99,42,7,Ink);
            Line(t,43,58,30,43,4,Bone);Line(t,85,58,99,42,4,Bone);
            Line(t,96,35,109,107,6,Gold);
            Line(t,85,70,112,70,4,BoneShade);
            Ellipse(t,64,85,28,26,Ink);
            Ellipse(t,64,87,25,22,Bone);
            Ellipse(t,54,88,7,8,Ink);Ellipse(t,74,88,7,8,Ink);
            Ellipse(t,55,90,2,3,Mist);Ellipse(t,75,90,2,3,Mist);
            Poly(t,Ink,56,69,64,60,72,69);
            for(int i=0;i<5;i++)Line(t,53+i*5,69,53+i*5,76,2,Ink);
        }

        static void Wisp(Texture2D t)
        {
            Color glow=new Color(.48f,.85f,.93f);
            Color body=new Color(.55f,.73f,.87f,.84f);
            Ellipse(t,64,60,41,50,new Color(.21f,.55f,.73f,.18f));
            Poly(t,new Color(.38f,.74f,.85f,.30f),64,14,82,30,73,49,99,34,88,66,101,85,79,83,64,112,49,83,29,85,40,63,28,34,54,50);
            Ellipse(t,64,68,29,35,new Color(.25f,.47f,.66f,.74f));
            Ellipse(t,64,72,25,29,body);
            Line(t,43,53,31,27,9,glow);
            Line(t,84,53,99,24,9,glow);
            Ellipse(t,51,77,7,10,Ink);Ellipse(t,77,77,7,10,Ink);
            Ellipse(t,52,81,3,4,new Color(.91f,.99f,.97f));
            Ellipse(t,78,81,3,4,new Color(.91f,.99f,.97f));
            Ellipse(t,64,58,6,10,new Color(.23f,.50f,.64f));
            Ellipse(t,64,55,2,5,new Color(.88f,.98f,.95f));
            for(int i=0;i<6;i++)
            {
                int x=22+i*17;
                Ellipse(t,x,37+(i%2)*13,3+(i%3),4,new Color(.66f,.94f,.97f,.62f));
            }
        }

        static void Guardian(Texture2D t,int variant)
        {
            bool marsh=variant==2;
            bool knight=variant==1;
            Color armor=marsh?new Color(.37f,.53f,.42f):
                knight?new Color(.49f,.37f,.57f):new Color(.42f,.39f,.49f);
            Color trim=marsh?new Color(.63f,.77f,.46f):
                knight?new Color(.75f,.61f,.78f):Gold;
            Color glow=marsh?new Color(.68f,.98f,.65f):knight?new Color(.79f,.66f,.96f):Mist;
            Ellipse(t,64,16,44,10,new Color(.04f,.07f,.09f,.30f));
            // Shoulders/armor are much wider than ordinary mobs.
            Poly(t,Ink,22,25,25,77,41,93,88,92,105,76,110,25,89,18,64,41,42,19);
            Poly(t,armor,27,26,28,75,42,89,87,88,101,74,104,29,85,23,64,46,44,22);
            Poly(t,Ink,39,60,17,66,24,96,51,86);
            Poly(t,Ink,88,60,110,66,104,96,77,86);
            Poly(t,armor,39,61,24,67,30,87,53,80);
            Poly(t,armor,89,61,105,67,98,87,75,80);
            Line(t,38,47,89,47,8,trim);
            Line(t,38,35,87,35,4,BoneShade);
            Poly(t,Ink,44,81,36,112,52,103,64,99,77,104,93,112,84,81);
            Poly(t,trim,48,80,44,107,56,99,64,95,72,99,84,106,80,80);
            Ellipse(t,64,88,33,25,Ink);
            Ellipse(t,64,89,29,22,Bone);
            // Distinct crown/horns per boss.
            Line(t,47,99,27,121,9,Ink);
            Line(t,81,99,103,122,9,Ink);
            Line(t,47,99,27,121,5,Bone);
            Line(t,81,99,103,122,5,Bone);
            if(knight)
            {
                Line(t,64,105,64,127,8,trim);
                Line(t,38,82,26,107,7,trim);
                Line(t,91,82,101,107,7,trim);
                Line(t,93,43,118,101,8,Gold);
                Ellipse(t,117,107,10,12,glow);
            }
            else if(marsh)
            {
                Ellipse(t,30,53,17,13,new Color(.27f,.51f,.36f));
                Ellipse(t,98,52,16,15,new Color(.27f,.51f,.36f));
                Ellipse(t,24,89,9,13,new Color(.49f,.65f,.41f));
                Ellipse(t,107,90,8,10,new Color(.49f,.65f,.41f));
                Ellipse(t,64,115,13,7,new Color(.65f,.78f,.49f));
                Line(t,42,35,31,18,5,trim);
                Line(t,85,35,94,18,5,trim);
            }
            else
            {
                Ellipse(t,64,114,15,8,trim);
                Line(t,35,51,19,82,7,BoneShade);
                Line(t,91,51,111,83,7,BoneShade);
            }
            Ellipse(t,52,89,8,11,Ink);Ellipse(t,77,89,8,11,Ink);
            Ellipse(t,53,91,4,5,glow);Ellipse(t,78,91,4,5,glow);
            Poly(t,Ink,57,72,64,65,71,72,64,77);
            for(int i=0;i<5;i++)Line(t,51+i*6,70,51+i*6,77,2,Ink);
            Ellipse(t,64,57,10,7,new Color(glow.r,glow.g,glow.b,.48f));
            Ellipse(t,64,57,4,4,glow);
        }

        static void Dot(Texture2D t,int x,int y,Color color)
        {
            if(x>=0 && y>=0 && x<t.width && y<t.height)t.SetPixel(x,y,color);
        }
        static void Ellipse(Texture2D t,int cx,int cy,int rx,int ry,Color color)
        {
            for(int y=Mathf.Max(0,cy-ry);y<=Mathf.Min(t.height-1,cy+ry);y++)
            for(int x=Mathf.Max(0,cx-rx);x<=Mathf.Min(t.width-1,cx+rx);x++)
            {
                float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
                if(dx*dx+dy*dy<1)Dot(t,x,y,color);
            }
        }
        static void Line(Texture2D t,int x1,int y1,int x2,int y2,int thickness,Color color)
        {
            int steps=Mathf.Max(1,Mathf.RoundToInt(Vector2.Distance(
                new Vector2(x1,y1),new Vector2(x2,y2))));
            for(int i=0;i<=steps;i++)
            {
                float v=(float)i/steps;
                Ellipse(t,Mathf.RoundToInt(Mathf.Lerp(x1,x2,v)),
                    Mathf.RoundToInt(Mathf.Lerp(y1,y2,v)),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),color);
            }
        }
        static void Arc(Texture2D t,int cx,int cy,int rx,int ry,int start,int end,int thickness,Color color)
        {
            for(int a=start;a<=end;a+=5)
            {
                float theta=a*Mathf.Deg2Rad;
                Ellipse(t,Mathf.RoundToInt(cx+Mathf.Cos(theta)*rx),
                    Mathf.RoundToInt(cy+Mathf.Sin(theta)*ry),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),color);
            }
        }
        static void Poly(Texture2D t,Color color,params int[] points)
        {
            int n=points.Length/2;
            int low=t.height,high=0;
            for(int i=1;i<points.Length;i+=2)
            {
                low=Mathf.Min(low,points[i]);high=Mathf.Max(high,points[i]);
            }
            for(int y=Mathf.Max(0,low);y<=Mathf.Min(t.height-1,high);y++)
            {
                List<float> cuts=new List<float>();
                for(int i=0;i<n;i++)
                {
                    int j=(i+1)%n;
                    int x1=points[2*i],y1=points[2*i+1];
                    int x2=points[2*j],y2=points[2*j+1];
                    if((y1<=y&&y2>y)||(y2<=y&&y1>y))
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

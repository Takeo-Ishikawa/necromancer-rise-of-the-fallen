using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    public enum NecroPose { Idle=0, Walk=1, Attack=2, Cast=3, Raise=4, Hurt=5 }
    public enum NecroDirection { Down=0, Up=1, Left=2, Right=3 }

    /// <summary>
    /// A real sprite atlas containing individually addressable animation frames.
    /// It does not pretend that perspective-derived frames are hand-drawn art.
    /// </summary>
    public sealed class NecroFrameSet
    {
        public readonly string Kind;
        public readonly Texture2D Atlas;
        public readonly int Cell;
        readonly Sprite[][][] clips;
        static readonly int[] ClipLengths={3,6,4,4,5,2};
        static readonly float[] ClipFps={3.5f,10f,12f,9f,8f,12f};

        internal NecroFrameSet(string kind,Texture2D atlas,int cell,bool hero)
        {
            Kind=kind;
            Atlas=atlas;
            Cell=cell;
            clips=new Sprite[6][][];
            int states=hero?6:4;
            for(int state=0;state<states;state++)
            {
                int pose=hero?state:(state==3?5:state);
                clips[pose]=new Sprite[4][];
                for(int direction=0;direction<4;direction++)
                {
                    clips[pose][direction]=new Sprite[ClipLengths[pose]];
                    for(int frame=0;frame<ClipLengths[pose];frame++)
                    {
                        Rect rect=new Rect(frame*cell,(state*4+direction)*cell,
                            cell,cell);
                        clips[pose][direction][frame]=Sprite.Create(
                            atlas,rect,new Vector2(.5f,.42f),100f,0,
                            SpriteMeshType.FullRect);
                        clips[pose][direction][frame].name=
                            kind+"_"+((NecroPose)pose)+"_"+((NecroDirection)direction)+"_"+frame;
                    }
                }
            }
        }

        public int FrameCount(NecroPose pose)
        {
            if(clips[(int)pose]==null)pose=NecroPose.Attack;
            return ClipLengths[(int)pose];
        }
        public float Duration(NecroPose pose)
        {
            if(clips[(int)pose]==null)pose=NecroPose.Attack;
            return ClipLengths[(int)pose]/ClipFps[(int)pose];
        }
        public Sprite Frame(NecroPose pose,NecroDirection direction,float seconds,bool loop)
        {
            int p=(int)pose;
            if(clips[p]==null)p=(int)NecroPose.Attack;
            int d=(int)direction;
            Sprite[] sequence=clips[p][d];
            int n=sequence.Length;
            int index=Mathf.FloorToInt(Mathf.Max(0,seconds)*ClipFps[p]);
            index=loop?index%n:Mathf.Min(n-1,index);
            return sequence[index];
        }
        public bool Has(NecroPose pose) {return clips[(int)pose]!=null;}
    }

    /// <summary>
    /// Runtime 2D-frame authoring. The approved illustration is sampled to create
    /// separate directional frames and distinct walk / attack / ritual poses.
    /// Caches one RGBA atlas per kind; no pixel transforms happen during combat.
    ///
    /// Limitations: frames for rear/side directions are constructed from the
    /// approved front view. Production hand-illustrated directional sheets can
    /// replace these later using Resources/AnimationSheets/<kind>.
    /// </summary>
    public static class NecroFrameLibrary
    {
        static readonly Dictionary<string,NecroFrameSet> cache=
            new Dictionary<string,NecroFrameSet>();
        public static readonly string[] AnimatedCreatures={
            "rat","bat","slime","skeleton","wisp",
            "warden","grave_lord","marsh_lord"
        };

        public static bool Supports(string kind)
        {
            if(kind=="necromante")return true;
            for(int i=0;i<AnimatedCreatures.Length;i++)
                if(kind==AnimatedCreatures[i])return true;
            return false;
        }

        public static NecroFrameSet Get(string kind,Sprite source)
        {
            if(!Supports(kind)||source==null)return null;
            if(cache.TryGetValue(kind,out NecroFrameSet existing))return existing;
            bool hero=kind=="necromante";
            int cell=hero?96:80;
            int states=hero?6:4;
            int width=6*cell,height=states*4*cell;

            // Future asset replacement: atlases with the same 6-column/
            // four-direction layout can be imported without changing gameplay.
            Texture2D atlas=Resources.Load<Texture2D>("AnimationSheets/"+kind);
            bool generated=false;
            if(atlas==null)
            {
                Color32[] original=ReadablePixels(source.texture,
                    out int sourceWidth,out int sourceHeight);
                Color32[] pixels=new Color32[width*height];
                for(int pose=0;pose<states;pose++)
                {
                    int action=hero?pose:(pose==3?5:pose);
                    int count=FrameCount((NecroPose)action);
                    for(int dir=0;dir<4;dir++)
                        for(int frame=0;frame<count;frame++)
                            DrawFrame(original,sourceWidth,sourceHeight,pixels,width,
                                cell,action,dir,frame,count,hero,kind);
                }
                atlas=new Texture2D(width,height,TextureFormat.RGBA32,false);
                atlas.name="Necromancer frame atlas - "+kind;
                atlas.wrapMode=TextureWrapMode.Clamp;
                atlas.filterMode=FilterMode.Bilinear;
                atlas.SetPixels32(pixels);
                atlas.Apply(false,true);
                generated=true;
            }
            else if(atlas.width!=width || atlas.height!=height)
            {
                Debug.LogWarning("Ignoring invalid animation sheet "+kind+
                    "; expected "+width+"x"+height);
                return null;
            }
            NecroFrameSet set=new NecroFrameSet(kind,atlas,cell,hero);
            cache[kind]=set;
            if(generated)
                Debug.Log("[Necromancer Animation] Generated "+kind+" ("+
                    width+" x "+height+")");
            return set;
        }

        static int FrameCount(NecroPose pose)
        {
            switch(pose)
            {
                case NecroPose.Idle:return 3;
                case NecroPose.Walk:return 6;
                case NecroPose.Raise:return 5;
                case NecroPose.Hurt:return 2;
                default:return 4;
            }
        }

        static Color32[] ReadablePixels(Texture2D source,out int width,out int height)
        {
            width=source.width;height=source.height;
            RenderTexture previous=RenderTexture.active;
            RenderTexture temporary=RenderTexture.GetTemporary(width,height,0,
                RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default);
            try
            {
                Graphics.Blit(source,temporary);
                RenderTexture.active=temporary;
                Texture2D copy=new Texture2D(width,height,TextureFormat.RGBA32,false);
                copy.ReadPixels(new Rect(0,0,width,height),0,0,false);
                copy.Apply();
                Color32[] colors=copy.GetPixels32();
                if(Application.isPlaying)UnityEngine.Object.Destroy(copy);
                else UnityEngine.Object.DestroyImmediate(copy);
                return colors;
            }
            finally
            {
                RenderTexture.active=previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        static void DrawFrame(Color32[] src,int sw,int sh,Color32[] dest,
            int atlasWidth,int cell,int pose,int direction,int frame,
            int frameCount,bool hero,string kind)
        {
            float progress=frameCount<=1?0:frame/(float)frameCount;
            float cycle=progress*Mathf.PI*2f;
            int ox=frame*cell, oy=(pose*4+direction)*cell;
            bool walking=pose==(int)NecroPose.Walk;
            bool attack=pose==(int)NecroPose.Attack;
            bool casting=pose==(int)NecroPose.Cast;
            bool raising=pose==(int)NecroPose.Raise;
            bool hurt=pose==(int)NecroPose.Hurt;
            bool front=direction==0,back=direction==1;
            bool left=direction==2;

            // Each frame has a distinct silhouette deformation rather than
            // relying entirely on one moving SpriteRenderer.
            float stride=walking?Mathf.Sin(cycle):0f;
            float limb=walking?Mathf.Sin(cycle+Mathf.PI*.5f):0f;
            float recoil=attack?Mathf.Sin(progress*Mathf.PI)*.075f:0f;
            float rise=raising?Mathf.Sin(progress*Mathf.PI)*.09f:0f;
            float cast=casting?Mathf.Sin(progress*Mathf.PI)*.075f:0f;
            float recoilHurt=hurt?(frame==0?-.09f:.04f):0f;
            float perspective=(front||back)?1f:.83f;
            float breath=pose==(int)NecroPose.Idle?
                (frame==1?.012f:frame==2?-.008f:0f):0f;
            bool flip=left;

            for(int py=0;py<cell;py++)
            {
                float v=(py+.5f)/cell;
                for(int px=0;px<cell;px++)
                {
                    float u=(px+.5f)/cell;
                    float bodyHeight=Mathf.Clamp01((v-.10f)/.75f);
                    float horizontal=limb*(1f-bodyHeight)*.036f+
                        stride*(.25f+bodyHeight)*.025f;
                    float wave=(raising?rise:casting?cast:0f)*(bodyHeight-.3f);
                    float shift=(recoil+recoilHurt)*(bodyHeight*.8f+.2f);
                    float sourceU=.5f+(u-.5f)/perspective-horizontal-wave-shift;
                    float sourceV=v-(walking ? Mathf.Abs(stride)*.023f : 0f)
                        -(attack ? recoil*.55f : 0f)-rise*.14f;
                    sourceV=.5f+(sourceV-.5f)/(1f+breath);
                    if(flip)sourceU=1f-sourceU;

                    int sx=Mathf.RoundToInt(sourceU*(sw-1));
                    int sy=Mathf.RoundToInt(sourceV*(sh-1));
                    if(sx<0||sx>=sw||sy<0||sy>=sh)continue;
                    Color32 color=src[sy*sw+sx];
                    if(color.a<8)continue;

                    if(back && hero && v>.50f && v<.78f &&
                       u>.33f && u<.67f)
                    {
                        // Simple hair/hood covering of the front-facing face.
                        // It does NOT substitute an artist-made rear view.
                        color.r=(byte)Mathf.Lerp(color.r,170f,.65f);
                        color.g=(byte)Mathf.Lerp(color.g,160f,.65f);
                        color.b=(byte)Mathf.Lerp(color.b,179f,.65f);
                    }
                    else if(!front && !back && hero)
                    {
                        color.r=(byte)Mathf.Lerp(color.r,140f,.13f);
                    }
                    if(hurt && frame==0)
                    {
                        color.r=(byte)Mathf.Lerp(color.r,240f,.32f);
                        color.g=(byte)Mathf.Lerp(color.g,119f,.30f);
                    }
                    int at=(oy+py)*atlasWidth+(ox+px);
                    dest[at]=color;
                }
            }

            // Hand-drawn accents generated per frame, not applied as a
            // fullscreen overlay: staff flare and the summoning glyph.
            if(casting || raising)
            {
                float size=raising?cell*.33f:cell*.15f;
                int cx=ox+Mathf.RoundToInt(cell*(left?.30f:.74f));
                int cy=oy+Mathf.RoundToInt(cell*(raising?.28f:.69f));
                int radius=Mathf.RoundToInt(size*(.65f+.24f*Mathf.Sin(progress*Mathf.PI)));
                Color32 tint=new Color32(130,242,230,220);
                for(int angle=0;angle<360;angle+=6)
                {
                    float rad=angle*Mathf.Deg2Rad;
                    int x=cx+Mathf.RoundToInt(Mathf.Cos(rad)*radius);
                    int y=cy+Mathf.RoundToInt(Mathf.Sin(rad)*radius*.56f);
                    if(x>=ox&&x<ox+cell&&y>=oy&&y<oy+cell)
                        dest[y*atlasWidth+x]=tint;
                }
                if(hero && casting)
                {
                    int x=cx,y=cy;
                    if(x>=ox&&x<ox+cell&&y>=oy&&y<oy+cell)
                        dest[y*atlasWidth+x]=new Color32(229,254,238,255);
                }
            }
            if(kind=="bat" && walking)
            {
                // Edge pulse produces visibly different flight silhouettes
                // even when the creature is otherwise stationary.
                int startX=ox+Mathf.RoundToInt(cell*.08f);
                int tipY=oy+Mathf.RoundToInt(cell*(.49f+.12f*stride));
                for(int dx=0;dx<6;dx++)
                {
                    int x=startX+dx;
                    if(tipY>=oy&&tipY<oy+cell)dest[tipY*atlasWidth+x]=
                        new Color32(120,98,146,205);
                }
            }
        }
    }
}

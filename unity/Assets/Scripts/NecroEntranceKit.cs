using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    [Serializable] public sealed class BonehollowEntranceLayout
    {
        public int revision;
        public string area,description;
        public BonehollowEntrancePiece[] modules;
    }

    [Serializable] public sealed class BonehollowEntrancePiece
    {
        public string id,kind,surface;
        public float x,y,width,height,rotation,alpha;
    }

    /// <summary>
    /// A replaceable vertical-slice art kit for the natural cave mouth.
    /// Every independently editable piece is placed by JSON. The rock grid
    /// remains the ONLY collision authority; none of these drawables has a
    /// collider or mutates any stage/character/save state.
    /// </summary>
    public static class NecroEntranceKit
    {
        const int TextureSize=96;
        public const string LayoutPath="Data/bonehollow_entrance_modules";
        static readonly Dictionary<string,Sprite> cache=
            new Dictionary<string,Sprite>();
        static readonly Color Deep=new Color(.14f,.12f,.19f,.87f);
        static readonly Color Stone=new Color(.34f,.30f,.38f,.92f);
        static readonly Color StoneMid=new Color(.46f,.41f,.48f,.92f);
        static readonly Color Rim=new Color(.65f,.58f,.60f,.72f);
        static readonly Color Bone=new Color(.78f,.73f,.63f,.93f);
        static readonly Color BoneShade=new Color(.43f,.37f,.35f,.77f);

        public static BonehollowEntranceLayout ReadLayout()
        {
            TextAsset data=Resources.Load<TextAsset>(LayoutPath);
            return data==null?null:
                JsonUtility.FromJson<BonehollowEntranceLayout>(data.text);
        }

        public static bool KnownKind(string kind)
        {
            return kind=="floor_silt" || kind=="floor_fracture" ||
                kind=="floor_grit" || kind=="wall_facet" ||
                kind=="wall_rim" || kind=="rock_chip" ||
                kind=="rock_slate" || kind=="bone_shards" ||
                kind=="bone_rib";
        }

        public static bool Valid(LevelDefinition area,BonehollowEntrancePiece piece)
        {
            if(area==null || !NecroCaveNetwork.HasNetwork(area) ||
                piece==null || string.IsNullOrEmpty(piece.id) ||
                !KnownKind(piece.kind) ||
                piece.width<=0f || piece.height<=0f ||
                piece.alpha<=0f || piece.alpha>1f)return false;
            if(piece.x<2f || piece.y<1f ||
                piece.x>area.width-2f || piece.y>area.height-1f)
                return false;
            if(piece.surface=="wall")
            {
                if(piece.width>1f || piece.height>1f)return false;
                return NecroCaveNetwork.IsWall(area,
                    Mathf.FloorToInt(piece.x),Mathf.FloorToInt(piece.y));
            }
            if(piece.surface!="floor")return false;
            if(!NecroCaveNetwork.HasClearance(area,piece.x,piece.y,.85f))
                return false;
            Vector2 location=new Vector2(piece.x,piece.y);
            if(Vector2.Distance(location,
                new Vector2(area.startX,area.startY))<2.5f)return false;
            // Low-contrast decals never take the place of encounter artwork.
            if(area.enemies!=null)foreach(Spawn spawn in area.enemies)
                if(Vector2.Distance(location,
                    new Vector2(spawn.x,spawn.y))<1.5f)return false;
            if(area.resources!=null)foreach(ResourceSpawn resource in area.resources)
                if(Vector2.Distance(location,
                    new Vector2(resource.x,resource.y))<1.2f)return false;
            return true;
        }

        public static int Populate(LevelDefinition area,Transform parent)
        {
            if(area==null || area.id!="bonehollow" || parent==null)return 0;
            BonehollowEntranceLayout layout=ReadLayout();
            if(layout==null || layout.revision!=1 ||
                layout.area!=area.id || layout.modules==null)
            {
                Debug.LogWarning("Bonehollow entrance art layout unavailable; "+
                    "the navigable procedural terrain remains intact.");
                return 0;
            }
            GameObject group=new GameObject("Bonehollow / Entrance modular kit v1");
            group.transform.SetParent(parent,false);
            int created=0;
            foreach(BonehollowEntrancePiece piece in layout.modules)
            {
                if(!Valid(area,piece))
                {
                    Debug.LogWarning("Bonehollow art piece skipped: "+
                        (piece==null?"null":piece.id));
                    continue;
                }
                Sprite source=Get(piece.kind);
                if(source==null)continue;
                GameObject objectRoot=new GameObject(
                    "Entrance piece / "+piece.id+" / "+piece.kind);
                objectRoot.transform.SetParent(group.transform,false);
                Vector2 position=NecroLevels.At(area,piece.x,piece.y);
                objectRoot.transform.position=new Vector3(position.x,position.y,0f);
                objectRoot.transform.rotation=Quaternion.Euler(0f,0f,piece.rotation);
                SpriteRenderer renderer=objectRoot.AddComponent<SpriteRenderer>();
                renderer.sprite=source;
                renderer.sortingOrder=piece.kind.StartsWith("floor_")?-47:
                    piece.surface=="wall"?-45:-4;
                renderer.color=new Color(1f,1f,1f,piece.alpha);
                Vector2 size=source.bounds.size;
                objectRoot.transform.localScale=new Vector3(
                    piece.width/size.x,piece.height/size.y,1f);
                created++;
            }
            return created;
        }

        public static Sprite Get(string kind)
        {
            if(!KnownKind(kind))return null;
            if(cache.TryGetValue(kind,out Sprite existing))return existing;
            Color[] image=new Color[TextureSize*TextureSize];
            Paint(image,kind);
            Texture2D texture=new Texture2D(
                TextureSize,TextureSize,TextureFormat.RGBA32,false);
            texture.name="Bonehollow modular / "+kind;
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.SetPixels(image);
            texture.Apply(false,true);
            Sprite sprite=Sprite.Create(texture,new Rect(0,0,
                TextureSize,TextureSize),new Vector2(.5f,.5f),
                100f,0,SpriteMeshType.FullRect);
            sprite.name=kind;
            cache[kind]=sprite;
            return sprite;
        }

        static void Pixel(Color[] image,int x,int y,Color ink)
        {
            if(x<0||y<0||x>=TextureSize||y>=TextureSize||
                ink.a<=.001f)return;
            int index=y*TextureSize+x;
            Color old=image[index];
            float alpha=ink.a+old.a*(1f-ink.a);
            if(alpha<=0f)return;
            image[index]=new Color(
                (ink.r*ink.a+old.r*old.a*(1f-ink.a))/alpha,
                (ink.g*ink.a+old.g*old.a*(1f-ink.a))/alpha,
                (ink.b*ink.a+old.b*old.a*(1f-ink.a))/alpha,alpha);
        }

        static void Oval(Color[] image,float cx,float cy,
            float rx,float ry,Color tint,bool feather=true)
        {
            int x0=Mathf.Max(0,Mathf.FloorToInt((cx-rx)*TextureSize));
            int x1=Mathf.Min(TextureSize-1,Mathf.CeilToInt((cx+rx)*TextureSize));
            int y0=Mathf.Max(0,Mathf.FloorToInt((cy-ry)*TextureSize));
            int y1=Mathf.Min(TextureSize-1,Mathf.CeilToInt((cy+ry)*TextureSize));
            for(int y=y0;y<=y1;y++)
                for(int x=x0;x<=x1;x++)
                {
                    float dx=((x+.5f)/TextureSize-cx)/Mathf.Max(.001f,rx);
                    float dy=((y+.5f)/TextureSize-cy)/Mathf.Max(.001f,ry);
                    float d=dx*dx+dy*dy;
                    if(d>=1f)continue;
                    Color color=tint;
                    if(feather)color.a*=Mathf.Pow(1f-d,1.8f);
                    Pixel(image,x,y,color);
                }
        }

        static void Stroke(Color[] image,float x0,float y0,
            float x1,float y1,float width,Color ink)
        {
            int steps=Mathf.Max(2,Mathf.CeilToInt(
                Vector2.Distance(new Vector2(x0,y0),
                    new Vector2(x1,y1))*TextureSize*1.5f));
            for(int step=0;step<=steps;step++)
            {
                float t=(float)step/steps;
                Oval(image,Mathf.Lerp(x0,x1,t),Mathf.Lerp(y0,y1,t),
                    width,width,ink,false);
            }
        }

        static void Polygon(Color[] image,Color tint,params Vector2[] v)
        {
            if(v==null||v.Length<3)return;
            float xmin=1f,ymin=1f,xmax=0f,ymax=0f;
            foreach(Vector2 vertex in v)
            {
                xmin=Mathf.Min(xmin,vertex.x);xmax=Mathf.Max(xmax,vertex.x);
                ymin=Mathf.Min(ymin,vertex.y);ymax=Mathf.Max(ymax,vertex.y);
            }
            for(int y=Mathf.Max(0,Mathf.FloorToInt(ymin*TextureSize));
                y<=Mathf.Min(TextureSize-1,Mathf.CeilToInt(ymax*TextureSize));y++)
                for(int x=Mathf.Max(0,Mathf.FloorToInt(xmin*TextureSize));
                    x<=Mathf.Min(TextureSize-1,Mathf.CeilToInt(xmax*TextureSize));x++)
                {
                    Vector2 p=new Vector2((x+.5f)/TextureSize,
                        (y+.5f)/TextureSize);
                    bool inside=false;
                    for(int i=0,j=v.Length-1;i<v.Length;j=i++)
                        if((v[i].y>p.y)!=(v[j].y>p.y) &&
                            p.x<(v[j].x-v[i].x)*(p.y-v[i].y)/
                                (v[j].y-v[i].y)+v[i].x)inside=!inside;
                    if(inside)Pixel(image,x,y,tint);
                }
        }

        static Vector2 P(float x,float y){return new Vector2(x,y);}

        static void Rock(Color[] im,bool large)
        {
            Oval(im,.51f,.19f,.39f,.13f,new Color(.07f,.06f,.12f,.31f));
            if(large)
            {
                Polygon(im,Deep,P(.14f,.23f),P(.16f,.55f),P(.38f,.79f),
                    P(.71f,.83f),P(.86f,.53f),P(.81f,.22f));
                Polygon(im,Stone,P(.18f,.26f),P(.20f,.56f),P(.41f,.73f),
                    P(.54f,.52f),P(.50f,.24f));
                Polygon(im,StoneMid,P(.41f,.73f),P(.70f,.77f),P(.82f,.50f),
                    P(.56f,.53f));
                Polygon(im,Rim,P(.21f,.56f),P(.40f,.72f),P(.54f,.54f),
                    P(.46f,.57f));
            }
            else
            {
                Polygon(im,Deep,P(.11f,.24f),P(.21f,.43f),P(.38f,.56f),
                    P(.59f,.30f),P(.70f,.20f));
                Polygon(im,StoneMid,P(.16f,.26f),P(.23f,.42f),P(.35f,.50f),
                    P(.42f,.25f));
                Polygon(im,Stone,P(.43f,.21f),P(.52f,.51f),P(.75f,.59f),
                    P(.85f,.25f));
                Polygon(im,Rim,P(.52f,.48f),P(.75f,.57f),P(.66f,.45f));
            }
            Stroke(im,.27f,.35f,.37f,.46f,.012f,Rim);
        }

        static void Paint(Color[] im,string kind)
        {
            if(kind=="floor_silt")
            {
                Oval(im,.44f,.48f,.46f,.32f,
                    new Color(.42f,.32f,.32f,.34f));
                Oval(im,.61f,.41f,.30f,.25f,
                    new Color(.59f,.44f,.37f,.22f));
                Oval(im,.26f,.61f,.21f,.18f,
                    new Color(.23f,.20f,.27f,.27f));
                Stroke(im,.17f,.37f,.35f,.32f,.007f,
                    new Color(.70f,.56f,.47f,.31f));
            }
            else if(kind=="floor_fracture")
            {
                Color cut=new Color(.12f,.10f,.16f,.73f);
                Color rim=new Color(.64f,.58f,.61f,.43f);
                Stroke(im,.10f,.32f,.33f,.46f,.012f,cut);
                Stroke(im,.33f,.46f,.57f,.37f,.015f,cut);
                Stroke(im,.57f,.37f,.86f,.67f,.010f,cut);
                Stroke(im,.35f,.46f,.43f,.71f,.010f,cut);
                Stroke(im,.19f,.30f,.34f,.44f,.006f,rim);
                Stroke(im,.58f,.40f,.84f,.71f,.005f,rim);
            }
            else if(kind=="floor_grit")
            {
                for(int n=0;n<29;n++)
                {
                    uint h=unchecked((uint)(n*987643+76391));
                    h=(h^(h>>13))*1274126177u;
                    float x=.12f+(h&255u)/255f*.76f;
                    float y=.18f+((h>>8)&255u)/255f*.60f;
                    float radius=.007f+((h>>16)&3u)*.003f;
                    Oval(im,x,y,radius,radius*.7f,
                        (n%3==0)?Rim:Stone,false);
                }
            }
            else if(kind=="wall_facet")
            {
                Polygon(im,Deep,P(.09f,.11f),P(.08f,.66f),P(.41f,.94f),
                    P(.90f,.73f),P(.93f,.16f),P(.56f,.07f));
                Polygon(im,Stone,P(.12f,.19f),P(.15f,.64f),P(.41f,.87f),
                    P(.49f,.58f),P(.43f,.15f));
                Polygon(im,StoneMid,P(.43f,.15f),P(.51f,.57f),P(.86f,.69f),
                    P(.88f,.20f));
                Polygon(im,Rim,P(.16f,.65f),P(.41f,.84f),P(.51f,.59f),
                    P(.46f,.62f));
                Stroke(im,.22f,.44f,.38f,.53f,.008f,
                    new Color(.14f,.13f,.20f,.58f));
            }
            else if(kind=="wall_rim")
            {
                Polygon(im,Deep,P(.07f,.38f),P(.15f,.67f),P(.36f,.81f),
                    P(.69f,.69f),P(.91f,.43f),P(.81f,.15f),P(.54f,.27f));
                Polygon(im,Stone,P(.14f,.46f),P(.24f,.64f),P(.43f,.70f),
                    P(.47f,.43f),P(.29f,.30f));
                Polygon(im,StoneMid,P(.47f,.45f),P(.65f,.64f),P(.86f,.43f),
                    P(.74f,.21f));
                Stroke(im,.21f,.62f,.43f,.69f,.010f,Rim);
                Stroke(im,.50f,.60f,.70f,.61f,.009f,Rim);
            }
            else if(kind=="rock_chip")Rock(im,false);
            else if(kind=="rock_slate")Rock(im,true);
            else if(kind=="bone_shards")
            {
                Oval(im,.50f,.24f,.34f,.13f,
                    new Color(.07f,.06f,.10f,.28f));
                Stroke(im,.21f,.36f,.73f,.53f,.043f,BoneShade);
                Stroke(im,.22f,.39f,.73f,.56f,.033f,Bone);
                Stroke(im,.32f,.63f,.72f,.31f,.030f,Bone);
                Oval(im,.24f,.40f,.042f,.038f,Bone,false);
                Oval(im,.72f,.56f,.041f,.041f,Bone,false);
                Oval(im,.32f,.63f,.039f,.036f,Bone,false);
            }
            else if(kind=="bone_rib")
            {
                Oval(im,.51f,.29f,.33f,.11f,
                    new Color(.09f,.08f,.12f,.25f));
                Stroke(im,.21f,.29f,.77f,.44f,.029f,BoneShade);
                Stroke(im,.22f,.33f,.77f,.48f,.022f,Bone);
                for(int j=0;j<4;j++)
                {
                    float x=.31f+j*.12f;
                    Stroke(im,x,.34f,x-.05f,.52f+(j%2)*.05f,.022f,BoneShade);
                    Stroke(im,x+.01f,.37f,x-.04f,.55f+(j%2)*.05f,.016f,Bone);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Lightweight map for the active mobile IMGUI HUD. Uses the same world
    /// coordinates, bounds and obstacles as collision. Discovery is persisted
    /// per cave region as a compact bit mask in the existing save format.
    /// No imported artwork, camera or RenderTexture is required.
    /// </summary>
    public partial class NecroPrototype
    {
        sealed class MiniMapRegion
        {
            public LevelDefinition definition;
            public int columns,rows;
            public bool[] revealed;
            public Color32[] pixels;
            public Texture2D texture;
            public Vector2 lastProbe=new Vector2(-999f,-999f);
            public float lastProbeTime=-10f;
        }

        readonly Dictionary<int,MiniMapRegion> miniMapRegions=
            new Dictionary<int,MiniMapRegion>();
        MiniMapRegion miniMapRefuge;
        bool miniMapExpanded;
        int miniMapZoom;
        static readonly Color MapBackdrop=new Color(.065f,.055f,.085f,.98f);
        static readonly Color MapBorder=new Color(.58f,.49f,.38f,.95f);
        static readonly Color MapPlayer=new Color(.38f,1f,.64f);
        static readonly Color MapEnemy=new Color(.98f,.33f,.39f);
        static readonly Color MapServant=new Color(.40f,.87f,.95f);
        static readonly Color MapPortal=new Color(1f,.76f,.32f);

        MiniMapRegion EnsureMiniMapRegion()
        {
            if(level==null)return null;
            MiniMapRegion region;
            if(inSanctuary)
            {
                if(miniMapRefuge==null || miniMapRefuge.definition!=level)
                    miniMapRefuge=NewMiniMapRegion(level,null,true);
                region=miniMapRefuge;
            }
            else if(!miniMapRegions.TryGetValue(stage,out region) ||
                    region.definition!=level)
            {
                NecroStageRecord snapshot=previousStageSnapshots!=null &&
                    stage>=0 && stage<previousStageSnapshots.Length
                    ? previousStageSnapshots[stage] : null;
                region=NewMiniMapRegion(level,
                    snapshot!=null?snapshot.exploration:null,false);
                miniMapRegions[stage]=region;
            }
            return region;
        }

        MiniMapRegion NewMiniMapRegion(LevelDefinition definition,
            string serialized,bool revealAll)
        {
            MiniMapRegion map=new MiniMapRegion();
            map.definition=definition;
            map.columns=Mathf.Max(1,Mathf.CeilToInt(definition.width));
            map.rows=Mathf.Max(1,Mathf.CeilToInt(definition.height));
            int length=map.columns*map.rows;
            map.revealed=new bool[length];
            map.pixels=new Color32[length];
            if(revealAll)
            {
                for(int i=0;i<length;i++)map.revealed[i]=true;
            }
            else if(!string.IsNullOrEmpty(serialized))
            {
                try
                {
                    byte[] stored=Convert.FromBase64String(serialized);
                    if(stored.Length==(length+7)/8)
                        for(int i=0;i<length;i++)
                            map.revealed[i]=(stored[i>>3]&(1<<(i&7)))!=0;
                }
                catch(FormatException)
                {
                    Debug.LogWarning("Necromancer minimap: invalid saved discovery; resetting.");
                }
            }
            map.texture=new Texture2D(map.columns,map.rows,
                TextureFormat.RGBA32,false);
            map.texture.name="Necromancer minimap - "+definition.id;
            map.texture.filterMode=FilterMode.Point;
            map.texture.wrapMode=TextureWrapMode.Clamp;
            RefreshMiniMapTexture(map);
            return map;
        }

        static string PackMiniMap(MiniMapRegion map)
        {
            if(map==null || map.revealed==null)return null;
            byte[] packed=new byte[(map.revealed.Length+7)/8];
            for(int i=0;i<map.revealed.Length;i++)
                if(map.revealed[i])packed[i>>3]|=(byte)(1<<(i&7));
            return Convert.ToBase64String(packed);
        }

        string SaveMiniMapForStage(StageInstance instance)
        {
            if(instance==null || stages==null)return null;
            for(int i=0;i<stages.Length;i++)
            {
                if(stages[i]!=instance)continue;
                MiniMapRegion map;
                return miniMapRegions.TryGetValue(i,out map)?PackMiniMap(map):null;
            }
            return null;
        }

        void RevealMiniMapAtPlayer()
        {
            if(player==null || player.body==null || level==null ||
                inSanctuary)return;
            MiniMapRegion map=EnsureMiniMapRegion();
            if(map==null)return;
            Vector2 point=player.body.position;
            if(Time.time-map.lastProbeTime<.12f ||
                (point-map.lastProbe).sqrMagnitude<.25f)return;
            map.lastProbe=point;
            map.lastProbeTime=Time.time;
            int x0=Mathf.Max(0,Mathf.FloorToInt(point.x-5.3f));
            int x1=Mathf.Min(map.columns-1,Mathf.CeilToInt(point.x+5.3f));
            int y0=Mathf.Max(0,Mathf.FloorToInt(point.y-5.3f));
            int y1=Mathf.Min(map.rows-1,Mathf.CeilToInt(point.y+5.3f));
            bool changed=false;
            for(int y=y0;y<=y1;y++)
                for(int x=x0;x<=x1;x++)
                {
                    float dx=x+.5f-point.x,dy=y+.5f-point.y;
                    if(dx*dx+dy*dy>28.1f)continue;
                    int i=y*map.columns+x;
                    if(map.revealed[i])continue;
                    map.revealed[i]=true;
                    changed=true;
                }
            if(changed)RefreshMiniMapTexture(map);
        }

        void RefreshMiniMapTexture(MiniMapRegion map)
        {
            LevelDefinition zone=map.definition;
            for(int y=0;y<map.rows;y++)
                for(int x=0;x<map.columns;x++)
                {
                    int index=y*map.columns+x;
                    if(!map.revealed[index])
                    {
                        map.pixels[index]=new Color32(13,11,19,255);
                        continue;
                    }
                    // Small stable noise suggests stone, without generating
                    // separate geometry or exaggerating corridor locations.
                    int noise=(x*73+y*151+x*y*17)&15;
                    Color32 color=new Color32((byte)(59+noise),
                        (byte)(54+noise),(byte)(69+noise),255);
                    if(zone.obstacles!=null)
                        foreach(Obstacle rock in zone.obstacles)
                        {
                            float dx=x+.5f-rock.x;
                            float dy=zone.height-(y+.5f)-rock.y;
                            if(dx*dx+dy*dy>rock.r*rock.r)continue;
                            color=new Color32((byte)(101+noise),
                                (byte)(91+noise),(byte)(107+noise),255);
                            break;
                        }
                    map.pixels[index]=color;
                }
            map.texture.SetPixels32(map.pixels);
            map.texture.Apply(false,false);
        }

        bool MiniMapDiscovered(MiniMapRegion map,Vector2 p)
        {
            int x=Mathf.FloorToInt(p.x),y=Mathf.FloorToInt(p.y);
            return x>=0 && y>=0 && x<map.columns && y<map.rows &&
                map.revealed[y*map.columns+x];
        }

        // The map coordinates are always Unity world coordinates; the image
        // is flipped vertically by IMGUI's texture UV convention.
        static Rect MiniMapWindow(LevelDefinition zone,Vector2 center,float zoom,
            bool centered)
        {
            float w=zone.width/zoom,h=zone.height/zoom;
            if(!centered)return new Rect(0f,0f,zone.width,zone.height);
            w=Mathf.Min(w,zone.width);h=Mathf.Min(h,zone.height);
            return new Rect(
                Mathf.Clamp(center.x-w*.5f,0f,zone.width-w),
                Mathf.Clamp(center.y-h*.5f,0f,zone.height-h),w,h);
        }

        static Rect MiniMapFit(Rect bounds,Rect world)
        {
            float ratio=world.width/Mathf.Max(.1f,world.height);
            float w=Mathf.Min(bounds.width,bounds.height*ratio);
            float h=w/ratio;
            return new Rect(bounds.center.x-w*.5f,bounds.center.y-h*.5f,w,h);
        }

        static Vector2 MiniMapPosition(Rect drawn,Rect world,Vector2 point)
        {
            return new Vector2(drawn.x+(point.x-world.xMin)/world.width*drawn.width,
                drawn.y+(world.yMax-point.y)/world.height*drawn.height);
        }

        void DrawMiniMapDot(Rect bounds,Rect world,Vector2 point,
            Color color,float diameter)
        {
            if(!world.Contains(point))return;
            Vector2 p=MiniMapPosition(bounds,world,point);
            FillUi(new Rect(p.x-diameter*.5f-1f,p.y-diameter*.5f-1f,
                diameter+2f,diameter+2f),new Color(.04f,.035f,.06f,.94f));
            FillUi(new Rect(p.x-diameter*.5f,p.y-diameter*.5f,
                diameter,diameter),color);
        }

        void DrawMiniMapWorld(MiniMapRegion map,Rect canvas,bool expanded)
        {
            float localWidth=Mathf.Min(level.width,17.5f);
            float localHeight=Mathf.Min(level.height,17.5f);
            Vector2 hero=player.body.position;
            Rect local=new Rect(
                Mathf.Clamp(hero.x-localWidth*.5f,0f,level.width-localWidth),
                Mathf.Clamp(hero.y-localHeight*.5f,0f,level.height-localHeight),
                localWidth,localHeight);
            Rect view=expanded?
                MiniMapWindow(level,hero,
                    miniMapZoom==0?1f:miniMapZoom==1?1.7f:2.7f,
                    miniMapZoom!=0):local;
            Rect draw=MiniMapFit(canvas,view);
            FillUi(canvas,MapBackdrop);
            Rect uv=new Rect(view.x/level.width,view.y/level.height,
                view.width/level.width,view.height/level.height);
            GUI.DrawTextureWithTexCoords(draw,map.texture,uv,true);
            float dot=expanded?7f:3.6f;
            if(inSanctuary)
            {
                Vector2 gate=sanctuaryExit;
                if(MiniMapDiscovered(map,gate))
                    DrawMiniMapDot(draw,view,gate,MapPortal,dot+1f);
            }
            else
            {
                Vector2 gate=NecroLevels.At(level,level.sealX,level.sealY);
                if(MiniMapDiscovered(map,gate))
                    DrawMiniMapDot(draw,view,gate,MapPortal,dot+1f);
                Vector2 start=NecroLevels.At(level,level.startX,level.startY);
                if(MiniMapDiscovered(map,start))
                    DrawMiniMapDot(draw,view,start,MapServant,dot);
                if(current!=null)
                    foreach(Actor enemy in current.enemies)
                    {
                        if(enemy==null || enemy.body==null || enemy.hp<=0)continue;
                        Vector2 at=enemy.body.position;
                        // Discovery is about places, not omniscient tracking:
                        // hostiles must also be near the player to be shown.
                        if(!MiniMapDiscovered(map,at) ||
                            (at-(Vector2)player.body.position).sqrMagnitude>144f)
                            continue;
                        bool boss=enemy.kind=="rotboar" ||
                            enemy.kind=="warden" || enemy.kind=="grave_lord" ||
                            enemy.kind=="marsh_lord";
                        DrawMiniMapDot(draw,view,at,MapEnemy,boss?dot+3f:dot);
                    }
            }
            foreach(Actor ally in servants)
                if(ally!=null && ally.body!=null &&
                    MiniMapDiscovered(map,ally.body.position))
                    DrawMiniMapDot(draw,view,ally.body.position,MapServant,dot);
            DrawMiniMapDot(draw,view,player.body.position,MapPlayer,dot+3f);
            // Cardinal north is always upward: no texture rotation.
            GUI.Label(new Rect(canvas.x+3f,canvas.y+1f,16f,17f),
                "N",hudAction);
        }

        void DrawMiniMapCompact(float width)
        {
            if(level==null || player==null)return;
            MiniMapRegion map=EnsureMiniMapRegion();
            if(map==null)return;
            Rect outer=new Rect(width-94f,5f,88f,86f);
            FillUi(outer,MapBorder);
            FillUi(new Rect(outer.x+1f,outer.y+1f,
                outer.width-2f,outer.height-2f),MapBackdrop);
            DrawMiniMapWorld(map,new Rect(outer.x+5f,outer.y+5f,
                outer.width-10f,outer.height-10f),false);
            // The whole preview is a real button. Movement input excludes
            // this top-right region, so taps do not start the joystick.
            if(GUI.Button(outer,GUIContent.none,GUIStyle.none))
            {
                miniMapExpanded=true;
                miniMapZoom=0;
                touchMove=Vector2.zero;
                joystickFingerId=-1;
            }
        }

        void DrawMiniMapExpanded(float width,float height)
        {
            MiniMapRegion map=EnsureMiniMapRegion();
            if(map==null)return;
            EnsureStyles();
            FillUi(new Rect(0f,0f,width,height),new Color(.025f,.020f,.035f,.98f));
            Rect panel=new Rect(10f,Mathf.Max(8f,(height-620f)*.5f),
                width-20f,Mathf.Min(height-16f,620f));
            FillUi(panel,MapBorder);
            FillUi(new Rect(panel.x+2f,panel.y+2f,
                panel.width-4f,panel.height-4f),MapBackdrop);
            GUI.Label(new Rect(panel.x+12f,panel.y+10f,
                panel.width-110f,29f),
                inSanctuary?"MAPA DO REFÚGIO":level.title,hudTitle);
            if(GUI.Button(new Rect(panel.xMax-96f,panel.y+9f,86f,32f),
                "FECHAR",buttonStyle))miniMapExpanded=false;

            Rect canvas=new Rect(panel.x+12f,panel.y+55f,
                panel.width-24f,panel.height-162f);
            DrawMiniMapWorld(map,canvas,true);
            float controlsY=panel.yMax-97f;
            if(GUI.Button(new Rect(panel.x+15f,controlsY,55f,34f),
                "-",buttonStyle))miniMapZoom=Mathf.Max(0,miniMapZoom-1);
            GUI.Label(new Rect(panel.x+77f,controlsY+5f,
                panel.width-154f,25f),
                miniMapZoom==0?"MAPA INTEIRO":
                miniMapZoom==1?"AMPLIAÇÃO 1":"AMPLIAÇÃO 2",smallStyle);
            if(GUI.Button(new Rect(panel.xMax-70f,controlsY,55f,34f),
                "+",buttonStyle))miniMapZoom=Mathf.Min(2,miniMapZoom+1);

            GUI.Label(new Rect(panel.x+8f,panel.yMax-52f,
                panel.width-16f,22f),
                "VERDE: VOCÊ   AZUL: SERVOS   VERMELHO: INIMIGOS",
                smallStyle);
            GUI.Label(new Rect(panel.x+8f,panel.yMax-32f,
                panel.width-16f,22f),
                "DOURADO: SAÍDA   ESCURO: NÃO EXPLORADO",
                smallStyle);
        }
    }
}

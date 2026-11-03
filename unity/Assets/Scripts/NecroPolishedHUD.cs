using System;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Functional gothic portrait HUD for portrait Android displays.
    /// All buttons call the existing mechanics; no button is decorative.
    /// Textures are generated once and reused, with no per-frame allocations.
    /// </summary>
    public partial class NecroPrototype
    {
        Texture2D hudRing,stickDisc,smallRing,healthTexture;
        GUIStyle hudSmall,hudValue,hudTitle,hudAction,hudNumber;
        float hudNoticeFade;
        bool armyInspect,gearInspect;

        static readonly Color UiGold=new Color(.80f,.69f,.57f);
        static readonly Color UiCream=new Color(.93f,.86f,.84f);
        static readonly Color UiPink=new Color(.84f,.40f,.53f);
        static readonly Color UiMana=new Color(.47f,.78f,.82f);
        static readonly Color UiViolet=new Color(.66f,.42f,.83f);
        static readonly Color UiDark=new Color(.105f,.079f,.15f,.92f);

        static Texture2D GenerateDisc(int size,float innerRadius,float outerRadius)
        {
            Texture2D tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
            Color[] pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f-size/2f)/(size*.5f);
                float dy=(y+.5f-size/2f)/(size*.5f);
                float radius=Mathf.Sqrt(dx*dx+dy*dy);
                float rim=Mathf.SmoothStep(0,1,Mathf.InverseLerp(innerRadius,outerRadius,radius));
                float edge=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.965f,1,radius));
                Color pale=new Color(.84f,.68f,.67f,0);
                if(radius<1f)
                {
                    pale=Color.Lerp(new Color(.21f,.15f,.25f,.73f),
                        new Color(.79f,.63f,.65f,.94f),rim);
                    pale.a*=edge;
                }
                pixels[y*size+x]=pale;
            }
            tex.SetPixels(pixels);tex.Apply();
            tex.filterMode=FilterMode.Bilinear;
            return tex;
        }

        void EnsureHudArt()
        {
            if(hudRing!=null)return;
            hudRing=GenerateDisc(128,.83f,.92f);
            stickDisc=GenerateDisc(128,.74f,.88f);
            smallRing=GenerateDisc(64,.64f,.82f);
            healthTexture=Texture2D.whiteTexture;

            hudSmall=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,
                fontSize=11,wordWrap=true};
            hudSmall.normal.textColor=UiCream;
            hudValue=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleLeft,
                fontStyle=FontStyle.Bold,fontSize=13};
            hudValue.normal.textColor=UiCream;
            hudTitle=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,
                fontSize=16,fontStyle=FontStyle.Bold};
            hudTitle.normal.textColor=UiCream;
            hudAction=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,
                fontSize=11,fontStyle=FontStyle.Bold};
            hudAction.normal.textColor=UiCream;
            hudNumber=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,
                fontSize=17,fontStyle=FontStyle.Bold};
            hudNumber.normal.textColor=UiGold;
        }

        void FillUi(Rect bounds,Color tint)
        {
            Color previous=GUI.color;GUI.color=tint;
            GUI.DrawTexture(bounds,Texture2D.whiteTexture);
            GUI.color=previous;
        }

        void RingUi(Rect rect,Texture2D texture,Color tint)
        {
            Color previous=GUI.color;GUI.color=tint;
            GUI.DrawTexture(rect,texture,ScaleMode.StretchToFill,true);
            GUI.color=previous;
        }

        void PanelUi(Rect rect)
        {
            FillUi(rect,new Color(.11f,.085f,.16f,.83f));
            FillUi(new Rect(rect.x,rect.y,rect.width,1),UiGold*.8f);
            FillUi(new Rect(rect.x,rect.yMax-1,rect.width,1),UiGold*.65f);
        }

        void DrawMeterUi(Rect rect,float fraction,Color tint,string text)
        {
            fraction=Mathf.Clamp01(fraction);
            FillUi(rect,new Color(.07f,.055f,.10f,.98f));
            FillUi(new Rect(rect.x+2,rect.y+2,(rect.width-4)*fraction,rect.height-4),tint);
            FillUi(new Rect(rect.x,rect.y,rect.width,1.3f),UiGold*.82f);
            FillUi(new Rect(rect.x,rect.yMax-1.3f,rect.width,1.3f),UiGold*.5f);
            GUI.Label(rect,text,hudSmall);
        }

        void DrawPortraitUi(Rect rect)
        {
            RingUi(rect,hudRing,Color.white);
            Sprite hero=NecroArt.Get("necromante");
            if(hero!=null&&hero.texture!=null)
            {
                // The source artwork is already transparent; crop to head and shoulders.
                Rect image=new Rect(rect.x+12,rect.y+10,rect.width-24,rect.height-24);
                GUI.DrawTextureWithTexCoords(image,hero.texture,
                    new Rect(.15f,.40f,.72f,.58f),true);
            }
            RingUi(rect,hudRing,new Color(.88f,.78f,.64f,1));
        }

        void DrawMiniMapUi(Rect bounds)
        {
            RingUi(bounds,hudRing,Color.white);
            Rect core=new Rect(bounds.x+12,bounds.y+12,bounds.width-24,bounds.height-24);
            FillUi(core,new Color(.20f,.17f,.27f,.80f));
            float cx=core.center.x,cy=core.center.y;
            float view=9f;
            for(int i=0;i<level.obstacles.Length;i++)
            {
                Vector2 at=NecroLevels.At(level,level.obstacles[i].x,level.obstacles[i].y);
                Vector2 d=(at-(Vector2)player.body.position)*2.8f;
                if(d.magnitude<view*2.8f)
                    FillUi(new Rect(cx+d.x-1,cy-d.y-1,3,3),new Color(.52f,.48f,.61f,.75f));
            }
            foreach(Actor enemy in current.enemies)
            {
                Vector2 d=((Vector2)enemy.body.position-(Vector2)player.body.position)*3.2f;
                if(d.magnitude<35)
                    FillUi(new Rect(cx+d.x-2,cy-d.y-2,4,4),UiPink);
            }
            foreach(Actor ally in servants)
            {
                Vector2 d=((Vector2)ally.body.position-(Vector2)player.body.position)*3.2f;
                if(d.magnitude<35)
                    FillUi(new Rect(cx+d.x-2,cy-d.y-2,4,4),UiMana);
            }
            FillUi(new Rect(cx-3,cy-3,6,6),UiGold);
            GUI.Label(new Rect(bounds.x,bounds.y+2,bounds.width,15),"N",hudAction);
            RingUi(bounds,hudRing,UiGold);
        }

        void DrawIconUi(Rect rect,string glyph,string caption,Action click,bool enabled=true)
        {
            RingUi(rect,hudRing,enabled?Color.white:new Color(.5f,.5f,.6f,.7f));
            Rect glyphArea=new Rect(rect.x,rect.y+rect.height*.19f,
                rect.width,rect.height*.47f);
            GUI.Label(glyphArea,glyph,hudNumber);
            GUI.Label(new Rect(rect.x-8,rect.yMax-9,rect.width+16,22),
                caption,hudAction);
            GUI.enabled=enabled;
            if(GUI.Button(rect,GUIContent.none,GUIStyle.none))click();
            GUI.enabled=true;
        }

        void DrawPolishedHUD(float width,float height)
        {
            EnsureHudArt();
            float edge=9f;
            PanelUi(new Rect(0,0,width,116));
            DrawPortraitUi(new Rect(8,7,91,91));
            DrawMeterUi(new Rect(105,12,199,22),player.hp/player.maxHp,UiPink,
                Mathf.CeilToInt(player.hp)+" / "+Mathf.CeilToInt(player.maxHp));
            DrawMeterUi(new Rect(105,39,199,20),(float)mana/maxMana,UiMana,
                mana+" / "+maxMana);
            GUI.Label(new Rect(109,62,170,22),"NÍVEL "+playerLevel+" · XP "+
                experience+"/"+ExperienceThreshold(),hudValue);
            GUI.Label(new Rect(108,85,201,23),"CRISTAIS "+shards+
                "   ·   SERVOS "+CommandUsed()+"/"+CommandLimit,hudSmall);
            DrawMiniMapUi(new Rect(width-107,4,103,103));

            PanelUi(new Rect(76,119,width-152,31));
            GUI.Label(new Rect(79,119,width-158,31),level.title,hudTitle);
            PanelUi(new Rect(14,157,width-28,47));
            string goal=Time.time<noticeUntil ? notice :
                current.claimed?"Região conquistada — explore as passagens.":level.objective;
            GUI.Label(new Rect(22,158,width-44,44),goal,hudSmall);

            // Inspect panels are modal, and their buttons should not overlap
            // underlying skill or navigation controls.
            if(armyInspect){DrawArmyInspect(width,height);return;}
            if(gearInspect){DrawGearInspect(width,height);return;}

            if(stage>0&&NearEntrance())
                if(GUI.Button(new Rect(24,210,width-48,37),
                    "RETORNAR: "+levels[stage-1].title,buttonStyle))Retreat();
            if(current.claimed && stage+1<levels.Length && NearSeal())
                if(GUI.Button(new Rect(24,210,width-48,37),
                    "AVANÇAR: "+levels[stage+1].title,buttonStyle))Advance();
            if(CanAccessCrypt())
                if(GUI.Button(new Rect(24,253,width-48,38),
                    "ENTRAR NA CRIPTA",buttonStyle))OpenCrypt();

            float bottom=height-78;
            // Visual joystick, driven by real multi-touch data in Update.
            Rect pad=new Rect(9,bottom-197,146,146);
            RingUi(pad,stickDisc,Color.white);
            Vector2 stick=touchMove*24f;
            RingUi(new Rect(pad.center.x-28+stick.x,pad.center.y-28-stick.y,56,56),
                smallRing,UiGold);
            GUI.Label(new Rect(15,bottom-47,137,25),"MOVIMENTO",hudAction);

            float right=width-173;
            float top=bottom-196;
            DrawIconUi(new Rect(right,top,75,75),"✦",
                spellUnlocked?"MAGIA":"GOLPE",Cast);
            DrawIconUi(new Rect(right+89,top,75,75),"☠","REVIVER",Raise);
            DrawIconUi(new Rect(right,top+94,75,75),
                huntOrder?"⚔":"◇",huntOrder?"CAÇA":"DEFESA",
                ()=>huntOrder=!huntOrder);
            DrawIconUi(new Rect(right+89,top+94,75,75),
                "✧","EVOLUIR",()=>{touchMove=Vector2.zero;upgradeOpen=true;});

            // Functional quick-navigation buttons.
            float slotWidth=(width-16)/4f;
            string[] labels={"SERVOS","CRIPTA","RITUAIS","EQUIP."};
            for(int i=0;i<4;i++)
            {
                Rect slot=new Rect(8+i*slotWidth,height-59,slotWidth-3,48);
                PanelUi(slot);
                GUI.Label(slot,labels[i],hudAction);
                int choice=i;
                if(GUI.Button(slot,GUIContent.none,GUIStyle.none))
                {
                    if(choice==0)armyInspect=!armyInspect;
                    else if(choice==1 || choice==2)
                    {
                        if(!OpenCrypt())Announce("Retorne ao altar da caverna para abrir a Cripta.");
                    }
                    else gearInspect=!gearInspect;
                }
            }

        }

        void DrawArmyInspect(float width,float height)
        {
            Rect r=new Rect(20,height*.30f,width-40,Mathf.Min(310,height*.37f));
            PanelUi(r);
            GUI.Label(new Rect(r.x+8,r.y+10,r.width-16,32),
                "EXÉRCITO · "+servants.Count+" servos",hudTitle);
            int max=Mathf.Min(servants.Count,5);
            for(int i=0;i<max;i++)
            {
                Actor a=servants[i];
                GUI.Label(new Rect(r.x+22,r.y+48+i*34,r.width-44,30),
                    a.kind.ToUpperInvariant()+"    "+
                    Mathf.CeilToInt(a.hp)+"/"+Mathf.CeilToInt(a.maxHp)+" VIDA",
                    hudSmall);
            }
            if(GUI.Button(new Rect(r.x+14,r.yMax-57,r.width*.5f-19,41),
                "DISPENSAR",buttonStyle))Dismiss();
            if(GUI.Button(new Rect(r.center.x+5,r.yMax-57,r.width*.5f-19,41),
                "FECHAR",buttonStyle))armyInspect=false;
        }

        void DrawGearInspect(float width,float height)
        {
            Rect r=new Rect(32,height*.36f,width-64,194);
            PanelUi(r);
            GUI.Label(new Rect(r.x+10,r.y+13,r.width-20,28),
                "EQUIPAMENTOS",hudTitle);
            GUI.Label(new Rect(r.x+15,r.y+52,r.width-30,30),
                "FOCO ARCANO    GRAU "+staffTier+"/3",hudValue);
            GUI.Label(new Rect(r.x+15,r.y+91,r.width-30,30),
                "MANTO    GRAU "+mantleTier+"/3",hudValue);
            if(GUI.Button(new Rect(r.x+35,r.y+144,r.width-70,39),
                "FECHAR",buttonStyle))gearInspect=false;
        }
    }
}

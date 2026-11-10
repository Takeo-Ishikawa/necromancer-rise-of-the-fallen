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
        int armyPage;

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
            Sprite frame=heroSprites[0];
            if(frame!=null)
            {
                // The HUD portrait shows the same clean artwork as gameplay.
                Texture2D atlas=frame.texture;
                Rect sub=frame.rect;
                Rect uv=new Rect(
                    (sub.x+sub.width*.16f)/atlas.width,
                    (sub.y+sub.height*.38f)/atlas.height,
                    sub.width*.68f/atlas.width,
                    sub.height*.60f/atlas.height);
                Rect image=new Rect(rect.x+12,rect.y+9,
                    rect.width-24,rect.height-24);
                GUI.DrawTextureWithTexCoords(image,atlas,uv,true);
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
            // Less UI obstruction: the first 89 virtual pixels hold all stats.
            PanelUi(new Rect(0,0,width,89));
            DrawPortraitUi(new Rect(8,5,69,69));
            float meterX=82f, meterWidth=Mathf.Max(100f,width-172f);
            string healthLabel=Mathf.CeilToInt(player.hp)+"/"+
                Mathf.CeilToInt(player.maxHp);
            bool recentHit=!inSanctuary && !gameOver &&
                Time.time<heroHitFeedbackUntil;
            if(recentHit)
                healthLabel+="  -"+Mathf.CeilToInt(heroLastDamage)+
                    (heroLastGuarded>=.5f?
                    " (MANTO "+Mathf.CeilToInt(heroLastGuarded)+")":"");
            DrawMeterUi(new Rect(meterX,8,meterWidth,17),
                player.hp/player.maxHp,
                recentHit?new Color(.98f,.39f,.43f):UiPink,healthLabel);
            if(HeroCanUseMagic)
                DrawMeterUi(new Rect(meterX,29,meterWidth,16),
                    (float)mana/maxMana,UiMana,mana+"/"+maxMana);
            else GUI.Label(new Rect(meterX,29,meterWidth,16),"COMBATE FÍSICO",hudSmall);
            GUI.Label(new Rect(83,47,meterWidth+5,19),
                "NÍVEL "+playerLevel+" · XP "+experience+
                "/"+ExperienceThreshold(),hudValue);
            GUI.Label(new Rect(80,65,meterWidth+10,18),
                "CRISTAIS "+shards+" · SERVOS "+CommandUsed()+"/"+CommandLimit,
                hudSmall);
            DrawMiniMapUi(new Rect(width-86,4,81,81));

            GUI.Label(new Rect(68,91,width-136,25),level.title,hudTitle);
            // Never cover gameplay with the permanent "region conquered" panel.
            if(Time.time<noticeUntil)
            {
                PanelUi(new Rect(22,118,width-44,30));
                GUI.Label(new Rect(29,119,width-58,28),notice,hudSmall);
            }
            else if(!current.claimed)
                GUI.Label(new Rect(20,118,width-40,24),level.objective,hudSmall);

            // Inspect panels are modal, and their buttons should not overlap
            // underlying skill or navigation controls.
            if(armyInspect){DrawArmyInspect(width,height);return;}
            if(gearInspect){DrawGearInspect(width,height);return;}

            if(stage>0&&NearEntrance())
                if(GUI.Button(new Rect(24,160,width-48,37),
                    "RETORNAR: "+levels[stage-1].title,buttonStyle))Retreat();
            if(current.claimed && stage+1<levels.Length && NearSeal())
                if(GUI.Button(new Rect(24,160,width-48,37),
                    "AVANÇAR: "+levels[stage+1].title,buttonStyle))Advance();
            if(CanAccessCrypt())
                if(GUI.Button(new Rect(24,205,width-48,38),
                    "ENTRAR NA CRIPTA",buttonStyle))OpenCrypt();

            float bottom=height-64;
            Rect pad=new Rect(14,bottom-177,126,126);
            RingUi(pad,stickDisc,Color.white);
            Vector2 stick=touchMove*23f;
            RingUi(new Rect(pad.center.x-26+stick.x,
                pad.center.y-26-stick.y,52,52),smallRing,UiGold);
            GUI.Label(new Rect(16,bottom-42,123,22),"MOVIMENTO",hudAction);

            float right=width-153;
            float top=bottom-175;
            DrawIconUi(new Rect(right,top,68,68),"☠","REVIVER",Raise);
            DrawIconUi(new Rect(right+78,top,68,68),
                huntOrder?"⚔":"◇",huntOrder?"CAÇA":"DEFESA",
                ToggleServantTactics);
            DrawIconUi(new Rect(right+39,top+80,68,68),
                "✧","EVOLUIR",()=>{touchMove=Vector2.zero;upgradeOpen=true;});

            // Functional quick-navigation buttons.
            float slotWidth=(width-16)/4f;
            string[] labels={"SERVOS","CRIPTA","RITUAIS","EQUIP."};
            for(int i=0;i<4;i++)
            {
                Rect slot=new Rect(8+i*slotWidth,height-48,slotWidth-3,39);
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
            int pages=Mathf.Max(1,Mathf.CeilToInt(servants.Count/5f));
            armyPage=Mathf.Clamp(armyPage,0,pages-1);
            int start=armyPage*5;
            int max=Mathf.Min(servants.Count,start+5);
            for(int i=start;i<max;i++)
            {
                Actor a=servants[i];
                GUI.Label(new Rect(r.x+22,r.y+43+(i-start)*32,r.width-44,29),
                    a.kind.ToUpperInvariant()+"    "+
                    Mathf.CeilToInt(a.hp)+"/"+Mathf.CeilToInt(a.maxHp)+" VIDA",
                    hudSmall);
            }
            if(pages>1)
            {
                if(GUI.Button(new Rect(r.x+22,r.yMax-103,92,31),
                    "<",buttonStyle))armyPage=Mathf.Max(0,armyPage-1);
                GUI.Label(new Rect(r.center.x-52,r.yMax-102,104,29),
                    (armyPage+1)+"/"+pages,hudSmall);
                if(GUI.Button(new Rect(r.xMax-114,r.yMax-103,92,31),
                    ">",buttonStyle))armyPage=Mathf.Min(pages-1,armyPage+1);
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
            GUI.Label(new Rect(r.x+15,r.y+122,r.width-30,20),
                "+"+(mantleTier*18)+" HP | -"+
                Mathf.RoundToInt(MantleDamageReduction()*100f)+
                "% dano recebido",hudSmall);
            if(GUI.Button(new Rect(r.x+35,r.y+144,r.width-70,39),
                "FECHAR",buttonStyle))gearInspect=false;
        }
    }
}

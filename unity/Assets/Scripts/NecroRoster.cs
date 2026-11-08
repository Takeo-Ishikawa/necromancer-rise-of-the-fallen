using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        const string RosterKey="necro.companions.v1";
        readonly List<Actor> resting=new List<Actor>();
        bool inventoryOpen,confirmRelease;
        Actor selectedCompanion;
        Vector2 partyScroll,restScroll;
        GUIStyle board,tile,chosen,button,title,heading,caption;
        static readonly Rect RosterButton=new Rect(12,14,104,40);

        [Serializable] class RosterData
        {
            public List<NecroActorRecord> party=new List<NecroActorRecord>();
            public List<NecroActorRecord> stored=new List<NecroActorRecord>();
        }

        static NecroActorRecord CompanionRecord(Actor a)
        {
            return new NecroActorRecord{kind=a.kind,hp=a.hp,maxHp=a.maxHp,attack=a.attack};
        }

        void SaveCompanions()
        {
            var data=new RosterData();
            foreach(var a in servants)data.party.Add(CompanionRecord(a));
            foreach(var a in resting)data.stored.Add(CompanionRecord(a));
            PlayerPrefs.SetString(RosterKey,JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        void OnApplicationPause(bool paused)
        {
            if(paused && player!=null && current!=null)SaveCompanions();
        }

        bool FindPartySpace(Actor mover,out Vector2 position)
        {
            for(int i=0;i<Mathf.Max(72,servants.Count*3);i++)
            {
                if(TryFindFormationSpot(mover,i,out position))return true;
            }
            position=player.body.position;
            return false;
        }

        void LoadCompanions()
        {
            if(!PlayerPrefs.HasKey(RosterKey))return;
            var data=JsonUtility.FromJson<RosterData>(PlayerPrefs.GetString(RosterKey));
            if(data==null)return;
            RestoreGroup(data.party,true);
            RestoreGroup(data.stored,false);
        }

        void RestoreGroup(List<NecroActorRecord> records,bool active)
        {
            if(records==null)return;
            foreach(var record in records)
            {
                if(record==null || record.hp<=0 ||
                    string.IsNullOrEmpty(record.kind) ||
                    record.kind=="necromante")continue;
                var root=persistentRoot.transform;
                Actor a=record.kind=="rotboar"?CreateRotboar(player.body.position,root,true):
                    Create(record.kind,player.body.position,true,root);
                a.maxHp=record.maxHp;a.hp=record.hp;a.attack=record.attack;
                if(active && CommandUsed()+CommandCost(a.kind)<=CommandLimit &&
                    FindPartySpace(a,out Vector2 position))
                {
                    a.body.position=position;
                    servants.Add(a);
                }
                else
                {
                    resting.Add(a);
                    a.body.gameObject.SetActive(false);
                }
            }
        }

        void TransferSelected()
        {
            Actor a=selectedCompanion;
            if(servants.Remove(a))
            {
                a.pupAttackTime=0f;
                if(a.view!=null)a.view.SetPupAction(0,Vector2.zero);
                if(a.kind=="rotboar")SetBoarPose(a,BoarPose.Idle,0);
                a.body.gameObject.SetActive(false);
                resting.Add(a);
            }
            else
            {
                if(!FindPartySpace(a,out Vector2 position))
                {rosterNotice="Procure uma área livre para chamar este servo.";return;}
                resting.Remove(a);
                a.body.position=position;
                a.body.gameObject.SetActive(true);
                servants.Add(a);
            }
            rosterNotice="";
            SaveCompanions();
        }

        static string CompanionName(Actor a)
        {
            if(a.kind=="rotboar")return "Rotboar";
            if(a.kind=="grave_pup")return "Grave Pup";
            string readable=a.kind.Replace('_',' ');
            return readable.Length>0 ?
                char.ToUpperInvariant(readable[0])+readable.Substring(1):
                "Servo";
        }

        static Texture2D PanelPaint(Color fill,Color rim)
        {
            const int size=32;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(0,Mathf.Abs(x-15.5f)-8);
                float dy=Mathf.Max(0,Mathf.Abs(y-15.5f)-8);
                float edge=Mathf.Max(Mathf.Sqrt(dx*dx+dy*dy)-7.5f,
                    Mathf.Max(Mathf.Abs(x-15.5f),Mathf.Abs(y-15.5f))-15.5f);
                Color c=Color.Lerp(fill,rim,Mathf.Clamp01(edge+1.6f));
                c.a*=Mathf.Clamp01(1-edge);
                tex.SetPixel(x,y,c);
            }
            tex.wrapMode=TextureWrapMode.Clamp;tex.Apply();
            return tex;
        }

        GUIStyle Surface(Color fill,Color rim)
        {
            var style=new GUIStyle{alignment=TextAnchor.MiddleCenter,fontSize=14,
                border=new RectOffset(10,10,10,10),wordWrap=true};
            style.normal.background=PanelPaint(fill,rim);
            style.normal.textColor=UiCream;
            style.hover=style.normal;style.active=style.normal;
            return style;
        }

        string rosterNotice="";
        void EnsureRosterStyles()
        {
            if(board!=null)return;
            board=Surface(new Color(.19f,.13f,.17f,.98f),UiGold);
            tile=Surface(new Color(.25f,.20f,.24f),new Color(.55f,.45f,.50f));
            chosen=Surface(new Color(.20f,.31f,.28f),new Color(.67f,.94f,.79f));
            button=Surface(new Color(.32f,.46f,.39f),new Color(.68f,.79f,.68f));
            title=new GUIStyle(hudTitle){fontSize=25};
            heading=new GUIStyle(hudTitle){fontSize=16};
            caption=new GUIStyle(hudSmall){fontSize=13};
        }

        void Portrait(Actor a,Rect rect)
        {
            Sprite sprite=a.kind=="rotboar"?alliedBoarFrames[0]:
                a.kind=="grave_pup"?NecroCreatureView.PupIdle(true):
                NecroArt.Get(a.kind);
            if(sprite==null)return;
            Rect uv=sprite.rect; Texture2D texture=sprite.texture;
            GUI.DrawTextureWithTexCoords(rect,texture,new Rect(uv.x/texture.width,
                uv.y/texture.height,uv.width/texture.width,uv.height/texture.height));
        }

        void CompanionTile(Actor a,Rect rect)
        {
            if(GUI.Button(rect,"",a==selectedCompanion?chosen:tile))
            {selectedCompanion=a;confirmRelease=false;rosterNotice="";}
            Portrait(a,rect);
        }

        void ScrollByTouch(Rect area,ref Vector2 scroll,bool horizontal)
        {
            if(Input.touchCount==0 || Event.current.type!=EventType.Repaint)return;
            Touch t=Input.GetTouch(0);
            float scale=GUI.matrix.m00;
            Vector2 p=new Vector2(t.position.x,Screen.height-t.position.y)/scale;
            if(t.phase!=TouchPhase.Moved || !area.Contains(p))return;
            if(horizontal)scroll.x-=t.deltaPosition.x/scale;
            else scroll.y+=t.deltaPosition.y/scale;
        }

        bool DrawRoster(float w,float h)
        {
            EnsureRosterStyles();
            if(!inventoryOpen)
            {
                if(!GUI.Button(RosterButton,"Meus servos",tile))return false;
                inventoryOpen=true;
                selectedCompanion=servants.Count>0?servants[0]:resting.Count>0?resting[0]:null;
                confirmRelease=false;rosterNotice="";touchMove=Vector2.zero;joystickFingerId=-1;
                Time.timeScale=0;
            }
            float width=Mathf.Min(w-16,490),height=Mathf.Min(h-18,720);
            GUI.BeginGroup(new Rect((w-width)/2,(h-height)/2,width,height));
            GUI.Box(new Rect(0,0,width,height),"",board);
            GUI.Label(new Rect(42,12,width-84,40),"Meus servos",title);
            if(GUI.Button(new Rect(width-51,9,40,40),"×",tile))
            {inventoryOpen=false;Time.timeScale=1;GUI.EndGroup();return true;}
            GUI.Label(new Rect(16,57,width-32,30),"Acompanhando · "+servants.Count,heading);
            float inner=width-32;
            Rect row=new Rect(16,90,inner,75);
            Rect grid=new Rect(16,211,inner,Mathf.Max(55,height-365));
            // Touch coordinates must be converted out of this centered group.
            Vector2 origin=new Vector2((w-width)/2,(h-height)/2);
            Rect screenRow=row;screenRow.position+=origin;
            Rect screenGrid=grid;screenGrid.position+=origin;
            ScrollByTouch(screenRow,ref partyScroll,true);
            float cell=Mathf.Min(60f,inner/6f);
            partyScroll=GUI.BeginScrollView(row,partyScroll,new Rect(0,0,
                Mathf.Max(inner,servants.Count*cell),cell),false,false);
            for(int i=0;i<servants.Count;i++)
                CompanionTile(servants[i],new Rect(i*cell,1,cell-3,cell-3));
            GUI.EndScrollView();
            GUI.Label(new Rect(16,172,inner,32),"Recolhidos · "+resting.Count,heading);
            float side=(inner-17)/4f;
            ScrollByTouch(screenGrid,ref restScroll,false);
            restScroll=GUI.BeginScrollView(grid,restScroll,new Rect(0,0,inner-17,
                Mathf.Max(grid.height,((resting.Count+3)/4)*side)));
            for(int i=0;i<resting.Count;i++)
            {
                float y=(i/4)*side;
                if(y+side<restScroll.y || y>restScroll.y+grid.height)continue;
                CompanionTile(resting[i],new Rect((i%4)*side,y,side-4,side-4));
            }
            GUI.EndScrollView();
            if(selectedCompanion!=null)
            {
                bool active=servants.Contains(selectedCompanion);
                string name=CompanionName(selectedCompanion);
                GUI.Label(new Rect(12,height-144,width-24,34),name+
                    (active?" · Acompanhando":" · Recolhido"),heading);
                GUI.Label(new Rect(12,height-111,width-24,28),confirmRelease?
                    "Libertar este servo definitivamente?":rosterNotice,caption);
                Rect first=new Rect(16,height-70,(width-42)/2,48);
                Rect second=first;second.x=first.xMax+10;
                if(confirmRelease)
                {
                    if(GUI.Button(first,"Cancelar",tile))confirmRelease=false;
                    if(GUI.Button(second,"Sim, libertar",button))
                    {
                        servants.Remove(selectedCompanion);resting.Remove(selectedCompanion);
                        Destroy(selectedCompanion.body.gameObject);
                        selectedCompanion=null;confirmRelease=false;SaveCompanions();
                    }
                }
                else
                {
                    if(GUI.Button(first,active?"Recolher":"Chamar",button))TransferSelected();
                    if(GUI.Button(second,"Libertar…",tile))confirmRelease=true;
                }
            }
            else GUI.Label(new Rect(12,height-95,width-24,50),
                servants.Count+resting.Count==0?"Erga uma criatura para começar.":"Selecione um servo.",caption);
            GUI.EndGroup();
            return true;
        }
    }
}

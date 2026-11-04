using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Portrait HUD, multi-touch controls and paused army-upgrade panel.
    /// GUI coordinates use a virtual 420px wide reference layout.
    /// </summary>
    public partial class NecroPrototype
    {
        GUIStyle titleStyle,labelStyle,smallStyle,buttonStyle;
        void EnsureStyles()
        {
            if(titleStyle!=null)return;
            titleStyle=new GUIStyle(GUI.skin.label) {
                fontSize=16,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor=new Color(.86f,.91f,.83f);
            labelStyle=new GUIStyle(GUI.skin.label) {
                fontSize=13,alignment=TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor=new Color(.92f,.87f,.88f);
            smallStyle=new GUIStyle(GUI.skin.label) {
                fontSize=11,alignment=TextAnchor.MiddleCenter,wordWrap=true
            };
            smallStyle.normal.textColor=new Color(.85f,.81f,.86f);
            buttonStyle=new GUIStyle(GUI.skin.button) {
                fontSize=13,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,
                wordWrap=true
            };
        }
        Vector2 joystickAnchor;
        Texture2D attackDisc,bindDisc,pearl;

        bool MovementArea(Vector2 point,float scale)
        {
            Vector2 gui=new Vector2(point.x,Screen.height-point.y)/scale;
            return !RosterButton.Contains(gui) && point.x<Screen.width*.65f &&
                !AttackRect(scale).Contains(gui) && !BindRect(scale).Contains(gui);
        }

        void ReadMovement(float scale)
        {
            bool active=false;
            foreach(Touch touch in Input.touches)
            {
                if(touch.fingerId!=joystickFingerId)
                {
                    if(joystickFingerId!=-1 || touch.phase!=TouchPhase.Began ||
                        !MovementArea(touch.position,scale))continue;
                    joystickFingerId=touch.fingerId;
                    joystickAnchor=touch.position;
                }
                if(touch.phase==TouchPhase.Ended || touch.phase==TouchPhase.Canceled)
                {
                    joystickFingerId=-1;
                    continue;
                }
                touchMove=Vector2.ClampMagnitude(
                    (touch.position-joystickAnchor)/(46f*scale),1f);
                active=true;
                break;
            }
            if(Input.touchCount==0)
            {
                if(Input.GetMouseButtonDown(0) && MovementArea(Input.mousePosition,scale))
                {
                    joystickFingerId=-2;
                    joystickAnchor=Input.mousePosition;
                }
                if(joystickFingerId==-2 && Input.GetMouseButton(0))
                {
                    touchMove=Vector2.ClampMagnitude(
                        ((Vector2)Input.mousePosition-joystickAnchor)/(46f*scale),1f);
                    active=true;
                }
            }
            if(!active){joystickFingerId=-1;touchMove=Vector2.zero;}
        }

        void DrawAction(Rect rect,bool binding,string caption,bool ready)
        {
            if(attackDisc==null)
            {
                attackDisc=Resources.Load<Texture2D>("HUD/attack");
                bindDisc=Resources.Load<Texture2D>("HUD/bind");
                pearl=Resources.Load<Texture2D>("HUD/pearl");
            }
            var art=binding?bindDisc:attackDisc;
            RingUi(new Rect(rect.x,rect.y+2f,rect.width,rect.height),art,new Color(0,0,0,.22f));
            RingUi(rect,art,ready?Color.white:new Color(.72f,.70f,.76f,.92f));
            if(!binding && caption!="ATACAR")
                GUI.Label(new Rect(rect.x-7f,rect.y-19f,rect.width+14f,18f),caption,hudAction);
        }

        Rect AttackRect(float scale)
        {
            return new Rect(Screen.width/scale-108f,Screen.height/scale-214f,82f,82f);
        }

        Rect BindRect(float scale)
        {
            Rect rect=AttackRect(scale);
            rect.x+=6f;
            rect.y+=96f;
            rect.width=70f;
            rect.height=70f;
            return rect;
        }

        void ReadManualAttack(float scale)
        {
            Rect button=AttackRect(scale);
            Rect bind=BindRect(scale);
            bool pressed=false;
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch=Input.GetTouch(i);
                Vector2 point=new Vector2(touch.position.x,Screen.height-touch.position.y)/scale;
                if(touch.phase!=TouchPhase.Began)continue;
                if(button.Contains(point))pressed=true;
                else if(bind.Contains(point))BindNearbyCreature();
            }
            if(Input.touchCount==0 && Input.GetMouseButtonDown(0))
            {
                Vector3 pos=Input.mousePosition;
                Vector2 point=new Vector2(pos.x,Screen.height-pos.y)/scale;
                pressed=button.Contains(point);
                if(bind.Contains(point))BindNearbyCreature();
            }
            if(pressed)manualQueued=true;
            if(!manualQueued)return;
            if(Closest(ManualRange)==null)
            {
                noTargetUntil=Time.time+.45f;
                manualQueued=false;
            }
            else if(Time.time>=castCooldown && !releasePending)
            {
                manualQueued=false;
                noTargetUntil=0;
                CastAtRange(ManualRange);
            }
        }

        void OnGUI()
        {
            if(player==null || current==null)return;
            EnsureHudArt();
            float scale=Mathf.Clamp(Screen.width/420f,.5f,2.5f);
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            if(DrawRoster(Screen.width/scale,Screen.height/scale))return;
            Rect button=AttackRect(scale);
            DrawAction(button,false,Time.time<noTargetUntil?"SEM ALVO":manualQueued?"NA FILA":"ATACAR",
                Time.time>=castCooldown);
            if(CanBindNearby())DrawAction(BindRect(scale),true,"ERGUER",true);
            if(joystickFingerId!=-1)
            {
                Vector2 center=new Vector2(joystickAnchor.x,Screen.height-joystickAnchor.y)/scale;
                Rect pad=new Rect(center.x-55f,center.y-55f,110f,110f);
                RingUi(pad,stickDisc,new Color(.92f,.87f,.82f,.22f));
                Vector2 stick=touchMove*38f;
                RingUi(new Rect(center.x-19f+stick.x,center.y-19f-stick.y,38f,38f),
                    pearl,new Color(1f,.98f,.94f,.92f));
            }
        }


        void DrawCryptPanel(float width,float height)
        {
            float top=Mathf.Max(10f,(height-660f)*.40f);
            GUI.Box(new Rect(10,top,width-20,650),"CRIPTA DO NECROMANTE");
            GUI.Label(new Rect(27,top+28,width-54,30),
                "FRAGMENTOS "+shards+"   |   NICHOS "+vault.Count+"/"+VaultCapacity(),
                smallStyle);
            GUI.Label(new Rect(28,top+63,width-56,26),
                "EXÉRCITO ATIVO  "+servants.Count+"   •   COMANDO "+CommandUsed()+"/"+CommandLimit,
                smallStyle);
            GUI.enabled=servants.Count>0 && vault.Count<VaultCapacity();
            if(GUI.Button(new Rect(42,top+93,width-84,45),
                "GUARDAR SERVO MAIS PRÓXIMO",buttonStyle))StoreServant();
            GUI.enabled=true;

            GUI.Label(new Rect(30,top+149,width-60,25),
                "SERVOS ARMAZENADOS",titleStyle);
            int first=vaultPage*3;
            if(vaultPage>0 && first>=vault.Count){vaultPage=0;first=0;}
            for(int n=0;n<3;n++)
            {
                int index=first+n;
                float y=top+182+n*48;
                if(index>=vault.Count) {
                    GUI.Box(new Rect(25,y,width-50,42),"- nicho vazio -");
                    continue;
                }
                NecroActorRecord actor=vault[index];
                GUI.Box(new Rect(25,y,width-50,42),"");
                GUI.Label(new Rect(33,y+4,width-172,32),
                    actor.kind.ToUpperInvariant()+"  "+
                    Mathf.CeilToInt(actor.hp)+"/"+Mathf.CeilToInt(actor.maxHp),
                    labelStyle);
                GUI.enabled=CommandUsed()+CommandCost(actor.kind)<=CommandLimit;
                if(GUI.Button(new Rect(width-133,y+3,105,36),
                    "RETIRAR",buttonStyle))WithdrawServant(index);
                GUI.enabled=true;
            }
            if(GUI.Button(new Rect(42,top+330,90,34),"< ANTERIOR",buttonStyle))
                vaultPage=Mathf.Max(0,vaultPage-1);
            GUI.Label(new Rect(144,top+329,width-288,35),
                (vaultPage+1)+"/"+Mathf.Max(1,Mathf.CeilToInt(vault.Count/3f)),smallStyle);
            if(GUI.Button(new Rect(width-132,top+330,90,34),"PRÓXIMO >",buttonStyle))
                vaultPage=Mathf.Min(Mathf.Max(0,Mathf.CeilToInt(vault.Count/3f)-1),vaultPage+1);

            GUI.Label(new Rect(32,top+373,width-64,29),"RITUAIS PERMANENTES",titleStyle);
            RitualButton(new Rect(26,top+407,width-52,43),
                "VITALIDADE  "+lifeRitual+"/5   CUSTO "+VitalityCost(),
                lifeRitual<5 && shards>=VitalityCost(),0);
            RitualButton(new Rect(26,top+453,width-52,43),
                "EXPANDIR NICHOS  "+vaultRitual+"/2   CUSTO "+VaultCost(),
                vaultRitual<2 && shards>=VaultCost(),1);
            RitualButton(new Rect(26,top+499,width-52,43),
                "FLUXO DE ÂNIMA  "+spiritRitual+"/3   CUSTO "+SpiritCost(),
                spiritRitual<3 && shards>=SpiritCost(),2);

            if(GUI.Button(new Rect(28,top+557,width*.43f,43),
                "SALVAR",buttonStyle))
            {
                if(SaveProgress())Announce("Progresso salvo no aparelho.");
                else Announce("Erro ao salvar: verifique o armazenamento.");
            }
            if(GUI.Button(new Rect(width*.53f,top+557,width*.40f,43),
                "VOLTAR",buttonStyle))CloseCrypt();
            GUI.Label(new Rect(30,top+606,width-60,28),
                Time.time<noticeUntil?notice:"Servos armazenados ficam protegidos.",
                smallStyle);
        }

        void RitualButton(Rect area,string label,bool available,int ritual)
        {
            GUI.enabled=available;
            if(GUI.Button(area,label,buttonStyle))
            {
                if(ritual==0)RitualVitality();
                else if(ritual==1)RitualVault();
                else RitualSpirit();
            }
            GUI.enabled=true;
        }
        void DrawUpgradePanel(float width,float height)
        {
            float top=Mathf.Max(18,(height-620)*.5f);
            GUI.Box(new Rect(13,top,width-26,620),"EVOLUÇÃO DOS SERVOS");
            GUI.Label(new Rect(30,top+34,width-60,30),
                "Fragmentos: "+shards+"    |    Cada grau: +24% vida, +18% dano",smallStyle);
            int row=0;
            foreach(string kind in upgradeKinds)
            {
                float y=top+78+row*64;
                int rank=Rank(kind),cost=2+2*rank;
                GUI.Label(new Rect(29,y,170,48),kind.ToUpperInvariant()+
                    "     Grau "+rank+"/3",labelStyle);
                GUI.enabled=rank<3 && shards>=cost;
                if(GUI.Button(new Rect(width-137,y,103,44),
                    rank==3?"MÁXIMO":"CUSTO "+cost,buttonStyle))
                    UpgradeSpecies(kind);
                GUI.enabled=true;
                row++;
            }
            if(GUI.Button(new Rect(55,top+548,width-110,54),
                "VOLTAR AO COMBATE",buttonStyle))
                upgradeOpen=false;
        }
    }
}

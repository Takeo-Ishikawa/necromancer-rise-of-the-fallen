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
        void OnGUI()
        {
            if(player==null || current==null)return;
            EnsureHudArt();
            float scale=Mathf.Clamp(Screen.width/420f,.5f,2.5f);
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            Rect pad=new Rect(14f,Screen.height/scale-241f,126f,126f);
            RingUi(pad,stickDisc,Color.white);
            Vector2 stick=touchMove*23f;
            RingUi(new Rect(pad.center.x-26f+stick.x,pad.center.y-26f-stick.y,52f,52f),smallRing,UiGold);
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

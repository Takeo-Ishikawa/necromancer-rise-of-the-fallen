using UnityEngine;
using UnityEngine.SceneManagement;

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
            EnsureStyles();
            float scale=Mathf.Clamp(Screen.width/420f,.50f,2.5f);
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w=Screen.width/scale,h=Screen.height/scale;

            if(gameOver)
            {
                GUI.Box(new Rect(20,h*.31f,w-40,235),"O FIM DA EXPEDIÇÃO");
                GUI.Label(new Rect(36,h*.31f+50,w-72,70),
                    "O necromante foi derrotado. O progresso desta expedição foi perdido.",
                    smallStyle);
                if(GUI.Button(new Rect(60,h*.31f+155,w-120,53),"RECOMEÇAR",buttonStyle))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if(upgradeOpen)
            {
                DrawUpgradePanel(w,h);
                return;
            }

            GUI.Box(new Rect(7,7,w-14,163),level.title);
            GUI.Label(new Rect(18,31,w-36,23),
                "VIDA "+Mathf.CeilToInt(player.hp)+"/"+Mathf.CeilToInt(player.maxHp)+
                "     ÂNIMA "+mana+"/"+maxMana,labelStyle);
            GUI.Label(new Rect(18,51,w-36,21),
                "NÍVEL "+playerLevel+"     XP "+experience+"/"+ExperienceThreshold()+
                "     FRAGMENTOS "+shards,labelStyle);
            GUI.Label(new Rect(18,72,w-36,21),
                "SERVOS "+servants.Count+"   COMANDO "+CommandUsed()+"/"+CommandLimit+
                "   ORDEM "+(huntOrder?"CAÇA":"DEFESA"),labelStyle);
            GUI.Label(new Rect(18,93,w-36,21),
                "FOCO T"+staffTier+"   MANTO T"+mantleTier+"   RECURSOS "+gathered,labelStyle);
            GUI.Label(new Rect(18,117,w-36,43),
                Time.time<noticeUntil?notice:
                current.claimed?"Região conquistada. Procure um portal.":level.objective,
                smallStyle);

            if(completed)
                GUI.Box(new Rect(w*.12f,175,w*.76f,62),
                    "O exército dominou todas as regiões!");

            // Navigation is intentionally local to each map: entering a passage
            // is explicit, so the player cannot teleport accidentally.
            if(stage>0 && NearEntrance())
                if(GUI.Button(new Rect(w*.19f,180,w*.62f,44),
                    "RETORNAR: "+levels[stage-1].title,buttonStyle))Retreat();
            if(current.claimed && stage+1<levels.Length && NearSeal())
                if(GUI.Button(new Rect(w*.18f,228,w*.64f,48),
                    "ENTRAR: "+levels[stage+1].title,buttonStyle))Advance();

            float cy=h-140;
            float joystickX=85, joystickY=cy+5;
            GUI.Box(new Rect(8,cy-61,166,176),"MOVIMENTO");
            Vector2 move=Vector2.zero;
            if(GUI.RepeatButton(new Rect(65,cy-31,44,45),"▲",buttonStyle))move.y++;
            if(GUI.RepeatButton(new Rect(65,cy+67,44,45),"▼",buttonStyle))move.y--;
            if(GUI.RepeatButton(new Rect(15,cy+19,44,45),"◀",buttonStyle))move.x--;
            if(GUI.RepeatButton(new Rect(115,cy+19,44,45),"▶",buttonStyle))move.x++;
            touchMove=move.normalized;

            float right=w-132;
            if(GUI.Button(new Rect(right,cy-84,122,48),spellUnlocked?"MAGIA":"GOLPE",buttonStyle))
                Cast();
            if(GUI.Button(new Rect(right,cy-30,122,48),"REVIVER",buttonStyle))
                Raise();
            if(GUI.Button(new Rect(right,cy+24,122,44),huntOrder?"CAÇA":"DEFESA",buttonStyle))
                huntOrder=!huntOrder;
            if(GUI.Button(new Rect(right,cy+73,122,38),"DISPENSAR",buttonStyle))
                Dismiss();
            if(GUI.Button(new Rect(173,cy+116,w-304,40),"EVOLUIR",buttonStyle))
                {touchMove=Vector2.zero;upgradeOpen=true;}
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

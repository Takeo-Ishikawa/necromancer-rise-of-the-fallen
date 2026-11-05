using UnityEngine;

namespace Necromancer
{
    // Separate from numbered combat stages to preserve existing save indices.
    public partial class NecroPrototype
    {
        bool inSanctuary;
        StageInstance sanctuary;
        LevelDefinition sanctuaryLevel;
        readonly Vector2 sanctuaryExit = new Vector2(8f,18.6f);

        void EnterSanctuary()
        {
            if(sanctuary==null)
            {
                TextAsset data=Resources.Load<TextAsset>("Data/sanctuary");
                sanctuaryLevel=JsonUtility.FromJson<LevelDefinition>(data.text);
                sanctuary=new StageInstance { root=new GameObject("Safe Chamber"), claimed=true };
                sanctuary.backdrop=Visual("Empty chamber background",
                    new Vector2(sanctuaryLevel.width/2,sanctuaryLevel.height/2),
                    NecroArt.Environment(sanctuaryLevel.background),-50,sanctuary.root.transform);
                FitGround(sanctuary.backdrop,sanctuaryLevel);
            }
            if(current!=null)current.root.SetActive(false);
            inSanctuary=true;
            current=sanctuary;
            level=sanctuaryLevel;
            current.root.SetActive(true);
            ResetTravelCombat();
            player.body.position=NecroLevels.At(level,level.startX,level.startY);
            PlaceTravelCompanions();
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            UpdateWorldCamera();
        }

        void ResetTravelCombat()
        {
            manualQueued=false;
            releasePending=false;
            castTarget=null;
            servantOrder=null;
            heroineCastPulse=0;
            foreach(Projectile orb in projectiles)
                if(orb.body!=null)Destroy(orb.body.gameObject);
            projectiles.Clear();
            foreach(Actor ally in servants)
            {
                ally.pupAttackTime=0;
                ally.catchingUp=false;
                if(ally.kind=="rotboar")SetBoarPose(ally,BoarPose.Idle,0);
                if(ally.kind=="grave_pup" && ally.view!=null)ally.view.SetPupAction(0,Vector2.zero);
            }
        }

        void PlaceTravelCompanions()
        {
            foreach(Actor ally in servants)ally.body.position=new Vector2(-1000,-1000);
            foreach(Actor ally in servants)
            {
                bool placed=false;
                for(int ring=1;ring<=16 && !placed;ring++)
                    for(int slot=0;slot<ring*12;slot++)
                    {
                        float angle=slot*Mathf.PI*2/(ring*12);
                        Vector2 at=(Vector2)player.body.position+
                            new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(ring*.75f);
                        if(Blocked(at,ally.radius) || ActorAt(ally,at,at)!=null)continue;
                        ally.body.position=at;
                        placed=true;
                        break;
                    }
                if(!placed)ally.body.position=player.body.position;
            }
        }

        bool SanctuaryBlocked(Vector2 p,float radius)
        {
            // Broad hall narrows to the painted natural doorway at the top.
            if(p.y-radius<1.4f || p.y+radius>21f)return true;
            float inset=p.y<4f?Mathf.Lerp(2.2f,1.4f,(p.y-1.4f)/2.6f):1.4f;
            if(p.y>17.2f)inset=Mathf.Lerp(1.4f,6.3f,Mathf.Clamp01((p.y-17.2f)/1.4f));
            return p.x-radius<inset || p.x+radius>level.width-inset;
        }

        bool SanctuaryTravelAvailable()
        {
            return !gameOver && (inSanctuary
                ? Vector2.Distance(player.body.position,sanctuaryExit)<1.8f
                : stage==0 && NearEntrance());
        }

        Rect SanctuaryTravelRect(float scale)
        {
            return new Rect(24,68,Screen.width/scale-48,44);
        }

        void DrawSanctuaryNavigation(float scale)
        {
            EnsureStyles();
            if(inSanctuary)GUI.Label(new Rect(125,16,Screen.width/scale-137,35),
                "REFÚGIO · ÁREA SEGURA",smallStyle);
            if(!SanctuaryTravelAvailable())
            {
                if(inSanctuary)GUI.Label(new Rect(24,60,Screen.width/scale-48,30),
                    "Siga ao norte para entrar na caverna.",smallStyle);
                return;
            }
            if(GUI.Button(SanctuaryTravelRect(scale),
                inSanctuary?"ENTRAR NA CAVERNA":"VOLTAR AO REFÚGIO",buttonStyle))
            {
                SaveCompanions();
                if(inSanctuary)LoadStage(0);
                else EnterSanctuary();
            }
        }

        void UpdateWorldCamera()
        {
            float hy=Mathf.Min(5.6f,level.height/2,level.width/(2*cam.aspect));
            cam.orthographicSize=hy;
            float hx=hy*cam.aspect;
            Vector3 p=player.body.position;
            cam.transform.position=new Vector3(Mathf.Clamp(p.x,hx,level.width-hx),
                Mathf.Clamp(p.y,hy,level.height-hy),-10);
        }
    }
}

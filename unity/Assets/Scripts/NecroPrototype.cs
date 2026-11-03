using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// World lifecycle, stage persistence and camera.
    /// The combat/progression and mobile interface live in separate partial files.
    /// </summary>
    public partial class NecroPrototype : MonoBehaviour
    {
        class Actor
        {
            public string kind;
            public Transform body;
            public float hp,maxHp,speed,attack,radius,reach,cooldown,flash;
            public bool ally;
        }
        class Corpse
        {
            public string kind;
            public Transform body;
            public float time;
        }
        class Projectile
        {
            public Transform body;
            public Vector2 direction;
            public float time;
        }
        class Drop
        {
            public Transform body;
            public int slot,tier;
        }
        class ResourceNode
        {
            public Transform body;
            public string type;
        }
        class StageInstance
        {
            public GameObject root;
            public SpriteRenderer backdrop;
            public Transform seal;
            public Transform cryptAltar;
            public readonly List<Actor> enemies = new List<Actor>();
            public readonly List<Corpse> corpses = new List<Corpse>();
            public readonly List<Drop> drops = new List<Drop>();
            public readonly List<ResourceNode> resources = new List<ResourceNode>();
            public bool claimed, bossDefeated, bossBound;
        }

        const int CommandLimit=4;
        LevelDefinition[] levels;
        LevelDefinition level;
        StageInstance[] stages;
        StageInstance current;
        readonly List<Actor> servants = new List<Actor>();
        readonly List<Projectile> projectiles = new List<Projectile>();
        GameObject persistentRoot;
        Actor player;
        Camera cam;
        int stage, mana=55,maxMana=55, shards,kills,playerLevel=1,experience;
        int staffTier,mantleTier, gathered;
        readonly Dictionary<string,int> speciesRanks=new Dictionary<string,int>();
        readonly string[] upgradeKinds={"rat","slime","bat","spider","snake","skeleton","wisp"};
        float manaFraction,noticeUntil,hitFlash;
        bool spellUnlocked,huntOrder,upgradeOpen,gameOver,completed;
        string notice="Derrote uma criatura e ressuscite seu corpo.";
        Vector2 touchMove, facing=Vector2.right;

        void Start()
        {
            levels=NecroLevels.Load();
            if(levels.Length==0){Debug.LogError("No levels configured.");enabled=false;return;}
            stages=new StageInstance[levels.Length];
            Application.targetFrameRate=60;
            cam=Camera.main;
            if(cam==null)
            {
                GameObject camera=new GameObject("Main Camera");
                camera.tag="MainCamera";
                cam=camera.AddComponent<Camera>();
            }
            cam.orthographic=true;
            cam.orthographicSize=5.6f;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.10f,.09f,.14f);
            persistentRoot=new GameObject("Persistent Army and Necromancer");
            stage=0;level=levels[0];
            Vector2 start=NecroLevels.At(level,level.startX,level.startY);
            player=Create("necromante",start,true,persistentRoot.transform);
            player.maxHp=115;player.hp=115;player.speed=3.4f;
            LoadStage(0);
            RestoreProgress();
        }

        SpriteRenderer Visual(string label,Vector2 at,Sprite sprite,int order,Transform parent)
        {
            GameObject obj=new GameObject(label);
            obj.transform.SetParent(parent,false);
            obj.transform.position=new Vector3(at.x,at.y,0);
            SpriteRenderer renderer=obj.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;
            renderer.sortingOrder=order;
            return renderer;
        }

        static void Fit(SpriteRenderer renderer,float width,float height)
        {
            Vector2 size=renderer.sprite.bounds.size;
            renderer.transform.localScale=new Vector3(width/size.x,height/size.y,1);
        }

        Actor Create(string kind,Vector2 at,bool ally,Transform parent)
        {
            float diameter=kind=="warden"||kind=="grave_lord"||kind=="marsh_lord"?1.24f:
                kind=="necromante"?1.28f:kind=="slime"?.76f:kind=="wisp"?.70f:.72f;
            var sprite=Visual((ally?"Servo ":"Hostil ")+kind,at,NecroArt.Get(kind),ally?15:12,parent);
            Fit(sprite,diameter,diameter);
            Actor a=new Actor{
                kind=kind,ally=ally,body=sprite.transform,radius=diameter*.30f,
                maxHp=kind=="marsh_lord"?460:kind=="grave_lord"?320:
                    kind=="warden"?230:kind=="skeleton"?75:kind=="wisp"?58:
                    kind=="slime"?54:kind=="necromante"?115:34,
                attack=kind=="marsh_lord"?27:kind=="grave_lord"?22:
                    kind=="warden"?15:kind=="skeleton"?12:kind=="wisp"?9:
                    kind=="slime"?10:kind=="necromante"?26:8,
                speed=kind=="marsh_lord"?1.3f:kind=="grave_lord"?1.45f:
                    kind=="warden"?1.4f:kind=="bat"||kind=="wisp"?2.7f:
                    kind=="slime"?1.4f:1.9f,
                reach=kind=="bat"||kind=="wisp"?1.10f:.66f
            };
            a.hp=a.maxHp;
            if(ally && kind!="necromante")ApplySpeciesRank(a);
            return a;
        }

        void LoadStage(int index)
        {
            if(index<0 || index>=levels.Length)return;
            if(current!=null)current.root.SetActive(false);
            stage=index;
            level=levels[stage];
            if(stages[stage]==null) {
                StageInstance next=new StageInstance();
                next.root=new GameObject("Region "+(stage+1)+": "+level.title);
                stages[stage]=next;
                current=next;
                Sprite background=NecroArt.Environment(level.background);
                next.backdrop=Visual("Ground",new Vector2(level.width*.5f,level.height*.5f),
                    background,-50,next.root.transform);
                Fit(next.backdrop,level.width,level.height);
                foreach(Obstacle rock in level.obstacles) {
                    SpriteRenderer stone=Visual("Rock",NecroLevels.At(level,rock.x,rock.y),
                        NecroArt.Stone(),-3,next.root.transform);
                    Fit(stone,rock.r*2f,rock.r*2f);
                }
                next.seal=Visual("Necromantic Gate",NecroLevels.At(level,level.sealX,level.sealY),
                    NecroArt.Effect(),-1,next.root.transform).transform;
                next.seal.localScale=Vector3.one*2.2f;
                foreach(Spawn spawn in level.enemies)
                    next.enemies.Add(Create(spawn.kind,NecroLevels.At(level,spawn.x,spawn.y),
                        false,next.root.transform));
                if(level.resources!=null)foreach(ResourceSpawn res in level.resources)
                    SpawnResource(res.type,NecroLevels.At(level,res.x,res.y));
            } else {
                current=stages[stage];
                current.root.SetActive(true);
            }
            // All servants survive transitions with their HP and level intact.
            Vector2 start=NecroLevels.At(level,level.startX,level.startY);
            player.body.position=start;
            for(int i=0;i<servants.Count;i++) {
                float angle=i*2.399963f;
                servants[i].body.position=start+
                    new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.6f;
            }
            foreach(Projectile orb in projectiles)
                if(orb.body!=null)Destroy(orb.body.gameObject);
            projectiles.Clear();
            upgradeOpen=false;
            cryptOpen=false;
            Announce(level.title+": explore e domine as criaturas.");
        }

        void Advance()
        {
            if(!current.claimed || stage+1>=levels.Length)return;
            LoadStage(stage+1);
            SaveProgress();
        }

        void Retreat()
        {
            if(stage==0)return;
            LoadStage(stage-1);
            SaveProgress();
        }

        void Update()
        {
            if(player==null || level==null || gameOver)return;
            float dt=Mathf.Min(.045f,Time.deltaTime);
            if(!upgradeOpen && !cryptOpen)
            {
                Vector2 keyboard=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
                Vector2 control=keyboard+touchMove;
                foreach(Touch touch in Input.touches)
                {
                    // Left-lower quadrant is a virtual analog joystick.
                    if(touch.position.x<Screen.width*.43f && touch.position.y<Screen.height*.42f)
                        control+=(touch.position-new Vector2(Screen.width*.18f,Screen.height*.18f))/
                            Mathf.Max(75,Screen.width*.17f);
                }
                if(control.sqrMagnitude>1)control.Normalize();
                if(control.sqrMagnitude>.01f)
                {
                    facing=control.normalized;
                    Move(player,control*player.speed*dt);
                }
                Regenerate(dt);
                if(Input.GetKeyDown(KeyCode.Space))Cast();
                if(Input.GetKeyDown(KeyCode.E))Raise();
                if(Input.GetKeyDown(KeyCode.Q))huntOrder=!huntOrder;
                if(Input.GetKeyDown(KeyCode.Tab))upgradeOpen=true;
                if(Input.GetKeyDown(KeyCode.C))OpenCrypt();
                if(Input.GetKeyDown(KeyCode.Return))TryPortal();
                if(player.hp>0)
                {
                    UpdateActors(current.enemies,dt,false);
                    UpdateActors(servants,dt,true);
                    UpdateProjectiles(dt);
                }
                TickCorpses(dt);
                CollectNearby();
                if(!current.claimed && current.enemies.Count==0 &&
                    (!level.requiresGuardian || (current.bossDefeated && current.bossBound)))
                    Claim();
                if(current.seal!=null)
                    current.seal.Rotate(0,0,dt*(current.claimed?18:5));
            }
            Autosave(dt);
            Vector3 position=player.body.position;
            float hy=cam.orthographicSize,hx=hy*cam.aspect;
            cam.transform.position=new Vector3(
                Mathf.Clamp(position.x,hx,Mathf.Max(hx,level.width-hx)),
                Mathf.Clamp(position.y,hy,Mathf.Max(hy,level.height-hy)),-10);
        }

        bool NearSeal()
        {
            return current.seal!=null &&
                Vector2.Distance(player.body.position,current.seal.position)<2.2f;
        }

        bool NearEntrance()
        {
            Vector2 start=NecroLevels.At(level,level.startX,level.startY);
            return Vector2.Distance(player.body.position,start)<2.1f;
        }

        void TryPortal()
        {
            if(stage>0 && NearEntrance()){Retreat();return;}
            if(current.claimed && stage+1<levels.Length && NearSeal())Advance();
        }

        void Claim()
        {
            current.claimed=true;
            if(!string.IsNullOrEmpty(level.baseBackground))
            {
                current.backdrop.sprite=NecroArt.Environment(level.baseBackground);
                Fit(current.backdrop,level.width,level.height);
            }
            current.seal.localScale=Vector3.one*3.2f;
            if(stage==0)ShowCryptAltar();
            if(stage==levels.Length-1) {
                completed=true;
                Announce("As regioes foram conquistadas. Seu exercito prevaleceu!");
            } else {
                Announce("Regiao conquistada! Aproxime-se do selo central para prosseguir.");
            }
            SaveProgress();
        }

        void Announce(string message)
        {
            notice=message;
            noticeUntil=Time.time+4f;
        }

        void Regenerate(float dt)
        {
            manaFraction+=dt*(3.5f+(playerLevel-1)*.4f+spiritRitual*.65f);
            if(manaFraction>=1 && mana<maxMana)
            {
                int regenerated=(int)manaFraction;
                manaFraction-=regenerated;
                mana=Mathf.Min(maxMana,mana+regenerated);
            }
        }
    }
}

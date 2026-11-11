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
            public BoarPose boarPose;
            public Vector2 chargeDirection;
            public SpriteRenderer boarArt;
            public Vector3 boarVisualScale;
            public Vector2 boarLastPosition;
            public int boarFacingIndex;
            public float boarFrameClock;
            public float bindUntil;
            public float pupAttackTime;
            public Vector2 pupHeading;
            public bool pupHit,catchingUp;
            public float followStragglerSeconds;
            public bool ally;
            // Transient hostile AI focus: never serialized into the v1 save.
            public Actor hostileFocus;
            // Unsaved combat memory: foes return to their creation point
            // once the party retreats or breaks visual contact.
            public Vector2 lairPosition;
            public float targetMemoryUntil;
            public float reanimationUntil;
            public NecroCreatureView view;
        }
        class Corpse
        {
            public string kind;
            public Transform body;
            public float time;
            public NecroCreatureView view;
        }
        class Projectile
        {
            public Transform body;
            public Vector2 direction;
            public float time;
            public float age;
            public SpriteRenderer renderer;
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
            // Cached visual state; remains transient and absent from saves.
            public SpriteRenderer artwork;
            public Vector3 defaultScale;
            public Color shimmerTint;
            public float shimmerOffset;
        }
        class StageInstance
        {
            public GameObject root;
            public SpriteRenderer backdrop;
            public Transform seal;
            public Transform cryptAltar;
            public readonly List<Actor> enemies = new List<Actor>();
            public readonly List<NecroCreatureView> fallenPups=new List<NecroCreatureView>();
            public readonly List<Actor> fallenBoars=new List<Actor>();
            public readonly List<Corpse> corpses = new List<Corpse>();
            public readonly List<Drop> drops = new List<Drop>();
            public readonly List<ResourceNode> resources = new List<ResourceNode>();
            public bool claimed, bossDefeated, bossBound;
            public float respawnTimer=RespawnInterval;
            public int respawnIndex;
        }

        const int CommandLimit=12;
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
        int joystickFingerId=-1;

        void Start()
        {
            InitializeCharacterSelection();
        }

        void BeginSelectedCharacterWorld()
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
            InitializeHeroAnimation();
            EnterSanctuary();
            spellUnlocked=true;
            RestoreSanctuaryProfile();
            LoadCompanions();
            PlaceTravelCompanions();
            // Fifteen-second scheduler: saving never runs on every Update frame.
            // The Unity scheduler pauses naturally with the roster time scale.
            InvokeRepeating(nameof(PeriodicAutosave),15f,15f);
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

        static void FitGround(SpriteRenderer renderer,LevelDefinition area)
        {
            if(area.background=="entrada_da_caverna_bonehollow")
            {
                // Tile the imported floor at its native pixel density.
                // FullRect sprites allow SpriteRenderer tiled draw mode.
                renderer.drawMode=SpriteDrawMode.Tiled;
                renderer.transform.localScale=Vector3.one;
                renderer.size=new Vector2(area.width,area.height);
            }
            else
            {
                renderer.drawMode=SpriteDrawMode.Simple;
                Fit(renderer,area.width,area.height);
            }
        }

        Actor Create(string kind,Vector2 at,bool ally,Transform parent)
        {
            float diameter=kind=="warden"||kind=="grave_lord"||kind=="marsh_lord"?1.24f:
                kind=="grave_pup"?1.08f:kind=="necromante"?1.28f:kind=="slime"?.76f:kind=="wisp"?.70f:.72f;
            var sprite=Visual((ally?"Servo ":"Hostil ")+kind,at,kind=="grave_pup"?NecroCreatureView.PupIdle(ally):NecroArt.Get(kind),ally?15:12,parent);
            Fit(sprite,diameter,diameter);
            Actor a=new Actor{
                kind=kind,ally=ally,body=sprite.transform,radius=diameter*.30f,
                lairPosition=at,
                maxHp=kind=="marsh_lord"?460:kind=="grave_lord"?320:
                    kind=="warden"?230:kind=="skeleton"?75:kind=="wisp"?58:
                    kind=="grave_pup"?82:kind=="slime"?54:kind=="necromante"?115:34,
                attack=kind=="marsh_lord"?27:kind=="grave_lord"?22:
                    kind=="warden"?15:kind=="skeleton"?12:kind=="wisp"?9:
                    kind=="slime"?10:kind=="necromante"?26:8,
                speed=kind=="marsh_lord"?1.3f:kind=="grave_lord"?1.45f:
                    kind=="warden"?1.4f:kind=="bat"||kind=="wisp"?2.7f:
                    kind=="grave_pup"?(ally?3.8f:2.35f):kind=="slime"?1.4f:1.9f,
                reach=kind=="bat"||kind=="wisp"?1.10f:.66f
            };
            a.hp=a.maxHp;
            if(ally && kind!="necromante")ApplySpeciesRank(a);
            if(kind!="necromante")
                a.view=NecroCreatureView.Attach(sprite,kind,ally);
            return a;
        }

        // Existing saves from <= 0.9.14 never recorded placed resources.
        // The optional marker disambiguates old empty snapshots from new
        // stages whose pickups have genuinely all been collected.
        void PopulateStageResources(NecroStageRecord snapshot)
        {
            if(snapshot!=null && snapshot.visited && snapshot.resourcesInitialized)
            {
                if(snapshot.resources==null)return;
                foreach(NecroResourceRecord saved in snapshot.resources)
                    if(saved!=null && !string.IsNullOrEmpty(saved.type))
                        SpawnResource(saved.type,new Vector2(saved.x,saved.y));
                return;
            }
            if(level.resources==null)return;
            foreach(ResourceSpawn resource in level.resources)
                if(resource!=null && !string.IsNullOrEmpty(resource.type))
                    SpawnResource(resource.type,
                        NecroLevels.At(level,resource.x,resource.y));
        }

        void LoadStage(int index)
        {
            if(index<0 || index>=levels.Length)return;
            if(current!=null)current.root.SetActive(false);
            if(inSanctuary)RestoreCombatActorDepth();
            inSanctuary=false;
            ResetTravelCombat();
            stage=index;
            level=levels[stage];
            if(stages[stage]==null) {
                StageInstance next=new StageInstance();
                next.respawnTimer=RespawnIntervalForStage();
                next.root=new GameObject("Region "+(stage+1)+": "+level.title);
                stages[stage]=next;
                current=next;
                Sprite background=NecroArt.Environment(level.background);
                next.backdrop=Visual("Ground",new Vector2(level.width*.5f,level.height*.5f),
                    background,-50,next.root.transform);
                FitGround(next.backdrop,level);
                // A visible marker is required for NearSeal and progression.
                Vector2 sealAt=NecroLevels.At(level,level.sealX,level.sealY);
                SpriteRenderer sealArt=Visual("Selo de passagem",sealAt,
                    NecroArt.RitualCircle(),5,next.root.transform);
                Fit(sealArt,1.85f,1.30f);
                sealArt.color=new Color(.56f,.56f,.92f,.64f);
                next.seal=sealArt.transform;
                // The painted cave already includes its walls and scenery.
                if(level.background!="forgotten_cave")
                {
                    if(level.id=="bonehollow")
                        NecroCaveZones.Populate(level,next.root.transform);
                    NecroScenery.Populate(level,next.root.transform);
                    foreach(Obstacle rock in level.obstacles) {
                        SpriteRenderer stone=Visual("Rock",NecroLevels.At(level,rock.x,rock.y),
                            NecroArt.Stone(),-3,next.root.transform);
                        Fit(stone,rock.r*2f,rock.r*2f);
                    }
                }
                NecroStageRecord lastSnapshot=
                    previousStageSnapshots!=null &&
                    stage<previousStageSnapshots.Length
                    ? previousStageSnapshots[stage] : null;
                if(lastSnapshot!=null && lastSnapshot.visited)
                {
                    next.claimed=lastSnapshot.claimed;
                    next.bossDefeated=lastSnapshot.bossDefeated;
                    next.bossBound=lastSnapshot.bossBound;
                    if(next.claimed)ActivateRegionSeal();
                }
                if(level.id=="bonehollow")
                    next.enemies.Add(CreateRotboar(NecroLevels.At(level,
                        level.rotboarX>0f?level.rotboarX:12.8f,
                        level.rotboarY>0f?level.rotboarY:6.3f),
                        next.root.transform));
                foreach(Spawn spawn in level.enemies)
                {
                    if(IsBoss(spawn.kind) &&
                        !NecroRegionRules.NeedsGuardian(level.requiresGuardian,
                            next.claimed,next.bossDefeated))continue;
                    next.enemies.Add(Create(spawn.kind,
                        NecroLevels.At(level,spawn.x,spawn.y),
                        false,next.root.transform));
                }
                PopulateStageResources(lastSnapshot);
                if(stage==0 && next.claimed)ShowCryptAltar();
            } else {
                current=stages[stage];
                current.root.SetActive(true);
            }
            // All servants survive transitions with their HP and level intact.
            Vector2 start=NecroLevels.At(level,level.startX,level.startY);
            player.body.position=start;
            miniMapExpanded=false;
            EnsureMiniMapRegion();
            RevealMiniMapAtPlayer();
            // A short arrival grace period prevents portal ambushes.
            heroDamageImmuneUntil=Time.time+HeroArrivalGraceSeconds;
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
            PlaceTravelCompanions();
            UpdateWorldCamera();

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
            if(characterSelectionOpen)
            {
                UpdateCharacterSelection();
                return;
            }
            if(player==null || level==null || gameOver || inventoryOpen)return;
            if(wardrobeOpen)
            {
                if(Input.GetKeyDown(KeyCode.Escape))CloseSanctuaryWardrobe();
                return;
            }
            if(meditationOpen)
            {
                if(Input.GetKeyDown(KeyCode.Escape))CloseSanctuaryMeditation();
                return;
            }
            if(Input.GetKeyDown(KeyCode.M))miniMapExpanded=!miniMapExpanded;
            if(miniMapExpanded)
            {
                if(Input.GetKeyDown(KeyCode.Escape))miniMapExpanded=false;
                touchMove=Vector2.zero;
                joystickFingerId=-1;
                return;
            }
            float dt=Mathf.Min(.045f,Time.deltaTime);
            Vector2 keyboard=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
            float hudScale=Mathf.Clamp(Screen.width/420f,.5f,2.5f);
            ReadMovement(hudScale);
            Vector2 control=keyboard+touchMove;
            if(control.sqrMagnitude>1)control.Normalize();
            heroineWalking=control.sqrMagnitude>.01f;
            if(heroineWalking)
            {
                facing=control.normalized;
                Move(player,control*player.speed*(1f+agilityPoints*.06f)*FountainSpeedMultiplier*dt);
            }
            RevealMiniMapAtPlayer();
            if(inSanctuary)ReadSanctuaryInteractionKey();
            else
            {
                CollectNearby();
                UpdateResourceExploration();
            }
            Regenerate(dt);
            if(!inSanctuary)
            {
                ReadActiveSkillInput(hudScale);
                ReadManualAttack(hudScale);
            }
            if(!inSanctuary)UpdateActors(current.enemies,dt,false);
            if(gameOver)return;
            UpdateRegionProgress();
            UpdateActors(servants,dt,true);
            if(!inSanctuary)TickCorpses(dt);
            if(!inSanctuary)UpdateProjectiles(dt);
            if(!inSanctuary)TickRespawns(dt);
            AnimateHeroVisual(dt);
            UpdateSanctuaryActorDepth();
            UpdateSanctuaryInteractionVisual();
            UpdateWorldCamera();
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
            if(current==null || current.claimed || inSanctuary)return;
            current.claimed=true;
            if(!string.IsNullOrEmpty(level.baseBackground))
            {
                current.backdrop.sprite=NecroArt.Environment(level.baseBackground);
                FitGround(current.backdrop,level);
            }
            ActivateRegionSeal();
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
            // Never accumulate a hidden regeneration bank at full anima.
            if(mana>=maxMana){manaFraction=0f;return;}
            manaFraction+=dt*(3.5f+(playerLevel-1)*.4f+spiritRitual*.65f+(SkillUnlocked(2)?1.5f:0f));
            if(manaFraction>=1 && mana<maxMana)
            {
                int regenerated=(int)manaFraction;
                manaFraction-=regenerated;
                mana=Mathf.Min(maxMana,mana+regenerated);
            }
        }
    }
}

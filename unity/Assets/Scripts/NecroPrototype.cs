using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Protótipo de jogo Unity 2D: exploracao, servos automaticos,
    /// magia ativa, reanimacao e conquista da primeira base.
    /// </summary>
    public class NecroPrototype : MonoBehaviour
    {
        class Actor
        {
            public string kind;
            public Transform body;
            public float hp,maxHp,speed,attack,radius,reach,cooldown;
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

        LevelDefinition[] levels;
        LevelDefinition level;
        readonly List<Transform> stageObjects=new List<Transform>();
        readonly List<Actor> enemies=new List<Actor>(),servants=new List<Actor>();
        readonly List<Corpse> corpses=new List<Corpse>();
        readonly List<Projectile> projectiles=new List<Projectile>();
        Actor player;
        SpriteRenderer backdrop;
        Transform seal;
        Camera cam;
        int stage,commandLimit=4,mana=55,maxMana=55,souls,kills,playerLevel=1;
        float manaFraction,noticeUntil;
        bool spellUnlocked,guardianDefeated,claimed,huntOrder;
        string notice="Derrote uma criatura e reanime seu corpo.";
        Vector2 touchMove;

        void Start()
        {
            levels=NecroLevels.Load();
            if(levels.Length==0){enabled=false;return;}
            Application.targetFrameRate=60;
            cam=Camera.main;
            if(cam==null)
            {
                GameObject root=new GameObject("Main Camera");
                root.tag="MainCamera";
                cam=root.AddComponent<Camera>();
            }
            cam.orthographic=true;cam.orthographicSize=5.6f;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.10f,.09f,.14f);
            LoadStage(0,false);
        }

        void LoadStage(int index,bool keepMinions)
        {
            List<string> army=new List<string>();
            if(keepMinions)foreach(Actor minion in servants)army.Add(minion.kind);
            foreach(Transform t in stageObjects)if(t!=null)Destroy(t.gameObject);
            stageObjects.Clear();enemies.Clear();servants.Clear();corpses.Clear();projectiles.Clear();
            stage=index;level=levels[index];guardianDefeated=false;claimed=false;
            Vector2 at=NecroLevels.At(level,level.startX,level.startY);
            backdrop=Visual("Cenario - "+level.id,
                new Vector2(level.width*.5f,level.height*.5f),
                NecroArt.Environment(level.background),-50);
            Fit(backdrop,level.width,level.height);
            foreach(Obstacle rock in level.obstacles)
            {
                SpriteRenderer visual=Visual("Rochedo",NecroLevels.At(level,rock.x,rock.y),
                    NecroArt.Stone(),-3);
                Fit(visual,rock.r*2,rock.r*2);
            }
            seal=Visual("Nucleo Necromantico",NecroLevels.At(level,level.sealX,level.sealY),
                NecroArt.Effect(),-1).transform;
            seal.localScale=Vector3.one*1.5f;
            player=Create("necromante",at,true);
            player.maxHp=115+12*(playerLevel-1);player.hp=player.maxHp;
            player.speed=3.4f;
            foreach(Spawn spawn in level.enemies)
                enemies.Add(Create(spawn.kind,NecroLevels.At(level,spawn.x,spawn.y),false));
            foreach(string kind in army)
            {
                Actor minion=Create(kind,at+Random.insideUnitCircle*.55f,true);
                minion.attack*=1.4f;servants.Add(minion);
            }
            Announce(index==0?"A Caverna Esquecida: purifique o local.":"Tuneis inferiores: novas criaturas.");
        }

        SpriteRenderer Visual(string label,Vector2 at,Sprite sprite,int order)
        {
            GameObject obj=new GameObject(label);
            obj.transform.position=new Vector3(at.x,at.y,0);
            SpriteRenderer render=obj.AddComponent<SpriteRenderer>();
            render.sprite=sprite;render.sortingOrder=order;
            stageObjects.Add(obj.transform);return render;
        }
        static void Fit(SpriteRenderer renderer,float w,float h)
        {
            Vector2 sz=renderer.sprite.bounds.size;
            renderer.transform.localScale=new Vector3(w/sz.x,h/sz.y,1);
        }
        Actor Create(string kind,Vector2 at,bool ally)
        {
            float diameter=kind=="warden"?1.20f:kind=="necromante"?.95f:
                kind=="slime"?.68f:kind=="bat"?.72f:.65f;
            SpriteRenderer r=Visual((ally?"Aliado ":"Inimigo ")+kind,at,NecroArt.Get(kind),ally?15:12);
            Fit(r,diameter,diameter);
            Actor a=new Actor{kind=kind,ally=ally,body=r.transform,radius=diameter*.32f,
                maxHp=kind=="warden"?230:kind=="slime"?54:kind=="necromante"?115:34,
                attack=kind=="warden"?15:kind=="slime"?10:kind=="necromante"?26:8,
                speed=kind=="bat"?2.6f:kind=="warden"?1.4f:kind=="slime"?1.4f:1.9f,
                reach=kind=="bat"?1.05f:.65f};
            a.hp=a.maxHp;return a;
        }

        void Update()
        {
            if(player==null || level==null)return;
            float dt=Mathf.Min(.045f,Time.deltaTime);
            Vector2 control=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"))+touchMove;
            foreach(Touch touch in Input.touches)
            {
                if(touch.position.x<Screen.width*.43f && touch.position.y<Screen.height*.42f)
                    control+=(touch.position-new Vector2(Screen.width*.18f,Screen.height*.18f))
                              /Mathf.Max(75,Screen.width*.17f);
            }
            if(control.sqrMagnitude>1)control.Normalize();
            if(player.hp>0)Move(player,control*player.speed*dt);
            manaFraction+=dt*3.5f;
            if(manaFraction>=1 && mana<maxMana)
            {
                int regenerated=(int)manaFraction;manaFraction-=regenerated;
                mana=Mathf.Min(maxMana,mana+regenerated);
            }
            if(Input.GetKeyDown(KeyCode.Space))Cast();
            if(Input.GetKeyDown(KeyCode.E))Raise();
            if(Input.GetKeyDown(KeyCode.Q))huntOrder=!huntOrder;
            if(Input.GetKeyDown(KeyCode.Return) && claimed && stage+1<levels.Length)LoadStage(stage+1,true);
            if(player.hp>0)
            {
                UpdateActors(enemies,dt,false);
                UpdateActors(servants,dt,true);
                UpdateProjectiles(dt);
            }
            for(int i=corpses.Count-1;i>=0;i--)
            {
                corpses[i].time-=dt;
                if(corpses[i].time<=0)
                {
                    Destroy(corpses[i].body.gameObject);
                    corpses.RemoveAt(i);
                }
            }
            if(!claimed && enemies.Count==0 && (!level.requiresGuardian || guardianDefeated))
                Claim();
            if(seal!=null)seal.Rotate(0,0,dt*(claimed?18:5));
            Vector3 at=player.body.position;
            float hy=cam.orthographicSize,hx=hy*cam.aspect;
            cam.transform.position=new Vector3(
                Mathf.Clamp(at.x,hx,Mathf.Max(hx,level.width-hx)),
                Mathf.Clamp(at.y,hy,Mathf.Max(hy,level.height-hy)),-10);
        }

        void UpdateActors(List<Actor> actors,float dt,bool friendly)
        {
            for(int i=actors.Count-1;i>=0;i--)
            {
                Actor self=actors[i];
                if(self.hp<=0)continue;
                self.cooldown=Mathf.Max(0,self.cooldown-dt);
                Actor target=null;
                float best=friendly?(huntOrder?4.7f:3.1f):3.9f;
                if(friendly)
                {
                    foreach(Actor enemy in enemies)
                    {
                        float d=Vector2.Distance(self.body.position,enemy.body.position);
                        if(d<best){best=d;target=enemy;}
                    }
                    if(Vector2.Distance(self.body.position,player.body.position)>5)target=null;
                } else {
                    float d=Vector2.Distance(self.body.position,player.body.position);
                    if(d<best){best=d;target=player;}
                    foreach(Actor ally in servants)
                    {
                        d=Vector2.Distance(self.body.position,ally.body.position);
                        if(d<best){best=d;target=ally;}
                    }
                }
                if(target!=null)
                {
                    float distance=Vector2.Distance(self.body.position,target.body.position);
                    if(distance>self.reach+target.radius)
                        Move(self,((Vector2)(target.body.position-self.body.position)).normalized*self.speed*dt);
                    else if(self.cooldown<=0)
                    {
                        self.cooldown=self.kind=="warden"?1.5f:1.15f;
                        Damage(target,self.attack);
                    }
                }
                else if(friendly)
                {
                    float angle=i*2.399963f;
                    Vector2 desired=(Vector2)player.body.position+new Vector2(
                        Mathf.Cos(angle),Mathf.Sin(angle))*.9f;
                    Vector2 delta=desired-(Vector2)self.body.position;
                    if(delta.magnitude>.25f)Move(self,delta.normalized*self.speed*dt);
                }
            }
        }
        bool Blocked(Vector2 p,float radius)
        {
            if(p.x<radius || p.y<radius || p.x>level.width-radius || p.y>level.height-radius)
                return true;
            foreach(Obstacle rock in level.obstacles)
                if(Vector2.Distance(p,NecroLevels.At(level,rock.x,rock.y))<radius+rock.r)
                    return true;
            return false;
        }
        void Move(Actor actor,Vector2 displacement)
        {
            Vector2 at=actor.body.position;
            Vector2 next=at+new Vector2(displacement.x,0);
            if(!Blocked(next,actor.radius))at=next;
            next=at+new Vector2(0,displacement.y);
            if(!Blocked(next,actor.radius))at=next;
            actor.body.position=new Vector3(at.x,at.y,0);
        }
        void Damage(Actor victim,float amount)
        {
            if(victim.hp<=0)return;
            victim.hp-=amount;
            if(victim.hp>0)return;
            if(victim==player){Announce("O ritual falhou. Reinicie a expedicao.");return;}
            if(victim.ally)servants.Remove(victim);
            else
            {
                enemies.Remove(victim);kills++;souls++;
                if(victim.kind=="warden")guardianDefeated=true;
                if(!spellUnlocked){spellUnlocked=true;Announce("Sua primeira magia foi despertada!");}
                SpriteRenderer marker=Visual("Corpo: "+victim.kind,victim.body.position,
                    NecroArt.Effect(),9);
                Fit(marker,.42f,.42f);
                marker.color=new Color(.78f,.70f,.91f,.7f);
                corpses.Add(new Corpse{kind=victim.kind,body=marker.transform,
                    time=victim.kind=="warden"?100:38});
                if(kills%5==0){playerLevel++;maxMana+=7;Announce("Nivel "+playerLevel+" alcancado.");}
            }
            Destroy(victim.body.gameObject);
        }
        Actor Closest(float max)
        {
            Actor found=null;float best=max;
            foreach(Actor a in enemies)
            {
                float d=Vector2.Distance(a.body.position,player.body.position);
                if(d<best){best=d;found=a;}
            }
            return found;
        }
        void Cast()
        {
            if(player==null || player.hp<=0)return;
            Actor enemy=Closest(spellUnlocked?3.6f:.96f);
            if(enemy==null){Announce("Nenhum inimigo ao alcance.");return;}
            if(!spellUnlocked){Damage(enemy,26);return;}
            if(mana<9){Announce("Anima insuficiente.");return;}
            mana-=9;
            SpriteRenderer orb=Visual("Orbe espectral",player.body.position,NecroArt.Effect(),20);
            Fit(orb,.34f,.34f);
            Vector2 vector=((Vector2)enemy.body.position-(Vector2)player.body.position).normalized;
            projectiles.Add(new Projectile{body=orb.transform,direction=vector,time=1});
        }
        void UpdateProjectiles(float dt)
        {
            for(int i=projectiles.Count-1;i>=0;i--)
            {
                Projectile p=projectiles[i];
                p.body.position+=(Vector3)(p.direction*(dt*7));
                p.time-=dt;
                bool finished=p.time<=0 || Blocked(p.body.position,.05f);
                if(!finished)
                {
                    // Iteracao indexada: Damage remove inimigos ao morrer.
                    for(int j=enemies.Count-1;j>=0;j--)
                        if(Vector2.Distance(p.body.position,enemies[j].body.position)<enemies[j].radius+.15f)
                        {
                            Damage(enemies[j],34+playerLevel*3);
                            finished=true;break;
                        }
                }
                if(finished){Destroy(p.body.gameObject);projectiles.RemoveAt(i);}
            }
        }
        int Command()
        {
            int used=0;
            foreach(Actor a in servants)
                used+=a.kind=="warden"?3:a.kind=="slime"?2:1;
            return used;
        }
        void Raise()
        {
            if(player==null || player.hp<=0)return;
            Corpse closest=null;float best=1.1f;
            foreach(Corpse corpse in corpses)
            {
                float d=Vector2.Distance(corpse.body.position,player.body.position);
                if(d<best){best=d;closest=corpse;}
            }
            if(closest==null){Announce("Aproxime-se de um corpo.");return;}
            int command=closest.kind=="warden"?3:closest.kind=="slime"?2:1;
            int cost=closest.kind=="warden"?37:closest.kind=="slime"?22:16;
            if(Command()+command>commandLimit){Announce("Limite de comando atingido.");return;}
            if(mana<cost){Announce("Anima insuficiente.");return;}
            mana-=cost;
            Actor minion=Create(closest.kind,closest.body.position,true);
            minion.attack*=1.4f;
            servants.Add(minion);
            Destroy(closest.body.gameObject);corpses.Remove(closest);
            Announce("Servo recrutado: "+minion.kind+".");
        }
        void Claim()
        {
            claimed=true;
            if(stage==0)
            {
                backdrop.sprite=NecroArt.Environment(level.baseBackground);
                Fit(backdrop,level.width,level.height);
            }
            if(seal!=null)seal.localScale=Vector3.one*2.5f;
            Announce(stage==0?"A caverna agora e sua primeira base!":"Tuneis purificados!");
        }
        void Announce(string message){notice=message;noticeUntil=Time.time+4f;}
        void OnGUI()
        {
            if(player==null || level==null)return;
            float scale=Mathf.Clamp(Screen.width/420f,.65f,1.6f);
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w=Screen.width/scale,h=Screen.height/scale;
            GUI.Box(new Rect(8,8,w-16,115),level.title);
            GUI.Label(new Rect(17,32,w-32,21),"Vida "+Mathf.CeilToInt(player.hp)+"/"+player.maxHp+
                " | Anima "+mana+"/"+maxMana);
            GUI.Label(new Rect(17,54,w-32,21),"Nivel "+playerLevel+" | Servos "+servants.Count+
                " | Comando "+Command()+"/4 | Almas "+souls);
            GUI.Label(new Rect(17,79,w-32,32),Time.time<noticeUntil?notice:"Explore, derrote e ressuscite.");
            float cy=h-128;
            Vector2 direction=Vector2.zero;
            if(GUI.RepeatButton(new Rect(70,cy-65,56,52),"▲"))direction.y++;
            if(GUI.RepeatButton(new Rect(70,cy+49,56,52),"▼"))direction.y--;
            if(GUI.RepeatButton(new Rect(14,cy-8,55,52),"◀"))direction.x--;
            if(GUI.RepeatButton(new Rect(127,cy-8,55,52),"▶"))direction.x++;
            touchMove=direction.normalized;
            if(GUI.Button(new Rect(w-127,cy-64,114,53),spellUnlocked?"MAGIA":"GOLPE"))Cast();
            if(GUI.Button(new Rect(w-127,cy-5,114,53),"REVIVER"))Raise();
            if(GUI.Button(new Rect(w-127,cy+54,114,46),huntOrder?"CACAR":"DEFENDER"))
                huntOrder=!huntOrder;
            if(claimed && stage+1<levels.Length && GUI.Button(
                new Rect(w*.26f,h*.25f,w*.48f,52),"ENTRAR NOS TUNEIS"))
                LoadStage(stage+1,true);
            if(player.hp<=0 && GUI.Button(new Rect(w*.26f,h*.46f,w*.48f,55),"REINICIAR"))
                LoadStage(stage,false);
        }
    }
}

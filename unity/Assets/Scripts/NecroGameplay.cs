using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Independent combat, necromancy, drops and equipment progression.
    /// Uses persistent actors and stage-owned corpses/resources/monsters.
    /// </summary>
    public partial class NecroPrototype
    {
        static bool IsBoss(string kind)
        {
            return kind=="warden" || kind=="grave_lord" || kind=="marsh_lord";
        }
        static int CommandCost(string kind)
        {
            return kind=="marsh_lord"?4:
                kind=="warden"||kind=="grave_lord"?3:
                kind=="slime"||kind=="skeleton"?2:1;
        }
        static int ManaCost(string kind)
        {
            return kind=="marsh_lord"?50:
                kind=="warden"||kind=="grave_lord"?37:
                kind=="slime"||kind=="skeleton"?22:16;
        }
        static int ExperienceValue(string kind)
        {
            return IsBoss(kind)?150:kind=="skeleton"?29:kind=="wisp"?25:
                kind=="slime"?20:kind=="bat"?17:14;
        }
        int ExperienceThreshold() {return 28+playerLevel*18;}
        int CommandUsed()
        {
            int used=0;
            foreach(Actor servant in servants)used+=CommandCost(servant.kind);
            return used;
        }
        int Rank(string kind)
        {
            return speciesRanks.TryGetValue(kind,out int result)?result:0;
        }
        void ApplySpeciesRank(Actor actor)
        {
            int rank=Rank(actor.kind);
            actor.maxHp*=1+.24f*rank;
            actor.hp=actor.maxHp;
            actor.attack*=(1+.18f*rank)*1.4f;
        }
        bool UpgradeSpecies(string kind)
        {
            int before=Rank(kind);
            int cost=2+before*2;
            if(before>=3){Announce("Esta criatura ja esta no grau maximo.");return false;}
            if(shards<cost){Announce("Fragmentos de alma insuficientes.");return false;}
            shards-=cost;
            speciesRanks[kind]=before+1;
            foreach(Actor servant in servants)
            {
                if(servant.kind!=kind)continue;
                float newMax=servant.maxHp*(1+.24f*(before+1))/(1+.24f*before);
                servant.hp=Mathf.Min(newMax,servant.hp+(newMax-servant.maxHp));
                servant.maxHp=newMax;
                servant.attack*=(1+.18f*(before+1))/(1+.18f*before);
            }
            foreach(NecroActorRecord stored in vault)
            {
                if(stored.kind!=kind)continue;
                float max=stored.maxHp*(1+.24f*(before+1))/(1+.24f*before);
                stored.hp=Mathf.Min(max,stored.hp+(max-stored.maxHp));
                stored.maxHp=max;
                stored.attack*=(1+.18f*(before+1))/(1+.18f*before);
            }
            Announce(kind.ToUpperInvariant()+" evoluiu para grau "+(before+1)+".");
            SaveProgress();
            return true;
        }

        bool Blocked(Vector2 point,float radius)
        {
            if(point.x<radius || point.y<radius ||
                point.x>level.width-radius || point.y>level.height-radius)return true;
            foreach(Obstacle obstacle in level.obstacles)
                if(Vector2.Distance(point,NecroLevels.At(level,obstacle.x,obstacle.y)) <
                    radius+obstacle.r)return true;
            return false;
        }
        bool Visible(Vector2 from,Vector2 to)
        {
            float distance=Vector2.Distance(from,to);
            int samples=Mathf.CeilToInt(distance/.22f);
            for(int i=1;i<samples;i++)
                if(Blocked(Vector2.Lerp(from,to,(float)i/samples),.04f))return false;
            return true;
        }
        void Move(Actor actor,Vector2 displacement)
        {
            Vector2 at=actor.body.position;
            Vector2 x=at+new Vector2(displacement.x,0);
            if(!Blocked(x,actor.radius))at=x;
            Vector2 y=at+new Vector2(0,displacement.y);
            if(!Blocked(y,actor.radius))at=y;
            actor.body.position=new Vector3(at.x,at.y,0);
        }
        void MoveToward(Actor actor,Vector2 destination,float dt)
        {
            Vector2 at=actor.body.position;
            Vector2 delta=destination-at;
            if(delta.sqrMagnitude<.005f)return;
            Vector2 normal=delta.normalized;
            Vector2 direct=normal*actor.speed*dt;
            Vector2 prior=actor.body.position;
            Move(actor,direct);
            if(Vector2.Distance(prior,actor.body.position)>.02f*dt)return;
            // Sample lateral paths when a rock blocks the direct path.
            foreach(float angle in new float[]{35,-35,70,-70,105,-105})
            {
                Vector2 candidate=Quaternion.Euler(0,0,angle)*normal;
                Move(actor,candidate*actor.speed*dt);
                if(Vector2.Distance(prior,actor.body.position)>.02f*dt)break;
            }
        }
        void UpdateActors(List<Actor> actors,float dt,bool friendly)
        {
            for(int i=actors.Count-1;i>=0;i--)
            {
                if(i>=actors.Count)continue;
                Actor self=actors[i];
                if(self.hp<=0)continue;
                self.cooldown=Mathf.Max(0,self.cooldown-dt);
                self.flash=Mathf.Max(0,self.flash-dt);
                if(self.kind=="rotboar")
                {
                    UpdateRotboar(self,dt);
                    continue;
                }
                if(self.kind=="grave_pup")
                {
                    Actor prey=friendly?servantOrder:player;
                    if(!friendly && servants.Count>0 &&
                        Vector2.Distance(self.body.position,servants[0].body.position)<
                        Vector2.Distance(self.body.position,player.body.position))
                        prey=servants[0];
                    if(prey!=null && prey.hp>0)
                        UpdatePup(self,prey,dt);
                    else if(friendly)
                    {
                        servantOrder=null;
                        self.pupAttackTime=0;
                        self.view.SetPupAction(0,Vector2.zero);
                        FollowNecromancer(self,dt);
                    }
                    continue;
                }
                Actor target=null;
                float closest=friendly?(huntOrder?5.6f:3.2f):4.1f;
                if(friendly)
                {
                    foreach(Actor enemy in current.enemies)
                    {
                        float distance=Vector2.Distance(self.body.position,enemy.body.position);
                        if(enemy.hp>0 && distance<closest){closest=distance;target=enemy;}
                    }
                    if(Vector2.Distance(self.body.position,player.body.position)>6)target=null;
                }
                else
                {
                    float distance=Vector2.Distance(self.body.position,player.body.position);
                    if(player.hp>0 && distance<closest){closest=distance;target=player;}
                    foreach(Actor friend in servants)
                    {
                        distance=Vector2.Distance(self.body.position,friend.body.position);
                        if(friend.hp>0 && distance<closest){closest=distance;target=friend;}
                    }
                }
                if(target!=null)
                {
                    float distance=Vector2.Distance(self.body.position,target.body.position);
                    bool obstructed=!Visible(self.body.position,target.body.position);
                    if(distance>self.reach+target.radius || obstructed)
                        MoveToward(self,target.body.position,dt);
                    else if(self.cooldown<=0)
                    {
                        self.cooldown=IsBoss(self.kind)?1.55f:1.18f;
                        if(self.view!=null)self.view.Strike();
                        Damage(target,self.attack);
                    }
                }
                else if(friendly)
                {
                    float theta=i*2.399963f;
                    Vector2 desired=(Vector2)player.body.position+
                        new Vector2(Mathf.Cos(theta),Mathf.Sin(theta))*.95f;
                    if(Vector2.Distance(self.body.position,desired)>.32f)
                        MoveToward(self,desired,dt);
                }
            }
        }
        // The companion can linger until the player leaves its comfort range.
        bool catchingUp;
        void FollowNecromancer(Actor pup,float dt)
        {
            Vector2 hero=player.body.position;
            Vector2 offset=(Vector2)pup.body.position-hero;
            float distance=offset.magnitude;
            if(distance>2.7f)catchingUp=true;
            else if(distance<=1.7f)catchingUp=false;

            if(catchingUp)
                MoveToward(pup,hero,dt);
            else if(distance<1.05f)
            {
                Vector2 away=distance>.01f?offset/distance:-facing;
                Move(pup,away*Mathf.Min(1.05f-distance,pup.speed*.65f*dt));
            }
        }

        void UpdatePup(Actor pup,Actor prey,float dt)
        {
            const float windup=.42f, leap=.24f;
            Vector2 offset=prey.body.position-pup.body.position;
            if(pup.pupAttackTime<=0)
            {
                if(!pup.ally && offset.magnitude>4.1f)return;
                if(offset.magnitude>1.35f || !Visible(pup.body.position,prey.body.position))
                {
                    MoveToward(pup,prey.body.position,dt);
                    return;
                }
                if(pup.cooldown>0)return;
                pup.pupHeading=offset.sqrMagnitude>.0001f?offset.normalized:Vector2.right;
                pup.pupAttackTime=windup+leap;
                pup.pupHit=false;
                pup.view.SetPupAction(2,pup.pupHeading);
            }
            float previous=pup.pupAttackTime;
            pup.pupAttackTime=Mathf.Max(0,previous-dt);
            float leapStep=Mathf.Min(previous,leap)-Mathf.Min(pup.pupAttackTime,leap);
            if(leapStep>0)
            {
                pup.view.SetPupAction(3,pup.pupHeading);
                Move(pup,pup.pupHeading*(5.8f*leapStep));
                if(!pup.pupHit && Vector2.Distance(prey.body.position,pup.body.position)<pup.radius+prey.radius &&
                    Visible(pup.body.position,prey.body.position))
                {
                    Damage(prey,pup.attack);
                    pup.pupHit=true;
                }
            }
            if(pup.pupAttackTime<=0)
            {
                pup.cooldown=.95f;
                pup.view.SetPupAction(0,pup.pupHeading);
            }
        }

        void Damage(Actor victim,float amount)
        {
            if(victim.hp<=0)return;
            if(victim==player)
            {
                heroineHitPulse=.27f;
                return;
            }
            victim.hp-=amount;
            victim.flash=.16f;
            if(victim.view!=null)victim.view.Flash();
            NecroCombatFx.Emit(victim.body.position,current.root.transform,0);
            if(victim.hp>0)return;
            if(victim.ally)
            {
                servants.Remove(victim);
                servantOrder=null;
            }
            else current.enemies.Remove(victim);
            if(victim.kind=="grave_pup")
            {
                victim.view.Fall();
                if(!victim.ally)current.fallenPups.Add(victim.view);
            }
            else if(victim.kind=="rotboar")StartCoroutine(FallRotboar(victim));
            else Destroy(victim.body.gameObject);
        }
        Actor servantOrder;

        NecroCreatureView NearbyFallenPup()
        {
            NecroCreatureView nearest=null;
            float best=1.3f;
            for(int i=current.fallenPups.Count-1;i>=0;i--)
            {
                var pup=current.fallenPups[i];
                if(pup==null || !pup.CanBind)
                {
                    current.fallenPups.RemoveAt(i);
                    continue;
                }
                if(servants.Count>0)continue;
                float distance=Vector2.Distance(player.body.position,pup.transform.position);
                if(distance<best && Visible(player.body.position,pup.transform.position))
                {
                    nearest=pup;
                    best=distance;
                }
            }
            return nearest;
        }

        void BindNearbyPup()
        {
            var pup=NearbyFallenPup();
            if(pup==null)return;
            Vector2 at=pup.transform.position;
            catchingUp=false;
            servants.Add(Create("grave_pup",at,true,persistentRoot.transform));
            current.fallenPups.Remove(pup);
            Destroy(pup.gameObject);
            NecroCombatFx.Emit(at,current.root.transform,1);
            heroineRaisePulse=.92f;
        }

        Actor Closest(float maxDistance)
        {
            Actor found=null;
            float best=maxDistance;
            foreach(Actor actor in current.enemies)
            {
                if(actor.hp<=0)continue;
                float distance=Vector2.Distance(actor.body.position,player.body.position);
                if(distance<best && Visible(player.body.position,actor.body.position)){best=distance;found=actor;}
            }
            return found;
        }
        float castCooldown;
        const float ManualRange=5.4f;
        bool manualQueued;
        float noTargetUntil;

        void CastAtRange(float range)
        {
            if(player.hp<=0 || upgradeOpen || castCooldown>Time.time)return;
            Actor target=Closest(range);
            if(target==null){Announce("Nenhum alvo ao alcance.");return;}
            if(servants.Count>0)servantOrder=target;
            if(!spellUnlocked)
            {
                heroineStrikePulse=.53f;
                castCooldown=Time.time+.47f;
                Damage(target,26+staffTier*5+(playerLevel-1)*2);
                return;
            }
            castAim=((Vector2)(target.body.position-player.body.position)).normalized;
            if(castAim.sqrMagnitude<.01f)castAim=facing;
            castTarget=target;
            releasePending=true;
            heroineCastPulse=CastDuration;
            castCooldown=Time.time+.72f;
        }

        void ReleaseSkull()
        {
            Vector2 center=player.body.position;
            Vector2 hands=center+new Vector2(castAim.x<0?-.43f:.43f,.19f);
            if(Blocked(hands,.05f) || !Visible(center,hands))hands=center;
            Vector2 direction=castAim;
            if(castTarget!=null && castTarget.body!=null && castTarget.hp>0)
            {
                Vector2 delta=(Vector2)castTarget.body.position-hands;
                if(delta.sqrMagnitude>.0001f)direction=delta.normalized;
            }
            castTarget=null;
            SpriteRenderer orb=Visual("Caveira espectral",hands,skullSprite,20,current.root.transform);
            orb.flipY=direction.x<0;
            orb.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            projectiles.Add(new Projectile{body=orb.transform,direction=direction,time=.9f});
        }
        void UpdateProjectiles(float dt)
        {
            for(int i=projectiles.Count-1;i>=0;i--)
            {
                Projectile bolt=projectiles[i];
                bolt.body.position+=(Vector3)(bolt.direction*(dt*7));
                bolt.time-=dt;
                bool finished=bolt.time<=0 || Blocked(bolt.body.position,.05f);
                if(!finished)
                {
                    for(int j=current.enemies.Count-1;j>=0;j--)
                    {
                        Actor enemy=current.enemies[j];
                        if(Vector2.Distance(bolt.body.position,enemy.body.position)<enemy.radius+.15f)
                        {
                            Damage(enemy,34+staffTier*8+(playerLevel-1)*3);
                            finished=true;break;
                        }
                    }
                }
                if(finished)
                {
                    Destroy(bolt.body.gameObject);
                    projectiles.RemoveAt(i);
                }
            }
        }
        void TickCorpses(float dt)
        {
            for(int i=current.corpses.Count-1;i>=0;i--)
            {
                Corpse body=current.corpses[i];
                body.time-=dt;
                if(body.time<=0)
                {
                    Destroy(body.body.gameObject);
                    current.corpses.RemoveAt(i);
                }
            }
            hitFlash=Mathf.Max(0,hitFlash-dt);
        }
        void Raise()
        {
            if(player.hp<=0 || upgradeOpen)return;
            Corpse nearest=null;
            float best=1.3f;
            foreach(Corpse body in current.corpses)
            {
                float d=Vector2.Distance(body.body.position,player.body.position);
                if(d<best){best=d;nearest=body;}
            }
            if(nearest==null){Announce("Aproxime-se de um cadaver.");return;}
            int cost=ManaCost(nearest.kind),command=CommandCost(nearest.kind);
            if(CommandUsed()+command>CommandLimit)
            {
                Announce("Comando cheio. Dispense um servo.");return;
            }
            if(mana<cost){Announce("Anima insuficiente para ressuscitar.");return;}
            mana-=cost;
            Actor servant=Create(nearest.kind,nearest.body.position,true,persistentRoot.transform);
            servants.Add(servant);
            heroineRaisePulse=.92f;
            NecroCombatFx.Emit(nearest.body.position,current.root.transform,1);
            if(IsBoss(nearest.kind))current.bossBound=true;
            Destroy(nearest.body.gameObject);
            current.corpses.Remove(nearest);
            Announce("Alma vinculada: "+servant.kind+".");
            SaveProgress();
        }
        void Dismiss()
        {
            Actor target=null;float best=2.2f;
            foreach(Actor servant in servants)
            {
                float d=Vector2.Distance(servant.body.position,player.body.position);
                if(d<best){best=d;target=servant;}
            }
            if(target==null){Announce("Aproxime-se do servo a dispensar.");return;}
            servants.Remove(target);
            Destroy(target.body.gameObject);
            Announce("Servo dispensado. Pontos de comando liberados.");
            SaveProgress();
        }
        void SpawnDrop(Vector2 position,int slot,int tier)
        {
            SpriteRenderer marker=Visual(slot==0?"Foco arcano":"Manto",position,
                NecroArt.Get(slot==0?"focus":"mantle"),8,current.root.transform);
            Fit(marker,.52f,.52f);
            current.drops.Add(new Drop{body=marker.transform,slot=slot,tier=tier});
        }
        void SpawnResource(string type,Vector2 position)
        {
            SpriteRenderer marker=Visual("Recurso "+type,position,
                NecroArt.Get(type),6,current.root.transform);
            Fit(marker,.56f,.56f);
            current.resources.Add(new ResourceNode{body=marker.transform,type=type});
        }
        void CollectNearby()
        {
            for(int i=current.drops.Count-1;i>=0;i--)
            {
                Drop drop=current.drops[i];
                if(Vector2.Distance(player.body.position,drop.body.position)>.64f)continue;
                if(drop.slot==0)
                {
                    if(drop.tier>staffTier) {
                        staffTier=drop.tier;Announce("Foco arcano grau "+staffTier+" equipado.");
                    }
                    else {shards++;Announce("Foco duplicado convertido em fragmento.");}
                }
                else
                {
                    if(drop.tier>mantleTier) {
                        int increase=drop.tier-mantleTier;
                        mantleTier=drop.tier;
                        player.maxHp+=18*increase;player.hp=Mathf.Min(player.maxHp,player.hp+18*increase);
                        Announce("Manto grau "+mantleTier+" equipado.");
                    }
                    else {shards++;Announce("Manto duplicado convertido em fragmento.");}
                }
                Destroy(drop.body.gameObject);
                current.drops.RemoveAt(i);
                SaveProgress();
            }
            for(int i=current.resources.Count-1;i>=0;i--)
            {
                ResourceNode res=current.resources[i];
                if(Vector2.Distance(player.body.position,res.body.position)>.70f)continue;
                shards+=2;gathered++;
                player.hp=Mathf.Min(player.maxHp,player.hp+9);
                mana=Mathf.Min(maxMana,mana+12);
                Announce("Recurso "+res.type+" coletado: +2 fragmentos.");
                Destroy(res.body.gameObject);
                current.resources.RemoveAt(i);
                SaveProgress();
            }
        }
    }
}


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
            return IsBoss(kind)?150:kind=="rotboar"?38:
                kind=="grave_pup"?18:kind=="skeleton"?29:kind=="wisp"?25:
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
            if(inSanctuary)return SanctuaryBlocked(point,radius);
            if(point.x<radius || point.y<radius ||
                point.x>level.width-radius || point.y>level.height-radius)return true;
            // The continuous cave uses a shared wall grid. It is not an open
            // rectangular field with merely decorative walls.
            if(NecroCaveNetwork.Blocked(level,point,radius))return true;
            if(level.obstacles!=null)foreach(Obstacle obstacle in level.obstacles)
            {
                // Fast circle collision for complex maps and many following servants.
                float dx=point.x-obstacle.x;
                float dy=point.y-(level.height-obstacle.y);
                float safe=radius+obstacle.r;
                if(dx*dx+dy*dy<safe*safe)return true;
            }
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
        static bool OverlapsActor(Actor mover,Actor other,Vector2 from,Vector2 to)
        {
            if(other==mover || other==null || other.hp<=0 || other.body==null)return false;
            Vector2 center=other.body.position;
            float radius=mover.radius+other.radius;
            float nextDistance=(to-center).sqrMagnitude;
            // Allow actors created in contact to move apart.
            return nextDistance<radius*radius &&
                nextDistance<=(from-center).sqrMagnitude+.000001f;
        }

        Actor ActorAt(Actor mover,Vector2 from,Vector2 to)
        {
            if(OverlapsActor(mover,player,from,to))return player;
            foreach(Actor servant in servants)
                if(OverlapsActor(mover,servant,from,to))return servant;
            foreach(Actor enemy in current.enemies)
                if(OverlapsActor(mover,enemy,from,to))return enemy;
            return null;
        }

        const float PushSpeedFactor=.62f;
        readonly List<Actor> pushedBodies=new List<Actor>();

        void CollectPushContacts(Actor mover,Vector2 from,Vector2 to,List<Actor> group)
        {
            foreach(Actor other in group)
                if(OverlapsActor(mover,other,from,to) && !pushedBodies.Contains(other))
                    pushedBodies.Add(other);
        }

        bool TryPushStep(Vector2 from,Vector2 step)
        {
            Vector2 next=from+step;
            if(Blocked(next,player.radius))return false;
            pushedBodies.Clear();
            CollectPushContacts(player,from,next,servants);
            CollectPushContacts(player,from,next,current.enemies);
            // Check the entire chain before displacing any creature.
            for(int i=0;i<pushedBodies.Count;i++)
            {
                Actor body=pushedBodies[i];
                Vector2 pos=body.body.position;
                Vector2 dest=pos+step;
                if(Blocked(dest,body.radius))return false;
                CollectPushContacts(body,pos,dest,servants);
                CollectPushContacts(body,pos,dest,current.enemies);
            }
            foreach(Actor body in pushedBodies)body.body.position+=(Vector3)step;
            return true;
        }

        Actor Move(Actor actor,Vector2 displacement,bool slide=true)
        {
            Vector2 at=actor.body.position;
            int steps=Mathf.Max(1,Mathf.CeilToInt(displacement.magnitude/.035f));
            Vector2 step=displacement/steps;
            Actor contact=null;
            for(int i=0;i<steps;i++)
            {
                Vector2 next=at+step;
                Actor hit=ActorAt(actor,at,next);
                bool wall=Blocked(next,actor.radius);
                if(hit==null && !wall)at=next;
                else if(actor==player && hit!=null && TryPushStep(at,step*PushSpeedFactor))
                    at+=step*PushSpeedFactor;
                else
                {
                    contact=hit;
                    if(!slide)break;
                    Vector2 x=at+new Vector2(step.x,0);
                    if(!Blocked(x,actor.radius) && ActorAt(actor,at,x)==null)at=x;
                    Vector2 y=at+new Vector2(0,step.y);
                    if(!Blocked(y,actor.radius) && ActorAt(actor,at,y)==null)at=y;
                }
            }
            actor.body.position=new Vector3(at.x,at.y,0);
            return contact;
        }
        static readonly float[] AvoidanceAngles={35f,-35f,70f,-70f,105f,-105f};
        void MoveToward(Actor actor,Vector2 destination,float dt,
            float followBoost=1f)
        {
            Vector2 at=actor.body.position;
            Vector2 delta=destination-at;
            if(delta.sqrMagnitude<.005f)return;
            Vector2 normal=delta.normalized;
            float travelSpeed=actor.speed*GraveCommandSpeedMultiplier(actor)*
                Mathf.Max(1f,followBoost);
            // Clamp the step to avoid oscillation around formation slots.
            float distance=Mathf.Min(delta.magnitude,travelSpeed*dt);
            Vector2 direct=normal*distance;
            Vector2 prior=actor.body.position;
            Move(actor,direct);
            if(Vector2.Distance(prior,actor.body.position)>.02f*dt)return;
            // Sample lateral paths when a rock blocks the direct path.
            foreach(float angle in AvoidanceAngles)
            {
                Vector2 candidate=Quaternion.Euler(0,0,angle)*normal;
                Move(actor,candidate*distance);
                if(Vector2.Distance(prior,actor.body.position)>.02f*dt)break;
            }
        }
        void UpdateActors(List<Actor> actors,float dt,bool friendly)
        {
            for(int i=actors.Count-1;i>=0;i--)
            {
                // A fatal hit must stop the remaining hostiles immediately.
                if(gameOver)return;
                if(i>=actors.Count)continue;
                Actor self=actors[i];
                if(self.hp<=0)continue;
                self.cooldown=Mathf.Max(0,self.cooldown-dt);
                self.flash=Mathf.Max(0,self.flash-dt);
                // Visual emergence must complete before newly bound souls fight.
                if(friendly && Time.time<self.reanimationUntil)continue;
                if(self.kind=="rotboar")
                {
                    UpdateRotboar(self,dt);
                    continue;
                }
                if(self.kind=="grave_pup")
                {
                    Actor prey=friendly?FindServantTarget(self):
                        ClosestPartyMember(self);
                    if(prey!=null && prey.hp>0)
                        UpdatePup(self,prey,dt);
                    else
                    {
                        // A lost or defeated target must cancel a pending leap.
                        self.pupAttackTime=0f;
                        if(self.view!=null)
                            self.view.SetPupAction(0,Vector2.zero);
                        if(friendly)FollowNecromancer(self,dt);
                        else ReturnToLair(self,dt);
                    }
                    continue;
                }
                // All hostile species now honor the same party threat policy.
                Actor target=friendly?FindServantTarget(self):
                    ClosestPartyMember(self);
                if(target!=null)
                {
                    float distance=Vector2.Distance(self.body.position,target.body.position);
                    bool obstructed=!Visible(self.body.position,target.body.position);
                    if(distance>self.reach+target.radius || obstructed)
                        MoveToward(self,target.body.position,dt);
                    else if(self.cooldown<=0)
                    {
                        self.cooldown=EnemyStrikeCooldown(self);
                        if(self.view!=null)
                            self.view.Strike(target.body.position-self.body.position);
                        Damage(target,friendly?SkillServantDamage(self.attack):self.attack);
                    }
                }
                else if(friendly)FollowNecromancer(self,dt);
                else ReturnToLair(self,dt);
            }
        }
        // Shared target selection for standard hostiles, Grave Pups and Rotboars.
        // Each species retains its own acquisition range and attack animation.
        // A 20% retarget margin prevents rapid player/servant target flipping.
        Actor ClosestPartyMember(Actor enemy)
        {
            if(enemy==null || enemy.body==null || player==null ||
                player.body==null || player.hp<=0f)return null;
            // End a chase beyond the enemy's territory; returning creatures
            // cannot build an endless train behind the player.
            if(NecroEncounterRules.OutsideTerritory(
                (Vector2)enemy.body.position,enemy.lairPosition))
            {
                enemy.hostileFocus=null;
                enemy.targetMemoryUntil=0f;
                return null;
            }
            float range=enemy.kind=="grave_pup"?
                HostilePupDetectionDistance():
                enemy.kind=="rotboar"?5.5f:4.1f;
            float rangeSquared=range*range;
            Vector2 source=enemy.body.position;
            Actor nearest=null;
            float best=float.PositiveInfinity;
            float heroDistance=(player.body.position-enemy.body.position).sqrMagnitude;
            if(heroDistance<=rangeSquared &&
                Visible(source,player.body.position))
            {
                nearest=player;
                best=NecroTargetRules.ThreatScore(heroDistance,false,1f);
            }
            float companionWeight=ServantThreatDistanceWeight();
            foreach(Actor ally in servants)
            {
                if(ally==null || ally.hp<=0f || ally.body==null)continue;
                float unweighted=(ally.body.position-enemy.body.position).sqrMagnitude;
                // Weighted threat must never extend the physical aggro radius.
                if(unweighted>rangeSquared)continue;
                if(!Visible(source,ally.body.position))continue;
                float distance=(ally.body.position-enemy.body.position).sqrMagnitude*
                    companionWeight;
                if(distance<best){best=distance;nearest=ally;}
            }

            Actor previous=enemy.hostileFocus;
            if(previous!=null && previous.hp>0f && previous.body!=null &&
                (previous==player || servants.Contains(previous)))
            {
                float separation=((Vector2)previous.body.position-source).sqrMagnitude;
                if(separation<=rangeSquared)
                {
                    bool stillSeen=Visible(source,previous.body.position);
                    if(stillSeen)
                        enemy.targetMemoryUntil=Time.time+
                            NecroEncounterRules.SightMemorySeconds;
                    // Short memory lets foes round a corner, but not follow
                    // invisible targets forever or through entire walls.
                    if(stillSeen || Time.time<enemy.targetMemoryUntil)
                    {
                        float heldScore=NecroTargetRules.ThreatScore(
                            separation,previous!=player,companionWeight);
                        if(nearest==null ||
                            NecroTargetRules.KeepFocus(heldScore,best))
                            nearest=previous;
                    }
                }
            }
            if(nearest!=previous)
                enemy.targetMemoryUntil=nearest!=null?
                    Time.time+NecroEncounterRules.SightMemorySeconds:0f;
            enemy.hostileFocus=nearest;
            return nearest;
        }

        void ReturnToLair(Actor enemy,float dt)
        {
            if(enemy==null || enemy.ally || enemy.body==null)return;
            if(!NecroEncounterRules.NeedsReturn(
                (Vector2)enemy.body.position,enemy.lairPosition))return;
            // A slower walk back keeps encounters readable; normal obstacle
            // avoidance and collision still govern every movement step.
            MoveToward(enemy,enemy.lairPosition,dt*.72f);
        }

        // Formation and catch-up logic lives in NecroServantFormation.cs.

        void UpdatePup(Actor pup,Actor prey,float dt)
        {
            float windup=PupWindupDuration(pup);
            const float leap=.24f;
            Vector2 offset=prey.body.position-pup.body.position;
            if(pup.pupAttackTime<=0)
            {
                if(!pup.ally &&
                   offset.magnitude>HostilePupDetectionDistance())return;
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
                Actor contact=Move(pup,pup.pupHeading*(5.8f*leapStep),false);
                if(!pup.pupHit && (contact==prey || Vector2.Distance(prey.body.position,pup.body.position)<=pup.radius+prey.radius+.04f) &&
                    Visible(pup.body.position,prey.body.position))
                {
                    Damage(prey,pup.ally?SkillServantDamage(pup.attack):pup.attack);
                    pup.pupHit=true;
                }
            }
            if(pup.pupAttackTime<=0)
            {
                pup.cooldown=PupRecoveryCooldown(pup);
                pup.view.SetPupAction(0,pup.pupHeading);
            }
        }

        void Damage(Actor victim,float amount)
        {
            if(inSanctuary)return;
            if(victim.hp<=0)return;
            if(victim==player)
            {
                ApplyHeroDamage(amount);
                return;
            }
            victim.hp-=amount;
            victim.flash=.16f;
            if(victim.view!=null)victim.view.Flash();
            NecroCombatFx.Emit(victim.body.position,current.root.transform,0,
                victim.ally);
            if(victim.hp>0)return;
            if(victim.ally)
            {
                servants.Remove(victim);
                SaveCompanions();
            }
            else
            {
                current.enemies.Remove(victim);
                if(IsBoss(victim.kind))current.bossDefeated=true;
            }
            if(victim.kind=="grave_pup")
            {
                victim.view.Fall();
                if(!victim.ally)current.fallenPups.Add(victim.view);
            }
            else if(victim.kind=="rotboar")StartCoroutine(FallRotboar(victim));
            else if(victim.ally)
            {
                // Summoned creatures dissipate; they cannot be resurrected.
                if(victim.view!=null)victim.view.FallToCorpse(0f);
                else Destroy(victim.body.gameObject);
            }
            else SpawnRecoverableCorpse(victim);
            // A single rising soul marks death without covering the corpse.
            NecroCombatFx.Emit(victim.body.position,current.root.transform,7,
                victim.ally);
            // Defeated hostiles award XP exactly once, including servant kills.
            if(!victim.ally)AwardMeditationExperience(victim.kind);
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
                float distance=Vector2.Distance(player.body.position,pup.transform.position);
                if(distance<best && Visible(player.body.position,pup.transform.position))
                {
                    nearest=pup;
                    best=distance;
                }
            }
            return nearest;
        }

        Actor NearbyFallenBoar()
        {
            Actor nearest=null;
            float best=1.8f;
            for(int i=current.fallenBoars.Count-1;i>=0;i--)
            {
                Actor boar=current.fallenBoars[i];
                if(boar.body==null || Time.time>=boar.bindUntil)
                {
                    current.fallenBoars.RemoveAt(i);
                    continue;
                }
                float distance=Vector2.Distance(player.body.position,boar.body.position);
                if(distance<best && Visible(player.body.position,boar.body.position))
                {nearest=boar;best=distance;}
            }
            return nearest;
        }

        bool CanBindNearby()
        {
            if(!HeroCanUseMagic)return false;
            return NearbyCorpse()!=null || NearbyFallenPup()!=null ||
                NearbyFallenBoar()!=null;
        }

        void BindNearbyCreature()
        {
            if(!HeroCanUseMagic)return;
            var corpse=NearbyCorpse();
            var boar=NearbyFallenBoar();
            var pup=NearbyFallenPup();
            float cd=corpse!=null ?
                Vector2.Distance(player.body.position,corpse.body.position):
                float.MaxValue;
            float bd=boar!=null ?
                Vector2.Distance(player.body.position,boar.body.position):
                float.MaxValue;
            float pd=pup!=null ?
                Vector2.Distance(player.body.position,pup.transform.position):
                float.MaxValue;
            if(corpse!=null && cd<=bd && cd<=pd)
            {
                Raise();
                return;
            }
            if(pup!=null && pd<=bd)
            {
                BindNearbyPup();
                return;
            }
            if(boar==null)return;
            if(CommandUsed()+CommandCost(boar.kind)>CommandLimit)
            {
                Announce("Comando cheio. Dispense um servo.");return;
            }
            Vector2 at=boar.body.position;
            Actor servant=CreateRotboar(at,persistentRoot.transform,true);
            servants.Add(servant);
            current.fallenBoars.Remove(boar);
            Destroy(boar.body.gameObject);
            AnimateReanimation(servant,at);
            SaveCompanions();
            SaveProgress();
        }

        void BindNearbyPup()
        {
            if(!HeroCanUseMagic)return;
            var pup=NearbyFallenPup();
            if(pup==null)return;
            if(CommandUsed()+CommandCost("grave_pup")>CommandLimit)
            {
                Announce("Comando cheio. Dispense um servo.");return;
            }
            Vector2 at=pup.transform.position;
            Actor servant=Create("grave_pup",at,true,persistentRoot.transform);
            servants.Add(servant);
            current.fallenPups.Remove(pup);
            Destroy(pup.gameObject);
            AnimateReanimation(servant,at);
            SaveCompanions();
            SaveProgress();
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
            if(!HeroCanUseMagic)range=Mathf.Min(range,EffectiveManualRange);
            if(player.hp<=0 || upgradeOpen || castCooldown>Time.time)return;
            Actor target=Closest(range);
            if(target==null){Announce("Nenhum alvo ao alcance.");return;}
            if(servants.Count>0)servantOrder=target;
            if(!HeroCanUseMagic || !spellUnlocked)
            {
                heroineStrikePulse=.53f;
                castCooldown=Time.time+.47f;
                // Face the opponent before showing the unarmed strike pose.
                Vector2 strikeDelta=target.body.position-player.body.position;
                if(strikeDelta.sqrMagnitude>.0001f)
                {
                    facing=strikeDelta.normalized;
                    walkDirection=DirectionFrom(facing);
                }
                Damage(target,(26+staffTier*5+(playerLevel-1)*2+powerPoints*2)*FountainDamageMultiplier);
                NecroCombatFx.Emit(target.body.position,current.root.transform,10);
                return;
            }
            castAim=((Vector2)(target.body.position-player.body.position)).normalized;
            if(castAim.sqrMagnitude<.01f)castAim=facing;
            facing=castAim; // The cast pose should face the actual target.
            castTarget=target;
            releasePending=true;
            heroineCastPulse=CastDuration;
            castCooldown=Time.time+(SkillUnlocked(5) ? .60f : .72f);
        }

        void ReleaseSkull()
        {
            if(!HeroCanUseMagic)return;
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
            projectiles.Add(new Projectile{
                body=orb.transform,renderer=orb,direction=direction,time=.9f
            });
            NecroCombatFx.Emit(hands,current.root.transform,8);
        }
        void UpdateProjectiles(float dt)
        {
            for(int i=projectiles.Count-1;i>=0;i--)
            {
                Projectile bolt=projectiles[i];
                if(bolt.body==null)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }
                Vector2 before=bolt.body.position;
                Vector2 after=before+bolt.direction*(dt*7f);
                bolt.body.position=after;
                bolt.time-=dt;
                bolt.age+=dt;
                UpdateSpectralProjectileVisual(bolt);
                bool terrain=Blocked(after,.05f) || !Visible(before,after);
                bool finished=bolt.time<=0f || terrain;
                bool impact=terrain;
                if(!finished)
                {
                    for(int j=current.enemies.Count-1;j>=0;j--)
                    {
                        Actor enemy=current.enemies[j];
                        if(enemy==null || enemy.hp<=0f || enemy.body==null)
                            continue;
                        if(SpectralSegmentHit(before,after,enemy.body.position,
                            enemy.radius+.15f))
                        {
                            Damage(enemy,SkillSpectralDamage(
                                34+staffTier*8+(playerLevel-1)*3+
                                powerPoints*4)*FountainDamageMultiplier);
                            finished=true;
                            impact=true;
                            break;
                        }
                    }
                }
                if(finished)
                {
                    if(impact)
                        NecroCombatFx.Emit(after,current.root.transform,9);
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
                if(body==null || body.body==null)
                {
                    current.corpses.RemoveAt(i);
                    continue;
                }
                body.time-=dt;
                if(body.time<=0f)
                {
                    Destroy(body.body.gameObject);
                    current.corpses.RemoveAt(i);
                }
                else if(body.view!=null)
                    body.view.SetCorpseRemaining(body.time);
            }
            hitFlash=Mathf.Max(0,hitFlash-dt);
        }
        void Raise()
        {
            if(!HeroCanUseMagic)return;
            if(player.hp<=0 || upgradeOpen)return;
            Corpse nearest=NearbyCorpse();
            if(nearest==null){Announce("Aproxime-se de um cadaver.");return;}
            int cost=SkillRaiseManaCost(nearest.kind),command=CommandCost(nearest.kind);
            if(CommandUsed()+command>CommandLimit)
            {
                Announce("Comando cheio. Dispense um servo.");return;
            }
            if(mana<cost){Announce("Anima insuficiente para ressuscitar.");return;}
            mana-=cost;
            Vector2 at=nearest.body.position;
            Actor servant=nearest.kind=="rotboar" ?
                CreateRotboar(at,persistentRoot.transform,true) :
                Create(nearest.kind,at,true,persistentRoot.transform);
            servants.Add(servant);
            AnimateReanimation(servant,at);
            if(IsBoss(nearest.kind))current.bossBound=true;
            Destroy(nearest.body.gameObject);
            current.corpses.Remove(nearest);
            Announce("Alma vinculada: "+servant.kind+".");
            SaveCompanions();
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
            SaveCompanions();
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
            Color shimmer=type=="cristal"?
                new Color(.58f,.89f,1f):
                type=="fungo"?new Color(.66f,.97f,.75f):
                new Color(.92f,.79f,.63f);
            current.resources.Add(new ResourceNode{
                body=marker.transform,type=type,artwork=marker,
                defaultScale=marker.transform.localScale,
                shimmerTint=shimmer,
                shimmerOffset=position.x*.63f+position.y*.47f
            });
        }
        void CollectNearby()
        {
            // One save snapshot for any number of resources collected in the
            // same frame; the loop must not write the whole save per item.
            bool collected=false;
            for(int i=current.drops.Count-1;i>=0;i--)
            {
                Drop drop=current.drops[i];
                if(drop==null || drop.body==null)
                {
                    current.drops.RemoveAt(i);
                    continue;
                }
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
                collected=true;
            }
            for(int i=current.resources.Count-1;i>=0;i--)
            {
                ResourceNode res=current.resources[i];
                if(res==null || res.body==null)
                {
                    current.resources.RemoveAt(i);
                    continue;
                }
                if(Vector2.Distance(player.body.position,res.body.position)>.70f)continue;
                shards+=2;gathered++;
                float healed=NecroExplorationRules.ActualHealing(
                    player.hp,player.maxHp,HealFromGatheredResource());
                player.hp=Mathf.Min(player.maxHp,player.hp+healed);
                int manaGained=Mathf.Min(12,Mathf.Max(0,maxMana-mana));
                mana+=manaGained;
                RecordResourceReward(healed,manaGained);
                // A single short effect for a collected resource, never per frame.
                if(current.root!=null)
                    NecroCombatFx.Emit((Vector2)res.body.position,
                        current.root.transform,9);
                Destroy(res.body.gameObject);
                current.resources.RemoveAt(i);
                collected=true;
            }
            if(collected)SaveProgress();
        }
    }
}


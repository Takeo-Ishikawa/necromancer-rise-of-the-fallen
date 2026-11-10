using System.Collections;
using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        enum BoarPose { Idle, Windup, Charge }
        readonly Sprite[] boarFrames=new Sprite[3];
        readonly Sprite[] alliedBoarFrames=new Sprite[3];

        readonly Sprite[] boarFallFrames=new Sprite[3];

        static void LoadBoarFrames(string path,Sprite[] frames)
        {
            if(frames[0]!=null)return;
            var sheet=Resources.Load<Texture2D>(path);
            for(int i=0;i<frames.Length;i++)
                frames[i]=Sprite.Create(sheet,new Rect(256*i,0,256,256),
                    new Vector2(.5f,.26f),256f,0,SpriteMeshType.FullRect);
        }

        IEnumerator FallRotboar(Actor boar)
        {
            StageInstance owner=current; // A transition can happen during the fall animation.
            LoadBoarFrames("Creatures/rotboar_fallen",boarFallFrames);
            var art=boar.boarArt;
            art.sortingOrder=2;
            art.color=Color.white;
            art.transform.localScale=boar.boarVisualScale;
            art.sprite=boarFallFrames[0];
            yield return new WaitForSeconds(.14f);
            if(art==null)yield break;
            art.sprite=boarFallFrames[1];
            yield return new WaitForSeconds(.18f);
            if(art==null)yield break;
            art.sprite=boarFallFrames[2];
            if(!boar.ally)
            {
                boar.bindUntil=Time.time+14f;
                owner.fallenBoars.Add(boar);
            }
            float remaining=14f;
            while(remaining>0f && art!=null)
            {
                remaining-=Time.deltaTime;
                // The last moments dissolve; players can bind the body while
                // it is still visible, then the corpse disappears naturally.
                float alpha=Mathf.Clamp01(remaining/.72f);
                art.color=new Color(1f,1f,1f,alpha);
                yield return null;
            }
            if(art!=null)Destroy(art.gameObject);
        }

        Actor CreateRotboar(Vector2 at,Transform parent,bool ally=false)
        {
            LoadBoarFrames("Creatures/rotboar",boarFrames);
            if(ally && alliedBoarFrames[0]==null)
            {
                var sheet=Resources.Load<Texture2D>("Creatures/rotboar_ally");
                float cell=sheet.width/3f;
                for(int i=0;i<3;i++)
                    alliedBoarFrames[i]=Sprite.Create(sheet,
                        new Rect(cell*i,0,cell,sheet.height),
                        new Vector2(.5f,.37f),cell,0,SpriteMeshType.FullRect);
            }
            var art=Visual("Rotboar",at,ally?alliedBoarFrames[0]:boarFrames[0],ally?15:12,parent);
            Fit(art,2.8f,2.8f);
            art.flipX=true;
            return new Actor {kind="rotboar",body=art.transform,boarArt=art,
                lairPosition=at,
                boarVisualScale=art.transform.localScale,
                hp=180,maxHp=180,radius=.54f,attack=19f,speed=ally?3.8f:1.5f,ally=ally};
        }

        void SetBoarPose(Actor boar,BoarPose pose,float duration)
        {
            boar.boarPose=pose;
            boar.boarArt.sprite=(boar.ally?alliedBoarFrames:boarFrames)[(int)pose];
            boar.boarArt.transform.localScale=boar.boarVisualScale*
                (pose==BoarPose.Windup?.94f:pose==BoarPose.Charge?1.06f:1f);
            boar.cooldown=duration;
            if(pose==BoarPose.Windup && current!=null && !inSanctuary)
                NecroCombatFx.Emit(boar.body.position,current.root.transform,
                    boar.ally?6:5);
        }

        void UpdateRotboar(Actor boar,float dt)
        {
            // Boars have a direct SpriteRenderer and need their own hit tint.
            boar.boarArt.color=boar.flash>0f ?
                (boar.ally?new Color(.70f,.96f,1f):
                    new Color(1f,.58f,.62f)) : Color.white;
            Vector2 at=boar.body.position;
            if(boar.boarPose==BoarPose.Windup)
            {
                if(boar.cooldown<=0)SetBoarPose(boar,BoarPose.Charge,.55f);
                return;
            }
            if(boar.boarPose==BoarPose.Charge)
            {
                Vector2 next=at+boar.chargeDirection*(6.8f*dt);
                if(boar.cooldown<=0 || Blocked(next,boar.radius))
                {
                    SetBoarPose(boar,BoarPose.Idle,1.25f);
                    return;
                }
                Actor hit=Move(boar,next-at,false);
                bool stopped=Vector2.Distance(boar.body.position,next)>.001f;
                if(hit!=null && hit.ally!=boar.ally)
                    Damage(hit,boar.ally?SkillServantDamage(boar.attack):boar.attack);
                if(hit!=null || stopped)
                    SetBoarPose(boar,BoarPose.Idle,1.25f);
                return;
            }
            if(boar.cooldown>0)return;
            Actor target=boar.ally?FindServantTarget(boar):
                ClosestPartyMember(boar);
            if(target==null || target.hp<=0f || target.body==null)
            {
                if(boar.ally)FollowNecromancer(boar,dt);
                else ReturnToLair(boar,dt);
                Vector2 movement=(Vector2)boar.body.position-at;
                if(Mathf.Abs(movement.x)>.001f)
                    boar.boarArt.flipX=movement.x<0;
                return;
            }
            Vector2 delta=(Vector2)target.body.position-at;
            if(delta.magnitude>5.5f || !Visible(at,target.body.position))return;
            if(Mathf.Abs(delta.x)>.02f)boar.boarArt.flipX=delta.x<0;
            if(delta.magnitude>3.2f)
                MoveToward(boar,target.body.position,dt);
            else
            {
                boar.chargeDirection=delta.sqrMagnitude>.0001f?delta.normalized:Vector2.left;
                SetBoarPose(boar,BoarPose.Windup,
                    RotboarWindupDuration(boar));
            }
        }
    }
}

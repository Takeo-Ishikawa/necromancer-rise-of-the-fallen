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
            LoadBoarFrames("Creatures/rotboar_fallen",boarFallFrames);
            var art=boar.boarArt;
            art.sortingOrder=2;
            art.sprite=boarFallFrames[0];
            yield return new WaitForSeconds(.14f);
            if(art==null)yield break;
            art.sprite=boarFallFrames[1];
            yield return new WaitForSeconds(.18f);
            if(art==null)yield break;
            art.sprite=boarFallFrames[2];
            if(!boar.ally)
            {
                boar.bindUntil=Time.time+3f;
                current.fallenBoars.Add(boar);
            }
            yield return new WaitForSeconds(3f);
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
                hp=180,maxHp=180,radius=.54f,attack=19f,speed=ally?3.8f:1.5f,ally=ally};
        }

        void SetBoarPose(Actor boar,BoarPose pose,float duration)
        {
            boar.boarPose=pose;
            boar.boarArt.sprite=(boar.ally?alliedBoarFrames:boarFrames)[(int)pose];
            boar.cooldown=duration;
        }

        void UpdateRotboar(Actor boar,float dt)
        {
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
                boar.body.position=next;
                Actor hit=null;
                if(boar.ally)
                {
                    foreach(Actor enemy in current.enemies)
                        if(enemy.hp>0 && Vector2.Distance(next,enemy.body.position)<=boar.radius+enemy.radius)
                        {hit=enemy;break;}
                }
                else if(servants.Count>0 && Vector2.Distance(next,servants[0].body.position)<=boar.radius+servants[0].radius)
                    hit=servants[0];
                else if(!boar.ally && Vector2.Distance(next,player.body.position)<=boar.radius+player.radius)
                    hit=player;
                if(hit!=null)
                {
                    Damage(hit,boar.attack);
                    SetBoarPose(boar,BoarPose.Idle,1.25f);
                }
                return;
            }
            if(boar.cooldown>0)return;
            Actor target=boar.ally?servantOrder:player;
            if(!boar.ally && servants.Count>0 && Vector2.Distance(at,servants[0].body.position)<Vector2.Distance(at,player.body.position))
                target=servants[0];
            if(boar.ally && (target==null || target.hp<=0))
            {
                servantOrder=null;
                FollowNecromancer(boar,dt);
                Vector2 movement=(Vector2)boar.body.position-at;
                if(Mathf.Abs(movement.x)>.001f)boar.boarArt.flipX=movement.x<0;
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
                SetBoarPose(boar,BoarPose.Windup,.8f);
            }
        }
    }
}

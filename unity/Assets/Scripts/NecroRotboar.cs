using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        enum BoarPose { Idle, Windup, Charge }
        readonly Sprite[] boarFrames=new Sprite[3];

        Actor CreateRotboar(Vector2 at,Transform parent)
        {
            if(boarFrames[0]==null)
            {
                var sheet=Resources.Load<Texture2D>("Creatures/rotboar");
                for(int i=0;i<boarFrames.Length;i++)
                    boarFrames[i]=Sprite.Create(sheet,new Rect(256*i,0,256,256),
                        new Vector2(.5f,.26f),256f,0,SpriteMeshType.FullRect);
            }
            var art=Visual("Rotboar",at,boarFrames[0],12,parent);
            Fit(art,2.8f,2.8f);
            art.flipX=true;
            return new Actor {kind="rotboar",body=art.transform,boarArt=art,
                hp=180,maxHp=180,radius=.54f,attack=19f,speed=1.5f};
        }

        void SetBoarPose(Actor boar,BoarPose pose,float duration)
        {
            boar.boarPose=pose;
            boar.boarArt.sprite=boarFrames[(int)pose];
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
                if(servants.Count>0 && Vector2.Distance(next,servants[0].body.position)<=boar.radius+servants[0].radius)
                    hit=servants[0];
                else if(Vector2.Distance(next,player.body.position)<=boar.radius+player.radius)
                    hit=player;
                if(hit!=null)
                {
                    Damage(hit,boar.attack);
                    SetBoarPose(boar,BoarPose.Idle,1.25f);
                }
                return;
            }
            if(boar.cooldown>0)return;
            Actor target=player;
            if(servants.Count>0 && Vector2.Distance(at,servants[0].body.position)<Vector2.Distance(at,player.body.position))
                target=servants[0];
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

using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        const float CastDuration=.62f;
        const float CastRelease=.28f;
        readonly Sprite[] heroSprites=new Sprite[32];
        SpriteRenderer witchArtRenderer;
        Sprite skullSprite;
        int walkDirection;
        float walkClock,heroineCastPulse,heroineStrikePulse,heroineRaisePulse,heroineHitPulse;
        bool heroineWalking,releasePending;
        Vector2 castAim;
        Actor castTarget;

        void InitializeHeroAnimation()
        {
            var texture=Resources.Load<Texture2D>("Heroine/spectral");
            texture.filterMode=FilterMode.Bilinear;
            for(int i=0;i<24;i++)
            {
                if(i>=12 && i<16)continue;
                Rect region=new Rect(i%4*256,texture.height-(1+i/4)*256,256,256);
                heroSprites[i]=Sprite.Create(texture,region,new Vector2(.5f,14f/256f),
                    138f,0,SpriteMeshType.FullRect);
            }
            var idle=Resources.Load<Texture2D>("Heroine/idle");
            for(int i=0;i<8;i++)
            {
                Rect rect=new Rect(i%4*256,idle.height-(1+i/4)*256,256,256);
                heroSprites[24+i]=Sprite.Create(idle,rect,new Vector2(.5f,14f/256f),
                    138f,0,SpriteMeshType.FullRect);
            }
            var skullRegion=new Rect(0,texture.height-1024+14,256,72);
            skullSprite=Sprite.Create(texture,skullRegion,new Vector2(.5f,.5f),270f,0,SpriteMeshType.FullRect);

            var root=player.body.GetComponent<SpriteRenderer>();
            root.enabled=false;
            var body=new GameObject("Spectral heroine");
            body.transform.SetParent(player.body,false);
            Vector3 size=player.body.lossyScale;
            body.transform.localScale=new Vector3(1/size.x,1/size.y,1);
            body.transform.localPosition=new Vector3(0,-.52f/size.y,0);
            witchArtRenderer=body.AddComponent<SpriteRenderer>();
            witchArtRenderer.sortingOrder=root.sortingOrder;
            witchArtRenderer.sprite=heroSprites[24];

            var shadow=Visual("Soft contact shadow",player.body.position,
                NecroArt.Shadow(),root.sortingOrder-3,body.transform);
            shadow.transform.localPosition=Vector3.zero;
            Fit(shadow,.70f,.19f);
            shadow.color=new Color(.16f,.13f,.23f,.43f);
        }

        static int DirectionFrom(Vector2 move)
        {
            float angle=Mathf.Atan2(-move.x,-move.y)*Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle/45f)+8)%8;
        }

        void AnimateHeroVisual(float dt)
        {
            heroineHitPulse=Mathf.Max(0,heroineHitPulse-dt);
            if(heroineWalking)
            {
                walkDirection=DirectionFrom(facing);
                walkClock+=dt;
            }
            else walkClock=0;

            int frame;
            bool flip;
            if(heroineCastPulse>0)
            {
                heroineCastPulse=Mathf.Max(0,heroineCastPulse-dt);
                float elapsed=CastDuration-heroineCastPulse;
                if(releasePending && elapsed>=CastRelease)
                {
                    releasePending=false;
                    ReleaseSkull();
                }
                frame=elapsed<.13f?8:elapsed<CastRelease?9:elapsed<.45f?10:11;
                flip=castAim.x<0;
            }
            else if(!heroineWalking)
            {
                frame=24+walkDirection;
                flip=false;
            }
            else
            {
                int phase=(int)(walkClock*7.5f)%4;
                flip=phase>=2;
                int direction=flip?(8-walkDirection)%8:walkDirection;
                frame=direction+(phase%2==1?16:0);
            }
            witchArtRenderer.sprite=heroSprites[frame];
            witchArtRenderer.flipX=flip;
            witchArtRenderer.color=heroineHitPulse>0?new Color(1,.83f,.87f):Color.white;
        }
    }
}

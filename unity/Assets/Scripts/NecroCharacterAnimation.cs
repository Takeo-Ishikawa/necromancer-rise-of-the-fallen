using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        const float CastDuration=.62f;
        const float CastRelease=.28f;
        const float StrikeVisualDuration=.53f;
        const float RaiseVisualDuration=.92f;
        readonly Sprite[] heroSprites=new Sprite[32];
        SpriteRenderer witchArtRenderer;
        Sprite skullSprite;
        const float HeroPaintedFootOffset = .52f;
        const float SanctuaryHeroScale = 1.9f;
        Vector3 heroineNormalScale;
        int walkDirection;
        float walkClock,heroineCastPulse,heroineStrikePulse,heroineRaisePulse,heroineHitPulse;
        bool heroineWalking,releasePending;
        Vector2 castAim;
        Actor castTarget;

        void InitializeHeroAnimation()
        {
            var texture=Resources.Load<Texture2D>(SelectedCharacter.spectralResource);
            texture.filterMode=FilterMode.Bilinear;
            for(int i=0;i<24;i++)
            {
                if(i>=12 && i<16 && !(i==13 && SelectedCharacter.previewMotion))continue;
                Rect region=new Rect(i%4*256,texture.height-(1+i/4)*256,256,256);
                heroSprites[i]=Sprite.Create(texture,region,new Vector2(.5f,14f/256f),
                    138f,0,SpriteMeshType.FullRect);
            }
            var idle=Resources.Load<Texture2D>(SelectedCharacter.idleResource);
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
            body.transform.localPosition=new Vector3(0,-HeroPaintedFootOffset/size.y,0);
            heroineNormalScale=body.transform.localScale;
            witchArtRenderer=body.AddComponent<SpriteRenderer>();
            witchArtRenderer.sortingOrder=19; // Above creatures (12–15), below the spectral shot (20).
            witchArtRenderer.sprite=heroSprites[24];

            var shadow=Visual("Soft contact shadow",player.body.position,
                NecroArt.Shadow(),root.sortingOrder-3,body.transform);
            shadow.transform.localPosition=Vector3.zero;
            Fit(shadow,.70f,.19f);
            shadow.color=new Color(.16f,.13f,.23f,.43f);
        }

        // Sanctuary keeps the full portrait-room camera, so its painted
        // protagonist needs a distinct *visual-only* scale. Never alter actor
        // radius, speed or combat artwork; feet stay on the same floor anchor.
        void SetSanctuaryHeroAppearance(bool sanctuaryEnabled)
        {
            if(witchArtRenderer==null)return;
            witchArtRenderer.transform.localScale=heroineNormalScale*
                (sanctuaryEnabled?SanctuaryHeroScale:1f);
        }

        static int DirectionFrom(Vector2 move)
        {
            float angle=Mathf.Atan2(-move.x,-move.y)*Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle/45f)+8)%8;
        }

        void AnimateHeroVisual(float dt)
        {
            heroineHitPulse=Mathf.Max(0f,heroineHitPulse-dt);
            heroineStrikePulse=Mathf.Max(0f,heroineStrikePulse-dt);
            heroineRaisePulse=Mathf.Max(0f,heroineRaisePulse-dt);
            if(heroineWalking)
            {
                walkDirection=DirectionFrom(facing);
                walkClock+=dt;
            }
            else walkClock=0;

            int frame;
            bool flip;
            float strikeElapsed=StrikeVisualDuration-heroineStrikePulse;
            float raiseElapsed=RaiseVisualDuration-heroineRaisePulse;
            float visualLean=0f;
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
                visualLean=Mathf.Sin(Mathf.Clamp01(elapsed/CastDuration)*Mathf.PI)*.035f;
            }
            else if(heroineRaisePulse>0f)
            {
                // Reuse the approved spectral casting poses for a distinct
                // necromancy gesture; animation only, no gameplay lock.
                frame=raiseElapsed<.18f?8:
                    raiseElapsed<.43f?9:raiseElapsed<.68f?10:11;
                flip=facing.x<-.12f;
                visualLean=-.025f*Mathf.Sin(
                    Mathf.Clamp01(raiseElapsed/RaiseVisualDuration)*Mathf.PI);
            }
            else if(heroineStrikePulse>0f)
            {
                // A short forward lunge, then recoil. Strike damage remains
                // instant and the existing 0.47 second cooldown is unchanged.
                float phase=Mathf.Clamp01(strikeElapsed/StrikeVisualDuration);
                frame=phase<.28f?24+walkDirection:
                    phase<.72f?16+walkDirection:24+walkDirection;
                flip=false;
                if(SelectedCharacter.previewMotion && phase>=.28f && phase<.72f &&
                   Mathf.Abs(facing.x)>.5f)
                {
                    frame=13;
                    flip=facing.x<0f;
                }
                visualLean=.095f*Mathf.Sin(phase*Mathf.PI);
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
                if(SelectedCharacter.previewMotion)
                {
                    // Keep asymmetric costume panels stable; do not mirror an
                    // entire directional pose halfway through the walk cycle.
                    flip=false;
                    // Alternate contact and passing poses in all eight directions.
                    // Each cell is a replacement, never an overlay of two bodies.
                    frame=phase==0?walkDirection:
                        phase==2?16+walkDirection:24+walkDirection;
                }
            }
            witchArtRenderer.sprite=heroSprites[frame];
            witchArtRenderer.flipX=flip;
            bool invulnerable=!inSanctuary &&
                Time.time<heroDamageImmuneUntil;
            bool pulseVisible=invulnerable && Mathf.Sin(Time.time*22f)>0f;
            witchArtRenderer.color=heroineHitPulse>0f ?
                new Color(1f,.72f,.78f) :
                pulseVisible?new Color(1f,.82f,.90f,.82f):Color.white;
            // Move only the drawn layer, preserving the gameplay collider,
            // contact shadow, camera and the sanctuary's visual-only scale.
            Vector3 pose=witchArtRenderer.transform.localPosition;
            pose.x=visualLean*(facing.x<0f?-1f:1f);
            pose.y=-HeroPaintedFootOffset/player.body.lossyScale.y;
            if(SelectedCharacter.previewMotion && heroineWalking &&
               heroineCastPulse<=0f && heroineStrikePulse<=0f && heroineRaisePulse<=0f)
                pose.y+=Mathf.Abs(Mathf.Sin(walkClock*7.5f*Mathf.PI))*.018f;
            witchArtRenderer.transform.localPosition=pose;
        }
    }
}

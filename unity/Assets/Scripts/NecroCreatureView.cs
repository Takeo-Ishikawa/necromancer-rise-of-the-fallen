using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Cosmetic child renderer for hostile and summoned creatures. Collision,
    /// AI, actor position and save logic remain on the original root Transform.
    /// One component per creature; no dynamically allocated animation frames.
    /// </summary>
    public sealed class NecroCreatureView : MonoBehaviour
    {
        Transform art,halo;
        SpriteRenderer artRenderer,haloRenderer;
        Vector3 lastPosition;
        float phase,impactTimer,attackTimer,bodyScale,frameClock;
        bool friendly,flying,boss,slime,spider;
        NecroFrameSet frames;
        NecroPose currentPose=NecroPose.Idle;
        NecroDirection currentDirection=NecroDirection.Down;
        Vector3 animationScale=Vector3.one;
        Color baseTint;
        string type;

        static Sprite[] pupFrames, alliedPupFrames, fallenPupFrames;
        const float FallenHold = 14f;
        const float DissolveDuration = .8f;
        int pupAction;
        int ratFacing8;
        bool fallen;
        float fallenAt;
        bool corpseMode,corpseAutoCleanup;
        float corpseAge,corpseRemaining,revivalClock;
        const float CorpseSettleSeconds=.56f;
        const float ReanimationSeconds=.70f;

        static Sprite[] LoadPupFrames(string path)
        {
            var sheet=Resources.Load<Texture2D>(path);
            var sprites=new Sprite[4];
            for(int i=0;i<sprites.Length;i++)
                sprites[i]=Sprite.Create(sheet,new Rect(i*256,0,256,256),
                    new Vector2(.5f,.31f),256f,0,SpriteMeshType.FullRect);
            return sprites;
        }

        public static Sprite PupIdle(bool ally=false)
        {
            if(ally)
            {
                if(alliedPupFrames==null)alliedPupFrames=LoadPupFrames("Creatures/grave_pup_ally");
                return alliedPupFrames[0];
            }
            if(pupFrames==null)pupFrames=LoadPupFrames("Creatures/grave_pup");
            return pupFrames[0];
        }

        public bool CanBind => fallen && !friendly && Time.time-fallenAt<FallenHold;
        public float FallenRemaining => CanBind?
            Mathf.Max(0f,FallenHold-(Time.time-fallenAt)):0f;

        public void Fall()
        {
            if(fallenPupFrames==null)fallenPupFrames=LoadPupFrames("Creatures/grave_pup_fallen");
            fallen=true;
            fallenAt=Time.time;
            art.localPosition=Vector3.zero;
            art.localScale=Vector3.one;
            art.localRotation=Quaternion.identity;
            // Clear attack telegraph visuals before starting corpse frames.
            haloRenderer.color=new Color(.05f,.07f,.09f,.38f);
            artRenderer.sprite=fallenPupFrames[0];
            artRenderer.color=Color.white;
            // Draw the body beneath living actors and their shadows.
            artRenderer.sortingOrder=2;
            haloRenderer.sortingOrder=1;
        }

        void AnimateFallenPup()
        {
            float elapsed=Time.time-fallenAt-FallenHold;
            if(elapsed<0f)return;
            if(elapsed>=DissolveDuration)
            {
                Destroy(gameObject);
                return;
            }
            float progress=elapsed/DissolveDuration;
            int frame=1+Mathf.Min(2,(int)(progress*3f));
            artRenderer.sprite=fallenPupFrames[frame];
            artRenderer.color=new Color(1f,1f,1f,Mathf.Clamp01((1f-progress)*3f));
            Color shadow=haloRenderer.color;
            shadow.a=.38f*(1f-progress);
            haloRenderer.color=shadow;
        }

        // Ordinary enemies keep their original illustration, with a short
        // collapse and a subdued body left behind for a real Raise ritual.
        // Summoned allies fade away instead of leaving recyclable corpses.
        public void FallToCorpse(float remaining,bool restored=false)
        {
            if(fallen || corpseMode)return;
            corpseMode=true;
            corpseAutoCleanup=remaining<=0f;
            corpseRemaining=remaining;
            corpseAge=restored?CorpseSettleSeconds:0f;
            attackTimer=0f;
            impactTimer=0f;
            revivalClock=0f;
            artRenderer.sortingOrder=2;
            haloRenderer.sortingOrder=1;
        }

        public void SetCorpseRemaining(float remaining)
        {
            corpseRemaining=Mathf.Max(0f,remaining);
        }

        public void Reanimate()
        {
            if(fallen || corpseMode)return;
            revivalClock=ReanimationSeconds;
        }

        void AnimateRatCorpse(float dt)
        {
            corpseAge+=dt;
            float ease=Mathf.Clamp01(corpseAge/NecroRatAtlas.DeathDuration);
            Sprite frame=NecroRatAtlas.Death(corpseAge);
            if(frame!=null)artRenderer.sprite=frame;
            artRenderer.flipX=ratFacing8==5 || ratFacing8==6 || ratFacing8==7;
            art.localRotation=Quaternion.identity;
            art.localPosition=Vector3.down*.09f;
            art.localScale=animationScale;

            float alpha=corpseAutoCleanup ?
                Mathf.Clamp01((.95f-corpseAge)/.40f) :
                Mathf.Clamp01(corpseRemaining/.85f);
            Color tone=Color.Lerp(baseTint,new Color(.54f,.53f,.62f,1f),ease);
            tone.a=alpha;
            artRenderer.color=tone;
            haloRenderer.color=new Color(.06f,.07f,.09f,.30f*alpha);
            if(corpseAutoCleanup && corpseAge>=.95f)
                Destroy(gameObject);
        }

        void AnimateOrdinaryCorpse(float dt)
        {
            if(type=="rat" && NecroRatAtlas.HasDeath)
            {
                AnimateRatCorpse(dt);
                return;
            }
            corpseAge+=dt;
            float ratio=Mathf.Clamp01(corpseAge/CorpseSettleSeconds);
            float ease=ratio*ratio*(3f-2f*ratio);
            if(frames!=null)
                artRenderer.sprite=frames.Frame(NecroPose.Hurt,
                    currentDirection,Mathf.Min(corpseAge,.18f),false);
            float angle=currentDirection==NecroDirection.Left?-54f:54f;
            art.localRotation=Quaternion.Euler(0f,0f,angle*ease);
            art.localPosition=new Vector3(0f,-.17f*ease,0f);
            art.localScale=new Vector3(
                animationScale.x*(1f+.10f*ease),
                animationScale.y*(1f-.46f*ease),1f);
            float alpha=corpseAutoCleanup ?
                Mathf.Clamp01((.95f-corpseAge)/.40f) :
                Mathf.Clamp01(corpseRemaining/.85f);
            Color tone=Color.Lerp(baseTint,
                new Color(.54f,.53f,.62f,1f),ease);
            tone.a=alpha;
            artRenderer.color=tone;
            haloRenderer.color=new Color(.06f,.07f,.09f,
                .34f*(1f-.45f*ease)*alpha);
            if(corpseAutoCleanup && corpseAge>=.95f)
                Destroy(gameObject);
        }

        void ApplyReanimationVisual(float dt)
        {
            if(revivalClock<=0f)return;
            float ratio=1f-Mathf.Clamp01(revivalClock/ReanimationSeconds);
            float ease=ratio*ratio*(3f-2f*ratio);
            art.localScale*=Mathf.Lerp(.68f,1f,ease);
            Color before=artRenderer.color;
            artRenderer.color=Color.Lerp(
                new Color(.40f,.99f,.82f,.48f),before,ease);
            Color haloTint=haloRenderer.color;
            haloTint.a=Mathf.Max(haloTint.a,.66f*(1f-ease));
            haloRenderer.color=haloTint;
            revivalClock=Mathf.Max(0f,revivalClock-dt);
        }

        public void SetPupAction(int pose,Vector2 heading)
        {
            pupAction=pose;
            if(Mathf.Abs(heading.x)>.01f)artRenderer.flipX=heading.x<0;
        }

        public static NecroCreatureView Attach(SpriteRenderer original,string kind,bool ally)
        {
            NecroCreatureView v=original.gameObject.AddComponent<NecroCreatureView>();
            v.type=kind;
            v.friendly=ally;
            v.flying=kind=="wisp"||kind=="bat";
            v.boss=kind=="warden"||kind=="grave_lord"||kind=="marsh_lord";
            v.slime=kind=="slime";
            v.spider=kind=="spider";
            v.phase=original.transform.position.x*.47f+original.transform.position.y*.37f;
            v.lastPosition=original.transform.position;
            v.bodyScale=1f;
            v.baseTint=ally?new Color(.82f,.99f,.95f,1f):Color.white;

            GameObject body=new GameObject("Animated Creature Artwork");
            body.transform.SetParent(original.transform,false);
            v.art=body.transform;
            v.artRenderer=body.AddComponent<SpriteRenderer>();
            v.artRenderer.sprite=original.sprite;
            v.artRenderer.sortingOrder=original.sortingOrder;
            v.artRenderer.color=v.baseTint;
            original.enabled=false;
            if(kind!="grave_pup" && !(kind=="rat" && NecroRatAtlas.HasWalk))
                v.frames=NecroFrameLibrary.Get(kind,original.sprite);
            if(v.frames!=null)
            {
                Vector2 normalSize=original.sprite.bounds.size;
                Vector2 sheetSize=v.frames.Frame(
                    NecroPose.Idle,NecroDirection.Down,0f,true).bounds.size;
                v.animationScale=new Vector3(
                    normalSize.x/sheetSize.x,normalSize.y/sheetSize.y,1f);
            }

            if(kind=="rat" && NecroRatAtlas.HasWalk)
            {
                Sprite ratIdle=NecroRatAtlas.Walk(0,0f,false);
                Vector2 originalSize=original.sprite.bounds.size;
                Vector2 atlasSize=ratIdle.bounds.size;
                v.animationScale=new Vector3(
                    originalSize.x/atlasSize.x,originalSize.y/atlasSize.y,1f);
            }

            GameObject circle=new GameObject(ally?"Summoned Soul Mark":"Creature Shadow");
            circle.transform.SetParent(original.transform,false);
            v.halo=circle.transform;
            v.haloRenderer=circle.AddComponent<SpriteRenderer>();
            v.haloRenderer.sprite=ally?NecroArt.RitualCircle():NecroArt.Shadow();
            v.haloRenderer.sortingOrder=original.sortingOrder-1;
            // Pup paws end 16 pixels above the atlas cell bottom, with a 31% pivot.
            float shadowY=kind=="grave_pup" && !ally ? -.22f :
                kind=="rat" && NecroRatAtlas.HasWalk ? -.12f : -.32f;
            v.halo.localPosition=new Vector3(0,shadowY,0);
            v.halo.localScale=new Vector3(v.boss ? 1.55f : 1.05f,.34f,1);
            v.haloRenderer.color=ally?new Color(.50f,.91f,.80f,.38f):
                new Color(.05f,.07f,.09f,.38f);
            return v;
        }

        public void Flash()
        {
            impactTimer=.21f;
        }
        public void Strike()
        {
            Strike(Vector2.zero);
        }

        public void Strike(Vector2 direction)
        {
            // Standing attackers should face their actual target.
            if(direction.sqrMagnitude>.0001f)
            {
                currentDirection=Direction(direction,currentDirection);
                if(type=="rat")
                    ratFacing8=NecroRatAtlas.Facing8(direction,ratFacing8);
                if(frames==null && Mathf.Abs(direction.x)>.01f)
                    artRenderer.flipX=direction.x<0f;
            }
            currentPose=NecroPose.Attack;
            frameClock=0f;
            attackTimer=type=="rat" && NecroRatAtlas.HasWalk && NecroRatAtlas.HasAttack ?
                NecroRatAtlas.AttackDuration :
                frames!=null ? Mathf.Min(.32f,frames.Duration(NecroPose.Attack)) : .24f;
        }

        void LateUpdate()
        {
            if(art==null)return;
            if(fallen)
            {
                AnimateFallenPup();
                return;
            }
            float dt=Time.deltaTime;
            if(corpseMode)
            {
                AnimateOrdinaryCorpse(dt);
                return;
            }
            impactTimer=Mathf.Max(0,impactTimer-dt);
            attackTimer=Mathf.Max(0,attackTimer-dt);
            float t=Time.time+phase;
            Vector3 at=transform.position;
            Vector2 delta=at-lastPosition;
            bool moving=delta.sqrMagnitude>.0000015f;
            lastPosition=at;

            if(type=="grave_pup")
            {
                bool bracing=pupAction==2;
                bool lunging=pupAction==3;
                int pose=pupAction!=0?pupAction:
                    moving && Mathf.Sin(t*13f)>0?1:0;
                artRenderer.sprite=friendly?alliedPupFrames[pose]:pupFrames[pose];
                if(pupAction==0 && Mathf.Abs(delta.x)>.0001f)
                    artRenderer.flipX=delta.x<0;
                float lift=lunging?.14f:
                    moving && pupAction==0?.024f*Mathf.Abs(Mathf.Sin(t*13f)):0f;
                // Existing ground circle signals the windup, no new objects.
                float hurtSquash=impactTimer>0f?.93f:1f;
                art.localScale=new Vector3(
                    bracing?1.07f:lunging?1.04f:1f,
                    (bracing?.89f:lunging?1.09f:1f)*hurtSquash,1f);
                art.localPosition=Vector3.up*(bracing?-.045f:lift);
                artRenderer.color=impactTimer>0f ?
                    (friendly?new Color(.70f,.97f,1f):new Color(1f,.58f,.63f)):
                    baseTint;
                float ringAlpha=bracing?.46f+.15f*
                    (.5f+.5f*Mathf.Sin(t*18f)):
                    friendly?.34f:.38f;
                haloRenderer.color=bracing ?
                    (friendly?new Color(.49f,.98f,.83f,ringAlpha):
                        new Color(1f,.42f,.37f,ringAlpha)):
                    (friendly?new Color(.50f,.91f,.80f,ringAlpha):
                        new Color(.05f,.07f,.09f,ringAlpha));
                if(friendly)halo.localRotation=Quaternion.Euler(0f,0f,t*18f);
                ApplyReanimationVisual(dt);
                return;
            }
            if(type=="rat" && NecroRatAtlas.HasWalk)
            {
                if(moving && attackTimer<=0f)
                    ratFacing8=NecroRatAtlas.Facing8(delta,ratFacing8);
                bool ratAttacking=attackTimer>0f && NecroRatAtlas.HasAttack;
                Sprite ratSprite;
                if(ratAttacking)
                {
                    frameClock+=dt;
                    ratSprite=NecroRatAtlas.Attack(frameClock);
                }
                else
                {
                    frameClock=0f;
                    ratSprite=NecroRatAtlas.Walk(ratFacing8,t,moving);
                }
                if(ratSprite!=null)
                {
                    artRenderer.sprite=ratSprite;
                    artRenderer.flipX=ratAttacking &&
                        (ratFacing8==5||ratFacing8==6||ratFacing8==7);
                    art.localRotation=Quaternion.identity;
                    art.localPosition=new Vector3(0,
                        moving && !ratAttacking ? .015f*Mathf.Sin(t*14f) : 0f,0);
                    float scale=impactTimer>0f?1.035f:1f;
                    art.localScale=animationScale*scale;
                    artRenderer.color=impactTimer>0f?
                        (friendly?new Color(.90f,.99f,1f):
                            new Color(1f,.65f,.67f)):baseTint;
                    if(friendly)
                    {
                        float pulse=.24f+.13f*(.5f+.5f*Mathf.Sin(t*3f));
                        haloRenderer.color=new Color(.51f,.91f,.81f,pulse);
                        halo.localRotation=Quaternion.Euler(0,0,t*18f);
                    }
                    ApplyReanimationVisual(dt);
                    return;
                }
            }

            if(moving && attackTimer<=0f)
                currentDirection=Direction(delta,currentDirection);

            NecroPose next=impactTimer>0f?NecroPose.Hurt:
                attackTimer>0f?NecroPose.Attack:
                moving?NecroPose.Walk:NecroPose.Idle;
            if(next!=currentPose){currentPose=next;frameClock=0f;}
            else frameClock+=dt;

            float bob=(flying ? .055f : slime ? .020f : boss ? .016f : .022f)*
                Mathf.Sin(t*(flying?5.2f:moving?12f:2.5f));
            float squash=slime ? .035f*Mathf.Sin(t*(moving?11f:3.4f)) : 0f;
            if(frames!=null)
            {
                artRenderer.sprite=frames.Frame(currentPose,currentDirection,
                    frameClock,currentPose==NecroPose.Idle||
                    currentPose==NecroPose.Walk);
                artRenderer.flipX=false;
                art.localRotation=Quaternion.identity;
                art.localPosition=new Vector3(0,bob,0);
            }
            else
            {
                // Species without frame atlases retain their lightweight
                // movement animation and still respond to attack events.
                float swing=(spider ? 3.4f : boss ? .55f : 2.1f)*
                    Mathf.Sin(t*(moving?9f:2f));
                art.localRotation=Quaternion.Euler(0,0,
                    swing+(attackTimer>0f?Mathf.Sin(attackTimer*30f)*8f:0f));
                art.localPosition=new Vector3(attackTimer > 0f ? .07f : 0f,bob,0);
                if(attackTimer<=0f)
                {
                    if(delta.x>.0001f)artRenderer.flipX=false;
                    else if(delta.x<-.0001f)artRenderer.flipX=true;
                }
            }

            float attackScale=attackTimer>0f ? .045f : 0f;
            float recoilScale=impactTimer>0f ? .035f : 0f;
            art.localScale=new Vector3(
                animationScale.x*(bodyScale+squash+attackScale+recoilScale),
                animationScale.y*(bodyScale-squash+attackScale-recoilScale),1f);

            artRenderer.color=impactTimer>0f?
                friendly?new Color(.90f,.99f,1f,1f):
                    new Color(1f,.52f,.60f,1f):baseTint;

            if(friendly)
            {
                float pulse=.24f+.13f*(.5f+.5f*Mathf.Sin(t*3f));
                haloRenderer.color=new Color(.51f,.91f,.81f,pulse);
                halo.localRotation=Quaternion.Euler(0,0,t*18f);
            }
            if(flying)halo.localScale=new Vector3(1.05f,.25f,1);
            ApplyReanimationVisual(dt);
        }

        static NecroDirection Direction(Vector2 movement,NecroDirection previous)
        {
            if(movement.sqrMagnitude<.00001f)return previous;
            if(Mathf.Abs(movement.x)>Mathf.Abs(movement.y)*1.2f)
                return movement.x<0f?NecroDirection.Left:NecroDirection.Right;
            return movement.y<0f?NecroDirection.Down:NecroDirection.Up;
        }

    }

}

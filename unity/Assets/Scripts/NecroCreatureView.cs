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

        static Sprite[] pupFrames;
        int pupAction;
        public static Sprite PupIdle()
        {
            if(pupFrames==null)
            {
                var sheet=Resources.Load<Texture2D>("Creatures/grave_pup");
                pupFrames=new Sprite[4];
                for(int i=0;i<4;i++)
                    pupFrames[i]=Sprite.Create(sheet,new Rect(i*256,0,256,256),
                        new Vector2(.5f,.31f),256f,0,SpriteMeshType.FullRect);
            }
            return pupFrames[0];
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
            if(kind!="grave_pup")v.frames=NecroFrameLibrary.Get(kind,original.sprite);
            if(v.frames!=null)
            {
                Vector2 normalSize=original.sprite.bounds.size;
                Vector2 sheetSize=v.frames.Frame(
                    NecroPose.Idle,NecroDirection.Down,0f,true).bounds.size;
                v.animationScale=new Vector3(
                    normalSize.x/sheetSize.x,normalSize.y/sheetSize.y,1f);
            }

            GameObject circle=new GameObject(ally?"Summoned Soul Mark":"Creature Shadow");
            circle.transform.SetParent(original.transform,false);
            v.halo=circle.transform;
            v.haloRenderer=circle.AddComponent<SpriteRenderer>();
            v.haloRenderer.sprite=ally?NecroArt.RitualCircle():NecroArt.Shadow();
            v.haloRenderer.sortingOrder=original.sortingOrder-1;
            // Pup paws end 16 pixels above the atlas cell bottom, with a 31% pivot.
            float shadowY=kind=="grave_pup" && !ally ? -.22f : -.32f;
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
            attackTimer=.20f;
        }

        void LateUpdate()
        {
            if(art==null)return;
            float dt=Time.deltaTime;
            impactTimer=Mathf.Max(0,impactTimer-dt);
            attackTimer=Mathf.Max(0,attackTimer-dt);
            float t=Time.time+phase;
            Vector3 at=transform.position;
            Vector2 delta=at-lastPosition;
            bool moving=delta.sqrMagnitude>.0000015f;
            lastPosition=at;

            if(type=="grave_pup")
            {
                int pose=pupAction!=0?pupAction:moving && Mathf.Sin(t*13f)>0?1:0;
                artRenderer.sprite=pupFrames[pose];
                if(pupAction==0 && Mathf.Abs(delta.x)>.0001f)artRenderer.flipX=delta.x<0;
                float lift=pupAction==3?.14f:moving && pupAction==0?.024f*Mathf.Abs(Mathf.Sin(t*13f)):0f;
                art.localPosition=Vector3.up*lift;
                artRenderer.color=impactTimer>0?new Color(1,.65f,.68f):baseTint;
                return;
            }
            if(moving)
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
                if(delta.x>.0001f)artRenderer.flipX=false;
                else if(delta.x<-.0001f)artRenderer.flipX=true;
            }

            float attackScale=attackTimer > 0f ? .02f : 0f;
            art.localScale=new Vector3(animationScale.x*(bodyScale+squash),
                animationScale.y*(bodyScale-squash+attackScale),1f);

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

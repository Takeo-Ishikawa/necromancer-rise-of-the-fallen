using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Frame-based state animation using the art of the necromante.
    /// The renderer's root transform remains the authoritative position,
    /// collision, camera target and persistent save anchor.
    /// </summary>
    public partial class NecroPrototype
    {
        Transform witchArtTransform,ritualCircleTransform;
        SpriteRenderer witchArtRenderer,ritualCircleRenderer;
        NecroFrameSet heroineFrames;
        NecroPose heroinePose=NecroPose.Idle;
        NecroDirection heroineDirection=NecroDirection.Down;
        float heroineClock,heroineCastPulse,heroineRaisePulse;
        float heroineStrikePulse,heroineHitPulse;
        bool heroineWalking;

        void InitializeHeroAnimation()
        {
            if(player==null || player.body==null)return;
            SpriteRenderer rootRenderer=player.body.GetComponent<SpriteRenderer>();
            if(rootRenderer==null)return;

            GameObject image=new GameObject("Necromancer / directional frames");
            image.transform.SetParent(player.body,false);
            witchArtTransform=image.transform;
            witchArtRenderer=image.AddComponent<SpriteRenderer>();
            witchArtRenderer.sprite=rootRenderer.sprite;
            witchArtRenderer.sortingOrder=rootRenderer.sortingOrder;
            rootRenderer.enabled=false;

            // The sprite set contains actual distinct sprites/frames. It is
            // generated only once, then shared across future play sessions.
            heroineFrames=NecroFrameLibrary.Get("necromante",rootRenderer.sprite);
            if(heroineFrames!=null)
            {
                Vector2 original=rootRenderer.sprite.bounds.size;
                Vector2 replacement=heroineFrames.Frame(
                    NecroPose.Idle,NecroDirection.Down,0,true).bounds.size;
                // Slightly larger visual size, without changing collisions.
                // The hair, eyes and skull stay readable on a small handset.
                witchArtTransform.localScale=new Vector3(
                    original.x/replacement.x*1.13f,
                    original.y/replacement.y*1.13f,1);
            }

            GameObject circle=new GameObject("Necromantic rune circle");
            circle.transform.SetParent(player.body,false);
            ritualCircleTransform=circle.transform;
            ritualCircleRenderer=circle.AddComponent<SpriteRenderer>();
            ritualCircleRenderer.sprite=NecroArt.RitualCircle();
            ritualCircleRenderer.sortingOrder=witchArtRenderer.sortingOrder-2;
            ritualCircleTransform.localPosition=new Vector3(0,-.34f,0);
            ritualCircleTransform.localScale=new Vector3(1.16f,.46f,1);
            ritualCircleRenderer.color=new Color(.54f,.90f,.87f,.20f);
        }

        static NecroDirection DirectionFrom(Vector2 movement,NecroDirection previous)
        {
            if(movement.sqrMagnitude<.012f)return previous;
            if(Mathf.Abs(movement.x)>Mathf.Abs(movement.y)*1.2f)
                return movement.x<0f?NecroDirection.Left:NecroDirection.Right;
            return movement.y<0f?NecroDirection.Down:NecroDirection.Up;
        }

        void AnimateHeroVisual(float dt)
        {
            if(witchArtTransform==null)return;
            heroineCastPulse=Mathf.Max(0,heroineCastPulse-dt);
            heroineRaisePulse=Mathf.Max(0,heroineRaisePulse-dt);
            heroineStrikePulse=Mathf.Max(0,heroineStrikePulse-dt);
            heroineHitPulse=Mathf.Max(0,heroineHitPulse-dt);

            if(heroineWalking)
                heroineDirection=DirectionFrom(facing,heroineDirection);

            NecroPose next=heroineHitPulse>0f?NecroPose.Hurt:
                heroineRaisePulse>0f?NecroPose.Raise:
                heroineCastPulse>0f?NecroPose.Cast:
                heroineStrikePulse>0f?NecroPose.Attack:
                heroineWalking?NecroPose.Walk:NecroPose.Idle;
            if(next!=heroinePose)
            {
                heroinePose=next;
                heroineClock=0f;
            }
            else heroineClock+=dt;

            if(heroineFrames!=null)
            {
                witchArtRenderer.sprite=heroineFrames.Frame(
                    heroinePose,heroineDirection,heroineClock,
                    heroinePose==NecroPose.Idle||heroinePose==NecroPose.Walk);
                witchArtRenderer.flipX=false;
                witchArtTransform.localRotation=Quaternion.identity;
                witchArtTransform.localPosition=Vector3.zero;
            }
            else
            {
                // Fallback for older Unity projects missing an animation atlas.
                float stride=heroineWalking?Mathf.Sin(Time.time*12f)*.055f:0f;
                witchArtTransform.localPosition=new Vector3(0,stride,0);
                witchArtRenderer.flipX=heroineDirection==NecroDirection.Left;
            }

            witchArtRenderer.color=heroineHitPulse>0f?
                new Color(1f,.70f,.77f,1f):
                heroineRaisePulse>0f?new Color(.90f,1f,.98f,1f):
                Color.white;

            if(ritualCircleRenderer!=null)
            {
                float strength=heroineRaisePulse > 0f ? .85f :
                    heroineCastPulse > 0f ? .51f : .14f;
                ritualCircleRenderer.color=new Color(.58f,.88f,1f,strength);
                ritualCircleTransform.localRotation=Quaternion.Euler(0,0,
                    Time.time*(heroineRaisePulse>0f?58f:18f));
            }
        }
    }
}

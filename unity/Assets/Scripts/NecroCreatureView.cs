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
        float phase,impactTimer,attackTimer,bodyScale;
        bool friendly,flying,boss,slime,spider;
        Color baseTint;
        string type;

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

            GameObject circle=new GameObject(ally?"Summoned Soul Mark":"Creature Shadow");
            circle.transform.SetParent(original.transform,false);
            v.halo=circle.transform;
            v.haloRenderer=circle.AddComponent<SpriteRenderer>();
            v.haloRenderer.sprite=ally?NecroArt.RitualCircle():NecroArt.Shadow();
            v.haloRenderer.sortingOrder=original.sortingOrder-1;
            v.halo.localPosition=new Vector3(0,-.32f,0);
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

            float bob=(flying ? .11f : slime ? .025f : boss ? .020f : .035f)*
                Mathf.Sin(t*(flying?5.2f:moving?12f:2.5f));
            float swing=(spider ? 3.4f : boss ? .55f : 2.1f)*
                Mathf.Sin(t*(moving?9f:2f));
            float squash=slime ? .075f*Mathf.Sin(t*(moving?11f:3.4f)) : 0f;
            float flutter=flying && type=="bat" ? .09f*Mathf.Sin(t*16f):0f;

            art.localPosition=new Vector3(attackTimer > 0f ? .07f : 0f,bob,0);
            art.localRotation=Quaternion.Euler(0,0,
                swing+(attackTimer>0?Mathf.Sin(attackTimer*30f)*8f:0f));
            art.localScale=new Vector3(bodyScale+squash+flutter,
                bodyScale-squash+(attackTimer > 0f ? .03f : 0f),1);
            if(delta.x>0.0001f)artRenderer.flipX=false;
            else if(delta.x<-0.0001f)artRenderer.flipX=true;

            if(impactTimer>0)
                artRenderer.color=friendly?new Color(.90f,.99f,1,1):
                    new Color(1,.52f,.60f,1);
            else artRenderer.color=baseTint;

            if(friendly)
            {
                float pulse=.24f+.13f*(.5f+.5f*Mathf.Sin(t*3f));
                haloRenderer.color=new Color(.51f,.91f,.81f,pulse);
                halo.localRotation=Quaternion.Euler(0,0,t*18f);
            }
            if(flying)halo.localScale=new Vector3(1.05f,.25f,1);
        }
    }

}

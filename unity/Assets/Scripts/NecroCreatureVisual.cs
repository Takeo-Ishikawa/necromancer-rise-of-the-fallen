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
            v.halo.localScale=new Vector3(boss?1.55f:1.05f,.34f,1);
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

            float bob=(flying?.11f:slime?.025f:boss?.020f:.035f)*
                Mathf.Sin(t*(flying?5.2f:moving?12f:2.5f));
            float swing=(spider?3.4f:boss?.55f:2.1f)*
                Mathf.Sin(t*(moving?9f:2f));
            float squash=slime ? .075f*Mathf.Sin(t*(moving?11f:3.4f)) : 0f;
            float flutter=flying && type=="bat" ? .09f*Mathf.Sin(t*16f):0f;

            art.localPosition=new Vector3(attackTimer>0?.07f:0,bob,0);
            art.localRotation=Quaternion.Euler(0,0,
                swing+(attackTimer>0?Mathf.Sin(attackTimer*30f)*8f:0f));
            art.localScale=new Vector3(bodyScale+squash+flutter,
                bodyScale-squash+(attackTimer>0?.03f:0),1);
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

    /// <summary>
    /// Short-lived combat effects. Visual-only: they are never included in saves.
    /// Limit the number of simultaneous effects on low-end Android devices.
    /// </summary>
    public sealed class NecroCombatFx : MonoBehaviour
    {
        const int Capacity=30;
        static int active;
        SpriteRenderer sprite;
        float remaining,duration,rotationSpeed;
        Vector2 drift;
        Color tint;
        Vector3 size;

        public static void Emit(Vector2 position,Transform parent,int type)
        {
            if(active>=Capacity)return;
            GameObject obj=new GameObject(type==1?"Soul Reanimation":
                type==2?"Guardian Soul Release":"Hit");
            if(parent!=null)obj.transform.SetParent(parent,false);
            obj.transform.position=new Vector3(position.x,position.y,0);
            NecroCombatFx fx=obj.AddComponent<NecroCombatFx>();
            fx.duration=type==1?.76f:type==2?1.1f:.30f;
            fx.remaining=fx.duration;
            fx.rotationSpeed=type==0?90f:type==1?75f:-52f;
            fx.drift=type==0?new Vector2(.04f,.13f):
                new Vector2(0,.25f);
            fx.size=type==0?new Vector3(.38f,.38f,1):
                type==1?new Vector3(1.0f,.68f,1):new Vector3(1.5f,1.1f,1);
            fx.tint=type==0?new Color(.98f,.76f,.77f,.85f):
                type==1?new Color(.55f,.97f,.86f,.85f):
                new Color(.71f,.65f,.97f,.85f);
            fx.sprite=obj.AddComponent<SpriteRenderer>();
            fx.sprite.sprite=type==0?NecroArt.Effect():NecroArt.RitualCircle();
            fx.sprite.sortingOrder=18;
            fx.sprite.color=fx.tint;
            obj.transform.localScale=fx.size*.70f;
            active++;
        }

        void Update()
        {
            remaining-=Time.deltaTime;
            if(remaining<=0){Destroy(gameObject);return;}
            float elapsed=1f-remaining/duration;
            transform.position+=(Vector3)(drift*Time.deltaTime);
            transform.Rotate(0,0,rotationSpeed*Time.deltaTime);
            transform.localScale=size*Mathf.Lerp(.7f,1.34f,elapsed);
            Color c=tint;
            c.a=tint.a*(1f-elapsed)*(1f-elapsed);
            sprite.color=c;
        }

        void OnDestroy()
        {
            active=Mathf.Max(0,active-1);
        }
    }
}

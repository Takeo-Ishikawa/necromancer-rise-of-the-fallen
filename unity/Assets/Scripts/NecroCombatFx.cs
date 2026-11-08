using UnityEngine;

namespace Necromancer
{
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

        public static void Emit(Vector2 position,Transform parent,int type,
            bool alliedHit=false)
        {
            if(active>=Capacity)return;
            GameObject obj=new GameObject(type==1?"Soul Reanimation":
                type==2?"Guardian Soul Release":
                type==3?"Necrotic Soul Pulse":
                type==4?"Order of the Dead":
                type==5?"Rotboar Charge Warning":
                type==6?"Allied Rotboar Charge Warning":
                type==7?"Departing Soul":
                alliedHit?"Servant Hit":"Hit");
            if(parent!=null)obj.transform.SetParent(parent,false);
            obj.transform.position=new Vector3(position.x,position.y,0);
            NecroCombatFx fx=obj.AddComponent<NecroCombatFx>();
            fx.duration=type==7 ? .60f :
                type==5 || type==6 ? .82f :
                type==4 ? .85f : type==3 ? .52f :
                type == 1 ? .76f : type == 2 ? 1.1f : .30f;
            fx.remaining=fx.duration;
            fx.rotationSpeed=type==5?40f:type==6?-40f:
                type==4?-88f:type==3?105f:type==0?90f:type==1?75f:-52f;
            fx.drift=type==7 ? new Vector2(0f,.65f) :
                type==5 || type==6 || type==4 || type==3 ?
                Vector2.zero :
                type==0 ? new Vector2(.04f,.13f) : new Vector2(0,.25f);
            fx.size=type==7 ? new Vector3(.61f,.61f,1f) :
                type==5 || type==6 ? new Vector3(1.75f,1.12f,1f) :
                type==4 ? new Vector3(1.75f,1.75f,1f) :
                type==3 ? new Vector3(4.0f,4.0f,1f) :
                type==0 ? new Vector3(.38f,.38f,1f) :
                type==1 ? new Vector3(1.0f,.68f,1f) :
                new Vector3(1.5f,1.1f,1f);
            fx.tint=type==7 ?
                    (alliedHit?new Color(.47f,.95f,.78f,.80f):
                        new Color(.76f,.60f,.95f,.83f)) :
                type==5 ? new Color(.99f,.41f,.33f,.70f) :
                type==6 ? new Color(.37f,.92f,.78f,.65f) :
                type==4 ? new Color(.33f,.95f,.76f,.74f) :
                type==3 ? new Color(.62f,.41f,.92f,.78f) :
                type==0 ? (alliedHit?
                    new Color(.63f,.93f,1f,.85f):
                    new Color(.98f,.76f,.77f,.85f)) :
                type==1 ? new Color(.55f,.97f,.86f,.85f) :
                new Color(.71f,.65f,.97f,.85f);
            fx.sprite=obj.AddComponent<SpriteRenderer>();
            fx.sprite.sprite=type==0 || type==7?
                NecroArt.Effect():NecroArt.RitualCircle();
            fx.sprite.sortingOrder=type==7 ? 16 :
                type==5 || type==6 ? 10 :
                type==3?11:type==4?14:18;
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

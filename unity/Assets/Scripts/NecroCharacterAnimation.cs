using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Lightweight Unity 2D animation on top of the illustrated PNG.
    /// The actor transform remains the collision/AI anchor; only its child artwork
    /// bobs, turns, casts and lights up during resurrection.
    /// </summary>
    public partial class NecroPrototype
    {
        Transform witchArtTransform,ritualCircleTransform;
        SpriteRenderer witchArtRenderer,ritualCircleRenderer;
        float heroineCastPulse,heroineRaisePulse;
        bool heroineWalking;

        void InitializeHeroAnimation()
        {
            if(player==null || player.body==null)return;
            SpriteRenderer rootRenderer=player.body.GetComponent<SpriteRenderer>();
            if(rootRenderer==null)return;
            GameObject image=new GameObject("Witch / illustrated sprite / animated");
            image.transform.SetParent(player.body,false);
            witchArtTransform=image.transform;
            witchArtRenderer=image.AddComponent<SpriteRenderer>();
            witchArtRenderer.sprite=rootRenderer.sprite;
            witchArtRenderer.sortingOrder=rootRenderer.sortingOrder;
            rootRenderer.enabled=false;

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

        void AnimateHeroVisual(float dt)
        {
            if(witchArtTransform==null)return;
            heroineCastPulse=Mathf.Max(0,heroineCastPulse-dt);
            heroineRaisePulse=Mathf.Max(0,heroineRaisePulse-dt);
            float stride=heroineWalking ? Mathf.Sin(Time.time*12f)*.055f : 0f;
            float breathing=Mathf.Sin(Time.time*2.3f)*.018f;
            float burst=heroineCastPulse > 0f ? .045f : 0f;
            witchArtTransform.localPosition=new Vector3(0,stride+breathing+burst,0);
            witchArtTransform.localScale=new Vector3(1f+breathing,
                1f+breathing*.32f+(heroineRaisePulse > 0f ? .035f : 0f),1);
            witchArtTransform.localRotation=Quaternion.Euler(0,0,
                Mathf.Sin(Time.time*(heroineWalking?9f:2f))*(heroineWalking?2.5f:1.0f));
            if(Mathf.Abs(facing.x)>.10f)
                witchArtRenderer.flipX=facing.x<0f;
            if(heroineCastPulse>0)
                witchArtRenderer.color=new Color(1f,.92f,1f,1);
            else if(heroineRaisePulse>0)
                witchArtRenderer.color=new Color(.88f,1f,.99f,1);
            else witchArtRenderer.color=Color.white;

            if(ritualCircleRenderer!=null)
            {
                float strength=heroineRaisePulse > 0f ? .78f :
                    heroineCastPulse > 0f ? .42f :.15f;
                ritualCircleRenderer.color=new Color(.58f,.88f,1f,strength);
                ritualCircleTransform.localRotation=Quaternion.Euler(0,0,
                    Time.time*(heroineRaisePulse > 0f ? 58f : 18f));
            }
        }
    }
}

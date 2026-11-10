using System.Collections;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.8: one reusable corpse / soul transition system.
    /// Ordinary enemies leave a temporarily bindable original silhouette.
    /// Allied deaths fade without creating new corpses or save data.
    /// </summary>
    public partial class NecroPrototype
    {
        const float OrdinaryCorpseLifetime=14f;
        const int OrdinaryCorpseLimit=16;
        const float SoulReanimationDuration=.70f;

        void SpawnRecoverableCorpse(Actor victim)
        {
            if(victim==null || victim.body==null)return;
            if(victim.view==null)
            {
                Destroy(victim.body.gameObject);
                return;
            }
            // Lifetime is per visited stage. Reuse the dead actor renderer:
            // no extra PNG, GameObject, texture or collider is allocated.
            victim.view.FallToCorpse(OrdinaryCorpseLifetime);
            current.corpses.Add(new Corpse{
                kind=victim.kind,body=victim.body,
                time=OrdinaryCorpseLifetime,view=victim.view
            });
            // Keep heavy combat scenes bounded even with endless respawns.
            if(current.corpses.Count>OrdinaryCorpseLimit)
            {
                Corpse oldest=current.corpses[0];
                current.corpses.RemoveAt(0);
                if(oldest!=null && oldest.body!=null)
                    Destroy(oldest.body.gameObject);
            }
        }

        Corpse NearbyCorpse()
        {
            if(inSanctuary || player==null || current==null)return null;
            Corpse nearest=null;
            float best=1.3f;
            for(int i=current.corpses.Count-1;i>=0;i--)
            {
                Corpse entry=current.corpses[i];
                if(entry==null || entry.body==null || entry.time<=0f)
                {
                    current.corpses.RemoveAt(i);
                    continue;
                }
                float d=Vector2.Distance(entry.body.position,
                    player.body.position);
                if(d<best && Visible(player.body.position,entry.body.position))
                {
                    best=d;
                    nearest=entry;
                }
            }
            return nearest;
        }

        void AnimateReanimation(Actor servant,Vector2 at)
        {
            if(servant==null || servant.body==null)return;
            servant.reanimationUntil=Time.time+SoulReanimationDuration;
            if(servant.view!=null)servant.view.Reanimate();
            else if(servant.kind=="rotboar")
                StartCoroutine(ReanimateRotboar(servant));
            // Light ritual ring with the project's existing cached sprite.
            if(current!=null && current.root!=null)
                NecroCombatFx.Emit(at,current.root.transform,1);
            heroineRaisePulse=.92f;
        }

        IEnumerator ReanimateRotboar(Actor boar)
        {
            float elapsed=0f;
            while(elapsed<SoulReanimationDuration &&
                  boar!=null && boar.hp>0f && boar.boarArt!=null)
            {
                float ratio=Mathf.Clamp01(elapsed/SoulReanimationDuration);
                float ease=ratio*ratio*(3f-2f*ratio);
                boar.boarArt.transform.localScale=boar.boarVisualScale*
                    Mathf.Lerp(.70f,1f,ease);
                boar.boarArt.color=Color.Lerp(
                    new Color(.43f,.98f,.77f,.52f),Color.white,ease);
                elapsed+=Time.deltaTime;
                yield return null;
            }
            if(boar!=null && boar.hp>0f && boar.boarArt!=null)
            {
                boar.boarArt.transform.localScale=boar.boarVisualScale;
                boar.boarArt.color=Color.white;
            }
        }
    }
}

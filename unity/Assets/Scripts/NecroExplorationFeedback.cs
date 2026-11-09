using UnityEngine;

namespace Necromancer
{
    /// <summary>Pure, editor-testable rules for resource discovery and rewards.</summary>
    public static class NecroExplorationRules
    {
        public const float DiscoveryRadius=7f;

        public static bool CanReveal(float distanceSquared)
        {
            return distanceSquared<=DiscoveryRadius*DiscoveryRadius;
        }

        public static float MarkerScale(float seconds,float offset)
        {
            return 1f+.07f*(.5f+.5f*Mathf.Sin(seconds*3f+offset));
        }

        public static float ActualHealing(float current,float maximum,
            float proposed)
        {
            float available=Mathf.Max(0f,maximum-Mathf.Max(0f,current));
            return Mathf.Min(available,Mathf.Max(0f,proposed));
        }
    }

    /// <summary>
    /// 0.9.16: gentle shimmer on existing pickups, context-aware exploration
    /// text and short reward notifications. No new art, serialized fields or
    /// added player interactions.
    /// </summary>
    public partial class NecroPrototype
    {
        const float ResourceHintRefreshSeconds=.28f;
        const float ResourceToastDuration=2.2f;
        float nextResourceHintRefresh;
        string resourceHint="EXPLORE PARA ENCONTRAR RECURSOS";
        float resourceToastUntil;
        int resourceToastFragments,resourceToastCount,resourceToastMana;
        float resourceToastHealing;

        static string DisplayResourceType(string type)
        {
            if(type=="cristal")return "CRISTAL";
            if(type=="ossos")return "OSSOS";
            if(type=="fungo")return "FUNGO";
            return "RECURSO";
        }

        void RecordResourceReward(float healing,int manaGain)
        {
            if(Time.time>=resourceToastUntil)
            {
                resourceToastFragments=0;
                resourceToastHealing=0f;
                resourceToastMana=0;
                resourceToastCount=0;
            }
            resourceToastFragments+=2;
            resourceToastHealing+=Mathf.Max(0f,healing);
            resourceToastMana+=Mathf.Max(0,manaGain);
            resourceToastCount++;
            resourceToastUntil=Time.time+ResourceToastDuration;
            // Update the nearest-resource hint as soon as a pickup disappears.
            nextResourceHintRefresh=0f;
        }

        void UpdateResourceExploration()
        {
            if(inSanctuary || current==null || player==null ||
                player.body==null)return;
            Vector2 origin=player.body.position;
            foreach(ResourceNode node in current.resources)
            {
                if(node==null || node.body==null || node.artwork==null)continue;
                float separation=((Vector2)node.body.position-origin).sqrMagnitude;
                if(NecroExplorationRules.CanReveal(separation))
                {
                    float size=NecroExplorationRules.MarkerScale(Time.time,
                        node.shimmerOffset);
                    node.body.localScale=node.defaultScale*size;
                    float brightness=.38f+.30f*
                        (.5f+.5f*Mathf.Sin(Time.time*3f+node.shimmerOffset));
                    node.artwork.color=Color.Lerp(node.shimmerTint,
                        Color.white,brightness);
                }
                else
                {
                    node.body.localScale=node.defaultScale;
                    node.artwork.color=Color.white;
                }
            }

            if(Time.time<nextResourceHintRefresh)return;
            nextResourceHintRefresh=Time.time+ResourceHintRefreshSeconds;
            int nearbyHostiles=0;
            foreach(Actor enemy in current.enemies)
            {
                if(enemy==null || enemy.hp<=0f || enemy.body==null)continue;
                Vector2 at=enemy.body.position;
                if((at-origin).sqrMagnitude>12.25f || !Visible(origin,at))
                    continue;
                nearbyHostiles++;
            }
            if(nearbyHostiles>0)
            {
                resourceHint=nearbyHostiles==1?
                    "ALERTA: INIMIGO PROXIMO":
                    "ALERTA: "+nearbyHostiles+" INIMIGOS PROXIMOS";
                return;
            }
            if(current.resources.Count==0)
            {
                resourceHint="TODOS OS RECURSOS COLETADOS";
                return;
            }
            ResourceNode nearest=null;
            float best=NecroExplorationRules.DiscoveryRadius*
                NecroExplorationRules.DiscoveryRadius;
            foreach(ResourceNode node in current.resources)
            {
                if(node==null || node.body==null)continue;
                Vector2 at=node.body.position;
                float separation=(at-origin).sqrMagnitude;
                if(separation>best || !Visible(origin,at))continue;
                best=separation;
                nearest=node;
            }
            resourceHint=nearest==null?
                "EXPLORE PARA ENCONTRAR RECURSOS":
                DisplayResourceType(nearest.type)+" A "+
                Mathf.CeilToInt(Mathf.Sqrt(best))+" m  ·  APROXIME-SE";
        }

        void DrawResourceRewardToast(float width,float y)
        {
            float fade=Mathf.Clamp01(
                (resourceToastUntil-Time.time)/.38f);
            float panelWidth=Mathf.Max(120f,width-78f);
            Rect bounds=new Rect(39f,y,panelWidth,43f);
            DrawWardrobeSurface(bounds,
                new Color(.075f,.15f,.17f,.94f*fade),
                new Color(.58f,.91f,.82f,.91f*fade));
            GUI.Label(new Rect(bounds.x+6f,bounds.y+1f,
                    bounds.width-12f,21f),
                resourceToastCount>1?
                    resourceToastCount+" RECURSOS RECOLHIDOS":
                    "RECURSO RECOLHIDO",hudAction);
            string gains="+"+resourceToastFragments+" FRAGMENTOS";
            if(resourceToastHealing>=.5f)
                gains+="  +"+Mathf.CeilToInt(resourceToastHealing)+" VIDA";
            if(resourceToastMana>0)
                gains+="  +"+resourceToastMana+" ANIMA";
            GUI.Label(new Rect(bounds.x+5f,bounds.y+21f,
                    bounds.width-10f,19f),gains,hudSmall);
        }
    }
}

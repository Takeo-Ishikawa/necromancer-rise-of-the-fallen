using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Remembers the heroine's actual walk through tight caverns so servants
    /// can form a single-file procession around bends instead of occupying
    /// overlapping radial slots. The trail is runtime-only and never saved.
    /// </summary>
    public partial class NecroPrototype
    {
        const int CaveTrailCapacity=160;
        const float CaveTrailSampleStep=.32f;
        readonly Vector2[] caveHeroTrail=new Vector2[CaveTrailCapacity];
        int caveTrailHead,caveTrailCount;
        LevelDefinition caveTrailLevel;

        void RecordCaveHeroTrail()
        {
            if(inSanctuary || !NecroCaveNetwork.HasNetwork(level) ||
                player==null || player.body==null)
            {
                caveTrailCount=0;
                caveTrailLevel=null;
                return;
            }
            Vector2 at=player.body.position;
            if(caveTrailLevel!=level || caveTrailCount==0)
            {
                caveTrailLevel=level;
                caveTrailHead=0;
                caveHeroTrail[0]=at;
                caveTrailCount=1;
                return;
            }
            Vector2 prior=caveHeroTrail[caveTrailHead];
            float distance=Vector2.Distance(prior,at);
            if(distance>6f)
            {
                // The player teleported or a stage was loaded: do not retain
                // breadcrumbs across disconnected worlds.
                caveTrailHead=0;
                caveHeroTrail[0]=at;
                caveTrailCount=1;
                return;
            }
            if(distance<CaveTrailSampleStep)return;
            int steps=Mathf.Min(24,Mathf.CeilToInt(distance/CaveTrailSampleStep));
            for(int i=1;i<=steps;i++)
            {
                caveTrailHead=(caveTrailHead+1)%CaveTrailCapacity;
                caveHeroTrail[caveTrailHead]=Vector2.Lerp(prior,at,
                    (float)i/steps);
                if(caveTrailCount<CaveTrailCapacity)caveTrailCount++;
            }
        }

        bool NarrowCaveAroundHero()
        {
            if(caveTrailCount<2 || player==null || player.body==null)return false;
            Vector2 heading=facing.sqrMagnitude>.01f ?
                facing.normalized : Vector2.right;
            Vector2 lateral=new Vector2(-heading.y,heading.x);
            Vector2 here=player.body.position;
            // Both flanks must be restricted. Near a single wall the
            // ordinary defensive formation remains preferable.
            return Blocked(here+lateral*1.05f,.38f) &&
                Blocked(here-lateral*1.05f,.38f);
        }

        bool TryCaveTrailSpot(Actor ally,int slot,out Vector2 spot)
        {
            spot=Vector2.zero;
            if(caveTrailCount<3 || ally==null)return false;
            float behind=player.radius+ally.radius+.55f;
            // Individual body sizes determine the space reserved along the
            // corridor; a Rotboar takes more room than a Grave Pup.
            for(int i=0;i<slot && i<servants.Count;i++)
                if(servants[i]!=null)
                    behind+=Mathf.Max(1.05f,
                        servants[i].radius+ally.radius+.45f);

            for(int j=0;j<caveTrailCount-1;j++)
            {
                Vector2 newer=caveHeroTrail[
                    (caveTrailHead-j+CaveTrailCapacity)%CaveTrailCapacity];
                Vector2 older=caveHeroTrail[
                    (caveTrailHead-j-1+CaveTrailCapacity)%CaveTrailCapacity];
                float segment=Vector2.Distance(newer,older);
                if(segment<.001f)continue;
                if(behind>segment)
                {
                    behind-=segment;
                    continue;
                }
                Vector2 chosen=Vector2.Lerp(newer,older,behind/segment);
                if(!FormationSpotOpen(ally,chosen))return false;
                spot=chosen;
                return true;
            }
            // Don't invent a trail point in rock when the player has walked
            // less than the required separation distance.
            return false;
        }
    }
}

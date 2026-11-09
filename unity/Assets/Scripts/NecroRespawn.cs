using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Infinite, bounded encounters for already-conquered regions.
    /// Reuses each biome's authored ordinary enemies; guardians are unique.
    /// Only the active scene ticks respawning, so transitions and saved
    /// regions do not spawn off-screen fleets while the player is away.
    /// </summary>
    public partial class NecroPrototype
    {
        const float RespawnInterval=12f; // Default for legacy stage snapshots.
        const float MinEnemySeparation=1.8f;

        void TickRespawns(float dt)
        {
            if(current==null || !current.root.activeInHierarchy)
                return;

            current.respawnTimer-=dt;
            if(current.respawnTimer>0f)return;
            current.respawnTimer=RespawnIntervalForStage();

            int commonSpawnKinds=0;
            foreach(Spawn entry in level.enemies)
                if(!IsBoss(entry.kind))commonSpawnKinds++;

            if(commonSpawnKinds==0)return;

            int alive=0;
            foreach(Actor enemy in current.enemies)
                if(enemy!=null && enemy.hp>0)alive++;

            if(alive>=RespawnPopulationLimitForStage())return;

            // Rotate through the authored spawn roster, avoiding a permanent
            // bias toward whichever monster appears first in levels.json.
            int selected=current.respawnIndex%commonSpawnKinds;
            current.respawnIndex++;
            string kind=null;
            int seen=0;
            foreach(Spawn spawn in level.enemies)
            {
                if(IsBoss(spawn.kind))continue;
                if(seen++==selected){kind=spawn.kind;break;}
            }
            if(string.IsNullOrEmpty(kind))return;

            // Spawn just outside the player's nearby combat area: this creates
            // new encounters naturally without enemies popping into view.
            Vector2 center=player.body.position;
            Vector2 gate=NecroLevels.At(level,level.sealX,level.sealY);
            Vector2 entrance=NecroLevels.At(level,level.startX,level.startY);
            for(int attempt=0;attempt<28;attempt++)
            {
                float angle=Random.Range(-Mathf.PI,Mathf.PI);
                float radius=Random.Range(SpawnMinDistanceForStage(),
                    SpawnMaxDistanceForStage());
                Vector2 at=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                if(Blocked(at,.47f))continue;
                if(Vector2.Distance(at,gate)<2.5f ||
                   Vector2.Distance(at,entrance)<2.5f)continue;
                // Never materialize a creature in the visible camera area.
                if(cam!=null && NecroEncounterRules.OnScreen(
                    cam.WorldToViewportPoint(
                        new Vector3(at.x,at.y,0f))))continue;
                // Resource collection should remain a brief safe moment.
                bool nearPickup=false;
                foreach(ResourceNode node in current.resources)
                {
                    if(node==null || node.body==null)continue;
                    if(NecroEncounterRules.NearPickup(
                        at,(Vector2)node.body.position))
                    {
                        nearPickup=true;
                        break;
                    }
                }
                if(nearPickup)continue;

                bool crowded=false;
                foreach(Actor enemy in current.enemies)
                {
                    if(enemy!=null && enemy.hp>0 &&
                       Vector2.Distance(at,enemy.body.position)<MinEnemySeparation)
                    {crowded=true;break;}
                }
                if(crowded)continue;
                foreach(Actor ally in servants)
                {
                    if(ally!=null && ally.hp>0 &&
                       Vector2.Distance(at,ally.body.position)<2.3f)
                    {crowded=true;break;}
                }
                if(crowded)continue;

                Actor spawned=Create(kind,at,false,current.root.transform);
                current.enemies.Add(spawned);
                return;
            }
        }
    }
}

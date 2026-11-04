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
        const float FirstRespawnSeconds=6f;
        const float SpawnMinDistance=5.0f;
        const float SpawnMaxDistance=8.5f;
        const float MinEnemySeparation=1.8f;
        const int MinLivingEnemies=10;
        const int MaxLivingEnemies=16;

        void TickRespawns(float dt)
        {
            if(current==null || !current.claimed || !current.root.activeInHierarchy)
                return;

            current.respawnTimer-=dt;
            if(current.respawnTimer>0f)return;
            current.respawnTimer=Random.Range(7.5f,11.5f);

            int commonSpawnKinds=0;
            foreach(Spawn entry in level.enemies)
                if(!IsBoss(entry.kind))commonSpawnKinds++;

            if(commonSpawnKinds==0)return;

            int alive=0;
            foreach(Actor enemy in current.enemies)
                if(enemy!=null && enemy.hp>0)alive++;

            int limit=Mathf.Clamp(commonSpawnKinds+4,MinLivingEnemies,MaxLivingEnemies);
            if(alive>=limit)return;

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
                float radius=Random.Range(SpawnMinDistance,SpawnMaxDistance);
                Vector2 at=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                if(Blocked(at,.47f))continue;
                if(Vector2.Distance(at,gate)<2.5f ||
                   Vector2.Distance(at,entrance)<2.5f)continue;

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

            // If all candidate positions are blocked, retry without creating
            // enemies inside walls or overlapping the player.
            current.respawnTimer=3.5f;
        }

        /// <summary>
        /// Hero targets and attacks automatically at no anima cost.
        /// Resurrection remains the player's deliberate, resource-consuming action.
        /// </summary>
        void TryAutomaticAttack()
        {
            if(gameOver || player==null || player.hp<=0 || Time.time<castCooldown)
                return;

            float range=spellUnlocked?3.8f:1.02f;
            Actor enemy=Closest(range);
            if(enemy==null)return;
            if(!Visible(player.body.position,enemy.body.position))return;

            // Both the initial melee strike and the unlocked spectral
            // projectile are basic attacks and never spend anima.
            Cast();
        }
    }
}

using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.12: transparent, deterministic region-based encounter balance.
    /// Only hostile encounter pacing, incoming damage and health gained from
    /// resources change. Companion stats, rewards and existing saves do not.
    /// </summary>
    public partial class NecroPrototype
    {
        // Easier introduction with reaction time; later regions retain risk.
        float IncomingHeroDamageMultiplier()
        {
            return stage==0 ? .82f :
                stage==1 ? 1f :
                stage==2 ? 1.08f : 1.14f;
        }

        float EnemyStrikeCooldown(Actor enemy)
        {
            if(IsBoss(enemy.kind))return 1.55f;
            return stage==0 ? 1.40f :
                stage==1 ? 1.20f :
                stage==2 ? 1.15f : 1.11f;
        }

        float HostilePupDetectionDistance()
        {
            return stage==0 ? 3.65f : 4.1f;
        }

        float PupWindupDuration(Actor pup)
        {
            return !pup.ally && stage==0 ? .56f : .42f;
        }

        float PupRecoveryCooldown(Actor pup)
        {
            return !pup.ally && stage==0 ? 1.25f : .95f;
        }

        float RotboarWindupDuration(Actor boar)
        {
            return !boar.ally && stage==0 ? 1.00f : .80f;
        }

        float RespawnIntervalForStage()
        {
            return stage==0 ? 19f :
                stage==1 ? 15f :
                stage==2 ? 12f : 11f;
        }

        int RespawnPopulationLimitForStage()
        {
            // The unified cave spans nine chambers; the old small-area cap
            // would leave almost its entire length empty after one sweep.
            if(NecroCaveNetwork.HasNetwork(level))return 26;
            return stage==0 ? 7 :
                stage==1 ? 4 :
                stage==2 ? 5 : 6;
        }

        float SpawnMinDistanceForStage()
        {
            if(NecroCaveNetwork.HasNetwork(level))return 7.0f;
            return stage==0 ? 4.3f :
                stage==1 ? 3.9f : 3.6f;
        }

        float SpawnMaxDistanceForStage()
        {
            if(NecroCaveNetwork.HasNetwork(level))return 13.0f;
            return stage==0 ? 5.3f :
                stage==1 ? 5.0f : 4.7f;
        }

        // An enemy is more likely to choose a nearby servant as the next
        // target. It still compares real distances and obeys collisions.
        // This rewards building an army without granting invulnerability.
        float ServantThreatDistanceWeight()
        {
            return stage==0 ? .62f :
                stage==1 ? .73f :
                stage==2 ? .82f : .90f;
        }

        float HealFromGatheredResource()
        {
            if(player==null)return 0f;
            float missing=Mathf.Max(0f,player.maxHp-player.hp);
            float amount=Mathf.Max(9f,player.maxHp*.12f);
            return Mathf.Min(missing,amount);
        }
    }
}

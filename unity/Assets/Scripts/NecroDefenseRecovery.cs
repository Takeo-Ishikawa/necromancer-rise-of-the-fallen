using UnityEngine;

namespace Necromancer
{
    /// <summary>0.9.13: deterministic hero armor and timed sanctuary healing.
    /// All persistent values are already represented in the v1 save format.</summary>
    public static class NecroDefenseRules
    {
        // Only the heroine benefits; servant and enemy stats are unchanged.
        public static float MantleReduction(int tier)
        {
            return Mathf.Clamp(tier,0,3)*.06f;
        }

        public static float DamageAfterMantle(float incoming,int tier)
        {
            return Mathf.Max(0f,incoming)*(1f-MantleReduction(tier));
        }

        public static float FountainHealing(float currentHp,float maximumHp)
        {
            if(maximumHp<=0f)return 0f;
            float missing=Mathf.Max(0f,
                maximumHp-Mathf.Clamp(currentHp,0f,maximumHp));
            // At most 30% of maximum HP per use, at least 18 HP where needed.
            return Mathf.Min(missing,Mathf.Max(18f,maximumHp*.30f));
        }
    }

    public partial class NecroPrototype
    {
        float MantleDamageReduction()
        {
            return NecroDefenseRules.MantleReduction(mantleTier);
        }

        float MitigateHeroDamage(float damage)
        {
            return NecroDefenseRules.DamageAfterMantle(damage,mantleTier);
        }

        float RecoverFountainHealth()
        {
            if(!inSanctuary || gameOver || player==null ||
                player.hp<=0f)return 0f;
            float healed=NecroDefenseRules.FountainHealing(player.hp,player.maxHp);
            player.hp=Mathf.Min(player.maxHp,player.hp+healed);
            return healed;
        }
    }
}

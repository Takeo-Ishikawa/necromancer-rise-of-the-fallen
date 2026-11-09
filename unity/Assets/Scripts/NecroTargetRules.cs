using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Stateless combat targeting rules for the 0.9.14 tactical update.
    /// Shared by hostile target selection and defensive servant decisions.
    /// </summary>
    public static class NecroTargetRules
    {
        public static float ThreatScore(float distanceSquared,
            bool servant,float servantWeight)
        {
            float value=Mathf.Max(0f,distanceSquared);
            return servant?value*Mathf.Clamp01(servantWeight):value;
        }

        public static bool KeepFocus(float previousScore,float bestScore)
        {
            // Hysteresis: change only when a new target is appreciably better.
            return previousScore<=bestScore*1.20f;
        }

        public static float DefenseScore(float servantDistance,
            float heroDistance)
        {
            float score=.55f*Mathf.Max(0f,servantDistance)+
                .45f*Mathf.Max(0f,heroDistance);
            // Enemies within striking distance of the heroine take priority.
            return heroDistance<=1.9f?score*.72f:score;
        }
    }
}

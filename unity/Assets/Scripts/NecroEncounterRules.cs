using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Stateless 0.9.17 encounter rules. Offscreen spawning and bounded
    /// pursuits make revisiting cave passages less chaotic.
    /// </summary>
    public static class NecroEncounterRules
    {
        public const float TerritoryRadius=8f;
        public const float ReturnStopRadius=.48f;
        public const float SightMemorySeconds=.85f;
        public const float PickupSafeRadius=1.85f;

        public static bool OutsideTerritory(Vector2 current,Vector2 origin)
        {
            return (current-origin).sqrMagnitude>
                TerritoryRadius*TerritoryRadius;
        }

        public static bool NeedsReturn(Vector2 current,Vector2 origin)
        {
            return (current-origin).sqrMagnitude>
                ReturnStopRadius*ReturnStopRadius;
        }

        public static bool OnScreen(Vector3 viewport)
        {
            // Unity viewport coordinates: z>0 indicates in front of camera.
            // Margins ensure a respawn does not pop in on a screen edge.
            return viewport.z>0f &&
                viewport.x>=-.10f && viewport.x<=1.10f &&
                viewport.y>=-.10f && viewport.y<=1.10f;
        }

        public static bool NearPickup(Vector2 spawn,Vector2 pickup)
        {
            return (spawn-pickup).sqrMagnitude<
                PickupSafeRadius*PickupSafeRadius;
        }
    }
}

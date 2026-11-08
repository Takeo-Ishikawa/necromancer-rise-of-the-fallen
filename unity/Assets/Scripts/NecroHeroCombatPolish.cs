using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.9: visual-only improvements to the free spectral attack.
    /// The existing projectile damage, cooldown, anima cost (zero),
    /// collision obstacles and travel cleanup are preserved.
    /// </summary>
    public partial class NecroPrototype
    {
        // A swept circle catches small creatures crossed between frames,
        // without increasing the projectile radius or speed.
        static bool SpectralSegmentHit(Vector2 from,Vector2 to,
            Vector2 center,float radius)
        {
            Vector2 delta=to-from;
            float lengthSquared=delta.sqrMagnitude;
            float fraction=lengthSquared>.000001f ?
                Mathf.Clamp01(Vector2.Dot(center-from,delta)/lengthSquared):
                0f;
            Vector2 closest=from+delta*fraction;
            return (center-closest).sqrMagnitude<radius*radius;
        }

        void UpdateSpectralProjectileVisual(Projectile bolt)
        {
            if(bolt==null || bolt.body==null || bolt.renderer==null)return;
            // Existing skull sprite, softly pulsing between aqua and violet.
            // No sprite generation or VFX instantiation per projectile frame.
            float wave=.5f+.5f*Mathf.Sin(bolt.age*22f);
            bolt.body.localScale=Vector3.one*(.94f+.13f*wave);
            Color tint=Color.Lerp(
                new Color(.58f,.99f,.88f,.95f),
                new Color(.87f,.72f,1f,.95f),wave);
            // Expiration fades the trail rather than popping out abruptly.
            tint.a*=Mathf.Clamp01(bolt.time/.16f);
            bolt.renderer.color=tint;
        }
    }
}

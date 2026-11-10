using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        // Capability belongs to the selected character, never to the shared save.
        bool HeroCanUseMagic {
            get { return characterDefinitions==null || !SelectedCharacter.meleeOnly; }
        }
        float EffectiveManualRange {
            get { return HeroCanUseMagic?ManualRange:1.35f; }
        }
    }
}

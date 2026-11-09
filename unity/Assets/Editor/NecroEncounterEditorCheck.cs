using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Verify the real C# rules before manual Unity platform builds.
    /// Does not replace a runtime play test.
    /// </summary>
    public static class NecroEncounterEditorCheck
    {
        static void Require(bool condition,string caseName)
        {
            if(!condition)
                throw new BuildFailedException(
                    "Encounter pacing regression: "+caseName);
        }

        public static void Verify()
        {
            Vector2 home=new Vector2(9f,6f);
            Require(!NecroEncounterRules.OutsideTerritory(
                home+new Vector2(8f,0f),home),"territory boundary");
            Require(NecroEncounterRules.OutsideTerritory(
                home+new Vector2(8.01f,0f),home),"territory break");
            Require(!NecroEncounterRules.NeedsReturn(
                home+new Vector2(.30f,0f),home),"resting at home");
            Require(NecroEncounterRules.NeedsReturn(
                home+new Vector2(.60f,0f),home),"return to den");
            Require(NecroEncounterRules.OnScreen(
                new Vector3(.5f,.5f,10f)),"visible spawn");
            Require(NecroEncounterRules.OnScreen(
                new Vector3(-.10f,1.10f,10f)),"screen margin");
            Require(!NecroEncounterRules.OnScreen(
                new Vector3(-.11f,.5f,10f)),"offscreen candidate");
            Require(!NecroEncounterRules.OnScreen(
                new Vector3(.5f,.5f,-10f)),"behind camera");
            Require(NecroEncounterRules.NearPickup(
                home,home+new Vector2(1f,0f)),"pickup exclusion");
            Require(!NecroEncounterRules.NearPickup(
                home,home+new Vector2(2.2f,0f)),"safe pickup distance");
            Require(NecroEncounterRules.SightMemorySeconds>0f &&
                NecroEncounterRules.SightMemorySeconds<1.5f,
                "brief sight memory");
        }
    }
}

using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Executes the same C# target-scoring rules used by the game before
    /// exporting a player. Does not replace in-game behavior validation.
    /// </summary>
    public static class NecroTargetEditorCheck
    {
        static void Expect(float found,float expected,string caseName)
        {
            if(float.IsNaN(found) || Mathf.Abs(found-expected)>.001f)
                throw new BuildFailedException("Targeting regression: "+
                    caseName+" ("+found+" != "+expected+").");
        }

        static void Require(bool condition,string caseName)
        {
            if(!condition)
                throw new BuildFailedException("Targeting regression: "+caseName);
        }

        public static void Verify()
        {
            Expect(NecroTargetRules.ThreatScore(9f,false,.62f),9f,
                "player distance");
            Expect(NecroTargetRules.ThreatScore(9f,true,.62f),5.58f,
                "servant threat");
            Expect(NecroTargetRules.ThreatScore(-1f,true,.62f),0f,
                "negative squared distance");
            Require(NecroTargetRules.KeepFocus(10f,9f),
                "small improvement keeps current target");
            Require(!NecroTargetRules.KeepFocus(12f,9f),
                "significant improvement changes target");
            Expect(NecroTargetRules.DefenseScore(3f,4f),3.45f,
                "regular defensive target");
            Expect(NecroTargetRules.DefenseScore(3f,1f),1.512f,
                "hero-adjacent priority");
            Require(NecroTargetRules.DefenseScore(3f,1f)<
                NecroTargetRules.DefenseScore(2f,4f),
                "defense prioritizes threats to heroine");
        }
    }
}

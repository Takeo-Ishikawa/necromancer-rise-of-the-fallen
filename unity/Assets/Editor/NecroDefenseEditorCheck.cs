using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>Exercise the actual C# defense rules before manual builds.</summary>
    public static class NecroDefenseEditorCheck
    {
        static void Expect(float actual,float expected,string name)
        {
            if(float.IsNaN(actual) || Mathf.Abs(actual-expected)>.001f)
                throw new BuildFailedException(
                    "Defense/recovery regression: "+name+
                    " (got "+actual+", expected "+expected+").");
        }

        public static void Verify()
        {
            Expect(NecroDefenseRules.MantleReduction(-1),0f,"negative tier");
            Expect(NecroDefenseRules.MantleReduction(0),0f,"tier 0");
            Expect(NecroDefenseRules.MantleReduction(1),.06f,"tier 1");
            Expect(NecroDefenseRules.MantleReduction(2),.12f,"tier 2");
            Expect(NecroDefenseRules.MantleReduction(3),.18f,"tier 3");
            Expect(NecroDefenseRules.MantleReduction(99),.18f,"tier cap");
            Expect(NecroDefenseRules.DamageAfterMantle(100f,0),100f,"unarmored");
            Expect(NecroDefenseRules.DamageAfterMantle(100f,3),82f,"armored");
            Expect(NecroDefenseRules.DamageAfterMantle(-5f,3),0f,"negative damage");
            Expect(NecroDefenseRules.FountainHealing(100f,100f),0f,"full HP");
            Expect(NecroDefenseRules.FountainHealing(60f,100f),30f,"regular heal");
            Expect(NecroDefenseRules.FountainHealing(95f,100f),5f,"heal cap");
            Expect(NecroDefenseRules.FountainHealing(20f,40f),18f,"minimum heal");
            Expect(NecroDefenseRules.FountainHealing(60f,200f),60f,"scaled heal");
            Expect(NecroDefenseRules.FountainHealing(0f,0f),0f,"invalid maximum");
        }
    }
}

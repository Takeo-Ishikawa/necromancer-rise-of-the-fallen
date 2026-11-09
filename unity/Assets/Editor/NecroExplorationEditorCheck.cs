using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>Check the actual C# resource discovery and reward math.</summary>
    public static class NecroExplorationEditorCheck
    {
        static void Expect(float found,float expected,string label)
        {
            if(float.IsNaN(found) || Mathf.Abs(found-expected)>.001f)
                throw new BuildFailedException("Exploration regression: "+
                    label+" ("+found+" != "+expected+")");
        }

        static void Require(bool value,string label)
        {
            if(!value)
                throw new BuildFailedException("Exploration regression: "+label);
        }

        public static void Verify()
        {
            Require(NecroExplorationRules.CanReveal(0f),"near resource");
            Require(NecroExplorationRules.CanReveal(49f),"reveal boundary");
            Require(!NecroExplorationRules.CanReveal(49.01f),
                "resource outside reveal radius");
            Expect(NecroExplorationRules.MarkerScale(0f,0f),1.035f,
                "neutral shimmer");
            Expect(NecroExplorationRules.MarkerScale(Mathf.PI/6f,0f),
                1.07f,"peak shimmer");
            Expect(NecroExplorationRules.ActualHealing(80f,100f,12f),
                12f,"normal healing");
            Expect(NecroExplorationRules.ActualHealing(97f,100f,12f),
                3f,"healing capped by missing HP");
            Expect(NecroExplorationRules.ActualHealing(100f,100f,12f),
                0f,"full health");
            Expect(NecroExplorationRules.ActualHealing(50f,100f,-10f),
                0f,"no negative healing");
        }
    }
}

using UnityEditor.Build;

namespace Necromancer.EditorTools
{
    public static class NecroDownedEditorCheck
    {
        static void Require(bool ok,string message)
        {
            if(!ok)throw new BuildFailedException("Downed mechanic: "+message);
        }
        public static void Verify()
        {
            Require(NecroDownedRules.WithinReach(1.68f,1.3f),"reach");
            Require(!NecroDownedRules.WithinReach(1.70f,1.3f),"out of reach");
            Require(NecroDownedRules.FragmentsFor("rat")==2,"rat resources");
            Require(NecroDownedRules.FragmentsFor("grave_pup")==3,"pup resources");
            Require(NecroDownedRules.FragmentsFor("rotboar")==5,"boar resources");
            Require(NecroDownedRules.FragmentsFor("grave_lord")==8,"boss resources");
            Require(NecroDownedRules.CanCapture(9,3,12),"cap allowed");
            Require(!NecroDownedRules.CanCapture(11,3,12),"cap denied");
            Require(NecroDownedRules.CaptureMana(true,true,18)==18,"raise cost");
            Require(NecroDownedRules.CaptureMana(true,false,18)==0,"pup cost");
            Require(NecroDownedRules.CaptureMana(false,true,18)==0,"physical");
            Require(NecroDownedRules.DownedDuration>=10f,"choice window");
        }
    }
}

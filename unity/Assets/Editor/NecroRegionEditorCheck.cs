using UnityEditor.Build;

namespace Necromancer.EditorTools
{
    public static class NecroRegionEditorCheck
    {
        static void Require(bool ok,string reason)
        {
            if(!ok)throw new BuildFailedException("Region progression: "+reason);
        }

        public static void Verify()
        {
            Require(!NecroRegionRules.CanClaim(false,false,false,false),
                "unreached ordinary seal cannot unlock");
            Require(NecroRegionRules.CanClaim(false,false,false,true),
                "ordinary seal unlocks on arrival");
            Require(!NecroRegionRules.CanClaim(true,false,false,true),
                "claimed stage cannot claim again");
            Require(!NecroRegionRules.CanClaim(false,true,false,true),
                "guardian stage must not be bypassed");
            Require(NecroRegionRules.CanClaim(false,true,true,false),
                "defeated guardian unlocks a stage");
            Require(NecroRegionRules.NeedsGuardian(true,false,false),
                "guardian must spawn when alive");
            Require(!NecroRegionRules.NeedsGuardian(true,false,true),
                "defeated guardian stays defeated after reload");
            Require(!NecroRegionRules.NeedsGuardian(true,true,false),
                "claimed guardian stage stays clear");
            Require(NecroRegionRules.CanAdvance(true,true,true),
                "passage opens after claim");
            Require(!NecroRegionRules.CanAdvance(false,true,true),
                "locked passage stays closed");
            Require(!NecroRegionRules.CanAdvance(true,false,true),
                "portal requires player proximity");
            Require(!NecroRegionRules.CanAdvance(true,true,false),
                "final stage has no next region");
        }
    }
}

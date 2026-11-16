using UnityEditor.Build;
using UnityEngine;

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

            // Unlike the Python preflight, this verifies the actual Unity
            // JsonUtility import and the exact collision code used in-game.
            LevelDefinition[] maps=NecroLevels.Load();
            Require(maps!=null && maps.Length==1 &&
                maps[0]!=null && maps[0].id=="bonehollow",
                "exactly one world map must load in Unity");
            LevelDefinition cave=maps[0];
            Require(cave.layoutRevision==2 && cave.width==160f &&
                cave.height==104f && NecroCaveNetwork.HasNetwork(cave),
                "unexpected unified cave revision or dimensions");
            Require(cave.biomes!=null && cave.biomes.Length==9,
                "nine subterranean biomes must deserialize");
            Require(cave.caveRows.Length==104,
                "underground geometry must cover every row");
            for(int row=0;row<cave.caveRows.Length;row++)
                Require(cave.caveRows[row]!=null &&
                    cave.caveRows[row].Length==160,
                    "invalid cavern solid wall at row "+row);
            Require(NecroCaveNetwork.IsWall(cave,0,0),
                "outer border must be solid");
            Require(NecroCaveNetwork.HasClearance(cave,cave.startX,
                cave.startY,1f), "entrance must be walkable");
            Require(NecroCaveNetwork.HasClearance(cave,cave.sealX,
                cave.sealY,1f), "terminal seal must be walkable");
            Require(NecroCaveNetwork.HasClearance(cave,cave.rotboarX,
                cave.rotboarY,1.6f), "Rotboar arena must have clearance");
            Require(cave.enemies!=null && cave.enemies.Length>=40,
                "unified cave must have distributed encounters");
            foreach(Spawn creature in cave.enemies)
                Require(creature!=null &&
                    NecroCaveNetwork.HasClearance(cave,creature.x,
                    creature.y,1f), "creature emerged inside cave wall");
            Require(cave.resources!=null && cave.resources.Length>=25,
                "cave resources must deserialize");
            foreach(ResourceSpawn resource in cave.resources)
                Require(resource!=null &&
                    NecroCaveNetwork.HasClearance(cave,resource.x,
                    resource.y,.60f), "resource trapped in cave wall");

            // Test the same cached routing used by followers at runtime.
            // Both small and broad servants must cross the cave without
            // crossing rock, including through its curved offshoots.
            Vector2 entrance=NecroLevels.At(cave,cave.startX,cave.startY);
            float[] bodySizes={.28f,.52f};
            foreach(float radius in bodySizes)
                foreach(CaveBiome biome in cave.biomes)
                {
                    Vector2 destination=NecroLevels.At(cave,biome.x,biome.y);
                    NecroCaveRoute route=NecroCaveNavigator.Build(cave,
                        entrance,destination,radius);
                    Require(route!=null && route.waypoints.Length>0,
                        "no cave route to biome "+biome.id);
                    Vector2 previous=entrance;
                    foreach(Vector2 point in route.waypoints)
                    {
                        Require(NecroCaveNavigator.Direct(cave,previous,
                            point,radius),
                            "route cuts cave wall near biome "+biome.id);
                        previous=point;
                    }
                    Require(NecroCaveNavigator.Direct(cave,previous,
                        destination,radius),
                        "route cannot reach biome center "+biome.id);
                }

        }
    }
}

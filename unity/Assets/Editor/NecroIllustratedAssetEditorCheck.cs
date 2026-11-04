using UnityEditor.Build;
using UnityEngine;
namespace Necromancer.EditorTools
{
    public static class NecroIllustratedAssetEditorCheck
    {
        public static void Verify()
        {
            Texture2D floor=Resources.Load<Texture2D>(
                "Backgrounds/entrada_da_caverna_bonehollow");
            Texture2D crystal=Resources.Load<Texture2D>("Scenery/crystal");
            Texture2D rocks=Resources.Load<Texture2D>("Scenery/rock_cluster");
            if(floor==null || floor.width!=512 || floor.height!=512)
                throw new BuildFailedException("Missing 512px cave floor PNG");
            if(crystal==null || crystal.width!=256 || crystal.height!=256 ||
               rocks==null || rocks.width!=256 || rocks.height!=256)
                throw new BuildFailedException("Illustrated prop PNG missing");
            Sprite ground=NecroArt.Environment("entrada_da_caverna_bonehollow");
            Sprite sanctuary=NecroArt.Environment("santuario_necromante_bonehollow");
            if(ground==null || sanctuary==null ||
               ground.texture.width!=512 || sanctuary.texture.width!=512)
                throw new BuildFailedException("Cave must load imported painting");
            Sprite rock=NecroArt.Stone();
            Sprite magic=NecroScenery.Get("crystal");
            if(rock==null || magic==null ||
               rock.texture.width!=256 || magic.texture.width!=256)
                throw new BuildFailedException("Scenery art not loaded");
            Debug.Log("[0.6.5c] PNG floor, rocks, crystals and sanctuary: PASS");
        }
    }
}

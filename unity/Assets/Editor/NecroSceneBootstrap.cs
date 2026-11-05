using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necromancer.EditorTools
{
    // Cria a cena inicial ao abrir este projeto Unity pela primeira vez.
    [InitializeOnLoad]
    public static class NecroSceneBootstrap
    {
        const string ScenePath = "Assets/Scenes/NecromancerPrototype.unity";
        static NecroSceneBootstrap()
        {
            EditorApplication.delayCall += Setup;
        }

        [MenuItem("Necromancer/Recriar cena inicial")]
        public static void Rebuild()
        {
            if(File.Exists(ScenePath)) File.Delete(ScenePath);
            Setup();
        }

        public static void Setup()
        {
            if(File.Exists(ScenePath))return;
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            GameObject camera=new GameObject("Main Camera");
            camera.tag="MainCamera";
            Camera cam=camera.AddComponent<Camera>();
            cam.orthographic=true;
            cam.orthographicSize=5.6f;
            cam.backgroundColor=new Color(.08f,.07f,.12f);
            camera.transform.position=new Vector3(15.36f,10.24f,-10);
            new GameObject("Necromancer 2D").AddComponent<Necromancer.NecroPrototype>();
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new []{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.Refresh();
            Debug.Log("Necromancer: cena inicial criada em "+ScenePath);
        }
    }

    // Garante que as imagens importadas em Resources sejam sprites 2D.
    public class NecroSpriteImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            bool sanctuaryProp=assetPath.Contains("/Resources/SanctuaryProps/");
            if(!sanctuaryProp &&
               !assetPath.Contains("/Resources/Backgrounds/") &&
               !assetPath.Contains("/Resources/Sprites/"))return;
            TextureImporter t=(TextureImporter)assetImporter;
            // Props are loaded as Texture2D so a bottom-foot pivot can be
            // created in code without altering the original source PNG.
            t.textureType=sanctuaryProp ?
                TextureImporterType.Default : TextureImporterType.Sprite;
            if(!sanctuaryProp)t.spriteImportMode=SpriteImportMode.Single;
            t.alphaIsTransparency=true;
            t.mipmapEnabled=false;
            t.filterMode=FilterMode.Bilinear;
            t.textureCompression=TextureImporterCompression.Compressed;
            t.maxTextureSize=sanctuaryProp?2048:4096;
        }
    }
}

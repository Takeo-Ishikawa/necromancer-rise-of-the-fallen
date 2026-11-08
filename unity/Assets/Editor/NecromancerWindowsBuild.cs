using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Manual Win64 preview. Its own Unity build target and Mono backend
    /// avoid Android signing, Android SDK and native IL2CPP cross-toolchain.
    /// Source scenes and game data are shared with the Android preview.
    /// </summary>
    public static class NecromancerWindowsBuild
    {
        public static void BuildWindows()
        {
            const string scenePath = "Assets/Scenes/NecromancerPrototype.unity";
            // The scene can be created by the editor bootstrap during import.
            // Verify everything and persist it before BuildPipeline.BuildPlayer.
            NecroSaveEditorCheck.Verify();
            NecroVisualEditorCheck.Verify();
            NecroIllustratedAssetEditorCheck.Verify();
            NecroFrameEditorCheck.Verify();
            NecroSceneBootstrap.Setup();
            if(!File.Exists(scenePath))
                throw new BuildFailedException("Missing scene: " + scenePath);

            var scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
            if(!scene.IsValid() || scene.isDirty)
                EditorSceneManager.SaveScene(scene,scenePath);
            EditorBuildSettings.scenes=new[] {
                new EditorBuildSettingsScene(scenePath,true)
            };
            AssetDatabase.SaveAssets();

            PlayerSettings.companyName="NecromancerStudio";
            PlayerSettings.productName="Necromancer (Windows Preview)";
            PlayerSettings.bundleVersion="0.9.11";
            // Windows Mono can be built on the Linux GameCI editor image.
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);

            // Keep all generated player files in their platform-specific folder.
            // Windows requires the executable, UnityPlayer.dll and _Data folder.
            string repository=Path.GetFullPath(
                Path.Combine(Application.dataPath,"..",".."));
            string output=Path.Combine(repository,"build","StandaloneWindows64",
                "necromancer-windows.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options=new BuildPlayerOptions {
                scenes=new[]{scenePath},
                locationPathName=output,
                target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.None
            };
            Debug.Log("[Necromancer] Exporting Windows x64 player: "+output);
            BuildReport report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=BuildResult.Succeeded)
                throw new BuildFailedException("Windows player build failed: "+
                    report.summary.result+", errors: "+report.summary.totalErrors);
            if(!File.Exists(output))
                throw new BuildFailedException("Windows exe missing: "+output);
            Debug.Log("[Necromancer] Windows build ready: "+
                report.summary.totalSize+" bytes");
        }
    }
}

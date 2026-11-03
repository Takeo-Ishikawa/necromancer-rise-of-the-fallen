using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>Entry point for GameCI Android build. Explicitly saves and registers the
    /// scene BEFORE calling BuildPipeline; avoids the 'Cannot build untitled scene' error.</summary>
    public static class NecromancerAndroidBuild
    {
        public static void BuildAndroid()
        {
            const string scenePath = "Assets/Scenes/NecromancerPrototype.unity";
            // The empty template is generated at first import, but the batch process may
            // invoke build before EditorApplication.delayCall has executed.
            // Test serialization before exporting a player.
            NecroSaveEditorCheck.Verify();
            NecroSceneBootstrap.Setup();
            if(!File.Exists(scenePath))
                throw new BuildFailedException("Missing scene: " + scenePath);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if(!scene.IsValid() || scene.isDirty)
                EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();

            PlayerSettings.companyName = "NecromancerStudio";
            PlayerSettings.productName = "Necromancer: Rise of the Fallen";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
                "com.takeo.necromancer.unity");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.bundleVersion = "0.5.0";
            PlayerSettings.Android.bundleVersionCode = 5;
            // Unity requires IL2CPP for ARM64 Android player builds.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;

            // Application.dataPath = repository/unity/Assets
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string apkPath = Path.Combine(repository, "build", "Android", "necromancer-unity.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(apkPath));

            var options = new BuildPlayerOptions {
                scenes = new[] {scenePath},
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            Debug.Log("[Necromancer] Exporting Android APK to " + apkPath);
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if(report.summary.result != BuildResult.Succeeded) {
                throw new BuildFailedException("Unity Android build failed: " +
                    report.summary.result + ", errors: " + report.summary.totalErrors);
            }
            Debug.Log("[Necromancer] APK created: " + apkPath +
                " (" + report.summary.totalSize + " bytes)");
        }
    }
}

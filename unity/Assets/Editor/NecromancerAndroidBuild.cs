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
            NecroVisualEditorCheck.Verify();
            NecroFrameEditorCheck.Verify();
            NecroSceneBootstrap.Setup();
            if(!File.Exists(scenePath))
                throw new BuildFailedException("Missing scene: " + scenePath);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if(!scene.IsValid() || scene.isDirty)
                EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();

            // Debug signing in ephemeral CI produces a NEW certificate on each run.
            // Never build the production package name with a temporary debug key.
            // Preview runs alongside 0.5; production must use a persistent private key.
            string mode=Environment.GetEnvironmentVariable("NECRO_ANDROID_MODE");
            if(string.IsNullOrEmpty(mode))mode="preview";
            bool preview=string.Equals(mode,"preview",StringComparison.OrdinalIgnoreCase);
            bool release=string.Equals(mode,"release",StringComparison.OrdinalIgnoreCase);
            if(!preview && !release)
                throw new BuildFailedException("Invalid NECRO_ANDROID_MODE (preview or release).");

            PlayerSettings.companyName = "NecromancerStudio";
            PlayerSettings.productName = preview
                ? "Necromancer (Preview 0.6.4)"
                : "Necromancer: Rise of the Fallen";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
                preview ? "com.takeo.necromancer.unity.preview064" :
                    "com.takeo.necromancer.unity");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.bundleVersion = "0.6.4";
            PlayerSettings.Android.bundleVersionCode = 10;

            if(release)
            {
                string keystore=Environment.GetEnvironmentVariable("NECRO_KEYSTORE_PATH");
                if(string.IsNullOrEmpty(keystore))
                    keystore=Path.GetFullPath(
                        Path.Combine(Application.dataPath,"..",".android-release.keystore"));
                string storePass=Environment.GetEnvironmentVariable("NECRO_KEYSTORE_PASSWORD");
                string keyAlias=Environment.GetEnvironmentVariable("NECRO_KEY_ALIAS");
                string aliasPass=Environment.GetEnvironmentVariable("NECRO_KEY_PASSWORD");
                if(string.IsNullOrEmpty(keystore) || !File.Exists(keystore) ||
                   string.IsNullOrEmpty(storePass) || string.IsNullOrEmpty(keyAlias) ||
                   string.IsNullOrEmpty(aliasPass))
                    throw new BuildFailedException(
                        "Release signing credentials not configured. See docs/ANDROID_SIGNING.md.");
                PlayerSettings.Android.useCustomKeystore=true;
                PlayerSettings.Android.keystoreName=keystore;
                PlayerSettings.Android.keystorePass=storePass;
                PlayerSettings.Android.keyaliasName=keyAlias;
                PlayerSettings.Android.keyaliasPass=aliasPass;
                Debug.Log("[Necromancer] Production signing: custom persistent keystore.");
            }
            else
            {
                PlayerSettings.Android.useCustomKeystore=false;
                Debug.Log("[Necromancer] Isolated preview application; no existing saves will be modified.");
            }
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

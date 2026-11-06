using UnityEditor;
using UnityEngine;

namespace Necromancer.Editor
{
    /// <summary>
    /// Android texture policy for the painted sanctuary. Future areas should add
    /// their resource paths here rather than disabling compression per asset.
    /// Textures remain Texture2D, because the game constructs sprites at runtime.
    /// </summary>
    public sealed class NecroSanctuaryImportSettings : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            bool prop = path.StartsWith("Assets/Resources/SanctuaryProps/") &&
                        path.EndsWith(".png");
            bool bg = path == "Assets/Resources/Backgrounds/safe_chamber_v3.jpg";
            if(!prop && !bg)return;

            TextureImporter t = assetImporter as TextureImporter;
            if(t == null)return;

            // Preserve artwork detail and alpha while avoiding wasted mip levels.
            // TextureImporterType.Default is intentional: Resources.Load<Texture2D>.
            t.textureType = TextureImporterType.Default;
            t.mipmapEnabled = false;
            t.alphaIsTransparency = prop;
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.sRGBTexture = true;
            t.maxTextureSize = 4096;
            t.textureCompression = TextureImporterCompression.CompressedHQ;

            // ETC2 runs on OpenGL ES 3 / Vulkan devices and needs no ASTC-only GPU.
            // RGB background: 4 bpp. Transparent props: 8 bpp instead of RGBA32.
            // Keep original PNG/JPG sources; compression is applied by Unity at build.
            TextureImporterPlatformSettings android =
                t.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = 2048;
            android.format = prop
                ? TextureImporterFormat.ETC2_RGBA8
                : TextureImporterFormat.ETC2_RGB4;
            android.textureCompression = TextureImporterCompression.CompressedHQ;
            android.compressionQuality = 100;
            android.crunchedCompression = false;
            t.SetPlatformTextureSettings(android);
        }
    }
}

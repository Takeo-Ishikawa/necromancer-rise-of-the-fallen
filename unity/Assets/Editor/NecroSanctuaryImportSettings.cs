using UnityEditor;
using UnityEngine;

namespace Necromancer.Editor
{
    // Avoid the low-resolution Android import preset for sanctuary only.
    public sealed class NecroSanctuaryImportSettings : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            bool prop = path.StartsWith("Assets/Resources/SanctuaryProps/") && path.EndsWith(".png");
            bool bg = path == "Assets/Resources/Backgrounds/safe_chamber_v3.jpg";
            if(!prop && !bg)return;
            TextureImporter t = assetImporter as TextureImporter;
            if(t==null)return;
            t.textureType=TextureImporterType.Default;
            t.mipmapEnabled=false;
            t.alphaIsTransparency=prop;
            t.filterMode=FilterMode.Bilinear;
            t.wrapMode=TextureWrapMode.Clamp;
            t.textureCompression=TextureImporterCompression.Uncompressed;
            t.maxTextureSize=4096;
            t.sRGBTexture=true;
            TextureImporterPlatformSettings a=t.GetPlatformTextureSettings("Android");
            a.name="Android";a.overridden=true;a.maxTextureSize=2048;
            a.format=prop?TextureImporterFormat.RGBA32:TextureImporterFormat.RGB24;
            a.textureCompression=TextureImporterCompression.Uncompressed;
            a.crunchedCompression=false;
            t.SetPlatformTextureSettings(a);
        }
    }
}

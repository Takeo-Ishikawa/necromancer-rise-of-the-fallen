using UnityEditor;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Unity's NPOT conversion can resize the 768x1024 (6x8) animation
    /// atlas to a square texture on Android. Preserve its exact aspect ratio.
    /// This importer only applies to the new character's assets.
    /// </summary>
    public sealed class NecroSombriaTextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(
                "Assets/Resources/Characters/NecromanteSombria/"))
                return;
            TextureImporter importer=(TextureImporter)assetImporter;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=2048;
            importer.mipmapEnabled=false;
            if(assetPath.EndsWith("/walk6.png"))
            {
                importer.alphaSource=TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
            }
        }
    }
}

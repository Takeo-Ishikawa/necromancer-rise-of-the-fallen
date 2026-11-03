using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Runs inside the Unity Editor before exporting Android. Checks actual
    /// raster output instead of only looking for source file names.
    /// </summary>
    public static class NecroVisualEditorCheck
    {
        public static void Verify()
        {
            string[] creatures={
                "rat","bat","slime","spider","snake","skeleton","wisp",
                "warden","grave_lord","marsh_lord"
            };
            HashSet<int> silhouettes=new HashSet<int>();
            foreach(string name in creatures)
            {
                Texture2D texture=new Texture2D(128,128,TextureFormat.RGBA32,false);
                texture.SetPixels(new Color[128*128]);
                if(!NecroCreatureArt.Draw(texture,name))
                    throw new BuildFailedException("No illustration for "+name);
                texture.Apply();
                Color32[] image=texture.GetPixels32();
                int count=0;
                int hash=17;
                unchecked
                {
                    for(int i=0;i<image.Length;i++)
                    {
                        if(image[i].a>50)count++;
                        if(i%17==0)
                        {
                            hash=hash*31+image[i].r;
                            hash=hash*31+image[i].g;
                            hash=hash*31+image[i].b;
                            hash=hash*31+image[i].a;
                        }
                    }
                }
                Object.DestroyImmediate(texture);
                if(count<450)
                    throw new BuildFailedException("Creature sprite is nearly empty: "+name);
                if(!silhouettes.Add(hash))
                    throw new BuildFailedException("Duplicate creature art: "+name);
            }
            string[] decorations={
                "stalagmite","crystal","bones","rune","fungus","reeds",
                "pool","headstone","broken_cross","lantern"
            };
            foreach(string name in decorations)
            {
                Sprite sprite=NecroScenery.Get(name);
                if(sprite==null || sprite.texture==null ||
                   sprite.texture.width<64 || sprite.texture.height<64)
                    throw new BuildFailedException("Missing scenery sprite: "+name);
            }
            if(NecroArt.Shadow()==null)
                throw new BuildFailedException("Creature shadow texture missing");
            Debug.Log("[Necromancer 0.6.2] Visual verification: 10 unique creature sprites "+
                "and 10 scenery types: PASS.");
        }
    }
}

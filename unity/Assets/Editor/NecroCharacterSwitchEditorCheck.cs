using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Verify the fourth character's imported atlas without relying on
    /// desktop-only dimensions or requiring access to personal save data.
    /// </summary>
    public static class NecroCharacterSwitchEditorCheck
    {
        static void Require(bool condition,string message)
        {
            if(!condition)throw new BuildFailedException(
                "Character menu: "+message);
        }

        public static void Verify()
        {
            NecroCharacterDefinition[] roster=NecroCharacterCatalog.Load();
            Require(roster!=null && roster.Length==4,
                "Four selectable characters required");
            int original=0, sombria=0;
            foreach(NecroCharacterDefinition entry in roster)
            {
                Require(entry!=null && entry.available,"Unavailable character");
                if(entry.id=="necromante")original++;
                if(entry.id=="necromante_sombria")
                {
                    sombria++;
                    Require(entry.meleeOnly,"New character is physical");
                    Texture2D atlas=Resources.Load<Texture2D>(
                        entry.walkSixResource);
                    Texture2D portrait=Resources.Load<Texture2D>(
                        entry.portraitResource);
                    Require(atlas!=null && atlas.width>=384 &&
                        atlas.height>=512,"Movement atlas not loadable");
                    Require(portrait!=null,"Portrait not loadable");
                    Require(atlas.width%6==0 && atlas.height%8==0,
                        "Atlas does not divide into 6 by 8 frames");
                    Require(atlas.width*4==atlas.height*3,
                        "Atlas aspect ratio distorted by import");
                }
            }
            Require(original==1 && sombria==1,"Original or new hero missing");
        }
    }
}

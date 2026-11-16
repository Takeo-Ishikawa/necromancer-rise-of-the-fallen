using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Real-Unity import and renderability check for the entrance vertical
    /// slice. Decorations MUST NOT change collision, spawns or save layout.
    /// </summary>
    public static class NecroEntranceEditorCheck
    {
        static void Require(bool ok,string message)
        {
            if(!ok)throw new BuildFailedException(
                "Bonehollow modular entrance: "+message);
        }

        public static void Verify()
        {
            LevelDefinition[] levels=NecroLevels.Load();
            Require(levels!=null && levels.Length==1 &&
                levels[0]!=null && levels[0].id=="bonehollow",
                "unified cave must load first");
            LevelDefinition map=levels[0];
            BonehollowEntranceLayout layout=NecroEntranceKit.ReadLayout();
            Require(layout!=null && layout.revision==1 &&
                layout.area==map.id && layout.modules!=null,
                "entrance module JSON missing or obsolete");
            Require(layout.modules.Length>=18,
                "vertical slice must have several independently placed pieces");
            HashSet<string> ids=new HashSet<string>();
            HashSet<string> kinds=new HashSet<string>();
            int walls=0,floors=0;
            foreach(BonehollowEntrancePiece module in layout.modules)
            {
                Require(module!=null && NecroEntranceKit.Valid(map,module),
                    "invalid position or wrong surface for an entrance module");
                Require(ids.Add(module.id),
                    "duplicate module ID "+module.id);
                kinds.Add(module.kind);
                if(module.surface=="wall")walls++;
                else floors++;
                Sprite sprite=NecroEntranceKit.Get(module.kind);
                Require(sprite!=null && sprite.texture!=null &&
                    sprite.texture.width==96 && sprite.texture.height==96,
                    "missing shared art for "+module.kind);
                // Every piece is purely visual: no Rigidbody or Collider
                // is generated, and map.caveRows is never modified.
            }
            Require(walls>=5 && floors>=10,
                "both wall relief and floor decoration must be modular");
            Require(kinds.Count==9,
                "expected floor variations, two rocks, two bones and wall variants");
        }
    }
}

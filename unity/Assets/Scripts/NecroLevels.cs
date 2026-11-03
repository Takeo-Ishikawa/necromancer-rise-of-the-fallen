using System;
using UnityEngine;

namespace Necromancer
{
    [Serializable] public class LevelCatalog { public LevelDefinition[] levels; }
    [Serializable] public class LevelDefinition
    {
        public string id,title,background,baseBackground,objective;
        public float width,height,startX,startY,sealX,sealY;
        public bool requiresGuardian;
        public Obstacle[] obstacles;
        public Spawn[] enemies;
        public ResourceSpawn[] resources;
    }
    [Serializable] public class Obstacle { public float x,y,r; }
    [Serializable] public class Spawn { public string kind; public float x,y; }

    [Serializable] public class ResourceSpawn { public string type; public float x,y; }
    public static class NecroLevels
    {
        public static LevelDefinition[] Load()
        {
            TextAsset json=Resources.Load<TextAsset>("Data/levels");
            if(json==null){Debug.LogError("Data/levels.json ausente.");return new LevelDefinition[0];}
            LevelCatalog catalog=JsonUtility.FromJson<LevelCatalog>(json.text);
            return catalog!=null && catalog.levels!=null?catalog.levels:new LevelDefinition[0];
        }
        // Mapa original: origem no canto superior esquerdo; Unity: origem inferior esquerda.
        public static Vector2 At(LevelDefinition level,float x,float y)
        {
            return new Vector2(x,level.height-y);
        }
    }
}

using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Soft environmental zones on a tiled floor; decorative, non-colliding,
    /// cached, and created only once when the first cave loads.
    /// </summary>
    public static class NecroCaveZones
    {
        static Sprite haze;

        static Sprite Haze()
        {
            if(haze!=null)return haze;
            const int size=128;
            Texture2D texture=new Texture2D(
                size,size,TextureFormat.RGBA32,false);
            Color32[] pixels=new Color32[size*size];
            for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float dx=(x-63.5f)/63.5f;
                    float dy=(y-63.5f)/63.5f;
                    float distance=Mathf.Sqrt(dx*dx+dy*dy);
                    float alpha=Mathf.Pow(
                        Mathf.Clamp01(1f-distance),2f);
                    pixels[y*size+x]=new Color32(
                        255,255,255,(byte)Mathf.RoundToInt(255f*alpha));
                }
            texture.SetPixels32(pixels);
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.Apply(false,true);
            haze=Sprite.Create(texture,new Rect(0,0,size,size),
                new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect);
            return haze;
        }

        // Each network biome has a small, one-time 128px atmospheric mask.
        // Painting haze only onto walkable cells keeps solid cave walls dark.
        static Sprite MaskedHaze(LevelDefinition level,float x,float y,
            float w,float h)
        {
            const int size=128;
            Texture2D texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            Color32[] pixels=new Color32[size*size];
            for(int j=0;j<size;j++)
                for(int i=0;i<size;i++)
                {
                    float fx=(i+.5f)/size;
                    float fy=(j+.5f)/size;
                    float dx=(fx-.5f)*2f;
                    float dy=(fy-.5f)*2f;
                    float distance=Mathf.Sqrt(dx*dx+dy*dy);
                    float falloff=Mathf.Pow(Mathf.Clamp01(1f-distance),2f);
                    // Cave-row coordinates grow down; sprite Y grows up.
                    float worldX=x+(fx-.5f)*w;
                    float jsonY=y+(.5f-fy)*h;
                    int cellX=Mathf.FloorToInt(worldX);
                    int cellY=Mathf.FloorToInt(jsonY);
                    if(NecroCaveNetwork.IsWall(level,cellX,cellY))
                        falloff=0f;
                    pixels[j*size+i]=new Color32(255,255,255,
                        (byte)Mathf.RoundToInt(255f*falloff));
                }
            texture.SetPixels32(pixels);
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.Apply(false,true);
            return Sprite.Create(texture,new Rect(0,0,size,size),
                new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect);
        }

        static void Patch(LevelDefinition level,Transform parent,
            string label,float x,float y,float w,float h,Color tint)
        {
            GameObject patch=new GameObject("Cave biome: "+label);
            patch.transform.SetParent(parent,false);
            Vector2 point=NecroLevels.At(level,x,y);
            patch.transform.position=new Vector3(point.x,point.y,0f);
            SpriteRenderer art=patch.AddComponent<SpriteRenderer>();
            art.sprite=NecroCaveNetwork.HasNetwork(level) ?
                MaskedHaze(level,x,y,w,h) : Haze();
            art.sortingOrder=-49;
            art.color=tint;
            Vector2 spriteSize=art.sprite.bounds.size;
            patch.transform.localScale=new Vector3(
                w/spriteSize.x,h/spriteSize.y,1f);
        }

        static void Landmark(LevelDefinition level,Transform parent,
            string kind,float x,float y)
        {
            Sprite asset=NecroScenery.Get(kind);
            GameObject mark=new GameObject("Cave landmark: "+kind);
            mark.transform.SetParent(parent,false);
            Vector2 at=NecroLevels.At(level,x,y);
            mark.transform.position=new Vector3(at.x,at.y,0f);
            SpriteRenderer render=mark.AddComponent<SpriteRenderer>();
            render.sprite=asset;
            render.sortingOrder=-5;
            Vector2 size=asset.bounds.size;
            mark.transform.localScale=new Vector3(
                .72f/size.x,.72f/size.y,1f);
        }

        public static void Populate(LevelDefinition level,Transform parent)
        {
            if(level==null||parent==null||level.id!="bonehollow")return;
            if(NecroCaveNetwork.HasNetwork(level) && level.biomes!=null)
            {
                for(int i=0;i<level.biomes.Length;i++)
                {
                    CaveBiome biome=level.biomes[i];
                    Color tint=biome.id=="surface" ?
                        new Color(.62f,.53f,.39f,.19f) :
                        biome.id=="ossadas" || biome.id=="ossuario" ?
                        new Color(.67f,.49f,.45f,.16f) :
                        biome.id=="fungos" ?
                        new Color(.27f,.66f,.51f,.18f) :
                        biome.id=="poco" ?
                        new Color(.44f,.34f,.65f,.17f) :
                        biome.id=="cristais" ?
                        new Color(.32f,.65f,.81f,.19f) :
                        biome.id=="lago" ?
                        new Color(.24f,.55f,.62f,.18f) :
                        biome.id=="covil" ?
                        new Color(.67f,.31f,.37f,.15f) :
                        new Color(.52f,.48f,.82f,.20f);
                    Patch(level,parent,biome.title,biome.x,biome.y,
                        biome.rx*1.8f,biome.ry*1.8f,tint);
                    if(biome.id=="surface")
                    {
                        // Visible daylight fades as the player leaves the
                        // rock opening; the mask excludes solid tunnel walls.
                        Patch(level,parent,"Fenda de luz solar",
                            biome.x+2f,biome.y-3.5f,14f,17f,
                            new Color(1f,.84f,.58f,.28f));
                    }
                    // A handful of landmarks, safely outside the spawn center.
                    if(biome.id=="covil" || biome.id=="selo")continue;
                    string kind=biome.id=="surface"?"lantern":
                        biome.id=="fungos"?"fungus":
                        biome.id=="cristais"?"crystal":
                        biome.id=="lago"?"pool":
                        biome.id=="poco"?"rune":"bones";
                    float landmarkX=biome.x+3.5f,landmarkY=biome.y+2.5f;
                    if(NecroCaveNetwork.HasClearance(level,landmarkX,
                        landmarkY,.75f))
                        Landmark(level,parent,kind,landmarkX,landmarkY);
                }
                return;
            }
            Patch(level,parent,"Entrada",8f,28.9f,15f,11f,
                new Color(.40f,.39f,.56f,.25f));
            Patch(level,parent,"Ossadas",18f,24f,20f,13f,
                new Color(.69f,.48f,.43f,.20f));
            Patch(level,parent,"Fungos",35f,27f,22f,14f,
                new Color(.31f,.65f,.48f,.24f));
            Patch(level,parent,"Poco",28f,17f,19f,14f,
                new Color(.54f,.38f,.65f,.20f));
            Patch(level,parent,"Cristais",17f,10f,20f,15f,
                new Color(.31f,.61f,.78f,.26f));
            Patch(level,parent,"Covil",46f,12f,18f,14f,
                new Color(.65f,.36f,.40f,.20f));
            Patch(level,parent,"Selo",49f,5.6f,15f,10f,
                new Color(.41f,.73f,.68f,.20f));

            Landmark(level,parent,"lantern",5.2f,28.6f);
            Landmark(level,parent,"bones",19.1f,23.1f);
            Landmark(level,parent,"bones",20f,28.7f);
            Landmark(level,parent,"fungus",32.5f,30.8f);
            Landmark(level,parent,"fungus",36.8f,24.9f);
            Landmark(level,parent,"crystal",13.6f,7.5f);
            Landmark(level,parent,"crystal",20.4f,9.5f);
            Landmark(level,parent,"rune",46.3f,7.4f);
            Landmark(level,parent,"bones",42.1f,13.5f);
        }
    }
}

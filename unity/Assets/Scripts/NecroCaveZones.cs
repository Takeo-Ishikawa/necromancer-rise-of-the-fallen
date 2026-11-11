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

        static void Patch(LevelDefinition level,Transform parent,
            string label,float x,float y,float w,float h,Color tint)
        {
            GameObject patch=new GameObject("Cave biome: "+label);
            patch.transform.SetParent(parent,false);
            Vector2 point=NecroLevels.At(level,x,y);
            patch.transform.position=new Vector3(point.x,point.y,0f);
            SpriteRenderer art=patch.AddComponent<SpriteRenderer>();
            art.sprite=Haze();
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

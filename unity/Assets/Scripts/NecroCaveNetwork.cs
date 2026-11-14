using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Collision, deterministic mineral material and room palette for one
    /// continuous cave. It builds one texture, not thousands of wall objects.
    /// The JSON grid is authoritative for both collision and minimap.
    /// </summary>
    public static class NecroCaveNetwork
    {
        const int PixelsPerUnit=16;
        static Sprite cachedGround;
        static LevelDefinition cachedLevel;
        static readonly Color32[] BiomeColors={
            new Color32(84,77,91,255),   // surface
            new Color32(78,65,73,255),   // ossadas
            new Color32(45,75,69,255),   // fungos
            new Color32(63,54,86,255),   // poço
            new Color32(58,70,98,255),   // cristais
            new Color32(86,71,72,255),   // ossuário
            new Color32(43,71,80,255),   // lago
            new Color32(86,61,72,255),   // covil
            new Color32(75,65,104,255)   // selo
        };

        public static bool HasNetwork(LevelDefinition level)
        {
            return level!=null && level.caveRows!=null &&
                level.caveRows.Length==(int)level.height;
        }

        /// <summary>JSON coordinates: (0,0) is north-west.</summary>
        public static bool IsWall(LevelDefinition level,int column,int row)
        {
            if(!HasNetwork(level))return false;
            if(row<0 || column<0 || row>=level.caveRows.Length ||
                column>=level.caveRows[row].Length)return true;
            return level.caveRows[row][column]=='#';
        }

        /// <summary>Unity coordinates: Y points upward. Radius-aware box test.</summary>
        public static bool Blocked(LevelDefinition level,Vector2 at,float radius)
        {
            if(!HasNetwork(level))return false;
            int x0=Mathf.FloorToInt(at.x-radius);
            int x1=Mathf.FloorToInt(at.x+radius);
            int y0=Mathf.FloorToInt(level.height-at.y-radius);
            int y1=Mathf.FloorToInt(level.height-at.y+radius);
            float radiusSquared=radius*radius;
            for(int y=y0;y<=y1;y++)
                for(int x=x0;x<=x1;x++)
                {
                    if(!IsWall(level,x,y))continue;
                    float nearestX=Mathf.Clamp(at.x,x,x+1f);
                    float bottom=level.height-y-1f;
                    float nearestY=Mathf.Clamp(at.y,bottom,bottom+1f);
                    float dx=at.x-nearestX,dy=at.y-nearestY;
                    if(dx*dx+dy*dy<=radiusSquared)return true;
                }
            return false;
        }

        /// <summary>For props whose coordinates follow levels.json (Y downward).</summary>
        public static bool HasClearance(LevelDefinition level,float jsonX,
            float jsonY,float radius)
        {
            return !Blocked(level,NecroLevels.At(level,jsonX,jsonY),radius);
        }

        public static int NearestBiome(LevelDefinition level,float x,float y)
        {
            if(level==null || level.biomes==null || level.biomes.Length==0)
                return 0;
            float best=float.MaxValue;
            int winner=0;
            for(int i=0;i<level.biomes.Length;i++)
            {
                CaveBiome biome=level.biomes[i];
                float dx=(x-biome.x)/Mathf.Max(1f,biome.rx);
                float dy=(y-biome.y)/Mathf.Max(1f,biome.ry);
                float dist=dx*dx+dy*dy;
                if(dist<best){best=dist;winner=i;}
            }
            return winner;
        }

        static uint Hash(int x,int y)
        {
            unchecked
            {
                uint n=(uint)(x*374761393+y*668265263);
                n=(n^(n>>13))*1274126177u;
                return n^(n>>16);
            }
        }

        static Color32 CellColor(LevelDefinition level,int x,int row)
        {
            bool wall=IsWall(level,x,row);
            // Slow geological bands mixed with fine variations avoid a rigid
            // single-color grid, without spawning per-cell GameObjects.
            int fine=(int)(Hash(x*7,row*11)&15u)-7;
            int stratum=(int)((Hash(x/5,row/4)>>8)&11u)-5;
            int variation=fine+stratum;
            bool openNorth=!IsWall(level,x,row-1);
            bool openSouth=!IsWall(level,x,row+1);
            bool openWest=!IsWall(level,x-1,row);
            bool openEast=!IsWall(level,x+1,row);
            if(wall)
            {
                // Light from the cave mouth reaches the upper-left rims.
                // The south-facing wall has a darker vertical face.
                bool exposed=openNorth || openSouth || openWest || openEast;
                if(exposed)
                {
                    int highlight=openNorth?12:openWest?7:
                        openEast?0:-7;
                    return new Color32((byte)(74+variation+highlight),
                        (byte)(65+variation+highlight),
                        (byte)(83+variation+highlight),255);
                }
                return new Color32((byte)(24+variation/2),
                    (byte)(22+variation/2),(byte)(32+variation/2),255);
            }

            int biome=NearestBiome(level,x+.5f,row+.5f);
            Color32 baseColor=BiomeColors[Mathf.Clamp(biome,0,BiomeColors.Length-1)];
            bool nearWall=!openNorth || !openSouth || !openWest || !openEast;
            int shade=nearWall?-13:0;

            // The first chamber is adjacent to the surface. A small warm
            // daylight gradient disappears gradually into the tunnel network.
            float dx=(x+.5f-level.startX)/12f;
            float dy=(row+.5f-level.startY)/10f;
            int daylight=Mathf.RoundToInt(23f*
                Mathf.Clamp01(1f-dx*dx-dy*dy));
            int red=baseColor.r+variation+shade+daylight;
            int green=baseColor.g+variation+shade+daylight*3/4;
            int blue=baseColor.b+variation+shade+daylight/3;
            return new Color32((byte)Mathf.Clamp(red,0,255),
                (byte)Mathf.Clamp(green,0,255),
                (byte)Mathf.Clamp(blue,0,255),255);
        }

        public static Sprite Ground(LevelDefinition level)
        {
            if(!HasNetwork(level))return null;
            if(cachedGround!=null && cachedLevel==level)return cachedGround;
            int columns=Mathf.RoundToInt(level.width);
            int rows=level.caveRows.Length;
            int width=columns*PixelsPerUnit,height=rows*PixelsPerUnit;

            // One baked RGB texture avoids GameObjects for 16,640 cave cells.
            Color32[] palette=new Color32[columns*rows];
            for(int row=0;row<rows;row++)
                for(int col=0;col<columns;col++)
                    palette[row*columns+col]=CellColor(level,col,row);

            Color32[] pixels=new Color32[width*height];
            for(int y=0;y<height;y++)
            {
                int row=rows-1-y/PixelsPerUnit;
                int pixelRow=y*width;
                int paletteRow=row*columns;
                for(int x=0;x<width;x++)
                {
                    Color32 baseColor=palette[paletteRow+x/PixelsPerUnit];
                    int grain=(int)(Hash(x,y)&7u)-3;
                    // Fine low-contrast specks and sediment lines break the
                    // flat material without making silhouettes hard to read.
                    int mineral=(Hash(x/9,y/7)&63u)==0u ? 5 : 0;
                    pixels[pixelRow+x]=new Color32(
                        (byte)(baseColor.r+grain+mineral),
                        (byte)(baseColor.g+grain+mineral),
                        (byte)(baseColor.b+grain+mineral),255);
                }
            }
            Texture2D texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            texture.name="Bonehollow connected cave terrain";
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            cachedGround=Sprite.Create(texture,new Rect(0,0,width,height),
                new Vector2(.5f,.5f),PixelsPerUnit,0,SpriteMeshType.FullRect);
            cachedLevel=level;
            return cachedGround;
        }
    }
}

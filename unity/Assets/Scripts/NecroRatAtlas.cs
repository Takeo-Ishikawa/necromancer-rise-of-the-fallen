using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Hand-painted rat sprite atlases: 4x8 walking directions, 8-frame
    /// attack strip and 10-frame death strip. Caches all sliced sprites.
    /// Missing assets return null to preserve existing procedural fallback.
    /// </summary>
    public static class NecroRatAtlas
    {
        public const float AttackDuration=.48f;
        public const float DeathDuration=.56f;
        static Sprite[] walk,attack,death;
        static bool triedWalk,triedAttack,triedDeath;

        static Sprite[] Load(string path,int columns,int rows)
        {
            Texture2D atlas=Resources.Load<Texture2D>("Creatures/"+path);
            if(atlas==null)return null;
            if(atlas.width%columns!=0 || atlas.height%rows!=0 ||
                atlas.width/columns!=atlas.height/rows)
            {
                Debug.LogWarning("[Necromancer] Invalid rat atlas "+path+
                    " ("+atlas.width+"x"+atlas.height+")");
                return null;
            }
            int cell=atlas.width/columns;
            Sprite[] frames=new Sprite[columns*rows];
            for(int y=0;y<rows;y++)
                for(int x=0;x<columns;x++)
                {
                    int i=y*columns+x;
                    frames[i]=Sprite.Create(atlas,
                        new Rect(x*cell,(rows-y-1)*cell,cell,cell),
                        new Vector2(.5f,.19f),cell,0,SpriteMeshType.FullRect);
                    frames[i].name=path+"_"+y+"_"+x;
                }
            return frames;
        }

        static Sprite[] WalkFrames()
        {
            if(!triedWalk){triedWalk=true;walk=Load("rat_move",4,8);}
            return walk;
        }

        static Sprite[] AttackFrames()
        {
            if(!triedAttack){triedAttack=true;attack=Load("rat_attack",8,1);}
            return attack;
        }

        static Sprite[] DeathFrames()
        {
            if(!triedDeath){triedDeath=true;death=Load("rat_death",10,1);}
            return death;
        }

        public static bool HasWalk { get {return WalkFrames()!=null;} }
        public static bool HasAttack { get {return AttackFrames()!=null;} }
        public static bool HasDeath { get {return DeathFrames()!=null;} }

        public static int Facing8(Vector2 heading,int previous)
        {
            if(heading.sqrMagnitude<.0001f)return previous;
            float angle=Mathf.Atan2(heading.x,-heading.y)*Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle/45f)+8)%8;
        }

        public static Sprite Walk(int facing,float time,bool moving)
        {
            Sprite[] frames=WalkFrames();
            if(frames==null)return null;
            int direction=((facing%8)+8)%8;
            int step=moving?Mathf.FloorToInt(Mathf.Max(0f,time)*10f)%4:0;
            return frames[direction*4+step];
        }

        public static Sprite Attack(float elapsed)
        {
            Sprite[] frames=AttackFrames();
            if(frames==null)return null;
            int index=Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Max(0f,elapsed)/AttackDuration*frames.Length),
                0,frames.Length-1);
            return frames[index];
        }

        public static Sprite Death(float elapsed)
        {
            Sprite[] frames=DeathFrames();
            if(frames==null)return null;
            int index=Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Max(0f,elapsed)/DeathDuration*frames.Length),
                0,frames.Length-1);
            return frames[index];
        }
    }
}

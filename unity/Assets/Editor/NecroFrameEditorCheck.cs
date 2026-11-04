using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>
    /// Tests sprite-atlas creation in the actual Unity Editor on CI.
    /// This validates sprite boundaries, real clip frames, directional variants,
    /// and source-asset reuse before compiling the Android player.
    /// </summary>
    public static class NecroFrameEditorCheck
    {
        public static void Verify()
        {
            Texture2D source=Resources.Load<Texture2D>(
                "AnimationSheets/necromante_source");
            if(source==null || source.width%4!=0 || source.height%4!=0)
                throw new BuildFailedException(
                    "Illustrated four-direction heroine source sheet is missing.");
            int w,h;
            Color32[] sourcePixels=NecroFrameLibrary.ReadablePixels(source,out w,out h);
            int cellW=w/4,cellH=h/4;
            int[] hashes=new int[4];
            for(int d=0;d<4;d++)
            {
                int filled=0,hash=17;
                for(int y=3*cellH;y<4*cellH;y++)
                for(int x=d*cellW;x<(d+1)*cellW;x++)
                {
                    Color32 c=sourcePixels[y*w+x];
                    if(c.a>40)filled++;
                    unchecked
                    {
                        hash=hash*31+c.r;
                        hash=hash*31+c.g;
                        hash=hash*31+c.b;
                        hash=hash*31+c.a;
                    }
                }
                if(filled<45)
                    throw new BuildFailedException("Empty hero direction "+d);
                hashes[d]=hash;
            }
            for(int d=0;d<4;d++)
                for(int j=d+1;j<4;j++)
                    if(hashes[d]==hashes[j])
                        throw new BuildFailedException(
                            "Duplicated heroine facing illustration "+d+"/"+j);
            VerifySet("necromante",true);
            VerifySet("rat",false);
            VerifySet("bat",false);

            if(!NecroFrameLibrary.Supports("grave_lord") ||
               !NecroFrameLibrary.Supports("marsh_lord") ||
               NecroFrameLibrary.Supports("spider"))
                throw new BuildFailedException("Frame-supported creature list regressed.");

            Debug.Log("[Necromancer 0.6.5a] Painted directional hero source, "+
                "(hero, rat, bat, four directions, frame clips).");
        }

        static void VerifySet(string kind,bool hero)
        {
            Sprite original=NecroArt.Get(kind);
            if(original==null)
                throw new BuildFailedException("Missing animation source: "+kind);

            NecroFrameSet frames=NecroFrameLibrary.Get(kind,original);
            if(frames==null || frames.Atlas==null)
                throw new BuildFailedException("Atlas generation failed: "+kind);

            int cell=hero?96:80;
            int rows=hero?24:16;
            int columns=hero?8:6;
            if(frames.Cell!=cell || frames.Atlas.width!=cell*columns ||
               frames.Atlas.height!=cell*rows)
                throw new BuildFailedException("Atlas dimensions invalid: "+kind);

            NecroPose[] poses=hero?
                new[]{NecroPose.Idle,NecroPose.Walk,NecroPose.Attack,
                    NecroPose.Cast,NecroPose.Raise,NecroPose.Hurt}:
                new[]{NecroPose.Idle,NecroPose.Walk,NecroPose.Attack,NecroPose.Hurt};

            if(hero)
            {
                int[] expected={4,8,6,6,8,3};
                for(int pose=0;pose<expected.Length;pose++)
                    if(frames.FrameCount((NecroPose)pose)!=expected[pose])
                        throw new BuildFailedException(
                            "Hero clip length differs from approved sheet plan.");
            }
            foreach(NecroPose pose in poses)
            {
                if(!frames.Has(pose) || frames.FrameCount(pose)<2 ||
                   frames.Duration(pose)<=0)
                    throw new BuildFailedException("No multi-frame action "+
                        kind+" "+pose);
                for(int d=0;d<4;d++)
                {
                    NecroDirection direction=(NecroDirection)d;
                    Sprite first=frames.Frame(pose,direction,0,false);
                    Sprite second=frames.Frame(pose,direction,1f/24f,false);
                    Sprite last=frames.Frame(pose,direction,1000f,false);
                    if(first==null || last==null || second==null ||
                       first.texture!=frames.Atlas ||
                       first.rect.width!=cell ||
                       first.rect.height!=cell ||
                       first.rect.x<0 || first.rect.y<0 ||
                       first.rect.xMax>frames.Atlas.width ||
                       first.rect.yMax>frames.Atlas.height)
                        throw new BuildFailedException("Sprite bounds invalid "+
                            kind+" "+pose+" "+direction);
                    if(last==first)
                        throw new BuildFailedException("Animation does not advance "+
                            kind+" "+pose+" "+direction);
                }
            }

            // The frame set is cached rather than re-rasterized for every actor.
            if(!ReferenceEquals(frames,NecroFrameLibrary.Get(kind,original)))
                throw new BuildFailedException("Atlas cache regressed: "+kind);
        }
    }
}

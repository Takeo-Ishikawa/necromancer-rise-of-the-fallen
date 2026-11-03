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
            VerifySet("necromante",true);
            VerifySet("rat",false);
            VerifySet("bat",false);

            if(!NecroFrameLibrary.Supports("grave_lord") ||
               !NecroFrameLibrary.Supports("marsh_lord") ||
               NecroFrameLibrary.Supports("spider"))
                throw new BuildFailedException("Frame-supported creature list regressed.");

            Debug.Log("[Necromancer 0.6.3] Directional sprite atlases: PASS "+
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
            if(frames.Cell!=cell || frames.Atlas.width!=cell*6 ||
               frames.Atlas.height!=cell*rows)
                throw new BuildFailedException("Atlas dimensions invalid: "+kind);

            NecroPose[] poses=hero?
                new[]{NecroPose.Idle,NecroPose.Walk,NecroPose.Attack,
                    NecroPose.Cast,NecroPose.Raise,NecroPose.Hurt}:
                new[]{NecroPose.Idle,NecroPose.Walk,NecroPose.Attack,NecroPose.Hurt};

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

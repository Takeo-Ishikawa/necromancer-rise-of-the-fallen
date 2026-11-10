using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Eight-direction Rotboar atlases (32 frames each, 4 columns by 8 rows).
    /// PNGs remain in Resources/Creatures/Rotboar and are optional until
    /// imported; original three-frame assets are retained as a safe fallback.
    /// </summary>
    public partial class NecroPrototype
    {
        const int BoarWalk=0;
        const int BoarAttack=1;
        const int BoarDeath=2;
        const int BoarReanimate=3;
        readonly Sprite[][] rotboarAtlases=new Sprite[8][];
        readonly bool[] rotboarAtlasTried=new bool[8];

        static string RotboarAtlasPath(bool ally,int action)
        {
            string name=ally?"rotboar_ally":"rotboar";
            switch(action)
            {
                case BoarWalk:return "Creatures/Rotboar/"+name+"_move";
                case BoarAttack:return "Creatures/Rotboar/"+name+"_attack";
                case BoarDeath:return "Creatures/Rotboar/"+name+"_death";
                default:return "Creatures/Rotboar/"+name+"_reanimate";
            }
        }

        Sprite[] GetRotboarAtlas(bool ally,int action)
        {
            int key=(ally?4:0)+action;
            if(rotboarAtlasTried[key])return rotboarAtlases[key];
            rotboarAtlasTried[key]=true;
            string path=RotboarAtlasPath(ally,action);
            Texture2D sheet=Resources.Load<Texture2D>(path);
            if(sheet==null)return null; // Do not break old APK builds.
            if(sheet.width%4!=0 || sheet.height%8!=0 ||
                sheet.width/4!=sheet.height/8)
            {
                Debug.LogWarning("Necromancer: invalid Rotboar atlas grid: "+path);
                return null;
            }
            int cell=sheet.width/4;
            var frames=new Sprite[32];
            for(int row=0;row<8;row++)
                for(int col=0;col<4;col++)
                    frames[row*4+col]=Sprite.Create(sheet,
                        new Rect(col*cell,(7-row)*cell,cell,cell),
                        new Vector2(.5f,.18f),cell,0,SpriteMeshType.FullRect);
            rotboarAtlases[key]=frames;
            return frames;
        }

        static int BoarDirection8(Vector2 delta,int previous)
        {
            if(delta.sqrMagnitude<.0001f)return previous;
            float angle=Mathf.Atan2(delta.x,-delta.y)*Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle/45f)+8)%8;
        }

        void RenderRotboar(Actor boar,float dt)
        {
            if(boar==null || boar.body==null || boar.boarArt==null)return;
            if(Time.time<boar.reanimationUntil)return;
            Vector2 at=boar.body.position;
            Vector2 motion=at-boar.boarLastPosition;
            boar.boarLastPosition=at;
            if(boar.boarPose==BoarPose.Idle && motion.sqrMagnitude>.000004f)
                boar.boarFacingIndex=BoarDirection8(motion,boar.boarFacingIndex);
            else if(boar.boarPose!=BoarPose.Idle)
                boar.boarFacingIndex=BoarDirection8(
                    boar.chargeDirection,boar.boarFacingIndex);
            bool moving=motion.sqrMagnitude>.000004f;
            if(boar.boarPose==BoarPose.Idle && !moving)
                boar.boarFrameClock=0f;
            else
                boar.boarFrameClock+=Mathf.Max(0f,dt);
            int action=boar.boarPose==BoarPose.Idle?BoarWalk:BoarAttack;
            Sprite[] atlas=GetRotboarAtlas(boar.ally,action);
            if(atlas==null)return;
            int frame=boar.boarPose==BoarPose.Idle && !moving?0:
                Mathf.FloorToInt(boar.boarFrameClock*
                    (boar.boarPose==BoarPose.Idle?8f:9f))%4;
            boar.boarArt.sprite=atlas[boar.boarFacingIndex*4+frame];
            boar.boarArt.flipX=false;
        }
    }
}

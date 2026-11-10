using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.2: compact, readable progress status in the active (IMGUI) HUD.
    /// The older painted HUD is not always on the render path, so level-up
    /// messages must be drawn here rather than relying on its notification.
    /// No external art and no new UI click targets.
    /// </summary>
    public partial class NecroPrototype
    {
        void DrawProgressOverlay(float width,float height)
        {
            if(!inSanctuary)
            {
                float x=124f, w=Mathf.Max(120f,width-138f);
                Rect panel=new Rect(x,12f,w,77f);
                DrawWardrobeSurface(panel,
                    new Color(.09f,.065f,.13f,.94f),WardrobeMutedEdge);
                GUI.Label(new Rect(x+7f,15f,w-14f,20f),
                    "NIVEL "+playerLevel+"   XP "+
                    experience+"/"+ExperienceThreshold(),hudSmall);
                float progress=Mathf.Clamp01((float)experience/
                    Mathf.Max(1,ExperienceThreshold()));
                FillUi(new Rect(x+9f,39f,w-18f,6f),
                    new Color(.25f,.19f,.31f,.98f));
                FillUi(new Rect(x+9f,39f,(w-18f)*progress,6f),
                    new Color(.79f,.61f,.91f,.98f));
                // No altar actions are accessible in this simplified build.
                GUI.Label(new Rect(x+6f,49f,w-12f,17f),
                    resourceHint,wardrobeNoteStyle);
                GUI.Label(new Rect(x+6f,65f,w-12f,17f),
                    "FRAGMENTOS "+shards+" | RECURSOS "+
                    current.resources.Count,wardrobeNoteStyle);
            }

            if(Time.time<noticeUntil)
            {
                // Keep transient combat/level notifications clear of travel
                // navigation and fountain buff status near the screen top.
                float y=inSanctuary ? 156f :
                    SanctuaryTravelAvailable() ? 120f : 94f;
                Rect banner=new Rect(15f,y,width-30f,37f);
                DrawWardrobeSurface(banner,
                    new Color(.095f,.069f,.134f,.96f),WardrobeEdge);
                GUI.Label(new Rect(banner.x+8f,banner.y+4f,
                    banner.width-16f,banner.height-8f),notice,hudSmall);
            }
            if(!inSanctuary && Time.time<resourceToastUntil)
            {
                // Never cover main notifications, portal controls or XP panel.
                float toastY=Time.time<noticeUntil?
                    (SanctuaryTravelAvailable()?164f:138f):96f;
                DrawResourceRewardToast(width,toastY);
            }
        }
    }
}

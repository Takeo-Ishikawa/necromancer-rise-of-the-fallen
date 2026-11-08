using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.5: one targeting policy for Grave Pups, Rotboars and other allies.
    /// Hunt/Defend uses the already persisted huntOrder save field.
    /// </summary>
    public partial class NecroPrototype
    {
        const float ServantLeashDistance=6.5f;
        const float HuntPlayerRange=6.3f;
        const float DefensePlayerRange=3.8f;
        const float HuntServantRange=5.6f;
        const float DefenseServantRange=3.6f;

        bool IsCompanionTargetValid(Actor ally,Actor enemy,
            float playerRange,float allyRange)
        {
            return ally!=null && ally.body!=null && enemy!=null &&
                enemy.body!=null && enemy.hp>0f && !enemy.ally &&
                player!=null && player.body!=null &&
                Vector2.Distance(player.body.position,enemy.body.position)<=playerRange &&
                Vector2.Distance(ally.body.position,enemy.body.position)<=allyRange &&
                Visible(ally.body.position,enemy.body.position);
        }

        Actor FindServantTarget(Actor ally)
        {
            if(inSanctuary || current==null || player==null ||
                player.body==null || ally==null || ally.body==null)return null;
            if(servantOrder!=null && (servantOrder.hp<=0f ||
                servantOrder.body==null))servantOrder=null;
            if(graveCommandTarget!=null && (!GraveCommandActive ||
                graveCommandTarget.hp<=0f || graveCommandTarget.body==null))
                graveCommandTarget=null;

            // Never chase indefinitely beyond the necromancer's command radius.
            if(Vector2.Distance(ally.body.position,player.body.position)>
                ServantLeashDistance)return null;

            if(GraveCommandActive &&
                IsCompanionTargetValid(ally,graveCommandTarget,7.2f,7.2f))
                return graveCommandTarget;
            // Basic attack marked targets remain a priority in either stance.
            if(IsCompanionTargetValid(ally,servantOrder,6.3f,6.2f))
                return servantOrder;

            float playerRange=huntOrder?HuntPlayerRange:DefensePlayerRange;
            float servantRange=huntOrder?HuntServantRange:DefenseServantRange;
            Actor nearest=null;
            float nearestDistance=servantRange;
            foreach(Actor enemy in current.enemies)
            {
                if(!IsCompanionTargetValid(ally,enemy,playerRange,
                    servantRange))continue;
                float distance=Vector2.Distance(ally.body.position,
                    enemy.body.position);
                if(distance<nearestDistance)
                {
                    nearestDistance=distance;
                    nearest=enemy;
                }
            }
            return nearest;
        }

        Rect TacticModeButtonRect(float scale)
        {
            float w=Screen.width/scale,h=Screen.height/scale;
            return new Rect(w-154f,h-274f,142f,46f);
        }

        bool CanChangeTacticMode()
        {
            return !inSanctuary && !gameOver && player!=null &&
                player.hp>0f && !inventoryOpen && !wardrobeOpen &&
                !meditationOpen && !cryptOpen && !upgradeOpen &&
                !armyInspect && !gearInspect;
        }

        void ToggleServantTactics()
        {
            if(!CanChangeTacticMode())return;
            bool previous=huntOrder;
            huntOrder=!huntOrder;
            if(!SaveProgress())
            {
                huntOrder=previous;
                Announce("Nao foi possivel salvar a postura dos servos.");
                return;
            }
            Announce(huntOrder ?
                "Servos em CACA: perseguem inimigos proximos." :
                "Servos em DEFESA: protegem a necromante.");
        }

        void ReadServantTacticsInput(float scale)
        {
            if(!CanChangeTacticMode())return;
            bool pressed=Input.GetKeyDown(KeyCode.T);
            Rect rect=TacticModeButtonRect(scale);
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch=Input.GetTouch(i);
                if(touch.phase!=TouchPhase.Began)continue;
                Vector2 point=new Vector2(touch.position.x,
                    Screen.height-touch.position.y)/scale;
                if(rect.Contains(point))pressed=true;
            }
            if(Input.touchCount==0 && Input.GetMouseButtonDown(0))
            {
                Vector3 at=Input.mousePosition;
                Vector2 point=new Vector2(at.x,Screen.height-at.y)/scale;
                if(rect.Contains(point))pressed=true;
            }
            if(pressed)ToggleServantTactics();
        }

        void DrawServantTacticsButton(float scale)
        {
            EnsureHudArt();
            Rect rect=TacticModeButtonRect(scale);
            bool hunting=huntOrder;
            DrawWardrobeSurface(rect,
                hunting ? new Color(.26f,.12f,.20f,.96f) :
                    new Color(.085f,.18f,.19f,.96f),
                hunting ? new Color(.84f,.62f,.73f,.94f) :
                    new Color(.55f,.84f,.80f,.94f));
            GUI.Label(new Rect(rect.x+5f,rect.y+2f,rect.width-10f,24f),
                hunting?"POSTURA: CACA":"POSTURA: DEFESA",hudAction);
            GUI.Label(new Rect(rect.x+5f,rect.y+24f,rect.width-10f,18f),
                "T - ALTERAR",hudSmall);
        }
    }
}

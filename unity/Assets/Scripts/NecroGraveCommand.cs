using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.4: an active tactical order for existing companions.
    /// R on Windows or ORDEM on touch screens. Temporary modifiers affect
    /// attacks and movement at runtime; saved servant stats never change.
    /// </summary>
    public partial class NecroPrototype
    {
        const int GraveCommandManaCost=20;
        const float GraveCommandDuration=8f;
        const float GraveCommandCooldownSeconds=18f;
        const float GraveCommandDamageBonus=1.25f;
        const float GraveCommandSpeedBonus=1.20f;
        float graveCommandUntil,graveCommandCooldownUntil;
        float graveCommandFailureNoticeUntil;
        Actor graveCommandTarget;

        bool GraveCommandActive
        {
            get { return !inSanctuary && Time.time<graveCommandUntil; }
        }

        float GraveCommandDamageMultiplier
        {
            get { return GraveCommandActive ? GraveCommandDamageBonus : 1f; }
        }

        float GraveCommandSpeedMultiplier(Actor actor)
        {
            return actor!=null && actor.ally && actor!=player &&
                GraveCommandActive ? GraveCommandSpeedBonus : 1f;
        }

        int LivingServantCount()
        {
            int total=0;
            foreach(Actor ally in servants)
                if(ally!=null && ally.hp>0f && ally.body!=null)total++;
            return total;
        }

        Rect GraveCommandButtonRect(float scale)
        {
            float w=Screen.width/scale,h=Screen.height/scale;
            // Three touch targets in one row: ORDEM, PULSO and ATACAR.
            return new Rect(w-282f,h-208f,78f,78f);
        }

        bool CanCastGraveCommand()
        {
            if(!HeroCanUseMagic)return false;
            return !inSanctuary && current!=null &&
                player!=null && player.hp>0f && !gameOver &&
                !inventoryOpen && !wardrobeOpen && !meditationOpen &&
                !cryptOpen && !upgradeOpen && !armyInspect && !gearInspect &&
                LivingServantCount()>0 && mana>=GraveCommandManaCost &&
                Time.time>=graveCommandCooldownUntil;
        }

        bool TryCastGraveCommand()
        {
            if(!HeroCanUseMagic)return false;
            if(inSanctuary || current==null || player==null ||
                player.hp<=0f || gameOver || inventoryOpen || wardrobeOpen ||
                meditationOpen || cryptOpen || upgradeOpen ||
                armyInspect || gearInspect)return false;
            if(Time.time<graveCommandCooldownUntil)return false;
            int living=LivingServantCount();
            if(living==0)
            {
                if(Time.time>=graveCommandFailureNoticeUntil)
                {
                    Announce("Ordem dos Mortos: nenhum servo ativo.");
                    graveCommandFailureNoticeUntil=Time.time+1f;
                }
                return false;
            }
            if(mana<GraveCommandManaCost)
            {
                if(Time.time>=graveCommandFailureNoticeUntil)
                {
                    Announce("Ordem dos Mortos: anima insuficiente.");
                    graveCommandFailureNoticeUntil=Time.time+1f;
                }
                return false;
            }

            mana-=GraveCommandManaCost;
            graveCommandUntil=Time.time+GraveCommandDuration;
            graveCommandCooldownUntil=Time.time+GraveCommandCooldownSeconds;
            heroineRaisePulse=.42f;

            // Prioritize a visible enemy for allied grave pups, rotboars,
            // and generic servants while they are within engagement range.
            // Do not overwrite the manual attack order: the focus target
            // is temporary and disappears when the buff ends.
            graveCommandTarget=Closest(7.2f);

            // Existing ritual circles, limited by the CombatFx effect pool.
            NecroCombatFx.Emit(player.body.position,current.root.transform,4);
            int effects=0;
            foreach(Actor ally in servants)
            {
                if(ally==null || ally.hp<=0f || ally.body==null)continue;
                if(effects>=8)break;
                NecroCombatFx.Emit(ally.body.position,current.root.transform,4);
                effects++;
            }
            SaveProgress();
            Announce("Ordem dos Mortos: "+living+
                (living==1?" servo fortalecido.":" servos fortalecidos."));
            return true;
        }

        void ReadGraveCommandInput(float scale)
        {
            if(!HeroCanUseMagic)return;
            bool pressed=Input.GetKeyDown(KeyCode.R);
            Rect button=GraveCommandButtonRect(scale);
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch=Input.GetTouch(i);
                if(touch.phase!=TouchPhase.Began)continue;
                Vector2 point=new Vector2(touch.position.x,
                    Screen.height-touch.position.y)/scale;
                if(button.Contains(point))pressed=true;
            }
            if(Input.touchCount==0 && Input.GetMouseButtonDown(0))
            {
                Vector3 mouse=Input.mousePosition;
                Vector2 point=new Vector2(mouse.x,Screen.height-mouse.y)/scale;
                if(button.Contains(point))pressed=true;
            }
            if(pressed)TryCastGraveCommand();
        }

        void DrawGraveCommandButton(float scale)
        {
            if(!HeroCanUseMagic)return;
            EnsureHudArt();
            Rect button=GraveCommandButtonRect(scale);
            float active=Mathf.Max(0f,graveCommandUntil-Time.time);
            float remaining=Mathf.Max(0f,graveCommandCooldownUntil-Time.time);
            bool ready=CanCastGraveCommand();
            Color edge=active>0f ?
                new Color(.58f,.92f,.79f,.98f) :
                ready ? new Color(.66f,.79f,.85f,.96f) :
                    new Color(.45f,.48f,.52f,.89f);
            Color background=active>0f ?
                new Color(.08f,.22f,.20f,.98f) :
                ready ? new Color(.09f,.16f,.23f,.97f) :
                    new Color(.075f,.09f,.12f,.94f);
            DrawWardrobeSurface(button,background,edge);
            string title=active>0f ? "ATIVO "+Mathf.CeilToInt(active)+"s" :
                remaining>0f ? Mathf.CeilToInt(remaining)+"s" : "ORDEM";
            GUI.Label(new Rect(button.x+3f,button.y+8f,
                button.width-6f,23f),title,hudAction);
            GUI.Label(new Rect(button.x+3f,button.y+34f,
                button.width-6f,18f),GraveCommandManaCost+" ANIMA",hudSmall);
            GUI.Label(new Rect(button.x+3f,button.y+54f,
                button.width-6f,18f),"R",hudAction);
            if(active>0f)
            {
                float fraction=Mathf.Clamp01(active/GraveCommandDuration);
                FillUi(new Rect(button.x+4f,button.y+button.height-4f,
                    (button.width-8f)*fraction,2f),
                    new Color(.58f,.95f,.79f,.95f));
            }
            else if(remaining>0f)
            {
                float fraction=Mathf.Clamp01(remaining/GraveCommandCooldownSeconds);
                FillUi(new Rect(button.x+4f,button.y+button.height-4f,
                    (button.width-8f)*fraction,2f),
                    new Color(.50f,.69f,.77f,.83f));
            }
        }
    }
}

using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.3: first active necromancer spell.
    /// Q on Windows or a touch button on Android. It consumes anima (unlike
    /// the free basic attack), strikes only visible hostiles near the player
    /// and never affects servants or the safe chamber.
    /// </summary>
    public partial class NecroPrototype
    {
        const int SoulPulseManaCost=18;
        const float SoulPulseRadius=2.7f;
        const float SoulPulseCooldownSeconds=9f;
        float soulPulseCooldownUntil;
        float soulPulseFailureNoticeUntil;

        Rect SoulPulseButtonRect(float scale)
        {
            float w=Screen.width/scale,h=Screen.height/scale;
            return new Rect(w-195f,h-208f,78f,78f);
        }

        void ResetActiveSkillInput()
        {
            // No effect persists across the refuge / battle boundary.
            // The cooldown itself remains active to prevent travel resets.
            soulPulseFailureNoticeUntil=0f;
            // End the active order on travel, but keep its cooldown.
            graveCommandUntil=0f;
            graveCommandFailureNoticeUntil=0f;
            graveCommandTarget=null;
        }

        bool CanCastSoulPulse()
        {
            return !inSanctuary && player!=null && player.hp>0f &&
                !gameOver && !inventoryOpen && !wardrobeOpen &&
                !meditationOpen && !cryptOpen && !upgradeOpen &&
                !armyInspect && !gearInspect &&
                mana>=SoulPulseManaCost &&
                Time.time>=soulPulseCooldownUntil;
        }

        bool TryCastSoulPulse()
        {
            if(inSanctuary || player==null || gameOver || player.hp<=0f ||
                inventoryOpen || wardrobeOpen || meditationOpen ||
                cryptOpen || upgradeOpen || armyInspect || gearInspect)return false;
            if(Time.time<soulPulseCooldownUntil)
                return false;
            if(mana<SoulPulseManaCost)
            {
                if(Time.time>=soulPulseFailureNoticeUntil)
                {
                    Announce("Anima insuficiente para Pulso Necrotico.");
                    soulPulseFailureNoticeUntil=Time.time+1f;
                }
                return false;
            }

            Vector2 center=player.body.position;
            // A spell cannot waste anima when there are no reachable targets.
            int targets=0;
            for(int i=0;i<current.enemies.Count;i++)
            {
                Actor enemy=current.enemies[i];
                if(enemy!=null && enemy.hp>0f && enemy.body!=null &&
                   Vector2.Distance(center,enemy.body.position)<=
                       SoulPulseRadius+enemy.radius &&
                   Visible(center,enemy.body.position))targets++;
            }
            if(targets==0)
            {
                if(Time.time>=soulPulseFailureNoticeUntil)
                {
                    Announce("Pulso Necrotico: nenhum inimigo proximo.");
                    soulPulseFailureNoticeUntil=Time.time+1f;
                }
                return false;
            }

            int levelBefore=playerLevel;
            mana-=SoulPulseManaCost;
            soulPulseCooldownUntil=Time.time+SoulPulseCooldownSeconds;
            // Reuse an existing ritual-circle sprite for a fast, bounded
            // pulse effect. No new textures, particles or animation sheets.
            NecroCombatFx.Emit(center,current.root.transform,3);
            heroineRaisePulse=.40f;

            // Damage() removes dead enemies. Iterate in reverse to safely
            // handle multiple deaths during the same pulse.
            float amount=(25f+3f*(playerLevel-1)+powerPoints*3f+
                staffTier*4f)*FountainDamageMultiplier;
            pulseAwardBatch=true;
            try
            {
                for(int i=current.enemies.Count-1;i>=0;i--)
                {
                    Actor enemy=current.enemies[i];
                    if(enemy==null || enemy.hp<=0f || enemy.body==null)continue;
                    if(Vector2.Distance(center,enemy.body.position)>
                        SoulPulseRadius+enemy.radius)continue;
                    if(!Visible(center,enemy.body.position))continue;
                    Damage(enemy,amount);
                }
            }
            finally { pulseAwardBatch=false; }
            // One snapshot for the whole pulse; prevent expensive repeated
            // file writes for area-of-effect multi-kills.
            SaveProgress();
            if(playerLevel>levelBefore)
                Announce("NIVEL "+playerLevel+
                    "! Visite o altar para distribuir pontos.");
            else
                Announce("Pulso Necrotico! "+targets+
                    (targets==1?" alvo atingido.":" alvos atingidos."));
            return true;
        }

        bool pulseAwardBatch;

        void ReadActiveSkillInput(float scale)
        {
            if(inSanctuary || gameOver || player==null)return;
            bool pressed=Input.GetKeyDown(KeyCode.Q);
            Rect rect=SoulPulseButtonRect(scale);
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
            if(pressed)TryCastSoulPulse();
            ReadGraveCommandInput(scale);
        }

        void DrawSoulPulseButton(float scale)
        {
            EnsureHudArt();
            float remaining=Mathf.Max(0f,soulPulseCooldownUntil-Time.time);
            Rect button=SoulPulseButtonRect(scale);
            bool ready=CanCastSoulPulse();
            Color edge=ready ?
                new Color(.77f,.58f,.95f,.95f) :
                new Color(.45f,.38f,.55f,.88f);
            Color fill=ready ?
                new Color(.18f,.105f,.29f,.97f) :
                new Color(.09f,.075f,.13f,.94f);
            DrawWardrobeSurface(button,fill,edge);
            GUI.Label(new Rect(button.x+3f,button.y+9f,button.width-6f,22f),
                remaining>0f ? Mathf.CeilToInt(remaining)+"s" : "PULSO",
                hudAction);
            GUI.Label(new Rect(button.x+3f,button.y+33f,button.width-6f,19f),
                SoulPulseManaCost+" ANIMA",hudSmall);
            GUI.Label(new Rect(button.x+3f,button.y+54f,button.width-6f,18f),
                "Q",hudAction);
            if(remaining>0f)
            {
                float ratio=Mathf.Clamp01(remaining/SoulPulseCooldownSeconds);
                FillUi(new Rect(button.x+4f,button.y+button.height-4f,
                    (button.width-8f)*ratio,2f),
                    new Color(.75f,.55f,.93f,.85f));
            }
        }
    }
}

using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.11 — physical risk for the necromancer and a safe defeat loop.
    /// Hero health is already persisted by the existing v1 save snapshot.
    /// No new assets or serialized fields are needed.
    /// </summary>
    public partial class NecroPrototype
    {
        const float HeroHitInvulnerabilitySeconds=1.10f;
        const float HeroArrivalGraceSeconds=1.10f;
        float heroDamageImmuneUntil;

        void ApplyHeroDamage(float damage)
        {
            if(inSanctuary || gameOver || player==null ||
                player.hp<=0f || Time.time<heroDamageImmuneUntil)return;
            float scaledDamage=damage*IncomingHeroDamageMultiplier();
            float amount=Mathf.Max(1f,MitigateHeroDamage(scaledDamage));
            float before=player.hp;
            player.hp=Mathf.Max(0f,player.hp-amount);
            if(before>player.maxHp*.25f && player.hp>0f &&
               player.hp<=player.maxHp*.25f)
                Announce("VIDA CRITICA! Recuar e procurar recursos.");
            heroineHitPulse=.34f;
            heroDamageImmuneUntil=Time.time+HeroHitInvulnerabilitySeconds;
            if(current!=null && current.root!=null)
                NecroCombatFx.Emit(player.body.position,
                    current.root.transform,0,true);
            if(player.hp<=0f)
                BeginHeroDefeat();
        }

        void BeginHeroDefeat()
        {
            if(gameOver)return;
            gameOver=true;
            // Stop queued projectiles/attacks and touch movement so the
            // character cannot act behind the defeat screen.
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            heroineWalking=false;
            manualQueued=false;
            releasePending=false;
            castTarget=null;
            heroineCastPulse=0f;
            heroineStrikePulse=0f;
            heroineRaisePulse=0f;
            Announce("A necromante caiu. Retorne ao refugio.");
        }

        void DrawHeroDefeatPanel(float width,float height)
        {
            EnsureHudArt();
            FillUi(new Rect(0f,0f,width,height),
                new Color(.025f,.016f,.046f,.91f));
            float panelWidth=Mathf.Min(width-24f,372f);
            float panelHeight=Mathf.Min(height-20f,276f);
            Rect panel=new Rect((width-panelWidth)*.5f,
                (height-panelHeight)*.5f,panelWidth,panelHeight);
            DrawWardrobeSurface(panel,new Color(.105f,.075f,.15f,.98f),
                new Color(.70f,.50f,.70f,.96f));
            float contentWidth=panel.width-24f;
            GUI.Label(new Rect(panel.x+12f,panel.y+15f,
                contentWidth,30f),"A NECROMANTE CAIU",hudTitle);
            GUI.Label(new Rect(panel.x+12f,panel.y+53f,
                contentWidth,44f),
                "Sua alma permanece ligada ao refugio.",hudSmall);
            GUI.Label(new Rect(panel.x+12f,panel.y+97f,
                contentWidth,42f),
                "Ao retornar: 65% de vida, anima restaurada.",hudSmall);
            int loss=Mathf.Min(2,Mathf.Max(0,shards));
            GUI.Label(new Rect(panel.x+12f,panel.y+141f,
                contentWidth,36f),
                "Perda: "+loss+" fragmento"+(loss==1?"":"s")+
                ". Seus servos e equipamentos permanecem.",hudSmall);
            Rect action=new Rect(panel.x+14f,panel.y+panel.height-65f,
                panel.width-28f,46f);
            DrawWardrobeSurface(action,new Color(.25f,.16f,.30f,.98f),
                new Color(.84f,.71f,.88f,.96f));
            GUI.Label(action,"RETORNAR AO REFUGIO",hudAction);
            bool clicked=GUI.Button(action,GUIContent.none,GUIStyle.none);
            // Keyboard works even though Update() is suspended at gameOver.
            Event evt=Event.current;
            bool keyboard=evt.type==EventType.KeyDown &&
                (evt.keyCode==KeyCode.Return ||
                 evt.keyCode==KeyCode.KeypadEnter ||
                 evt.keyCode==KeyCode.Space);
            if(clicked || keyboard)
            {
                if(keyboard)evt.Use();
                RecoverAfterDeath();
            }
        }
    }
}

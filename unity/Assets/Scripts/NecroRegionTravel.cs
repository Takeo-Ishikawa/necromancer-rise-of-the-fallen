using UnityEngine;

namespace Necromancer
{
    public static class NecroRegionRules
    {
        public static bool CanClaim(bool claimed,bool guardianRequired,
            bool guardianDefeated,bool nearSeal)
        {
            return !claimed && (guardianRequired?guardianDefeated:nearSeal);
        }

        public static bool NeedsGuardian(bool required,bool claimed,bool defeated)
        {
            return required && !claimed && !defeated;
        }

        public static bool CanAdvance(bool claimed,bool nearSeal,bool hasNextRegion)
        {
            return claimed && nearSeal && hasNextRegion;
        }
    }

    // Keeps region navigation in the active GUI, not the legacy HUD.
    public partial class NecroPrototype
    {
        void ActivateRegionSeal()
        {
            if(current==null || current.seal==null)return;
            SpriteRenderer renderer=current.seal.GetComponent<SpriteRenderer>();
            if(renderer!=null)renderer.color=new Color(.58f,1f,.77f,.91f);
        }

        void UpdateRegionProgress()
        {
            if(inSanctuary || gameOver || level==null || current==null)return;
            if(NecroRegionRules.CanClaim(current.claimed,
                level.requiresGuardian,current.bossDefeated,NearSeal()))
                Claim();
        }

        bool RegionTravelAvailable()
        {
            if(inSanctuary || gameOver || current==null || player==null ||
                levels==null)return false;
            return (stage>0 && NearEntrance()) ||
                NecroRegionRules.CanAdvance(current.claimed,NearSeal(),
                    stage+1<levels.Length);
        }

        Rect RegionTravelRect(float scale)
        {
            return new Rect(24f,192f,Screen.width/scale-48f,44f);
        }

        void DrawRegionNavigation(float scale)
        {
            if(inSanctuary || gameOver || current==null || player==null)return;
            EnsureStyles();
            Rect rect=RegionTravelRect(scale);
            if(stage>0 && NearEntrance())
            {
                if(GUI.Button(rect,"VOLTAR: "+levels[stage-1].title,
                    buttonStyle))Retreat();
                return;
            }
            if(levels.Length==1 && NecroCaveNetwork.HasNetwork(level))
            {
                GUI.Label(rect,NearSeal() ?
                    (current.claimed?"CORAÇÃO DA CAVERNA DESCOBERTO":
                        "O SELO AGUARDA") :
                    "EXPLORE OS TÚNEIS E BIOMAS • USE O MINIMAPA",
                    smallStyle);
                return;
            }
            if(NearSeal())
            {
                if(RegionTravelAvailable() && stage+1<levels.Length)
                {
                    if(GUI.Button(rect,"AVANCAR: "+levels[stage+1].title,
                        buttonStyle))Advance();
                }
                else GUI.Label(rect,
                    level.requiresGuardian && !current.claimed?
                    "DERROTE O GUARDIAO PARA ABRIR O SELO":
                    "ULTIMA REGIAO CONQUISTADA",smallStyle);
                return;
            }
            Vector2 destination=NecroLevels.At(level,level.sealX,level.sealY);
            Vector2 delta=destination-(Vector2)player.body.position;
            string direction=Mathf.Abs(delta.x)>=Mathf.Abs(delta.y)?
                (delta.x>=0f?"DIREITA":"ESQUERDA"):
                (delta.y>=0f?"ACIMA":"ABAIXO");
            string label=level.requiresGuardian && !current.claimed?
                "GUARDIAO / SELO":"SELO DE PASSAGEM";
            GUI.Label(rect,label+" · "+Mathf.CeilToInt(delta.magnitude)+
                "m  "+direction,smallStyle);
        }
    }
}

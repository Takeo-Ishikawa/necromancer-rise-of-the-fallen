using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Permanent meditation attributes. One point per character level earned by
    /// defeating hostile creatures. The optional v1 save fields remain compatible
    /// with previous Android installs. No new textures are generated or imported.
    /// </summary>
    public partial class NecroPrototype
    {
        const int MeditationAttributeCap=12;
        bool meditationOpen;
        int vitalityPoints,powerPoints,agilityPoints;

        int MeditationPointsAvailable()
        {
            // Derive the balance from level and invested points to avoid duplication.
            return Mathf.Max(0,playerLevel-1-
                vitalityPoints-powerPoints-agilityPoints);
        }

        void RestoreMeditationAllocations(NecroSaveData saved)
        {
            int budget=Mathf.Max(0,playerLevel-1);
            vitalityPoints=Mathf.Clamp(saved.vitalityPoints,0,
                Mathf.Min(MeditationAttributeCap,budget));
            budget-=vitalityPoints;
            powerPoints=Mathf.Clamp(saved.powerPoints,0,
                Mathf.Min(MeditationAttributeCap,budget));
            budget-=powerPoints;
            agilityPoints=Mathf.Clamp(saved.agilityPoints,0,
                Mathf.Min(MeditationAttributeCap,budget));
        }

        void AwardMeditationExperience(string kind)
        {
            kills++;
            if(playerLevel<100)
            {
                experience+=ExperienceValue(kind);
                bool gained=false;
                while(playerLevel<100 && experience>=ExperienceThreshold())
                {
                    experience-=ExperienceThreshold();
                    playerLevel++;
                    gained=true;
                }
                if(gained)
                    Announce("NIVEL "+playerLevel+
                        "! Novo ponto disponivel no altar.");
            }
            SaveProgress();
        }

        void OpenSanctuaryMeditation()
        {
            if(!inSanctuary || inventoryOpen || wardrobeOpen ||
                gameOver || player==null)return;
            meditationOpen=true;
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            Announce("Altar: distribua seus pontos de atributo.");
        }

        void CloseSanctuaryMeditation()
        {
            meditationOpen=false;
            touchMove=Vector2.zero;
            joystickFingerId=-1;
        }

        void InvestMeditationPoint(int index)
        {
            if(!inSanctuary || !meditationOpen || gameOver || player==null ||
                MeditationPointsAvailable()<=0)return;
            if(index==0)
            {
                if(vitalityPoints>=MeditationAttributeCap)return;
                vitalityPoints++;
                player.maxHp+=12f;
                player.hp=Mathf.Min(player.maxHp,player.hp+12f);
                Announce("Vitalidade aumentada: +12 vida maxima.");
            }
            else if(index==1)
            {
                if(powerPoints>=MeditationAttributeCap)return;
                powerPoints++;
                Announce("Poder aumentado: +2 golpe, +4 projetil.");
            }
            else if(index==2)
            {
                if(agilityPoints>=MeditationAttributeCap)return;
                agilityPoints++;
                Announce("Agilidade aumentada: +6% velocidade.");
            }
            else return;
            SaveProgress();
        }

        void DrawMeditationAttributeCard(Rect area,string name,int rank,
            string description,int index)
        {
            DrawWardrobeSurface(area,WardrobeCardBackground,WardrobeMutedEdge);
            float left=area.x+12f, availableWidth=area.width-24f;
            GUI.Label(new Rect(left,area.y+4f,availableWidth*.60f,22f),
                name,wardrobeSectionStyle);
            GUI.Label(new Rect(left+availableWidth*.60f,area.y+4f,
                availableWidth*.40f,22f),
                "GRAU "+rank+"/"+MeditationAttributeCap,wardrobeGradeStyle);
            GUI.Label(new Rect(left,area.y+24f,availableWidth,18f),
                description,wardrobeBodyStyle);
            bool canInvest=rank<MeditationAttributeCap &&
                MeditationPointsAvailable()>0;
            string caption=rank>=MeditationAttributeCap ?
                "GRAU MAXIMO" : "INVESTIR 1 PONTO";
            if(WardrobeActionButton(new Rect(area.x+11f,area.y+area.height-32f,
                area.width-22f,29f),caption,canInvest))
                InvestMeditationPoint(index);
        }

        void DrawSanctuaryMeditationPanel(float width,float height)
        {
            EnsureWardrobeStyles();
            float w=Mathf.Min(width-24f,390f);
            float h=Mathf.Min(height-24f,456f);
            float x=(width-w)*.5f,y=(height-h)*.5f;
            FillUi(new Rect(0f,0f,width,height),WardrobeShade);
            FillUi(new Rect(x+5f,y+7f,w,h),
                new Color(.012f,.009f,.025f,.62f));
            DrawWardrobeSurface(new Rect(x,y,w,h),
                WardrobeBackground,WardrobeEdge);
            FillUi(new Rect(x+2f,y+2f,w-4f,52f),
                new Color(.56f,.38f,.62f,.12f));
            GUI.Label(new Rect(x+14f,y+8f,w-28f,32f),
                "ALTAR DE MEDITACAO",wardrobeTitleStyle);
            GUI.Label(new Rect(x+12f,y+45f,w-24f,25f),
                "NIVEL "+playerLevel+"  |  PONTOS LIVRES: "+
                MeditationPointsAvailable(),wardrobeResourceStyle);
            FillUi(new Rect(x+17f,y+75f,w-34f,1f),
                new Color(.70f,.57f,.73f,.68f));

            float cardHeight=(h-183f)/3f;
            float cardTop=y+85f;
            DrawMeditationAttributeCard(
                new Rect(x+15f,cardTop,w-30f,cardHeight),
                "VITALIDADE",vitalityPoints,
                "+12 vida maxima por ponto",0);
            DrawMeditationAttributeCard(
                new Rect(x+15f,cardTop+cardHeight+8f,w-30f,cardHeight),
                "PODER",powerPoints,
                "+2 golpe e +4 projetil por ponto",1);
            DrawMeditationAttributeCard(
                new Rect(x+15f,cardTop+(cardHeight+8f)*2f,
                    w-30f,cardHeight),
                "AGILIDADE",agilityPoints,
                "+6% movimento por ponto",2);

            GUI.Label(new Rect(x+14f,y+h-75f,w-28f,20f),
                "Ganhe pontos subindo de nivel na caverna.",
                wardrobeNoteStyle);
            FillUi(new Rect(x+17f,y+h-51f,w-34f,1f),
                new Color(.70f,.57f,.73f,.49f));
            if(WardrobeActionButton(
                new Rect(x+23f,y+h-46f,w-46f,37f),
                "FECHAR",true,true))
                CloseSanctuaryMeditation();
        }
    }
}

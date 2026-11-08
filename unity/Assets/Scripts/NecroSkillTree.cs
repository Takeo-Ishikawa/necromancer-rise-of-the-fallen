using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.1: first practical skill tree. The balance is derived from level,
    /// and only one bit per node is persisted in the existing v1 JSON save.
    /// Each branch has a prerequisite and a real runtime effect.
    /// </summary>
    public partial class NecroPrototype
    {
        const int SkillTreeNodeCount=6;
        int learnedSkillMask;
        bool meditationSkillsTab;
        Vector2 meditationAttributeScroll,meditationSkillScroll;

        static readonly int[] SkillPrerequisite={-1,0,-1,2,-1,4};
        static readonly string[] SkillNames={
            "LACO SOMBRIO","SERVO VORAZ",
            "FLUXO DE ANIMA","CEIFA DE ALMAS",
            "ECO ESPECTRAL","RITMO PROFANO"
        };
        static readonly string[] SkillDescriptions={
            "-3 anima ao ressuscitar","Servos causam +15% dano",
            "+1.5 anima por segundo","+4 anima por inimigo abatido",
            "Projetil causa +12% dano","Projetil: -0.12s de recarga"
        };
        static readonly string[] SkillBranches={
            "NECROMANCIA","ESSENCIA","MAGIA ESPECTRAL"
        };

        bool SkillUnlocked(int index)
        {
            return index>=0 && index<SkillTreeNodeCount &&
                (learnedSkillMask&(1<<index))!=0;
        }

        int LearnedSkillCount()
        {
            int count=0;
            for(int i=0;i<SkillTreeNodeCount;i++)
                if(SkillUnlocked(i))count++;
            return count;
        }

        int AvailableSkillPoints()
        {
            // One independent skill point every TWO levels (3,5,7,9,11,13).
            // Attribute points remain independent of the skill budget.
            return Mathf.Max(0,(playerLevel-1)/2-LearnedSkillCount());
        }

        void RestoreSkillTree(NecroSaveData saved)
        {
            int mask=saved.learnedSkills&((1<<SkillTreeNodeCount)-1);
            // Old saves contain 0; invalid dependency chains are discarded.
            for(int i=1;i<SkillTreeNodeCount;i+=2)
                if((mask&(1<<i))!=0 &&
                   (mask&(1<<SkillPrerequisite[i]))==0)
                    mask&=~(1<<i);
            int budget=Mathf.Max(0,(playerLevel-1)/2);
            // Remove deepest upgrades first when saved data exceeds level budget.
            for(int i=SkillTreeNodeCount-1;i>=0;i--)
            {
                int count=0;
                for(int k=0;k<SkillTreeNodeCount;k++)
                    if((mask&(1<<k))!=0)count++;
                if(count<=budget)break;
                mask&=~(1<<i);
            }
            for(int i=1;i<SkillTreeNodeCount;i+=2)
                if((mask&(1<<i))!=0 &&
                   (mask&(1<<SkillPrerequisite[i]))==0)
                    mask&=~(1<<i);
            learnedSkillMask=mask;
        }

        int SkillRaiseManaCost(string kind)
        {
            return Mathf.Max(1,ManaCost(kind)-(SkillUnlocked(0)?3:0));
        }

        float SkillServantDamage(float baseDamage)
        {
            return baseDamage*(SkillUnlocked(1)?1.15f:1f);
        }

        float SkillSpectralDamage(float baseDamage)
        {
            return baseDamage*(SkillUnlocked(4)?1.12f:1f);
        }

        void UnlockSkillTreeNode(int index)
        {
            if(!inSanctuary || !meditationOpen || !meditationSkillsTab ||
                gameOver || player==null || index<0 || index>=SkillTreeNodeCount ||
                AvailableSkillPoints()<=0 || SkillUnlocked(index))return;
            int prerequisite=SkillPrerequisite[index];
            if(prerequisite>=0 && !SkillUnlocked(prerequisite))return;
            learnedSkillMask|=1<<index;
            SaveProgress();
            Announce("Habilidade aprendida: "+SkillNames[index]+".");
        }

        void DrawSkillTreeNode(Rect area,int id)
        {
            bool unlocked=SkillUnlocked(id);
            int predecessor=SkillPrerequisite[id];
            bool prerequisiteMet=predecessor<0 || SkillUnlocked(predecessor);
            bool available=!unlocked && prerequisiteMet && AvailableSkillPoints()>0;
            DrawWardrobeSurface(area,WardrobeCardBackground,
                unlocked?WardrobeEdge:WardrobeMutedEdge);
            float pad=8f;
            GUI.Label(new Rect(area.x+pad,area.y+4f,area.width-pad*2f,24f),
                SkillNames[id],wardrobeSectionStyle);
            GUI.Label(new Rect(area.x+pad,area.y+29f,area.width-pad*2f,35f),
                SkillDescriptions[id],wardrobeBodyStyle);
            string action=unlocked?"APRENDIDA":
                !prerequisiteMet?"REQUER ANTERIOR":
                AvailableSkillPoints()<=0?"SEM PONTOS":"APRENDER";
            if(WardrobeActionButton(new Rect(area.x+pad,area.y+area.height-38f,
                area.width-pad*2f,33f),action,available))
                UnlockSkillTreeNode(id);
        }

        void DrawSkillTreeContent(Rect view)
        {
            const float rowHeight=151f;
            const float totalHeight=463f;
            float innerWidth=Mathf.Max(220f,view.width-20f);
            meditationSkillScroll=GUI.BeginScrollView(view,meditationSkillScroll,
                new Rect(0f,0f,innerWidth,totalHeight),false,true);
            float gap=23f;
            float cardWidth=(innerWidth-gap-12f)/2f;
            for(int branch=0;branch<3;branch++)
            {
                float top=branch*rowHeight;
                GUI.Label(new Rect(7f,top,innerWidth-14f,22f),
                    SkillBranches[branch],wardrobeResourceStyle);
                float left=6f,right=left+cardWidth+gap;
                // The connector remains visible between node I and II.
                FillUi(new Rect(left+cardWidth,top+75f,gap,2f),
                    SkillUnlocked(branch*2) ? WardrobeEdge:WardrobeMutedEdge);
                DrawSkillTreeNode(new Rect(left,top+25f,cardWidth,116f),branch*2);
                DrawSkillTreeNode(new Rect(right,top+25f,cardWidth,116f),branch*2+1);
            }
            GUI.EndScrollView();
        }

        void DrawMeditationAttributesContent(Rect view)
        {
            const float cardHeight=111f;
            float innerWidth=Mathf.Max(220f,view.width-20f);
            meditationAttributeScroll=GUI.BeginScrollView(view,
                meditationAttributeScroll,
                new Rect(0f,0f,innerWidth,cardHeight*3f+20f),false,true);
            DrawMeditationAttributeCard(
                new Rect(6f,0f,innerWidth-12f,cardHeight),
                "VITALIDADE",vitalityPoints,
                "+12 vida maxima por ponto",0);
            DrawMeditationAttributeCard(
                new Rect(6f,cardHeight+10f,innerWidth-12f,cardHeight),
                "PODER",powerPoints,
                "+2 golpe e +4 projetil por ponto",1);
            DrawMeditationAttributeCard(
                new Rect(6f,(cardHeight+10f)*2f,innerWidth-12f,cardHeight),
                "AGILIDADE",agilityPoints,
                "+6% movimento por ponto",2);
            GUI.EndScrollView();
        }
    }
}

using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.8.1: functional equipment station, using current staffTier and
    /// mantleTier (the SAME data stored in the version-1 save format).
    /// Future gear slots can extend this panel; no new PNG assets needed.
    /// </summary>
    public partial class NecroPrototype
    {
        bool wardrobeOpen;

        int FocusUpgradeCost() { return 8 + 6 * staffTier; }
        int MantleUpgradeCost() { return 10 + 6 * mantleTier; }

        void OpenSanctuaryWardrobe()
        {
            if(!inSanctuary || inventoryOpen || gameOver)return;
            wardrobeOpen = true;
            touchMove = Vector2.zero;
            joystickFingerId = -1;
            Announce("Guarda-roupa: equipamento e aprimoramentos.");
        }

        void CloseSanctuaryWardrobe()
        {
            wardrobeOpen = false;
            touchMove = Vector2.zero;
            joystickFingerId = -1;
        }

        void UpgradeSanctuaryEquipment(bool focus)
        {
            if(!inSanctuary || !wardrobeOpen || gameOver || player==null)return;
            int tier = focus ? staffTier : mantleTier;
            if(tier>=3) { Announce("Equipamento no grau maximo."); return; }
            int cost = focus ? FocusUpgradeCost() : MantleUpgradeCost();
            if(shards<cost) { Announce("Fragmentos insuficientes."); return; }
            shards -= cost;
            if(focus)
            {
                staffTier++;
                Announce("Foco arcano aprimorado para grau "+staffTier+".");
            }
            else
            {
                mantleTier++;
                player.maxHp += 18f;
                player.hp = Mathf.Min(player.maxHp,player.hp + 18f);
                Announce("Manto aprimorado para grau "+mantleTier+".");
            }
            SaveProgress();
        }

        void DrawSanctuaryWardrobePanel(float width,float height)
        {
            EnsureStyles();
            float w = Mathf.Min(width-24f,390f);
            float h = Mathf.Min(height-30f,393f);
            float x=(width-w)*.5f, y=(height-h)*.5f;
            Rect panel=new Rect(x,y,w,h);
            GUI.Box(panel,GUIContent.none);

            GUI.Label(new Rect(x+16f,y+13f,w-32f,30f),
                "GUARDA-ROUPA",titleStyle);
            GUI.Label(new Rect(x+14f,y+47f,w-28f,25f),
                "FRAGMENTOS DISPONIVEIS: "+shards,smallStyle);

            GUI.Label(new Rect(x+18f,y+83f,w-36f,25f),
                "FOCO ARCANO - GRAU "+staffTier+"/3",labelStyle);
            GUI.Label(new Rect(x+18f,y+111f,w-36f,24f),
                "Bonus: +"+(staffTier*5)+" corpo a corpo | +"+(staffTier*8)+
                " projetil",smallStyle);
            int focusCost = FocusUpgradeCost();
            GUI.enabled = staffTier<3 && shards>=focusCost;
            if(GUI.Button(new Rect(x+18f,y+141f,w-36f,40f),
                staffTier>=3 ? "FOCO NO MAXIMO" :
                "APRIMORAR FOCO - "+focusCost+" FRAGMENTOS",buttonStyle))
                UpgradeSanctuaryEquipment(true);
            GUI.enabled = true;

            GUI.Label(new Rect(x+18f,y+193f,w-36f,25f),
                "MANTO - GRAU "+mantleTier+"/3",labelStyle);
            GUI.Label(new Rect(x+18f,y+221f,w-36f,24f),
                "Bonus: +"+(mantleTier*18)+" VIDA MAXIMA",smallStyle);
            int mantleCost = MantleUpgradeCost();
            GUI.enabled = mantleTier<3 && shards>=mantleCost;
            if(GUI.Button(new Rect(x+18f,y+250f,w-36f,40f),
                mantleTier>=3 ? "MANTO NO MAXIMO" :
                "APRIMORAR MANTO - "+mantleCost+" FRAGMENTOS",buttonStyle))
                UpgradeSanctuaryEquipment(false);
            GUI.enabled = true;

            GUI.Label(new Rect(x+16f,y+302f,w-32f,29f),
                "Novas pecas: obtidas como recompensas na caverna.",smallStyle);
            if(GUI.Button(new Rect(x+25f,y+h-51f,w-50f,39f),
                "FECHAR",buttonStyle))
                CloseSanctuaryWardrobe();
        }
    }
}

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

        // All interface layers reuse the built-in 1x1 texture via FillUi.
        // No additional sprite sheets, imported images or runtime textures.
        static readonly Color WardrobeShade =
            new Color(.025f,.018f,.045f,.52f);
        static readonly Color WardrobeBackground =
            new Color(.095f,.069f,.134f,.97f);
        static readonly Color WardrobeCardBackground =
            new Color(.071f,.052f,.102f,.975f);
        static readonly Color WardrobeEdge =
            new Color(.72f,.60f,.78f,.96f);
        static readonly Color WardrobeMutedEdge =
            new Color(.47f,.38f,.55f,.90f);

        GUIStyle wardrobeTitleStyle,wardrobeResourceStyle,wardrobeSectionStyle;
        GUIStyle wardrobeGradeStyle,wardrobeBodyStyle,wardrobeNoteStyle;
        GUIStyle wardrobeActiveButtonText,wardrobeInactiveButtonText;

        void EnsureWardrobeStyles()
        {
            if(wardrobeTitleStyle != null)return;
            // GUIStyle objects are created only once and reused every OnGUI.
            wardrobeTitleStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 19,
                fontStyle = FontStyle.Bold
            };
            wardrobeTitleStyle.normal.textColor =
                new Color(.98f,.94f,.87f,1f);

            wardrobeResourceStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            wardrobeResourceStyle.normal.textColor =
                new Color(.92f,.81f,.62f,1f);

            wardrobeSectionStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleLeft, fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            wardrobeSectionStyle.normal.textColor =
                new Color(.98f,.93f,.92f,1f);

            wardrobeGradeStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleRight, fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            wardrobeGradeStyle.normal.textColor =
                new Color(.87f,.76f,.98f,1f);

            wardrobeBodyStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleLeft, fontSize = 12,
                wordWrap = true
            };
            wardrobeBodyStyle.normal.textColor =
                new Color(.91f,.85f,.94f,1f);

            wardrobeNoteStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 11,
                wordWrap = true
            };
            wardrobeNoteStyle.normal.textColor =
                new Color(.79f,.76f,.83f,1f);

            wardrobeActiveButtonText = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 13,
                fontStyle = FontStyle.Bold, wordWrap = true
            };
            wardrobeActiveButtonText.normal.textColor =
                new Color(.99f,.96f,.91f,1f);

            wardrobeInactiveButtonText = new GUIStyle(wardrobeActiveButtonText);
            wardrobeInactiveButtonText.normal.textColor =
                new Color(.77f,.72f,.81f,1f);
        }

        void DrawWardrobeSurface(Rect r,Color background,Color edge)
        {
            FillUi(r,edge);
            FillUi(new Rect(r.x+1.5f,r.y+1.5f,
                Mathf.Max(0f,r.width-3f),Mathf.Max(0f,r.height-3f)),
                background);
        }

        bool WardrobeActionButton(Rect bounds,string caption,bool available,
            bool closeButton=false)
        {
            Color fill = available
                ? closeButton ? new Color(.31f,.23f,.37f,.98f) :
                    new Color(.285f,.195f,.39f,.97f)
                : new Color(.16f,.135f,.19f,.95f);
            Color edge = available
                ? new Color(.85f,.71f,.86f,.98f)
                : new Color(.46f,.40f,.50f,.89f);
            FillUi(new Rect(bounds.x+1f,bounds.y+3f,
                bounds.width,bounds.height),new Color(.015f,.012f,.03f,.48f));
            DrawWardrobeSurface(bounds,fill,edge);
            FillUi(new Rect(bounds.x+2f,bounds.y+2f,
                bounds.width-4f,2f),
                available ? new Color(.98f,.87f,.96f,.24f) :
                    new Color(.70f,.66f,.73f,.12f));
            GUI.Label(bounds,caption,available ?
                wardrobeActiveButtonText : wardrobeInactiveButtonText);
            // A transparent input surface keeps native touch/click handling.
            return available && GUI.Button(bounds,GUIContent.none,GUIStyle.none);
        }

        void DrawWardrobeGearCard(Rect area,string equipment,int tier,
            string bonus,int cost,bool focus)
        {
            DrawWardrobeSurface(area,WardrobeCardBackground,WardrobeMutedEdge);
            FillUi(new Rect(area.x+2f,area.y+2f,
                area.width-4f,2f),new Color(.78f,.63f,.87f,.19f));
            float innerX=area.x+14f,innerWidth=area.width-28f;

            GUI.Label(new Rect(innerX,area.y+10f,innerWidth*.62f,24f),
                equipment,wardrobeSectionStyle);
            GUI.Label(new Rect(innerX+innerWidth*.59f,area.y+10f,
                innerWidth*.41f,24f),"GRAU "+tier+"/3",wardrobeGradeStyle);
            GUI.Label(new Rect(innerX,area.y+38f,innerWidth,22f),
                bonus,wardrobeBodyStyle);
            bool maxed=tier>=3;
            bool available=!maxed && shards>=cost;
            string buttonLabel=maxed ? "GRAU MAXIMO" :
                (focus ? "APRIMORAR FOCO - " : "APRIMORAR MANTO - ") +
                cost+" FRAGMENTOS";
            if(WardrobeActionButton(new Rect(area.x+12f,area.y+72f,
                area.width-24f,38f),buttonLabel,available))
                UpgradeSanctuaryEquipment(focus);
        }

        void DrawSanctuaryWardrobePanel(float width,float height)
        {
            EnsureWardrobeStyles();
            // Virtual reference width is 420; typical Android portrait viewport
            // has enough height for this bounded and centered 444px panel.
            float w=Mathf.Min(width-24f,390f);
            float h=Mathf.Min(height-24f,444f);
            float x=(width-w)*.5f,y=(height-h)*.5f;
            Rect panel=new Rect(x,y,w,h);

            // Darken the world before drawing a nearly opaque stone/plum panel.
            FillUi(new Rect(0f,0f,width,height),WardrobeShade);
            FillUi(new Rect(x+5f,y+7f,w,h),
                new Color(.012f,.009f,.025f,.62f));
            DrawWardrobeSurface(panel,WardrobeBackground,WardrobeEdge);
            FillUi(new Rect(x+2f,y+2f,w-4f,51f),
                new Color(.56f,.38f,.62f,.12f));
            FillUi(new Rect(x+17f,y+79f,w-34f,1f),
                new Color(.70f,.57f,.73f,.68f));

            GUI.Label(new Rect(x+18f,y+12f,w-36f,32f),
                "GUARDA-ROUPA",wardrobeTitleStyle);
            GUI.Label(new Rect(x+18f,y+48f,w-36f,25f),
                "FRAGMENTOS DISPONIVEIS: "+shards,wardrobeResourceStyle);

            DrawWardrobeGearCard(new Rect(x+15f,y+91f,w-30f,122f),
                "FOCO ARCANO",staffTier,
                "Bonus: +"+(staffTier*5)+" corpo a corpo | +"+
                (staffTier*8)+" projetil",FocusUpgradeCost(),true);
            DrawWardrobeGearCard(new Rect(x+15f,y+226f,w-30f,122f),
                "MANTO",mantleTier,
                "Bonus: +"+(mantleTier*18)+" vida | -"+
                Mathf.RoundToInt(MantleDamageReduction()*100f)+
                "% dano recebido",
                MantleUpgradeCost(),false);

            GUI.Label(new Rect(x+16f,y+357f,w-32f,25f),
                "Novas pecas podem ser encontradas na caverna.",
                wardrobeNoteStyle);
            FillUi(new Rect(x+17f,y+h-59f,w-34f,1f),
                new Color(.70f,.57f,.73f,.49f));
            if(WardrobeActionButton(new Rect(x+24f,y+h-48f,
                w-48f,38f),"FECHAR",true,true))
                CloseSanctuaryWardrobe();
        }
    }
}

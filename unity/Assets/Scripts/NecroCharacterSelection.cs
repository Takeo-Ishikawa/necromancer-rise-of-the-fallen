using UnityEngine;

namespace Necromancer
{
    public partial class NecroPrototype
    {
        const string SelectedCharacterKey="necro.selected-character.v1";
        bool characterSelectionOpen=true, characterStartRequested, characterHasSave;
        bool inGameCharacterMenu;
        int activeCharacterIndex;
        int selectedCharacterIndex;
        NecroCharacterDefinition[] characterDefinitions;
        Texture2D characterPreview, characterPortrait;
        string characterSelectionError;
        Vector2 characterListScroll;
        GUIStyle characterLogo,characterName,characterCopy,characterTag;
        NecroCharacterDefinition SelectedCharacter {
            get { return characterDefinitions[selectedCharacterIndex]; }
        }

        void InitializeCharacterSelection()
        {
            Application.targetFrameRate=60;
            characterDefinitions=NecroCharacterCatalog.Load();
            string remembered=PlayerPrefs.GetString(SelectedCharacterKey,"necromante");
            selectedCharacterIndex=System.Array.FindIndex(characterDefinitions,
                c=>c.available && c.id==remembered);
            if(selectedCharacterIndex<0)
                selectedCharacterIndex=System.Array.FindIndex(characterDefinitions,c=>c.available);
            NecroSaveData saved;
            characterHasSave=NecroSaveStore.TryLoad(out saved);
            activeCharacterIndex=selectedCharacterIndex;
            RefreshCharacterPreview();
        }

        void RefreshCharacterPreview()
        {
            characterPreview=string.IsNullOrEmpty(SelectedCharacter.walkSixResource)
                ? Resources.Load<Texture2D>(SelectedCharacter.idleResource)
                : Resources.Load<Texture2D>(SelectedCharacter.walkSixResource);
            characterPortrait=string.IsNullOrEmpty(SelectedCharacter.portraitResource)
                ? null:Resources.Load<Texture2D>(SelectedCharacter.portraitResource);
            characterSelectionError=null;
        }

        // Open the existing selection screen without reloading the world.
        // The character in play is unchanged until APPLY is pressed.
        void OpenInGameCharacterMenu()
        {
            if(characterSelectionOpen || player==null || current==null ||
               gameOver || inventoryOpen || wardrobeOpen || meditationOpen)
                return;
            activeCharacterIndex=selectedCharacterIndex;
            inGameCharacterMenu=true;
            characterSelectionOpen=true;
            characterStartRequested=false;
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            manualQueued=false;
            characterListScroll.x=Mathf.Max(0f,
                selectedCharacterIndex*124f-124f);
            RefreshCharacterPreview();
        }

        void CancelInGameCharacterMenu()
        {
            if(!inGameCharacterMenu)return;
            selectedCharacterIndex=activeCharacterIndex;
            characterSelectionOpen=false;
            inGameCharacterMenu=false;
            characterStartRequested=false;
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            RefreshCharacterPreview();
        }

        void UpdateCharacterSelection()
        {
            // Startup runs once, outside OnGUI's layout/repaint event passes.
            if(characterDefinitions==null)return;
            if(inGameCharacterMenu && Input.GetKeyDown(KeyCode.Escape))
            {
                CancelInGameCharacterMenu();
                return;
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow))CycleCharacter(-1);
            if(Input.GetKeyDown(KeyCode.RightArrow))CycleCharacter(1);
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                characterStartRequested=true;
            if(!characterStartRequested)return;
            characterStartRequested=false;
            var selected=SelectedCharacter;
            if(!selected.available)return;
            if(!string.IsNullOrEmpty(selected.walkSixResource))
            {
                // The fourth character has a unique atlas contract, while
                // the existing three still follow their original dimensions.
                // Android may resample non-power-of-two textures. The importer
                // now preserves NPOT dimensions, but never reject usable art
                // merely because an imported texture was resized.
                if(characterPreview==null || characterPreview.width<384 ||
                   characterPreview.height<512)
                {
                    characterSelectionError=
                        "Sprites da Necromante Sombria ausentes ou incompletos.";
                    return;
                }
            }
            else
            {
                bool physicalAtlas=!string.IsNullOrEmpty(selected.combatResource);
                var spectral=Resources.Load<Texture2D>(
                    physicalAtlas?selected.combatResource:selected.spectralResource);
                if(characterPreview==null || characterPreview.width!=1024 ||
                   characterPreview.height!=512 || spectral==null ||
                   spectral.width!=1024 || spectral.height!=2048)
                {
                    characterSelectionError="Arte indisponível. Selecione outro personagem.";
                    return;
                }
                if(!string.IsNullOrEmpty(selected.walkResource))
                {
                    var walking=Resources.Load<Texture2D>(selected.walkResource);
                    if(walking==null || walking.width!=1024 || walking.height!=2048)
                    {
                        characterSelectionError="Animação de caminhada indisponível.";
                        return;
                    }
                }
            }
            touchMove=Vector2.zero;
            joystickFingerId=-1;
            manualQueued=false;
            if(inGameCharacterMenu)
            {
                if(selectedCharacterIndex!=activeCharacterIndex)
                {
                    if(!SaveProgress())
                    {
                        characterSelectionError=
                            "Não foi possível salvar. Tente novamente.";
                        return;
                    }
                    // Keep the position, equipment, companions, resources,
                    // cleared regions and v1 saves. Replace only the avatar
                    // renderer and its character-specific capabilities.
                    ResetTravelCombat();
                    ResetHeroAnimationForCharacterSwap();
                    InitializeHeroAnimation();
                    SetSanctuaryHeroAppearance(inSanctuary);
                    Announce("Personagem alterada: "+selected.name+".");
                }
                inGameCharacterMenu=false;
            }
            else
            {
                BeginSelectedCharacterWorld();
                if(player==null || current==null)return;
            }
            activeCharacterIndex=selectedCharacterIndex;
            PlayerPrefs.SetString(SelectedCharacterKey,selected.id);
            PlayerPrefs.Save();
            characterSelectionOpen=false;
        }

        void CycleCharacter(int direction)
        {
            for(int step=1;step<=characterDefinitions.Length;step++)
            {
                int index=(selectedCharacterIndex+direction*step+characterDefinitions.Length)%characterDefinitions.Length;
                if(!characterDefinitions[index].available)continue;
                selectedCharacterIndex=index;
                RefreshCharacterPreview();
                characterListScroll.x=Mathf.Max(0f,index*124f-124f);
                return;
            }
        }

        void EnsureCharacterStyles()
        {
            if(characterLogo!=null)return;
            EnsureWardrobeStyles();
            characterLogo=new GUIStyle(wardrobeTitleStyle){fontSize=29};
            characterName=new GUIStyle(wardrobeTitleStyle){fontSize=25};
            characterCopy=new GUIStyle(wardrobeNoteStyle){fontSize=13,wordWrap=true};
            characterTag=new GUIStyle(wardrobeResourceStyle){fontSize=11};
        }

        void DrawCharacterSelection()
        {
            if(characterDefinitions==null)return;
            EnsureCharacterStyles();
            Matrix4x4 previousMatrix=GUI.matrix;
            Color previousColor=GUI.color;
            GUI.matrix=Matrix4x4.identity;
            GUI.color=Color.white;
            FillUi(new Rect(0,0,Screen.width,Screen.height),new Color(.025f,.021f,.045f));
            Rect safe=Screen.safeArea;
            float scale=Mathf.Max(.01f,Mathf.Min(safe.width/420f,safe.height/720f));
            float x=safe.x+(safe.width-420f*scale)*.5f;
            float y=Screen.height-safe.yMax+(safe.height-720f*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.identity,new Vector3(scale,scale,1));
            try
            {
                DrawWardrobeSurface(new Rect(8,8,404,704),new Color(.065f,.047f,.095f),WardrobeMutedEdge);
                FillUi(new Rect(28,106,364,1),WardrobeMutedEdge);
                GUI.Label(new Rect(20,24,380,42),"NECROMANCER",characterLogo);
                GUI.Label(new Rect(20,65,380,22),"R I S E   O F   T H E   F A L L E N",characterTag);
                GUI.Label(new Rect(20,116,380,24),
                    inGameCharacterMenu?"TROCAR PERSONAGEM":"ESCOLHA SEU PERSONAGEM",
                    characterTag);

                // Use only the front-facing cell, never draw the whole spritesheet.
                DrawWardrobeSurface(new Rect(84,151,252,244),WardrobeCardBackground,
                    new Color(.32f,.47f,.46f));
                FillUi(new Rect(115,374,190,2),new Color(.41f,.76f,.68f));
                if(characterPortrait!=null)
                {
                    // Crop the approved vertical artwork proportionally.
                    // ScaleToFit made the woman a thin, tiny strip with black
                    // sides in the nearly square character-selection frame.
                    GUI.DrawTextureWithTexCoords(new Rect(94,152,232,236),
                        characterPortrait,new Rect(.15f,.35f,.70f,.54f),true);
                }
                else if(characterPreview!=null)
                {
                    float breath=Mathf.Sin(Time.unscaledTime*1.6f)*2f;
                    GUI.DrawTextureWithTexCoords(new Rect(90,146+breath,240,240),
                        characterPreview,new Rect(0,.5f,.25f,.5f),true);
                }
                GUI.Label(new Rect(25,400,370,36),SelectedCharacter.name,characterName);
                GUI.Label(new Rect(25,438,370,23),SelectedCharacter.subtitle,characterTag);
                GUI.Label(new Rect(35,465,350,53),SelectedCharacter.description,characterCopy);

                characterListScroll=GUI.BeginScrollView(new Rect(26,530,368,86),characterListScroll,
                    new Rect(0,0,Mathf.Max(368,characterDefinitions.Length*124f-8),62));
                for(int i=0;i<characterDefinitions.Length;i++)
                {
                    var entry=characterDefinitions[i];
                    Rect card=new Rect(i*124f,0,116,62);
                    bool selected=i==selectedCharacterIndex;
                    DrawWardrobeSurface(card,selected?new Color(.17f,.27f,.26f):WardrobeCardBackground,
                        selected?new Color(.54f,.86f,.73f):WardrobeMutedEdge);
                    GUI.Label(new Rect(card.x+4,5,108,27),entry.name,wardrobeActiveButtonText);
                    GUI.Label(new Rect(card.x+4,33,108,20),entry.available?
                        (inGameCharacterMenu && i==activeCharacterIndex?
                            "EM USO":selected?"ESCOLHIDA":"DISPONÍVEL"):
                        "EM BREVE",wardrobeNoteStyle);
                    if(entry.available && GUI.Button(card,GUIContent.none,GUIStyle.none))
                    {
                        selectedCharacterIndex=i;
                        RefreshCharacterPreview();
                    }
                }
                GUI.EndScrollView();
                if(inGameCharacterMenu)
                {
                    if(WardrobeActionButton(new Rect(36,628,204,48),
                        "USAR PERSONAGEM",true))
                        characterStartRequested=true;
                    if(WardrobeActionButton(new Rect(252,628,132,48),
                        "CANCELAR",true))
                        CancelInGameCharacterMenu();
                }
                else if(WardrobeActionButton(new Rect(36,628,348,48),
                    characterHasSave?"CONTINUAR JORNADA":"INICIAR JORNADA",true))
                    characterStartRequested=true;
                GUI.Label(new Rect(26,680,368,27),characterSelectionError ??
                    (inGameCharacterMenu?"Troque sem reiniciar sua jornada.":
                        characterHasSave?"Seu progresso será mantido.":
                        "Uma nova jornada começa no refúgio."),
                    wardrobeNoteStyle);
            }
            finally { GUI.matrix=previousMatrix; GUI.color=previousColor; }
        }
    }
}

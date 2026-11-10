using System;
using UnityEngine;

namespace Necromancer
{
    [Serializable]
    public class NecroCharacterDefinition
    {
        public string id, name, subtitle, description, idleResource, spectralResource, walkResource, combatResource;
        public bool available;
        public bool meleeOnly;
        // Enables the character-specific strike in spectral cell 13.
        public bool previewMotion;
    }

    [Serializable]
    public class NecroCharacterCatalog
    {
        public NecroCharacterDefinition[] characters;

        public static NecroCharacterDefinition DefaultCharacter()
        {
            return new NecroCharacterDefinition {
                id="necromante", name="A Necromante", subtitle="Entre a vida e a morte",
                description="Lute de perto e erga os caídos para formar seu exército.",
                idleResource="Heroine/idle", spectralResource="Heroine/spectral", available=true
            };
        }

        public static NecroCharacterDefinition[] Load()
        {
            TextAsset asset=Resources.Load<TextAsset>("Data/characters");
            if(asset!=null)
            {
                try
                {
                    var catalog=JsonUtility.FromJson<NecroCharacterCatalog>(asset.text);
                    if(catalog!=null && catalog.characters!=null && catalog.characters.Length>0)
                    {
                        var ids=new System.Collections.Generic.HashSet<string>();
                        bool playable=false;
                        foreach(var character in catalog.characters)
                        {
                            if(character==null || string.IsNullOrEmpty(character.id) ||
                               !ids.Add(character.id))throw new ArgumentException("Invalid character ID");
                            playable|=character.available;
                        }
                        if(playable)return catalog.characters;
                    }
                }
                catch(Exception error) { Debug.LogWarning("Character catalog: "+error.Message); }
            }
            return new[]{DefaultCharacter()};
        }
    }
}

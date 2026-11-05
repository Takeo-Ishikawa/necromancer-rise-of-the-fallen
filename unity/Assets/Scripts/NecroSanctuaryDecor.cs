using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Necromancer
{
    /// <summary>
    /// Independent painted furniture for the safe chamber. Layout is stored in
    /// Resources/Data/sanctuary_props.json; combat stages and saves are untouched.
    /// Each prop gets its own GameObject and SpriteRenderer in the Safe Chamber.
    /// </summary>
    public partial class NecroPrototype
    {
        [Serializable]
        class SanctuaryPropLayout
        {
            public SanctuaryProp[] props;
        }

        [Serializable]
        class SanctuaryProp
        {
            public string id;
            public string asset;
            public string layer; // wall, floor or object
            public float x, y, width, height;
            // Optional axis-aligned footprint relative to the sprite's bottom anchor.
            public float blockOffsetX, blockOffsetY, blockWidth, blockHeight;
        }

        class SanctuaryPropInstance
        {
            public SanctuaryProp definition;
            public SpriteRenderer renderer;
        }

        readonly List<SanctuaryPropInstance> sanctuaryProps =
            new List<SanctuaryPropInstance>();

        static int SanctuaryDepth(float worldY)
        {
            // Higher positions (farther north) render behind lower positions.
            return Mathf.RoundToInt((24f-worldY)*10f);
        }

        void BuildSanctuaryDecorations(Transform parent)
        {
            TextAsset layoutText = Resources.Load<TextAsset>("Data/sanctuary_props");
            if(layoutText == null)
            {
                Debug.LogWarning("Safe chamber prop layout is missing.");
                return;
            }
            SanctuaryPropLayout layout =
                JsonUtility.FromJson<SanctuaryPropLayout>(layoutText.text);
            if(layout == null || layout.props == null)return;

            sanctuaryProps.Clear();
            foreach(SanctuaryProp prop in layout.props)
            {
                if(prop == null || string.IsNullOrEmpty(prop.id) ||
                   string.IsNullOrEmpty(prop.asset) ||
                   prop.width <= 0 || prop.height <= 0)continue;

                Texture2D texture =
                    Resources.Load<Texture2D>("SanctuaryProps/" + prop.asset);
                if(texture == null)
                {
                    // A missing image must not create an invisible obstacle.
                    Debug.LogWarning("Safe chamber sprite missing: " + prop.asset);
                    continue;
                }

                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                bool floor = prop.layer == "floor";
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, floor ? .5f : .05f),
                    100f, 0, SpriteMeshType.FullRect);
                sprite.name = "Safe chamber / " + prop.id;

                GameObject instance = new GameObject("Decoration / " + prop.id);
                instance.transform.SetParent(parent, false);
                instance.transform.position = new Vector3(prop.x, prop.y, 0);
                SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder =
                    floor ? -20 : prop.layer == "wall" ? -10 : SanctuaryDepth(prop.y);
                Fit(renderer, prop.width, prop.height);

                sanctuaryProps.Add(new SanctuaryPropInstance {
                    definition = prop, renderer = renderer
                });
            }
        }

        bool SanctuaryDecorationBlocked(Vector2 point, float radius)
        {
            foreach(SanctuaryPropInstance instance in sanctuaryProps)
            {
                SanctuaryProp prop = instance.definition;
                if(prop.blockWidth <= 0 || prop.blockHeight <= 0)continue;
                float cx = prop.x + prop.blockOffsetX;
                float cy = prop.y + prop.blockOffsetY;
                // Axis-aligned footprint; only furniture bases block movement.
                if(Mathf.Abs(point.x-cx) < prop.blockWidth*.5f+radius &&
                   Mathf.Abs(point.y-cy) < prop.blockHeight*.5f+radius)
                    return true;
            }
            return false;
        }

        static void SetSanctuaryActorDepth(Actor actor, bool enabled)
        {
            if(actor == null || actor.body == null)return;
            SortingGroup group = actor.body.GetComponent<SortingGroup>();
            if(!enabled)
            {
                if(group != null)group.enabled = false;
                return;
            }
            if(group == null)group = actor.body.gameObject.AddComponent<SortingGroup>();
            group.enabled = true;
            group.sortingOrder = SanctuaryDepth(actor.body.position.y) + 2;
        }

        void UpdateSanctuaryActorDepth()
        {
            if(!inSanctuary)return;
            SetSanctuaryActorDepth(player, true);
            foreach(Actor servant in servants)SetSanctuaryActorDepth(servant, true);
        }

        void RestoreCombatActorDepth()
        {
            SetSanctuaryActorDepth(player, false);
            foreach(Actor servant in servants)SetSanctuaryActorDepth(servant, false);
        }
    }
}

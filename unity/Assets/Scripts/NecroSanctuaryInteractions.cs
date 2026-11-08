using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancer
{
    // The data file defines where interactions happen; each action is handled
    // here, without additional textures or changes to player save format.
    public partial class NecroPrototype
    {
        [Serializable] class SanctuaryStationList
        {
            public SanctuaryStation[] stations;
        }

        [Serializable] class SanctuaryStation
        {
            public string id, kind, label;
            public float x, y, radius, durationSeconds, cooldownSeconds;
            public float damageMultiplier, speedMultiplier, effectX, effectY;
        }

        SanctuaryStation[] sanctuaryStations = new SanctuaryStation[0];
        readonly Dictionary<string,float> stationCooldowns =
            new Dictionary<string,float>();
        float fountainBuffUntil;
        float fountainDamageBonus = 1f;
        float fountainSpeedBonus = 1f;
        SpriteRenderer fountainGlow;
        Vector3 fountainGlowBaseScale;

        // Read-only modifiers. Base stats and persistent saves are untouched.
        float FountainDamageMultiplier =>
            Time.time < fountainBuffUntil ? fountainDamageBonus : 1f;
        float FountainSpeedMultiplier =>
            Time.time < fountainBuffUntil ? fountainSpeedBonus : 1f;

        void BuildSanctuaryInteractions(Transform parent)
        {
            TextAsset source = Resources.Load<TextAsset>("Data/sanctuary_interactions");
            if(source == null)
            {
                Debug.LogWarning("Missing sanctuary_interactions.json; room stays playable.");
                return;
            }

            SanctuaryStationList data =
                JsonUtility.FromJson<SanctuaryStationList>(source.text);
            sanctuaryStations = data != null && data.stations != null
                ? data.stations : new SanctuaryStation[0];
            foreach(SanctuaryStation station in sanctuaryStations)
            {
                if(station == null || station.kind != "fountain_buff")continue;
                GameObject halo = new GameObject("Fountain / blessing pulse");
                halo.transform.SetParent(parent, false);
                halo.transform.position =
                    new Vector3(station.effectX, station.effectY, 0f);
                fountainGlow = halo.AddComponent<SpriteRenderer>();
                fountainGlow.sprite = NecroArt.RitualCircle();
                fountainGlow.sortingOrder = SanctuaryDepth(station.y + .8f) + 3;
                Fit(fountainGlow, 1.9f, .72f);
                fountainGlowBaseScale = halo.transform.localScale;
                fountainGlow.enabled = false;
                break;
            }
        }

        SanctuaryStation NearbySanctuaryStation()
        {
            if(!inSanctuary || player == null || gameOver)return null;
            SanctuaryStation nearest = null;
            float closest = float.MaxValue;
            foreach(SanctuaryStation station in sanctuaryStations)
            {
                if(station == null || station.radius <= 0f ||
                    string.IsNullOrEmpty(station.id))continue;
                float distance = Vector2.Distance(player.body.position,
                    new Vector2(station.x, station.y));
                if(distance < station.radius && distance < closest)
                {
                    closest = distance;
                    nearest = station;
                }
            }
            return nearest;
        }

        float StationCooldownRemaining(SanctuaryStation station)
        {
            if(station == null)return 0f;
            float until;
            return stationCooldowns.TryGetValue(station.id, out until)
                ? Mathf.Max(0f, until - Time.time) : 0f;
        }

        void ActivateSanctuaryStation(SanctuaryStation station)
        {
            // A button press/keyboard command must be checked again at dispatch
            // time to stop remote activation or duplicate taps.
            if(station == null || !inSanctuary || inventoryOpen ||
                wardrobeOpen || meditationOpen || gameOver ||
                NearbySanctuaryStation() != station ||
                StationCooldownRemaining(station) > 0f)return;

            if(station.kind == "wardrobe")
            {
                OpenSanctuaryWardrobe();
                return;
            }
            if(station.kind == "meditation")
            {
                OpenSanctuaryMeditation();
                return;
            }
            if(station.kind != "fountain_buff")
            {
                Debug.LogWarning("Unknown sanctuary station kind: " + station.kind);
                return;
            }
            float duration = Mathf.Clamp(station.durationSeconds, 5f, 300f);
            float cooldown = Mathf.Max(duration, station.cooldownSeconds);
            fountainDamageBonus = Mathf.Clamp(station.damageMultiplier, 1f, 2f);
            fountainSpeedBonus = Mathf.Clamp(station.speedMultiplier, 1f, 2f);
            fountainBuffUntil = Time.time + duration;
            stationCooldowns[station.id] = Time.time + cooldown;
            // Proximity and cooldown are checked before granting this heal.
            float healed=RecoverFountainHealth();
            NecroCombatFx.Emit(player.body.position, sanctuary.root.transform, 1);
            Announce("Bencao arcana: +dano e +velocidade por " +
                Mathf.CeilToInt(duration) + "s; +" +
                Mathf.CeilToInt(healed) + " vida.");
            SaveProgress();
            UpdateSanctuaryInteractionVisual();
        }

        void ReadSanctuaryInteractionKey()
        {
            if(Input.GetKeyDown(KeyCode.E))
                ActivateSanctuaryStation(NearbySanctuaryStation());
        }

        void UpdateSanctuaryInteractionVisual()
        {
            if(fountainGlow == null || !inSanctuary)return;
            bool active = Time.time < fountainBuffUntil;
            fountainGlow.enabled = active;
            if(!active)return;
            float wave = Mathf.Sin(Time.time * 4.8f);
            fountainGlow.color =
                new Color(.48f, .99f, .87f, .17f + (wave + 1f) * .07f);
            fountainGlow.transform.localScale =
                fountainGlowBaseScale * (1f + .035f * wave);
        }

        Rect SanctuaryInteractionButtonRect(float scale)
        {
            float w = Screen.width / scale, h = Screen.height / scale;
            float x = Mathf.Max(148f, w - 198f);
            return new Rect(x, h - 224f, Mathf.Max(110f, w - x - 12f), 51f);
        }

        void DrawSanctuaryInteractionUi(float scale)
        {
            float w = Screen.width / scale;
            float remaining = fountainBuffUntil - Time.time;
            if(remaining > 0f)
            {
                Rect status = new Rect(w - 192f, 116f, 181f, 29f);
                GUI.Box(status, GUIContent.none);
                GUI.Label(status, "BENCAO ARCANA  " +
                    Mathf.CeilToInt(remaining) + "s", smallStyle);
            }

            SanctuaryStation station = NearbySanctuaryStation();
            if(station == null || inventoryOpen || wardrobeOpen || meditationOpen)return;
            float cooldown = StationCooldownRemaining(station);
            bool active = station.kind == "fountain_buff" && remaining > 0f;
            string caption = active ? "BENCAO ATIVA" :
                cooldown > 0f ? "RECARGA  " + Mathf.CeilToInt(cooldown) + "s" :
                station.label;
            Rect button = SanctuaryInteractionButtonRect(scale);
            GUI.Label(new Rect(button.x - 2f, button.y - 27f,
                    button.width + 4f, 24f),
                station.kind == "wardrobe" ? "GUARDA-ROUPA  |  EQUIPAMENTOS" :
                station.kind == "meditation" ? "ALTAR  |  ATRIBUTOS PERMANENTES" :
                "FONTE ARCANA  |  CURA + BENCAO TEMPORARIA", smallStyle);
            GUI.enabled = !active && cooldown <= 0f && !gameOver;
            if(GUI.Button(button, caption, buttonStyle))
                ActivateSanctuaryStation(station);
            GUI.enabled = true;
        }
    }
}

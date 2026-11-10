#!/usr/bin/env python3
"""Static regression checks for Unity 2022.3 project; NOT a runtime play test."""
import json
import re
import struct
import zlib
from pathlib import Path

root = Path(__file__).resolve().parents[1]
unity = root / "unity"
required = [
    "ProjectSettings/ProjectVersion.txt",
    "Packages/manifest.json",
    "Assets/Editor/NecroSceneBootstrap.cs",
    "Assets/Editor/NecromancerAndroidBuild.cs",
    "Assets/Editor/NecromancerWindowsBuild.cs",
    "Assets/Scripts/NecroLevels.cs",
    "Assets/Scripts/NecroArt.cs",
    "Assets/Scripts/NecroPrototype.cs",
    "Assets/Scripts/NecroGameplay.cs",
    "Assets/Scripts/NecroRespawn.cs",
    "Assets/Scripts/NecroUI.cs",
    "Assets/Scripts/NecroPolishedHUD.cs",
    "Assets/Scripts/NecroCreatureArt.cs",
    "Assets/Scripts/NecroCreatureView.cs",
    "Assets/Scripts/NecroCorpseRitual.cs",
    "Assets/Scripts/NecroFrameLibrary.cs",
    "Assets/Scripts/NecroHeroSheetBuilder.cs",
    "Assets/Scripts/NecroHeroPaintedSource.cs",
    "Assets/Resources/AnimationSheets/necromante_source.png",
    "Assets/Editor/NecroFrameEditorCheck.cs",
    "Assets/Editor/NecroDefenseEditorCheck.cs",
    "Assets/Editor/NecroTargetEditorCheck.cs",
    "Assets/Editor/NecroExplorationEditorCheck.cs",
    "Assets/Editor/NecroEncounterEditorCheck.cs",
    "Assets/Editor/NecroRegionEditorCheck.cs",
    "Assets/Editor/NecroDownedEditorCheck.cs",
    "Assets/Editor/NecroSombriaTextureImport.cs",
    "Assets/Editor/NecroCharacterSwitchEditorCheck.cs",
    "Assets/Scripts/NecroCombatFx.cs",
    "Assets/Scripts/NecroScenery.cs",
    "Assets/Editor/NecroVisualEditorCheck.cs",
    "Assets/Scripts/NecroCharacterAnimation.cs",
    "Assets/Scripts/NecroHeroCombatPolish.cs",
    "Assets/Scripts/NecroHeroSurvival.cs",
    "Assets/Scripts/NecroDifficultyBalance.cs",
    "Assets/Scripts/NecroDefenseRecovery.cs",
    "Assets/Scripts/NecroTargetRules.cs",
    "Assets/Scripts/NecroExplorationFeedback.cs",
    "Assets/Scripts/NecroEncounterRules.cs",
    "Assets/Scripts/NecroRegionTravel.cs",
    "Assets/Scripts/NecroDownedChoices.cs",
    "Assets/Scripts/NecroSanctuaryInteractions.cs",
    "Assets/Scripts/NecroSanctuaryWardrobe.cs",
    "Assets/Scripts/NecroSanctuaryMeditation.cs",
    "Assets/Scripts/NecroSkillTree.cs",
    "Assets/Scripts/NecroInterfaceScope.cs",
    "Assets/Scripts/NecroSoulPulse.cs",
    "Assets/Scripts/NecroGraveCommand.cs",
    "Assets/Scripts/NecroServantTactics.cs",
    "Assets/Scripts/NecroServantFormation.cs",
    "Assets/Scripts/NecroProgressHud.cs",
    "Assets/Resources/Data/sanctuary_interactions.json",
    "Assets/Resources/Sprites/necromante.png",
    "Assets/Scripts/NecroCrypt.cs",
    "Assets/Scripts/NecroSaveData.cs",
    "Assets/Editor/NecroSaveEditorCheck.cs",
    "Assets/Resources/Data/levels.json",
]
for filename in required:
    assert (unity / filename).is_file(), f"File missing: {filename}"

manifest = json.loads((unity / "Packages/manifest.json").read_text())
assert "com.unity.2d.sprite" in manifest["dependencies"]

levels = json.loads((unity / "Assets/Resources/Data/levels.json").read_text())["levels"]
assert [area["id"] for area in levels] == [
    "bonehollow", "lower_tunnels", "necropolis", "marsh"
]
kinds = set()
for area in levels:
    assert area["width"] > 20 and area["height"] > 15
    assert area["objective"] and len(area["enemies"]) >= (3 if area["id"]=="bonehollow" else 6)
    assert len(area["resources"]) >= 3
    kinds.update(entity["kind"] for entity in area["enemies"])
    assert area["startX"] > 0 and area["startY"] > 0
    for entity in [
        *area["enemies"], *area["resources"],
        {"kind": "player", "x": area["startX"], "y": area["startY"]},
    ]:
        x, y = entity["x"], entity["y"]
        assert 0 < x < area["width"] and 0 < y < area["height"]
        assert all(
            (x - obstacle["x"]) ** 2 + (y - obstacle["y"]) ** 2
            > (obstacle["r"] + .2) ** 2
            for obstacle in area["obstacles"]
        ), f"Blocked start/spawn in {area['id']}: {entity}"
assert {"rat", "bat", "slime", "spider", "snake", "skeleton",
        "wisp", "grave_pup", "grave_lord", "marsh_lord"} <= kinds
assert len(levels[0]["enemies"])==3
assert {e["kind"] for e in levels[0]["enemies"]}=={"grave_pup"}
pup_art=(unity/"Assets/Resources/Creatures/grave_pup.png").read_bytes()
assert struct.unpack(">II",pup_art[16:24])==(1024,256) and pup_art[24:26]==bytes([8,6])

scripts = {
    name: (unity / "Assets/Scripts" / name).read_text()
    for name in ("NecroPrototype.cs", "NecroGameplay.cs", "NecroUI.cs",
                 "NecroArt.cs", "NecroCrypt.cs", "NecroSaveData.cs")
}
gameplay = scripts["NecroGameplay.cs"]
core = scripts["NecroPrototype.cs"]
ui = scripts["NecroUI.cs"]
for feature in ("LoadStage(", "StageInstance", "current.root.SetActive(false)",
                "UpdateActors(", "SpawnResource(", "ApplySpeciesRank("):
    assert any(feature in source for source in scripts.values()), feature
for feature in ("UpgradeSpecies(", "SpawnDrop(", "CollectNearby(", "Visible(",
                "Raise(", "Dismiss(", "ManaCost(", "CommandCost("):
    assert feature in gameplay, f"Missing gameplay rule {feature}"
# Respawning and auto combat must work in regions restored from older saves.
respawn=(unity/"Assets/Scripts/NecroRespawn.cs").read_text()
hud=(unity/"Assets/Scripts/NecroPolishedHUD.cs").read_text()
assert "const int CommandLimit=12;" in core
assert "TickRespawns(dt);" in core
assert "TryAutomaticAttack" not in core+respawn
assert "Input.GetKeyDown(KeyCode.Space)" not in core
# Both melee and spectral projectile are free. Only Raise() spends anima.
cast_body = gameplay.split("        void CastAtRange(float range)", 1)[1].split(
    "        void UpdateProjectiles(", 1
)[0]
raise_body = gameplay.split("        void Raise()", 1)[1].split(
    "        void Dismiss()", 1
)[0]
assert "mana-=9" not in gameplay
assert "mana<9" not in gameplay
assert "mana" not in cast_body, "Basic attack must be independent of anima"
assert "mana" not in respawn, "Autoattack must not depend on anima"
assert "mana<cost" in raise_body and "mana-=cost;" in raise_body
assert "SkillRaiseManaCost(nearest.kind)" in raise_body
assert "respawnTimer=RespawnInterval" in core
for feature in ("TickRespawns(float dt)", "IsBoss(entry.kind)",
                "MinEnemySeparation=1.8f", "current.enemies.Add(spawned)",
                "RespawnInterval=12f", "RespawnPopulationLimitForStage()",
                "RespawnIntervalForStage()", "SpawnMinDistanceForStage()",
                "SpawnMaxDistanceForStage()"):
    assert feature in respawn, f"Missing loop/autoattack feature {feature}"
assert 'spellUnlocked?"MAGIA":"GOLPE"' not in hud
# Automatic attack has no button or oversized permanent caption.
assert '"ATAQUE AUTOMÁTICO"' not in hud
assert 'CastAtRange(ManualRange);' in ui
assert '"REVIVER",Raise' in hud
assert '"EVOLUIR"' in hud
assert "int armyPage;" in hud
boss_kinds={"warden","grave_lord","marsh_lord"}
for area in levels:
    normal=[enemy for enemy in area["enemies"] if enemy["kind"] not in boss_kinds]
    assert normal, f"No respawnable regular enemies in {area['id']}"
    assert all(0 < x["x"] < area["width"] and 0 < x["y"] < area["height"] for x in normal)
assert "Input.touches" in ui
assert "DrawWitch(" in scripts["NecroArt.cs"]
assert "DrawCryptAltar(" in scripts["NecroArt.cs"]
crypt = scripts["NecroCrypt.cs"]
data = scripts["NecroSaveData.cs"]
for feature in ("OpenCrypt(", "StoreServant(", "WithdrawServant(",
                "RitualVitality(", "RitualVault(", "RitualSpirit(",
                "SaveProgress(", "RestoreProgress(", "RecordStage(",
                "RestoreStage(", "Autosave(",
                "RecoverAfterDeath("):
    assert feature in crypt, f"Missing Cripta 0.5 method: {feature}"
for feature in ("NecroSaveData", "NecroSaveStore", "Application.persistentDataPath",
                "SavePath", "BackupPath", "JsonUtility.FromJson",
                "JsonUtility.ToJson", "vault", "stages"):
    assert feature in data, f"Missing save component: {feature}"
assert "RestoreProgress();" not in scripts["NecroPrototype.cs"]
assert "SaveProgress();" not in gameplay.split("void Damage(",1)[1].split("Actor servantOrder;",1)[0]
assert "DrawCryptPanel(" in scripts["NecroUI.cs"]
assert "SceneManager.LoadScene" not in scripts["NecroUI.cs"]
assert "NecroSaveEditorCheck.Verify();" in (
    unity / "Assets/Editor/NecromancerAndroidBuild.cs"
).read_text()
assert "JsonUtility.ToJson(snapshot)" in (
    unity / "Assets/Editor/NecroSaveEditorCheck.cs"
).read_text()
assert "Backgrounds/" in scripts["NecroArt.cs"]
assert "AddComponent<Necromancer.NecroPrototype>()" in (
    unity / "Assets/Editor/NecroSceneBootstrap.cs"
).read_text()
builder = (unity / "Assets/Editor/NecromancerAndroidBuild.cs").read_text()
assert "AndroidArchitecture.ARM64" in builder
assert "ScriptingImplementation.IL2CPP" in builder
assert "BuildTarget.Android" in builder
assert re.search(r'BuildReport\s+report\s*=', builder)

# Validate the actual heroine sprite rather than trusting a placeholder mockup.
hero_png = (unity / "Assets/Resources/Sprites/necromante.png").read_bytes()
assert hero_png[:8] == bytes([137,80,78,71,13,10,26,10])
offset=8
idat=bytearray()
width=height=0
found_end=False
while offset < len(hero_png):
    assert offset+12 <= len(hero_png), "Truncated PNG chunk"
    length=struct.unpack(">I",hero_png[offset:offset+4])[0]
    tag=hero_png[offset+4:offset+8]
    payload=hero_png[offset+8:offset+8+length]
    checksum=struct.unpack(">I",hero_png[offset+8+length:offset+12+length])[0]
    assert (zlib.crc32(tag+payload)&0xffffffff)==checksum, f"PNG CRC mismatch {tag}"
    if tag==b"IHDR":
        width,height=struct.unpack(">II",payload[:8])
    if tag==b"IDAT":
        idat.extend(payload)
    offset+=12+length
    if tag==b"IEND":
        found_end=True
        break
assert found_end and width>=90 and height>=110
assert len(zlib.decompress(idat))>width*height, "Invalid sprite scanlines"

hud=(unity/"Assets/Scripts/NecroPolishedHUD.cs").read_text()
for name in ("DrawPolishedHUD(", "DrawPortraitUi(", "DrawMiniMapUi(",
             "DrawIconUi(", "DrawArmyInspect(", "DrawGearInspect("):
    assert name in hud, "Missing interactive UI: "+name
animation=(unity/"Assets/Scripts/NecroCharacterAnimation.cs").read_text()
for name in ("InitializeHeroAnimation(", "AnimateHeroVisual(", "ReleaseSkull()",
             "witchArtRenderer.flipX"):
    assert name in animation, "Missing character animation: "+name
assert "InitializeHeroAnimation();" in core
assert "AnimateHeroVisual(dt);" in core
assert "joystickFingerId" in core and "TouchPhase.Began" in ui and "ReadMovement(hudScale)" in core
onscreen=ui.split("void OnGUI()",1)[1].split("void DrawCryptPanel",1)[0]
assert "RingUi(pad,stickDisc" in onscreen and '"ATACAR"' in onscreen
# The current preview needs the companion roster and its painted controls.
assert (unity / "Assets/Scripts/NecroRoster.cs").is_file()
for control in ("attack", "bind", "pearl"):
    texture = unity / f"Assets/Resources/HUD/{control}.png"
    assert texture.read_bytes().startswith(b"\x89PNG\r\n\x1a\n"), texture
assert "ReadManualAttack(hudScale);" in core and "frame=24+walkDirection" in animation
idle=(unity/"Assets/Resources/Heroine/idle.png").read_bytes()
assert struct.unpack(">II",idle[16:24])==(1024,512) and idle[24:26]==bytes([8,6])
loop=core.split("void Update()",1)[1].split("bool NearSeal",1)[0]
for removed in ("TryPortal(", "Raise(", "Claim(", "Autosave("):
    assert removed not in loop
assert "CollectNearby();" in loop, "Gameplay must collect map resources"
assert "UpdateResourceExploration();" in loop, "Explore visuals must update after collecting"
assert "!current.claimed" not in respawn and "current.respawnTimer=RespawnIntervalForStage();" in respawn
assert 'Resources.Load<Texture2D>("Sprites/"+kind)' in scripts["NecroArt.cs"]
assert "RitualCircle()" in scripts["NecroArt.cs"]
assert "PlayerSettings.bundleVersion = \"0.9.22\"" in builder
assert "com.takeo.necromancer.unity.preview080" in builder
assert "NecroFrameEditorCheck.Verify();" in builder
assert "Assets/Resources/AnimationSheets/necromante_source.png" in required
hero_sheet=(unity/"Assets/Resources/AnimationSheets/necromante_source.png").read_bytes()
assert hero_sheet.startswith(bytes([137,80,78,71,13,10,26,10]))
sw,sh=struct.unpack(">II",hero_sheet[16:24])
assert sw>=120 and sh>=180 and sw%4==0 and sh%4==0
assert "NecroHeroSheetBuilder.Build(poses)" in (unity/"Assets/Scripts/NecroFrameLibrary.cs").read_text()
hero_painter=(unity/"Assets/Scripts/NecroHeroSheetBuilder.cs").read_text()
for token in ("Counts={4,8,6,6,8,3}", "return variant*4+direction;", "Texture2D Build(",
              "PaintFrame(", "EllipseRing(", "Glow("):
    assert token in hero_painter, token
frame_library=(unity/"Assets/Scripts/NecroFrameLibrary.cs").read_text()
frame_test=(unity/"Assets/Editor/NecroFrameEditorCheck.cs").read_text()
assert "NecroHeroPaintedSource.Build()" in frame_test
assert "int columns=hero?8:6" in frame_test
assert "NecroHeroPaintedSource.Build()" in frame_library
assert "Sprite frame=heroSprites[0]" in hud
painted=(unity/"Assets/Scripts/NecroHeroPaintedSource.cs").read_text()
for required in ("const int Tile=160", "PaintBody(", "Paint(", "Color32[]",
                 "Polygon(", "HatLight", "HairShade", "Mana", "texture.Apply("):
    assert required in painted, "Missing detailed heroine artwork: "+required
assert "return variant*4+direction;" in hero_painter
assert "No head or torso distortion" in hero_painter
frame_library=(unity/"Assets/Scripts/NecroFrameLibrary.cs").read_text()
hero_anim=(unity/"Assets/Scripts/NecroCharacterAnimation.cs").read_text()
creature_anim=(unity/"Assets/Scripts/NecroCreatureView.cs").read_text()
frame_editor=(unity/"Assets/Editor/NecroFrameEditorCheck.cs").read_text()
for required in ("NecroPose", "NecroDirection", "NecroFrameSet",
                 "ReadPixels(", "SetPixels32(", "Sprite.Create(", "frame*cell",
                 "AnimatedCreatures", "NecroPose.Raise", "NecroPose.Hurt"):
    assert required in frame_library, required
for required in ("heroSprites[frame]", "heroineStrikePulse",
                 "heroineHitPulse", "DirectionFrom("):
    assert required in hero_anim, required
for required in ("frames.Frame(", "currentPose", "currentDirection", "FrameCount"):
    assert required in (creature_anim+frame_library), required
assert "NecroFrameLibrary.Get(kind,original.sprite)" in creature_anim
assert "heroineStrikePulse=.53f;" in gameplay
assert "ApplyHeroDamage(amount);" in gameplay
assert "heroineHitPulse=.34f;" in (unity/"Assets/Scripts/NecroHeroSurvival.cs").read_text()
assert "VerifySet(\"necromante\",true);" in frame_editor
assert "com.takeo.necromancer.unity.preview080" in builder
assert "NecroVisualEditorCheck.Verify();" in builder
creature_art=(unity/"Assets/Scripts/NecroCreatureArt.cs").read_text()
creature_visual=(unity/"Assets/Scripts/NecroCreatureView.cs").read_text()
combat_fx=(unity/"Assets/Scripts/NecroCombatFx.cs").read_text()
scenery=(unity/"Assets/Scripts/NecroScenery.cs").read_text()
assert "NecroCreatureArt.Draw(t,kind)" in scripts["NecroArt.cs"]
assert "NecroCreatureView.Attach(sprite,kind,ally)" in core
assert "NecroScenery.Populate(level,next.root.transform)" in core
assert "NecroCombatFx.Emit(" in gameplay
for creature in ("rat","bat","slime","spider","snake","skeleton","wisp",
                 "warden","grave_lord","marsh_lord"):
    assert f'case "{creature}"' in creature_art, creature
for required in ("LateUpdate()", "Flash()", "Strike()"):
    assert required in creature_visual, required
for required in ("Capacity=30", "void Update()", "void OnDestroy()"):
    assert required in combat_fx, required
for scene in ("headstone","broken_cross","lantern","bones","fungus","reeds",
              "pool","rune","crystal"):
    assert f'case "{scene}"' in scenery, scene
assert "default: Stalagmite(t)" in scenery
assert "No illustration" in (unity/"Assets/Editor/NecroVisualEditorCheck.cs").read_text()
assert 'NECRO_ANDROID_MODE' in builder
assert "com.takeo.necromancer.unity.preview" in builder
assert "useCustomKeystore=true" in builder
assert "NECRO_KEYSTORE_PASSWORD" in builder
preview_workflow=(root/".github/workflows/unity-android.yml").read_text()
release_workflow=(root/".github/workflows/unity-release.yml").read_text()
assert '"mode": "preview"' in preview_workflow
assert '"mode": "release"' in release_workflow
assert "ANDROID_KEYSTORE_BASE64" in release_workflow
assert ".android-release.keystore" in release_workflow

# Production raster assets have PNG structure, dimensions and CRC checks.
for png_name,minimum in [
    ("Assets/Resources/Backgrounds/entrada_da_caverna_bonehollow.png",512),
    ("Assets/Resources/Scenery/crystal.png",256),
    ("Assets/Resources/Scenery/rock_cluster.png",256),
]:
    blob=(unity/png_name).read_bytes()
    assert blob[:8]==bytes([137,80,78,71,13,10,26,10]), png_name
    sw,sh=struct.unpack(">II",blob[16:24])
    assert sw>=minimum and sh>=minimum, png_name
    offset=8
    seen_image=False
    while offset+12<=len(blob):
        n=struct.unpack(">I",blob[offset:offset+4])[0]
        typ=blob[offset+4:offset+8]
        data=blob[offset+8:offset+8+n]
        expected=struct.unpack(">I",blob[offset+8+n:offset+12+n])[0]
        assert (zlib.crc32(typ+data)&0xffffffff)==expected, png_name
        if typ==b"IDAT":seen_image=True
        offset+=n+12
        if typ==b"IEND":break
    assert seen_image and offset==len(blob), png_name

asset_check=(unity/"Assets/Editor/NecroIllustratedAssetEditorCheck.cs").read_text()
assert "NecroIllustratedAssetEditorCheck.Verify();" in builder
assert "entrada_da_caverna_bonehollow" in asset_check
assert "SpriteDrawMode.Tiled" in core
assert "FitGround(next.backdrop,level);" in core
assert "FitGround(current.backdrop,level);" in core
assert 'joystickAnchor=touch.position' in ui and 'ReadMovement(hudScale);' in core
assert "Resources.Load<Texture2D>(\"Scenery/\"+kind)" in scenery
assert "Resources.Load<Texture2D>(\"Scenery/rock_cluster\")" in scripts["NecroArt.cs"]
assert "Time.time<noticeUntil" in hud
assert "Região conquistada — explore as passagens." not in hud
assert 'new Rect(0,0,width,89)' in hud
assert "Soft contact shadow" in hero_anim
spectral=(unity/"Assets/Resources/Heroine/spectral.png").read_bytes()
assert struct.unpack(">II",spectral[16:24])==(1024,2048)
assert spectral[24:26]==bytes([8,6]), "Heroine and projectile require explicit 8-bit RGBA"
assert "Resources.Load<Texture2D>(SelectedCharacter.spectralResource)" in hero_anim
assert "Resources.Load<Texture2D>(SelectedCharacter.idleResource)" in hero_anim
assert "Staff spectral aura" not in hero_anim


# 0.7.6 mobile asset budget/policy: keep obsolete art out of Resources.
policy=(unity/"Assets/Editor/NecroSanctuaryImportSettings.cs").read_text()
for token in ("TextureImporterFormat.ETC2_RGBA8",
              "TextureImporterFormat.ETC2_RGB4",
              "TextureImporterCompression.CompressedHQ",
              "t.mipmapEnabled = false",
              't.GetPlatformTextureSettings("Android")',
              "android.maxTextureSize = 2048"):
    assert token in policy, "Android texture policy missing: "+token
for obsolete in ("safe_chamber.png", "safe_chamber_v2.jpg"):
    assert not (unity/"Assets/Resources/Backgrounds"/obsolete).exists(), (
        "Legacy sanctuary painting returned to Resources: "+obsolete)
safe_data=json.loads((unity/"Assets/Resources/Data/sanctuary.json").read_text())
assert safe_data["background"]=="safe_chamber_v3"
assert (unity/"Assets/Resources/Backgrounds/safe_chamber_v3.jpg").is_file()
layout=json.loads((unity/"Assets/Resources/Data/sanctuary_props.json").read_text())
assert layout["version"]>=6
assert len(layout["props"])>=8
for prop in layout["props"]:
    if prop["layer"]!="disabled":
        asset=unity/"Assets/Resources/SanctuaryProps"/(prop["asset"]+".png")
        assert asset.is_file(), "Missing active sanctuary sprite: "+str(asset)

# 0.8.0: temporary, data-driven sanctuary blessing.
station_json=json.loads((unity/"Assets/Resources/Data/sanctuary_interactions.json").read_text())
fountains=[s for s in station_json["stations"] if s["kind"]=="fountain_buff"]
assert len(fountains)==1
assert station_json["version"]>=4
assert len(station_json["stations"])==1
assert all(s["kind"]=="fountain_buff" for s in station_json["stations"])
f=fountains[0]
assert f["id"]=="fountain" and f["radius"]>0
assert f["durationSeconds"]==45 and f["cooldownSeconds"]==120
assert f["damageMultiplier"]==1.25 and f["speedMultiplier"]==1.2
assert f["cooldownSeconds"]>=f["durationSeconds"]
assert 0<f["x"]<16 and 0<f["y"]<24
assert abs(f["y"]-16.85)>3.5/2+.384, "Interaction inside fountain collision"
interactions=(unity/"Assets/Scripts/NecroSanctuaryInteractions.cs").read_text()
for token in ("NearbySanctuaryStation()", "StationCooldownRemaining(",
              "ActivateSanctuaryStation(", "FountainDamageMultiplier",
              "FountainSpeedMultiplier", "KeyCode.E", "Time.time + duration",
              "NecroArt.RitualCircle()", "NearbySanctuaryStation() != station"):
    assert token in interactions, token
assert "BuildSanctuaryInteractions(sanctuary.root.transform);" in (
    unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "DrawSanctuaryInteractionUi(scale);" in ui
assert "SanctuaryInteractionButtonRect(scale).Contains(gui)" in ui
assert "FountainSpeedMultiplier*dt" in core
assert "FountainDamageMultiplier" in gameplay
assert "PlayerSettings.Android.bundleVersionCode = 30;" in builder


# Direct APK download + stable signing: regression checks for future sessions.
preview_workflow=(root/".github/workflows/unity-android.yml").read_text()
assert 'uses: actions/upload-artifact@v7' in preview_workflow
assert 'archive: false' in preview_workflow
assert 'path: build/Android/necromancer-unity.apk' in preview_workflow
for token in ('PREVIEW_KEYSTORE_BASE64', 'PREVIEW_KEYSTORE_PASSWORD',
              'unity/.android-preview.keystore', 'PREVIEW_RUN_NUMBER:',
              'necromancer-preview', 'Verify stable preview signing identity'):
    assert token in preview_workflow, 'Missing preview workflow setting: '+token
for token in ('NECRO_PREVIEW_BUILD_NUMBER', '10000 + previewBuildNumber',
              'if(release || preview)', 'preview?".android-preview.keystore"',
              'PlayerSettings.Android.useCustomKeystore=true',
              'LoadCiBuildSettings()', '.android-build-settings.json',
              'ci.previewRunNumber', 'ci.keystorePassword', 'ci.keyAlias'):
    assert token in builder, 'Missing stable APK upgrade logic: '+token
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'PlayerSettings.Android.useCustomKeystore=false' not in builder
assert (root/'docs/ANDROID_SIGNING.md').is_file()

# GameCI Docker ignores arbitrary env variables: required protected bridge.
assert 'name: Prepare protected Unity build settings' in preview_workflow
assert 'dest = Path("unity/.android-build-settings.json")' in preview_workflow
assert '"previewRunNumber": run_number' in preview_workflow
assert '"mode": "preview"' in preview_workflow
assert 'unity/.android-preview.keystore unity/.android-build-settings.json' in preview_workflow
release_workflow=(root/".github/workflows/unity-release.yml").read_text()
assert 'name: Prepare protected Unity release settings' in release_workflow
assert '"mode": "release"' in release_workflow
assert 'unity/.android-release.keystore unity/.android-build-settings.json' in release_workflow
gitignore=(root/".gitignore").read_text()
assert 'unity/.android-build-settings.json' in gitignore
assert 'unity/.android-preview.keystore' in gitignore
assert 'unity/.android-release.keystore' in gitignore


# 0.8.1: wardrobe upgrades reuse current equipped focus/mantle (no new art).
wardrobe_source=(unity/"Assets/Scripts/NecroSanctuaryWardrobe.cs").read_text()
for token in ("bool wardrobeOpen", "OpenSanctuaryWardrobe()",
              "CloseSanctuaryWardrobe()", "DrawSanctuaryWardrobePanel(",
              "UpgradeSanctuaryEquipment(", "FocusUpgradeCost(",
              "MantleUpgradeCost(", "SaveProgress();",
              "staffTier++;", "mantleTier++;",
              "player.maxHp += 18f", "player.hp = Mathf.Min("):
    assert token in wardrobe_source, "Missing wardrobe action: "+token
assert (unity/"Assets/Scripts/NecroSanctuaryWardrobe.cs").is_file()
assert 'if(station.kind == "wardrobe")' not in interactions
assert 'OpenSanctuaryWardrobe();' not in interactions
assert 'station.kind == "wardrobe" ? "GUARDA-ROUPA' not in interactions
assert 'if(wardrobeOpen)' in ui
assert 'DrawSanctuaryWardrobePanel(Screen.width/scale,Screen.height/scale);' in ui
assert 'if(wardrobeOpen)' in core
assert 'if(inSanctuary)SaveCompanions();' in crypt
assert 'playerX=checkpoint.x,playerY=checkpoint.y' in crypt
assert 'NecroLevels.At(levels[checkpointStage],' in crypt
assert 'levels[checkpointStage].startX,levels[checkpointStage].startY)' in crypt
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert '10000 + previewBuildNumber' in builder
assert 'LoadCiBuildSettings()' in builder


# 0.8.2: fully readable mobile wardrobe without any new textures.
for token in ("WardrobeBackground", "new Color(.095f,.069f,.134f,.97f)",
              "WardrobeCardBackground", "new Color(.071f,.052f,.102f,.975f)",
              "WardrobeShade", "DrawWardrobeSurface(", "DrawWardrobeGearCard(",
              "WardrobeActionButton(", "EnsureWardrobeStyles()",
              "wardrobeInactiveButtonText", "GUIStyle.none",
              "FillUi(new Rect(0f,0f,width,height),WardrobeShade)"):
    assert token in wardrobe_source, "Missing 0.8.2 visual treatment: "+token
assert 'GUI.Box(panel,GUIContent.none)' not in wardrobe_source
assert "Texture2D(" not in wardrobe_source
assert "Resources.Load" not in wardrobe_source


# Independent, on-demand Unity target builds (no unnecessary platform build).
android_preview=(root/".github/workflows/unity-android.yml").read_text()
windows_preview=(root/".github/workflows/unity-windows.yml").read_text()
legacy_android=(root/".github/workflows/android.yml").read_text()
assert "workflow_dispatch:" in android_preview
assert "workflow_dispatch:" in windows_preview
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert "targetPlatform: Android" in android_preview
assert "targetPlatform: StandaloneWindows64" in windows_preview
assert "NecromancerWindowsBuild.BuildWindows" in windows_preview
assert "NecromancerAndroidBuild.BuildAndroid" in android_preview
assert "build/StandaloneWindows64/necromancer-windows.exe" in windows_preview
assert "build/StandaloneWindows64/necromancer-windows_Data" in windows_preview
assert "necromancer-windows-x64" in windows_preview
assert "path: build/StandaloneWindows64/" in windows_preview
assert "path: build/Android/necromancer-unity.apk" in android_preview
assert "archive: false" in android_preview
assert "Unity project structure" not in windows_preview.split("name:",1)[0]
assert "      - 'app/**'" in legacy_android
assert "      - 'tests/com/**'" in legacy_android
win_builder=(unity/"Assets/Editor/NecromancerWindowsBuild.cs").read_text()
for token in ("public static void BuildWindows()", "NecroSceneBootstrap.Setup();",
              "NamedBuildTarget.Standalone", "ScriptingImplementation.Mono2x",
              "BuildTarget.StandaloneWindows64", "necromancer-windows.exe",
              "BuildPipeline.BuildPlayer(options)", "BuildResult.Succeeded"):
    assert token in win_builder, "Windows builder missing: "+token
assert "AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64" in builder
assert (root/"docs/COMPILACAO_ANDROID_WINDOWS.md").is_file()


# 0.9.0: meditation station and real XP, permanent attributes and safe loading.
meditation_source=(unity/"Assets/Scripts/NecroSanctuaryMeditation.cs").read_text()
assert station_json["version"]>=3
assert 'station.kind == "meditation"' not in interactions
assert 'OpenSanctuaryMeditation();' not in interactions
for token in ("RestoreMeditationAllocations(", "MeditationPointsAvailable()",
              "AwardMeditationExperience(", "playerLevel++;",
              "experience+=ExperienceValue(kind);", "vitalityPoints++",
              "powerPoints++", "agilityPoints++", "SaveProgress();",
              "DrawSanctuaryMeditationPanel(", "CloseSanctuaryMeditation()",
              "WardrobeActionButton(", "WardrobeBackground",
              "Mathf.Max(0,playerLevel-1-", "MeditationAttributeCap=12"):
    assert token in meditation_source, "Meditation missing: "+token
assert "if(!victim.ally)AwardMeditationExperience(victim.kind);" in gameplay
assert "powerPoints*2" in gameplay and "powerPoints*4" in gameplay
assert "(1f+agilityPoints*.06f)" in core
assert "if(meditationOpen)" in core
assert "RestoreSanctuaryProfile();" in core
assert "if(meditationOpen)" in ui
assert "DrawSanctuaryMeditationPanel(Screen.width/scale,Screen.height/scale);" in ui
assert "RestoreSanctuaryProfile()" in crypt
assert "previousStageSnapshots=saved.stages;" in crypt
assert "previousCheckpointStage=Mathf.Clamp(saved.stage,0,levels.Length-1);" in crypt
assert "previousStageSnapshots[i] : RecordStage(null)" in crypt
assert "previousCheckpointStage=save.stage;" in crypt
assert "vault.Clear();" in crypt
assert "vitalityPoints=vitalityPoints,powerPoints=powerPoints" in crypt
assert "RestoreMeditationAllocations(saved);" in crypt
assert "RestoreMeditationAllocations(save);" in crypt
assert "public int vitalityPoints,powerPoints,agilityPoints;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "public const int Version=1;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder


# 0.9.1: six persistent nodes across three skill branches. Skills share no
# budget with attributes, require predecessor nodes, and affect real gameplay.
skill_source=(unity/"Assets/Scripts/NecroSkillTree.cs").read_text()
for token in ("const int SkillTreeNodeCount=6", "SkillPrerequisite={-1,0,-1,2,-1,4}",
              "SkillRaiseManaCost(", "SkillServantDamage(", "SkillSpectralDamage(",
              "RestoreSkillTree(NecroSaveData saved)", "UnlockSkillTreeNode(",
              "(playerLevel-1)/2-LearnedSkillCount()", "SkillUnlocked(prerequisite)",
              "learnedSkillMask|=1<<index", "SaveProgress();",
              "DrawSkillTreeContent(", "DrawSkillTreeNode(", "GUI.BeginScrollView",
              "GUI.EndScrollView()", "DrawMeditationAttributesContent("):
    assert token in skill_source, "Missing skill tree implementation: "+token
assert "AvailableSkillPoints()" in meditation_source and "MeditationPointsAvailable()" in meditation_source
assert 'if(meditationSkillsTab)DrawSkillTreeContent(viewport);' in meditation_source
assert 'DrawMeditationAttributesContent(viewport);' in meditation_source
assert 'SkillUnlocked(3)' in meditation_source and 'mana=Mathf.Min(maxMana,mana+4);' in meditation_source
assert "public int learnedSkills;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "learnedSkills=learnedSkillMask" in crypt
assert "RestoreSkillTree(saved);" in crypt and "RestoreSkillTree(save);" in crypt
assert "SkillRaiseManaCost(nearest.kind)" in gameplay
assert "SkillSpectralDamage(" in gameplay and "staffTier*8+(playerLevel-1)*3+" in gameplay
assert "SkillUnlocked(5) ? .60f : .72f" in gameplay
assert "friendly?SkillServantDamage(self.attack):self.attack" in gameplay
assert "pup.ally?SkillServantDamage(pup.attack):pup.attack" in gameplay
boar_source=(unity/"Assets/Scripts/NecroRotboar.cs").read_text()
assert "boar.ally?SkillServantDamage(boar.attack):boar.attack" in boar_source
assert "(SkillUnlocked(2)?1.5f:0f)" in core
assert "Regenerate(dt);" in core, "Anima regen must run from gameplay Update"
assert "if(mana>=maxMana){manaFraction=0f;return;}" in core
assert "0.9.1" in (root/"docs/SANCTUARY_INTERACTIONS.md").read_text()
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]


# 0.9.2: progression UI, notifications and refundable skill allocation.
progress_hud=(unity/"Assets/Scripts/NecroProgressHud.cs").read_text()
assert "void DrawProgressOverlay(" in progress_hud
assert "Time.time<noticeUntil" in progress_hud
assert "MeditationPointsAvailable()" not in progress_hud
assert "AvailableSkillPoints()" not in progress_hud
assert "Mathf.Clamp01((float)experience/" in progress_hud
assert "DrawProgressOverlay(Screen.width/scale,Screen.height/scale);" in ui
assert 'public int learnedSkills;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
for token in ("bool meditationSkillsTab,skillRespecConfirm",
              "SkillRespecCost()", "ResetLearnedSkills()",
              "if(!SaveProgress())", "learnedSkillMask=previousMask;",
              "shards=previousShards;", "learnedSkillMask=0;",
              "skillRespecConfirm=true;",
              "skillRespecConfirm=false;",
              "Mathf.Clamp01"):
    assert token in skill_source + meditation_source, "0.9.2 missing: "+token
assert '"EXPERIENCIA "+experience+"/"+ExperienceThreshold()' in meditation_source
assert "WardrobeActionButton" in skill_source
assert "GUI.BeginScrollView" in skill_source
assert "0.9.2" in (root/"docs/SANCTUARY_INTERACTIONS.md").read_text()


# 0.9.3: first active combat ability, shared Windows Q / Android touch.
pulse_source=(unity/"Assets/Scripts/NecroSoulPulse.cs").read_text()
fx_source=(unity/"Assets/Scripts/NecroCombatFx.cs").read_text()
for token in ("SoulPulseManaCost=18", "SoulPulseRadius=2.7f",
              "SoulPulseCooldownSeconds=9f", "TryCastSoulPulse()",
              "ReadActiveSkillInput(float scale)", "Input.GetKeyDown(KeyCode.Q)",
              "SoulPulseButtonRect(float scale)", "DrawSoulPulseButton(float scale)",
              "Vector2.Distance(center,enemy.body.position)",
              "Visible(center,enemy.body.position)", "if(targets==0)",
              "mana-=SoulPulseManaCost", "Damage(enemy,amount)",
              "soulPulseCooldownUntil=Time.time+SoulPulseCooldownSeconds",
              "pulseAwardBatch=true;", "finally { pulseAwardBatch=false; }",
              "SaveProgress();", "if(playerLevel>levelBefore)",
              "heroineRaisePulse=.40f", "NecroCombatFx.Emit(center,current.root.transform,3)"):
    assert token in pulse_source, "Soul pulse incomplete: "+token
assert "ReadActiveSkillInput(hudScale);" in core
assert "SoulPulseButtonRect(scale).Contains(gui)" not in ui
assert "DrawSoulPulseButton(scale);" not in ui
assert "ResetActiveSkillInput();" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "if(!pulseAwardBatch)SaveProgress();" in meditation_source
assert 'type==3?"Necrotic Soul Pulse"' in fx_source
assert "type==7 ? 16 :" in fx_source
assert "type==5 || type==6 ? 10 :" in fx_source
assert "type==3?11:type==4?14:18;" in fx_source
assert "new Vector3(4.0f,4.0f,1f)" in fx_source
assert "SpriteRenderer" in fx_source and "NecroArt.RitualCircle()" in fx_source
assert "mana<cost" in raise_body and "mana-=cost;" in raise_body
assert "mana" not in cast_body  # Basic attack stays free.
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]


# 0.9.4: temporary tactical order for existing necromancer servants.
command_source=(unity/"Assets/Scripts/NecroGraveCommand.cs").read_text()
for token in ("GraveCommandManaCost=20", "GraveCommandDuration=8f",
              "GraveCommandCooldownSeconds=18f",
              "GraveCommandDamageBonus=1.25f",
              "GraveCommandSpeedBonus=1.20f",
              "bool GraveCommandActive", "GraveCommandDamageMultiplier",
              "GraveCommandSpeedMultiplier(Actor actor)",
              "LivingServantCount()", "GraveCommandButtonRect(float scale)",
              "CanCastGraveCommand()", "TryCastGraveCommand()",
              "Input.GetKeyDown(KeyCode.R)",
              "ReadGraveCommandInput(float scale)",
              "DrawGraveCommandButton(float scale)", "if(living==0)",
              "mana-=GraveCommandManaCost",
              "graveCommandUntil=Time.time+GraveCommandDuration",
              "graveCommandCooldownUntil=Time.time+GraveCommandCooldownSeconds",
              "graveCommandTarget=Closest(7.2f)",
              "NecroCombatFx.Emit(player.body.position,current.root.transform,4)",
              "SaveProgress();"):
    assert token in command_source, "Missing 0.9.4 servant command: "+token
assert "ReadGraveCommandInput(scale);" in pulse_source
assert "graveCommandUntil=0f;" in pulse_source
assert "graveCommandTarget=null;" in pulse_source
assert "GraveCommandSpeedMultiplier(actor)" in gameplay
assert "FindServantTarget(self)" in gameplay
assert "GraveCommandDamageMultiplier" in skill_source
assert "GraveCommandButtonRect(scale).Contains(gui)" not in ui
assert "DrawGraveCommandButton(scale);" not in ui
boar_source=(unity/"Assets/Scripts/NecroRotboar.cs").read_text()
assert "FindServantTarget(boar)" in boar_source
assert 'type==4?"Order of the Dead"' in fx_source
assert "new Vector3(1.75f,1.75f,1f)" in fx_source
assert "new Color(.33f,.95f,.76f,.74f)" in fx_source
assert 282-78 > 195 and 195-78 > 108, "Spell touch controls overlap"
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]


# 0.9.5: shared companion targeting and persistent Hunt / Defend stance.
tactics_source=(unity/"Assets/Scripts/NecroServantTactics.cs").read_text()
for token in ("ServantLeashDistance=6.5f",
              "HuntPlayerRange=6.3f", "DefensePlayerRange=3.8f",
              "HuntServantRange=5.6f", "DefenseServantRange=3.6f",
              "IsCompanionTargetValid(Actor ally,Actor enemy,",
              "Actor FindServantTarget(Actor ally)", "!enemy.ally",
              "Visible(ally.body.position,enemy.body.position)",
              "GraveCommandActive &&", "servantOrder,6.3f,6.2f",
              "TacticModeButtonRect(float scale)",
              "CanChangeTacticMode()", "ToggleServantTactics()",
              "ReadServantTacticsInput(float scale)",
              "DrawServantTacticsButton(float scale)",
              "Input.GetKeyDown(KeyCode.T)", "TouchPhase.Began",
              "Input.GetMouseButtonDown(0)", "huntOrder=!huntOrder;",
              "if(!SaveProgress())", "huntOrder=previous;"):
    assert token in tactics_source, "Tactics integration incomplete: "+token
assert "FindServantTarget(self)" in gameplay
assert "FindServantTarget(boar)" in boar_source
assert "else if(friendly)FollowNecromancer(self,dt);" in gameplay
assert "Actor prey=friendly?FindServantTarget(self)" in gameplay
assert "servantOrder=null;" not in gameplay.split(
    "void UpdateActors(",1)[1].split("void Damage(",1)[0], "Pup AI must not clear manual orders"
assert "ReadServantTacticsInput(scale);" in pulse_source
assert "TacticModeButtonRect(scale).Contains(gui)" in ui
assert "DrawServantTacticsButton(scale);" in ui
assert "ToggleServantTactics);" in hud
assert "huntOrder=saved.huntOrder;" in crypt
assert "huntOrder=save.huntOrder;" in crypt
assert "spellUnlocked=spellUnlocked,huntOrder=huntOrder" in crypt
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]


# 0.9.6 — deterministic follower ranks and safe catch-up.
formation_source=(unity/"Assets/Scripts/NecroServantFormation.cs").read_text()
for token in ("const float FormationFollowStart=.82f;",
              "const float FormationFollowStop=.28f;",
              "const float FormationRecoveryDistance=12f;",
              "const float FormationRecoveryDelay=5.5f;",
              "Vector2 PartyPosition(int slot,float bodyRadius=.30f)",
              "capacity=4+2*row", "facing.normalized",
              "sizeAdjust=Mathf.Max(0f,bodyRadius-.30f)",
              "bool FormationSpotOpen(Actor ally,Vector2 target)",
              "Blocked(target,ally.radius)",
              "ActorAt(ally,target,target)==null",
              "TryFindFormationSpot(Actor ally,int slot,out Vector2 target)",
              "for(int i=0;i<12;i++)",
              "float trailStep=Mathf.Max(.70f,ally.radius*1.85f);",
              "for(int depth=0;depth<12;depth++)",
              "bool FormationRecoverySafe(Actor ally)",
              "if(inSanctuary)return true;",
              "(hostile-at).sqrMagnitude<25f",
              "(hostile-leader).sqrMagnitude<25f",
              "void FollowNecromancer(Actor ally,float dt)",
              "ally.followStragglerSeconds+=dt",
              "FormationRecoverySafe(ally)",
              "ally.body.position=target",
              "MoveToward(ally,target,dt,catchup)"):
    assert token in formation_source, "Formation/recovery missing: "+token
assert "void MoveToward(Actor actor,Vector2 destination,float dt," in gameplay
assert "float followBoost=1f" in gameplay
assert "Mathf.Min(delta.magnitude,travelSpeed*dt)" in gameplay
assert "GraveCommandSpeedMultiplier(actor)" in gameplay, "Buff integration lost"
assert "Vector2 PartyPosition(int slot)" not in gameplay, "Duplicate old formation"
assert "void FollowNecromancer(Actor ally,float dt)" not in gameplay
assert "public float followStragglerSeconds;" in core
assert "followStragglerSeconds=0f;" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "TryFindFormationSpot(ally,index,out at)" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "TryFindFormationSpot(mover,i,out position)" in (unity/"Assets/Scripts/NecroRoster.cs").read_text()
assert "else if(friendly)FollowNecromancer(self,dt);" in gameplay
assert "FindServantTarget(self)" in gameplay
assert "FindServantTarget(boar)" in boar_source
assert "NecroSaveStore" in crypt and "huntOrder=huntOrder" in crypt
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]


# 0.9.7: attack-facing, telegraphs and distinction between allied/hostile hits.
creature_view=(unity/"Assets/Scripts/NecroCreatureView.cs").read_text()
rotboar_source=(unity/"Assets/Scripts/NecroRotboar.cs").read_text()
fx_polish=(unity/"Assets/Scripts/NecroCombatFx.cs").read_text()
for token in ("public void Strike(Vector2 direction)",
              "currentDirection=Direction(direction,currentDirection)",
              "frames.Duration(NecroPose.Attack)", "frameClock=0f",
              "bool bracing=pupAction==2", "bool lunging=pupAction==3",
              "float hurtSquash=impactTimer>0f",
              "haloRenderer.color=bracing ?",
              "if(moving && attackTimer<=0f)",
              "art.localScale=Vector3.one;",
              "if(attackTimer<=0f)"):
    assert token in creature_view, "Creature animation polish missing: "+token
assert "self.view.Strike(target.body.position-self.body.position);" in gameplay
assert "new float[]{35,-35,70,-70,105,-105}" not in gameplay
assert "static readonly float[] AvoidanceAngles=" in gameplay
assert "foreach(float angle in AvoidanceAngles)" in gameplay
for token in ("boarVisualScale=art.transform.localScale",
              "pose==BoarPose.Windup?.94f",
              "pose==BoarPose.Charge?1.06f",
              "boar.ally?6:5", "boar.boarArt.color=boar.flash>0f",
              "art.color=Color.white;"):
    assert token in rotboar_source, "Rotboar combat polish missing: "+token
assert "public Vector3 boarVisualScale;" in core
assert 'type==5?"Rotboar Charge Warning"' in fx_polish
assert 'type==6?"Allied Rotboar Charge Warning"' in fx_polish
assert "bool alliedHit=false" in fx_polish
assert "type==5 || type==6 ? .82f" in fx_polish
assert "type==5 || type==6 ? 10" in fx_polish
assert "victim.ally);" in gameplay
assert "const int Capacity=30;" in fx_polish
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/POLIMENTO_COMBATE_0_9_7.md").is_file()


# 0.9.8: ordinary enemies become boundable corpses. Reanimation and
# compatible saves must work across combat, mobile touch and Windows E.
corpse_source=(unity/"Assets/Scripts/NecroCorpseRitual.cs").read_text()
creature_v=(unity/"Assets/Scripts/NecroCreatureView.cs").read_text()
roster_source=(unity/"Assets/Scripts/NecroRoster.cs").read_text()
boar_source=(unity/"Assets/Scripts/NecroRotboar.cs").read_text()
fx_source=(unity/"Assets/Scripts/NecroCombatFx.cs").read_text()
for token in ("OrdinaryCorpseLifetime=14f", "OrdinaryCorpseLimit=16",
              "void SpawnRecoverableCorpse(Actor victim)",
              "victim.view.FallToCorpse(OrdinaryCorpseLifetime);",
              "current.corpses.Add(new Corpse{", "Corpse NearbyCorpse()",
              "Visible(player.body.position,entry.body.position)",
              "void AnimateReanimation(Actor servant,Vector2 at)",
              "servant.reanimationUntil=Time.time+SoulReanimationDuration",
              "servant.view.Reanimate()", "StartCoroutine(ReanimateRotboar(servant))",
              "IEnumerator ReanimateRotboar(Actor boar)"):
    assert token in corpse_source, "Corpse ritual missing: "+token
for token in ("public void FallToCorpse(float remaining,bool restored=false)",
              "public void SetCorpseRemaining(float remaining)",
              "public void Reanimate()", "void AnimateOrdinaryCorpse(float dt)",
              "void ApplyReanimationVisual(float dt)",
              "corpseAutoCleanup", "if(corpseMode)",
              "artRenderer.sprite=frames.Frame(NecroPose.Hurt,",
              "ApplyReanimationVisual(dt);"):
    assert token in creature_v, "Death/awakening animation missing: "+token
assert "else SpawnRecoverableCorpse(victim);" in gameplay
assert "else if(victim.ally)" in gameplay
assert "TickCorpses(dt);" in core
assert "if(!inSanctuary)TickCorpses(dt);" in core
assert "body.view.SetCorpseRemaining(body.time);" in gameplay
assert "if(friendly && Time.time<self.reanimationUntil)continue;" in gameplay
assert "public float reanimationUntil;" in core
assert "public NecroCreatureView view;" in core
assert "return NearbyCorpse()!=null || NearbyFallenPup()!=null" in gameplay
assert "if(corpse!=null && cd<=bd && cd<=pd)" in gameplay
assert "Corpse nearest=NearbyCorpse();" in gameplay
assert "Actor servant=nearest.kind==\"rotboar\"" in gameplay
assert "AnimateReanimation(servant,at);" in gameplay
assert "Input.GetKeyDown(KeyCode.E)" in ui
assert "CaptureDowned(downed);" in ui
assert "HarvestDowned(downed);" in ui
assert 'CanBindNearby())DrawAction(BindRect(scale),true,"ERGUER",true)' not in ui
assert "foreach(Corpse previous in current.corpses)" in crypt
assert "current.corpses.Clear();" in crypt
assert "shell.view.FallToCorpse(remaining,true);" in crypt
assert "body.time=remaining" in crypt or "time=remaining" in crypt
assert "state.corpses=corpses.ToArray();" in crypt
assert "if(record.kind==\"necromante\")" not in roster_source
assert "record.kind==\"necromante\")continue;" in roster_source
assert "CommandUsed()+CommandCost(a.kind)<=CommandLimit" in roster_source
assert "NecroArt.Get(a.kind)" in roster_source
assert "SaveCompanions();" in gameplay.split("void Raise()",1)[1].split("void Dismiss()",1)[0]
assert "SaveCompanions();" in gameplay.split("void Dismiss()",1)[1].split("void SpawnDrop(",1)[0]
assert "float remaining=14f;" in boar_source
assert "Mathf.Clamp01(remaining/.72f)" in boar_source
assert 'type==7?"Departing Soul"' in fx_source
assert "type==7 ? new Vector2(0f,.65f)" in fx_source
assert "type==0 || type==7 || type==8 || type==10?" in fx_source
assert "const int Capacity=30;" in fx_source
assert "NecroCombatFx.Emit(victim.body.position,current.root.transform,7," in gameplay
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/POLIMENTO_MORTE_REANIMACAO_0_9_8.md").is_file()


# 0.9.9: hero visual states and swept free-projectile impacts.
hero_polish=(unity/"Assets/Scripts/NecroHeroCombatPolish.cs").read_text()
hero_v=(unity/"Assets/Scripts/NecroCharacterAnimation.cs").read_text()
vfx_source=(unity/"Assets/Scripts/NecroCombatFx.cs").read_text()
for token in ("const float StrikeVisualDuration=.53f;",
              "const float RaiseVisualDuration=.92f;",
              "heroineStrikePulse=Mathf.Max(0f,heroineStrikePulse-dt);",
              "heroineRaisePulse=Mathf.Max(0f,heroineRaisePulse-dt);",
              "else if(heroineRaisePulse>0f)",
              "else if(heroineStrikePulse>0f)",
              "visualLean=.095f*Mathf.Sin(phase*Mathf.PI);",
              "witchArtRenderer.transform.localPosition=pose;",
              "heroineCastPulse=Mathf.Max(0,heroineCastPulse-dt);",
              "if(releasePending && elapsed>=CastRelease)"):
    assert token in hero_v, "Missing heroine attack/ritual pose: "+token
for token in ("static bool SpectralSegmentHit(Vector2 from,Vector2 to,",
              "Mathf.Clamp01(Vector2.Dot(center-from,delta)/lengthSquared)",
              "void UpdateSpectralProjectileVisual(Projectile bolt)",
              "Mathf.Sin(bolt.age*22f)",
              "bolt.renderer.color=tint;",
              "Mathf.Clamp01(bolt.time/.16f)"):
    assert token in hero_polish, "Missing spectral projectile polish: "+token
assert "public float age;" in core and "public SpriteRenderer renderer;" in core
assert "UpdateSpectralProjectileVisual(bolt);" in gameplay
assert "SpectralSegmentHit(before,after,enemy.body.position," in gameplay
assert "Visible(before,after)" in gameplay
assert "heroineStrikePulse=.53f;" in gameplay
assert "Vector2 strikeDelta=target.body.position-player.body.position;" in gameplay
assert "facing=castAim;" in gameplay
assert "NecroCombatFx.Emit(hands,current.root.transform,8);" in gameplay
assert "NecroCombatFx.Emit(after,current.root.transform,9);" in gameplay
assert "NecroCombatFx.Emit(target.body.position,current.root.transform,10);" in gameplay
assert "heroineRaisePulse=0f;" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "heroineStrikePulse=0f;" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
for token in ('type==8?"Spectral Muzzle"', 'type==9?"Spectral Impact"',
              'type==10?"Unarmed Strike"', "type==8 ? .25f",
              "type==9 ? .33f", "type==10 ? .25f",
              "type==8 ? 21 :", "type==9 || type==10 ? 18 :",
              "const int Capacity=30;"):
    assert token in vfx_source, "Combat FX regression: "+token
# No changes to combat economy or compatibility:
assert "heroineCastPulse=CastDuration;" in gameplay
assert "castCooldown=Time.time+(SkillUnlocked(5) ? .60f : .72f);" in gameplay
assert "projectiles.Add(new Projectile{" in gameplay
assert "direction=direction,time=.9f" in gameplay
assert "bolt.direction*(dt*7f)" in gameplay
assert "SkillSpectralDamage(" in gameplay
assert "mana" not in cast_body
assert "SpellUnlocked" not in hero_polish
assert "public const int Version=1;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/POLIMENTO_HERO_COMBATE_0_9_9.md").is_file()


# 0.9.10 — integrated review: persistence, companion command budget and
# correct boar factories on every snapshot restore path.
roster_audit=(unity/"Assets/Scripts/NecroRoster.cs").read_text()
crypt_audit=(unity/"Assets/Scripts/NecroCrypt.cs").read_text()
assert "InvokeRepeating(nameof(PeriodicAutosave),15f,15f);" in core
assert "private void PeriodicAutosave()" in crypt_audit
assert "Autosave(15f);" in crypt_audit
autosave_audit=crypt_audit.split("private void Autosave(float dt)",1)[1].split(
    "void RecoverAfterDeath()",1)[0]
assert "if(autoSaveClock<15f)return;" in autosave_audit
assert "if(!inSanctuary)SaveCompanions();" in autosave_audit
assert "SaveProgress();" in autosave_audit
assert "inventoryOpen || wardrobeOpen" in autosave_audit
assert "Autosave(" not in core.split("void Update()",1)[1].split(
    "bool NearSeal",1)[0], "Avoid per-frame autosaving"
assert "void OnApplicationPause(bool paused)" in roster_audit
assert "if(paused)SaveOnSuspendOrExit();" in roster_audit
assert "void OnApplicationQuit()" in roster_audit
assert "void SaveOnSuspendOrExit()" in roster_audit
suspend_body=roster_audit.split("void SaveOnSuspendOrExit()",1)[1].split(
    "void OnApplicationPause",1)[0]
assert "SaveCompanions();" in suspend_body
assert "SaveProgress()" in suspend_body
transfer_body=roster_audit.split("void TransferSelected()",1)[1].split(
    "static string CompanionName",1)[0]
assert "CommandUsed()+CommandCost(a.kind)>CommandLimit" in transfer_body
assert transfer_body.index("CommandUsed()+CommandCost(a.kind)>CommandLimit") < (
    transfer_body.index("resting.Remove(a);"))
assert "SaveCompanions();" in transfer_body and "SaveProgress();" in transfer_body
assert "SaveCompanions();SaveProgress();" in roster_audit, "Release must update both stores"
assert 'Actor CreateRestoredActor(string kind,Vector2 at,bool ally,' in crypt_audit
assert 'kind=="rotboar"?CreateRotboar(at,parent,ally)' in crypt_audit
assert "CreateRestoredActor(record.kind," in crypt_audit
assert "CreateRestoredActor(item.kind," in crypt_audit
assert crypt_audit.count("CreateRestoredActor(record.kind,")>=2
assert "if(!inSanctuary)SaveCompanions();" in crypt_audit
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/REVISAO_INTEGRADA_COMBATE_0_9_10.md").is_file()


# 0.9.11: fatal damage, transient hit protection and a safe recovery loop.
survival_source=(unity/"Assets/Scripts/NecroHeroSurvival.cs").read_text()
for token in ("HeroHitInvulnerabilitySeconds=1.10f",
              "HeroArrivalGraceSeconds=1.10f",
              "float heroDamageImmuneUntil;",
              "void ApplyHeroDamage(float damage)",
              "player.hp=Mathf.Max(0f,player.hp-amount);",
              "Time.time<heroDamageImmuneUntil",
              "heroDamageImmuneUntil=Time.time+HeroHitInvulnerabilitySeconds",
              "if(player.hp<=0f)",
              "BeginHeroDefeat();",
              "void BeginHeroDefeat()",
              "gameOver=true;",
              "manualQueued=false;",
              "releasePending=false;",
              "castTarget=null;",
              "void DrawHeroDefeatPanel(float width,float height)",
              "RETORNAR AO REFUGIO",
              "GUI.Button(action,GUIContent.none,GUIStyle.none)",
              "KeyCode.Return", "KeyCode.KeypadEnter", "KeyCode.Space",
              "RecoverAfterDeath();"):
    assert token in survival_source, "Hero survival missing: "+token
assert "if(victim==player)" in gameplay
assert "ApplyHeroDamage(amount);" in gameplay
assert "if(gameOver)return;" in gameplay.split("void UpdateActors(",1)[1].split(
    "Actor ClosestPartyMember(",1)[0]
assert "heroDamageImmuneUntil=Time.time+HeroArrivalGraceSeconds;" in core
assert "if(gameOver)return;" in core.split("void Update()",1)[1].split(
    "bool NearSeal()",1)[0]
assert "if(player==null || level==null || gameOver || inventoryOpen)return;" in core
assert "DrawHeroDefeatPanel(Screen.width/scale,Screen.height/scale);" in ui
assert ui.index("if(gameOver)\n            {\n                DrawHeroDefeatPanel") < (
    ui.index("if(DrawRoster(")), "Death modal must take precedence over roster"
assert "Time.time<heroDamageImmuneUntil" in animation
death_recovery=crypt.split("void RecoverAfterDeath()",1)[1].split(
    "\n        }\n",1)[0]
for token in ("if(!gameOver || player==null", "Mathf.Min(2,Mathf.Max(0,shards))",
              "shards-=lost;", "player.hp=Mathf.Max(1f,player.maxHp*.65f);",
              "mana=maxMana;", "gameOver=false;", "EnterSanctuary();",
              "SaveCompanions();", "SaveProgress()"):
    assert token in death_recovery, "Safe death recovery missing: "+token
assert "LoadStage(" not in death_recovery, "Death must return to the refuge"
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "public const int Version=1;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/VIDA_DERROTA_RECUPERACAO_0_9_11.md").is_file()


# 0.9.12: difficulty pacing must be gradual, deterministic and save-safe.
balance_source=(unity/"Assets/Scripts/NecroDifficultyBalance.cs").read_text()
for token in ("float IncomingHeroDamageMultiplier()",
              "stage==0 ? .82f", "stage==1 ? 1f", "stage==2 ? 1.08f",
              "1.14f", "float EnemyStrikeCooldown(Actor enemy)",
              "if(IsBoss(enemy.kind))return 1.55f;",
              "stage==0 ? 1.40f", "stage==1 ? 1.20f",
              "stage==2 ? 1.15f", "1.11f",
              "float HostilePupDetectionDistance()",
              "stage==0 ? 3.65f : 4.1f",
              "float PupWindupDuration(Actor pup)",
              "float PupRecoveryCooldown(Actor pup)",
              "float RotboarWindupDuration(Actor boar)",
              "stage==0 ? 1.00f : .80f",
              "float RespawnIntervalForStage()",
              "stage==0 ? 19f", "stage==1 ? 15f",
              "stage==2 ? 12f : 11f",
              "int RespawnPopulationLimitForStage()",
              "stage==0 ? 3", "stage==1 ? 4",
              "stage==2 ? 5 : 6",
              "float SpawnMinDistanceForStage()",
              "float SpawnMaxDistanceForStage()",
              "float ServantThreatDistanceWeight()",
              "stage==0 ? .62f",
              "float HealFromGatheredResource()",
              "Mathf.Max(9f,player.maxHp*.12f)",
              "Mathf.Min(missing,amount)"):
    assert token in balance_source, "Missing region balance invariant: "+token
assert "EnemyStrikeCooldown(self)" in gameplay
assert "float companionWeight=ServantThreatDistanceWeight();" in gameplay
assert "distance=(ally.body.position-enemy.body.position).sqrMagnitude*" in gameplay
assert "if(ally==null || ally.hp<=0f || ally.body==null)continue;" in gameplay
assert "PupWindupDuration(pup)" in gameplay
assert "HostilePupDetectionDistance()" in gameplay
assert "pup.cooldown=PupRecoveryCooldown(pup);" in gameplay
assert "RotboarWindupDuration(boar)" in boar_source
assert "HealFromGatheredResource()" in gameplay
assert "player.hp=Mathf.Min(player.maxHp,player.hp+healed);" in gameplay
assert "Mathf.Min(12,Mathf.Max(0,maxMana-mana))" in gameplay
assert "damage*IncomingHeroDamageMultiplier()" in survival_source
assert "if(before>player.maxHp*.25f" in survival_source
assert "heroDamageImmuneUntil=Time.time+HeroHitInvulnerabilitySeconds" in survival_source
assert "current.respawnTimer=RespawnIntervalForStage();" in respawn
assert "alive>=RespawnPopulationLimitForStage()" in respawn
assert "SpawnMinDistanceForStage()" in respawn and "SpawnMaxDistanceForStage()" in respawn
assert "next.respawnTimer=RespawnIntervalForStage();" in core
assert "current.respawnIndex++;" in respawn
assert "current.enemies.Add(spawned);" in respawn
assert "return stage==0 ? .82f" in balance_source, "First stage must be most accessible"
assert "return stage==0 ? 19f" in balance_source, "First stage needs more room"
assert "0.9.22" in builder and "0.9.22" in win_builder
assert "public const int Version=1;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/BALANCEAMENTO_DIFICULDADE_0_9_12.md").is_file()


# 0.9.13: mantle protection and finite sanctuary healing remain save-safe.
defense_source=(unity/"Assets/Scripts/NecroDefenseRecovery.cs").read_text()
for token in ("public static class NecroDefenseRules",
              "Mathf.Clamp(tier,0,3)*.06f",
              "Mathf.Max(0f,incoming)*(1f-MantleReduction(tier))",
              "Mathf.Min(missing,Mathf.Max(18f,maximumHp*.30f))",
              "float MitigateHeroDamage(float damage)",
              "float RecoverFountainHealth()",
              "if(!inSanctuary || gameOver || player==null",
              "player.hp=Mathf.Min(player.maxHp,player.hp+healed);"):
    assert token in defense_source, "Defense/recovery missing: "+token
assert "float scaledDamage=damage*IncomingHeroDamageMultiplier();" in survival_source
assert "Mathf.Max(1f,MitigateHeroDamage(scaledDamage));" in survival_source
assert "float healed=RecoverFountainHealth();" in interactions
assert interactions.index("StationCooldownRemaining(station) > 0f") < (
    interactions.index("float healed=RecoverFountainHealth();"))
assert "SaveProgress();" in interactions.split(
    "float healed=RecoverFountainHealth();",1)[1].split(
    "UpdateSanctuaryInteractionVisual();",1)[0]
assert "Mathf.RoundToInt(MantleDamageReduction()*100f)" in wardrobe_source
assert "Mathf.RoundToInt(MantleDamageReduction()*100f)" in (
    unity/"Assets/Scripts/NecroPolishedHUD.cs").read_text()
assert "NecroDefenseEditorCheck.Verify();" in builder
assert "NecroDefenseEditorCheck.Verify();" in win_builder
editor_check=(unity/"Assets/Editor/NecroDefenseEditorCheck.cs").read_text()
for token in ("MantleReduction(0)", "MantleReduction(3)",
              "MantleReduction(99)", "DamageAfterMantle(100f,3)",
              "FountainHealing(100f,100f)", "FountainHealing(60f,100f)",
              "FountainHealing(95f,100f)", "FountainHealing(20f,40f)"):
    assert token in editor_check, "Missing Unity C# check: "+token
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/DEFESA_E_RECUPERACAO_0_9_13.md").is_file()


# 0.9.14: common hostile target scoring, safe focus transitions, protective
# servant targeting and lightweight hit feedback, without new save fields.
target_rules=(unity/"Assets/Scripts/NecroTargetRules.cs").read_text()
target_check=(unity/"Assets/Editor/NecroTargetEditorCheck.cs").read_text()
for token in ("float ThreatScore(float distanceSquared,",
              "bool KeepFocus(float previousScore,float bestScore)",
              "previousScore<=bestScore*1.20f",
              "float DefenseScore(float servantDistance,",
              "heroDistance<=1.9f?score*.72f:score"):
    assert token in target_rules, "Missing combat selection rule: "+token
for token in ("ThreatScore(9f,false,.62f)",
              "ThreatScore(9f,true,.62f)",
              "KeepFocus(10f,9f)", "KeepFocus(12f,9f)",
              "DefenseScore(3f,4f)", "DefenseScore(3f,1f)"):
    assert token in target_check, "Missing C# target regression: "+token
assert "public Actor hostileFocus;" in core
assert "Actor target=friendly?FindServantTarget(self):" in gameplay
assert "ClosestPartyMember(self);" in gameplay
assert "enemy.hostileFocus=nearest;" in gameplay
assert "float separation=((Vector2)previous.body.position-source).sqrMagnitude;" in gameplay
assert "(previous.body.position-source).sqrMagnitude" not in gameplay
assert "previous==player || servants.Contains(previous)" in gameplay
assert "if(unweighted>rangeSquared)continue;" in gameplay
assert "if(self.view!=null)" in gameplay.split(
    "void UpdateActors(",1)[1].split(
    "// Shared target selection",1)[0]
assert "if(target==null || target.hp<=0f || target.body==null)" in boar_source
assert "NecroTargetRules.DefenseScore(distance," in tactics_source
assert "float score=huntOrder?distance:" in tactics_source
assert "!inSanctuary && !huntOrder && row==0" in formation_source
assert "heroHitFeedbackUntil=Time.time+1.2f;" in survival_source
assert "Mathf.Max(1f,scaledDamage)-amount" in survival_source
polished=(unity/"Assets/Scripts/NecroPolishedHUD.cs").read_text()
assert "Time.time<heroHitFeedbackUntil" in polished
assert "Mathf.CeilToInt(heroLastGuarded)" in polished
assert "NecroTargetEditorCheck.Verify();" in builder
assert "NecroTargetEditorCheck.Verify();" in win_builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/COMBATE_TATICO_0_9_14.md").is_file()


# 0.9.15: restore practical resource gathering while preserving collected state.
resource_code=core.split("void PopulateStageResources(",1)[1].split(
    "void LoadStage(int index)",1)[0]
assert "snapshot.visited && snapshot.resourcesInitialized" in resource_code
assert "if(snapshot.resources==null)return;" in resource_code
assert "foreach(NecroResourceRecord saved in snapshot.resources)" in resource_code
assert "foreach(ResourceSpawn resource in level.resources)" in resource_code
assert "NecroLevels.At(level,resource.x,resource.y)" in resource_code
load_body=core.split("void LoadStage(int index)",1)[1].split(
    "void Advance()",1)[0]
assert "PopulateStageResources(lastSnapshot);" in load_body
assert "previousStageSnapshots[stage]" in load_body
assert "CollectNearby();" in core.split("void Update()",1)[1].split(
    "bool NearSeal()",1)[0]
assert "public bool resourcesInitialized;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
crypt_resources=(unity/"Assets/Scripts/NecroCrypt.cs").read_text()
assert "state.resourcesInitialized=true;" in crypt_resources
assert "PopulateStageResources(saved);" in crypt_resources
assert "current.resources.Clear();" in crypt_resources
assert crypt_resources.index("current.resources.Clear();") < crypt_resources.index(
    "PopulateStageResources(saved);")
pickup=gameplay.split("void CollectNearby()",1)[1]
assert "shards+=2;gathered++;" in pickup
assert "HealFromGatheredResource()" in pickup
assert "if(collected)SaveProgress();" in pickup
assert "current.resources.RemoveAt(i);" in pickup
assert pickup.count("SaveProgress();")==1, "Batch saves once for collected items"
assert len(levels[0]["resources"])==5, "First cave must support first mantle upgrade"
assert all(r["type"]=="cristal" for r in levels[0]["resources"])
assert sum(2 for _ in levels[0]["resources"])>=10, "Starter resources fund one mantle rank"
progress_source=(unity/"Assets/Scripts/NecroProgressHud.cs").read_text()
assert '"FRAGMENTOS "+shards+" | RECURSOS "+' in progress_source
assert "Input.GetKeyDown(KeyCode.Space)" in ui
assert '"ESPACO",hudAction' in ui
save_editor=(unity/"Assets/Editor/NecroSaveEditorCheck.cs").read_text()
assert "resourcesInitialized=true" in save_editor
assert "Legacy resource migration marker regression." in save_editor
assert r'\"visited\"' in save_editor, "Legacy C# JSON string must escape quotes once"
assert r'\\"visited\\"' not in save_editor, "Double-escaped JSON literal breaks Unity compilation"
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/RECURSOS_COLETA_0_9_15.md").is_file()


# 0.9.16: resource discovery, bounded shimmer and nonintrusive loot feedback.
exploration_source=(unity/"Assets/Scripts/NecroExplorationFeedback.cs").read_text()
exploration_check=(unity/"Assets/Editor/NecroExplorationEditorCheck.cs").read_text()
for token in ("DiscoveryRadius=7f", "bool CanReveal(float distanceSquared)",
              "float MarkerScale(float seconds,float offset)",
              "float ActualHealing(float current,float maximum,",
              "void RecordResourceReward(float healing,int manaGain)",
              "void UpdateResourceExploration()",
              "void DrawResourceRewardToast(float width,float y)",
              "nextResourceHintRefresh=Time.time+ResourceHintRefreshSeconds",
              "DisplayResourceType(nearest.type)",
              "if(Time.time>=resourceToastUntil)",
              "resourceToastUntil=Time.time+ResourceToastDuration",
              "if(separation>best || !Visible(origin,at))continue;",
              "node.artwork.color=Color.white;"):
    assert token in exploration_source, "Exploration feedback missing: "+token
for token in ("CanReveal(49f)", "CanReveal(49.01f)",
              "MarkerScale(Mathf.PI/6f,0f)",
              "ActualHealing(97f,100f,12f)",
              "ActualHealing(100f,100f,12f)"):
    assert token in exploration_check, "Unity C# exploration test missing: "+token
for token in ("public SpriteRenderer artwork;",
              "public Vector3 defaultScale;",
              "public Color shimmerTint;",
              "public float shimmerOffset;"):
    assert token in core, "Resource visual cache missing: "+token
assert "UpdateResourceExploration();" in core.split("void Update()",1)[1].split(
    "bool NearSeal()",1)[0]
assert "RecordResourceReward(healed,manaGained);" in pickup
assert "NecroExplorationRules.ActualHealing(" in pickup
assert "NecroCombatFx.Emit((Vector2)res.body.position," in pickup
assert '"Recurso "+res.type+": +2 fragmentos' not in pickup
assert "if(collected)SaveProgress();" in pickup
assert "DrawResourceRewardToast(width,toastY);" in progress_source
assert "resourceHint,wardrobeNoteStyle" in progress_source
assert "if(!inSanctuary && Time.time<resourceToastUntil)" in progress_source
assert "NecroExplorationEditorCheck.Verify();" in builder
assert "NecroExplorationEditorCheck.Verify();" in win_builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert (root/"docs/EXPLORACAO_RECOMPENSAS_0_9_16.md").is_file()


# 0.9.17: hostile territory, sight memory and safer offscreen respawns.
encounter_rules=(unity/"Assets/Scripts/NecroEncounterRules.cs").read_text()
encounter_check=(unity/"Assets/Editor/NecroEncounterEditorCheck.cs").read_text()
for token in ("TerritoryRadius=8f", "ReturnStopRadius=.48f",
              "SightMemorySeconds=.85f", "PickupSafeRadius=1.85f",
              "bool OutsideTerritory(Vector2 current,Vector2 origin)",
              "bool NeedsReturn(Vector2 current,Vector2 origin)",
              "bool OnScreen(Vector3 viewport)",
              "viewport.z>0f", "bool NearPickup(Vector2 spawn,Vector2 pickup)"):
    assert token in encounter_rules, "Missing prolonged encounter rule: "+token
for token in ("OutsideTerritory(", "NeedsReturn(",
              "OnScreen(", "NearPickup(", "SightMemorySeconds>0f"):
    assert token in encounter_check, "Missing Unity editor combat regression: "+token
assert "public Vector2 lairPosition;" in core
assert "public float targetMemoryUntil;" in core
assert "lairPosition=at," in core and "lairPosition=at," in boar_source
assert "NecroEncounterRules.OutsideTerritory(" in gameplay
assert "enemy.hostileFocus=null;" in gameplay
assert "if(heroDistance<=rangeSquared &&" in gameplay
assert "Visible(source,player.body.position)" in gameplay
assert "if(!Visible(source,ally.body.position))continue;" in gameplay
assert "Time.time<enemy.targetMemoryUntil" in gameplay
assert "void ReturnToLair(Actor enemy,float dt)" in gameplay
assert "MoveToward(enemy,enemy.lairPosition,dt*.72f);" in gameplay
assert "else ReturnToLair(self,dt);" in gameplay
assert "else ReturnToLair(boar,dt);" in boar_source
assert "cam.WorldToViewportPoint(" in respawn
assert "NecroEncounterRules.OnScreen(" in respawn
assert "NecroEncounterRules.NearPickup(" in respawn
assert "if(nearPickup)continue;" in respawn
assert "int nearbyHostiles=0;" in exploration_source
assert "if(nearbyHostiles>0)" in exploration_source
assert '"ALERTA: INIMIGO PROXIMO"' in exploration_source
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert "NecroEncounterEditorCheck.Verify();" in builder
assert "NecroEncounterEditorCheck.Verify();" in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]
assert (root/"docs/ENCONTROS_E_EXPLORACAO_0_9_17.md").is_file()


# 0.9.22: access the actual four regions through the active cave GUI.
region=(unity/"Assets/Scripts/NecroRegionTravel.cs").read_text()
region_editor=(unity/"Assets/Editor/NecroRegionEditorCheck.cs").read_text()
for token in ("class NecroRegionRules", "bool CanClaim(", "bool NeedsGuardian(",
              "bool CanAdvance(", "void UpdateRegionProgress()",
              "bool RegionTravelAvailable()", "Rect RegionTravelRect(float scale)",
              "void DrawRegionNavigation(float scale)",
              "void ActivateRegionSeal()", "NearSeal()", "NearEntrance()",
              '"AVANCAR: "+levels[stage+1].title',
              '"VOLTAR: "+levels[stage-1].title'):
    assert token in region, "Region traversal feature missing: "+token
for token in ("CanClaim(false,false,false,true)",
              "CanClaim(false,true,false,true)",
              "CanClaim(false,true,true,false)",
              "NeedsGuardian(true,false,true)",
              "CanAdvance(true,true,true)"):
    assert token in region_editor, "Missing native C# regression: "+token
assert 'Visual("Selo de passagem",sealAt,' in core
assert "next.seal=sealArt.transform;" in core
assert "PopulateStageResources(lastSnapshot);" in core
assert "next.claimed=lastSnapshot.claimed;" in core
assert "next.bossDefeated=lastSnapshot.bossDefeated;" in core
assert "NecroRegionRules.NeedsGuardian(" in core
assert "UpdateRegionProgress();" in core
assert "ActivateRegionSeal();" in core
assert "current.bossDefeated=true;" in gameplay
assert "ActivateRegionSeal();" in crypt_resources
assert "RegionTravelAvailable() && RegionTravelRect(scale).Contains(gui)" in ui
assert "DrawRegionNavigation(scale);" in ui
assert "NecroRegionEditorCheck.Verify();" in builder
assert "NecroRegionEditorCheck.Verify();" in win_builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert (root/"docs/ACESSO_REGIOES_0_9_18.md").is_file()


# 0.9.22: remove UI skills, prayer/wardrobe interactions, keep saves safe.
scope=(unity/"Assets/Scripts/NecroInterfaceScope.cs").read_text()
assert "public const bool SanctuaryProgressionEnabled=false;" in scope
assert len(station_json["stations"])==1
assert station_json["stations"][0]["kind"]=="fountain_buff"
assert 'station.kind != "fountain_buff"' in interactions
assert 'if(station.kind != "fountain_buff")return;' in interactions
for source in (wardrobe_source,meditation_source,skill_source):
    assert "if(!NecroInterfaceScope.SanctuaryProgressionEnabled)return;" in source
for removed in ("DrawSoulPulseButton(scale);", "DrawGraveCommandButton(scale);",
                "SoulPulseButtonRect(scale).Contains(gui)",
                "GraveCommandButtonRect(scale).Contains(gui)"):
    assert removed not in ui
assert "DrawServantTacticsButton(scale);" in ui
assert "DrawRegionNavigation(scale);" in ui
assert "DrawAction(button,false," in ui
assert "DrawSanctuaryInteractionUi(scale);" in ui
assert "ReadActiveSkillInput(hudScale);" in core
assert "if(Input.GetKeyDown(KeyCode.Q))TryCastSoulPulse();" in pulse_source
assert "if(Input.GetKeyDown(KeyCode.R))TryCastGraveCommand();" in command_source
assert "TouchPhase.Began" not in pulse_source.split(
    "void ReadActiveSkillInput(float scale)",1)[1].split(
    "void DrawSoulPulseButton(float scale)",1)[0]
assert "TouchPhase.Began" not in command_source.split(
    "void ReadGraveCommandInput(float scale)",1)[1].split(
    "void DrawGraveCommandButton(float scale)",1)[0]
assert "MeditationPointsAvailable()" not in progress_source
assert "AvailableSkillPoints()" not in progress_source
assert "NIVEL " in meditation_source and "ALCANCADO!" in meditation_source
assert "NIVEL " in pulse_source and "ALCANCADO!" in pulse_source
assert "RestoreMeditationAllocations(saved);" in crypt
assert "RestoreSkillTree(saved);" in crypt
assert "public const int Version=1;" in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert (root/"docs/INTERFACE_SIMPLIFICADA_0_9_19.md").is_file()


# 0.9.22: extract or capture from one downed creature.
downed=(unity/"Assets/Scripts/NecroDownedChoices.cs").read_text()
downed_editor=(unity/"Assets/Editor/NecroDownedEditorCheck.cs").read_text()
for token in ("class NecroDownedRules", "OrdinaryReach=1.30f",
              "PupReach=1.35f", "RotboarReach=1.80f",
              "DownedDuration=14f", "FragmentsFor(string kind)",
              "bool CanCapture(int used,int cost,int maximum)",
              "int CaptureMana(bool mage,bool ordinary,int cost)",
              "struct DownedChoice", "DownedChoice FindNearbyDowned()",
              "bool ConsumeDowned(DownedChoice choice)",
              "void HarvestDowned(DownedChoice choice)",
              "void CaptureDowned(DownedChoice choice)",
              "void DrawDownedChoices(float scale)",
              "Rect DownedHarvestRect(float scale)",
              "Rect DownedCaptureRect(float scale)",
              "if(!ConsumeDowned(choice))return;",
              "current.corpses.Remove(choice.corpse)",
              "current.fallenPups.Remove(choice.pup)",
              "current.fallenBoars.Remove(choice.boar)",
              "SaveCompanions();", "SaveProgress();"):
    assert token in downed, "Missing downed behavior: "+token
for token in ('FragmentsFor("grave_pup")', 'FragmentsFor("rotboar")',
              'FragmentsFor("grave_lord")', "CanCapture(11,3,12)",
              "CaptureMana(false,true,18)"):
    assert token in downed_editor, "C# rule regression: "+token
assert "const float FallenHold = 14f;" in creature_v
assert "public float FallenRemaining =>" in creature_v
assert "boar.bindUntil=Time.time+14f;" in boar_source
assert "float remaining=14f;" in boar_source
assert "DrawDownedChoices(scale);" in ui
assert "if(downed.Valid && harvest.Contains(point))" in ui
assert "if(downed.Valid && capture.Contains(point))" in ui
assert "if(downed.Valid && Input.GetKeyDown(KeyCode.F))" in ui
assert "if(downed.Valid && Input.GetKeyDown(KeyCode.E))" in ui
assert "DownedHarvestRect(scale).Contains(gui)" in ui
assert "DownedCaptureRect(scale).Contains(gui)" in ui
assert "BindRect(scale).Contains(gui)" not in ui
assert "NecroDownedEditorCheck.Verify();" in builder
assert "NecroDownedEditorCheck.Verify();" in win_builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert (root/"docs/ESCOLHAS_CRIATURA_CAIDA_0_9_20.md").is_file()


selection=(unity/"Assets/Scripts/NecroCharacterSelection.cs").read_text()
# 0.9.22: in-game avatar selection, correct portrait crop and NPOT importer.
switch_import=(unity/"Assets/Editor/NecroSombriaTextureImport.cs").read_text()
switch_check=(unity/"Assets/Editor/NecroCharacterSwitchEditorCheck.cs").read_text()
for token in ("assetPath.StartsWith(",
              '"Assets/Resources/Characters/NecromanteSombria/"',
              "importer.npotScale=TextureImporterNPOTScale.None",
              "importer.maxTextureSize=2048;",
              "importer.mipmapEnabled=false;",
              "importer.textureCompression=TextureImporterCompression.Uncompressed;"):
    assert token in switch_import, "Sombria import regression: "+token
for token in ("NecroCharacterCatalog.Load()", "roster.Length==4",
              "entry.id==\"necromante_sombria\"",
              "atlas.width%6==0 && atlas.height%8==0",
              "atlas.width*4==atlas.height*3"):
    assert token in switch_check, "Missing native Unity character check: "+token
for token in ("bool inGameCharacterMenu;", "int activeCharacterIndex;",
              "void OpenInGameCharacterMenu()", "void CancelInGameCharacterMenu()",
              "if(inGameCharacterMenu && Input.GetKeyDown(KeyCode.Escape))",
              "if(inGameCharacterMenu)", "ResetTravelCombat();",
              "ResetHeroAnimationForCharacterSwap();",
              "InitializeHeroAnimation();",
              "SetSanctuaryHeroAppearance(inSanctuary);",
              "SaveProgress()", '"USAR PERSONAGEM"', '"CANCELAR"',
              '"EM USO"', "new Rect(.15f,.35f,.70f,.54f)"):
    assert token in selection, "Runtime character switch missing: "+token
assert "BeginSelectedCharacterWorld();" in selection
assert "if(selectedCharacterIndex!=activeCharacterIndex)" in selection
assert "selectedCharacterIndex=activeCharacterIndex;" in selection
assert "oldAvatar.SetActive(false);" in hero_anim
assert "System.Array.Clear(heroSprites,0,heroSprites.Length);" in hero_anim
assert "sixFrameWalkSprites=null;" in hero_anim
assert 'GUI.Button(CharacterMenuRect(scale),"PERSONAGEM",buttonStyle)' in ui
assert "CharacterMenuRect(scale).Contains(gui)" in ui
assert "OpenInGameCharacterMenu();" in ui
assert "NecroCharacterSwitchEditorCheck.Verify();" in builder
assert "NecroCharacterSwitchEditorCheck.Verify();" in win_builder
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert 'public const int Version=1;' in (unity/"Assets/Scripts/NecroSaveData.cs").read_text()
assert (root/"docs/MENU_PERSONAGENS_0_9_22.md").is_file()

print(f"Unity source verified: {len(levels)} regions, 0.7.5 high resolution idle/walk heroine, eight-direction animation, manual attacks, respawn, army, portrait PNG "
      f"({width}x{height}), HUD, animator, save and ARM64. "
      "Unity Editor build and device validation remain necessary.")


# Character selection must gate world construction, input and save callbacks.
selection=(unity/"Assets/Scripts/NecroCharacterSelection.cs").read_text()
characters=json.loads((unity/"Assets/Resources/Data/characters.json").read_text())["characters"]
assert len({c["id"] for c in characters})==len(characters)
assert [c["id"] for c in characters if c["available"]]==[
    "necromante","arlequina","necromante_fisica","necromante_sombria"
]
for c in characters:
    if not c["available"]: continue
    if c["id"]=="necromante_sombria":
        assert c["meleeOnly"] and c["walkSixResource"]=="Characters/NecromanteSombria/walk6"
        assert c["portraitResource"]=="Characters/NecromanteSombria/portrait"
        walking=(unity/"Assets/Resources"/(c["walkSixResource"]+".png")).read_bytes()
        portrait=(unity/"Assets/Resources"/(c["portraitResource"]+".jpg")).read_bytes()
        assert walking[:8]==b"\x89PNG\r\n\x1a\n"
        assert struct.unpack(">II",walking[16:24])==(768,1024)
        assert portrait[:2]==b"\xff\xd8" and portrait[-2:]==b"\xff\xd9"
        continue
    for field, size in (("idleResource", (1024,512)), ("combatResource" if c.get("combatResource") else "spectralResource", (1024,2048))):
        data=(unity/"Assets/Resources"/(c[field]+".png")).read_bytes()
        assert struct.unpack(">II",data[16:24])==size
startup=core.split("void Start()",1)[1].split("void BeginSelectedCharacterWorld()",1)[0]
assert "InitializeCharacterSelection();" in startup and "Create(" not in startup
assert core.index("if(characterSelectionOpen)") < core.index("ReadMovement(hudScale)")
assert ui.index("if(characterSelectionOpen)") < ui.index("if(player==null || current==null)return;")
assert "if(entry.available && GUI.Button" in selection
assert selection.index("BeginSelectedCharacterWorld();") < selection.index("characterSelectionOpen=false;")
assert "void OpenInGameCharacterMenu()" in selection
assert "void CancelInGameCharacterMenu()" in selection
assert "void ResetHeroAnimationForCharacterSwap()" in hero_anim
assert "Screen.safeArea" in selection and "Time.unscaledTime" in selection
# 0.9.22: fourth character has its own atlas without modifying original art.
assert [c["id"] for c in characters[:3]]==[
    "necromante","arlequina","necromante_fisica"]
catalog_source=(unity/"Assets/Scripts/NecroCharacterCatalog.cs").read_text()
assert "walkSixResource, portraitResource;" in catalog_source
assert "characterPortrait=string.IsNullOrEmpty(SelectedCharacter.portraitResource)" in selection
assert "characterPreview.width<384" in selection
assert "characterPreview.height<512" in selection
assert "characterPortrait==null" not in selection.split(
    "if(!string.IsNullOrEmpty(selected.walkSixResource))",1)[1].split(
    "else",1)[0]
assert "GUI.DrawTextureWithTexCoords(new Rect(94,152,232,236)," in selection
assert "Sprite[] sixFrameWalkSprites;" in hero_anim
assert "sixFrameWalkSprites=new Sprite[48];" in hero_anim
assert "for(int direction=0;direction<8;direction++)" in hero_anim
assert "for(int step=0;step<6;step++)" in hero_anim
assert "walk.height-(direction+1)*cellHeight" in hero_anim
assert "int cellWidth=walk.width/6;" in hero_anim
assert "int cellHeight=walk.height/8;" in hero_anim
assert "walkDirection*6+Mathf.Clamp(walkPose,0,5)" in hero_anim
assert 'PlayerSettings.bundleVersion = "0.9.22";' in builder
assert 'PlayerSettings.bundleVersion="0.9.22";' in win_builder
assert 'PlayerSettings.Android.bundleVersionCode = 30;' in builder
assert (root/"docs/NECROMANTE_SOMBRIA_0_9_21.md").is_file()
print("Character selection: 4 selectable characters, 48 new walk frames, existing art unchanged.")

# A shared save with unlocked magic must not bypass the physical character.
physical = next(c for c in json.loads((unity/'Assets/Resources/Data/characters.json').read_text())['characters'] if c['id'] == 'necromante_fisica')
assert physical['meleeOnly'] and physical['available']
assert 'if(!HeroCanUseMagic || !spellUnlocked)' in gameplay
assert 'range=Mathf.Min(range,EffectiveManualRange)' in gameplay
for filename, signatures in {
    'NecroGameplay.cs': ['bool CanBindNearby()', 'void BindNearbyCreature()', 'void BindNearbyPup()', 'void Raise()', 'void ReleaseSkull()'],
    'NecroSkillTree.cs': ['void UnlockSkillTreeNode(int index)'],
}.items():
    source = (root/'unity/Assets/Scripts'/filename).read_text()
    for signature in signatures:
        body = source.split(signature, 1)[1].split('{', 1)[1].lstrip()
        assert body.startswith('if(!HeroCanUseMagic)return'), signature
print('Physical character: selection, melee override and magic capability gates verified.')

# Regression: C# forbids a combat-frame local named pose inside the same method
# as the existing Vector3 pose variable, even when declared later (CS0136).
character_anim=(unity/"Assets/Scripts/NecroCharacterAnimation.cs").read_text()
combat_section=character_anim.split("if(physicalCombatSprites!=null)",1)[1].split(
    "witchArtRenderer.flipX=flip;",1)[0]
assert "int combatPose=0;" in combat_section
assert "combatPose=strikePhase" in combat_section
assert "combatPose=phase==0" in combat_section
assert "physicalCombatSprites[walkDirection*4+combatPose]" in combat_section
assert "int pose=0;" not in combat_section
assert "Vector3 pose=witchArtRenderer.transform.localPosition;" in character_anim

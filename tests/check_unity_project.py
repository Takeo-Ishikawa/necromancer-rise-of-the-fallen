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
    "Assets/Scripts/NecroFrameLibrary.cs",
    "Assets/Scripts/NecroHeroSheetBuilder.cs",
    "Assets/Scripts/NecroHeroPaintedSource.cs",
    "Assets/Resources/AnimationSheets/necromante_source.png",
    "Assets/Editor/NecroFrameEditorCheck.cs",
    "Assets/Scripts/NecroCombatFx.cs",
    "Assets/Scripts/NecroScenery.cs",
    "Assets/Editor/NecroVisualEditorCheck.cs",
    "Assets/Scripts/NecroCharacterAnimation.cs",
    "Assets/Scripts/NecroSanctuaryInteractions.cs",
    "Assets/Scripts/NecroSanctuaryWardrobe.cs",
    "Assets/Scripts/NecroSanctuaryMeditation.cs",
    "Assets/Scripts/NecroSkillTree.cs",
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
                "MaxLivingEnemies=4", "SpawnMinDistance=3.4f",
                "MinEnemySeparation=1.8f", "current.enemies.Add(spawned)",
                "RespawnInterval=12f", "if(alive>=MaxLivingEnemies)"):
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
assert "SaveProgress();" not in gameplay.split("void Damage(",1)[1].split("Actor Closest(",1)[0]
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
for removed in ("TryPortal(", "Raise(", "CollectNearby(", "Claim(", "Autosave("):
    assert removed not in loop
assert "!current.claimed" not in respawn and "current.respawnTimer=RespawnInterval;" in respawn
assert 'Resources.Load<Texture2D>("Sprites/"+kind)' in scripts["NecroArt.cs"]
assert "RitualCircle()" in scripts["NecroArt.cs"]
assert "PlayerSettings.bundleVersion = \"0.9.6\"" in builder
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
assert "heroineHitPulse=.27f;" in gameplay
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
assert "Resources.Load<Texture2D>(" in hero_anim and "Heroine/spectral" in hero_anim
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
wardrobes=[s for s in station_json["stations"] if s["kind"]=="wardrobe"]
assert len(wardrobes)==1
wardrobe=wardrobes[0]
assert wardrobe["id"]=="wardrobe" and wardrobe["radius"]>0
assert wardrobe["x"]>4.5 and wardrobe["y"]>9.5
meditation_stations=[s for s in station_json["stations"] if s["kind"]=="meditation"]
assert len(meditation_stations)==1
altar=meditation_stations[0]
assert altar["id"]=="meditation" and altar["radius"]>0
assert 9.5<altar["x"]<12 and 7<altar["y"]<12
assert abs(altar["x"]-13.67)>1.0, "Altar must be approached from outside furniture collision"
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
assert "PlayerSettings.Android.bundleVersionCode = 20;" in builder


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
assert 'if(station.kind == "wardrobe")' in interactions
assert 'OpenSanctuaryWardrobe();' in interactions
assert 'station.kind == "wardrobe" ? "GUARDA-ROUPA' in interactions
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
assert 'station.kind == "meditation"' in interactions
assert 'OpenSanctuaryMeditation();' in interactions
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
assert 'PlayerSettings.bundleVersion = "0.9.6";' in builder
assert 'PlayerSettings.bundleVersion="0.9.6";' in win_builder
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
assert "SkillSpectralDamage(34+staffTier*8" in gameplay
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
assert "MeditationPointsAvailable()" in progress_hud
assert "AvailableSkillPoints()" in progress_hud
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
              "TouchPhase.Began", "Input.GetMouseButtonDown(0)",
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
assert "SoulPulseButtonRect(scale).Contains(gui)" in ui
assert "DrawSoulPulseButton(scale);" in ui
assert "ResetActiveSkillInput();" in (unity/"Assets/Scripts/NecroSanctuary.cs").read_text()
assert "if(!pulseAwardBatch)SaveProgress();" in meditation_source
assert 'type==3?"Necrotic Soul Pulse"' in fx_source
assert "fx.sprite.sortingOrder=type==3?11:type==4?14:18;" in fx_source
assert "new Vector3(4.0f,4.0f,1f)" in fx_source
assert "SpriteRenderer" in fx_source and "NecroArt.RitualCircle()" in fx_source
assert "mana<cost" in raise_body and "mana-=cost;" in raise_body
assert "mana" not in cast_body  # Basic attack stays free.
assert 'PlayerSettings.bundleVersion = "0.9.6";' in builder
assert 'PlayerSettings.bundleVersion="0.9.6";' in win_builder
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
              "Input.GetKeyDown(KeyCode.R)", "TouchPhase.Began",
              "Input.GetMouseButtonDown(0)", "ReadGraveCommandInput(float scale)",
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
assert "GraveCommandButtonRect(scale).Contains(gui)" in ui
assert "DrawGraveCommandButton(scale);" in ui
boar_source=(unity/"Assets/Scripts/NecroRotboar.cs").read_text()
assert "FindServantTarget(boar)" in boar_source
assert 'type==4?"Order of the Dead"' in fx_source
assert "new Vector3(1.75f,1.75f,1f)" in fx_source
assert "new Color(.33f,.95f,.76f,.74f)" in fx_source
assert 282-78 > 195 and 195-78 > 108, "Spell touch controls overlap"
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert 'PlayerSettings.bundleVersion = "0.9.6";' in builder
assert 'PlayerSettings.bundleVersion="0.9.6";' in win_builder
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
assert 'PlayerSettings.bundleVersion = "0.9.6";' in builder
assert 'PlayerSettings.bundleVersion="0.9.6";' in win_builder
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
assert 'PlayerSettings.bundleVersion = "0.9.6";' in builder
assert 'PlayerSettings.bundleVersion="0.9.6";' in win_builder
assert '"com.takeo.necromancer.unity.preview080"' in builder
assert "  push:" not in android_preview.split("jobs:",1)[0]
assert "  push:" not in windows_preview.split("jobs:",1)[0]

print(f"Unity source verified: {len(levels)} regions, 0.7.5 high resolution idle/walk heroine, eight-direction animation, manual attacks, respawn, army, portrait PNG "
      f"({width}x{height}), HUD, animator, save and ARM64. "
      "Unity Editor build and device validation remain necessary.")


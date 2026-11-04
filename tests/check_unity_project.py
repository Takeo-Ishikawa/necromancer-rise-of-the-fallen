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
    assert area["objective"] and len(area["enemies"]) >= 6
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
        "wisp", "warden", "grave_lord", "marsh_lord"} <= kinds

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
assert "TryAutomaticAttack();" in core
assert "Input.GetKeyDown(KeyCode.Space)" not in core
# Both melee and spectral projectile are free. Only Raise() spends anima.
cast_body = gameplay.split("        void Cast()", 1)[1].split(
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
assert "ManaCost(nearest.kind)" in raise_body
assert "respawnTimer=FirstRespawnSeconds" in core
for feature in ("TickRespawns(float dt)", "current.claimed", "IsBoss(entry.kind)",
                "MaxLivingEnemies=16", "SpawnMinDistance=5.0f",
                "MinEnemySeparation=1.8f", "current.enemies.Add(spawned)",
                "TryAutomaticAttack()", "Cast();"):
    assert feature in respawn, f"Missing loop/autoattack feature {feature}"
assert 'spellUnlocked?"MAGIA":"GOLPE"' not in hud
# Automatic attack has no button or oversized permanent caption.
assert '"ATAQUE AUTOMÁTICO"' not in hud
assert 'TryAutomaticAttack();' in core
assert '"REVIVER",Raise' in hud
assert '"EVOLUIR"' in hud
assert "int armyPage;" in hud
boss_kinds={"warden","grave_lord","marsh_lord"}
for area in levels:
    normal=[enemy for enemy in area["enemies"] if enemy["kind"] not in boss_kinds]
    assert normal, f"No respawnable regular enemies in {area['id']}"
    assert all(0 < x["x"] < area["width"] and 0 < x["y"] < area["height"] for x in normal)
    assert 10 <= min(16,max(10,len(normal)+4)) <= 16
assert "Input.touches" in core
assert "DrawWitch(" in scripts["NecroArt.cs"]
assert "DrawCryptAltar(" in scripts["NecroArt.cs"]
crypt = scripts["NecroCrypt.cs"]
data = scripts["NecroSaveData.cs"]
for feature in ("OpenCrypt(", "StoreServant(", "WithdrawServant(",
                "RitualVitality(", "RitualVault(", "RitualSpirit(",
                "SaveProgress(", "RestoreProgress(", "RecordStage(",
                "RestoreStage(", "OnApplicationPause(", "Autosave(",
                "RecoverAfterDeath("):
    assert feature in crypt, f"Missing Cripta 0.5 method: {feature}"
for feature in ("NecroSaveData", "NecroSaveStore", "Application.persistentDataPath",
                "SavePath", "BackupPath", "JsonUtility.FromJson",
                "JsonUtility.ToJson", "vault", "stages"):
    assert feature in data, f"Missing save component: {feature}"
assert "RestoreProgress();" in scripts["NecroPrototype.cs"]
assert "SaveProgress();" in scripts["NecroGameplay.cs"]
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
for name in ("InitializeHeroAnimation(", "AnimateHeroVisual(", "ritualCircleRenderer",
             "witchArtRenderer.flipX"):
    assert name in animation, "Missing character animation: "+name
assert "InitializeHeroAnimation();" in core
assert "AnimateHeroVisual(dt);" in core
assert "joystickFingerId" in core and "TouchPhase.Began" in core
assert "DrawPolishedHUD(w,h);" in ui
assert 'Resources.Load<Texture2D>("Sprites/"+kind)' in scripts["NecroArt.cs"]
assert "RitualCircle()" in scripts["NecroArt.cs"]
assert "PlayerSettings.bundleVersion = \"0.6.5c\"" in builder
assert "com.takeo.necromancer.unity.preview065c" in builder
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
assert "NecroPose.Idle,NecroDirection.Down" in (unity/"Assets/Scripts/NecroPolishedHUD.cs").read_text()
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
for required in ("heroineFrames.Frame(", "heroineStrikePulse",
                 "heroineHitPulse", "DirectionFrom("):
    assert required in hero_anim, required
for required in ("frames.Frame(", "currentPose", "currentDirection", "FrameCount"):
    assert required in (creature_anim+frame_library), required
assert "NecroFrameLibrary.Get(kind,original.sprite)" in creature_anim
assert "heroineStrikePulse=.53f;" in gameplay
assert "heroineHitPulse=.27f;" in gameplay
assert "VerifySet(\"necromante\",true);" in frame_editor
assert "com.takeo.necromancer.unity.preview065c" in builder
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
assert "NECRO_ANDROID_MODE: preview" in preview_workflow
assert "NECRO_ANDROID_MODE: release" in release_workflow
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
assert 'new Vector2(77f,178f)' in core
assert "Resources.Load<Texture2D>(\"Scenery/\"+kind)" in scenery
assert "Resources.Load<Texture2D>(\"Scenery/rock_cluster\")" in scripts["NecroArt.cs"]
assert "Time.time<noticeUntil" in hud
assert "Região conquistada — explore as passagens." not in hud
assert 'new Rect(0,0,width,89)' in hud
assert "Soft contact shadow" in hero_anim
assert "Staff spectral aura" in hero_anim

print(f"Unity source verified: {len(levels)} regions, 0.6.5c high resolution idle/walk heroine, four-direction animation, automatic attacks, respawn, army, portrait PNG "
      f"({width}x{height}), HUD, animator, save and ARM64. "
      "Unity Editor build and device validation remain necessary.")

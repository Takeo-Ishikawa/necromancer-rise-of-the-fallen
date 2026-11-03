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
    "Assets/Scripts/NecroUI.cs",
    "Assets/Scripts/NecroPolishedHUD.cs",
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
assert "PlayerSettings.bundleVersion = \"0.6.1\"" in builder
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

print(f"Unity source verified: {len(levels)} regions, 0.6 portrait PNG "
      f"({width}x{height}), HUD, animator, save and ARM64. "
      "Unity Editor build and device validation remain necessary.")

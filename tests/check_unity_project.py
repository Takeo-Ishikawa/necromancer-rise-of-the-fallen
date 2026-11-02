#!/usr/bin/env python3
"""Checks the Unity data layout without falsely claiming Unity compilation."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
unity = root / "unity"
paths = [
    "ProjectSettings/ProjectVersion.txt",
    "Packages/manifest.json",
    "Assets/Editor/NecroSceneBootstrap.cs",
    "Assets/Resources/Data/levels.json",
    "Assets/Scripts/NecroLevels.cs",
    "Assets/Scripts/NecroArt.cs",
    "Assets/Scripts/NecroPrototype.cs",
    "README.md",
]
for path in paths:
    assert (unity/path).is_file(), f"Missing: {path}"
manifest = json.loads((unity/"Packages/manifest.json").read_text())
assert "com.unity.2d.sprite" in manifest["dependencies"]
levels = json.loads((unity/"Assets/Resources/Data/levels.json").read_text())["levels"]
assert len(levels) >= 2
for level in levels:
    assert level["width"] > 20 and level["height"] > 15
    assert len(level["enemies"]) >= 4
    for entity in [*level["enemies"], {"x": level["startX"], "y": level["startY"], "kind": "player"}]:
        x,y = entity["x"],entity["y"]
        assert 0 < x < level["width"] and 0 < y < level["height"]
        assert all((x-rock["x"])**2+(y-rock["y"])**2 >
            (rock["r"]+0.20)**2 for rock in level["obstacles"]),             (level["id"],entity["kind"],"collision at spawn")
assert levels[0]["baseBackground"] != levels[0]["background"]
src = (unity/"Assets/Scripts/NecroPrototype.cs").read_text()
for feature in ("LoadStage(", "UpdateActors(", "Cast(", "Raise(", "Claim(", "Input.touches"):
    assert feature in src, f"Missing feature: {feature}"
assert "AddComponent<Necromancer.NecroPrototype>()" in (
    unity/"Assets/Editor/NecroSceneBootstrap.cs").read_text()
print("Unity project structure and stage data: OK (NOT a Unity compile or APK test)")

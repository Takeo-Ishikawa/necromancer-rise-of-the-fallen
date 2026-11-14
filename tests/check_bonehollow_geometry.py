#!/usr/bin/env python3
"""Static, deterministic walkability audit for Bonehollow Map 01.

This checks the actual circular obstacles from Data/levels.json, not the
decorative cave patches. It does NOT replace Unity pathfinding, collision,
mobile performance or companion-group gameplay tests.
"""
from __future__ import annotations

from collections import deque
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LEVELS = ROOT / "unity" / "Assets" / "Resources" / "Data" / "levels.json"
STEP = 0.4
CLEARANCES = (0.4, 0.65)
NEIGHBORS = ((1, 0), (-1, 0), (0, 1), (0, -1),
             (1, 1), (1, -1), (-1, 1), (-1, -1))


def load_bonehollow() -> dict:
    catalog = json.loads(LEVELS.read_text(encoding="utf-8"))["levels"]
    result = next((level for level in catalog if level["id"] == "bonehollow"), None)
    if result is None:
        raise AssertionError("Bonehollow missing from levels.json")
    if result["width"] <= 0 or result["height"] <= 0:
        raise AssertionError("Invalid Bonehollow dimensions")
    return result


def has_clearance(level: dict, x: float, y: float, radius: float) -> bool:
    if not (radius <= x <= level["width"] - radius and
            radius <= y <= level["height"] - radius):
        return False
    for rock in level["obstacles"]:
        separation = math.hypot(x - rock["x"], y - rock["y"])
        if separation < radius + rock["r"]:
            return False
    return True


def reachable(level: dict, radius: float, targets: dict[str, tuple[float, float]]
              ) -> list[str]:
    cols = int(level["width"] / STEP)
    rows = int(level["height"] / STEP)

    def nearest_cell(x: float, y: float) -> tuple[int, int]:
        return (max(0, min(cols - 1, round(x / STEP - 0.5))),
                max(0, min(rows - 1, round(y / STEP - 0.5))))

    def point(cell: tuple[int, int]) -> tuple[float, float]:
        return ((cell[0] + 0.5) * STEP, (cell[1] + 0.5) * STEP)

    def clear(cell: tuple[int, int]) -> bool:
        x, y = cell
        return (0 <= x < cols and 0 <= y < rows and
                has_clearance(level, *point(cell), radius))

    start = nearest_cell(level["startX"], level["startY"])
    if not clear(start):
        return ["start (grid-sampled location blocked)"]
    visited = {start}
    frontier = deque([start])

    while frontier:
        x, y = frontier.popleft()
        for dx, dy in NEIGHBORS:
            nxt = (x + dx, y + dy)
            if nxt in visited or not clear(nxt):
                continue
            # Avoid walking diagonally through a blocked corner.
            if dx and dy and (not clear((x + dx, y)) or
                              not clear((x, y + dy))):
                continue
            visited.add(nxt)
            frontier.append(nxt)

    missing = []
    for label, (x, y) in targets.items():
        if not has_clearance(level, x, y, radius):
            missing.append(f"{label} (spawn inside collision)")
        elif nearest_cell(x, y) not in visited:
            missing.append(f"{label} (no sampled route)")
    return missing


def main() -> None:
    level = load_bonehollow()
    enemies = level.get("enemies") or []
    resources = level.get("resources") or []
    obstacles = level.get("obstacles") or []
    assert len(obstacles) >= 1, "No obstacles found"
    assert len(enemies) >= 1, "No enemy positions found"
    assert len(resources) >= 1, "No resource positions found"

    objectives = {
        "seal": (level["sealX"], level["sealY"]),
        "rotboar": (level["rotboarX"], level["rotboarY"]),
    }
    objectives.update({
        f"enemy_{i + 1}_{e['kind']}": (e["x"], e["y"])
        for i, e in enumerate(enemies)
    })
    objectives.update({
        f"resource_{i + 1}_{r['type']}": (r["x"], r["y"])
        for i, r in enumerate(resources)
    })
    failures = []
    for radius in CLEARANCES:
        missing = reachable(level, radius, objectives)
        if missing:
            failures.append(f"radius={radius:.2f}: " + ", ".join(missing))
        else:
            print(f"OK: clearance radius {radius:.2f}, "
                  f"{len(objectives)} reachable map targets")
    if failures:
        raise SystemExit("Bonehollow geometry audit failed:\n" +
                         "\n".join(failures))
    print(f"Bonehollow audit passed: {len(obstacles)} obstacles, "
          f"{len(enemies)} regular enemies, {len(resources)} resources. "
          "Unity gameplay/servants not tested.")


if __name__ == "__main__":
    main()

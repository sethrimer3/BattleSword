# BattleSword

A private, for-fun Warhammer 40K inspired Unity 3D shooter project.

## Unity Setup

- Open this repository folder in Unity Hub.
- Recommended editor line: Unity 2022.3 LTS.
- Unity will restore packages from `Packages/manifest.json` on first open.
- Put test scenes in `Assets/Scenes/Dev` and playable levels in `Assets/Scenes/Levels`.

## Project Layout

- `Assets/Art` - source-facing sprites, textures, materials, and models.
- `Assets/Audio` - music, SFX, and voice files.
- `Assets/Prefabs` - reusable game objects grouped by gameplay role.
- `Assets/Scenes` - dev sandboxes and real levels.
- `Assets/Scripts` - player, weapons, enemies, UI, and shared systems.
- `Assets/Settings` - Unity assets for rendering, input, quality, and other project settings.
- `Assets/Docs` - design notes, references, and collaboration docs.

## Collaboration Notes

- Do not commit Unity-generated `Library`, `Temp`, `Obj`, `Logs`, or local IDE files.
- Commit `.meta` files with their assets. Unity uses them to keep references stable.
- Large art/audio files are allowed in the repo, but consider Git LFS before the project grows.
- Keep experiments in `Assets/Scenes/Dev` so main levels stay easier to review.

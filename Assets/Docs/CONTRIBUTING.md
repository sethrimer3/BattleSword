# Contributing

## Branches

Use short feature branches for gameplay, art, and level work. Examples:

- `feature/player-movement`
- `feature/bolter-prototype`
- `art/blockout-props`

## Unity Hygiene

- Commit `.meta` files with every asset.
- Keep large experiments in `Assets/Scenes/Dev`.
- Prefer prefabs for reusable gameplay objects.
- Avoid editing the same scene at the same time unless the team has coordinated it.

## Naming

- C# scripts use `PascalCase`.
- Prefabs use clear gameplay names, such as `PlayerMarine`, `BoltRifle`, or `CultistGrunt`.
- Materials end with `_Mat`.
- Textures describe their channel or purpose, such as `Wall_Albedo` or `Armor_Normal`.

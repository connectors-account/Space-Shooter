Resources folder
================

This folder is reserved for runtime-loadable assets (via Resources.Load).

The game currently generates all its visuals procedurally (geometric shapes
created in code — see PrimitiveSprite in BulletController.cs), so no assets
are strictly required here.

To use your own art instead of the generated shapes:
1. Drop sprite/texture files into this folder (or anywhere under Assets/).
2. Create Player/Enemy/Bullet prefabs using those sprites.
3. Assign the prefabs to:
   - GameBootstrap.playerPrefabOverride / enemyPrefabOverride, and/or
   - PlayerController.bulletPrefab, EnemyController.bulletPrefab
   in the Inspector.

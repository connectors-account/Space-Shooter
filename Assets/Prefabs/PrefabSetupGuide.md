# Prefab Setup Guide

This guide explains how to set up all the prefabs for the Space Shooter game in Unity.

## Player Prefab

1. Create an empty GameObject, name it "Player"
2. Add components:
   - **SpriteRenderer**: Assign the player sprite (triangle or spaceship)
   - **Rigidbody2D**: Set Body Type to "Kinematic"
   - **BoxCollider2D** or **PolygonCollider2D**: Set "Is Trigger" to true
   - **PlayerController** script
   - **HealthSystem** script
   - **AudioSource**: For sound effects
3. Create child object "FirePoint" at position (0, 0.5, 0)
4. Set Tag to "Player"
5. Set Layer to "Player"
6. Configure PlayerController:
   - Move Speed: 8
   - Fire Rate: 0.2
   - Assign Bullet Prefab
   - Assign Fire Point transform
7. Configure HealthSystem:
   - Max Health: 5
8. Drag to Prefabs folder

## Bullet Prefab (Player)

1. Create empty GameObject, name it "PlayerBullet"
2. Add components:
   - **SpriteRenderer**: Small yellow rectangle or circle
   - **Rigidbody2D**: Body Type "Kinematic"
   - **BoxCollider2D**: "Is Trigger" = true
   - **Bullet** script
3. Set Tag to "PlayerBullet"
4. Set Layer to "Bullet"
5. Configure Bullet:
   - Speed: 15
   - Damage: 1
   - Is Player Bullet: true
6. Drag to Prefabs folder

## Enemy Bullet Prefab

1. Duplicate PlayerBullet, name it "EnemyBullet"
2. Change sprite color to red
3. Set Tag to "EnemyBullet"
4. Configure Bullet:
   - Is Player Bullet: false
5. Drag to Prefabs folder

## Basic Enemy Prefab

1. Create empty GameObject, name it "EnemyBasic"
2. Add components:
   - **SpriteRenderer**: Red square or enemy sprite
   - **Rigidbody2D**: Body Type "Kinematic"
   - **BoxCollider2D**: "Is Trigger" = true
   - **Enemy** script
   - **AudioSource**
3. Set Tag to "Enemy"
4. Set Layer to "Enemy"
5. Configure Enemy:
   - Enemy Type: Basic
   - Movement Pattern: Straight
   - Shooting Pattern: Single
   - Health: 1
   - Score Value: 100
   - Move Speed: 3
   - Assign Enemy Bullet Prefab
   - Power Up Drop Chance: 0.1
6. Drag to Prefabs folder

## Fast Enemy Prefab

1. Duplicate EnemyBasic, name it "EnemyFast"
2. Change sprite to smaller/different color (cyan)
3. Configure Enemy:
   - Enemy Type: Fast
   - Movement Pattern: Zigzag
   - Shooting Pattern: None
   - Health: 1
   - Score Value: 150
   - Move Speed: 6
4. Drag to Prefabs folder

## Tank Enemy Prefab

1. Duplicate EnemyBasic, name it "EnemyTank"
2. Change sprite to larger/different color (purple)
3. Configure Enemy:
   - Enemy Type: Tank
   - Movement Pattern: Straight
   - Shooting Pattern: Spread
   - Health: 5
   - Score Value: 300
   - Move Speed: 1.5
4. Drag to Prefabs folder

## Boss Enemy Prefab

1. Duplicate EnemyBasic, name it "Boss"
2. Make sprite much larger
3. Configure Enemy:
   - Enemy Type: Boss
   - Movement Pattern: Circular
   - Shooting Pattern: Burst
   - Health: 20
   - Score Value: 1000
   - Move Speed: 1
   - Fire Rate: 2
4. Drag to Prefabs folder

## Power-Up Prefabs

### Weapon Upgrade
1. Create empty GameObject, name it "PowerUpWeapon"
2. Add components:
   - **SpriteRenderer**: Orange/yellow star or "W" sprite
   - **Rigidbody2D**: Body Type "Kinematic"
   - **CircleCollider2D**: "Is Trigger" = true
   - **PowerUp** script
3. Set Tag to "PowerUp"
4. Configure PowerUp:
   - Type: WeaponUpgrade
   - Move Speed: 2
5. Drag to Prefabs folder

### Health Recovery
1. Duplicate PowerUpWeapon, name it "PowerUpHealth"
2. Change sprite to green cross or heart
3. Configure PowerUp:
   - Type: Health
   - Health Amount: 2
4. Drag to Prefabs folder

### Shield
1. Duplicate PowerUpWeapon, name it "PowerUpShield"
2. Change sprite to blue circle or shield icon
3. Configure PowerUp:
   - Type: Shield
   - Shield Duration: 5
4. Drag to Prefabs folder

## Background Prefab

1. Create empty GameObject, name it "ScrollingBackground"
2. Add child "Background1":
   - Add SpriteRenderer with space/star background
   - Set Sorting Layer to "Background"
   - Scale to fill screen
3. Add child "Background2":
   - Duplicate of Background1
   - Position above Background1
4. Add **ScrollingBackground** script to parent
5. Configure:
   - Scroll Speed: 2
   - Reset Height: 20 (based on sprite height)
6. Drag to Prefabs folder

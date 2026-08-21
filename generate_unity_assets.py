#!/usr/bin/env python3
"""
generate_unity_assets.py
Generates consistent .meta files (deterministic GUIDs) for every script and sprite,
plus prefab .prefab YAML files and the two scene YAML files, all cross-referenced by GUID.

Run once from the project root:
    python generate_unity_assets.py

This is a build-time helper; Unity regenerates/normalizes these on first open, but the
generated files are valid and let the project open with references already wired.
"""

import os
import hashlib

ROOT = os.path.dirname(os.path.abspath(__file__))
ASSETS = os.path.join(ROOT, "Assets")

SCRIPT_FILEID = 11500000
SPRITE_FILEID = 21300000


def guid_for(path_key: str) -> str:
    """Deterministic 32-hex-char GUID derived from a stable key."""
    return hashlib.md5(path_key.encode("utf-8")).hexdigest()


def write(path: str, content: str):
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


# ---------------------------------------------------------------------------
# 1. Collect scripts & sprites, assign GUIDs
# ---------------------------------------------------------------------------
def collect_files(folder, ext):
    result = {}
    for dirpath, _, filenames in os.walk(folder):
        for name in filenames:
            if name.endswith(ext):
                full = os.path.join(dirpath, name)
                rel = os.path.relpath(full, ROOT).replace("\\", "/")
                result[rel] = guid_for(rel)
    return result


scripts = collect_files(os.path.join(ASSETS, "Scripts"), ".cs")
sprites = collect_files(os.path.join(ASSETS, "Sprites"), ".png")

# Map class name -> guid (class name equals file name for these scripts).
script_guid_by_class = {}
for rel, g in scripts.items():
    cls = os.path.splitext(os.path.basename(rel))[0]
    script_guid_by_class[cls] = g

# Map sprite base name -> guid.
sprite_guid_by_name = {}
for rel, g in sprites.items():
    base = os.path.splitext(os.path.basename(rel))[0]
    sprite_guid_by_name[base] = g


# ---------------------------------------------------------------------------
# 2. .meta files
# ---------------------------------------------------------------------------
def script_meta(guid):
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "MonoImporter:\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 2\n"
        "  defaultReferences: []\n"
        "  executionOrder: 0\n"
        "  icon: {instanceID: 0}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def sprite_meta(guid):
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "TextureImporter:\n"
        "  internalIDToNameTable: []\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 11\n"
        "  mipmaps:\n"
        "    mipMapMode: 0\n"
        "    enableMipMap: 0\n"
        "    sRGBTexture: 1\n"
        "    linearTexture: 0\n"
        "    fadeOut: 0\n"
        "    borderMipMap: 0\n"
        "    mipMapsPreserveCoverage: 0\n"
        "    alphaTestReferenceValue: 0.5\n"
        "    mipMapFadeDistanceStart: 1\n"
        "    mipMapFadeDistanceEnd: 3\n"
        "  bumpmap:\n"
        "    convertToNormalMap: 0\n"
        "    externalNormalMap: 0\n"
        "    heightScale: 0.25\n"
        "    normalMapFilter: 0\n"
        "  isReadable: 0\n"
        "  streamingMipmaps: 0\n"
        "  streamingMipmapsPriority: 0\n"
        "  grayScaleToAlpha: 0\n"
        "  generateCubemap: 6\n"
        "  cubemapConvolution: 0\n"
        "  seamlessCubemap: 0\n"
        "  textureFormat: 1\n"
        "  maxTextureSize: 2048\n"
        "  textureSettings:\n"
        "    serializedVersion: 2\n"
        "    filterMode: 0\n"
        "    aniso: 1\n"
        "    mipBias: 0\n"
        "    wrapU: 1\n"
        "    wrapV: 1\n"
        "    wrapW: 1\n"
        "  nPOTScale: 0\n"
        "  lightmap: 0\n"
        "  compressionQuality: 50\n"
        "  spriteMode: 1\n"
        "  spriteExtrude: 1\n"
        "  spriteMeshType: 1\n"
        "  alignment: 0\n"
        "  spritePivot: {x: 0.5, y: 0.5}\n"
        "  spritePixelsToUnits: 100\n"
        "  spriteBorder: {x: 0, y: 0, z: 0, w: 0}\n"
        "  spriteGenerateFallbackPhysicsShape: 1\n"
        "  alphaUsage: 1\n"
        "  alphaIsTransparency: 1\n"
        "  spriteTessellationDetail: -1\n"
        "  textureType: 8\n"
        "  textureShape: 1\n"
        "  singleChannelComponent: 0\n"
        "  maxTextureSizeSet: 0\n"
        "  compressionQualitySet: 0\n"
        "  textureFormatSet: 0\n"
        "  applyGammaDecoding: 0\n"
        "  platformSettings:\n"
        "  - serializedVersion: 3\n"
        "    buildTarget: DefaultTexturePlatform\n"
        "    maxTextureSize: 2048\n"
        "    resizeAlgorithm: 0\n"
        "    textureFormat: -1\n"
        "    textureCompression: 0\n"
        "    compressionQuality: 50\n"
        "    crunchedCompression: 0\n"
        "    allowsAlphaSplitting: 0\n"
        "    overridden: 0\n"
        "    androidETC2FallbackOverride: 0\n"
        "    forceMaximumCompressionQuality_BC6H_BC7: 0\n"
        "  spriteSheet:\n"
        "    serializedVersion: 2\n"
        "    sprites: []\n"
        "    outline: []\n"
        "    physicsShape: []\n"
        "    bones: []\n"
        "    spriteID: 5e97eb03825dee720800000000000000\n"
        "    internalID: 0\n"
        "    vertices: []\n"
        "    indices: \n"
        "    edges: []\n"
        "    weights: []\n"
        "    secondaryTextures: []\n"
        "  spritePackingTag: \n"
        "  pSDRemoveMatte: 0\n"
        "  pSDShowRemoveMatteOption: 0\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def folder_meta(guid):
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "folderAsset: yes\n"
        "DefaultImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


for rel, g in scripts.items():
    write(os.path.join(ROOT, rel + ".meta"), script_meta(g))
for rel, g in sprites.items():
    write(os.path.join(ROOT, rel + ".meta"), sprite_meta(g))

# Folder metas (so Unity keeps folder GUIDs stable).
for dirpath, dirnames, _ in os.walk(ASSETS):
    for d in dirnames:
        full = os.path.join(dirpath, d)
        rel = os.path.relpath(full, ROOT).replace("\\", "/")
        write(os.path.join(ROOT, rel + ".meta"), folder_meta(guid_for("folder:" + rel)))
# Assets root meta not required.


def sprite_ref(name):
    g = sprite_guid_by_name.get(name)
    if not g:
        return "{fileID: 0}"
    return f"{{fileID: {SPRITE_FILEID}, guid: {g}, type: 3}}"


def script_ref(cls):
    g = script_guid_by_class.get(cls)
    if not g:
        return "{fileID: 0}"
    return f"{{fileID: {SCRIPT_FILEID}, guid: {g}, type: 3}}"


# ---------------------------------------------------------------------------
# 3. Prefab generation
# ---------------------------------------------------------------------------
HEADER = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"


def make_prefab(name, sprite_name, scripts_with_fields, tag="Untagged", layer=0,
                collider="circle", collider_radius=0.3, is_trigger=True,
                rb=True, rb_gravity=0, sorting_order=0, scale=1.0,
                trail=False, extra_components=""):
    """Build a single-GameObject prefab with SpriteRenderer, collider, RB and scripts."""
    go_id = 100000
    transform_id = 400000
    sr_id = 2120000
    rb_id = 5000000
    col_id = 6100000
    trail_id = 9600000

    parts = [HEADER]
    # GameObject
    comp_lines = [
        f"  - component: {{fileID: {transform_id}}}",
        f"  - component: {{fileID: {sr_id}}}",
    ]
    if collider:
        comp_lines.append(f"  - component: {{fileID: {col_id}}}")
    if rb:
        comp_lines.append(f"  - component: {{fileID: {rb_id}}}")
    if trail:
        comp_lines.append(f"  - component: {{fileID: {trail_id}}}")
    script_ids = []
    base = 1140000
    for i, (cls, fields) in enumerate(scripts_with_fields):
        sid = base + i
        script_ids.append((sid, cls, fields))
        comp_lines.append(f"  - component: {{fileID: {sid}}}")

    parts.append(
        f"--- !u!1 &{go_id}\n"
        "GameObject:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        "  serializedVersion: 6\n"
        "  m_Component:\n"
        + "\n".join(comp_lines) + "\n"
        "  m_Layer: " + str(layer) + "\n"
        f"  m_Name: {name}\n"
        f"  m_TagString: {tag}\n"
        "  m_Icon: {fileID: 0}\n"
        "  m_NavMeshLayer: 0\n"
        "  m_StaticEditorFlags: 0\n"
        "  m_IsActive: 1\n"
    )
    # Transform
    parts.append(
        f"--- !u!4 &{transform_id}\n"
        "Transform:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n"
        "  m_LocalPosition: {x: 0, y: 0, z: 0}\n"
        f"  m_LocalScale: {{x: {scale}, y: {scale}, z: {scale}}}\n"
        "  m_ConstrainProportionsScale: 0\n"
        "  m_Children: []\n"
        "  m_Father: {fileID: 0}\n"
        "  m_RootOrder: 0\n"
        "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n"
    )
    # SpriteRenderer
    parts.append(
        f"--- !u!212 &{sr_id}\n"
        "SpriteRenderer:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  m_Enabled: 1\n"
        "  m_CastShadows: 0\n"
        "  m_ReceiveShadows: 0\n"
        "  m_DynamicOccludee: 1\n"
        "  m_StaticShadowCaster: 0\n"
        "  m_MotionVectors: 1\n"
        "  m_LightProbeUsage: 1\n"
        "  m_ReflectionProbeUsage: 1\n"
        "  m_RayTracingMode: 0\n"
        "  m_RayTraceProcedural: 0\n"
        "  m_RenderingLayerMask: 1\n"
        "  m_RendererPriority: 0\n"
        "  m_Materials:\n"
        "  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}\n"
        "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n"
        "  m_FlipX: 0\n"
        "  m_FlipY: 0\n"
        "  m_DrawMode: 0\n"
        "  m_Size: {x: 1, y: 1}\n"
        "  m_AdaptiveModeThreshold: 0.5\n"
        "  m_SpriteTileMode: 0\n"
        "  m_WasSpriteAssigned: 1\n"
        "  m_MaskInteraction: 0\n"
        "  m_SpriteSortPoint: 0\n"
        f"  m_Sprite: {sprite_ref(sprite_name)}\n"
        f"  m_SortingOrder: {sorting_order}\n"
    )
    # Collider
    if collider == "circle":
        parts.append(
            f"--- !u!58 &{col_id}\n"
            "CircleCollider2D:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_Enabled: 1\n"
            "  m_Density: 1\n"
            "  m_Material: {fileID: 0}\n"
            f"  m_IsTrigger: {1 if is_trigger else 0}\n"
            "  m_UsedByEffector: 0\n"
            "  m_UsedByComposite: 0\n"
            "  m_Offset: {x: 0, y: 0}\n"
            f"  m_Radius: {collider_radius}\n"
        )
    elif collider == "box":
        parts.append(
            f"--- !u!61 &{col_id}\n"
            "BoxCollider2D:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_Enabled: 1\n"
            "  m_Density: 1\n"
            "  m_Material: {fileID: 0}\n"
            f"  m_IsTrigger: {1 if is_trigger else 0}\n"
            "  m_UsedByEffector: 0\n"
            "  m_UsedByComposite: 0\n"
            "  m_Offset: {x: 0, y: 0}\n"
            f"  m_Size: {{x: {collider_radius*2}, y: {collider_radius*2}}}\n"
            "  m_EdgeRadius: 0\n"
        )
    # Rigidbody2D
    if rb:
        parts.append(
            f"--- !u!50 &{rb_id}\n"
            "Rigidbody2D:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  serializedVersion: 4\n"
            "  m_BodyType: 0\n"
            "  m_Simulated: 1\n"
            "  m_UseFullKinematicContacts: 0\n"
            "  m_UseAutoMass: 0\n"
            "  m_Mass: 1\n"
            "  m_LinearDrag: 0\n"
            "  m_AngularDrag: 0.05\n"
            f"  m_GravityScale: {rb_gravity}\n"
            "  m_Material: {fileID: 0}\n"
            "  m_Interpolate: 0\n"
            "  m_SleepingMode: 1\n"
            "  m_CollisionDetection: 0\n"
            "  m_Constraints: 0\n"
        )
    # TrailRenderer
    if trail:
        parts.append(
            f"--- !u!96 &{trail_id}\n"
            "TrailRenderer:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_Enabled: 1\n"
            "  m_Materials:\n"
            "  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}\n"
            "  m_Time: 0.3\n"
            "  m_Parameters:\n"
            "    serializedVersion: 3\n"
            "    widthMultiplier: 0.2\n"
            "    widthCurve:\n"
            "      serializedVersion: 2\n"
            "      m_Curve:\n"
            "      - serializedVersion: 3\n"
            "        time: 0\n"
            "        value: 1\n"
            "        inSlope: 0\n"
            "        outSlope: 0\n"
            "        tangentMode: 0\n"
            "        weightedMode: 0\n"
            "        inWeight: 0\n"
            "        outWeight: 0\n"
            "      m_PreInfinity: 2\n"
            "      m_PostInfinity: 2\n"
            "      m_RotationOrder: 4\n"
            "    colorGradient:\n"
            "      serializedVersion: 2\n"
            "      key0: {r: 0, g: 1, b: 1, a: 0.5}\n"
            "      key1: {r: 0, g: 1, b: 1, a: 0}\n"
            "      key2: {r: 0, g: 0, b: 0, a: 0}\n"
            "      key3: {r: 0, g: 0, b: 0, a: 0}\n"
            "      key4: {r: 0, g: 0, b: 0, a: 0}\n"
            "      key5: {r: 0, g: 0, b: 0, a: 0}\n"
            "      key6: {r: 0, g: 0, b: 0, a: 0}\n"
            "      key7: {r: 0, g: 0, b: 0, a: 0}\n"
            "      ctime0: 0\n"
            "      ctime1: 65535\n"
            "      ctime2: 0\n"
            "      ctime3: 0\n"
            "      ctime4: 0\n"
            "      ctime5: 0\n"
            "      ctime6: 0\n"
            "      ctime7: 0\n"
            "      atime0: 0\n"
            "      atime1: 65535\n"
            "      atime2: 0\n"
            "      atime3: 0\n"
            "      atime4: 0\n"
            "      atime5: 0\n"
            "      atime6: 0\n"
            "      atime7: 0\n"
            "      m_Mode: 0\n"
            "      m_ColorSpace: -1\n"
            "      m_NumColorKeys: 2\n"
            "      m_NumAlphaKeys: 2\n"
            "  m_MinVertexDistance: 0.1\n"
            "  m_Autodestruct: 0\n"
            "  m_Emitting: 1\n"
        )
    # MonoBehaviours
    for sid, cls, fields in script_ids:
        field_str = "".join(fields) if fields else ""
        parts.append(
            f"--- !u!114 &{sid}\n"
            "MonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_Enabled: 1\n"
            "  m_EditorHideFlags: 0\n"
            f"  m_Script: {script_ref(cls)}\n"
            f"  m_Name: \n"
            "  m_EditorClassIdentifier: \n"
            + field_str
        )

    content = "".join(parts)
    prefab_path = os.path.join(ASSETS, "Prefabs", name + ".prefab")
    write(prefab_path, content)
    # Prefab meta
    write(prefab_path + ".meta",
          "fileFormatVersion: 2\n"
          f"guid: {guid_for('Assets/Prefabs/' + name + '.prefab')}\n"
          "PrefabImporter:\n"
          "  externalObjects: {}\n"
          "  userData: \n"
          "  assetBundleName: \n"
          "  assetBundleVariant: \n")
    return prefab_path


os.makedirs(os.path.join(ASSETS, "Prefabs"), exist_ok=True)

# Bullets
make_prefab("PlayerBullet", "bullet_player",
            [("Bullet", ["  firePoint: {fileID: 0}\n"] if False else [])],
            tag="PlayerBullet", layer=8, collider="circle", collider_radius=0.12,
            rb=True, sorting_order=2)
make_prefab("EnemyBullet", "bullet_enemy",
            [("Bullet", [])],
            tag="EnemyBullet", layer=8, collider="circle", collider_radius=0.12,
            rb=True, sorting_order=2)

# Enemies
make_prefab("EnemyA", "enemy_a", [("EnemyTypeA", [])], tag="Enemy", layer=7,
            collider="circle", collider_radius=0.24, rb=True, sorting_order=1)
make_prefab("EnemyB", "enemy_b", [("EnemyTypeB", [])], tag="Enemy", layer=7,
            collider="circle", collider_radius=0.24, rb=True, sorting_order=1)
make_prefab("EnemyBoss", "enemy_boss", [("EnemyBoss", [])], tag="Boss", layer=7,
            collider="circle", collider_radius=0.6, rb=True, sorting_order=1)

# Player
make_prefab("Player", "player_ship",
            [("PlayerController", []), ("PlayerShooter", []), ("PlayerHealth", [])],
            tag="Player", layer=6, collider="circle", collider_radius=0.28,
            rb=True, sorting_order=3, trail=True)

# Power-ups
powerup_defs = [
    ("PowerUp_Speed", "powerup_speed", 0),
    ("PowerUp_Rapid", "powerup_rapid", 1),
    ("PowerUp_Triple", "powerup_triple", 2),
    ("PowerUp_Shield", "powerup_shield", 3),
    ("PowerUp_Health", "powerup_health", 4),
    ("PowerUp_Bomb", "powerup_bomb", 5),
]
for pname, psprite, ptype in powerup_defs:
    make_prefab(pname, psprite,
                [("PowerUp", [f"  type: {ptype}\n", f"  poolTag: {pname}\n"])],
                tag="PowerUp", layer=0, collider="circle", collider_radius=0.2,
                rb=True, sorting_order=2)

# Explosion prefab: GameObject + ParticleSystem + ExplosionEffect.
def make_explosion_prefab():
    go_id = 100000
    tr_id = 400000
    ps_id = 1980000
    psr_id = 1990000
    fx_id = 1140000
    content = HEADER
    content += (
        f"--- !u!1 &{go_id}\n"
        "GameObject:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        "  serializedVersion: 6\n"
        "  m_Component:\n"
        f"  - component: {{fileID: {tr_id}}}\n"
        f"  - component: {{fileID: {ps_id}}}\n"
        f"  - component: {{fileID: {psr_id}}}\n"
        f"  - component: {{fileID: {fx_id}}}\n"
        "  m_Layer: 0\n"
        "  m_Name: Explosion\n"
        "  m_TagString: Untagged\n"
        "  m_Icon: {fileID: 0}\n"
        "  m_NavMeshLayer: 0\n"
        "  m_StaticEditorFlags: 0\n"
        "  m_IsActive: 1\n"
    )
    content += (
        f"--- !u!4 &{tr_id}\n"
        "Transform:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n"
        "  m_LocalPosition: {x: 0, y: 0, z: 0}\n"
        "  m_LocalScale: {x: 1, y: 1, z: 1}\n"
        "  m_ConstrainProportionsScale: 0\n"
        "  m_Children: []\n"
        "  m_Father: {fileID: 0}\n"
        "  m_RootOrder: 0\n"
        "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n"
    )
    content += (
        f"--- !u!198 &{ps_id}\n"
        "ParticleSystem:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  serializedVersion: 6\n"
        "  lengthInSec: 0.6\n"
        "  simulationSpeed: 1\n"
        "  stopAction: 0\n"
        "  cullingMode: 0\n"
        "  ringBufferMode: 0\n"
        "  ringBufferLoopRange: {x: 0, y: 1}\n"
        "  looping: 0\n"
        "  prewarm: 0\n"
        "  playOnAwake: 0\n"
    )
    content += (
        f"--- !u!199 &{psr_id}\n"
        "ParticleSystemRenderer:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  m_Enabled: 1\n"
        "  m_Materials:\n"
        "  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}\n"
        "  m_SortingOrder: 5\n"
    )
    content += (
        f"--- !u!114 &{fx_id}\n"
        "MonoBehaviour:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        f"  m_GameObject: {{fileID: {go_id}}}\n"
        "  m_Enabled: 1\n"
        "  m_EditorHideFlags: 0\n"
        f"  m_Script: {script_ref('ExplosionEffect')}\n"
        "  m_Name: \n"
        "  m_EditorClassIdentifier: \n"
    )
    path = os.path.join(ASSETS, "Prefabs", "Explosion.prefab")
    write(path, content)
    write(path + ".meta",
          "fileFormatVersion: 2\n"
          f"guid: {guid_for('Assets/Prefabs/Explosion.prefab')}\n"
          "PrefabImporter:\n"
          "  externalObjects: {}\n"
          "  userData: \n"
          "  assetBundleName: \n"
          "  assetBundleVariant: \n")


make_explosion_prefab()

print("Prefabs and meta files generated.")
print(f"  {len(scripts)} script metas, {len(sprites)} sprite metas.")



# ---------------------------------------------------------------------------
# 4. Scene generation
# ---------------------------------------------------------------------------
class SceneBuilder:
    """Accumulates Unity YAML objects with unique fileIDs and writes a .unity scene."""

    def __init__(self):
        self.blocks = []
        self._id = 100
        self.root_order = 0

    def next_id(self):
        self._id += 2
        return self._id

    def add_settings(self):
        # Minimal but valid scene settings blocks Unity expects.
        self.blocks.append(
            "--- !u!29 &1\n"
            "OcclusionCullingSettings:\n"
            "  m_ObjectHideFlags: 0\n"
            "  serializedVersion: 2\n"
            "  m_OcclusionBakeSettings:\n"
            "    smallestOccluder: 5\n"
            "    smallestHole: 0.25\n"
            "    backfaceThreshold: 100\n"
            "  m_SceneGUID: 00000000000000000000000000000000\n"
            "  m_OcclusionCullingData: {fileID: 0}\n"
        )
        self.blocks.append(
            "--- !u!104 &2\n"
            "RenderSettings:\n"
            "  m_ObjectHideFlags: 0\n"
            "  serializedVersion: 9\n"
            "  m_Fog: 0\n"
            "  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}\n"
            "  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}\n"
            "  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}\n"
            "  m_AmbientIntensity: 1\n"
            "  m_AmbientMode: 3\n"
            "  m_SkyboxMaterial: {fileID: 0}\n"
            "  m_HaloStrength: 0.5\n"
            "  m_FlareStrength: 1\n"
            "  m_FlareFadeSpeed: 3\n"
            "  m_Sun: {fileID: 0}\n"
            "  m_IndirectSpecularColor: {r: 0, g: 0, b: 0, a: 1}\n"
            "  m_UseRadianceAmbientProbe: 0\n"
        )
        self.blocks.append(
            "--- !u!157 &3\n"
            "LightmapSettings:\n"
            "  m_ObjectHideFlags: 0\n"
            "  serializedVersion: 12\n"
            "  m_GIWorkflowMode: 1\n"
            "  m_GISettings:\n"
            "    serializedVersion: 2\n"
            "    m_BounceScale: 1\n"
            "    m_IndirectOutputScale: 1\n"
            "    m_AlbedoBoost: 1\n"
            "    m_EnvironmentLightingMode: 0\n"
            "    m_EnableBakedLightmaps: 0\n"
            "    m_EnableRealtimeLightmaps: 0\n"
            "  m_LightmapEditorSettings:\n"
            "    serializedVersion: 12\n"
            "    m_Resolution: 2\n"
            "    m_BakeResolution: 40\n"
            "    m_AtlasSize: 1024\n"
            "  m_LightingDataAsset: {fileID: 0}\n"
            "  m_LightingSettings: {fileID: 0}\n"
        )
        self.blocks.append(
            "--- !u!196 &4\n"
            "NavMeshSettings:\n"
            "  serializedVersion: 2\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_BuildSettings:\n"
            "    serializedVersion: 3\n"
            "    agentTypeID: 0\n"
            "    agentRadius: 0.5\n"
            "    agentHeight: 2\n"
            "    agentSlope: 45\n"
            "    agentClimb: 0.4\n"
            "  m_NavMeshData: {fileID: 0}\n"
        )

    def add_gameobject(self, name, components_desc, tag="Untagged", layer=0, pos=(0, 0, 0),
                       parent_transform_id=0, scale=(1, 1, 1)):
        """
        components_desc: list of ('type', builder_fn(go_id, self_id)) tuples returning YAML.
        Always creates a Transform first. Returns (go_id, transform_id).
        """
        go_id = self.next_id()
        transform_id = self.next_id()

        comp_ids = [("transform", transform_id)]
        comp_blocks = []
        for ctype, fn in components_desc:
            cid = self.next_id()
            comp_ids.append((ctype, cid))
            comp_blocks.append(fn(go_id, cid, transform_id))

        comp_lines = "\n".join(f"  - component: {{fileID: {cid}}}" for _, cid in comp_ids)
        self.blocks.append(
            f"--- !u!1 &{go_id}\n"
            "GameObject:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            "  serializedVersion: 6\n"
            "  m_Component:\n"
            f"{comp_lines}\n"
            f"  m_Layer: {layer}\n"
            f"  m_Name: {name}\n"
            f"  m_TagString: {tag}\n"
            "  m_Icon: {fileID: 0}\n"
            "  m_NavMeshLayer: 0\n"
            "  m_StaticEditorFlags: 0\n"
            "  m_IsActive: 1\n"
        )
        self.blocks.append(
            f"--- !u!4 &{transform_id}\n"
            "Transform:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n"
            f"  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}\n"
            f"  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}\n"
            "  m_ConstrainProportionsScale: 0\n"
            "  m_Children: []\n"
            f"  m_Father: {{fileID: {parent_transform_id}}}\n"
            f"  m_RootOrder: {self.root_order}\n"
            "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n"
        )
        self.root_order += 1
        for block in comp_blocks:
            self.blocks.append(block)
        return go_id, transform_id

    def mono(self, cls, extra=""):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!114 &{cid}\n"
                "MonoBehaviour:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  m_Enabled: 1\n"
                "  m_EditorHideFlags: 0\n"
                f"  m_Script: {script_ref(cls)}\n"
                "  m_Name: \n"
                "  m_EditorClassIdentifier: \n"
                + extra
            )
        return ("mono_" + cls, fn)

    def camera(self):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!20 &{cid}\n"
                "Camera:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  m_Enabled: 1\n"
                "  serializedVersion: 2\n"
                "  m_ClearFlags: 2\n"
                "  m_BackGroundColor: {r: 0.02, g: 0.02, b: 0.05, a: 1}\n"
                "  m_projectionMatrixMode: 1\n"
                "  m_Orthographic: 1\n"
                "  m_OrthographicSize: 5\n"
                "  m_FieldOfView: 60\n"
                "  m_NearClipPlane: 0.3\n"
                "  m_FarClipPlane: 1000\n"
                "  m_Depth: -1\n"
                "  m_CullingMask:\n"
                "    serializedVersion: 2\n"
                "    m_Bits: 4294967295\n"
                "  m_RenderingPath: -1\n"
                "  m_TargetTexture: {fileID: 0}\n"
                "  m_TargetDisplay: 0\n"
                "  m_TargetEye: 3\n"
                "  m_HDR: 1\n"
                "  m_AllowMSAA: 1\n"
                "  m_AllowDynamicResolution: 0\n"
                "  m_ForceIntoRT: 0\n"
                "  m_OcclusionCulling: 1\n"
                "  m_StereoConvergence: 10\n"
                "  m_StereoSeparation: 0.022\n"
            )
        return ("camera", fn)

    def audiolistener(self):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!81 &{cid}\n"
                "AudioListener:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  m_Enabled: 1\n"
            )
        return ("audiolistener", fn)

    def sprite_renderer(self, sprite_name, order=0):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!212 &{cid}\n"
                "SpriteRenderer:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  m_Enabled: 1\n"
                "  m_CastShadows: 0\n"
                "  m_ReceiveShadows: 0\n"
                "  m_MotionVectors: 1\n"
                "  m_LightProbeUsage: 1\n"
                "  m_ReflectionProbeUsage: 1\n"
                "  m_RenderingLayerMask: 1\n"
                "  m_RendererPriority: 0\n"
                "  m_Materials:\n"
                "  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}\n"
                "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n"
                "  m_FlipX: 0\n"
                "  m_FlipY: 0\n"
                "  m_DrawMode: 0\n"
                "  m_Size: {x: 1, y: 1}\n"
                "  m_WasSpriteAssigned: 1\n"
                "  m_MaskInteraction: 0\n"
                "  m_SpriteSortPoint: 0\n"
                f"  m_Sprite: {sprite_ref(sprite_name)}\n"
                f"  m_SortingOrder: {order}\n"
            )
        return ("spriterenderer", fn)

    def circle_collider(self, radius=0.28, trigger=True):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!58 &{cid}\n"
                "CircleCollider2D:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  m_Enabled: 1\n"
                "  m_Density: 1\n"
                "  m_Material: {fileID: 0}\n"
                f"  m_IsTrigger: {1 if trigger else 0}\n"
                "  m_UsedByEffector: 0\n"
                "  m_UsedByComposite: 0\n"
                "  m_Offset: {x: 0, y: 0}\n"
                f"  m_Radius: {radius}\n"
            )
        return ("circlecollider", fn)

    def rigidbody2d(self, gravity=0):
        def fn(go_id, cid, _tr):
            return (
                f"--- !u!50 &{cid}\n"
                "Rigidbody2D:\n"
                "  m_ObjectHideFlags: 0\n"
                "  m_CorrespondingSourceObject: {fileID: 0}\n"
                "  m_PrefabInstance: {fileID: 0}\n"
                "  m_PrefabAsset: {fileID: 0}\n"
                f"  m_GameObject: {{fileID: {go_id}}}\n"
                "  serializedVersion: 4\n"
                "  m_BodyType: 0\n"
                "  m_Simulated: 1\n"
                "  m_UseFullKinematicContacts: 0\n"
                "  m_UseAutoMass: 0\n"
                "  m_Mass: 1\n"
                "  m_LinearDrag: 0\n"
                "  m_AngularDrag: 0.05\n"
                f"  m_GravityScale: {gravity}\n"
                "  m_Material: {fileID: 0}\n"
                "  m_Interpolate: 0\n"
                "  m_SleepingMode: 1\n"
                "  m_CollisionDetection: 0\n"
                "  m_Constraints: 0\n"
            )
        return ("rigidbody2d", fn)

    def write_scene(self, path):
        content = HEADER + "".join(self.blocks)
        write(path, content)
        write(path + ".meta",
              "fileFormatVersion: 2\n"
              f"guid: {guid_for(os.path.relpath(path, ROOT).replace(chr(92), '/'))}\n"
              "DefaultImporter:\n"
              "  externalObjects: {}\n"
              "  userData: \n"
              "  assetBundleName: \n"
              "  assetBundleVariant: \n")


def build_main_menu():
    sb = SceneBuilder()
    sb.add_settings()
    # Camera + AudioListener
    sb.add_gameobject("Main Camera", [sb.camera(), sb.audiolistener()],
                      tag="MainCamera", pos=(0, 0, -10))
    # Managers (persistent-capable)
    sb.add_gameobject("GameManager", [sb.mono("GameManager")])
    sb.add_gameobject("AudioManager", [sb.mono("AudioManager")])
    # MainMenuController host
    sb.add_gameobject("MainMenuController", [sb.mono("MainMenuController")])
    sb.write_scene(os.path.join(ASSETS, "Scenes", "MainMenu.unity"))


def prefab_ref(prefab_name):
    g = guid_for('Assets/Prefabs/' + prefab_name + '.prefab')
    return f"{{fileID: 100000, guid: {g}, type: 3}}"


def build_object_pool_extra():
    """Serialized 'pools' list wiring every pool tag to its prefab."""
    entries = [
        ("PlayerBullet", "PlayerBullet", 60),
        ("EnemyBullet", "EnemyBullet", 120),
        ("EnemyA", "EnemyA", 30),
        ("EnemyB", "EnemyB", 30),
        ("EnemyBoss", "EnemyBoss", 2),
        ("Explosion", "Explosion", 30),
        ("PowerUp_Speed", "PowerUp_Speed", 5),
        ("PowerUp_Rapid", "PowerUp_Rapid", 5),
        ("PowerUp_Triple", "PowerUp_Triple", 5),
        ("PowerUp_Shield", "PowerUp_Shield", 5),
        ("PowerUp_Health", "PowerUp_Health", 5),
        ("PowerUp_Bomb", "PowerUp_Bomb", 5),
    ]
    lines = "  pools:\n"
    for tag, prefab, size in entries:
        lines += (
            f"  - tag: {tag}\n"
            f"    prefab: {prefab_ref(prefab)}\n"
            f"    size: {size}\n"
        )
    return lines


def build_game_scene():
    sb = SceneBuilder()
    sb.add_settings()
    sb.add_gameobject("Main Camera", [sb.camera(), sb.audiolistener(), sb.mono("CameraShake")],
                      tag="MainCamera", pos=(0, 0, -10))
    sb.add_gameobject("GameManager", [sb.mono("GameManager")])
    sb.add_gameobject("AudioManager", [sb.mono("AudioManager")])
    sb.add_gameobject("ObjectPool", [sb.mono("ObjectPool", build_object_pool_extra())])
    sb.add_gameobject("ScoreManager", [sb.mono("ScoreManager")])
    sb.add_gameobject("WaveManager", [sb.mono("WaveManager")])
    sb.add_gameobject("EnemySpawner", [sb.mono("EnemySpawner")])
    sb.add_gameobject("ParallaxBackground", [sb.mono("ParallaxBackground")])
    sb.add_gameobject("UIManager", [sb.mono("UIManager")])
    # Player instance (playable without manual prefab drag).
    sb.add_gameobject(
        "Player",
        [
            sb.sprite_renderer("player_ship", order=3),
            sb.circle_collider(0.28, trigger=True),
            sb.rigidbody2d(0),
            sb.mono("PlayerController"),
            sb.mono("PlayerShooter"),
            sb.mono("PlayerHealth"),
        ],
        tag="Player", layer=6, pos=(0, -3.5, 0))
    sb.write_scene(os.path.join(ASSETS, "Scenes", "GameScene.unity"))


os.makedirs(os.path.join(ASSETS, "Scenes"), exist_ok=True)
build_main_menu()
build_game_scene()
print("Scenes generated (MainMenu.unity, GameScene.unity).")

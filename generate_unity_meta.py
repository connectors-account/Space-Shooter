#!/usr/bin/env python3
"""
Generates Unity .meta files (deterministic GUIDs), the two .unity scene files and
EditorBuildSettings.asset for the SpaceShooter project.

GUIDs are derived from the asset path with md5 so they are stable across regeneration,
which lets the scene files and build settings reference scripts/scenes reliably.

Safe to re-run: existing .meta files are left untouched unless --force is passed.
"""
import hashlib
import os
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))
ASSETS = os.path.join(ROOT, "Assets")
PROJECT_SETTINGS = os.path.join(ROOT, "ProjectSettings")
FORCE = "--force" in sys.argv


def guid_for(rel_path: str) -> str:
    """Deterministic 32-char hex GUID for a project-relative asset path."""
    return hashlib.md5(rel_path.replace(os.sep, "/").encode("utf-8")).hexdigest()


def write(path: str, content: str):
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def script_meta(guid: str) -> str:
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


def folder_meta(guid: str) -> str:
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


def default_meta(guid: str) -> str:
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "DefaultImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def rel(path: str) -> str:
    return os.path.relpath(path, ROOT)


def ensure_meta_tree():
    """Create .meta files for every folder and file under Assets (except .meta themselves)."""
    for dirpath, dirnames, filenames in os.walk(ASSETS):
        dirnames.sort()
        filenames.sort()
        # Folder meta (skip the Assets root itself).
        if os.path.abspath(dirpath) != os.path.abspath(ASSETS):
            meta = dirpath + ".meta"
            if FORCE or not os.path.exists(meta):
                write(meta, folder_meta(guid_for(rel(dirpath))))
        for fn in filenames:
            if fn.endswith(".meta"):
                continue
            full = os.path.join(dirpath, fn)
            meta = full + ".meta"
            if not (FORCE or not os.path.exists(meta)):
                continue
            g = guid_for(rel(full))
            if fn.endswith(".cs"):
                write(meta, script_meta(g))
            elif fn.endswith(".unity"):
                write(meta, default_meta(g))
            else:
                write(meta, default_meta(g))


SCENE_TEMPLATE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {{r: 0.5, g: 0.5, b: 0.5, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.212, g: 0.227, b: 0.259, a: 1}}
  m_AmbientEquatorColor: {{r: 0.114, g: 0.125, b: 0.133, a: 1}}
  m_AmbientGroundColor: {{r: 0.047, g: 0.043, b: 0.035, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.478, b: 0.627, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 0}}
  m_IndirectSpecularColor: {{r: 0, g: 0, b: 0, a: 1}}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {{fileID: 0}}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {{fileID: 0}}
  m_LightingSettings: {{fileID: 0}}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {{fileID: 0}}
--- !u!1 &519420028
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 519420030}}
  - component: {{fileID: 519420029}}
  m_Layer: 0
  m_Name: {bootstrap_name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &519420030
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 519420028}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &519420029
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 519420028}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
"""


def write_scene(scene_path: str, bootstrap_name: str, script_rel_path: str):
    script_guid = guid_for(script_rel_path)
    content = SCENE_TEMPLATE.format(
        bootstrap_name=bootstrap_name,
        script_guid=script_guid,
    )
    write(scene_path, content)


def write_build_settings(gameplay_rel, mainmenu_rel):
    mainmenu_guid = guid_for(mainmenu_rel)
    gameplay_guid = guid_for(gameplay_rel)
    content = (
        "%YAML 1.1\n"
        "%TAG !u! tag:unity3d.com,2011:\n"
        "--- !u!1045 &1\n"
        "EditorBuildSettings:\n"
        "  m_ObjectHideFlags: 0\n"
        "  serializedVersion: 2\n"
        "  m_Scenes:\n"
        "  - enabled: 1\n"
        f"    path: {mainmenu_rel.replace(os.sep, '/')}\n"
        f"    guid: {mainmenu_guid}\n"
        "  - enabled: 1\n"
        f"    path: {gameplay_rel.replace(os.sep, '/')}\n"
        f"    guid: {gameplay_guid}\n"
        "  m_configObjects: {}\n"
    )
    write(os.path.join(PROJECT_SETTINGS, "EditorBuildSettings.asset"), content)


def main():
    # Scene files first so their .meta get generated in the tree pass.
    mainmenu_scene = os.path.join(ASSETS, "Scenes", "MainMenu.unity")
    gameplay_scene = os.path.join(ASSETS, "Scenes", "GamePlay.unity")

    write_scene(mainmenu_scene, "MainMenuBootstrap",
                "Assets/Scripts/Bootstrap/MainMenuBootstrap.cs")
    write_scene(gameplay_scene, "GameBootstrap",
                "Assets/Scripts/Bootstrap/GameBootstrap.cs")

    ensure_meta_tree()

    write_build_settings(
        rel(gameplay_scene),
        rel(mainmenu_scene),
    )

    print("Meta files, scenes and build settings generated.")
    print("MainMenu scene guid:", guid_for(rel(mainmenu_scene)))
    print("GamePlay scene guid:", guid_for(rel(gameplay_scene)))


if __name__ == "__main__":
    main()

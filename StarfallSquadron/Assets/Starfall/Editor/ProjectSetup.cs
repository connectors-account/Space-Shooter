using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starfall.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string Root="Assets/Starfall/";
        public const string ScenePath=Root+"Generated/Starfall.unity";
        private const string Stamp=Root+"Generated/setup-v1.txt";
        static ProjectSetup() { EditorApplication.delayCall+=AutoSetup; }
        private static void AutoSetup()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall+=AutoSetup; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(Stamp)) return;
            try
            {
                Scene current=SceneManager.GetActiveScene();
                bool open=current.IsValid() && string.IsNullOrEmpty(current.path) && !current.isDirty;
                Generate();
                if (open && !Application.isBatchMode) EditorSceneManager.OpenScene(ScenePath);
                Debug.Log("Starfall setup complete. Open Assets/Starfall/Generated/Starfall.unity, then press Play.");
            }
            catch (Exception e) { Debug.LogException(e); }
        }
        [MenuItem("Starfall/Open Game Scene")]
        public static void OpenGame()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(Stamp)) Generate();
            EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Starfall/Rebuild Generated Content")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before rebuilding.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Rebuild Starfall", "This replaces the generated scene, prefabs and GameAssets. Handwritten scripts and bundled art/audio are not changed.", "Rebuild", "Cancel")) return;
            Generate(); EditorSceneManager.OpenScene(ScenePath);
        }
        public static void Generate()
        {
            Directory.CreateDirectory(Root+"Generated/Prefabs");
            AssetDatabase.Refresh();
            ConfigureSprites(); ConfigureInput(); ConfigurePlayer();
            GameAssets assets=AssetDatabase.LoadAssetAtPath<GameAssets>(Root+"Generated/GameAssets.asset");
            if (assets==null) { assets=ScriptableObject.CreateInstance<GameAssets>(); AssetDatabase.CreateAsset(assets,Root+"Generated/GameAssets.asset"); }
            assets.player=Prefab("Player","player",1.25f,4);
            assets.enemies=new[] { Prefab("Scout","scout",1.05f,3),Prefab("Fighter","fighter",1.3f,3),Prefab("Gunship","gunship",1.65f,3),Prefab("Dreadnought","boss",3.3f,3) };
            assets.bolt=Prefab("Bolt","bolt",.38f,5);
            assets.pickups=new[] { Prefab("Repair","repair",.65f,6),Prefab("TripleShot","spread",.65f,6),Prefab("Shield","shield",.65f,6) };
            assets.particle=Load<Sprite>("Art/particle.png");
            assets.fire=Clip("fire"); assets.enemy=Clip("enemy"); assets.hit=Clip("hit"); assets.explosion=Clip("explosion");
            assets.pickup=Clip("pickup"); assets.wave=Clip("wave"); assets.win=Clip("win"); assets.lose=Clip("lose"); assets.ui=Clip("ui"); assets.ambient=Clip("ambient");
            EditorUtility.SetDirty(assets);
            Scene previous=SceneManager.GetActiveScene();
            Scene scene=SceneManager.GetSceneByPath(ScenePath);
            bool alreadyOpen=scene.IsValid() && scene.isLoaded;
            if (alreadyOpen)
            {
                foreach (GameObject root in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);
            }
            else scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); cameraObject.tag="MainCamera";
                var camera=cameraObject.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=5;
                camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.008f,.017f,.047f);
                camera.nearClipPlane=.1f; camera.farClipPlane=30; cameraObject.transform.position=new Vector3(0,0,-10);
                var gameObject=new GameObject("Starfall Game",typeof(FlightHud),typeof(FlightGame));
                var serialized=new SerializedObject(gameObject.GetComponent<FlightGame>());
                serialized.FindProperty("assets").objectReferenceValue=assets; serialized.ApplyModifiedPropertiesWithoutUndo();
                if (!EditorSceneManager.SaveScene(scene,ScenePath)) throw new IOException("Could not save game scene.");
            }
            finally
            {
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                if (!alreadyOpen) EditorSceneManager.CloseScene(scene,true);
            }
            EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(ScenePath,true) };
            AssetDatabase.SaveAssets();
            File.WriteAllText(Stamp,"Starfall setup version 1. Generated by ProjectSetup.cs.\n");
            AssetDatabase.Refresh();
        }
        private static T Load<T>(string relative) where T : UnityEngine.Object
        {
            T value=AssetDatabase.LoadAssetAtPath<T>(Root+relative);
            if (value==null) throw new FileNotFoundException("Required bundled asset missing: "+Root+relative);
            return value;
        }
        private static AudioClip Clip(string name) { return Load<AudioClip>("Audio/"+name+".wav"); }
        private static SpriteRenderer Prefab(string name,string image,float size,int order)
        {
            var go=new GameObject(name);
            try
            {
                var view=go.AddComponent<SpriteRenderer>(); view.sprite=Load<Sprite>("Art/"+image+".png"); view.sortingOrder=order;
                Vector3 dimensions=view.sprite.bounds.size;
                go.transform.localScale=Vector3.one*(size/Mathf.Max(dimensions.x,dimensions.y));
                GameObject saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"Generated/Prefabs/"+name+".prefab");
                if (saved==null) throw new IOException("Could not save prefab "+name);
                return saved.GetComponent<SpriteRenderer>();
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        private static void ConfigureSprites()
        {
            foreach (string path in Directory.GetFiles(Root+"Art","*.png"))
            {
                string normalized=path.Replace('\\','/');
                if (normalized.EndsWith("atlas.png")) continue;
                var importer=AssetImporter.GetAtPath(normalized) as TextureImporter;
                if (importer==null) throw new IOException("Sprite importer missing: "+path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=normalized.EndsWith("particle.png") ? 16 : 100;
                importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
                importer.filterMode=FilterMode.Bilinear; importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=512; importer.SaveAndReimport();
            }
        }
        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName="Starfall Studio"; PlayerSettings.productName="Starfall Squadron";
            PlayerSettings.bundleVersion="1.0.0";
            PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true; PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            // ProjectSettings.asset pins activeInputHandler=0 (Input Manager).
            QualitySettings.vSyncCount=1;
        }
        private static void ConfigureInput()
        {
            UnityEngine.Object[] objects=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            if (objects.Length==0) throw new FileNotFoundException("Missing ProjectSettings/InputManager.asset");
            var settings=new SerializedObject(objects[0]); var axes=settings.FindProperty("m_Axes");
            AddAxis(axes,"PadX",0,false); AddAxis(axes,"PadY",1,true);
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void AddAxis(SerializedProperty axes,string name,int axis,bool invert)
        {
            SerializedProperty entry=null;
            for (int i=0;i<axes.arraySize;i++)
            {
                var item=axes.GetArrayElementAtIndex(i);
                if (item.FindPropertyRelative("m_Name").stringValue==name) { entry=item; break; }
            }
            if (entry==null) { axes.arraySize++; entry=axes.GetArrayElementAtIndex(axes.arraySize-1); }
            entry.FindPropertyRelative("m_Name").stringValue=name;
            foreach (string field in new[] { "descriptiveName","descriptiveNegativeName","negativeButton","positiveButton","altNegativeButton","altPositiveButton" }) entry.FindPropertyRelative(field).stringValue="";
            entry.FindPropertyRelative("gravity").floatValue=0;
            entry.FindPropertyRelative("dead").floatValue=.2f;
            entry.FindPropertyRelative("sensitivity").floatValue=1;
            entry.FindPropertyRelative("snap").boolValue=false;
            entry.FindPropertyRelative("invert").boolValue=invert;
            entry.FindPropertyRelative("type").intValue=2;
            entry.FindPropertyRelative("axis").intValue=axis;
            entry.FindPropertyRelative("joyNum").intValue=0;
        }
        [MenuItem("Starfall/Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before building.");
            if (!File.Exists(Stamp)) Generate();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Install Windows build support for Unity 6000.0.62f1 in Unity Hub.");
            ConfigurePlayer();
            Directory.CreateDirectory("Builds/Windows");
            var options=new BuildPlayerOptions { scenes=new[] { ScenePath }, locationPathName="Builds/Windows/Starfall.exe", target=BuildTarget.StandaloneWindows64, options=BuildOptions.None };
            BuildReport report=BuildPipeline.BuildPlayer(options);
            if (report.summary.result!=BuildResult.Succeeded) throw new BuildFailedException("Windows build failed: "+report.summary.result);
            Debug.Log("Windows x64 build saved to Builds/Windows/Starfall.exe. Distribute the entire Windows folder.");
        }
    }
}

// SOURCE CHECK ONLY. These signatures are not Unity and do not simulate gameplay.
// Kept OUTSIDE Assets so Unity never imports or builds this file.
using System;
namespace UnityEngine {
 public class Object { public string name; public static implicit operator bool(Object o)=>o!=null; public static T Instantiate<T>(T o,Vector3 p,Quaternion q,Transform t)=>o; public static void Destroy(Object o){} public static void DestroyImmediate(Object o){} }
 public class Component:Object { public GameObject gameObject; public Transform transform; public T GetComponent<T>()=>default(T); }
 public class Behaviour:Component { public bool enabled; }
 public class MonoBehaviour:Behaviour {}
 public class GameObject:Object { public Transform transform; public int layer; public string tag; public GameObject(string n){name=n;} public T AddComponent<T>() where T:Component=>default(T); public T GetComponent<T>()=>default(T); public T[] GetComponentsInChildren<T>(bool b)=>new T[0]; public void SetActive(bool b){} }
 public class Transform:Component { public Vector3 position,localPosition,localScale; public Quaternion rotation; public int childCount; public Transform GetChild(int i)=>null; public void SetParent(Transform t,bool b){} }
 public class RequireComponent:Attribute { public RequireComponent(Type a){} public RequireComponent(Type a,Type b){} public RequireComponent(Type a,Type b,Type c){} }
 public struct Vector2 { public float x,y; public Vector2(float x,float y){this.x=x;this.y=y;} public static Vector2 zero=>new Vector2(); public static Vector2 down=>new Vector2(0,-1); public Vector2 normalized=>this; public float magnitude=>0; public static Vector2 ClampMagnitude(Vector2 v,float f)=>v; public static Vector2 operator +(Vector2 a,Vector2 b)=>a; public static Vector2 operator *(Vector2 a,float b)=>a; public static implicit operator Vector3(Vector2 a)=>new Vector3(a.x,a.y,0); }
 public struct Vector3 { public float x,y,z; public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;} public static Vector3 one=>new Vector3(1,1,1); public static Vector3 up=>new Vector3(0,1,0); public static Vector3 down=>new Vector3(0,-1,0); public static Vector3 operator +(Vector3 a,Vector3 b)=>a; public static Vector3 operator *(Vector3 a,float b)=>a; public static implicit operator Vector2(Vector3 a)=>new Vector2(a.x,a.y); }
 public struct Quaternion { public static Quaternion identity=>new Quaternion(); public static Quaternion Euler(float x,float y,float z)=>identity; }
 public struct Color { public float r,g,b,a; public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;} public static Color white=>new Color(); public static Color cyan=>new Color(); public static Color green=>new Color(); public static Color yellow=>new Color(); }
 public static class Mathf { public const float PI=(float)Math.PI,Deg2Rad=PI/180,Rad2Deg=180/PI; public static float Sin(float v)=>(float)Math.Sin(v); public static float Cos(float v)=>(float)Math.Cos(v); public static float Atan2(float x,float y)=>(float)Math.Atan2(x,y); public static float Abs(float v)=>Math.Abs(v); public static float Min(float a,float b)=>Math.Min(a,b); public static int Min(int a,int b)=>Math.Min(a,b); public static float Max(float a,float b)=>Math.Max(a,b); public static int Max(int a,int b)=>Math.Max(a,b); public static float Clamp(float a,float lo,float hi)=>Math.Min(hi,Math.Max(lo,a)); }
 public static class Time { public static float time,deltaTime,fixedDeltaTime,timeScale; }
 public static class Application { public static int targetFrameRate; public static bool isBatchMode; public static void Quit(){} }
 public static class PlayerPrefs { public static int GetInt(string k,int d)=>d; public static void SetInt(string k,int v){} public static void Save(){} }
 public enum KeyCode { M,Escape,P,Return,R,D,RightArrow,A,LeftArrow,W,UpArrow,S,DownArrow,Space }
 public static class Input { public static bool GetKeyDown(KeyCode k)=>false; public static bool GetKey(KeyCode k)=>false; }
 public class Sprite:Object {} public class SpriteRenderer:Behaviour { public Sprite sprite; public Color color; public int sortingOrder; }
 public enum RigidbodyType2D { Kinematic } public enum RigidbodyInterpolation2D { Interpolate }
 public class Rigidbody2D:Component { public Vector2 position; public float gravityScale; public bool useFullKinematicContacts; public RigidbodyType2D bodyType; public RigidbodyInterpolation2D interpolation; public void MovePosition(Vector2 p){} }
 public class Collider2D:Behaviour { public bool isTrigger; } public class CircleCollider2D:Collider2D { public float radius; }
 public struct ContactFilter2D { public bool useTriggers; public void SetLayerMask(LayerMask m){} }
 public struct LayerMask { public static implicit operator LayerMask(int i)=>new LayerMask(); public static int NameToLayer(string n)=>0; }
 public struct RaycastHit2D { public Collider2D collider; }
 public static class Physics2D { public static Vector2 gravity; public static int CircleCast(Vector2 p,float r,Vector2 d,ContactFilter2D f,RaycastHit2D[] h,float l)=>0; }
 public class AudioClip:Object { public float length; } public class AudioListener:Behaviour {}
 public class AudioSource:Behaviour { public bool playOnAwake,ignoreListenerPause,mute; public float spatialBlend,volume; public AudioClip clip; public void Play(){} }
 public enum CameraClearFlags { SolidColor } public class Camera:Behaviour { public float aspect,orthographicSize; public bool orthographic; public Rect rect; public CameraClearFlags clearFlags; public Color backgroundColor; }
 public struct Rect { public Rect(float x,float y,float w,float h){} }
 public struct Matrix4x4 { public static Matrix4x4 TRS(Vector3 a,Quaternion b,Vector3 c)=>new Matrix4x4(); }
 public class Texture2D:Object { public static Texture2D whiteTexture; }
 public enum FontStyle { Bold } public enum TextAnchor { MiddleCenter } public enum FilterMode { Bilinear }
 public class RectOffset { public RectOffset(int a,int b,int c,int d){} }
 public class GUIStyleState { public Color textColor; } public class GUIStyle { public GUIStyle(GUIStyle s){} public int fontSize; public FontStyle fontStyle; public RectOffset padding; public TextAnchor alignment; public GUIStyleState normal=new GUIStyleState(); }
 public class GUISkin { public GUIStyle button,label; }
 public static class GUI { public static Matrix4x4 matrix; public static Color color; public static GUISkin skin; public static void Label(Rect r,string s,GUIStyle g){} public static bool Button(Rect r,string s,GUIStyle g)=>false; public static void DrawTexture(Rect r,Texture2D t){} }
 public static class Screen { public static int width,height; } public enum FullScreenMode { Windowed } public enum ColorSpace { Gamma }
 public static class QualitySettings { public static int vSyncCount; } public static class Debug { public static void Log(object o){} public static void LogWarning(object o){} public static void LogException(Exception e){} }
}
namespace UnityEngine.SceneManagement { public struct Scene { public bool isDirty,isLoaded; public UnityEngine.GameObject[] GetRootGameObjects()=>new UnityEngine.GameObject[0]; } public static class SceneManager { public static Scene GetActiveScene()=>new Scene(); public static Scene GetSceneByPath(string p)=>new Scene(); } }
namespace UnityEditor {
 using UnityEngine;
 public class InitializeOnLoadAttribute:Attribute {} public class MenuItem:Attribute { public MenuItem(string s){} }
 public static class EditorApplication { public static Action delayCall; public static bool isPlayingOrWillChangePlaymode,isCompiling,isUpdating,isPlaying; }
 public static class SessionState { public static bool GetBool(string k,bool v)=>v; public static void SetBool(string k,bool v){} }
 public static class EditorUtility { public static bool DisplayDialog(string a,string b,string c,string d)=>true; }
 public static class AssetDatabase { public static void Refresh(){} public static void SaveAssets(){} public static T LoadAssetAtPath<T>(string s) where T:UnityEngine.Object=>default(T); public static UnityEngine.Object[] LoadAllAssetsAtPath(string s)=>new UnityEngine.Object[0]; }
 public static class PlayerSettings { public static string companyName,productName,bundleVersion; public static int defaultScreenWidth,defaultScreenHeight; public static bool resizableWindow,runInBackground; public static FullScreenMode fullScreenMode; public static ColorSpace colorSpace; public static void SetScriptingBackend(Build.NamedBuildTarget n,ScriptingImplementation s){} }
 public enum ScriptingImplementation { Mono2x }
 public class SerializedObject { public SerializedObject(UnityEngine.Object o){} public SerializedProperty FindProperty(string p)=>new SerializedProperty(); public void ApplyModifiedPropertiesWithoutUndo(){} }
 public class SerializedProperty { public int intValue; public string stringValue; public SerializedProperty GetArrayElementAtIndex(int i)=>this; }
 public class AssetImporter { public static AssetImporter GetAtPath(string p)=>null; }
 public enum TextureImporterType { Sprite } public enum SpriteImportMode { Single } public enum TextureImporterCompression { Uncompressed }
 public class TextureImporter:AssetImporter { public TextureImporterType textureType; public SpriteImportMode spriteImportMode; public float spritePixelsPerUnit; public bool alphaIsTransparency,mipmapEnabled; public FilterMode filterMode; public TextureImporterCompression textureCompression; public void SaveAndReimport(){} }
 public static class PrefabUtility { public static GameObject SaveAsPrefabAsset(GameObject o,string p)=>o; }
 public class EditorBuildSettingsScene { public EditorBuildSettingsScene(string p,bool b){} } public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
 public static class GameObjectUtility { public static int GetMonoBehavioursWithMissingScriptCount(GameObject g)=>0; }
 public enum BuildTarget { StandaloneWindows64 } public enum BuildTargetGroup { Standalone } public enum BuildOptions { None }
 public struct BuildPlayerOptions { public string[] scenes; public string locationPathName; public BuildTarget target; public BuildOptions options; }
 public static class BuildPipeline { public static bool IsBuildTargetSupported(BuildTargetGroup g,BuildTarget t)=>true; public static Build.Reporting.BuildReport BuildPlayer(BuildPlayerOptions o)=>null; }
 public static class EditorUserBuildSettings { public static BuildTarget activeBuildTarget; }
}
namespace UnityEditor.Build { public struct NamedBuildTarget { public static NamedBuildTarget Standalone; } }
namespace UnityEditor.Build.Reporting { public enum BuildResult { Succeeded } public class BuildSummary { public BuildResult result; } public class BuildReport { public BuildSummary summary; } }
namespace UnityEditor.SceneManagement {
 using UnityEngine.SceneManagement;
 public enum NewSceneSetup { EmptyScene } public enum NewSceneMode { Single } public enum OpenSceneMode { Additive }
 public static class EditorSceneManager { public static bool SaveCurrentModifiedScenesIfUserWantsTo()=>true; public static Scene NewScene(NewSceneSetup a,NewSceneMode b)=>new Scene(); public static bool SaveScene(Scene s,string p)=>true; public static Scene OpenScene(string p,OpenSceneMode m)=>new Scene(); public static bool CloseScene(Scene s,bool b)=>true; }
}

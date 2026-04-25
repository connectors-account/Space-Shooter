using UnityEngine;

namespace SpaceShooter.Core
{
    public static class SceneAutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            var bootstrapObject = GameObject.Find("RuntimeBootstrap");
            if (bootstrapObject == null)
            {
                bootstrapObject = new GameObject("RuntimeBootstrap");
                bootstrapObject.AddComponent<RuntimeBootstrap>();
            }
        }
    }
}

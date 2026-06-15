#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UniverseSceneCreator
{
    [MenuItem("Universe Sandbox Lite/Create Demo Scene")]
    public static void CreateDemoScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("Universe Sandbox Lite");
        root.AddComponent<UniverseSimulation>();
        root.AddComponent<UniverseHUD>();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 65f;
        camera.farClipPlane = 5000f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.005f, 0.007f, 0.015f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraController>();
        cameraObject.transform.position = new Vector3(0f, 24f, -48f);
        cameraObject.transform.LookAt(Vector3.zero);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.07f, 0.08f, 0.12f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/UniverseSandboxLite.unity");
        AssetDatabase.Refresh();

        Debug.Log("Universe Sandbox Lite demo scene created at Assets/Scenes/UniverseSandboxLite.unity");
    }
}
#endif

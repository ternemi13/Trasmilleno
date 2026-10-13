using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.AI;
using Object = UnityEngine.Object;
namespace Transmilleno.Editor
{
    [InitializeOnLoad]
    public static class QualityTools
    {
        static TestRunnerApi runner;
        static QualityTools()
        {
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results());
        }
        [MenuItem("Tools/Transmilleno/QA/Run EditMode Tests")]
        public static void EditTests()
        {
            SessionState.SetString("Transmilleno.TestPath", "Docs/editmode-results.xml");
            runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "Transmilleno.EditTests" } }));
        }
        [MenuItem("Tools/Transmilleno/QA/Run PlayMode Tests")]
        public static void PlayTests()
        {
            SessionState.SetString("Transmilleno.TestPath", "Docs/playmode-results.xml");
            runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode, assemblyNames = new[] { "Transmilleno.PlayTests" } }));
        }
        sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor t)
            {
            }
            public void TestStarted(ITestAdaptor t)
            {
            }
            public void TestFinished(ITestResultAdaptor r)
            {
                if (r.Test.IsSuite)
                    return;
                Debug.Log("[QA] " + r.Test.Name + " " + r.ResultState + " " + r.Message);
            }
            public void RunFinished(ITestResultAdaptor r)
            {
                var path = SessionState.GetString("Transmilleno.TestPath", "Docs/test-results.xml");
                TestRunnerApi.SaveResultToFile(r, path);
                Debug.Log("[QA] Passed=" + r.PassCount + " Failed=" + r.FailCount + " Saved=" + path);
            }
        }
        [MenuItem("Tools/Transmilleno/QA/Capture Models")]
        public static void CaptureModels()
        {
            var config = Resources.Load<GameProgressionConfig>("Progression");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(.6f, .65f, .7f);
            var light = new GameObject("Photo key").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.8f;
            light.transform.rotation = Quaternion.Euler(35, -35, 0);
            foreach (var entry in config.catalog.entries)
            {
                var model = Object.Instantiate(entry.model);
                var renderers = model.GetComponentsInChildren<Renderer>();
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers)
                    b.Encapsulate(r.bounds);
                var cam = new GameObject("Photo camera").AddComponent<Camera>();
                cam.backgroundColor = new Color(.12f, .16f, .19f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.transform.position = b.center + new Vector3(.8f, .3f, 1) * b.size.magnitude;
                cam.transform.LookAt(b.center);
                Capture(cam, "Docs/Captures/model-" + entry.key + ".png", 1000, 800);
                Object.DestroyImmediate(cam.gameObject);
                Object.DestroyImmediate(model);
            }
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Boot.unity");
        }
        [MenuItem("Tools/Transmilleno/QA/Capture and Validate Levels")]
        public static void CaptureLevels()
        {
            var config = Resources.Load<GameProgressionConfig>("Progression");
            var report = new System.Text.StringBuilder("# Route validation\n\n");
            foreach (var level in config.levels)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var world = LevelWorld.Create(level, config.catalog, false);
                var cam = new GameObject("Overview").AddComponent<Camera>();
                cam.backgroundColor = RenderSettings.fogColor;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.transform.position = new Vector3(-17, 12, -24);
                cam.transform.LookAt(new Vector3(0, 0, 2));
                Capture(cam, "Docs/Captures/level-" + level.index.ToString("00") + ".png", 1280, 800);
                int reachable = 0;
                foreach (var route in level.routes)
                {
                    Vector3 landing = route.x < 0 ? new Vector3(-2.2f, .4f, route.z) : new Vector3(2.3f, .4f, route.z);
                    if (NavMesh.SamplePosition(landing, out var a, 2, NavMesh.AllAreas) && NavMesh.SamplePosition(new Vector3(2, .4f, 5.5f), out var b, 2, NavMesh.AllAreas))
                    {
                        var path = new NavMeshPath();
                        if (NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                            reachable++;
                    }
                }
                report.AppendLine("- Level " + level.index + " " + level.station + ": " + reachable + "/3 landing-to-boarding NavMesh routes.");
                if (reachable < 2)
                    Debug.LogError("[Routes] Insufficient routes at " + level.station);
            }
            File.WriteAllText("Docs/ROUTE-VALIDATION.md", report.ToString());
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Boot.unity");
        }
        public static void Capture(Camera cam, string path, int width, int height)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var old = cam.targetTexture;
            var active = RenderTexture.active;
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            cam.targetTexture = old;
            RenderTexture.active = active;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
        }
    }
}

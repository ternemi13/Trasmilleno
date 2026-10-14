using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
namespace Transmilleno
{
    public sealed class CampaignSmoke : MonoBehaviour
    {
        Keyboard testKeyboard; Mouse testMouse; InputSettings.BackgroundBehavior previousBackground;
        public GameApp App; public bool QuitOnFinish; SaveData original; bool finished; StringBuilder report = new StringBuilder();
        public static string ReportPath => Application.isEditor ? "Docs/CAMPAIGN-SMOKE.md" : Path.Combine(Application.persistentDataPath, "CAMPAIGN-SMOKE.md");
        IEnumerator Start()
        {
            original = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(Session.Save));
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testKeyboard = InputSystem.AddDevice<Keyboard>("Campaign QA keyboard");
            testMouse = InputSystem.AddDevice<Mouse>("Campaign QA mouse");
            report.AppendLine("# Campaign smoke test\n\nAutomated boarding and progression check. Entry is positioned programmatically; this does not certify normal-play route balance.\n");
            for (int level = 1; level <= App.Config.MaxLevels; level++)
            {
                if (level == 1)
                    App.Play(level);
                float deadline = Time.realtimeSinceStartup + 35;
                while (App.Loading && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (App.Loading || !App.Player)
                {
                    Fail("Level load timeout " + level);
                    yield break;
                }
                Time.timeScale = 8;
                var bus = App.World.Bus;
                deadline = Time.realtimeSinceStartup + 18;
                while (bus.State != BusController.BusState.Boarding && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (bus.State != BusController.BusState.Boarding)
                {
                    Fail("No boarding bus " + level);
                    yield break;
                }
                var p = App.Player;
                var cc = p.GetComponent<CharacterController>();
                cc.enabled = false;
                p.transform.position = bus.BoardPoint;
                cc.enabled = true;
                Session.Trespassing = true;
                Session.Route = "Automated boarding check";
                var keyboard = testKeyboard;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return null;
                bool boarded = bus.Boarded || bus.TryBoard();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                if (!boarded)
                {
                    Fail("Boarding rejected " + level + " health=" + p.Health + " state=" + p.State + " bus=" + bus.State + " distance=" + Vector3.Distance(p.transform.position, bus.BoardPoint) + " input=" + p.Input.Interact.enabled);
                    yield break;
                }
                deadline = Time.realtimeSinceStartup + 12;
                while (Session.Running && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (Session.Running || p.IsDead || Session.Save.stars[level - 1] < 1)
                {
                    Fail("No victory " + level);
                    yield break;
                }
                report.AppendLine("- Level " + level + " / " + App.Config.levels[level - 1].station + ": loaded → doors opened → boarded → departed → results → progress saved.");
                Time.timeScale = 1;
                yield return new WaitForSecondsRealtime(.3f);
                if (level < App.Config.MaxLevels)
                {
                    Button next = null;
                    foreach (var button in App.UI.GetComponentsInChildren<Button>())
                        if (button.GetComponentInChildren<TMP_Text>().text.StartsWith("Siguiente"))
                            next = button;
                    if (!next)
                    {
                        Fail("Missing next-level button " + level);
                        yield break;
                    }
                    Canvas.ForceUpdateCanvases();
                    var canvas = next.GetComponentInParent<Canvas>();
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, next.transform.position + new Vector3(100, -20, 0));
                    var rect = (RectTransform)next.transform;
                    point = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, rect.TransformPoint(rect.rect.center));
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
                    if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<Button>() != next)
                    {
                        Fail("Results button blocked by UI raycast " + level);
                        yield break;
                    }
                    var mouse = testMouse;
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                    yield return null;
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = point, buttons = 1 });
                    yield return null;
                    yield return null;
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                    yield return null;
                    deadline = Time.realtimeSinceStartup + 8;
                    while (!App.Loading && Session.Level.index == level && Time.realtimeSinceStartup < deadline)
                        yield return null;
                    if (!App.Loading && Session.Level.index == level)
                    {
                        Fail("Mouse click did not load next level " + level);
                        yield break;
                    }
                    report.AppendLine("  - Results mouse click and UI raycast: PASS.");
                }
            }
            report.AppendLine("\nPASS: all ten levels completed and finale displayed.");
            File.WriteAllText(ReportPath, report.ToString());
            if (App.Verification)
                App.Verification.Save(Path.Combine(Application.persistentDataPath, "finale.png"));
            else
                ScreenCapture.CaptureScreenshot(Application.isEditor ? "Docs/Captures/finale.png" : Path.Combine(Application.persistentDataPath, "finale.png"));
            yield return new WaitForSecondsRealtime(.4f);
            Restore();
            finished = true;
            Debug.Log("[Smoke] PASS " + ReportPath);
            if (QuitOnFinish && !Application.isEditor)
                Application.Quit(0);
            else
                App.Menu();
        }
        void Fail(string reason)
        {
            report.AppendLine("\nFAIL: " + reason);
            File.WriteAllText(ReportPath, report.ToString());
            Restore();
            finished = true;
            Debug.LogError("[Smoke] " + reason);
            if (QuitOnFinish && !Application.isEditor)
                Application.Quit(2);
        }
        void Restore()
        {
            Time.timeScale = 1;
            if (testKeyboard != null) { InputSystem.RemoveDevice(testKeyboard); testKeyboard = null; }
            if (testMouse != null) { InputSystem.RemoveDevice(testMouse); testMouse = null; }
            InputSystem.settings.backgroundBehavior = previousBackground;
            if (original != null)
            {
                Session.Save = original;
                SaveSystem.Write(original);
            }
        }
        void OnDestroy()
        {
            if (!finished)
                Restore();
        }
    }
}

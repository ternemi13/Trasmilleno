using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Transmilleno
{
    public sealed class CampaignSmoke : MonoBehaviour
    {
        public GameApp App; public bool QuitOnFinish; SaveData original; bool finished; StringBuilder report = new StringBuilder();
        public static string ReportPath => Application.isEditor ? "Docs/CAMPAIGN-SMOKE.md" : Path.Combine(Application.persistentDataPath, "CAMPAIGN-SMOKE.md");
        IEnumerator Start()
        {
            original = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(Session.Save));
            report.AppendLine("# Campaign smoke test\n\nAutomated boarding and progression check. Entry is positioned programmatically; this does not certify normal-play route balance.\n");
            for (int level = 1; level <= App.Config.MaxLevels; level++)
            {
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
                p.transform.position = bus.Stop + new Vector3(-2.7f, .4f, 1.5f);
                cc.enabled = true;
                Session.Trespassing = true;
                Session.Route = "Automated boarding check";
                var keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                InputSystem.Update();
                bool boarded = bus.TryBoard();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                if (!boarded)
                {
                    Fail("Boarding rejected " + level);
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
                yield return new WaitForSecondsRealtime(.2f);
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

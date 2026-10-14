using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
namespace Transmilleno
{
    public sealed class PerformanceProbe : MonoBehaviour
    {
        readonly float[] samples = new float[3600]; int count; float warmup = 5; bool written;
        void Update()
        {
            if (!Session.Running || written)
                return;
            if (warmup > 0)
            {
                warmup -= Time.unscaledDeltaTime;
                return;
            }
            samples[count++] = Time.unscaledDeltaTime * 1000;
            if (count < samples.Length)
                return;
            Array.Sort(samples);
            double sum = 0;
            foreach (float s in samples)
                sum += s;
            string report = "# Performance measurement\n\n" + DateTime.UtcNow.ToString("u") + " UTC\n\n- Environment: " + (Application.isEditor ? "Unity Editor" : "Windows player") + "\n- GPU: " + SystemInfo.graphicsDeviceName + "\n- CPU: " + SystemInfo.processorType + "\n- Resolution: " + Screen.width + " × " + Screen.height + "\n- Level: " + Session.Level.index + "\n- Frames: " + count + "\n- Mean: " + (sum / count).ToString("F2") + " ms (" + (1000 / (sum / count)).ToString("F1") + " FPS)\n- P95: " + samples[(int)(count * .95f)].ToString("F2") + " ms\n- Slowest frame: " + samples[count - 1].ToString("F2") + " ms (" + (1000 / samples[count - 1]).ToString("F1") + " FPS)\n- P99: " + samples[(int)(count * .99f)].ToString("F2") + " ms\n- Memory: " + (Profiler.GetTotalAllocatedMemoryLong() / 1048576) + " MiB\n\nEditor samples include editor overhead. Hidden Windows QA renders the full scene and UI at 1920 x 1080 into a RenderTexture and performs a synchronous GPU readback each frame; this adds capture overhead. Measurements certify only this machine and route.\n";
            string path = Application.isEditor ? "Docs/PERFORMANCE.md" : Path.Combine(Application.persistentDataPath, "PERFORMANCE.md");
            File.WriteAllText(path, report);
            written = true;
            Debug.Log("[Performance] Report saved: " + path);
            if (!Application.isEditor && Array.IndexOf(Environment.GetCommandLineArgs(), "--benchmark") >= 0)
                StartCoroutine(FinishBenchmark());
        }
        System.Collections.IEnumerator FinishBenchmark()
        {
            var app = GetComponent<GameApp>();
            if (app.Verification)
                app.Verification.Save(Path.Combine(Application.persistentDataPath, "benchmark.png"));
            else
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.persistentDataPath, "benchmark.png"));
            yield return new WaitForSecondsRealtime(.6f);
            Application.Quit(0);
        }
    }
}

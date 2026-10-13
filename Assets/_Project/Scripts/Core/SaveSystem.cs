using System;
using System.IO;
using UnityEngine;
namespace Transmilleno
{
    [Serializable] public class SaveData
    {
        public int version = 1, unlocked = 1, current = 1; public int[] stars = new int[10]; public float[] bestTimes = new float[10];
        public float master = .8f, music = .4f, effects = .8f, ambience = .5f, sensitivity = 1, shoulder=.45f;
        public int quality = 1, difficulty = 1, fps = 60, resolution = 1, windowMode; public bool invertY, vsync = true, hints = true, subtitles = true, post = true;
        public float renderScale = 1, drawDistance = 160, crowd = 1; public string bindings = "", language = "es-CO";
    }
    public static class SaveSystem
    {
        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, "progress-v1.json");
        public static SaveData Load(string path = null)
        {
            path ??= Path;
            foreach (var candidate in new[] { path, path + ".bak" })
            {
                try { if (!File.Exists(candidate)) continue; var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(candidate)); if (d == null || d.version != 1 || d.stars == null || d.stars.Length != 10 || d.bestTimes == null || d.bestTimes.Length != 10) continue; d.unlocked = Mathf.Clamp(d.unlocked, 1, 10); d.current = Mathf.Clamp(d.current, 1, d.unlocked); return d; }
                catch (Exception e) { Debug.LogWarning("[Save] Recuperación: " + e.Message); }
            }
            return new SaveData();
        }
        public static bool Write(SaveData data, string path = null)
        {
            path ??= Path;
            try { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)); File.WriteAllText(path + ".tmp", JsonUtility.ToJson(data, true)); if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak"); else File.Move(path + ".tmp", path); return true; }
            catch (Exception e) { Debug.LogWarning("[Save] " + e.Message); GameEvents.Notify("No se pudo guardar. Revisa el espacio disponible."); return false; }
        }
    }
}

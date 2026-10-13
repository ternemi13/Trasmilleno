using System;
using UnityEngine;

namespace Transmilleno
{
    [Serializable] public class CatalogEntry { public string key; public GameObject model; public float height = 1.75f; public Vector3 rotation; }






    public enum DamageType { Punch, Knife, Projectile, Baton, Bullet, Bus, Fall }
    public struct DamageInfo { public float amount; public DamageType type; public GameObject source; public Vector3 knockback; public DamageInfo(float amount, DamageType type, GameObject source, Vector3 force = default) { this.amount = amount; this.type = type; this.source = source; knockback = force; } }
    public interface IDamageable { void TakeDamage(DamageInfo damage); }

    public static class GameEvents
    {
        public static event Action<string> Message; public static event Action<Vector3, float> Noise;
        public static event Action<string> PlayerDied; public static event Action LevelCompleted;
        public static event Action<float> PlayerHurt;public static event Action<Vector3> Impact;public static void Hit(Vector3 position)=>Impact?.Invoke(position);
        public static void Notify(string text) => Message?.Invoke(text);
        public static void EmitNoise(Vector3 p, float radius) => Noise?.Invoke(p, radius);
        public static void Die(string reason) => PlayerDied?.Invoke(reason);
        public static void Complete() => LevelCompleted?.Invoke();
        public static void Hurt(float amount) => PlayerHurt?.Invoke(amount);
    }
    public sealed class Stamina
    {
        public float Value { get; private set; } = 100; float delay;
        public bool Spend(float value) { if (Value < value) return false; Value = Mathf.Max(0, Value - value); delay = 1; return true; }
        public void Tick(float delta, float multiplier = 1) { if (delay > 0) delay -= delta; else Value = Mathf.Min(100, Value + delta * 21 * multiplier); }
    }
    public static class Session
    {
        public static GameProgressionConfig Config;public static DifficultySettings Difficulty => Config&&Config.difficulties!=null&&Save!=null?Config.difficulties[Mathf.Clamp(Save.difficulty,0,Config.difficulties.Length-1)]:null;public static Actor Player; public static bool Running, Trespassing; public static float Alert, Elapsed;
        public static int Hits, Alerts; public static string Route = "Sin registrar";
        public static LevelDefinition Level; public static SaveData Save;
        public static void RaiseAlert(float value) { if (Alert < 1 && value > 0) Alerts++; Alert = Mathf.Clamp(Alert + value, 0, 3); }
        public static void Reset(LevelDefinition level) { Level = level; Alert = Elapsed = 0; Hits = Alerts = 0; Trespassing = false; Route = "Sin registrar"; Player = null; Running = false; }
    }
}

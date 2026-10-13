using UnityEngine;
namespace Transmilleno
{
    public abstract class Actor : MonoBehaviour, IDamageable
    {
        public float Health { get; protected set; } = 100; public bool IsDead => Health <= 0;
        public abstract void TakeDamage(DamageInfo damage);
    }
}

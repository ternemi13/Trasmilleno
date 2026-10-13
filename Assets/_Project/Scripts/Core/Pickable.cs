using UnityEngine;
namespace Transmilleno
{
    public class Pickable : MonoBehaviour
    {
        public bool knife; public float damage = 24; public Rigidbody Body
        {
            get; private set;
        }
        public bool Flying;
        float life; Vector3 origin; Transform originParent; GameObject owner; Collider hitCollider;
        void Awake()
        {
            origin = transform.position;
            originParent = transform.parent;
            Body = GetComponent<Rigidbody>();
            hitCollider = GetComponent<Collider>();
        }
        public void Hold(Transform socket)
        {
            Flying = false;
            life = 0;
            if (Body)
            {
                Body.isKinematic = true;
                Body.detectCollisions = false;
            }
            if (hitCollider)
                hitCollider.enabled = false;
            transform.SetParent(socket);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        public void Throw(Vector3 velocity, GameObject source)
        {
            owner = source;
            transform.SetParent(null);
            Body.isKinematic = false;
            Body.detectCollisions = true;
            hitCollider.enabled = true;
            Body.linearVelocity = velocity;
            Flying = true;
            life = 12;
        }
        void Update()
        {
            if (life > 0 && (life -= Time.deltaTime) <= 0)
            {
                Flying = false;
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
                transform.SetParent(originParent);
                transform.position = origin;
            }
        }
        void OnCollisionEnter(Collision c)
        {
            if (!Flying || c.gameObject == owner)
                return;
            Flying = false;
            var actor = c.collider.GetComponentInParent<Actor>();
            if (actor)
                actor.TakeDamage(new DamageInfo(damage + Mathf.Min(16, c.relativeVelocity.magnitude), DamageType.Projectile, owner));
            GameEvents.EmitNoise(transform.position, 18);
            GameEvents.Notify("Impacto: los guardias investigan el ruido");
        }
    }
}

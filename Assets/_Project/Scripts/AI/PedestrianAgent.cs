using UnityEngine;
using UnityEngine.AI;
namespace Transmilleno
{
    public sealed class PedestrianAgent : Actor
    {
        NavMeshAgent agent; PoseDriver pose; Vector3 home; float next; float panic; bool approaching, onBus; Vector3 door; Renderer[] visuals;
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            pose = GetComponentInChildren<PoseDriver>();
            home = transform.position;
            next = Time.time + Random.value;
            GameEvents.Noise += Hear;
            GameEvents.BusDoors += Doors;
            visuals = GetComponentsInChildren<Renderer>();
        }
        void OnDestroy()
        {
            GameEvents.Noise -= Hear;
            GameEvents.BusDoors -= Doors;
        }
        void Hear(Vector3 p, float r)
        {
            if (Vector3.Distance(p, transform.position) < r)
                panic = Time.time + 5;
        }
        void Update()
        {
            if (!Session.Running || IsDead || !agent || !agent.isOnNavMesh || onBus)
                return;
            if (pose)
                pose.Speed = agent.velocity.magnitude;
            if (approaching)
            {
                if (Vector3.Distance(transform.position, door) < 1.7f)
                {
                    onBus = true;
                    approaching = false;
                    agent.isStopped = true;
                    foreach (var r in visuals)
                        r.enabled = false;
                }
                return;
            }
            if (Time.time < next)
                return;
            next = Time.time + .6f;
            agent.speed = panic > Time.time ? 3.8f : 1.1f;
            if (!agent.hasPath || agent.remainingDistance < 1 || panic > Time.time)
            {
                var p = home + new Vector3(Random.Range(-2, 2), 0, Random.Range(-12, 12));
                if (panic > Time.time && Session.Player)
                    p = transform.position + (transform.position - Session.Player.transform.position).normalized * 8;
                if (NavMesh.SamplePosition(p, out var h, 1, NavMesh.AllAreas))
                    agent.SetDestination(h.position);
            }
        }
        void Doors(Vector3 point, bool open)
        {
            if (IsDead || !agent || !agent.isOnNavMesh)
                return;
            if (!open)
            {
                approaching = false;
                return;
            }
            if (onBus)
            {
                onBus = false;
                agent.isStopped = false;
                agent.Warp(point + Vector3.back * 1.5f);
                foreach (var r in visuals)
                    r.enabled = true;
                agent.SetDestination(home);
                return;
            }
            if (StationLayout.Inside(transform.position) && Vector3.Distance(transform.position, point) < 9)
            {
                door = point;
                approaching = true;
                agent.SetDestination(point);
            }
        }
        public override void TakeDamage(DamageInfo d)
        {
            GameEvents.Hit(transform.position + Vector3.up);
            Health -= d.amount;
            panic = Time.time + 10;
            Session.RaiseAlert(1);
            if (Health > 0)
                return;
            agent.enabled = false;
            if (pose)
                pose.enabled = false;
            transform.rotation = Quaternion.Euler(80, transform.eulerAngles.y, 0);
            var c = GetComponent<Collider>();
            if (c)
                c.enabled = false;
        }
    }
}

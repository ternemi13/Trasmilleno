using UnityEngine;
using UnityEngine.AI;
namespace Transmilleno
{
    public sealed class PedestrianAgent : Actor
    {
        NavMeshAgent agent;PoseDriver pose;Vector3 home;float next;float panic;
        void Start(){agent=GetComponent<NavMeshAgent>();pose=GetComponentInChildren<PoseDriver>();home=transform.position;next=Time.time+Random.value;GameEvents.Noise+=Hear;}
        void OnDestroy(){GameEvents.Noise-=Hear;}
        void Hear(Vector3 p,float r){if(Vector3.Distance(p,transform.position)<r)panic=Time.time+5;}
        void Update(){if(!Session.Running||IsDead||!agent||!agent.isOnNavMesh)return;if(pose)pose.Speed=agent.velocity.magnitude;if(Time.time<next)return;next=Time.time+.6f;agent.speed=panic>Time.time?3.8f:1.1f;if(!agent.hasPath||agent.remainingDistance<1||panic>Time.time){var p=home+new Vector3(Random.Range(-2,2),0,Random.Range(-12,12));if(panic>Time.time&&Session.Player)p=transform.position+(transform.position-Session.Player.transform.position).normalized*8;if(NavMesh.SamplePosition(p,out var h,5,NavMesh.AllAreas))agent.SetDestination(h.position);}}
        public override void TakeDamage(DamageInfo d){Health-=d.amount;panic=Time.time+10;Session.RaiseAlert(1);if(Health>0)return;agent.enabled=false;if(pose)pose.enabled=false;transform.rotation=Quaternion.Euler(80,transform.eulerAngles.y,0);var c=GetComponent<Collider>();if(c)c.enabled=false;}
    }
}

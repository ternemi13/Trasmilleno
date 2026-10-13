using UnityEngine;
using UnityEngine.AI;
namespace Transmilleno
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class GuardAgent : Actor
    {
        public enum GuardState { Patrol, Suspicious, Alert, Chase, Attack, Search, Stunned, Flee, Dead }
        public GuardState State {get;private set;} public bool Police; public PoseDriver Pose;
        NavMeshAgent agent; Vector3 home, lastSeen; float nextThink, suspicion, lostAt, strikeAt, stunUntil, bleedUntil, nextBleed; bool windup; LineRenderer tracer; float tracerUntil;
        void Awake(){agent=GetComponent<NavMeshAgent>();}
        void Start(){var definition=Session.Config?(Police?Session.Config.police:Session.Config.guard):null;Health=definition?definition.health+(Police?0:Session.Level.index*5):Police?120:65+Session.Level.index*5;home=transform.position;nextThink=Time.time+Random.value*.3f;agent.speed=Police?4.1f:3.1f+Session.Level.index*.07f;GameEvents.Noise+=Hear;tracer=gameObject.AddComponent<LineRenderer>();tracer.positionCount=2;tracer.startWidth=.035f;tracer.endWidth=.012f;tracer.material=new Material(Resources.Load<Material>("Materials/tracer"));tracer.material.color=new Color(1,.65f,.2f);tracer.enabled=false;}
        void OnDestroy(){GameEvents.Noise-=Hear;if(tracer&&tracer.sharedMaterial)Destroy(tracer.sharedMaterial);}
        void Update()
        {
            if(!Session.Running||IsDead)return;if(Pose)Pose.Speed=agent.velocity.magnitude;if(tracer&&Time.time>tracerUntil)tracer.enabled=false;
            if(Time.time<bleedUntil&&Time.time>nextBleed){nextBleed=Time.time+1;TakeDamage(new DamageInfo(3,DamageType.Punch,null));}
            if(Time.time<stunUntil){State=GuardState.Stunned;agent.isStopped=true;return;}if(!agent.isOnNavMesh)return;agent.isStopped=false;
            if(Time.time<nextThink)return;nextThink=Time.time+.18f; var player=Session.Player;if(!player||player.IsDead)return;
            Vector3 delta=player.transform.position-transform.position;float distance=delta.magnitude;bool los=!Physics.Linecast(transform.position+Vector3.up*1.5f,player.transform.position+Vector3.up,1<<0,QueryTriggerInteraction.Ignore);
            float perception=Session.Level.perception*(Session.Difficulty?Session.Difficulty.perception:1);
            bool visible=distance<perception&&los&&(Vector3.Dot(transform.forward,delta.normalized)>.3f||State==GuardState.Chase||distance<3);
            if(visible&&(Session.Trespassing||Session.Alert>.7f)){lastSeen=player.transform.position;lostAt=Time.time;suspicion+=.22f;if(suspicion<.6f){State=GuardState.Suspicious;return;}if(State!=GuardState.Chase&&State!=GuardState.Attack){Session.RaiseAlert(.45f);GameEvents.EmitNoise(lastSeen,12);}State=GuardState.Chase;}
            if(!Police&&Health<16){State=GuardState.Flee;Go(home+new Vector3(-9,0,0));return;}
            if(State==GuardState.Chase||State==GuardState.Attack)
            {
                if(Time.time-lostAt>7){State=GuardState.Search;suspicion=0;Go(lastSeen);return;}
                float reach=Police?10:1.75f;if(distance<reach&&los){agent.isStopped=true;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Vector3.ProjectOnPlane(delta,Vector3.up)),.45f);State=GuardState.Attack;
                    if(!windup&&Time.time>strikeAt){windup=true;strikeAt=Time.time+(Police?.8f:.65f);if(Pose)Pose.Attack=.45f;GameEvents.Notify(Police?"¡Policía apuntando! Busca cobertura.":"¡El celador prepara un golpe!");}
                    else if(windup&&Time.time>strikeAt){windup=false;strikeAt=Time.time+1.1f;if(Pose)Pose.Attack=1;if(Police){tracer.enabled=true;tracer.SetPosition(0,transform.position+Vector3.up*1.3f);tracer.SetPosition(1,player.transform.position+Vector3.up);tracerUntil=Time.time+.1f;}if(distance<reach&&(!Police||Random.value>.28f))player.TakeDamage(new DamageInfo(Police?15:12,Police?DamageType.Bullet:DamageType.Baton,gameObject));}
                }else{windup=false;Go(lastSeen);}return;
            }
            if(State==GuardState.Search&&Time.time-lostAt<10){Go(lastSeen);return;}
            State=GuardState.Patrol;suspicion=Mathf.Max(0,suspicion-.06f);if(!agent.hasPath||agent.remainingDistance<1)Go(home+new Vector3(Random.Range(-2f,2f),0,Random.Range(-7f,7f)));
        }
        void Go(Vector3 p){if(NavMesh.SamplePosition(p,out var h,5,NavMesh.AllAreas))agent.SetDestination(h.position);}
        void Hear(Vector3 p,float radius){if(IsDead||Vector3.Distance(p,transform.position)>radius)return;lastSeen=p;lostAt=Time.time;if(State!=GuardState.Chase&&State!=GuardState.Attack){State=GuardState.Search;Go(p);}}
        public override void TakeDamage(DamageInfo d){if(IsDead)return;GameEvents.Hit(transform.position+Vector3.up);Health=Mathf.Max(0,Health-d.amount);stunUntil=Time.time+.65f;windup=false;if(agent.enabled&&agent.isOnNavMesh)agent.Move(d.knockback*.15f);if(d.type==DamageType.Knife)bleedUntil=Time.time+4;if(d.source){lastSeen=d.source.transform.position;lostAt=Time.time;State=GuardState.Chase;}Session.RaiseAlert(.45f);if(!IsDead)return;State=GuardState.Dead;if(agent.isOnNavMesh)agent.isStopped=true;agent.enabled=false;if(Pose)Pose.enabled=false;transform.rotation=Quaternion.Euler(0,transform.eulerAngles.y,78);var c=GetComponent<Collider>();if(c)c.enabled=false;GameEvents.Notify(Police?"Policía neutralizado":"Celador neutralizado");}
    }
}

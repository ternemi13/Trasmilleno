using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
namespace Transmilleno
{
    public sealed class BusController : MonoBehaviour
    {
        public enum BusState { Waiting, Arriving, Braking, Opening, Boarding, Closing, Departing }
        public BusState State {get;private set;} public float Eta => State==BusState.Boarding?timer:State==BusState.Waiting?timer+8:8;
        public bool Reverse; public Transform Door; public float Speed {get;private set;} public Vector3 Stop = new Vector3(5.5f,0,5); public bool Boarded {get;private set;}
        float timer,progress;SplineContainer route;Vector3 prior;readonly Collider[] contacts=new Collider[12];
        void Start(){route=gameObject.AddComponent<SplineContainer>();var spline=new Spline();spline.Add(new BezierKnot(new float3(Stop.x,0,-65)));spline.Add(new BezierKnot(new float3(Stop.x,0,65)));spline.SetTangentMode(TangentMode.Linear);route.Spline=spline; // Spline is evaluated in local coordinates explicitly below.
            timer=Reverse?9:4;transform.position=new Vector3(Stop.x,0,-65);prior=transform.position;State=BusState.Waiting;}
        void Update()
        {
            if(!Session.Running)return;prior=transform.position;timer-=Time.deltaTime;
            switch(State){case BusState.Waiting:if(timer<=0){State=BusState.Arriving;progress=0;}break;
                case BusState.Arriving:case BusState.Braking:progress+=Time.deltaTime*(State==BusState.Braking?.035f:.075f);float stopT=(Stop.z+65)/130;if(progress>=stopT){progress=stopT;State=BusState.Opening;timer=1;GameEvents.Notify("Bus en estación · espera la apertura de puertas");}else if(progress>stopT-.1f)State=BusState.Braking;Move(progress);break;
                case BusState.Opening:if(Door)Door.localPosition=Vector3.Lerp(Door.localPosition,new Vector3(-1.45f,1.6f,3),Time.deltaTime*4);if(timer<=0){State=BusState.Boarding;timer=Session.Level.stopDuration;if(!Reverse)GameEvents.Doors(Stop+new Vector3(-2.7f,.4f,1.5f),true);GameEvents.EmitNoise(transform.position,5);}break;
                case BusState.Boarding:TryBoard();if(timer<=0){State=BusState.Closing;timer=1.5f;if(!Reverse)GameEvents.Doors(Stop,false);}break;
                case BusState.Closing:if(Door)Door.localPosition=Vector3.Lerp(Door.localPosition,new Vector3(-1.45f,1.6f,1.5f),Time.deltaTime*4);if(timer<=0){State=BusState.Departing;if(Boarded){Session.Running=false;GameEvents.Complete();return;}}break;
                case BusState.Departing:progress+=Time.deltaTime*.09f;Move(progress);if(progress>=1){State=BusState.Waiting;timer=Session.Level.busInterval;transform.position=new Vector3(Stop.x,0,-65);}break;}
            Speed=Vector3.Distance(prior,transform.position)/Mathf.Max(.001f,Time.deltaTime);if(Speed>2&&State!=BusState.Waiting){int n=Physics.OverlapBoxNonAlloc(transform.position+Vector3.up*1.4f,new Vector3(1.35f,1.4f,8.3f),contacts,Quaternion.identity,(1<<8)|(1<<9));for(int i=0;i<n;i++){var a=contacts[i].GetComponentInParent<Actor>();if(a)a.TakeDamage(new DamageInfo(999,DamageType.Bus,gameObject));}}
        }
        void Move(float t){transform.position=(Vector3)route.Spline.EvaluatePosition(Mathf.Clamp01(t));}
        public bool TryBoard()
        {
            if(State!=BusState.Boarding||Boarded||Reverse||!Session.Trespassing)return false;var p=Session.Player as PlayerController;if(!p||p.IsDead)return false;Vector3 door=Stop+new Vector3(-2.7f,.4f,1.5f);if(Vector3.Distance(p.transform.position,door)>2.05f)return false;
            if(!p.Input.Interact.WasPressedThisFrame()) {GameEvents.Notify("E   Subir al bus");return false;}Boarded=true;p.Board(transform);p.transform.localPosition=new Vector3(0,.4f,1.5f);return true;
        }
    }
}

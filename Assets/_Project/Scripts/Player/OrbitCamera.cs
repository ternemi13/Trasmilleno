using UnityEngine;
using Unity.Cinemachine;
namespace Transmilleno
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        public PlayerController Player; public CinemachineCamera VirtualCamera; float yaw, pitch=18, shake; Transform pivot;
        void Start() { pivot=new GameObject("Camera target").transform; GameEvents.PlayerHurt+=OnHurt; }
        void OnDestroy(){GameEvents.PlayerHurt-=OnHurt;if(pivot)Destroy(pivot.gameObject);}
        void OnHurt(float amount){shake=.22f;}
        void LateUpdate()
        {
            if(!Player||!pivot)return;
            if(Session.Running){var look=Player.Input.Look.ReadValue<Vector2>(); bool pad=Player.Input.Look.activeControl?.device is UnityEngine.InputSystem.Gamepad;float factor=pad?Time.deltaTime*130:.12f; yaw+=look.x*factor*Session.Save.sensitivity; pitch=Mathf.Clamp(pitch+look.y*factor*(Session.Save.invertY?1:-1),-20,68);}
            pivot.position=Player.transform.position+Vector3.up*1.45f;Quaternion rotation=Quaternion.Euler(pitch,yaw,0);Vector3 desired=pivot.position+rotation*new Vector3(Session.Save.shoulder,.3f,-4.6f);Vector3 delta=desired-pivot.position;
            if(Physics.SphereCast(pivot.position,.22f,delta.normalized,out var hit,delta.magnitude,1<<0,QueryTriggerInteraction.Ignore))desired=pivot.position+delta.normalized*Mathf.Max(.4f,hit.distance-.12f);
            shake=Mathf.Max(0,shake-Time.unscaledDeltaTime); VirtualCamera.transform.position=Vector3.Lerp(VirtualCamera.transform.position,desired,1-Mathf.Exp(-16*Time.unscaledDeltaTime))+Random.insideUnitSphere*shake*.08f;VirtualCamera.transform.LookAt(pivot.position+rotation*Vector3.right*.25f); var lens=VirtualCamera.Lens;lens.FieldOfView=Mathf.Lerp(lens.FieldOfView,Player.Speed>4?70:62,Time.unscaledDeltaTime*4);VirtualCamera.Lens=lens;
        }
    }
}

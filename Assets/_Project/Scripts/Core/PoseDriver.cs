using UnityEngine;
namespace Transmilleno
{
    public sealed class PoseDriver : MonoBehaviour
    {
        public Transform hips, spine, head, leftArm, rightArm, leftForearm, rightForearm, leftLeg, rightLeg, leftShin, rightShin;
        public Transform leftHand,rightHand;public Vector3 LeftGrip,RightGrip;public bool UseGrip;public float Speed, Attack, Climb; public bool Holding; float phase; Vector3 hipOrigin;
        void Start() { if (hips) hipOrigin = hips.localPosition; }
        void LateUpdate()
        {
            phase += Time.deltaTime * Mathf.Lerp(2, 11, Mathf.Clamp01(Speed / 5));
            float gait = Mathf.Sin(phase) * Mathf.Clamp01(Speed / 2) * 32;
            Attack = Mathf.Max(0, Attack - Time.deltaTime * 3.5f);
            if (hips) hips.localPosition = hipOrigin + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * .04f * Mathf.Clamp01(Speed));
            Set(leftLeg, gait, 0, 0); Set(rightLeg, -gait, 0, 0); Set(leftShin, Mathf.Max(0, -gait) * 1.5f, 0, 0); Set(rightShin, Mathf.Max(0, gait) * 1.5f, 0, 0);
            Set(leftArm, -gait - Climb * 130, 0, -74 + Climb * 45); Set(rightArm, gait - Attack * 100 - Climb * 130 - (Holding ? 45 : 0), 0, 74 - Climb * 45);
            Set(leftForearm, 0, -15 - Climb * 40, 0); Set(rightForearm, 0, 15 + Attack * 55 + (Holding ? 50 : 0), 0);
            Set(spine, Mathf.Clamp(Speed * 1.5f, 0, 8), Attack * -16, 0);if(UseGrip&&Climb>.1f){SolveArm(leftArm,leftForearm,leftHand,LeftGrip);SolveArm(rightArm,rightForearm,rightHand,RightGrip);}
        }
        void SolveArm(Transform upper,Transform lower,Transform hand,Vector3 target){if(!upper||!lower||!hand)return;Vector3 origin=upper.position;float a=Vector3.Distance(origin,lower.position),b=Vector3.Distance(lower.position,hand.position);Vector3 delta=target-origin;float d=Mathf.Clamp(delta.magnitude,.02f,a+b-.001f);var direction=delta.normalized;float cosine=Mathf.Clamp((a*a+d*d-b*b)/(2*a*d),-1,1);Vector3 bend=Vector3.Cross(transform.right,direction).normalized;Vector3 elbow=origin+direction*(a*cosine)+bend*(a*Mathf.Sqrt(1-cosine*cosine));upper.rotation=Quaternion.FromToRotation(lower.position-origin,elbow-origin)*upper.rotation;lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;}
        static void Set(Transform t, float x, float y, float z) { if(t) t.localRotation = Quaternion.Euler(x,y,z); }
    }
}

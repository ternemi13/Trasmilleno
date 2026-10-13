using UnityEngine;
namespace Transmilleno
{
    public sealed class PoseDriver : MonoBehaviour
    {
        public Transform hips, spine, head, leftArm, rightArm, leftForearm, rightForearm, leftLeg, rightLeg, leftShin, rightShin;
        public float Speed, Attack, Climb; public bool Holding; float phase; Vector3 hipOrigin;
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
            Set(spine, Mathf.Clamp(Speed * 1.5f, 0, 8), Attack * -16, 0);
        }
        static void Set(Transform t, float x, float y, float z) { if(t) t.localRotation = Quaternion.Euler(x,y,z); }
    }
}

using UnityEngine;
namespace Transmilleno
{
    public sealed class PoseDriver : MonoBehaviour
    {
        public Transform hips, spine, head, leftArm, rightArm, leftForearm, rightForearm, leftLeg, rightLeg, leftShin, rightShin;
        public Transform leftHand, rightHand; public Vector3 LeftGrip, RightGrip; public bool UseGrip; public float Speed, Attack, Climb; public bool Holding; float phase; Vector3 hipOrigin;
        public bool OriginalRig;
        Transform[] originalBones; Quaternion[] restLocal, restWorld; float leftSign, rightSign;
        void Start()
        {
            if (hips)
                hipOrigin = hips.localPosition;
            if (OriginalRig)
            {
                originalBones = new[] { leftLeg, rightLeg, leftShin, rightShin, leftArm, rightArm, leftForearm, rightForearm, spine };
                restLocal = new Quaternion[originalBones.Length];
                restWorld = new Quaternion[originalBones.Length];
                for (int i = 0; i < originalBones.Length; i++)
                {
                    restLocal[i] = originalBones[i].localRotation;
                    restWorld[i] = Quaternion.Inverse(transform.rotation) * originalBones[i].rotation;
                }
                leftSign = transform.InverseTransformPoint(leftArm.position).x < 0 ? 1 : -1;
                rightSign = transform.InverseTransformPoint(rightArm.position).x < 0 ? 1 : -1;
            }
        }
        void LateUpdate()
        {
            phase += Time.deltaTime * Mathf.Lerp(2, 11, Mathf.Clamp01(Speed / 5));
            float gait = Mathf.Sin(phase) * Mathf.Clamp01(Speed / 2) * 32;
            Attack = Mathf.Max(0, Attack - Time.deltaTime * 3.5f);
            if (hips)
                hips.localPosition = hipOrigin + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * .04f * Mathf.Clamp01(Speed));
            if (OriginalRig)
            {
                AnimateOriginal(gait);
                return;
            }
            Set(leftLeg, gait, 0, 0);
            Set(rightLeg, -gait, 0, 0);
            Set(leftShin, Mathf.Max(0, -gait) * 1.5f, 0, 0);
            Set(rightShin, Mathf.Max(0, gait) * 1.5f, 0, 0);
            Set(leftArm, -gait - Climb * 130, 0, -74 + Climb * 45);
            Set(rightArm, gait - Attack * 100 - Climb * 130 - (Holding ? 45 : 0), 0, 74 - Climb * 45);
            Set(leftForearm, 0, -15 - Climb * 40, 0);
            Set(rightForearm, 0, 15 + Attack * 55 + (Holding ? 50 : 0), 0);
            Set(spine, Mathf.Clamp(Speed * 1.5f, 0, 8), Attack * -16, 0);
            if (UseGrip && Climb > .1f)
            {
                SolveArm(leftArm, leftForearm, leftHand, LeftGrip);
                SolveArm(rightArm, rightForearm, rightHand, RightGrip);
            }
        }
        void AnimateOriginal(float gait)
        {
            for (int i = 0; i < originalBones.Length; i++)
                originalBones[i].localRotation = restLocal[i];
            originalBones[0].rotation = transform.rotation * Quaternion.Euler(gait, 0, 0) * restWorld[0];
            originalBones[1].rotation = transform.rotation * Quaternion.Euler(-gait, 0, 0) * restWorld[1];
            originalBones[2].localRotation = restLocal[2] * Quaternion.Euler(-Mathf.Max(0, -gait), 0, 0);
            originalBones[3].localRotation = restLocal[3] * Quaternion.Euler(-Mathf.Max(0, gait), 0, 0);
            originalBones[4].rotation = transform.rotation * Quaternion.Euler(-gait - Climb * 120, 0, leftSign * (74 - Climb * 40)) * restWorld[4];
            originalBones[5].rotation = transform.rotation * Quaternion.Euler(gait - Attack * 100 - Climb * 120, 0, rightSign * (74 - Climb * 40)) * restWorld[5];
            if (UseGrip && Climb > .1f)
            {
                SolveArm(leftArm, leftForearm, leftHand, LeftGrip);
                SolveArm(rightArm, rightForearm, rightHand, RightGrip);
            }
        }
        void SolveArm(Transform upper, Transform lower, Transform hand, Vector3 target)
        {
            if (!upper || !lower || !hand)
                return;
            Vector3 origin = upper.position;
            float a = Vector3.Distance(origin, lower.position), b = Vector3.Distance(lower.position, hand.position);
            Vector3 delta = target - origin;
            float d = Mathf.Clamp(delta.magnitude, .02f, a + b - .001f);
            var direction = delta.normalized;
            float cosine = Mathf.Clamp((a * a + d * d - b * b) / (2 * a * d), -1, 1);
            Vector3 bend = Vector3.Cross(transform.right, direction).normalized;
            Vector3 elbow = origin + direction * (a * cosine) + bend * (a * Mathf.Sqrt(1 - cosine * cosine));
            upper.rotation = Quaternion.FromToRotation(lower.position - origin, elbow - origin) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(hand.position - lower.position, target - lower.position) * lower.rotation;
        }
        static void Set(Transform t, float x, float y, float z)
        {
            if (t)
                t.localRotation = Quaternion.Euler(x, y, z);
        }
    }
}

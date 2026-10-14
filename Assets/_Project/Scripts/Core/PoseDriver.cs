using UnityEngine;
using System.Collections.Generic;
namespace Transmilleno
{
    // All IK is solved in world space, preserving each imported bone's native axes and bind pose.
    [DefaultExecutionOrder(40)]
    public sealed class PoseDriver : MonoBehaviour
    {
        public Transform hips, spine, head, leftArm, rightArm, leftForearm, rightForearm;
        public Transform leftHand, rightHand, leftLeg, rightLeg, leftShin, rightShin, leftFoot, rightFoot;
        public Vector3 LeftGrip, RightGrip;
        public bool UseGrip, Holding, OriginalRig, Armed, Firearm, Aiming;
        public float Speed, Attack, Climb;
        public Transform WeaponSocket
        {
            get; private set;
        }
        Transform[] fingers; Quaternion[] fingerRest;
        Quaternion rightPalmBasis = Quaternion.identity, leftPalmBasis = Quaternion.identity;
        SkinnedMeshRenderer[] skins; int[] gripShapes;
        Transform[] bones; Quaternion[] rest; Vector3 hipOrigin;
        Quaternion leftFootRest, rightFootRest; float phase, speed, climb, armed, aim, strikeTime = 10;
        float leftX, rightX, footY, height, previousYaw, turn; bool knifeStrike; int strikeSide;
        Vector3 previousPosition;
        public void Initialize()
        {
            if (bones != null || !hips)
                return;
            if (!leftFoot && leftShin && leftShin.childCount > 0)
                leftFoot = leftShin.GetChild(0);
            if (!rightFoot && rightShin && rightShin.childCount > 0)
                rightFoot = rightShin.GetChild(0);
            bones = new[] { hips, spine, head, leftArm, rightArm, leftForearm, rightForearm, leftHand, rightHand, leftLeg, rightLeg, leftShin, rightShin, leftFoot, rightFoot };
            rest = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                if (bones[i])
                    rest[i] = bones[i].localRotation;
            hipOrigin = hips.localPosition;
            height = Mathf.Max(1.5f, transform.InverseTransformPoint(head.position).y * 1.08f);
            leftX = transform.InverseTransformPoint(leftFoot.position).x;
            rightX = transform.InverseTransformPoint(rightFoot.position).x;
            footY = Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y, transform.InverseTransformPoint(rightFoot.position).y);
            leftFootRest = Quaternion.Inverse(transform.rotation) * leftFoot.rotation;
            rightFootRest = Quaternion.Inverse(transform.rotation) * rightFoot.rotation;
            var digits = new List<Transform>();
            foreach (var t in rightHand.GetComponentsInChildren<Transform>())
                if (t != rightHand && (t.name.Contains("Index") || t.name.Contains("Middle") || t.name.Contains("Ring") || t.name.Contains("Pinky")))
                    digits.Add(t);
            rightPalmBasis = PalmBasis(rightHand);
            leftPalmBasis = PalmBasis(leftHand);
            fingers = digits.ToArray();
            fingerRest = new Quaternion[fingers.Length];
            for (int i = 0; i < fingers.Length; i++)
                fingerRest[i] = fingers[i].localRotation;
            skins = GetComponentsInChildren<SkinnedMeshRenderer>();
            gripShapes = new int[skins.Length];
            for (int i = 0; i < skins.Length; i++)
                gripShapes[i] = skins[i].sharedMesh.GetBlendShapeIndex("RightHandGrip");
            previousPosition = transform.position;
            previousYaw = transform.eulerAngles.y;
            var socket = new GameObject("Palm weapon socket").transform;
            socket.SetParent(transform, false);
            WeaponSocket = socket;
        }
        void Start() => Initialize();
        public void Strike(bool knife, int combo = 0)
        {
            strikeTime = 0;
            knifeStrike = knife;
            strikeSide = combo % 2;
            Attack = 0;
        }
        void LateUpdate()
        {
            Initialize();
            if (bones == null)
                return;
            bool visible = false;
            for (int i = 0; i < skins.Length; i++)
                visible |= skins[i].enabled && (skins[i].isVisible || skins[i].updateWhenOffscreen);
            if (!visible)
            {
                previousPosition = transform.position;
                previousYaw = transform.eulerAngles.y;
                strikeTime += Time.deltaTime;
                return;
            }
            float dt = Time.deltaTime;
            if (Attack > .05f)
            {
                Strike(Armed);
                Attack = 0;
            }
            strikeTime += dt;
            float response = 1 - Mathf.Exp(-10 * dt);
            speed = Mathf.Lerp(speed, Speed, response);
            climb = Mathf.Lerp(climb, Climb, response);
            armed = Mathf.Lerp(armed, Armed || Holding ? 1 : 0, response);
            aim = Mathf.Lerp(aim, Aiming ? 1 : 0, response);
            for (int i = 0; i < skins.Length; i++)
                if (gripShapes[i] >= 0)
                    skins[i].SetBlendShapeWeight(gripShapes[i], armed * 100);
            float walk = Mathf.Clamp01(speed / 1.1f), run = Mathf.InverseLerp(2.4f, 5.5f, speed);
            float travel = Vector3.ProjectOnPlane(transform.position - previousPosition, Vector3.up).magnitude;
            previousPosition = transform.position;
            // Distance-driven phase prevents skating when an agent stops or changes speed.
            phase += Mathf.Min(travel, .3f) * Mathf.Lerp(5.7f, 3.9f, run);
            float yaw = transform.eulerAngles.y;
            turn = Mathf.Lerp(turn, Mathf.Clamp(Mathf.DeltaAngle(previousYaw, yaw) / Mathf.Max(dt, .001f), -180, 180), response);
            previousYaw = yaw;
            for (int i = 0; i < bones.Length; i++)
                if (bones[i])
                    bones[i].localRotation = rest[i];
            float breath = Mathf.Sin(Time.time * 2.1f) * .005f;
            Vector3 hipOffset = new Vector3(Mathf.Sin(phase) * .018f * walk, -walk * (.018f + .016f * Mathf.Cos(phase * 2)) + breath - climb * .38f, 0);
            hips.localPosition = hipOrigin + hips.parent.InverseTransformVector(transform.TransformVector(hipOffset));
            hips.rotation = transform.rotation * Quaternion.Euler(0, Mathf.Sin(phase) * 3 * walk, -turn * .012f) * Quaternion.Inverse(transform.rotation) * hips.rotation;
            spine.rotation = transform.rotation * Quaternion.Euler(run * 8, -Mathf.Sin(phase) * 5 * walk, Mathf.Sin(phase) * 1.5f * walk) * Quaternion.Inverse(transform.rotation) * spine.rotation;
            AnimateLeg(leftLeg, leftShin, leftFoot, leftX, phase, walk, run, leftFootRest);
            AnimateLeg(rightLeg, rightShin, rightFoot, rightX, phase + Mathf.PI, walk, run, rightFootRest);
            float leftSign = transform.InverseTransformPoint(leftArm.position).x > 0 ? 1 : -1;
            float rightSign = -leftSign;
            Vector3 l = new Vector3(leftSign * .28f, height * .49f, -Mathf.Sin(phase) * .16f * walk + .04f);
            Vector3 r = new Vector3(rightSign * .28f, height * .49f, Mathf.Sin(phase) * .16f * walk + .04f);
            l.y += run * .20f;
            r.y += run * .20f;
            r = Vector3.Lerp(r, new Vector3(rightSign * .27f, height * .61f, .30f), armed);
            r = Vector3.Lerp(r, new Vector3(rightSign * .12f, height * .80f, .58f), aim);
            l = Vector3.Lerp(l, new Vector3(rightSign * .075f, height * .793f, .55f), aim);
            // Anticipation 0-.14s, extension .14-.28s, follow-through .28-.42s, recovery .42-.65s.
            if (strikeTime < .65f && !Firearm)
            {
                float t = strikeTime;
                Vector3 ready = new Vector3(rightSign * .32f, height * .76f, .03f);
                Vector3 hit = new Vector3(knifeStrike ? leftSign * .14f : rightSign * .10f, height * .73f, .62f);
                Vector3 end = new Vector3(leftSign * .28f, height * .59f, .36f);
                Vector3 strike = t < .14f ? Vector3.Lerp(r, ready, Smooth(t / .14f)) : t < .28f ? Vector3.Lerp(ready, hit, Smooth((t - .14f) / .14f)) : t < .42f ? Vector3.Lerp(hit, end, Smooth((t - .28f) / .14f)) : Vector3.Lerp(end, r, Smooth((t - .42f) / .23f));
                if (!knifeStrike && strikeSide == 1)
                {
                    strike.x *= -1;
                    l = strike;
                }
                else
                    r = strike;
                float twist = Mathf.Sin(Mathf.Clamp01(t / .65f) * Mathf.PI) * (knifeStrike ? 17 : 10);
                spine.rotation = Quaternion.AngleAxis(twist, transform.up) * spine.rotation;
            }
            if (Firearm && strikeTime < .18f)
                r.z -= Mathf.Sin(strikeTime / .18f * Mathf.PI) * .055f;
            Vector3 lw = transform.TransformPoint(l), rw = transform.TransformPoint(r);
            if (UseGrip)
            {
                lw = Vector3.Lerp(lw, LeftGrip, climb);
                rw = Vector3.Lerp(rw, RightGrip, climb);
            }
            Solve(leftArm, leftForearm, leftHand, lw, transform.TransformDirection(new Vector3(leftSign, -.5f, -.45f)));
            Solve(rightArm, rightForearm, rightHand, rw, transform.TransformDirection(new Vector3(rightSign, -.5f, -.45f)));
            // Socket follows the palm in metres; imported skeleton scales never scale a weapon.
            rightHand.rotation = Quaternion.Slerp(rightHand.rotation, Quaternion.LookRotation(WeaponForward(), transform.up) * Quaternion.Inverse(rightPalmBasis), armed);
            if (Firearm)
                leftHand.rotation = Quaternion.Slerp(leftHand.rotation, Quaternion.LookRotation(transform.forward, transform.up) * Quaternion.Inverse(leftPalmBasis), aim);
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].localRotation = fingerRest[i];
                fingers[i].rotation = Quaternion.AngleAxis(-rightSign * armed * 52, transform.up) * fingers[i].rotation;
            }
            WeaponSocket.position = rightHand.position;
            Vector3 forward = (rightHand.position - rightForearm.position).normalized;
            if (Firearm)
                forward = Vector3.Lerp(transform.forward * .7f - transform.up * .7f, transform.forward, aim).normalized;
            else
                forward = Vector3.Slerp(transform.forward, forward, .35f);
            WeaponSocket.rotation = Quaternion.LookRotation(forward, transform.up);
            WeaponSocket.localScale = Vector3.one;
        }
        Vector3 WeaponForward() => Firearm ? Vector3.Lerp(transform.forward * .7f - transform.up * .7f, transform.forward, aim).normalized : transform.forward;
        Quaternion PalmBasis(Transform hand)
        {
            Transform middle = null, index = null, pinky = null;
            foreach (var finger in hand.GetComponentsInChildren<Transform>())
            {
                if (finger.name.Contains("Middle1"))
                    middle = finger;
                if (finger.name.Contains("Index1"))
                    index = finger;
                if (finger.name.Contains("Pinky1"))
                    pinky = finger;
            }
            if (middle && index && pinky)
                return Quaternion.LookRotation(hand.InverseTransformDirection(middle.position - hand.position), hand.InverseTransformDirection(index.position - pinky.position));
            float side = transform.InverseTransformPoint(hand.position).x > 0 ? 1 : -1;
            return Quaternion.LookRotation(Vector3.right * side, Vector3.up);
        }
        static float Smooth(float x) => x * x * (3 - 2 * x);
        void AnimateLeg(Transform upper, Transform lower, Transform foot, float x, float p, float walk, float run, Quaternion orientation)
        {
            float z = Mathf.Cos(p) * Mathf.Lerp(.19f, .43f, run) * walk;
            float lift = Mathf.Pow(Mathf.Max(0, Mathf.Sin(p)), 1.5f) * Mathf.Lerp(.085f, .19f, run) * walk;
            Vector3 local = new Vector3(x, footY + lift, z + .025f);
            if (climb > .01f)
                local = Vector3.Lerp(local, new Vector3(x * 1.6f, height * .30f + Mathf.Sin(p) * .08f, .18f), climb);
            Solve(upper, lower, foot, transform.TransformPoint(local), transform.forward);
            foot.rotation = transform.rotation * Quaternion.Euler(-Mathf.Max(0, Mathf.Sin(p)) * 18 * walk, 0, 0) * orientation;
        }
        static void Solve(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 pole)
        {
            if (!upper || !lower || !end)
                return;
            Vector3 origin = upper.position, delta = target - origin;
            float a = Vector3.Distance(origin, lower.position), b = Vector3.Distance(lower.position, end.position);
            float d = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + .005f, a + b - .005f);
            Vector3 dir = delta.normalized;
            Vector3 bend = Vector3.ProjectOnPlane(pole, dir).normalized;
            if (bend.sqrMagnitude < .01f)
                bend = Vector3.Cross(dir, Vector3.right).normalized;
            float cos = Mathf.Clamp((a * a + d * d - b * b) / (2 * a * d), -1, 1);
            Vector3 elbow = origin + dir * (a * cos) + bend * (a * Mathf.Sqrt(1 - cos * cos));
            upper.rotation = Quaternion.FromToRotation(lower.position - origin, elbow - origin) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(end.position - lower.position, target - lower.position) * lower.rotation;
        }
    }
}

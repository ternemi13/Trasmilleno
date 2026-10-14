using System.Collections.Generic;
using UnityEngine;
namespace Transmilleno
{
    // Built only on a lethal vehicle hit; the original skinned model becomes the body.
    public sealed class PlayerRagdoll : MonoBehaviour
    {
        Transform focus;
        public Vector3 Focus => focus ? focus.position : transform.position + Vector3.up * .9f;
        public void Launch(PlayerController player, DamageInfo damage)
        {
            player.GetComponent<CharacterController>().enabled = false;
            var pose = player.Pose;
            var bodies = new List<Rigidbody>();
            var shapes = new List<Collider>();
            Rigidbody Limb(Transform bone, Vector3 end, float radius, float mass, Rigidbody parent)
            {
                Vector3 segment = end - bone.position;
                var shape = new GameObject("Impact body collider");
                shape.layer = 8;
                shape.transform.SetParent(bone, false);
                shape.transform.position = bone.position + segment * .5f;
                shape.transform.rotation = segment.sqrMagnitude > .0001f ? Quaternion.FromToRotation(Vector3.up, segment.normalized) : Quaternion.identity;
                var capsule = shape.AddComponent<CapsuleCollider>();
                capsule.radius = radius;
                capsule.height = Mathf.Max(radius * 2, segment.magnitude + radius);
                var body = bone.gameObject.AddComponent<Rigidbody>();
                body.mass = mass;
                body.linearDamping = .15f;
                body.angularDamping = .6f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.solverIterations = 8;
                body.maxDepenetrationVelocity = 3;
                if (parent)
                {
                    var joint = bone.gameObject.AddComponent<CharacterJoint>();
                    joint.connectedBody = parent;
                    joint.axis = segment.sqrMagnitude > .0001f ? bone.InverseTransformDirection(segment.normalized) : Vector3.up;
                    joint.swingAxis = Vector3.Cross(joint.axis, Mathf.Abs(joint.axis.z) < .8f ? Vector3.forward : Vector3.up).normalized;
                    joint.lowTwistLimit = new SoftJointLimit { limit = -25 };
                    joint.highTwistLimit = new SoftJointLimit { limit = 25 };
                    joint.swing1Limit = new SoftJointLimit { limit = 65 };
                    joint.swing2Limit = new SoftJointLimit { limit = 35 };
                    joint.enablePreprocessing = false;
                    joint.enableProjection = true;
                    joint.projectionDistance = .06f;
                }
                bodies.Add(body); shapes.Add(capsule);
                return body;
            }
            if (pose && pose.hips && pose.spine && pose.head)
            {
                pose.enabled = false;
                var animator = pose.GetComponent<Animator>();
                if (animator) animator.enabled = false;
                foreach (var skin in pose.GetComponentsInChildren<SkinnedMeshRenderer>()) skin.updateWhenOffscreen = true;
                focus = pose.hips;
                var pelvis = Limb(pose.hips, pose.spine.position, .15f, 7, null);
                var chest = Limb(pose.spine, Vector3.Lerp(pose.spine.position, pose.head.position, .7f), .17f, 8, pelvis);
                Limb(pose.head, pose.head.position + Vector3.up * .08f, .12f, 3, chest);
                var leftArm = Limb(pose.leftArm, pose.leftForearm.position, .075f, 1.6f, chest);
                Limb(pose.leftForearm, pose.leftHand.position, .065f, 1.2f, leftArm);
                var rightArm = Limb(pose.rightArm, pose.rightForearm.position, .075f, 1.6f, chest);
                Limb(pose.rightForearm, pose.rightHand.position, .065f, 1.2f, rightArm);
                var leftLeg = Limb(pose.leftLeg, pose.leftShin.position, .10f, 3, pelvis);
                Limb(pose.leftShin, pose.leftFoot.position, .085f, 2, leftLeg);
                var rightLeg = Limb(pose.rightLeg, pose.rightShin.position, .10f, 3, pelvis);
                Limb(pose.rightShin, pose.rightFoot.position, .085f, 2, rightLeg);
                if (player.KnifeVisual) player.KnifeVisual.SetActive(false);
            }
            else
            {
                focus = transform;
                Limb(transform, transform.position + Vector3.up * 1.2f, .25f, 30, null);
            }
            var sourceColliders = damage.source ? damage.source.GetComponentsInChildren<Collider>() : new Collider[0];
            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = i + 1; j < shapes.Count; j++) Physics.IgnoreCollision(shapes[i], shapes[j]);
                foreach (var other in sourceColliders) Physics.IgnoreCollision(shapes[i], other);
            }
            Vector3 impulse = damage.knockback.sqrMagnitude > .1f ? damage.knockback : Vector3.forward * 9 + Vector3.up * 6;
            Vector3 tumble = Vector3.Cross(Vector3.up, impulse.normalized) * 2.4f;
            Vector3 center = bodies[0].worldCenterOfMass;
            foreach (var body in bodies)
            {
                body.linearVelocity = impulse + Vector3.Cross(tumble, body.worldCenterOfMass - center);
                body.angularVelocity = tumble;
            }
        }
    }
}

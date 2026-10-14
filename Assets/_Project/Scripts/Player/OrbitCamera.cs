using UnityEngine;
using Unity.Cinemachine;
namespace Transmilleno
{
    [DefaultExecutionOrder(-100)]
    public sealed class OrbitCamera : MonoBehaviour
    {
        public const float MinDistance = 1.8f, MaxDistance = 8;
        public PlayerController Player;
        public CinemachineCamera VirtualCamera;
        public float RequestedDistance { get; private set; } = 4.6f;
        public float CurrentDistance { get; private set; } = 4.6f;
        float yaw, pitch = 18, zoomDistance = 4.6f;
        Vector3 focus, focusVelocity;
        bool initialized;
        public void Zoom(float wheelDelta)
        {
            RequestedDistance = Mathf.Clamp(RequestedDistance - Mathf.Clamp(wheelDelta / 120f, -3, 3) * .6f, MinDistance, MaxDistance);
        }
        void LateUpdate()
        {
            if (!Player || !VirtualCamera) return;
            float dt = Time.unscaledDeltaTime;
            if (!initialized)
            {
                yaw = Player.transform.eulerAngles.y;
                focus = Player.transform.position + Vector3.up * 1.45f;
                initialized = true;
            }
            if (Session.Running && !Player.IsDead)
            {
                var look = Player.Input.Look.ReadValue<Vector2>();
                bool pad = Player.Input.Look.activeControl?.device is UnityEngine.InputSystem.Gamepad;
                float factor = pad ? dt * 130 : .12f;
                yaw += look.x * factor * Session.Save.sensitivity;
                pitch = Mathf.Clamp(pitch + look.y * factor * (Session.Save.invertY ? 1 : -1), -20, 68);
                Zoom(Player.Input.Zoom.ReadValue<float>());
            }
            Vector3 target = Player.Ragdoll ? Player.Ragdoll.Focus + Vector3.up * .25f : Player.transform.position + Vector3.up * 1.45f;
            focus = Vector3.SmoothDamp(focus, target, ref focusVelocity, Player.IsDead ? .12f : .055f, Mathf.Infinity, dt);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            zoomDistance = Mathf.Lerp(zoomDistance, RequestedDistance, 1 - Mathf.Exp(-12 * dt));
            Vector3 shoulder = focus + rotation * Vector3.right * Session.Save.shoulder;
            // Check the shoulder offset too: it must not move the cast origin through a wall.
            Vector3 offset = shoulder - focus;
            if (offset.sqrMagnitude > .001f && Physics.SphereCast(focus, .18f, offset.normalized, out var sideHit, offset.magnitude, (1 << 0) | (1 << 10), QueryTriggerInteraction.Ignore))
                shoulder = focus + offset.normalized * Mathf.Max(0, sideHit.distance - .05f);
            float safeDistance = zoomDistance;
            if (Physics.SphereCast(shoulder, .22f, -(rotation * Vector3.forward), out var hit, zoomDistance, (1 << 0) | (1 << 10), QueryTriggerInteraction.Ignore))
                safeDistance = Mathf.Max(.15f, hit.distance - .08f);
            // Snap inward before a wall; ease outward after clearing it.
            CurrentDistance = safeDistance < CurrentDistance ? safeDistance : Mathf.Lerp(CurrentDistance, safeDistance, 1 - Mathf.Exp(-9 * dt));
            VirtualCamera.transform.SetPositionAndRotation(shoulder - rotation * Vector3.forward * CurrentDistance, rotation);
            var lens = VirtualCamera.Lens;
            lens.FieldOfView = 62;
            VirtualCamera.Lens = lens;
        }
    }
}

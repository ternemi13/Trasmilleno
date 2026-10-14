using UnityEngine;
namespace Transmilleno
{
    // Coordinates surveyed on the supplied Calle 45 model after uniform metric conversion.
    public static class StationLayout
    {
        public const float Floor = 1.04f, HalfWidth = 2.18f, DoorZ = 9.38f;
        public static readonly Vector3 Spawn = new Vector3(0, Floor, -9);
        public static Vector3 Boarding(bool reverse) => new Vector3(reverse ? -1.55f : 1.55f, Floor, DoorZ);
        public static Vector3 Stop(bool reverse) => new Vector3(reverse ? -3.8f : 3.8f, 0, DoorZ + (reverse ? 1.5f : -1.5f));
        public static bool Inside(Vector3 p) => Mathf.Abs(p.x) < HalfWidth && p.z > .6f && p.z < 223 && p.y > .65f;
    }
}

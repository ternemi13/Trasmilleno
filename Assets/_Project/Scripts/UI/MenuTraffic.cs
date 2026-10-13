using UnityEngine;
namespace Transmilleno
{
    public sealed class MenuTraffic : MonoBehaviour
    {
        void Update()
        {
            var p = transform.localPosition;
            p.z = Mathf.PingPong(Time.unscaledTime * 5, 120) - 60;
            transform.localPosition = p;
        }
    }
}

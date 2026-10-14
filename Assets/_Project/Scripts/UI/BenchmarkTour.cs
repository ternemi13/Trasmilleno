using UnityEngine;
namespace Transmilleno
{
    // QA-only movement through the actual level, with guards, rain and both buses active.
    public sealed class BenchmarkTour : MonoBehaviour
    {
        public GameApp App; float elapsed, nextStrike; CharacterController motor;
        void Update()
        {
            if (!App || App.Loading || !App.Player || !Session.Running)
                return;
            if (!motor)
            {
                motor = App.Player.GetComponent<CharacterController>();
                App.Player.VerificationInvulnerable = true;
            }
            elapsed += Time.deltaTime;
            float z = 6 + Mathf.PingPong(elapsed * 3, 25);
            var target = new Vector3(Mathf.Sin(elapsed * .7f) * .8f, StationLayout.Floor, z);
            motor.enabled = false;
            App.Player.transform.position = target;
            motor.enabled = true;
            App.Player.Pose.Speed = 3;
            Session.Trespassing = true;
            if (elapsed > nextStrike)
            {
                nextStrike = elapsed + 2;
                App.Player.Pose.Strike(true);
            }
        }
    }
}

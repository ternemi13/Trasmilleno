using UnityEngine;
namespace Transmilleno
{
    public sealed class BusAudio : MonoBehaviour
    {
        AudioSource motor; AudioClip clip; BusController bus;
        void Start()
        {
            bus = GetComponent<BusController>();
            motor = gameObject.AddComponent<AudioSource>();
            motor.spatialBlend = 1;
            motor.rolloffMode = AudioRolloffMode.Linear;
            motor.minDistance = 4;
            motor.maxDistance = 50;
            int n = 22050;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / 22050f;
                data[i] = Mathf.Sin(t * 2 * Mathf.PI * 48) * .3f + Mathf.Sin(t * 2 * Mathf.PI * 96) * .12f + Mathf.Sin(t * 2 * Mathf.PI * 13) * .04f;
            }
            clip = AudioClip.Create("Articulated bus engine", n, 1, 22050, false);
            clip.SetData(data, 0);
            motor.clip = clip;
            motor.loop = true;
            motor.Play();
        }
        void Update()
        {
            if (!motor || !bus)
                return;
            motor.volume = (Session.Save?.effects ?? .8f) * .3f;
            motor.pitch = Mathf.Lerp(.65f, 1.4f, Mathf.Clamp01(bus.Speed / 13));
        }
        void OnDestroy()
        {
            if (clip)
                Destroy(clip);
        }
    }
}

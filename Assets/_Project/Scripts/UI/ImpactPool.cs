using UnityEngine;
namespace Transmilleno
{
    public sealed class ImpactPool : MonoBehaviour
    {
        readonly ParticleSystem[] pool = new ParticleSystem[12]; int cursor; Material material; Texture2D texture;
        void Start()
        {
            material = new Material(Resources.Load<Material>("Materials/particle"));
            texture = new Texture2D(32, 32);
            var pixels = new Color[1024];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f;
                    pixels[y * 32 + x] = new Color(1, 1, 1, Mathf.Clamp01(1 - d) * Mathf.Clamp01(1 - d));
                }
            texture.SetPixels(pixels);
            texture.Apply();
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Surface", 1);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
            for (int i = 0; i < pool.Length; i++)
            {
                var p = new GameObject("Impact pool " + i).AddComponent<ParticleSystem>();
                p.transform.SetParent(transform);
                p.Stop();
                var main = p.main;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = .3f;
                main.startSpeed = 2.8f;
                main.startSize = .09f;
                main.startColor = new Color(1, .73f, .38f);
                main.gravityModifier = .7f;
                main.maxParticles = 18;
                var emission = p.emission;
                emission.rateOverTime = 0;
                var shape = p.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = .12f;
                p.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
                pool[i] = p;
            }
            GameEvents.Impact += Burst;
        }
        void Burst(Vector3 position)
        {
            var p = pool[cursor++ % pool.Length];
            p.transform.position = position;
            p.Emit(12);
        }
        void OnDestroy()
        {
            GameEvents.Impact -= Burst;
            if (material)
                Destroy(material);
            if (texture)
                Destroy(texture);
        }
    }
}

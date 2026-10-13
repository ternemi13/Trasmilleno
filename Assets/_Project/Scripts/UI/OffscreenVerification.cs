using System.IO;
using UnityEngine;

namespace Transmilleno
{
    /// <summary>Explicit GPU rendering for hidden Windows QA processes. Normal play never uses this component.</summary>
    public sealed class OffscreenVerification : MonoBehaviour
    {
        public GameApp App;
        private RenderTexture target;
        private Camera renderCamera;
        private Canvas ui;
        private Texture2D readback;

        private void Start()
        {
            target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            target.Create();
            readback = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            ui = GetComponentInChildren<Canvas>();
        }

        private void LateUpdate()
        {
            if (!App.SceneCamera || !target)
                return;
            renderCamera = App.SceneCamera;
            renderCamera.enabled = false;
            renderCamera.targetTexture = target;
            ui.renderMode = RenderMode.ScreenSpaceCamera;
            ui.worldCamera = renderCamera;
            ui.planeDistance = .5f;
            Canvas.ForceUpdateCanvases();
            renderCamera.Render();
            // A synchronous readback completes the GPU work before frame timing is recorded.
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0, false);
            RenderTexture.active = previous;
        }

        public void Save(string path)
        {
            if (!readback)
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, readback.EncodeToPNG());
        }

        private void OnDestroy()
        {
            if (target)
            {
                target.Release();
                Destroy(target);
            }
            if (readback)
                Destroy(readback);
        }
    }
}

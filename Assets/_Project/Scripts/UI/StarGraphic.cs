using UnityEngine;
using UnityEngine.UI;
namespace Transmilleno
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StarGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            Vector2 center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * .48f;
            vh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i < 10; i++)
            {
                float a = (90 + i * 36) * Mathf.Deg2Rad;
                float r = radius * (i % 2 == 0 ? 1 : .43f);
                vh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, color, Vector2.zero);
            }
            for (int i = 0; i < 10; i++)
                vh.AddTriangle(0, i + 1, (i + 1) % 10 + 1);
        }
    }
}

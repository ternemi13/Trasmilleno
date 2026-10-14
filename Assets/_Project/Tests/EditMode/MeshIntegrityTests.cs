using NUnit.Framework;
using UnityEngine;
namespace Transmilleno.Tests
{
    public class MeshIntegrityTests
    {
        [Test] public void TrouserCuffsStayBoundToOneLeg()
        {
            var config = Resources.Load<GameProgressionConfig>("Progression");
            foreach (var skin in config.catalog.Get("player").model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var m = skin.sharedMesh; var v = m.vertices; var t = m.triangles; var w = m.boneWeights;
                for (int i = 0; i < t.Length; i += 3)
                {
                    if (v[t[i]].y > .5f || v[t[i+1]].y > .5f || v[t[i+2]].y > .5f) continue;
                    bool left = false, right = false;
                    for (int j=0;j<3;j++) { var b=w[t[i+j]].boneIndex0; left |= b>=11 && b<=13; right |= b>=14 && b<=16; }
                    Assert.IsFalse(left && right, "A cuff triangle is bound to opposing legs.");
                }
            }
        }
        [Test] public void SecondaryPedestrianHasConsistentVertexAndIndexBuffers()
        {
            var config=Resources.Load<GameProgressionConfig>("Progression");
            foreach(var skin in config.catalog.Get("pedestrian2").model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var m=skin.sharedMesh;var vertices=m.vertices;var triangles=m.triangles;
                Assert.AreEqual(m.vertexCount,m.boneWeights.Length);
                for(int t=0;t<triangles.Length;t+=3)
                {
                    Assert.Less(Vector3.Distance(vertices[triangles[t]],vertices[triangles[t+1]]),.5f,"Corrupt triangle spanning the character: "+m.name);
                    Assert.Less(Vector3.Distance(vertices[triangles[t+1]],vertices[triangles[t+2]]),.5f,"Corrupt triangle spanning the character: "+m.name);
                }
            }
        }
    }
}

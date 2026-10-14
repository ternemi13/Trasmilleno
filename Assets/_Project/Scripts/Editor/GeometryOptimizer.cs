using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
namespace Transmilleno.Editor
{
    public static class GeometryOptimizer
    {
        public static Mesh SaveMesh(string path, Mesh source)
        {
            var target = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!target)
            {
                AssetDatabase.CreateAsset(source, path);
                return source;
            }
            // CopySerialized can retain the old native vertex buffer while replacing indices.
            // Use the Mesh API so resized geometry, skin weights and index buffers stay coherent.
            target.Clear(false);
            target.indexFormat = source.indexFormat;
            target.vertices = source.vertices;
            target.normals = source.normals;
            target.tangents = source.tangents;
            target.uv = source.uv;
            target.uv2 = source.uv2;
            target.colors32 = source.colors32;
            target.subMeshCount = source.subMeshCount;
            for (int i = 0; i < source.subMeshCount; i++)
                target.SetTriangles(source.GetTriangles(i), i, false);
            target.bindposes = source.bindposes;
            target.boneWeights = source.boneWeights;
            var dv = new Vector3[source.vertexCount];
            var dn = new Vector3[source.vertexCount];
            var dt = new Vector3[source.vertexCount];
            for (int b = 0; b < source.blendShapeCount; b++)
            for (int f = 0; f < source.GetBlendShapeFrameCount(b); f++)
            {
                source.GetBlendShapeFrameVertices(b, f, dv, dn, dt);
                target.AddBlendShapeFrame(source.GetBlendShapeName(b), source.GetBlendShapeFrameWeight(b, f), dv, dn, dt);
            }
            target.bounds = source.bounds;
            target.name = source.name;
            EditorUtility.SetDirty(target);
            Object.DestroyImmediate(source);
            return target;
        }
        public static void Simplify(Mesh mesh, float cell)
        {
            var v = mesh.vertices;
            var uv = mesh.uv;
            var normals = mesh.normals;
            var map = new int[v.Length];
            var unique = new Dictionary<(int, int, int, int, int), int>();
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var ns = new List<Vector3>();
            for (int i = 0; i < v.Length; i++)
            {
                var t = uv.Length == v.Length ? uv[i] : Vector2.zero;
                var key = (Mathf.RoundToInt(v[i].x / cell), Mathf.RoundToInt(v[i].y / cell), Mathf.RoundToInt(v[i].z / cell), Mathf.RoundToInt(t.x * 64), Mathf.RoundToInt(t.y * 64));
                if (!unique.TryGetValue(key, out int n))
                {
                    n = verts.Count;
                    unique[key] = n;
                    verts.Add(v[i]);
                    uvs.Add(t);
                    ns.Add(normals.Length == v.Length ? normals[i] : Vector3.up);
                }
                map[i] = n;
            }
            var subs = new List<int>[mesh.subMeshCount];
            for (int j = 0; j < subs.Length; j++)
            {
                subs[j] = new List<int>();
                var tri = mesh.GetTriangles(j);
                for (int i = 0; i < tri.Length; i += 3)
                {
                    int a = map[tri[i]], b = map[tri[i + 1]], c = map[tri[i + 2]];
                    if (a == b || b == c || a == c)
                        continue;
                    subs[j].Add(a);
                    subs[j].Add(b);
                    subs[j].Add(c);
                }
            }
            mesh.Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(ns);
            mesh.subMeshCount = subs.Length;
            for (int j = 0; j < subs.Length; j++)
                mesh.SetTriangles(subs[j], j);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }
        public static void Build(GameObject root, string key)
        {
            var groups = new Dictionary<string, List<CombineInstance>>();
            var materials = new Dictionary<string, Material>();
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (!mf || !mf.sharedMesh)
                    continue;
                Vector3 center = root.transform.InverseTransformPoint(r.bounds.center);
                string tag = "Static";
                if (key == "station")
                {
                    // Keep the original two sliding leaves separate from the baked station.
                    if (Mathf.Abs(center.x) > 2 && Mathf.Abs(center.x) < 2.3f && r.bounds.size.x < .15f && center.y > 1.01f && center.y < 3.3f && Mathf.Abs(center.z - StationLayout.DoorZ) < .96f)
                        tag = (center.x < 0 ? "SouthDoor" : "NorthDoor") + (center.z < StationLayout.DoorZ ? "A" : "B");
                    else
                        tag = "Static_" + Mathf.FloorToInt(center.z / 20);
                }
                for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                {
                    var mat = r.sharedMaterials[Mathf.Min(i, r.sharedMaterials.Length - 1)];
                    string id = tag + "|" + mat.name;
                    if (!groups.TryGetValue(id, out var list))
                    {
                        list = new List<CombineInstance>();
                        groups[id] = list;
                        materials[id] = mat;
                    }
                    list.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = i, transform = root.transform.worldToLocalMatrix * r.transform.localToWorldMatrix });
                }
            }
            Directory.CreateDirectory("Assets/_Project/Art/Optimized");
            int index = 0;
            var holders = new Dictionary<string, Transform>();
            foreach (var pair in groups)
            {
                string tag = pair.Key.Split('|')[0];
                if (!holders.TryGetValue(tag, out var parent))
                {
                    parent = new GameObject(tag).transform;
                    parent.SetParent(root.transform, false);
                    holders[tag] = parent;
                }
                var mesh = new Mesh { indexFormat = IndexFormat.UInt32, name = key + "_" + index };
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                if (key == "pistol")
                    Simplify(mesh, .0015f);
                mesh.Optimize();
                string path = "Assets/_Project/Art/Optimized/" + key + "_" + index++ + ".asset";
                mesh = SaveMesh(path, mesh);
                var part = new GameObject(pair.Key.Split('|')[1]);
                part.transform.SetParent(parent, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = materials[pair.Key];
            }
            foreach (var r in renderers)
            {
                Object.DestroyImmediate(r.GetComponent<MeshFilter>());
                Object.DestroyImmediate(r);
            }
            // Remove the thousands of empty source nodes from the derived prefab only.
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i);
                if (!holders.ContainsValue(child))
                    Object.DestroyImmediate(child.gameObject);
            }
            Debug.Log("[Geometry] " + key + ": " + renderers.Length + " renderers → " + groups.Count);
        }
    }
}

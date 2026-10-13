using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
namespace Transmilleno.Editor
{
    public static class RigBuilder
    {
        static readonly string[] Names = { "Hips", "Spine", "Chest", "Neck", "Head", "LeftUpperArm", "LeftLowerArm", "LeftHand", "RightUpperArm", "RightLowerArm", "RightHand", "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "RightUpperLeg", "RightLowerLeg", "RightFoot", "LeftToes", "RightToes" };
        static int BoneIndex(string name, bool positive)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("hand") || n.Contains("thumb") || n.Contains("index") || n.Contains("pinky"))
                return positive ? 7 : 10;
            if (n.Contains("forearm") || n.Contains("lowerarm"))
                return positive ? 6 : 9;
            if (n.Contains("arm"))
                return positive ? 5 : 8;
            if (n.Contains("toe"))
                return positive ? 17 : 18;
            if (n.Contains("foot"))
                return positive ? 13 : 16;
            if (n.Contains("upleg") || n.Contains("upperleg"))
                return positive ? 11 : 14;
            if (n.Contains("leg"))
                return positive ? 12 : 15;
            if (n.Contains("head"))
                return 4;
            if (n.Contains("neck"))
                return 3;
            if (n.Contains("spine2") || n.Contains("chest") || n.Contains("shoulder"))
                return 2;
            if (n.Contains("spine"))
                return 1;
            return 0;
        }
        static bool PreserveOriginal(GameObject root, string key)
        {
            string[] aliases = { "Hips", "Spine", "Spine2", "Neck", "Head", "LeftArm", "LeftForeArm", "LeftHand", "RightArm", "RightForeArm", "RightHand", "LeftUpLeg", "LeftLeg", "LeftFoot", "RightUpLeg", "RightLeg", "RightFoot", "LeftToeBase", "RightToeBase" };
            var all = root.GetComponentsInChildren<Transform>();
            var mapped = new Transform[aliases.Length];
            foreach (var bone in root.GetComponentInChildren<SkinnedMeshRenderer>().bones)
            {
                string name = bone.name;
                int colon = name.LastIndexOf(':');
                if (colon >= 0)
                    name = name.Substring(colon + 1);
                int suffix = name.LastIndexOf('_');
                if (suffix >= 0 && int.TryParse(name.Substring(suffix + 1), out _))
                    name = name.Substring(0, suffix);
                for (int i = 0; i < aliases.Length; i++)
                    if (name == aliases[i])
                        mapped[i] = bone;
            }
            for (int i = 0; i < mapped.Length; i++)
                if (!mapped[i])
                    return false;
            var human = new HumanBone[mapped.Length];
            for (int i = 0; i < mapped.Length; i++)
                human[i] = new HumanBone { boneName = mapped[i].name, humanName = Names[i], limit = new HumanLimit { useDefaultValues = true } };
            var skeleton = new SkeletonBone[all.Length];
            for (int i = 0; i < all.Length; i++)
                skeleton[i] = new SkeletonBone { name = all[i].name, position = all[i].localPosition, rotation = all[i].localRotation, scale = all[i].localScale };
            var avatar = AvatarBuilder.BuildHumanAvatar(root, new HumanDescription { human = human, skeleton = skeleton, upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f, armStretch = .05f, legStretch = .05f });
            avatar.name = key + "_Avatar";
            string path = "Assets/_Project/Art/Animations/" + key + "_Avatar.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Avatar>(path);
            if (existing)
            {
                EditorUtility.CopySerialized(avatar, existing);
                Object.DestroyImmediate(avatar);
                avatar = existing;
            }
            else
                AssetDatabase.CreateAsset(avatar, path);
            var animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.enabled = false;
            var pose = root.AddComponent<PoseDriver>();
            pose.OriginalRig = true;
            pose.hips = mapped[0];
            pose.spine = mapped[1];
            pose.head = mapped[4];
            pose.leftArm = mapped[5];
            pose.leftForearm = mapped[6];
            pose.leftHand = mapped[7];
            pose.rightArm = mapped[8];
            pose.rightForearm = mapped[9];
            pose.rightHand = mapped[10];
            pose.leftLeg = mapped[11];
            pose.leftShin = mapped[12];
            pose.rightLeg = mapped[14];
            pose.rightShin = mapped[15];
            Debug.Log("[Rig] Preserved original skin and skeleton: " + key + " Humanoid=" + avatar.isHuman);
            return true;
        }
        // Skin static and already-skinned user geometry onto a reproducible humanoid skeleton.
        public static void Rig(GameObject root, string key)
        {
            if (root.GetComponentInChildren<SkinnedMeshRenderer>() && PreserveOriginal(root, key))
                return;
            var renderers = root.GetComponentsInChildren<Renderer>();
            var baked = new List<(Mesh, Material[], BoneWeight[])>();
            var landmarks = new Dictionary<string, Vector3>();
            Bounds bounds = new Bounds();
            bool first = true;
            foreach (var r in renderers)
            {
                Mesh m = null;
                BoneWeight[] retained = null;
                if (r is SkinnedMeshRenderer skin)
                {
                    m = new Mesh();
                    skin.BakeMesh(m);
                    retained = skin.sharedMesh.boneWeights;
                    int[] map = new int[skin.bones.Length];
                    for (int j = 0; j < map.Length; j++)
                    {
                        var bone = skin.bones[j];
                        Vector3 pos = root.transform.InverseTransformPoint(bone.position);
                        map[j] = BoneIndex(bone.name, pos.x > 0);
                        if (!bone.name.ToLowerInvariant().Contains("root") && !bone.name.ToLowerInvariant().Contains("shoulder") && !bone.name.ToLowerInvariant().Contains("end") && !landmarks.ContainsKey(Names[map[j]]))
                            landmarks[Names[map[j]]] = pos;
                    }
                    for (int j = 0; j < retained.Length; j++)
                    {
                        var w = retained[j];
                        w.boneIndex0 = map[w.boneIndex0];
                        w.boneIndex1 = map[w.boneIndex1];
                        w.boneIndex2 = map[w.boneIndex2];
                        w.boneIndex3 = map[w.boneIndex3];
                        retained[j] = w;
                    }
                }
                else
                {
                    var mf = r.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh)
                        m = Object.Instantiate(mf.sharedMesh);
                }
                if (!m)
                    continue;
                var matrix = root.transform.worldToLocalMatrix * r.transform.localToWorldMatrix;
                var vertices = m.vertices;
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
                m.vertices = vertices;
                m.RecalculateBounds();
                m.RecalculateNormals();
                if (first)
                {
                    bounds = m.bounds;
                    first = false;
                }
                else
                    bounds.Encapsulate(m.bounds);
                baked.Add((m, r.sharedMaterials, retained));
            }
            while (root.transform.childCount > 0)
                Object.DestroyImmediate(root.transform.GetChild(0).gameObject);
            float h = bounds.size.y;
            Transform Bone(string name, Transform parent, Vector3 world)
            {
                var t = new GameObject(name).transform;
                t.SetParent(parent);
                t.position = root.transform.TransformPoint(landmarks.TryGetValue(name, out var landmark) ? landmark : world * h);
                return t;
            }
            var hips = Bone("Hips", root.transform, new Vector3(0, .52f, 0));
            var spine = Bone("Spine", hips, new Vector3(0, .63f, 0));
            var chest = Bone("Chest", spine, new Vector3(0, .75f, 0));
            var neck = Bone("Neck", chest, new Vector3(0, .87f, 0));
            var head = Bone("Head", neck, new Vector3(0, .94f, 0));
            var la = Bone("LeftUpperArm", chest, new Vector3(.15f, .8f, 0));
            var lf = Bone("LeftLowerArm", la, new Vector3(.3f, .8f, 0));
            var lh = Bone("LeftHand", lf, new Vector3(.43f, .8f, 0));
            var ra = Bone("RightUpperArm", chest, new Vector3(-.15f, .8f, 0));
            var rf = Bone("RightLowerArm", ra, new Vector3(-.3f, .8f, 0));
            var rh = Bone("RightHand", rf, new Vector3(-.43f, .8f, 0));
            var ll = Bone("LeftUpperLeg", hips, new Vector3(.085f, .51f, 0));
            var ls = Bone("LeftLowerLeg", ll, new Vector3(.085f, .28f, 0));
            var lfoot = Bone("LeftFoot", ls, new Vector3(.085f, .055f, .025f));
            var rl = Bone("RightUpperLeg", hips, new Vector3(-.085f, .51f, 0));
            var rs = Bone("RightLowerLeg", rl, new Vector3(-.085f, .28f, 0));
            var rfoot = Bone("RightFoot", rs, new Vector3(-.085f, .055f, .025f));
            var lt = Bone("LeftToes", lfoot, new Vector3(.085f, .025f, .1f));
            var rt = Bone("RightToes", rfoot, new Vector3(-.085f, .025f, .1f));
            Transform[] bones = { hips, spine, chest, neck, head, la, lf, lh, ra, rf, rh, ll, ls, lfoot, rl, rs, rfoot, lt, rt };
            var binds = new Matrix4x4[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                binds[i] = bones[i].worldToLocalMatrix * root.transform.localToWorldMatrix;
            int mi = 0;
            foreach (var item in baked)
            {
                var m = item.Item1;
                var weights = new BoneWeight[m.vertexCount];
                var vs = m.vertices;
                for (int i = 0; i < vs.Length; i++)
                {
                    Vector3 p = vs[i] / h;
                    bool left = p.x > 0;
                    int primary = 0, second = 1;
                    float blend = 0;
                    if (p.y > .88f)
                    {
                        primary = 4;
                        second = 3;
                        blend = Mathf.Clamp01((.93f - p.y) / .05f) * .25f;
                    }
                    else if (p.y > .71f && Mathf.Abs(p.x) > .16f)
                    {
                        float x = Mathf.Abs(p.x);
                        primary = left ? 5 : 8;
                        second = left ? 6 : 9;
                        blend = Mathf.InverseLerp(.25f, .35f, x);
                        if (x > .4f)
                        {
                            primary = left ? 7 : 10;
                            blend = 0;
                        }
                    }
                    else if (p.y > .58f)
                    {
                        primary = p.y > .72f ? 2 : 1;
                        second = 0;
                        blend = 0;
                    }
                    else if (p.y < .5f)
                    {
                        primary = left ? 11 : 14;
                        second = left ? 12 : 15;
                        blend = 1 - Mathf.InverseLerp(.24f, .34f, p.y);
                        if (p.y < .1f)
                        {
                            primary = left ? 13 : 16;
                            blend = 0;
                        }
                    }
                    weights[i] = new BoneWeight { boneIndex0 = primary, weight0 = 1 - blend, boneIndex1 = second, weight1 = blend };
                }
                m.boneWeights = item.Item3 != null && item.Item3.Length == m.vertexCount ? item.Item3 : weights;
                m.bindposes = binds;
                string path = "Assets/_Project/Art/Animations/" + key + "_skin_" + mi + ".asset";
                m.name = System.IO.Path.GetFileNameWithoutExtension(path);
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing)
                {
                    EditorUtility.CopySerialized(m, existing);
                    Object.DestroyImmediate(m);
                    m = existing;
                }
                else
                    AssetDatabase.CreateAsset(m, path);
                var g = new GameObject("Rigged geometry " + mi++);
                g.transform.SetParent(root.transform, false);
                var skin = g.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh = m;
                skin.sharedMaterials = item.Item2;
                skin.bones = bones;
                skin.rootBone = hips;
                skin.localBounds = new Bounds(Vector3.up * h * .5f, new Vector3(h * 1.5f, h * 1.6f, h));
                skin.updateWhenOffscreen = false;
            }
            var human = new HumanBone[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                human[i] = new HumanBone { boneName = bones[i].name, humanName = bones[i].name, limit = new HumanLimit { useDefaultValues = true } };
            var skel = new List<SkeletonBone>();
            skel.Add(new SkeletonBone { name = root.name, position = Vector3.zero, rotation = Quaternion.identity, scale = Vector3.one });
            foreach (var b in bones)
                skel.Add(new SkeletonBone { name = b.name, position = b.localPosition, rotation = b.localRotation, scale = b.localScale });
            var description = new HumanDescription { human = human, skeleton = skel.ToArray(), upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f, armStretch = .05f, legStretch = .05f, feetSpacing = 0, hasTranslationDoF = false };
            var avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            avatar.name = key + "_Avatar";
            string avpath = "Assets/_Project/Art/Animations/" + key + "_Avatar.asset";
            var av = AssetDatabase.LoadAssetAtPath<Avatar>(avpath);
            if (av)
            {
                EditorUtility.CopySerialized(avatar, av);
                Object.DestroyImmediate(avatar);
                avatar = av;
            }
            else
                AssetDatabase.CreateAsset(avatar, avpath);
            var animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            var pose = root.AddComponent<PoseDriver>();
            pose.hips = hips;
            pose.spine = spine;
            pose.head = head;
            pose.leftArm = la;
            pose.rightArm = ra;
            pose.leftForearm = lf;
            pose.rightForearm = rf;
            pose.leftLeg = ll;
            pose.rightLeg = rl;
            pose.leftShin = ls;
            pose.rightShin = rs;
            pose.leftHand = lh;
            pose.rightHand = rh;
            if (!avatar.isValid || !avatar.isHuman)
                Debug.LogWarning("[Rig] Review avatar: " + key);
            else
                Debug.Log("[Rig] Humanoid validated: " + key);
        }
    }
}

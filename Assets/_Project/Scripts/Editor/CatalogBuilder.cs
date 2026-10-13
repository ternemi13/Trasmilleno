using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Transmilleno.Editor
{
    public static class CatalogBuilder
    {
        const string Root="Assets/_Project/";
        [MenuItem("Tools/Transmilleno/Asset Catalog Builder")]
        public static AssetCatalog Build()
        {
            Directory.CreateDirectory(Root+"Data/Catalogs");Directory.CreateDirectory(Root+"Art/Animations");Directory.CreateDirectory(Root+"Art/Materials");Directory.CreateDirectory(Root+"Prefabs/Models");AssetDatabase.Refresh();var cat=BuildTools.Asset<AssetCatalog>(Root+"Data/Catalogs/AssetCatalog.asset");
            var defs=new[]{new Definition("player","Player","Assets/Modelos/personaje principal.glb",1.75f,true),new Definition("guard","Guards","Assets/Modelos/security_guard__rigged_3d_model.glb",1.78f,true),new Definition("police","Guards","Assets/Modelos/security_guard__rigged_3d_model.glb",1.83f,true),new Definition("pedestrian","Pedestrians","Assets/Modelos/tyler_the_creator.glb",1.73f,true),new Definition("pedestrian2","Pedestrians","Assets/Modelos/noyz_narcos_skull_hoodie_stance.glb",1.81f,true),new Definition("bus","TransMilenio",Root+"Art/Models/TransMilenio/Original/model.dae",18,false,true),new Definition("station","Station",Root+"Art/Models/Station/Calle45/model.dae",80,false,true),new Definition("stationSabana","Station",Root+"Art/Models/Station/Sabana/model.dae",48,false,true),new Definition("knife","Props","Assets/Modelos/cuchillo.glb",.28f),new Definition("stone","Props","Assets/Modelos/piedra.glb",.22f),new Definition("can","Props","Assets/Modelos/pepsi_lata_resized_with_lights_included.glb",.18f),new Definition("pistol","Props","Assets/Modelos/pistola.glb",.27f)};
            var list=new List<CatalogEntry>();foreach(var d in defs){var old=cat.entries==null?null:cat.Get(d.key);if(old!=null&&old.model&&!AssetDatabase.GetAssetPath(old.model).StartsWith(Root+"Prefabs/Models/")){list.Add(old);continue;}string selected=d.path;var category=Root+"Art/Models/"+d.category;var direct=Directory.GetFiles(category,"*",SearchOption.TopDirectoryOnly);Array.Sort(direct,StringComparer.OrdinalIgnoreCase);foreach(var p in direct){var ext=Path.GetExtension(p).ToLowerInvariant();if(ext==".glb"||ext==".fbx"||ext==".obj"){selected=p.Replace('\\','/');break;}}
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(selected);if(!source){Debug.LogError("Cannot import "+selected);continue;}var root=new GameObject(d.key);var model=(GameObject)PrefabUtility.InstantiatePrefab(source);model.transform.SetParent(root.transform,false);foreach(var c in model.GetComponentsInChildren<Camera>(true)){var extra=c.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();if(extra)Object.DestroyImmediate(extra);Object.DestroyImmediate(c);}foreach(var l in model.GetComponentsInChildren<Light>(true))Object.DestroyImmediate(l);foreach(var c in model.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
                if(d.key=="stationSabana"){foreach(var f in model.GetComponentsInChildren<MeshFilter>()){var renderer=f.GetComponent<Renderer>();if(renderer&&f.sharedMesh.vertexCount<=40&&renderer.bounds.size.y>300){Object.DestroyImmediate(renderer);Object.DestroyImmediate(f);}}}Bounds b=BoundsOf(model);float extent=d.rig?b.size.y:d.longAxis?b.size.z:Mathf.Max(b.size.x,b.size.y,b.size.z);float scale=d.size/Mathf.Max(.001f,extent);model.transform.localScale*=scale;model.transform.localPosition=new Vector3(-b.center.x,-b.min.y,-b.center.z)*scale;
                foreach(var r in model.GetComponentsInChildren<Renderer>(true)){var mats=r.sharedMaterials;for(int j=0;j<mats.Length;j++)mats[j]=Convert(mats[j],d.key);r.sharedMaterials=mats;}
                if(d.rig){RigBuilder.Rig(root,d.key);var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.025f,root.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();}
                string path=Root+"Prefabs/Models/"+d.key+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);list.Add(new CatalogEntry{key=d.key,model=prefab,height=d.size});Object.DestroyImmediate(root);
            }
            cat.entries=list.ToArray();EditorUtility.SetDirty(cat);AssetDatabase.SaveAssets();Debug.Log("[Catalog] "+list.Count+" user models normalized and registered.");return cat;
        }
        static Bounds BoundsOf(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("No renderer: "+g.name);Bounds b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
        static Material Convert(Material source,string category)
        {
            if(!source)return null;if(source.shader&&(source.shader.name.Contains("Universal")||source.shader.name.Contains("glTF"))&&category!="police")return source;
            string key=(category+"_"+source.name).Replace('/','_').Replace(':','_');foreach(char c in Path.GetInvalidFileNameChars())key=key.Replace(c,'_');string path=Root+"Art/Materials/"+key+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}Color color=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.color:Color.white;Texture texture=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):source.HasProperty("baseColorTexture")?source.GetTexture("baseColorTexture"):null;
            m.color=category=="police"?color*new Color(.58f,.7f,.5f):color;m.SetTexture("_BaseMap",texture);m.SetFloat("_Smoothness",.25f);m.SetFloat("_Cull",0);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
        }
        readonly struct Definition{public readonly string key,category,path;public readonly float size;public readonly bool rig,longAxis;public Definition(string key,string category,string path,float size,bool rig=false,bool longAxis=false){this.key=key;this.category=category;this.path=path;this.size=size;this.rig=rig;this.longAxis=longAxis;}}
    }
    public sealed class CatalogWatcher:AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] old){foreach(var p in imported)if(p.StartsWith("Assets/_Project/Art/Models/")&&(p.EndsWith(".glb")||p.EndsWith(".fbx")||p.EndsWith(".obj"))){EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlaying)CatalogBuilder.Build();};break;}}
    }
}

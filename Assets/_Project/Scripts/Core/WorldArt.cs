using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace Transmilleno
{
    public static class WorldArt
    {
        static Mesh bevel, cylinder; static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        public static Material Material(string key, Color color)
        {
            if(materials.TryGetValue(key,out var m)&&m)return m;m=Resources.Load<Material>("Materials/"+key);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=key;m.color=color;m.SetFloat("_Smoothness",key=="glass"?.75f:.18f);m.enableInstancing=true;}materials[key]=m;return m;
        }
        public static GameObject Part(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool collider=true, bool round=false)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=round?Cylinder():Bevel();g.AddComponent<MeshRenderer>().sharedMaterial=mat;if(collider)g.AddComponent<BoxCollider>();return g;
        }
        public static TextMeshPro Sign(string text,Transform parent,Vector3 pos,float size,Color color,float angle=0)
        {
            var g=new GameObject("Sign "+text);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,angle,0);var t=g.AddComponent<TextMeshPro>();t.font=Resources.Load<TMP_FontAsset>("GameFont");t.text=text;t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(12,2);return t;
        }
        public static Mesh Bevel()
        {
            if(bevel)return bevel;var v=new List<Vector3>();var tris=new List<int>();float h=.5f,b=.44f;
            System.Action<Vector3[]> face=points=>{int start=v.Count;Vector3 center=Vector3.zero;foreach(var p in points)center+=p;bool flip=Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),center)<0;v.AddRange(points);for(int j=1;j<points.Length-1;j++){tris.Add(start);tris.Add(start+(flip?j+1:j));tris.Add(start+(flip?j:j+1));}};
            for(int axis=0;axis<3;axis++)for(int s=-1;s<=1;s+=2){var p=new Vector3[4];for(int k=0;k<4;k++){var q=Vector3.zero;q[axis]=s*h;q[(axis+1)%3]=(k==0||k==3?-b:b);q[(axis+2)%3]=(k<2?-b:b);p[k]=q;}face(p);}
            for(int a=0;a<3;a++)for(int baxis=a+1;baxis<3;baxis++)for(int s=-1;s<=1;s+=2)for(int t=-1;t<=1;t+=2){int c=3-a-baxis;Vector3 p=Vector3.zero,q=Vector3.zero;p[a]=s*h;p[baxis]=t*b;q[a]=s*b;q[baxis]=t*h;p[c]=-b;q[c]=-b;var r=q;r[c]=b;var u=p;u[c]=b;face(new[]{p,q,r,u});}
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)face(new[]{new Vector3(x*h,y*b,z*b),new Vector3(x*b,y*h,z*b),new Vector3(x*b,y*b,z*h)});
            bevel=new Mesh{name="Chamfered architectural module"};bevel.SetVertices(v);bevel.SetTriangles(tris,0);var uv=new Vector2[v.Count];for(int i=0;i<v.Count;i++)uv[i]=new Vector2(v[i].x+v[i].z,v[i].y+v[i].z);bevel.uv=uv;bevel.RecalculateNormals();bevel.RecalculateBounds();return bevel;
        }
        public static Mesh Cylinder()
        {
            if(cylinder)return cylinder;var v=new List<Vector3>();var ts=new List<int>();for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;int k=v.Count;v.AddRange(new[]{new Vector3(Mathf.Cos(a)*.5f,-.5f,Mathf.Sin(a)*.5f),new Vector3(Mathf.Cos(a)*.5f,.5f,Mathf.Sin(a)*.5f),new Vector3(Mathf.Cos(b)*.5f,.5f,Mathf.Sin(b)*.5f),new Vector3(Mathf.Cos(b)*.5f,-.5f,Mathf.Sin(b)*.5f),new Vector3(0,.5f,0),new Vector3(0,-.5f,0)});ts.AddRange(new[]{k,k+1,k+2,k,k+2,k+3,k+4,k+2,k+1,k+5,k,k+3});}cylinder=new Mesh{name="Machined cylinder"};cylinder.SetVertices(v);cylinder.SetTriangles(ts,0);cylinder.RecalculateNormals();cylinder.RecalculateBounds();return cylinder;
        }
        public static GameObject Model(AssetCatalog catalog,string key,Transform parent,Vector3 position,Quaternion rotation)
        {
            var entry=catalog.Get(key);if(entry==null||!entry.model)return null;var g=Object.Instantiate(entry.model,parent);g.name=key;g.transform.localPosition=position;g.transform.localRotation=rotation;return g;
        }
    }
}

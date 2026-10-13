using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;
namespace Transmilleno.Editor
{
    public static class BuildTools
    {
        const string Root="Assets/_Project/";const string ResourcesPath=Root+"Resources/";
        static readonly string[] stations={"Calle 45","Marly","Calle 57","Flores","Héroes","Calle 72","Av. Jiménez","Ricaurte","Portal Sur","Portal Norte"};
        [MenuItem("Tools/Transmilleno/Build Everything")]
        public static void BuildEverything()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before building.");
            foreach(var p in new[]{"Resources","Resources/Materials","Data/Levels","Data/Weapons","Data/Enemies","Data/Settings","Data/Catalogs","Prefabs/Player","Prefabs/Enemies","Prefabs/Buses","Prefabs/Station","Prefabs/Props","Prefabs/UI","Prefabs/Systems","Scenes","Art/Materials","Art/Textures","Art/Animations","Art/VFX","Art/UI","Audio/Music","Audio/SFX","Audio/Mixers","Tests/EditMode","Tests/PlayMode"})Directory.CreateDirectory(Root+p);
            Directory.CreateDirectory("Docs/Captures");AssetDatabase.Refresh();SetupFont();MakeMaterials();var catalog=CatalogBuilder.Build();
            var config=Asset<GameProgressionConfig>(ResourcesPath+"Progression.asset");config.catalog=catalog;config.levels=new LevelDefinition[10];
            for(int i=0;i<10;i++){var level=Asset<LevelDefinition>(Root+"Data/Levels/Level_"+(i+1).ToString("00")+".asset");level.index=i+1;level.station=stations[i];level.subtitle=i<2?"Aprende a leer la estación":i<6?"Más ojos. Menos tiempo.":"La ciudad no duerme";level.guards=i<2?i+1:2+i/3;level.police=i<3?0:1+i/4;level.pedestrians=6+i;level.busInterval=Mathf.Max(12,28-i);level.stopDuration=Mathf.Max(9,16-i*.6f);level.perception=9+i*.6f;level.night=i>=7;level.rain=i==8||i==9;level.knife=i>=3;level.layout=i%4;level.spawn=new Vector3(-10,.2f,-18+i%3);level.routes=new[]{new Vector3(-4.4f,.4f,-15),new Vector3(-3.25f,.4f,-6),new Vector3(3.2f,.4f,-9)};config.levels[i]=level;EditorUtility.SetDirty(level);}
            config.difficulties=new DifficultySettings[3];EditorUtility.SetDirty(config);
            foreach(var n in new[]{"Casual","Normal","Difícil"}){var d=Asset<DifficultySettings>(Root+"Data/Settings/"+n+".asset");d.damage=n=="Casual"?.65f:n=="Difícil"?1.35f:1;d.perception=n=="Casual"?.75f:n=="Difícil"?1.2f:1;d.stamina=n=="Casual"?1.35f:n=="Difícil"?.85f:1;config.difficulties[n=="Casual"?0:n=="Normal"?1:2]=d;EditorUtility.SetDirty(d);}
            foreach(var n in new[]{"Fists","Knife","Throwable"}){var w=Asset<WeaponDefinition>(Root+"Data/Weapons/"+n+".asset");w.damage=n=="Knife"?38:n=="Throwable"?24:15;w.bleeding=n=="Knife";if(n=="Fists")config.fists=w;else if(n=="Knife")config.knife=w;else config.throwable=w;EditorUtility.SetDirty(w);}
            foreach(var n in new[]{"Guard","Police"}){var e=Asset<EnemyDefinition>(Root+"Data/Enemies/"+n+".asset");e.police=n=="Police";e.health=e.police?120:70;if(e.police)config.police=e;else config.guard=e;EditorUtility.SetDirty(e);}
            ConfigureProject();
            var boot=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var app=new GameObject("Transmilleno · Game services").AddComponent<GameApp>();app.Configure(config);EditorSceneManager.SaveScene(boot,Root+"Scenes/Boot.unity");
            foreach(var name in new[]{"MainMenu","Gameplay"}){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,Root+"Scenes/"+name+".unity");}
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/Boot.unity",true),new EditorBuildSettingsScene(Root+"Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(Root+"Scenes/Gameplay.unity",true)};
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"Scenes/Boot.unity");Debug.Log("[Transmilleno] Build Everything complete. Ten levels, catalog, rigs, materials and Boot ready.");
        }
        internal static T Asset<T>(string path) where T:ScriptableObject{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(!a){Directory.CreateDirectory(Path.GetDirectoryName(path));a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
        static void SetupFont(){if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ResourcesPath+"GameFont.asset"))return;var source=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");if(!source)throw new Exception("Import TMP Essential Resources first.");AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source),ResourcesPath+"GameFont.asset");}
        static void MakeMaterials()
        {
            string[] keys={"concrete","metal","red","cream","yellow","glass","asphalt","rain"};Color[] colors={new Color(.48f,.5f,.46f),new Color(.12f,.17f,.18f),new Color(.75f,.12f,.11f),new Color(.88f,.82f,.65f),new Color(.95f,.63f,.14f),new Color(.25f,.45f,.49f),new Color(.12f,.15f,.17f),new Color(.6f,.7f,.8f)};
            for(int k=0;k<keys.Length;k++){var path=ResourcesPath+"Materials/"+keys[k]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=colors[k];m.SetFloat("_Smoothness",keys[k]=="glass"?.78f:.2f);m.SetFloat("_Metallic",keys[k]=="metal"?.7f:0);m.enableInstancing=true;var texpath=Root+"Art/Textures/"+keys[k]+".asset";var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texpath);if(!tex){tex=new Texture2D(64,64,TextureFormat.RGBA32,true);tex.name=keys[k]+" weathering";var random=new System.Random(84+k);var pixels=new Color[64*64];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float value=.86f+(float)random.NextDouble()*.14f;if(keys[k]=="concrete"&&(x%32==0||y%32==0))value*=.72f;pixels[y*64+x]=new Color(value,value,value,1);}tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,texpath);}m.SetTexture("_BaseMap",tex);EditorUtility.SetDirty(m);}
        }
        static void ConfigureProject()
        {
            PlayerSettings.productName="Transmilleno";PlayerSettings.companyName="Bogotá Arcade";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.runInBackground=true;PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.IL2CPP);
            var ps=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=ps.FindProperty("activeInputHandler");if(input!=null){input.intValue=1;ps.ApplyModifiedPropertiesWithoutUndo();}
            var tag=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tag.FindProperty("layers");layers.GetArrayElementAtIndex(8).stringValue="Player";layers.GetArrayElementAtIndex(9).stringValue="Characters";layers.GetArrayElementAtIndex(10).stringValue="Buses";tag.ApplyModifiedPropertiesWithoutUndo();Physics.IgnoreLayerCollision(9,9,true);
        }
        [MenuItem("Tools/Transmilleno/Setup Packages")]
        public static void SetupPackages(){UnityEditor.PackageManager.Client.AddAndRemove(new[]{"com.unity.inputsystem","com.unity.cinemachine","com.unity.ai.navigation","com.unity.splines","com.unity.cloud.gltfast"});}
        [MenuItem("Tools/Transmilleno/Validate Project")]
        public static void ValidateProject(){var config=Resources.Load<GameProgressionConfig>("Progression");if(!config||config.levels.Length!=10)throw new Exception("Campaign data missing");foreach(var level in config.levels){if(level.routes==null||level.routes.Length<3||level.guards<1||level.busInterval<=0)throw new Exception("Invalid level: "+level.name);}foreach(var e in config.catalog.entries)if(!e.model)throw new Exception("Missing model: "+e.key);Debug.Log("[Transmilleno] Data and model references validated. Run route/play tests for gameplay validation.");}
        [MenuItem("Tools/Transmilleno/Build Windows")]
        public static void BuildWindows(){Directory.CreateDirectory("Builds/Windows");var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"Scenes/Boot.unity",Root+"Scenes/MainMenu.unity",Root+"Scenes/Gameplay.unity"},locationPathName="Builds/Windows/Transmilleno.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Docs/BUILD-RESULT.txt",r.summary.result+"\nErrors: "+r.summary.totalErrors+"\nTime: "+r.summary.totalTime);if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed");}
    }
}

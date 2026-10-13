using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;
namespace Transmilleno
{
    public sealed class GameApp : MonoBehaviour
    {
        [SerializeField] private GameProgressionConfig config;
        public GameProgressionConfig Config => config; public PlayerController Player {get;private set;} public LevelWorld World {get;private set;} public GameUI UI {get;private set;}
        public bool InMenu {get;private set;}=true; public bool Loading {get;private set;} Camera cameraMain; Coroutine transition; float alertTick; public int LastStars {get;private set;}
        public void Configure(GameProgressionConfig value)=>config=value;
        void Awake(){DontDestroyOnLoad(gameObject);Session.Config=config;Session.Save=SaveSystem.Load();UI=gameObject.AddComponent<GameUI>();UI.App=this;gameObject.AddComponent<GameAudio>();gameObject.AddComponent<ImpactPool>();gameObject.AddComponent<PerformanceProbe>();GameEvents.PlayerDied+=OnDeath;GameEvents.LevelCompleted+=OnComplete;}
        IEnumerator Start(){ApplySettings();yield return ShowMenuRoutine();if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--smoke-test")>=0){var smoke=gameObject.AddComponent<CampaignSmoke>();smoke.App=this;smoke.QuitOnFinish=true;}else if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--benchmark")>=0){Play(10);}}
        void OnDestroy(){GameEvents.PlayerDied-=OnDeath;GameEvents.LevelCompleted-=OnComplete;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        void Update(){if(Session.Running){Session.Elapsed+=Time.deltaTime;if(Time.time>alertTick){alertTick=Time.time+1;Session.Alert=Mathf.Max(0,Session.Alert-.035f);}if(Player&&Player.Input.Pause.WasPressedThisFrame())Pause();}else if(Player&&!InMenu&&!Loading&&Player.Input.Pause.WasPressedThisFrame()&&UI.Paused)Resume();if(InMenu&&cameraMain){float a=Time.unscaledTime*.018f;cameraMain.transform.position=new Vector3(-17+Mathf.Sin(a)*2,9,-23+Mathf.Cos(a)*2);cameraMain.transform.LookAt(new Vector3(0,1,0));}}
        public void Play(int index){if(Loading)return;index=Mathf.Clamp(index,1,config.MaxLevels);transition=StartCoroutine(LoadLevel(index));}
        public void Menu(){if(!Loading)transition=StartCoroutine(ShowMenuRoutine());}
        IEnumerator ShowMenuRoutine(){Loading=true;Session.Running=false;Time.timeScale=1;Unlock();UI.Loading("BOGOTÁ TE ESPERA");yield return SceneManager.LoadSceneAsync("MainMenu");Player=null;Session.Reset(config.levels[0]);World=LevelWorld.Create(config.levels[0],config.catalog,false);CreateCamera();InMenu=true;Loading=false;UI.MainMenu();}
        IEnumerator LoadLevel(int index)
        {
            Loading=true;Session.Running=false;Time.timeScale=1;Unlock();UI.Loading("PRÓXIMA PARADA\n"+config.levels[index-1].station.ToUpperInvariant());yield return SceneManager.LoadSceneAsync("Gameplay");Session.Reset(config.levels[index-1]);InMenu=false;yield return null;World=LevelWorld.Create(Session.Level,config.catalog,true);yield return null;
            var p=new GameObject("Player");p.layer=8;p.transform.position=Session.Level.spawn;var cc=p.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.28f;cc.center=Vector3.up*.9f;cc.stepOffset=.32f;cc.slopeLimit=48;cc.skinWidth=.035f;
            var visual=WorldArt.Model(config.catalog,"player",p.transform,Vector3.zero,Quaternion.identity);Player=p.AddComponent<PlayerController>();Player.Pose=visual?visual.GetComponentInChildren<PoseDriver>():null;var hand=new GameObject("Hand socket");hand.transform.SetParent(Player.Pose&&Player.Pose.rightForearm?Player.Pose.rightForearm:p.transform,false);hand.transform.localPosition=Player.Pose?new Vector3(-.22f,0,0):new Vector3(.4f,1.2f,.35f);Player.Hand=hand.transform;Player.KnifeVisual=WorldArt.Model(config.catalog,"knife",Player.Hand,Vector3.zero,Quaternion.Euler(0,0,90));
            CreateCamera();var vc=new GameObject("Cinemachine orbital camera").AddComponent<CinemachineCamera>();vc.transform.position=p.transform.position+new Vector3(0,2,-4);cameraMain.gameObject.AddComponent<CinemachineBrain>();var orbit=cameraMain.gameObject.AddComponent<OrbitCamera>();orbit.Player=Player;orbit.VirtualCamera=vc;Player.View=cameraMain.transform;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--benchmark")<0){Session.Save.current=index;SaveSystem.Write(Session.Save);}Loading=false;Session.Running=true;UI.Hud();Lock();GameEvents.Notify("Encuentra una entrada. Sube al bus cuando abra las puertas.");
        }
        void CreateCamera(){cameraMain=new GameObject("Main Camera").AddComponent<Camera>();cameraMain.tag="MainCamera";cameraMain.nearClipPlane=.08f;cameraMain.farClipPlane=Session.Save.drawDistance;cameraMain.backgroundColor=RenderSettings.fogColor;cameraMain.clearFlags=CameraClearFlags.SolidColor;cameraMain.gameObject.AddComponent<AudioListener>();cameraMain.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=Session.Save.post;}
        public void Pause(){Session.Running=false;Time.timeScale=0;Unlock();UI.Pause();}
        public void Resume(){Time.timeScale=1;Session.Running=true;UI.Hud();Lock();}
        void OnDeath(string reason){Session.Running=false;Time.timeScale=0;Unlock();UI.Death(reason);}
        void OnComplete(){Session.Running=false;Time.timeScale=0;Unlock();int i=Session.Level.index-1;LastStars=Session.Hits==0&&Session.Alerts<=1?3:Session.Hits<5?2:1;Session.Save.stars[i]=Mathf.Max(Session.Save.stars[i],LastStars);if(Session.Save.bestTimes[i]<=0||Session.Elapsed<Session.Save.bestTimes[i])Session.Save.bestTimes[i]=Session.Elapsed;Session.Save.unlocked=Mathf.Min(config.MaxLevels,Mathf.Max(Session.Save.unlocked,i+2));Session.Save.current=Mathf.Min(config.MaxLevels,i+2);SaveSystem.Write(Session.Save);UI.Results();}
        public void NewGame(){Session.Save.unlocked=Session.Save.current=1;Session.Save.stars=new int[10];Session.Save.bestTimes=new float[10];SaveSystem.Write(Session.Save);Play(1);}
        public void ApplySettings(){var s=Session.Save;AudioListener.volume=s.master;QualitySettings.SetQualityLevel(Mathf.Clamp(s.quality,0,QualitySettings.names.Length-1));QualitySettings.vSyncCount=s.vsync?1:0;Application.targetFrameRate=s.fps;QualitySettings.shadowDistance=s.quality==0?0:60;QualitySettings.globalTextureMipmapLimit=s.quality==0?1:0;QualitySettings.antiAliasing=s.quality>=2?4:2;if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)urp.renderScale=s.renderScale;if(cameraMain){cameraMain.farClipPlane=s.drawDistance;cameraMain.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing=s.post;}SaveSystem.Write(s);}
        public void ApplyDisplay(){int[] widths={1280,1920,2560};int[] heights={720,1080,1440};int r=Mathf.Clamp(Session.Save.resolution,0,2);Screen.SetResolution(widths[r],heights[r],Session.Save.windowMode==0?FullScreenMode.FullScreenWindow:Session.Save.windowMode==1?FullScreenMode.Windowed:FullScreenMode.ExclusiveFullScreen);}
        static void Lock(){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}static void Unlock(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    }
}

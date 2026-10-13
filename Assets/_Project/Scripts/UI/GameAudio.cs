using UnityEngine;
namespace Transmilleno
{
    public sealed class GameAudio : MonoBehaviour
    {
        AudioSource music,ambience;readonly AudioSource[] pool=new AudioSource[12];AudioClip click,impact,city,calm;int cursor;
        void Start(){click=Tone("UI",660,.07f,false);impact=Tone("Impact",90,.18f,true);city=Tone("City",43,4,true);calm=Music();music=gameObject.AddComponent<AudioSource>();music.clip=calm;music.loop=true;music.Play();ambience=gameObject.AddComponent<AudioSource>();ambience.clip=city;ambience.loop=true;ambience.Play();for(int i=0;i<pool.Length;i++){var g=new GameObject("Spatial voice "+i);g.transform.SetParent(transform);pool[i]=g.AddComponent<AudioSource>();pool[i].rolloffMode=AudioRolloffMode.Linear;pool[i].minDistance=2;pool[i].maxDistance=30;}GameEvents.Noise+=Noise;GameEvents.PlayerHurt+=Hurt;}
        void OnDestroy(){GameEvents.Noise-=Noise;GameEvents.PlayerHurt-=Hurt;Destroy(click);Destroy(impact);Destroy(city);Destroy(calm);}
        void Update(){if(!music||Session.Save==null)return;music.volume=Session.Save.music*.25f;music.pitch=Mathf.Lerp(music.pitch,1+Session.Alert*.055f,Time.unscaledDeltaTime);ambience.volume=Session.Save.ambience*.15f;}
        void Noise(Vector3 p,float radius){var a=pool[cursor++%pool.Length];a.transform.position=p;a.spatialBlend=radius==0?0:1;a.volume=Session.Save.effects*(radius==0?.3f:.45f);a.clip=radius==0?click:impact;a.Play();}
        void Hurt(float amount){Noise(Session.Player?Session.Player.transform.position:Vector3.zero,3);}
        static AudioClip Tone(string name,float hz,float seconds,bool noise){int n=Mathf.CeilToInt(22050*seconds);var samples=new float[n];var random=new System.Random(28);for(int i=0;i<n;i++){float time=i/22050f;float envelope=seconds>1?.7f:Mathf.Pow(1-(float)i/n,2);samples[i]=((noise?((float)random.NextDouble()*2-1)*.15f:0)+Mathf.Sin(time*hz*Mathf.PI*2)*.25f)*envelope;}var clip=AudioClip.Create(name,n,1,22050,false);clip.SetData(samples,0);return clip;}
        static AudioClip Music(){int n=22050*16;var data=new float[n];float[] notes={130.81f,155.56f,174.61f,116.54f};for(int i=0;i<n;i++){float t=i/22050f;float note=notes[(int)(t/4)%4];float beat=t% .5f;float kick=Mathf.Sin(2*Mathf.PI*(48*t+Mathf.Exp(-beat*20)*2))*Mathf.Exp(-beat*24)*.14f;data[i]=(Mathf.Sin(t*note*2*Mathf.PI)+Mathf.Sin(t*note*1.5f*2*Mathf.PI)*.4f)*.055f+kick;}var c=AudioClip.Create("Ciudad nocturna · original procedural",n,1,22050,false);c.SetData(data,0);return c;}
    }
}

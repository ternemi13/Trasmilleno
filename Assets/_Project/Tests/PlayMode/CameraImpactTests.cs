using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Cinemachine;
namespace Transmilleno.Tests
{
    public class CameraImpactTests
    {
        GameObject player, ground, bus, camera, view, wall;
        LevelDefinition level;
        Mouse mouse; InputSettings.BackgroundBehavior background;
        [UnitySetUp] public IEnumerator Setup()
        {
            background=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            Session.Config = null; Session.Save = new SaveData();
            level = ScriptableObject.CreateInstance<LevelDefinition>(); level.index=1; level.stopDuration=30;
            Session.Reset(level);
            ground = new GameObject("Ground"); ground.transform.position = Vector3.down*.12f;
            ground.AddComponent<BoxCollider>().size = new Vector3(100,.2f,200);
            player = new GameObject("Player"); player.layer=8;
            var cc=player.AddComponent<CharacterController>(); cc.height=1.75f; cc.radius=.28f; cc.center=Vector3.up*.9f;
            player.AddComponent<AudioListener>();
            view = new GameObject("View");
            player.AddComponent<PlayerController>().View=view.transform;
            mouse=InputSystem.AddDevice<Mouse>("Camera QA mouse");
            yield return null; Session.Running=true;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            InputSystem.settings.backgroundBehavior=background;
            Session.Running=false; Time.timeScale=1;
            foreach(var g in new[]{player,ground,bus,camera,view,wall}) if(g)Object.Destroy(g);
            Object.Destroy(level); InputSystem.RemoveDevice(mouse); yield return null;
        }
        void Position(Vector3 p)
        {
            var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=p;cc.enabled=true;Physics.SyncTransforms();
        }
        void CreateBus(bool reverse=false)
        {
            bus=new GameObject("Physical bus");bus.layer=10;
            var box=bus.AddComponent<BoxCollider>();box.center=Vector3.up*1.45f;box.size=new Vector3(2.8f,2.9f,17.8f);
            bus.AddComponent<BusController>().Reverse=reverse;
        }
        [UnityTest] public IEnumerator WheelZoomClampsAndKeepsOrientationStable()
        {
            camera=new GameObject("Camera test");var orbit=camera.AddComponent<OrbitCamera>();
            orbit.Player=player.GetComponent<PlayerController>();orbit.VirtualCamera=view.AddComponent<CinemachineCamera>();
            yield return null;
            Quaternion rotation=view.transform.rotation;
            InputSystem.QueueStateEvent(mouse,new MouseState {scroll=new Vector2(0,120)});
            yield return null; yield return null;
            Assert.That(orbit.RequestedDistance,Is.EqualTo(4).Within(.01f));
            Assert.That(Quaternion.Angle(rotation,view.transform.rotation),Is.LessThan(.01f));
            for(int i=0;i<20;i++)orbit.Zoom(120);
            Assert.AreEqual(OrbitCamera.MinDistance,orbit.RequestedDistance);
            for(int i=0;i<30;i++)orbit.Zoom(-120);
            Assert.AreEqual(OrbitCamera.MaxDistance,orbit.RequestedDistance);
            wall=new GameObject("Behind camera");wall.transform.position=new Vector3(0,1.5f,-1.5f);
            wall.AddComponent<BoxCollider>().size=new Vector3(20,5,.2f);Physics.SyncTransforms();
            yield return null;yield return null;
            Assert.Greater(view.transform.position.z,-1.4f,"Camera must retract before the wall");
        }
        [UnityTest] public IEnumerator FrontSurfaceContactKillsAndLaunchesWithoutRestoringStamina()
        {
            CreateBus(); var p=player.GetComponent<PlayerController>();
            float deadline=Time.time+7;
            while(bus.GetComponent<BusController>().Speed<2 && Time.time<deadline)yield return null;
            Position(bus.transform.position+Vector3.forward*9.18f);
            p.Stamina.Spend(70);float stamina=p.Stamina.Value;
            deadline=Time.time+1;
            while(!p.IsDead&&Time.time<deadline)yield return null;
            Assert.True(p.IsDead,"Actual front bumper contact must kill");Assert.NotNull(p.Ragdoll);
            Vector3 start=p.Ragdoll.Focus;
            yield return new WaitForSeconds(.3f);
            Assert.Greater(p.Ragdoll.Focus.z,start.z+1);
            Assert.Greater(p.Ragdoll.Focus.y,start.y+.4f);
            Assert.That(p.Stamina.Value,Is.EqualTo(stamina).Within(.01f));
        }
        [UnityTest] public IEnumerator SideContactInReverseDirectionIsLethal()
        {
            CreateBus(true);var p=player.GetComponent<PlayerController>();float deadline=Time.time+12;
            while(bus.GetComponent<BusController>().Speed<2&&Time.time<deadline)yield return null;
            Position(bus.transform.position+Vector3.right*1.69f);
            deadline=Time.time+1;while(!p.IsDead&&Time.time<deadline)yield return null;
            Assert.True(p.IsDead,"Bus side contact must not miss the controller capsule");
            Assert.Less(player.GetComponent<Rigidbody>().linearVelocity.z,0);
        }
        [UnityTest] public IEnumerator ParkedBusDoesNotRefillStaminaAndBoardingFreezesIt()
        {
            CreateBus();var p=player.GetComponent<PlayerController>();yield return null;
            Position(bus.transform.position+Vector3.right*1.69f);p.Stamina.Spend(70);
            yield return new WaitForSeconds(.5f);
            Assert.False(p.IsDead);Assert.That(p.Stamina.Value,Is.EqualTo(30).Within(.01f));
            yield return new WaitForSeconds(.8f);
            Assert.That(p.Stamina.Value,Is.InRange(33f,39f),"Only normal time-based regeneration is allowed");
            p.Board(bus.transform);float stamina=p.Stamina.Value;
            yield return new WaitForSeconds(.4f);Assert.AreEqual(stamina,p.Stamina.Value);
        }
        [UnityTest] public IEnumerator OriginalPlayerMeshHasAnArticulatedImpactBody()
        {
            var config=Resources.Load<GameProgressionConfig>("Progression");
            var visual=Object.Instantiate(config.catalog.Get("player").model,player.transform);
            var p=player.GetComponent<PlayerController>();p.Pose=visual.GetComponent<PoseDriver>();p.Pose.Initialize();
            yield return null;
            p.TakeDamage(new DamageInfo(999,DamageType.Bus,null,new Vector3(0,6.5f,10)));
            Assert.AreEqual(11,visual.GetComponentsInChildren<Rigidbody>().Length);
            Assert.AreEqual(10,visual.GetComponentsInChildren<CharacterJoint>().Length);
            Vector3 start=p.Ragdoll.Focus;yield return new WaitForSeconds(.4f);
            Assert.Greater(p.Ragdoll.Focus.z,start.z+1);
            Assert.Greater(p.Ragdoll.Focus.y,start.y+.3f);
            Assert.Less(Vector3.Dot(p.Pose.hips.up,Vector3.up),.9f,"Impact must tumble the torso, not translate an upright statue");
            Assert.Less(Vector3.Distance(p.Pose.head.position,p.Pose.hips.position),1.2f,"Ragdoll joints must keep the mesh attached");
        }
    }
}

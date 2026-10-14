using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Transmilleno.Tests
{
    public class StationRegressionTests
    {
        LevelWorld world; GameObject player;
        [UnitySetUp] public IEnumerator Setup()
        {
            Session.Config=Resources.Load<GameProgressionConfig>("Progression");Session.Save=new SaveData();Session.Reset(Session.Config.levels[0]);
            world=LevelWorld.Create(Session.Level,Session.Config.catalog,true);
            player=new GameObject("Traversal test");player.layer=8;var cc=player.AddComponent<CharacterController>();cc.height=1.75f;cc.center=Vector3.up*.9f;cc.radius=.25f;
            player.AddComponent<AudioListener>();player.AddComponent<PlayerController>();yield return null;Session.Running=true;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Session.Running=false;Object.Destroy(player);Object.Destroy(world.gameObject);yield return null;}
        void Position(Vector3 pos,Vector3 forward){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=pos;player.transform.forward=forward;cc.enabled=true;Physics.SyncTransforms();}
        [UnityTest] public IEnumerator OriginalTurnstilesAreVaultable()
        {
            Position(new Vector3(.62f,1.04f,-.8f),Vector3.forward);
            Assert.True(player.GetComponent<PlayerController>().TryClimb(),"Original entrance turnstile must accept vault");
            yield return new WaitForSeconds(.85f);
            Assert.Greater(player.transform.position.z,.5f);Assert.Less(player.transform.position.y,1.2f);
        }
        [UnityTest] public IEnumerator OriginalAccessFenceSupportsClimbing()
        {
            Position(new Vector3(-2.8f,.02f,-5),Vector3.right);
            Assert.True(player.GetComponent<PlayerController>().TryClimb());
            Assert.AreEqual(PlayerController.MoveState.Fence,player.GetComponent<PlayerController>().State);
            var p=player.GetComponent<PlayerController>();p.View=player.transform;
            var keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));
            float deadline=Time.time+4;
            while(player.transform.position.x < -1.3f && Time.time<deadline)yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            Assert.Greater(player.transform.position.x,-StationLayout.HalfWidth+.3f,"Entire player capsule must cross the original access rail");
            Assert.AreNotEqual(PlayerController.MoveState.Fence,p.State);
            Assert.AreNotEqual(PlayerController.MoveState.Vaulting,p.State);
        }
        [UnityTest] public IEnumerator BothDirectionsUseTheOriginalPlatformDoors()
        {
            Assert.NotNull(world.Bus);Assert.NotNull(world.SouthBus);
            Assert.AreEqual(2,world.Bus.PlatformDoors.Length);Assert.AreEqual(2,world.SouthBus.PlatformDoors.Length);
            foreach(var d in world.Bus.PlatformDoors)Assert.NotNull(d);
            foreach(var d in world.SouthBus.PlatformDoors)Assert.NotNull(d);
            float deadline=Time.time+24;
            while(world.SouthBus.State!=BusController.BusState.Boarding && Time.time<deadline)yield return null;
            Assert.AreEqual(BusController.BusState.Boarding,world.SouthBus.State);
            Assert.Less(world.SouthBus.transform.forward.z,-.99f);
            Assert.Greater(world.Bus.transform.forward.z,.99f);
            Assert.That(world.SouthBus.transform.position.z,Is.EqualTo(world.SouthBus.Stop.z).Within(.05f));
            Assert.That(world.SouthBus.BoardPoint.y,Is.EqualTo(StationLayout.Floor));
        }
    }
}

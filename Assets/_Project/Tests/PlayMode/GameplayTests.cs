using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Transmilleno.Tests
{
    public class GameplayTests
    {
        GameObject ground,player,obstacle,bus;LevelDefinition level; Keyboard testKeyboard; InputSettings.BackgroundBehavior background;
        [UnitySetUp] public IEnumerator Setup(){background=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;testKeyboard=InputSystem.AddDevice<Keyboard>("Gameplay QA keyboard");level=ScriptableObject.CreateInstance<LevelDefinition>();level.index=1;level.stopDuration=.4f;level.busInterval=1;Session.Save=new SaveData();Session.Reset(level);ground=new GameObject("Test ground");var box=ground.AddComponent<BoxCollider>();box.size=new Vector3(100,.2f,200);ground.transform.position=Vector3.down*.11f;player=new GameObject("Test player");player.layer=8;player.transform.position=Vector3.zero;var cc=player.AddComponent<CharacterController>();cc.height=1.75f;cc.center=Vector3.up*.9f;cc.radius=.28f;player.AddComponent<PlayerController>();yield return null;Session.Running=true;}
        [UnityTearDown] public IEnumerator TearDown(){InputSystem.RemoveDevice(testKeyboard);InputSystem.settings.backgroundBehavior=background;Session.Running=false;Time.timeScale=1;Object.Destroy(player);Object.Destroy(ground);if(obstacle)Object.Destroy(obstacle);if(bus)Object.Destroy(bus);Object.Destroy(level);yield return null;}
        [UnityTest] public IEnumerator PlayerVaultsMarkedObstacle(){obstacle=new GameObject("Vault test");obstacle.transform.position=new Vector3(0,.5f,.8f);var c=obstacle.AddComponent<BoxCollider>();c.size=new Vector3(2,1,.3f);obstacle.AddComponent<Climbable>();Physics.SyncTransforms();var p=player.GetComponent<PlayerController>();Assert.True(p.TryClimb());yield return new WaitForSeconds(.85f);Assert.Greater(player.transform.position.z,1.4f);Assert.Greater(player.transform.position.y,-.1f);}
        [UnityTest] public IEnumerator BusImpactIsLethal(){var p=player.GetComponent<PlayerController>();player.GetComponent<CharacterController>().enabled=false;player.transform.position=new Vector3(5.5f,0,-56);player.GetComponent<CharacterController>().enabled=true;bus=new GameObject("Moving bus");bus.AddComponent<BusController>();float deadline=Time.time+10;while(!p.IsDead&&Time.time<deadline)yield return null;Assert.True(p.IsDead);Assert.AreEqual(PlayerController.MoveState.Dead,p.State);}
        [UnityTest] public IEnumerator BoardingRequiresOpenDoorsAndPaidZoneAccess(){bus=new GameObject("Test bus");var b=bus.AddComponent<BusController>();yield return null;Assert.False(b.TryBoard());float deadline=Time.time+18;while(b.State!=BusController.BusState.Boarding&&Time.time<deadline)yield return null;Assert.AreEqual(BusController.BusState.Boarding,b.State);player.GetComponent<CharacterController>().enabled=false;player.transform.position=b.Stop+new Vector3(-2.7f,.4f,1.5f);Assert.False(b.TryBoard());Session.Trespassing=true;var keyboard=testKeyboard;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return null;bool boarded=b.Boarded||b.TryBoard();InputSystem.QueueStateEvent(keyboard,new KeyboardState());Assert.True(boarded);bool completed=false;System.Action completion=()=>completed=true;GameEvents.LevelCompleted+=completion;yield return new WaitForSeconds(2.1f);GameEvents.LevelCompleted-=completion;Assert.True(completed);}
    }
}

using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace Transmilleno.Tests
{
    public class CoreTests
    {
        [Test] public void ExhaustionRejectsOverspend(){var s=new Stamina();Assert.True(s.Spend(90));Assert.False(s.Spend(11));Assert.AreEqual(10,s.Value);}
        [Test] public void StaminaWaitsThenRegenerates(){var s=new Stamina();s.Spend(50);s.Tick(.5f);Assert.AreEqual(50,s.Value);s.Tick(.6f);s.Tick(1);Assert.Greater(s.Value,50);s.Tick(100);Assert.AreEqual(100,s.Value);}
        [Test] public void SaveRoundTripPreservesProgress(){string p=Path.Combine(Path.GetTempPath(),"transmilleno-test-"+System.Guid.NewGuid()+".json");try{var d=new SaveData{unlocked=5,current=4,sensitivity=1.7f};d.stars[2]=3;Assert.True(SaveSystem.Write(d,p));var loaded=SaveSystem.Load(p);Assert.AreEqual(5,loaded.unlocked);Assert.AreEqual(3,loaded.stars[2]);Assert.AreEqual(1.7f,loaded.sensitivity);}finally{if(File.Exists(p))File.Delete(p);}}
        [Test] public void CorruptSaveRecoversBackup(){string p=Path.Combine(Path.GetTempPath(),"transmilleno-backup-"+System.Guid.NewGuid()+".json");try{SaveSystem.Write(new SaveData{unlocked=4},p);SaveSystem.Write(new SaveData{unlocked=7},p);File.WriteAllText(p,"broken");var d=SaveSystem.Load(p);Assert.AreEqual(4,d.unlocked);}finally{foreach(var x in new[]{p,p+".bak",p+".tmp"})if(File.Exists(x))File.Delete(x);}}
        [Test] public void CampaignContainsTenProgressiveLevels(){var c=Resources.Load<GameProgressionConfig>("Progression");Assert.NotNull(c);Assert.AreEqual(10,c.levels.Length);for(int i=0;i<10;i++){Assert.AreEqual(i+1,c.levels[i].index);Assert.GreaterOrEqual(c.levels[i].routes.Length,3);Assert.Greater(c.levels[i].busInterval,0);if(i>0)Assert.GreaterOrEqual(c.levels[i].guards+c.levels[i].police,c.levels[i-1].guards+c.levels[i-1].police);}}
        [Test] public void CatalogIncludesAllUserModelRoles(){var c=Resources.Load<GameProgressionConfig>("Progression").catalog;foreach(var key in new[]{"player","guard","police","pedestrian","pedestrian2","bus","station","stationSabana","knife","stone","can","pistol"})Assert.NotNull(c.Get(key)?.model,key);}
        [Test] public void DamagePayloadPreservesTypeAndForce(){var info=new DamageInfo(38,DamageType.Knife,null,new Vector3(1,0,0));Assert.AreEqual(38,info.amount);Assert.AreEqual(DamageType.Knife,info.type);Assert.AreEqual(Vector3.right,info.knockback);}
    }
}

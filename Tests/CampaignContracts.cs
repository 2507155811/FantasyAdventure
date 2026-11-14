using System;
using System.Collections.Generic;
// Storage and reward boundary stand-ins: these contracts do not run the Unity physics/render loop.
namespace UnityEngine {
 public static class Mathf {public static int Clamp(int v,int min,int max){return Math.Max(min,Math.Min(max,v));} public static int Min(int a,int b){return Math.Min(a,b);} }
 public static class PlayerPrefs {
  static readonly Dictionary<string,string> data=new Dictionary<string,string>();
  public static int GetInt(string k,int f=0){return data.ContainsKey(k)?int.Parse(data[k]):f;}
  public static string GetString(string k,string f=""){return data.ContainsKey(k)?data[k]:f;}
  public static void SetInt(string k,int v){data[k]=v.ToString();} public static void SetString(string k,string v){data[k]=v;}
  public static bool HasKey(string k){return data.ContainsKey(k);} public static void DeleteKey(string k){data.Remove(k);} public static void Save(){}
 }
}
public class ChapterOne { public static ChapterOne Instance=new ChapterOne(); public static implicit operator bool(ChapterOne value){return value!=null;} public int Rewards; public void AddReward(int coins,int xp){Rewards++;} }
public static class CampaignContracts {
 static int checks; static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;}
 public static void Main(){
  UnityEngine.PlayerPrefs.SetInt("FA.coins",77);UnityEngine.PlayerPrefs.SetInt("FA.weapon",3);
  GameSave.MigrateLegacy();Check(GameSave.GetInt("coins")==77&&GameSave.GetInt("weapon")==3&&GameSave.GetInt("weaponOwned.3")==1,"legacy migration preserves selected weapon ownership");
  Check(CampaignCatalog.Maps.Length==67&&CampaignCatalog.Quests.Length==99,"catalog coverage");
  var unique=new HashSet<int>(); foreach(int id in CampaignCatalog.Route){Check(id>=0&&id<67&&unique.Add(id),"route unique");}
  Check(CampaignCatalog.Route[0]==41&&CampaignCatalog.Route[CampaignCatalog.Route.Length-1]==40,"story endpoints");
  foreach(var map in CampaignCatalog.Maps){
   Check(map.Id>=0&&map.Id<67&&map.Theme>=0&&map.Theme<8&&map.Npc>=0&&map.Npc<8,"map artwork indices");
   GameSave.New(0);CampaignQuestLog.Enter(map.Id);
   foreach(var q in CampaignCatalog.Quests){
    Check(q.Map>=-1&&q.Map<67&&q.Item>=0&&q.Item<8&&q.Count>0,"quest bounds");
    if(q.Map!=map.Id)continue;
    Check(!map.Safe||q.Goal!=QuestGoal.Kill,"safe maps cannot require enemy kills");
    if(q.Goal==QuestGoal.Kill)Check(q.Count<=6,"available enemies cover kill objective");
    if(q.Goal!=QuestGoal.Reach)CampaignQuestLog.Event(q.Goal,map.Id,q.Item,q.Count);
   }
   Check(CampaignQuestLog.ReadyToLeave(map.Id),"map objectives can settle");
   CampaignQuestLog.Settle(map.Id,true);int rewards=ChapterOne.Instance.Rewards;
   CampaignQuestLog.Settle(map.Id,true);Check(ChapterOne.Instance.Rewards==rewards,"quest rewards idempotent");
   foreach(var q in CampaignCatalog.Quests)if(q.Map==map.Id)Check(CampaignQuestLog.Done(q),"map quest completed");
  }
  GameSave.New(0);GameSave.SetInt("coins",123);GameSave.SetString("custom","first");GameSave.Save();
  GameSave.New(1);Check(GameSave.GetInt("coins")==30,"slot isolation");GameSave.SetInt("coins",456);GameSave.Save();
  GameSave.Select(0);Check(GameSave.GetInt("coins")==123&&GameSave.GetString("custom")=="first","slot restoration");
  GameSave.New(0);Check(GameSave.GetInt("coins")==30&&GameSave.GetString("custom")=="","new game removes registered progress");
  GameSave.Select(1);Check(GameSave.GetInt("coins")==456,"reset preserves other slot");
  foreach(var q in CampaignCatalog.Quests)if(q.Goal==QuestGoal.Reach&&q.Map>=0){
   GameSave.New(2);CampaignQuestLog.Enter(q.Map);CampaignQuestLog.Settle(q.Map);
   Check(!CampaignQuestLog.Done(q),"talk does not prematurely satisfy exit quest");
   CampaignQuestLog.Settle(q.Map,true);Check(CampaignQuestLog.Done(q),"exit satisfies reach quest");
  }
  Console.WriteLine("Completed "+checks+" campaign data/save/quest contracts across all 67 regions.");
 }
}

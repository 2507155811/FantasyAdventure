using System;
using System.Collections.Generic;
namespace UnityEngine {
 public class Object {public static implicit operator bool(Object o){return o!=null;}}
 public class MonoBehaviour:Object {}
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}}
 public class Transform {public Vector3 position;}
 public static class Time {public static float timeScale=1;}
 public static class Mathf {public static int Clamp(int v,int a,int b){return Math.Max(a,Math.Min(b,v));}public static int Max(int a,int b){return Math.Max(a,b);}public static int Min(int a,int b){return Math.Min(a,b);}}
 public class Camera:Object {public static Camera main;public T GetComponent<T>(){return default(T);}}
 public static class PlayerPrefs {
  static readonly Dictionary<string,string> data=new Dictionary<string,string>();
  public static int GetInt(string k,int f=0){return data.ContainsKey(k)?int.Parse(data[k]):f;}
  public static string GetString(string k,string f=""){return data.ContainsKey(k)?data[k]:f;}
  public static void SetInt(string k,int v){data[k]=v.ToString();} public static void SetString(string k,string v){data[k]=v;}
  public static bool HasKey(string k){return data.ContainsKey(k);}public static void DeleteKey(string k){data.Remove(k);}public static void Save(){}
 }
}
public class PlayerController2D:UnityEngine.Object {
 public int health=100,maxHealth=100,mana=60,maxMana=60,redPotions=3,bluePotions=2;
 public UnityEngine.Transform transform=new UnityEngine.Transform();public WeaponEquipment equipment=new WeaponEquipment();
 public void RecoverAt(UnityEngine.Vector3 p){transform.position=p;health=maxHealth;mana=maxMana;}
 public void TakeDamage(int amount){health-=amount;}
}
public class WeaponEquipment {public void ReloadSave(){}}
public class CampaignStageGenerator:UnityEngine.Object {public float Width=100;public int Builds;public void Build(CampaignMap map){Builds++;}}
public class DemoHUD:UnityEngine.Object {public void ShowTitle(){}public static void ShowMessage(string s){}}
public class CameraFollow2D:UnityEngine.Object {public void SnapToTarget(){}}
public enum GameSound {Upgrade,Clear,Coin}
public class GameAudio:UnityEngine.Object {public static GameAudio Instance;public static void Play(GameSound s){}public void ChangeTheme(int theme){}}
public static class CampaignFlowContracts {
 static int checks;static void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
 static void Complete(ChapterOne game){
  if(!game.Map.Safe){for(int i=0;i<6;i++)game.Reward(5,false);if(game.Map.Boss)game.Reward(35,true);}
  foreach(var quest in CampaignCatalog.Quests){if(quest.Map!=game.MapId)continue;
   if(quest.Goal==QuestGoal.Collect)game.AddItem(quest.Item,quest.Count);
   else if(quest.Goal==QuestGoal.Talk)game.Talk();
   else if(quest.Goal==QuestGoal.Skill||quest.Goal==QuestGoal.Upgrade)CampaignQuestLog.Event(quest.Goal,game.MapId,quest.Item,quest.Count);
  }
  game.ExitMap();
 }
 public static void Main(){
  var game=new ChapterOne{player=new PlayerController2D(),stage=new CampaignStageGenerator(),hud=new DemoHUD()};
  typeof(ChapterOne).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(game,null);
  game.StartSession(0,true);Check(game.MapId==41&&game.Playing,"new game home");
  Check(!game.IsUnlocked(40)&&game.IsUnlocked(66)&&!game.IsUnlocked(52),"story and postgame locks");
  game.Travel(40);Check(game.MapId==41,"locked fast travel blocked");
  for(int i=0;i<CampaignCatalog.Route.Length;i++){
   int map=CampaignCatalog.Route[i];Check(game.MapId==map,"main route sequence "+map);
   if(!game.Map.Safe){game.ExitMap();Check(game.MapId==map&&!game.Completed,"combat gate cannot be skipped");}
   Complete(game);
   Check(GameSave.GetInt("mapClear."+map)==1,"region completion saved");
  }
  Check(game.Completed&&GameSave.GetInt("storyClear")==1,"main story reaches ending");
  int endingCoins=game.Coins;game.ExitMap();Check(game.Coins==endingCoins,"ending reward once");
  Check(game.IsUnlocked(52)&&!game.IsUnlocked(64),"postgame opens first floor");
  game.Travel(52);for(int floor=52;floor<=64;floor++){
   Check(game.MapId==floor&&game.IsUnlocked(floor),"dungeon floor unlock "+floor);Complete(game);
  }
  Check(game.MapId==52&&GameSave.GetInt("dungeonCycle")==1,"dungeon completion starts next cycle");
  game.Travel(66);Check(game.RequiredKills==0,"training room has no kill gate");game.ExitMap();Check(game.MapId==41,"training room exit");
  game.Save();int coins=game.Coins,xp=game.Experience;
  game.ReturnTitle();Check(!game.Playing&&game.MenuOpen&&UnityEngine.Time.timeScale==0,"return to title pauses");
  game.StartSession(0,false);Check(game.Coins==coins&&game.Experience==xp&&game.MapId==41,"continue restores profile");
  game.StartSession(1,true);Check(game.Coins==30&&game.Experience==0&&!game.IsUnlocked(40),"new slot is isolated");
  game.StartSession(0,false);Check(game.Coins==coins&&game.Experience==xp,"switch back restores original slot");
  Console.WriteLine("Completed "+checks+" campaign manager progression contracts: new game, 46-region ending, 13-floor challenge, title and save restore. Rendering and physics are stand-ins.");
 }
}

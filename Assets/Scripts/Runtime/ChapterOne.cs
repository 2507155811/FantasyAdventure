using UnityEngine;

// Kept under the original component name so existing scenes remain compatible.
public class ChapterOne : MonoBehaviour
{
    public static ChapterOne Instance{get;private set;}
    public PlayerController2D player;
    public CampaignStageGenerator stage;
    public DemoHUD hud;
    public int Coins{get;private set;}
    public int Experience{get;private set;}
    public int Defeated{get;private set;}
    public int Region{get;private set;}
    public bool Completed{get;private set;}
    public bool MenuOpen{get;set;}
    public bool Playing{get;private set;}
    public bool BossDefeated{get;private set;}
    public int MapId{get;private set;}=41;
    public CampaignMap Map{get{return CampaignCatalog.Maps[MapId];}}
    public int Level{get{return Mathf.Clamp(1+Experience/80,1,99);}}
    public int RequiredKills{get{return Map.Safe?0:3;}}
    public string RegionName{get{return Map.Name;}}
    public string Objective
    {
        get
        {
            if(Completed)return "时间已修复，奶奶平安归来";
            if(Map.Boss&&!BossDefeated)return "击败区域首领 · "+CampaignQuestLog.Objective(MapId);
            if(Defeated<RequiredKills)return "击退敌人 "+Defeated+"/"+RequiredKills+" · "+CampaignQuestLog.Objective(MapId);
            return CampaignQuestLog.Objective(MapId);
        }
    }
    float checkpoint=3;
    void Awake(){Instance=this;GameSave.MigrateLegacy();MenuOpen=true;Time.timeScale=0;}
    void Start(){hud.ShowTitle();}
    public void StartSession(int slot,bool fresh)
    {
        if(fresh)GameSave.New(slot);else GameSave.Select(slot);
        Coins=GameSave.GetInt("coins",0);Experience=GameSave.GetInt("exp",0);
        player.equipment.ReloadSave();Playing=true;Completed=false;MenuOpen=false;Time.timeScale=1;
        LoadMap(Mathf.Clamp(GameSave.GetInt("map",41),0,66),true);
        if(MapId==40&&GameSave.GetInt("storyClear",0)!=0)Completed=true;
    }
    void LoadMap(int map,bool restoreCheckpoint)
    {
        MapId=map;Defeated=0;BossDefeated=false;Region=0;Completed=false;
        checkpoint=restoreCheckpoint&&GameSave.GetInt("checkpointMap",-1)==map&&GameSave.GetInt("checkpoint",0)>0?stage.Width*.5f:3;
        CampaignQuestLog.Enter(map);RefreshStats(false);stage.Build(Map);
        player.redPotions=Mathf.Max(1,GameSave.GetInt("redPotions",3));
        player.bluePotions=GameSave.GetInt("bluePotions",2);
        player.RecoverAt(new Vector3(checkpoint,.5f,0));SnapCamera();
        GameSave.SetInt("map",map);GameSave.SetString("mapName",Map.Name);Save();
        if(GameAudio.Instance)GameAudio.Instance.ChangeTheme(Map.Theme);
        DemoHUD.ShowMessage(Map.Name+" · 上下爬梯，上键交谈/进入，下 + 跳跃落下平台");
    }
    void Update()
    {
        if(!Playing||MenuOpen||Completed||!player||player.health<=0)return;
        if(player.transform.position.y< -8){player.TakeDamage(99999);return;}
        if(player.transform.position.x>=stage.Width*.5f&&checkpoint<stage.Width*.5f)
        {
            checkpoint=stage.Width*.5f;Region=1;player.health=player.maxHealth;player.mana=player.maxMana;
            player.redPotions=Mathf.Max(2,player.redPotions);
            GameSave.SetInt("checkpointMap",MapId);GameSave.SetInt("checkpoint",1);Save();
            DemoHUD.ShowMessage("检查点已保存，生命与魔法恢复");
        }
    }
    public void RefreshStats(bool heal)
    {
        int oldHealth=player.maxHealth;
        player.maxHealth=100+(Level-1)*4+CampaignInventory.Passive(0)*12+GameSave.GetInt("relic",0)*10;
        player.maxMana=60+(Level-1)+CampaignInventory.Passive(2)*10+GameSave.GetInt("relic",0)*5;
        if(heal){player.health=player.maxHealth;player.mana=player.maxMana;}
        else player.health=Mathf.Clamp(player.health+Mathf.Max(0,player.maxHealth-oldHealth),0,player.maxHealth);
    }
    public void AddReward(int coins,int exp)
    {
        int previous=Level;Coins+=Mathf.Max(0,coins);Experience+=Mathf.Max(0,exp);
        if(Level>previous){RefreshStats(true);GameAudio.Play(GameSound.Upgrade);DemoHUD.ShowMessage("升级！生命与魔法已恢复，获得技能点");}
    }
    public void Reward(int coins,bool boss)
    {
        if(!Playing||Completed)return;
        Defeated++;AddReward(coins,boss?70:12);
        CampaignQuestLog.Event(QuestGoal.Kill,MapId);
        if(boss)
        {
            BossDefeated=true;AddItem(4,1);AddReward(35,30);
            CampaignInventory.UnlockWeapon(1+Map.Order%3);GameAudio.Play(GameSound.Clear);
            DemoHUD.ShowMessage("首领已击败，获得时间结晶和武器；完成任务后前往出口");
        }
        else GameAudio.Play(GameSound.Coin);
        Save();
    }
    public void AddItem(int item,int amount)
    {
        CampaignInventory.Add(item,amount);CampaignQuestLog.Event(QuestGoal.Collect,MapId,item,amount);
        GameAudio.Play(GameSound.Coin);DemoHUD.ShowMessage("获得 "+CampaignCatalog.ItemNames[item]+" ×"+amount);Save();
    }
    public void Talk()
    {
        if(Map.Boss&&!BossDefeated){DemoHUD.ShowMessage("先击败首领，再帮助这里的居民");return;}
        CampaignQuestLog.Event(QuestGoal.Talk,MapId);CampaignQuestLog.Settle(MapId);Save();
    }
    public bool SpendCoins(int amount){if(amount<0||Coins<amount)return false;Coins-=amount;Save();return true;}
    public bool IsUnlocked(int map)
    {
        if(map==66)return true;
        int index=CampaignCatalog.RouteIndex(map);
        if(index>=0)return index<=GameSave.GetInt("route",0);
        if(GameSave.GetInt("storyClear",0)==0)return false;
        if(map>=52&&map<=64)return map-52<=GameSave.GetInt("dungeonFloor",0);
        return true;
    }
    public void Travel(int map)
    {
        if(!IsUnlocked(map)){DemoHUD.ShowMessage("先完成前面的主线地区");return;}
        MenuOpen=false;Time.timeScale=1;LoadMap(map,false);
    }
    public void ExitMap()
    {
        if(Defeated<RequiredKills||Map.Boss&&!BossDefeated||!CampaignQuestLog.ReadyToLeave(MapId))
        {DemoHUD.ShowMessage("先完成当前目标："+Objective);return;}
        CampaignQuestLog.Settle(MapId,true);
        if(GameSave.GetInt("mapClear."+MapId,0)==0){GameSave.SetInt("mapClear."+MapId,1);AddReward(25,25);}
        int index=CampaignCatalog.RouteIndex(MapId);
        if(index==CampaignCatalog.Route.Length-1)
        {
            if(GameSave.GetInt("storyClear",0)==0)AddReward(300,150);
            GameSave.SetInt("storyClear",1);Completed=true;Save();
            GameAudio.Play(GameSound.Clear);DemoHUD.ShowMessage("所有时间线路恢复，奶奶和伙伴们平安归来");return;
        }
        if(index>=0)
        {
            GameSave.SetInt("route",Mathf.Max(GameSave.GetInt("route",0),index+1));
            LoadMap(CampaignCatalog.Route[index+1],false);
        }
        else
        {
            if(MapId>=52&&MapId<=63)GameSave.SetInt("dungeonFloor",Mathf.Max(GameSave.GetInt("dungeonFloor",0),MapId-51));
            int next=MapId>=52&&MapId<=63?MapId+1:MapId==65?52:MapId==64?52:41;
            if(MapId==64)GameSave.SetInt("dungeonCycle",GameSave.GetInt("dungeonCycle",0)+1);
            LoadMap(next,false);
        }
    }
    public void Retry()
    {
        player.RecoverAt(new Vector3(checkpoint,.5f,0));SnapCamera();player.redPotions=Mathf.Max(1,player.redPotions);
        MenuOpen=false;Time.timeScale=1;DemoHUD.ShowMessage("已回到检查点");
    }
    public void ReturnTitle(){Save();Playing=false;MenuOpen=true;Time.timeScale=0;hud.ShowTitle();}
    public void Save()
    {
        if(!Playing)return;
        GameSave.SetInt("coins",Coins);GameSave.SetInt("exp",Experience);
        GameSave.SetInt("redPotions",player.redPotions);GameSave.SetInt("bluePotions",player.bluePotions);GameSave.Save();
    }
    void SnapCamera(){if(Camera.main){var follow=Camera.main.GetComponent<CameraFollow2D>();if(follow)follow.SnapToTarget();}}
    void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
}

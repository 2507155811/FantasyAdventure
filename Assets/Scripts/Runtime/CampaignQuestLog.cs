using UnityEngine;

public static class CampaignQuestLog
{
    public static int Progress(CampaignQuest quest){return GameSave.GetInt("quest."+quest.Id,0);}
    public static bool Done(CampaignQuest quest){return GameSave.GetInt("questDone."+quest.Id,0)!=0;}
    public static bool Active(CampaignQuest quest){return GameSave.GetInt("questActive."+quest.Id,0)!=0;}
    public static void Enter(int map)
    {
        foreach(var quest in CampaignCatalog.Quests)
        {
            if(quest.Map!=map&&quest.Map!=-1)continue;
            GameSave.SetInt("questActive."+quest.Id,1);
            if(quest.Goal==QuestGoal.Upgrade)GameSave.SetInt("quest."+quest.Id,Mathf.Min(quest.Count,GameSave.GetInt("upgrades",0)));
        }
    }
    public static void Event(QuestGoal goal,int map,int item=0,int amount=1)
    {
        foreach(var quest in CampaignCatalog.Quests)
        {
            if(!Active(quest)||Done(quest)||quest.Goal!=goal||(quest.Map!=map&&quest.Map!=-1))continue;
            if(goal==QuestGoal.Collect&&quest.Item!=item)continue;
            GameSave.SetInt("quest."+quest.Id,Mathf.Min(quest.Count,Progress(quest)+amount));
        }
    }
    public static bool ReadyToLeave(int map)
    {
        foreach(var quest in CampaignCatalog.Quests)
            if(quest.Map==map&&!quest.Optional&&quest.Goal!=QuestGoal.Reach&&Progress(quest)<quest.Count)return false;
        return true;
    }
    public static string Objective(int map)
    {
        foreach(var quest in CampaignCatalog.Quests)
        {
            if(quest.Map!=map||Done(quest)||quest.Optional)continue;
            if(quest.Goal==QuestGoal.Reach)return quest.Title+"：到出口按上";
            if(Progress(quest)>=quest.Count)continue;
            string action=quest.Goal==QuestGoal.Collect?"收集"+CampaignCatalog.ItemNames[quest.Item]:quest.Goal==QuestGoal.Kill?"击败敌人":quest.Goal==QuestGoal.Skill?"施放技能":quest.Goal==QuestGoal.Upgrade?"升级武器":"与居民交谈";
            return quest.Title+" · "+action+" "+Progress(quest)+"/"+quest.Count;
        }
        return "前往出口，按上进入下一地区";
    }
    public static void Settle(int map,bool reachedExit=false)
    {
        if(reachedExit)Event(QuestGoal.Reach,map);
        foreach(var quest in CampaignCatalog.Quests)
        {
            if(!Active(quest)||Done(quest)||(quest.Map!=map&&quest.Map!=-1)||Progress(quest)<quest.Count)continue;
            GameSave.SetInt("questDone."+quest.Id,1);
            if(ChapterOne.Instance)ChapterOne.Instance.AddReward(15+quest.Count*3,20+quest.Count*5);
        }
    }
}

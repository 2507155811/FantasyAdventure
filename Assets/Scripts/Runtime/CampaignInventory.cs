using UnityEngine;

public static class CampaignInventory
{
    public static int Count(int item){return GameSave.GetInt("item."+item,0);}
    public static void Add(int item,int count){GameSave.SetInt("item."+item,Mathf.Clamp(Count(item)+count,0,999));}
    public static bool Spend(int item,int count)
    {
        if(count<=0||Count(item)<count)return false;
        GameSave.SetInt("item."+item,Count(item)-count);return true;
    }
    public static bool OwnsWeapon(int family){return family==0||GameSave.GetInt("weaponOwned."+family,0)!=0;}
    public static void UnlockWeapon(int family){GameSave.SetInt("weaponOwned."+family,1);}
    public static int SkillRank(int family){return GameSave.GetInt("skill."+family,1);}
    public static int Passive(int kind){return GameSave.GetInt("passive."+kind,0);}
    public static int Points
    {
        get
        {
            int level=ChapterOne.Instance?ChapterOne.Instance.Level:1;
            int spent=0;for(int i=0;i<4;i++)spent+=SkillRank(i)-1;
            for(int i=0;i<3;i++)spent+=Passive(i);
            return Mathf.Max(0,level+1-spent);
        }
    }
    public static bool Train(int family)
    {
        if(Points<=0||SkillRank(family)>=10||!OwnsWeapon(family))return false;
        GameSave.SetInt("skill."+family,SkillRank(family)+1);GameSave.Save();return true;
    }
    public static bool TrainPassive(int kind)
    {
        if(Points<=0||Passive(kind)>=20)return false;
        GameSave.SetInt("passive."+kind,Passive(kind)+1);GameSave.Save();
        if(ChapterOne.Instance)ChapterOne.Instance.RefreshStats(false);return true;
    }
}

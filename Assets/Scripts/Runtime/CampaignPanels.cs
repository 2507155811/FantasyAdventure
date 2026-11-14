using System.Collections.Generic;
using UnityEngine;

public partial class DemoHUD
{
    int titlePage,confirmSlot=-1,menuTab,questPage,questFilter,worldPage;
    string resident="";
    bool shopAuthorized;
    public void ShowTitle(){titlePage=0;confirmSlot=-1;equipmentOpen=false;}
    public void ShowResident(string name){resident=name;shopAuthorized=true;menuTab=7;ToggleEquipment(true);}
    void DrawTitle(GameplayViewport view)
    {
        var chapter=ChapterOne.Instance;
        if(chapter.stage&&chapter.stage.backgrounds!=null&&chapter.stage.backgrounds.Length>0)
        {
            Sprite backdrop=chapter.stage.backgrounds[0];Rect source=backdrop.textureRect;
            float visibleHeight=source.width*view.Height/view.Width;
            source.y+=Mathf.Max(0,(source.height-visibleHeight)*.5f);source.height=Mathf.Min(source.height,visibleHeight);
            GUI.DrawTextureWithTexCoords(new Rect(0,0,view.Width,view.Height),backdrop.texture,new Rect(source.x/backdrop.texture.width,source.y/backdrop.texture.height,source.width/backdrop.texture.width,source.height/backdrop.texture.height));
        }
        float center=view.Width*.5f;
        Panel(new Rect(center-410,80,820,560));
        GUI.Label(new Rect(center-380,105,760,65),"幻想冒险：异世界之旅",title);
        GUI.Label(new Rect(center-370,171,740,32),"家园 · 异世 · 圣地 · 时空线路",button);
        HudArtwork.Draw(new Rect(center-355,255,210,300),player.GetComponent<HeroSpriteAnimator2D>().Portrait);
        bool previousEnabled=GUI.enabled;if(confirmSlot>=0)GUI.enabled=false;
        if(titlePage==0)
        {
            if(Button(new Rect(center-70,245,330,62),"新游戏"))titlePage=2;
            if(Button(new Rect(center-70,325,330,62),"继续冒险"))titlePage=1;
            if(Button(new Rect(center-70,405,330,62),GameAudio.Instance&&GameAudio.Instance.Enabled?"声音：开启":"声音：关闭")&&GameAudio.Instance)GameAudio.Instance.Toggle();
            if(Button(new Rect(center-70,485,330,62),"退出游戏"))Application.Quit();
            GUI.Label(new Rect(center-370,585,740,30),"操作：方向盘四向移动/攀爬，跳跃、攻击、技能；上键交谈与进入",button);
        }
        else
        {
            for(int slot=0;slot<3;slot++)
            {
                string text="存档 "+(slot+1)+" · "+GameSave.Summary(slot);
                if(Button(new Rect(center-90,235+slot*95,460,78),text))
                {
                    if(titlePage==1)
                    {
                        if(GameSave.Exists(slot)){equipmentOpen=false;chapter.StartSession(slot,false);}
                        else DemoHUD.ShowMessage("这个存档还是空的，请选择新游戏");
                    }
                    else if(GameSave.Exists(slot))confirmSlot=slot;
                    else{equipmentOpen=false;chapter.StartSession(slot,true);}
                }
            }
            if(Button(new Rect(center+100,550,270,52),"返回"))titlePage=0;
        }
        GUI.enabled=previousEnabled;
        if(confirmSlot>=0)
        {
            Panel(new Rect(center-300,265,600,240));
            GUI.Label(new Rect(center-270,290,540,65),"覆盖存档 "+(confirmSlot+1)+" 并开始新游戏？",title);
            if(Button(new Rect(center-250,395,220,60),"覆盖并开始")){int slot=confirmSlot;confirmSlot=-1;chapter.StartSession(slot,true);}
            if(Button(new Rect(center+30,395,220,60),"取消"))confirmSlot=-1;
        }
        if(Time.unscaledTime<messageUntil)GUI.Label(new Rect(center-390,645,780,42),message,button);
    }
    void CampaignPanel()
    {
        float cx=(AdventureControls.View.Width-1280)*.5f;
        Panel(new Rect(160+cx,100,960,560));
        string[] tabs={"装备","背包","技能","任务","地图","商店","存档"};
        for(int i=0;i<tabs.Length;i++)if(Button(new Rect(178+cx+i*131,122,122,50),tabs[i]))menuTab=i;
        switch(menuTab)
        {
            case 0:DrawWeaponPanel();break;
            case 1:Bag(cx);break;
            case 2:Skills(cx);break;
            case 3:Quests(cx);break;
            case 4:WorldMap(cx);break;
            case 5:if(shopAuthorized&&(!ChapterOne.Instance.Map.Boss||ChapterOne.Instance.BossDefeated))Shop(cx);else GUI.Label(new Rect(220+cx,300,820,130),"与居民交谈后可以购买补给。首领尚在时，先准备好再进入战斗。",button);break;
            case 6:SavePanel(cx);break;
            default:Resident(cx);break;
        }
        if(Button(new Rect(920+cx,604,165,40),"返回游戏"))ToggleEquipment(false);
    }
    void Bag(float cx)
    {
        GUI.Label(new Rect(185+cx,194,870,35),"背包 · 时间物品与旅途补给",title);
        if(Button(new Rect(195+cx,244,410,50),"红药 ×"+player.redPotions+" · 恢复生命")){player.UseRedPotion();ChapterOne.Instance.Save();}
        if(Button(new Rect(625+cx,244,450,50),"蓝药 ×"+player.bluePotions+" · 恢复魔法")){player.UseBluePotion();ChapterOne.Instance.Save();}
        var art=ChapterOne.Instance.stage.castItems;
        for(int i=0;i<8;i++)
        {
            float x=195+cx+i%4*220,y=320+i/4*125;Panel(new Rect(x,y,205,110));
            HudArtwork.Draw(new Rect(x+8,y+8,70,85),art[8+i]);
            GUI.Label(new Rect(x+85,y+20,110,70),CampaignCatalog.ItemNames[i]+"\n× "+CampaignInventory.Count(i),label);
        }
        GUI.Label(new Rect(195+cx,568,700,30),"任务收集进度独立记录；时间结晶可用于强化护符。",label);
    }
    void Skills(float cx)
    {
        GUI.Label(new Rect(190+cx,190,880,40),"技能成长 · 可用技能点 "+CampaignInventory.Points,title);
        for(int i=0;i<4;i++)
        {
            float x=190+cx+i*225;Panel(new Rect(x,245,212,150));
            GUI.Label(new Rect(x+10,258,192,55),WeaponEquipment.Names[i]+" · "+WeaponEquipment.Skills[i],button);
            GUI.Label(new Rect(x+10,315,192,30),"等级 "+CampaignInventory.SkillRank(i)+" / 10",button);
            if(Button(new Rect(x+20,353,172,34),"提升 · 1 技能点"))
                DemoHUD.ShowMessage(CampaignInventory.Train(i)?"技能提升成功":"需要对应武器、技能点，且未达到上限");
        }
        string[] names={"体力 · 增加生命","力量 · 增加伤害","专注 · 魔法恢复"};
        for(int i=0;i<3;i++)
        {
            float x=190+cx+i*300;Panel(new Rect(x,428,286,138));
            GUI.Label(new Rect(x+10,440,266,45),names[i]+"\n等级 "+CampaignInventory.Passive(i)+" / 20",button);
            if(Button(new Rect(x+25,505,236,42),"提升 · 1 技能点"))
                DemoHUD.ShowMessage(CampaignInventory.TrainPassive(i)?"被动能力提升成功":"技能点不足或已达到上限");
        }
    }
    void Quests(float cx)
    {
        string[] filters={"进行中","已完成","全部委托"};
        for(int i=0;i<3;i++)if(Button(new Rect(190+cx+i*220,190,205,40),filters[i])){questFilter=i;questPage=0;}
        var list=new List<CampaignQuest>();
        foreach(var quest in CampaignCatalog.Quests)
            if(questFilter==2||questFilter==1&&CampaignQuestLog.Done(quest)||questFilter==0&&CampaignQuestLog.Active(quest)&&!CampaignQuestLog.Done(quest))list.Add(quest);
        int pages=Mathf.Max(1,(list.Count+6)/7);questPage=Mathf.Clamp(questPage,0,pages-1);
        for(int row=0;row<7&&questPage*7+row<list.Count;row++)
        {
            var q=list[questPage*7+row];float y=244+row*45;Panel(new Rect(190+cx,y,895,41));
            string status=CampaignQuestLog.Done(q)?"完成":CampaignQuestLog.Progress(q)+"/"+q.Count;
            GUI.Label(new Rect(205+cx,y+5,570,34),q.Title+" · "+(q.Map<0?"全世界":CampaignCatalog.Maps[q.Map].Name),label);
            GUI.Label(new Rect(790+cx,y+5,260,34),status,button);
        }
        if(list.Count==0)GUI.Label(new Rect(220+cx,330,800,60),"这里还没有委托记录。与居民交谈，继续探索新的地区。",button);
        if(Button(new Rect(190+cx,568,140,32),"上一页"))questPage--;
        GUI.Label(new Rect(350+cx,568,220,32),(questPage+1)+" / "+pages,button);
        if(Button(new Rect(590+cx,568,140,32),"下一页"))questPage++;
    }
    void WorldMap(float cx)
    {
        var ordered=new List<CampaignMap>(CampaignCatalog.Maps);ordered.Sort((a,b)=>a.Order.CompareTo(b.Order));
        int pages=(ordered.Count+17)/18;worldPage=Mathf.Clamp(worldPage,0,pages-1);
        GUI.Label(new Rect(190+cx,185,880,40),"世界地图 · 完成主线逐步开放地区",title);
        for(int i=0;i<18&&worldPage*18+i<ordered.Count;i++)
        {
            var map=ordered[worldPage*18+i];bool unlocked=ChapterOne.Instance.IsUnlocked(map.Id);
            string state=GameSave.GetInt("mapClear."+map.Id,0)!=0?" ✓":unlocked?"":" · 未解锁";
            if(Button(new Rect(190+cx+i%3*300,238+i/3*55,286,48),map.Name+state))
            {
                if(unlocked){ToggleEquipment(false);ChapterOne.Instance.Travel(map.Id);}else DemoHUD.ShowMessage("完成前面的主线地区后开放");
            }
        }
        if(Button(new Rect(190+cx,574,140,30),"上一页"))worldPage--;
        GUI.Label(new Rect(350+cx,574,220,30),(worldPage+1)+" / "+pages,button);
        if(Button(new Rect(590+cx,574,140,30),"下一页"))worldPage++;
    }
    void Shop(float cx)
    {
        var chapter=ChapterOne.Instance;
        GUI.Label(new Rect(190+cx,190,880,40),"旅途补给 · 金币 "+chapter.Coins,title);
        string[] goods={"红药 · 恢复 45% 生命","蓝药 · 恢复 50% 魔法","获得锤","获得长枪","获得法杖","强化时间护符"};
        int relic=GameSave.GetInt("relic",0);
        int[] prices={10,8,45,55,65,50*(relic+1)};
        for(int i=0;i<goods.Length;i++)
        {
            float y=242+i*54;GUI.Label(new Rect(205+cx,y+6,610,42),goods[i]+(i==5?" · 等级 "+relic+"/10":""),label);
            if(Button(new Rect(845+cx,y,220,44),prices[i]+" 金币"))
            {
                if(i>=2&&i<=4&&CampaignInventory.OwnsWeapon(i-1)){DemoHUD.ShowMessage("已经获得这类武器");continue;}
                if(i==5&&(relic>=10||CampaignInventory.Count(4)<1)){DemoHUD.ShowMessage("需要一枚时间结晶，且护符未达到上限");continue;}
                if(!chapter.SpendCoins(prices[i])){DemoHUD.ShowMessage("金币不足");continue;}
                if(i==0)player.redPotions++;else if(i==1)player.bluePotions++;
                else if(i<5)CampaignInventory.UnlockWeapon(i-1);
                else{CampaignInventory.Spend(4,1);GameSave.SetInt("relic",relic+1);chapter.RefreshStats(false);}
                chapter.Save();GameAudio.Play(GameSound.Coin);DemoHUD.ShowMessage("购买成功");
            }
        }
    }
    void SavePanel(float cx)
    {
        GUI.Label(new Rect(190+cx,195,880,45),"存档 "+(GameSave.Slot+1)+" · "+ChapterOne.Instance.RegionName,title);
        GUI.Label(new Rect(225+cx,265,820,90),"检查点、任务、金币、武器与成长会自动保存。\n返回主菜单可以选择其他存档；新游戏覆盖前会再次确认。",label);
        if(Button(new Rect(260+cx,405,300,65),"保存进度")){ChapterOne.Instance.Save();DemoHUD.ShowMessage("进度已保存");}
        if(Button(new Rect(650+cx,405,390,65),"保存并返回主菜单"))ChapterOne.Instance.ReturnTitle();
    }
    void Resident(float cx)
    {
        GUI.Label(new Rect(190+cx,190,880,50),resident,title);
        GUI.Label(new Rect(220+cx,265,820,165),CampaignStory.Dialogue(ChapterOne.Instance.Map),label);
        GUI.Label(new Rect(220+cx,435,820,55),ChapterOne.Instance.Objective,label);
        if(Button(new Rect(220+cx,520,250,60),"旅途补给"))menuTab=5;
        if(Button(new Rect(515+cx,520,250,60),"查看委托"))menuTab=3;
        if(Button(new Rect(810+cx,520,250,60),"继续旅行"))ToggleEquipment(false);
    }
}

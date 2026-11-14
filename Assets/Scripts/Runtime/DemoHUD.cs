using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoHUD : MonoBehaviour
{
    public PlayerController2D player;
    public EnemyAI2D boss;
    public Font interfaceFont;
    static string message = "";
    static float messageUntil;
    readonly VitalsFlaskHUD vitals = new VitalsFlaskHUD();
    readonly List<Texture2D> ownedTextures = new List<Texture2D>();
    GUIStyle label, title, button;
    Texture2D white;
    bool equipmentOpen;
    public static void ShowMessage(string text) { message = text; messageUntil = Time.time + 4; }

    void Styles()
    {
        if (label != null) return;
        white = new Texture2D(1,1); white.SetPixel(0,0,Color.white); white.Apply(); ownedTextures.Add(white);
        label = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
        label.normal.textColor = new Color(1,.95f,.8f);
        title = new GUIStyle(label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        button = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        if (interfaceFont) { label.font = interfaceFont; title.font = interfaceFont; button.font = interfaceFont; }
    }
    void Fill(Rect rect, Color color)
    {
        Color previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect,white); GUI.color = previous;
    }
    void Panel(Rect rect)
    {
        Fill(rect,new Color(.65f,.49f,.25f,.97f));
        Fill(new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4),new Color(.12f,.19f,.17f,.96f));
    }
    bool Button(Rect rect, string text)
    {
        Panel(rect); return GUI.Button(rect,text,button);
    }
    bool Hold(Rect rect,string text)
    {
        Panel(rect); return GUI.RepeatButton(rect,text,button);
    }
    void ToggleEquipment(bool open)
    {
        equipmentOpen = open;
        if (ChapterOne.Instance) ChapterOne.Instance.MenuOpen = open;
        Time.timeScale = open ? 0 : 1;
        DemoInput.Horizontal = 0; DemoInput.ClearOneShot();
    }
    void OnGUI()
    {
        Styles();
        Matrix4x4 previous = GUI.matrix;
        Rect safe = Screen.safeArea;
        float scale = Mathf.Min(safe.width/1280f,safe.height/720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x+(safe.width-1280*scale)*.5f,
            Screen.height-safe.yMax+(safe.height-720*scale)*.5f,0),Quaternion.identity,new Vector3(scale,scale,1));
        if (!player) { GUI.matrix = previous; return; }
        var chapter = ChapterOne.Instance;
        vitals.Draw(player,new Rect(14,12,200,116),interfaceFont);
        GUI.Label(new Rect(18,142,300,28),"等级 "+(chapter ? chapter.Level : 1)+"  金币 "+(chapter ? chapter.Coins : 0),label);
        GUI.Label(new Rect(18,172,300,28),"自动红药 ×"+player.redPotions+"  魔法自动恢复",label);
        if (chapter)
        {
            Panel(new Rect(405,14,470,74));
            GUI.Label(new Rect(420,21,440,27),chapter.RegionName,label);
            GUI.Label(new Rect(420,49,440,30),chapter.Objective,label);
        }
        if (Time.time < messageUntil)
        {
            Panel(new Rect(290,106,700,60)); GUI.Label(new Rect(307,116,670,46),message,label);
        }
        if (boss && !boss.IsDead && player.transform.position.x > 95)
        {
            Panel(new Rect(390,190,500,52));
            float fraction = Mathf.Clamp01((float)boss.Health/boss.maxHealth);
            Fill(new Rect(402,221,476*fraction,9),boss.WindingUp ? new Color(1,.65f,.2f) : new Color(.8f,.23f,.15f));
            GUI.Label(new Rect(405,192,470,28),boss.WindingUp ? "古树守卫：重击蓄力，跳起避开！" : "古树守卫  "+boss.Health+" / "+boss.maxHealth,label);
        }
        if (!equipmentOpen && player.health>0 && (!chapter || !chapter.Completed))
        {
            if(Button(new Rect(1120,20,140,56),"装备"))ToggleEquipment(true);
            bool left = Hold(new Rect(25,590,108,100),"◀");
            bool right = Hold(new Rect(149,590,108,100),"▶");
            DemoInput.Horizontal = left ? -1 : right ? 1 : 0;
            if(Button(new Rect(894,585,100,100),"跳跃"))DemoInput.JumpPressed=true;
            if(Hold(new Rect(1008,562,125,125),"攻击"))DemoInput.AttackPressed=true;
            bool skillReady=player.SkillRemaining<=0&&player.mana>=15;
            if(Button(new Rect(1147,585,108,100),skillReady ? "技能" : player.SkillRemaining>0 ? player.SkillRemaining.ToString("0.0")+" 秒" : "魔法不足")&&skillReady)
                DemoInput.SkillPressed=true;
            GUI.Label(new Rect(888,527,370,28),player.equipment.FamilyName+" · "+player.equipment.SkillName,label);
        }
        if(equipmentOpen)EquipmentPanel();
        if(player.health<=0)
        {
            Panel(new Rect(380,260,520,200));GUI.Label(new Rect(405,280,470,50),"冒险暂时结束",title);
            if(Button(new Rect(465,365,350,60),"返回检查点"))chapter.Retry();
        }
        else if(chapter&&chapter.Completed&&!equipmentOpen)
        {
            Panel(new Rect(350,250,580,250));GUI.Label(new Rect(370,270,540,55),"第一大关 · 森林净化完成",title);
            GUI.Label(new Rect(395,340,490,40),"通关奖励：100 金币    可继续升级装备",label);
            if(Button(new Rect(390,413,230,58),"查看装备"))ToggleEquipment(true);
            if(Button(new Rect(650,413,230,58),"重玩本关"))
            {
                ToggleEquipment(false);PlayerPrefs.SetInt("FA.checkpoint",0);PlayerPrefs.Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        GUI.matrix=previous;
    }
    void EquipmentPanel()
    {
        var equipment=player.equipment;
        Panel(new Rect(160,190,960,355));GUI.Label(new Rect(180,204,920,48),"装备武器 · 更换后自动使用对应动作与技能",title);
        for(int i=0;i<4;i++)
        {
            Rect card=new Rect(185+i*235,268,210,155);Panel(card);
            Sprite icon=equipment.Icon(i);Rect tex=icon.textureRect;
            Rect draw=new Rect(card.x+77,card.y+10,56,70);
            float aspect=tex.width/tex.height;
            if(aspect<draw.width/draw.height){float w=draw.height*aspect;draw.x+=(draw.width-w)*.5f;draw.width=w;}
            else{float h=draw.width/aspect;draw.y+=(draw.height-h)*.5f;draw.height=h;}
            GUI.DrawTextureWithTexCoords(draw,icon.texture,new Rect(tex.x/icon.texture.width,tex.y/icon.texture.height,tex.width/icon.texture.width,tex.height/icon.texture.height));
            GUI.Label(new Rect(card.x+10,card.y+81,190,27),WeaponEquipment.Names[i]+" · "+WeaponEquipment.Skills[i],label);
            if(Button(new Rect(card.x+20,card.y+115,170,32),(int)equipment.Family==i?"已装备":"装备"))equipment.Equip(i);
        }
        GUI.Label(new Rect(188,440,440,50),equipment.FamilyName+" · "+WeaponEquipment.Qualities[equipment.Tier]+" · 攻击 "+equipment.Damage,label);
        if(Button(new Rect(650,440,240,52),equipment.Tier>=3?"已达最高品质":"升级 · "+equipment.UpgradeCost+" 金币"))
        {
            if(!equipment.Upgrade())ShowMessage(equipment.Tier>=3?"这把武器已经达到最高品质":"金币不足，击败敌人获得金币");
        }
        if(Button(new Rect(920,440,165,52),"返回游戏"))ToggleEquipment(false);
    }
    void OnApplicationPause(bool paused){if(paused&&ChapterOne.Instance)ChapterOne.Instance.Save();}
    void OnDestroy()
    {
        Time.timeScale=1;vitals.Dispose();foreach(var texture in ownedTextures)if(texture)Destroy(texture);
        DemoInput.Horizontal=0;DemoInput.ClearOneShot();
    }
}

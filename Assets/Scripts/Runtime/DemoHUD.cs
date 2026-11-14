using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DemoHUD : MonoBehaviour
{
    public static DemoHUD Instance{get;private set;}
    void Awake(){Instance=this;}
    public PlayerController2D player;
    public EnemyAI2D boss;
    public Font interfaceFont;
    public Sprite[] interfaceArt;
    static string message = "";
    static float messageUntil;
    readonly VitalsFlaskHUD vitals = new VitalsFlaskHUD();
    readonly List<Texture2D> ownedTextures = new List<Texture2D>();
    GUIStyle label, title, button;
    Texture2D white;
    bool equipmentOpen;
    public static void ShowMessage(string text) { message = text; messageUntil = Time.unscaledTime + 4; }

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
        if(interfaceArt!=null&&interfaceArt.Length==12){HudArtwork.Panel(rect,interfaceArt[7]);return;}
        Fill(rect,new Color(.65f,.49f,.25f,.97f));
        Fill(new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4),new Color(.12f,.19f,.17f,.96f));
    }
    bool Button(Rect rect, string text)
    {
        Panel(rect);bool clicked=GUI.Button(rect,text,button);if(clicked)GameAudio.Play(GameSound.Menu);return clicked;
    }
    void DrawControl(Rect rect,int icon,string text,bool pressed)
    {
        Color previous=GUI.color;
        if(pressed)GUI.color=new Color(.75f,.85f,.7f);
        if(interfaceArt!=null&&interfaceArt.Length==12)HudArtwork.Draw(rect,interfaceArt[icon]);else Panel(rect);
        GUI.color=previous;
        GUI.Label(new Rect(rect.x,rect.yMax-3,rect.width,23),text,button);
    }
    void Direction(Rect rect,string symbol,bool pressed)
    {
        Color previous=GUI.color;int size=button.fontSize;
        button.fontSize=30;GUI.color=pressed?new Color(.55f,1,.6f):new Color(1,.95f,.8f);
        GUI.Label(rect,symbol,button);button.fontSize=size;GUI.color=previous;
    }
    bool IconButton(Rect rect,int icon,string text)
    {
        DrawControl(rect,icon,text,false);
        return GUI.Button(rect,"",GUIStyle.none);
    }
    void ToggleEquipment(bool open)
    {
        equipmentOpen = open;
        if (ChapterOne.Instance) ChapterOne.Instance.MenuOpen = open;
        Time.timeScale = open ? 0 : 1;
        DemoInput.Horizontal = 0;DemoInput.Vertical=0; DemoInput.AttackHeld=false;DemoInput.ClearOneShot();
    }
    void OnGUI()
    {
        Styles();
        Matrix4x4 previous = GUI.matrix;
        Rect safe = Screen.safeArea;
        GameplayViewport view=AdventureControls.View;
        float cx=(view.Width-1280)*.5f;
        GUI.matrix=Matrix4x4.TRS(new Vector3(safe.x,Screen.height-safe.yMax,0),Quaternion.identity,new Vector3(view.Scale,view.Scale,1));
        if (!player) { GUI.matrix = previous; return; }
        var chapter = ChapterOne.Instance;
        if(chapter&&!chapter.Playing){DrawTitle(view);GUI.matrix=previous;return;}
        vitals.Draw(player,new Rect(14,view.Height-380,200,116),interfaceFont,interfaceArt);
        GUI.Label(new Rect(18,14,245,28),"等级 "+(chapter ? chapter.Level : 1)+"  金币 "+(chapter ? chapter.Coins : 0),label);
        GUI.Label(new Rect(18,view.Height-410,310,28),"自动红药 ×"+player.redPotions+"  魔法自动恢复",label);
        if (chapter)
        {
            Panel(new Rect(405+cx,14,470,74));
            GUI.Label(new Rect(420+cx,21,440,27),chapter.RegionName,label);
            GUI.Label(new Rect(420+cx,49,440,30),chapter.Objective,label);
        }
        if (Time.unscaledTime < messageUntil)
        {
            Panel(new Rect(290+cx,106,700,60)); GUI.Label(new Rect(307+cx,116,670,46),message,label);
        }
        if (boss && !boss.IsDead && player.transform.position.x > 75)
        {
            Panel(new Rect(390+cx,190,500,52));
            float fraction = Mathf.Clamp01((float)boss.Health/boss.maxHealth);
            Fill(new Rect(402+cx,221,476*fraction,9),boss.WindingUp ? new Color(1,.65f,.2f) : new Color(.8f,.23f,.15f));
            GUI.Label(new Rect(405+cx,192,470,28),boss.WindingUp ? "首领正在蓄力，准备躲避！" : "区域首领  "+boss.Health+" / "+boss.maxHealth,label);
        }
        if (!equipmentOpen && player.health>0 && (!chapter || !chapter.Completed))
        {
            if(IconButton(new Rect(view.Width-85,18,65,65),6,"装备")){shopAuthorized=false;menuTab=0;ToggleEquipment(true);}
            if(Button(new Rect(view.Width-180,25,82,45),GameAudio.Instance&&GameAudio.Instance.Enabled?"声音 开":"声音 关")&&GameAudio.Instance)GameAudio.Instance.Toggle();
            if(interfaceArt!=null&&interfaceArt.Length==12)HudArtwork.Draw(AdventureControls.PadRect,interfaceArt[0]);
            Direction(AdventureControls.LeftRect,"◀",DemoInput.Horizontal<0);
            Direction(AdventureControls.RightRect,"▶",DemoInput.Horizontal>0);
            Direction(AdventureControls.UpRect,"▲",DemoInput.Vertical>0);
            Direction(AdventureControls.DownRect,"▼",DemoInput.Vertical<0);
            DrawControl(AdventureControls.JumpRect,3,"跳跃",false);
            DrawControl(AdventureControls.AttackRect,4,"攻击",DemoInput.AttackHeld);
            bool skillReady=player.SkillRemaining<=0&&player.mana>=15;
            DrawControl(AdventureControls.SkillRect,5,skillReady ? "技能" : player.SkillRemaining>0 ? player.SkillRemaining.ToString("0.0")+" 秒" : "魔法不足",false);
            GUI.Label(new Rect(view.Width-392,view.Height-193,370,28),player.equipment.FamilyName+" · "+player.equipment.SkillName,label);
            var interaction=player.GetComponent<InteractionController>();
            if(interaction&&!string.IsNullOrEmpty(interaction.Hint))GUI.Label(new Rect(view.Width*.5f-230,view.Height-100,460,35),interaction.Hint,button);
        }
        if(equipmentOpen)
        {
            CampaignPanel();
            if(Time.unscaledTime<messageUntil){Panel(new Rect(view.Width*.5f-350,18,700,70));GUI.Label(new Rect(view.Width*.5f-332,30,664,48),message,label);}
        }
        if(player.health<=0)
        {
            Panel(new Rect(380+cx,260,520,200));GUI.Label(new Rect(405+cx,280,470,50),"冒险暂时结束",title);
            if(Button(new Rect(465+cx,365,350,60),"返回检查点"))chapter.Retry();
        }
        else if(chapter&&chapter.Completed&&!equipmentOpen)
        {
            Panel(new Rect(350+cx,250,580,250));GUI.Label(new Rect(370+cx,270,540,55),"时空旅行 · 主线通关",title);
            GUI.Label(new Rect(395+cx,340,490,40),"奶奶平安归来 · 地下城和未完成委托已开放",label);
            if(Button(new Rect(390+cx,413,230,58),"探索世界")){menuTab=4;ToggleEquipment(true);}
            if(Button(new Rect(650+cx,413,230,58),"返回主菜单"))chapter.ReturnTitle();
        }
        GUI.matrix=previous;
    }
    void DrawWeaponPanel()
    {
        float cx=(AdventureControls.View.Width-1280)*.5f;
        var equipment=player.equipment;
        Panel(new Rect(160+cx,190,960,355));GUI.Label(new Rect(180+cx,204,920,48),"装备武器 · 更换后自动使用对应动作与技能",title);
        for(int i=0;i<4;i++)
        {
            Rect card=new Rect(185+cx+i*235,268,210,155);Panel(card);
            Sprite icon=equipment.Icon(i);Rect tex=icon.textureRect;
            Rect draw=new Rect(card.x+77,card.y+10,56,70);
            float aspect=tex.width/tex.height;
            if(aspect<draw.width/draw.height){float w=draw.height*aspect;draw.x+=(draw.width-w)*.5f;draw.width=w;}
            else{float h=draw.width/aspect;draw.y+=(draw.height-h)*.5f;draw.height=h;}
            GUI.DrawTextureWithTexCoords(draw,icon.texture,new Rect(tex.x/icon.texture.width,tex.y/icon.texture.height,tex.width/icon.texture.width,tex.height/icon.texture.height));
            GUI.Label(new Rect(card.x+10,card.y+81,190,27),WeaponEquipment.Names[i]+" · "+WeaponEquipment.Skills[i],label);
            if(Button(new Rect(card.x+20,card.y+115,170,32),!CampaignInventory.OwnsWeapon(i)?"尚未获得":(int)equipment.Family==i?"已装备":"装备"))equipment.Equip(i);
        }
        GUI.Label(new Rect(188+cx,440,440,50),equipment.FamilyName+" · "+WeaponEquipment.Qualities[equipment.Tier]+" · 攻击 "+equipment.Damage,label);
        if(Button(new Rect(650+cx,440,240,52),equipment.Tier>=3?"已达最高品质":"升级 · "+equipment.UpgradeCost+" 金币"))
        {
            if(!equipment.Upgrade())ShowMessage(equipment.Tier>=3?"这把武器已经达到最高品质":"金币不足，击败敌人获得金币");
        }
        if(Button(new Rect(920+cx,440,165,52),"返回游戏"))ToggleEquipment(false);
    }
    void OnApplicationPause(bool paused){if(paused&&ChapterOne.Instance)ChapterOne.Instance.Save();}
    void OnDestroy()
    {
        if(Instance==this)Instance=null;
        Time.timeScale=1;vitals.Dispose();foreach(var texture in ownedTextures)if(texture)Destroy(texture);
        DemoInput.Horizontal=0;DemoInput.Vertical=0;DemoInput.AttackHeld=false;DemoInput.ClearOneShot();
    }
}

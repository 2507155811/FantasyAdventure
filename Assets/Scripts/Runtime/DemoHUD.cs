using UnityEngine;
public class DemoHUD:MonoBehaviour{
 static int coins;static string message="森林入口：击败怪物，找到森林守卫！";static float messageUntil;PlayerController2D player;GUIStyle title,label,btn;
 public static void AddCoins(int n){coins+=n;}public static void ShowMessage(string s){message=s;messageUntil=Time.time+4;}
 void OnGUI(){
 if(Time.time>messageUntil&&messageUntil>0)message="";
 if(!player)player=FindObjectOfType<PlayerController2D>();
 title=new GUIStyle(GUI.skin.label){fontSize=21,fontStyle=FontStyle.Bold,normal={textColor=Color.white}};
 label=new GUIStyle(GUI.skin.label){fontSize=16,normal={textColor=Color.white}};
 btn=new GUIStyle(GUI.skin.button){fontSize=17,fontStyle=FontStyle.Bold};
 GUI.Box(new Rect(12,12,310,108),"");
 if(player){GUI.Label(new Rect(24,18,290,28),"♥ 生命 "+player.health+"/"+player.maxHealth,title);GUI.Label(new Rect(24,48,290,24),"✦ 魔法 "+player.mana+"/"+player.maxMana,label);GUI.Label(new Rect(24,76,290,24),"金币 "+coins+"   红药 "+player.redPotions+" 蓝药 "+player.bluePotions,label);}
 if(Time.time<messageUntil||message!="")GUI.Box(new Rect(Screen.width*.32f,12,Screen.width*.48f,42),message);
 float y=Screen.height-90,x=18;
 if(GUI.RepeatButton(new Rect(x,y,62,62),"◀"))DemoInput.Horizontal=-1;else if(GUI.RepeatButton(new Rect(x+68,y,62,62),"▶"))DemoInput.Horizontal=1;else DemoInput.Horizontal=0;
 float r=Screen.width-300;TouchButton(new Rect(r,y+10,58,58),"跳",0);TouchButton(new Rect(r+62,y+10,58,58),"闪",1);TouchButton(new Rect(r+124,y+10,70,58),"攻击",2);TouchButton(new Rect(r+198,y+10,70,58),"技能",3);
 TouchButton(new Rect(Screen.width-150,18,64,42),"红药",4);TouchButton(new Rect(Screen.width-80,18,64,42),"蓝药",5);
 }
 void TouchButton(Rect r,string s,int i){if(!GUI.Button(r,s,btn))return;switch(i){case 0:DemoInput.JumpPressed=true;break;case 1:DemoInput.DodgePressed=true;break;case 2:DemoInput.AttackPressed=true;break;case 3:DemoInput.SkillPressed=true;break;case 4:DemoInput.RedPotionPressed=true;break;case 5:DemoInput.BluePotionPressed=true;break;}}
}
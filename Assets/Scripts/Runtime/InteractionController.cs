using UnityEngine;
[DefaultExecutionOrder(-20)]
public class InteractionController : MonoBehaviour
{
    public PlayerController2D player;
    public DemoHUD hud;
    public string Hint{get;private set;}
    void Update()
    {
        Hint="";
        if(!ChapterOne.Instance||!ChapterOne.Instance.Playing||ChapterOne.Instance.MenuOpen||player.health<=0)return;
        WorldInteraction closest=null;float distance=1.8f;
        foreach(var item in WorldInteraction.Active)
        {
            if(!item)continue;
            float d=Vector2.Distance(player.transform.position,item.transform.position+Vector3.up*.5f);
            if(d<distance){closest=item;distance=d;}
        }
        if(!closest)return;
        Hint="▲ "+(closest.kind==WorldInteraction.Kind.Npc?"与 "+closest.label+" 交谈":"进入下一地区");
        if(DemoInput.UpPressed||Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))
        {
            DemoInput.UpPressed=false;
            if(closest.kind==WorldInteraction.Kind.Exit)ChapterOne.Instance.ExitMap();
            else{ChapterOne.Instance.Talk();hud.ShowResident(closest.label);}
        }
    }
}

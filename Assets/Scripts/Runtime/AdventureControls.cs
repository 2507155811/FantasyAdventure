using UnityEngine;

// Each finger owns its control; IMGUI draws combat buttons but never captures their input.
[DefaultExecutionOrder(-100)]
public class AdventureControls : MonoBehaviour
{
    readonly CombatPointerState state = new CombatPointerState();
    public PlayerController2D player;
    public static GameplayViewport View {get{return GameplayViewport.Fit(Screen.safeArea.width,Screen.safeArea.height);}}
    public static Rect PadRect{get{return new Rect(30,View.Height-220,200,200);}}
    public static Rect LeftRect{get{return new Rect(30,View.Height-155,70,70);}}
    public static Rect UpRect{get{return new Rect(95,View.Height-220,70,70);}}
    public static Rect DownRect{get{return new Rect(95,View.Height-90,70,70);}}
    public static Rect RightRect{get{return new Rect(160,View.Height-155,70,70);}}
    public static Rect JumpRect {get{return new Rect(View.Width-386,View.Height-135,100,100);}}
    public static Rect AttackRect {get{return new Rect(View.Width-272,View.Height-158,125,125);}}
    public static Rect SkillRect {get{return new Rect(View.Width-133,View.Height-135,108,100);}}

    public static Vector2 ToLayout(Vector2 screenPoint)
    {
        Rect safe=Screen.safeArea;GameplayViewport view=View;
        return new Vector2(screenPoint.x-safe.x,Screen.height-screenPoint.y-(Screen.height-safe.yMax))/view.Scale;
    }
    static CombatControl Hit(Vector2 point)
    {
        if(PadRect.Contains(point))return CombatControl.DirectionPad;
        if(JumpRect.Contains(point))return CombatControl.Jump;
        if(AttackRect.Contains(point))return CombatControl.Attack;
        if(SkillRect.Contains(point))return CombatControl.Skill;
        return CombatControl.None;
    }
    void Update()
    {
        if(!player||player.health<=0||ChapterOne.Instance&&(ChapterOne.Instance.MenuOpen||ChapterOne.Instance.Completed))
        {ResetControls();return;}
        if(Input.touchCount>0)
        {
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch=Input.GetTouch(i);
                if(touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled){state.End(touch.fingerId);continue;}
                Vector2 point=ToLayout(touch.position);
                if(touch.phase==TouchPhase.Began)state.Begin(touch.fingerId,Hit(point));
                Move(touch.fingerId,point);
            }
        }
        else if(!Application.isMobilePlatform)
        {
            Vector2 point=ToLayout(Input.mousePosition);
            if(Input.GetMouseButtonDown(0))state.Begin(-1,Hit(point));
            if(Input.GetMouseButton(0))Move(-1,point);else state.End(-1);
        }
        else state.Clear();
        DemoInput.Horizontal=state.Horizontal;DemoInput.Vertical=state.Vertical;
        DemoInput.AttackHeld=state.Held(CombatControl.Attack);
        DemoInput.UpPressed|=state.ConsumeUp();
        DemoInput.JumpPressed|=state.ConsumeJump();
        DemoInput.SkillPressed|=state.ConsumeSkill();
    }
    void Move(int id,Vector2 point)
    {
        Vector2 delta=point-PadRect.center;int horizontal,vertical;
        CombatPointerState.Direction(delta.x/100,-delta.y/100,out horizontal,out vertical);
        state.MoveDirection(id,horizontal,vertical);
    }
    void ResetControls()
    {
        state.Clear();DemoInput.Horizontal=0;DemoInput.Vertical=0;DemoInput.AttackHeld=false;DemoInput.ClearOneShot();
    }
    void OnDisable(){ResetControls();}
    void OnApplicationFocus(bool focused){if(!focused)ResetControls();}
    void OnApplicationPause(bool paused){if(paused)ResetControls();}
}

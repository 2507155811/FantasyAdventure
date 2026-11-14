using System;

public static class GameplayInputContracts
{
    static int count;
    static void Check(bool condition,string scenario)
    {
        if(!condition)throw new Exception("FAIL: "+scenario);
        count++;Console.WriteLine("PASS: "+scenario);
    }
    public static void Main()
    {
        var state=new CombatPointerState();
        state.Begin(1,CombatControl.Left);state.Begin(2,CombatControl.Attack);
        Check(state.Horizontal==-1&&state.Held(CombatControl.Attack),"two fingers move and attack together");
        state.Begin(3,CombatControl.Jump);
        Check(state.ConsumeJump()&&state.Horizontal==-1&&state.Held(CombatControl.Attack),"jump preserves other fingers");
        Check(!state.ConsumeJump(),"jump fires once per press");
        state.End(2);
        Check(state.Horizontal==-1&&!state.Held(CombatControl.Attack),"releasing attack preserves movement");
        state.MoveDirection(1,1);
        Check(state.Horizontal==1,"direction finger slides without re-pressing");
        state.Begin(4,CombatControl.Skill);
        Check(state.ConsumeSkill()&&!state.ConsumeSkill(),"skill fires once per press");
        state.Begin(5,CombatControl.Left);
        Check(state.Horizontal==0,"opposing directional fingers cancel");
        state.End(1);Check(state.Horizontal==-1,"canceling one finger preserves the other");
        state.Clear();
        Check(state.Horizontal==0&&!state.Held(CombatControl.Attack)&&!state.ConsumeJump(),"focus/menu reset clears input ownership");
        state.Begin(10,CombatControl.Up);state.Begin(11,CombatControl.Attack);
        Check(state.Vertical==1&&state.ConsumeUp()&&!state.ConsumeUp()&&state.Held(CombatControl.Attack),"up interaction is one-shot while climbing and attacking");
        state.MoveDirection(10,0,-1);Check(state.Vertical==-1,"slide from up to down without releasing");
        state.MoveDirection(10,1,1);Check(state.ConsumeUp()&&state.Horizontal==1&&state.Vertical==1,"diagonal climbing preserves horizontal input");
        state.Clear();Check(!state.ConsumeUp()&&state.Vertical==0,"menu reset clears vertical input and interaction");
        int horizontal,vertical;CombatPointerState.Direction(.8f,-.8f,out horizontal,out vertical);
        Check(horizontal==1&&vertical==-1,"direction pad diagonal maps both axes");
        CombatPointerState.Direction(.1f,.1f,out horizontal,out vertical);Check(horizontal==0&&vertical==0,"direction pad has neutral deadzone");
        var clock=new CombatActionClock();clock.Start(10,.6,.5);
        Check(!clock.TryImpact(10.1)&&clock.IsActive(10.1),"windup causes no early damage");
        Check(clock.TryImpact(10.31)&&!clock.TryImpact(10.32),"impact is delivered exactly once");
        Check(!clock.IsActive(10.61)&&clock.Progress(10.61)==1,"recovery completes at the declared duration");
        clock.Start(20,.32,.42);
        Check(clock.TryImpact(20.8),"a slow render frame does not drop the impact");
        clock.Start(30,.6,.5);clock.Cancel();
        Check(!clock.TryImpact(31)&&!clock.IsActive(31),"hurt/equipment cancellation prevents ghost damage");
        double maximumJump=14.5*14.5/(2*9.81*3.2);
        Check(maximumJump>2.2+.275+.5,"highest ledge lies below maximum jump height");
        float[,] screens={{1280,720},{1920,1080},{2340,1080},{2560,1080},{2048,1536},{2560,1600},{960,640},{1136,640},{800,480},{2184,1032}};
        for(int i=0;i<screens.GetLength(0);i++)
        {
            float width=screens[i,0],height=screens[i,1];var view=GameplayViewport.Fit(width,height);
            Check(Math.Abs(view.Width*view.Scale-width)<.01&&Math.Abs(view.Height*view.Scale-height)<.01,
                "safe viewport fills "+width+"x"+height+" without aspect distortion");
            Check(view.Width>=999.9f&&view.Height>=719.9f&&view.Width-386>257&&view.Height-158>0,
                "combat controls remain inside safe area without overlapping at "+width+"x"+height);
        }
        Console.WriteLine("Completed "+count+" gameplay input/timing contracts.");
    }
}

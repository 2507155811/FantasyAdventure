using System;

// Absolute timestamps keep hit frames and animation in sync at different render rates.
public sealed class CombatActionClock
{
    double start, duration, impact;
    bool pending;
    public void Start(double now, double seconds, double impactFraction)
    {
        if(seconds<=0||impactFraction<0||impactFraction>1)throw new ArgumentOutOfRangeException();
        start=now;duration=seconds;impact=now+seconds*impactFraction;pending=true;
    }
    public bool IsActive(double now){return duration>0&&now<start+duration;}
    public float Progress(double now){return duration<=0?1:(float)Math.Max(0,Math.Min(1,(now-start)/duration));}
    public bool TryImpact(double now)
    {
        if(!pending||now<impact)return false;
        pending=false;return true;
    }
    public void Cancel(){duration=0;pending=false;}
}

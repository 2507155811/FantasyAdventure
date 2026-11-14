using System.Collections.Generic;

public enum CombatControl { None, Left, Right, Up, Down, DirectionPad, Jump, Attack, Skill }

// Pure pointer ownership state, shared by touchscreen and mouse adapters.
public sealed class CombatPointerState
{
    sealed class Pointer
    {
        public CombatControl control;
        public int direction,vertical;
    }
    readonly Dictionary<int, Pointer> pointers = new Dictionary<int, Pointer>();
    bool jumpQueued, skillQueued,upQueued;
    public void Begin(int id, CombatControl control)
    {
        pointers[id] = new Pointer { control = control, direction = control == CombatControl.Left ? -1 : control == CombatControl.Right ? 1 : 0, vertical=control==CombatControl.Up?1:control==CombatControl.Down?-1:0 };
        if(control==CombatControl.Up)upQueued=true;
        if (control == CombatControl.Jump) jumpQueued = true;
        if (control == CombatControl.Skill) skillQueued = true;
    }
    static bool Directional(CombatControl control){return control==CombatControl.Left||control==CombatControl.Right||control==CombatControl.Up||control==CombatControl.Down||control==CombatControl.DirectionPad;}
    public void MoveDirection(int id,int direction){MoveDirection(id,direction,0);}
    public void MoveDirection(int id,int direction,int vertical)
    {
        Pointer pointer;
        if(pointers.TryGetValue(id,out pointer)&&Directional(pointer.control))
        {
            if(pointer.vertical<=0&&vertical>0)upQueued=true;
            pointer.direction=direction<0?-1:direction>0?1:0;
            pointer.vertical=vertical<0?-1:vertical>0?1:0;
        }
    }
    public static void Direction(float x,float y,out int horizontal,out int vertical)
    {
        horizontal=vertical=0;
        float ax=System.Math.Abs(x),ay=System.Math.Abs(y);
        if(System.Math.Max(ax,ay)>1.35f||System.Math.Max(ax,ay)<.22f)return;
        if(ax>.3f&&ax>=ay*.65f)horizontal=x<0?-1:1;
        if(ay>.3f&&ay>=ax*.65f)vertical=y<0?-1:1;
    }
    public void End(int id) { pointers.Remove(id); }
    public int Horizontal
    {
        get { int result = 0; foreach (var pointer in pointers.Values) result += pointer.direction; return result < 0 ? -1 : result > 0 ? 1 : 0; }
    }
    public int Vertical
    {
        get{int result=0;foreach(var pointer in pointers.Values)result+=pointer.vertical;return result<0?-1:result>0?1:0;}
    }
    public bool Held(CombatControl control)
    {
        foreach (var pointer in pointers.Values) if (pointer.control == control) return true;
        return false;
    }
    public bool ConsumeUp(){bool result=upQueued;upQueued=false;return result;}
    public bool ConsumeJump() { bool result = jumpQueued; jumpQueued = false; return result; }
    public bool ConsumeSkill() { bool result = skillQueued; skillQueued = false; return result; }
    public void Clear() { pointers.Clear(); jumpQueued = skillQueued = upQueued = false; }
}

using System;

public struct GameplayViewport
{
    public float Width,Height,Scale;
    public static GameplayViewport Fit(float availableWidth,float availableHeight)
    {
        float width=Math.Max(1,availableWidth),height=Math.Max(1,availableHeight);
        float scale=Math.Min(width/1000f,height/720f);
        return new GameplayViewport{Width=width/scale,Height=height/scale,Scale=scale};
    }
}

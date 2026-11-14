using UnityEngine;

// Generated glass frames share one UI atlas; only the liquid is drawn dynamically.
public sealed class VitalsFlaskHUD
{
    const int Steps=32;
    readonly Texture2D[,] liquids=new Texture2D[2,Steps+1];
    GUIStyle caption,value;
    Sprite[] artwork;
    public void Draw(PlayerController2D player,Rect area,Font font=null,Sprite[] sprites=null)
    {
        artwork=sprites;
        if(caption==null)
        {
            caption=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=13,fontStyle=FontStyle.Bold};
            value=new GUIStyle(caption){fontSize=16};
            if(font){caption.font=font;value.font=font;}
        }
        Flask(new Rect(area.x,area.y,area.width*.5f,area.height),0,player.health,player.maxHealth,"生命 HP");
        Flask(new Rect(area.x+area.width*.5f,area.y,area.width*.5f,area.height),1,player.mana,player.maxMana,"魔法 MP");
    }
    void Flask(Rect target,int kind,int current,int maximum,string name)
    {
        Rect rect=target;
        Sprite frame=artwork!=null&&artwork.Length==12?artwork[8+kind]:null;
        if(frame)
        {
            float scale=Mathf.Min(target.width/frame.rect.width,target.height/frame.rect.height);
            float width=frame.rect.width*scale,height=frame.rect.height*scale;
            rect=new Rect(target.center.x-width*.5f,target.center.y-height*.5f,width,height);
        }
        int level=Mathf.RoundToInt((maximum>0?Mathf.Clamp01((float)current/maximum):0)*Steps);
        if(!liquids[kind,level])liquids[kind,level]=Liquid(kind,level);
        float diameter=rect.width*.76f;
        var bowl=new Rect(rect.center.x-diameter*.5f,rect.y+rect.height*.66f-diameter*.5f,diameter,diameter);
        GUI.DrawTexture(bowl,liquids[kind,level],ScaleMode.ScaleToFit,true);
        if(frame)HudArtwork.Draw(rect,frame);
        Label(new Rect(rect.x,rect.y+rect.height*.54f,rect.width,24),Mathf.Max(0,current).ToString(),value);
        Label(new Rect(rect.x,rect.y+rect.height*.72f,rect.width,21),"/ "+maximum,caption);
        Label(new Rect(target.x,target.yMax+3,target.width,22),name,caption);
    }
    static void Label(Rect rect,string text,GUIStyle style)
    {
        Color previous=GUI.color;GUI.color=new Color(.1f,.05f,.03f);
        GUI.Label(new Rect(rect.x+1,rect.y+2,rect.width,rect.height),text,style);
        GUI.color=new Color(1,.96f,.82f);GUI.Label(rect,text,style);GUI.color=previous;
    }
    static Texture2D Liquid(int kind,int level)
    {
        const int size=64;var pixels=new Color[size*size];
        Color liquid=kind==0?new Color(.94f,.12f,.035f):new Color(.03f,.55f,.95f);
        float surface=(size-1)*level/(float)Steps;
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float dx=x-31.5f,dy=y-31.5f;
            if(dx*dx+dy*dy>31*31)continue;
            Color color=new Color(.09f,.13f,.14f,.95f);
            if(level>0&&y<=surface)
            {
                color=Color.Lerp(liquid*.5f,liquid,(x+y)/100f);color.a=1;
                if(Mathf.Abs(y-surface)<1.5f)color=Color.Lerp(liquid,Color.white,.35f);
            }
            pixels[y*size+x]=color;
        }
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
        texture.name="Liquid_"+kind+"_"+level;texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
        texture.SetPixels(pixels);texture.Apply(false,true);return texture;
    }
    public void Dispose(){foreach(var texture in liquids)if(texture)Object.Destroy(texture);}
}

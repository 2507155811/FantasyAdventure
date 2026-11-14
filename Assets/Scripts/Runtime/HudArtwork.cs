using UnityEngine;

public static class HudArtwork
{
    public static void Draw(Rect target,Sprite sprite)
    {
        if(!sprite)return;
        Rect source=sprite.textureRect;
        float scale=Mathf.Min(target.width/source.width,target.height/source.height);
        float width=source.width*scale,height=source.height*scale;
        target=new Rect(target.center.x-width*.5f,target.center.y-height*.5f,width,height);
        Blit(target,sprite.texture,source);
    }
    public static void Panel(Rect target,Sprite sprite)
    {
        if(!sprite)return;
        Rect source=sprite.textureRect;
        float sourceBorder=Mathf.Min(source.width,source.height)*.17f;
        float border=Mathf.Min(18,Mathf.Min(target.width,target.height)*.2f);
        float[] sx={source.x,source.x+sourceBorder,source.xMax-sourceBorder,source.xMax};
        float[] sy={source.y,source.y+sourceBorder,source.yMax-sourceBorder,source.yMax};
        float[] dx={target.x,target.x+border,target.xMax-border,target.xMax};
        float[] dy={target.y,target.y+border,target.yMax-border,target.yMax};
        for(int y=0;y<3;y++)for(int x=0;x<3;x++)
            Blit(new Rect(dx[x],dy[y],dx[x+1]-dx[x],dy[y+1]-dy[y]),sprite.texture,
                new Rect(sx[x],sy[2-y],sx[x+1]-sx[x],sy[3-y]-sy[2-y]));
    }
    static void Blit(Rect target,Texture2D texture,Rect source)
    {
        GUI.DrawTextureWithTexCoords(target,texture,new Rect(source.x/texture.width,source.y/texture.height,source.width/texture.width,source.height/texture.height));
    }
}

using UnityEngine;

public class FootShadow2D : MonoBehaviour
{
    SpriteRenderer shadow;
    Sprite sprite;
    Texture2D texture;
    void Awake()
    {
        const int width=64,height=16;
        var pixels=new Color[width*height];
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            float dx=(x-31.5f)/31.5f,dy=(y-7.5f)/7.5f;
            pixels[y*width+x]=new Color(.04f,.08f,.04f,Mathf.Clamp01(1-dx*dx-dy*dy)*.3f);
        }
        texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.SetPixels(pixels);texture.Apply(false,true);
        sprite=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),80,0,SpriteMeshType.FullRect);
        var go=new GameObject("Ground shadow");go.transform.SetParent(transform,false);
        shadow=go.AddComponent<SpriteRenderer>();shadow.sprite=sprite;shadow.sortingOrder=1;
    }
    void LateUpdate()
    {
        shadow.enabled=false;
        foreach(var hit in Physics2D.RaycastAll(transform.position,Vector2.down,8))
        {
            if(!hit.collider.GetComponent<GroundPlatform2D>())continue;
            shadow.enabled=true;shadow.transform.position=hit.point+Vector2.up*.015f;
            float scale=1/(1+Mathf.Max(0,hit.distance-.65f)*.3f);
            shadow.transform.localScale=new Vector3(scale,scale,1);return;
        }
    }
    void OnDestroy(){if(sprite)Destroy(sprite);if(texture)Destroy(texture);}
}

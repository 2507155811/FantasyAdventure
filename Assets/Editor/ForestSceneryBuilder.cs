using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ForestSceneryBuilder
{
    const string DirectoryPath="Assets/Art/Forest";
    static Texture2D atlas;
    static Sprite ladderArt;
    public static Sprite Build(Transform cameraTransform)
    {
        string path=DirectoryPath+"/ForestAtlas.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new FileNotFoundException("Forest atlas missing",path);
        importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
        importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=4096;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
        importer.SaveAndReimport();atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Sprite sky=Slice("DistantForest",new Rect(0,1,1254,310),26,new Vector2(.5f,0));
        Sprite forest=Slice("OldForest",new Rect(0,314,1254,310),42,new Vector2(.5f,0));
        Sprite ground=Slice("GrassEarth",new Rect(0,629,1254,309),110,new Vector2(.5f,1));
        Sprite soil=Slice("ForestSoil",new Rect(0,739,1254,199),110,new Vector2(.5f,1));
        var earth=new GameObject("Forest underground");earth.transform.position=new Vector3(64,-1.5f,0);
        Tile(soil,earth.transform,new Rect(-68,-5,136,5),-2);
        Layer(cameraTransform,sky,"Distant hills",.85f,-30,-1);
        Layer(cameraTransform,forest,"Forest depth",.45f,-20,-.6f);
        Sprite[] props={
            Slice("LeafyTree",new Rect(0,956,240,270),110,new Vector2(.5f,0)),
            Slice("Fern",new Rect(204,1097,149,129),160,new Vector2(.5f,0)),
            Slice("MossRock",new Rect(357,1083,153,141),160,new Vector2(.5f,0)),
            Slice("Mushrooms",new Rect(513,1091,105,134),170,new Vector2(.5f,0)),
            Slice("Wildflowers",new Rect(623,1064,150,164),185,new Vector2(.5f,0)),
            Slice("Waystone",new Rect(777,1045,123,180),140,new Vector2(.5f,0))};
        ladderArt=Slice("Ladder",new Rect(910,981,88,246),100,new Vector2(.5f,0));
        var details=new GameObject("Forest details").transform;
        float[] trees={-2,16,34,55,72,92,131};
        foreach(float x in trees)Place(details,props[0],new Vector2(x,-.49f),1.5f,-5);
        float[] stones={3,36,41,81,111,126};
        foreach(float x in stones)Place(details,props[5],new Vector2(x,-.49f),1,-4);
        for(int i=0;i<43;i++)Place(details,props[1+i%4],new Vector2(-2+i*3.1f,-.49f),.48f+i%3*.08f,-3);
        return ground;
    }
    static void Layer(Transform cameraTransform,Sprite sprite,string name,float travel,int order,float bottom)
    {
        var group=new GameObject(name);var parallax=group.AddComponent<ForestParallax2D>();
        parallax.cameraTransform=cameraTransform;parallax.cameraTravel=travel;
        float width=sprite.bounds.size.x;
        for(int i=-3;i<=6;i++)
        {
            var art=Place(group.transform,sprite,new Vector2(21+i*width,bottom),1,order);
            art.flipX=(i&1)!=0;
            art.color=order==-20?new Color(.86f,.94f,.9f):new Color(.96f,.98f,.96f);
        }
    }
    public static void Ladder(float x,float bottom,float top,Collider2D platform)
    {
        var go=new GameObject("Climbable ladder");go.transform.position=new Vector3(x,(bottom+top)*.5f,0);
        var ladder=go.AddComponent<Ladder2D>();ladder.bottomY=bottom;ladder.topY=top;ladder.landingPlatform=platform;
        var trigger=go.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(1.15f,top-bottom+2);
        Place(go.transform,ladderArt,new Vector2(0,-(top-bottom)*.5f),(top-bottom)/ladderArt.bounds.size.y,-1);
    }
    // Neighboring tiles meet at the SAME source edge. Alternate mirroring avoids hard color seams.
    // Cuts retain pixels-per-unit and full-rect geometry; no stretching or spacing offsets.
    public static void Tile(Sprite source,Transform parent,Rect area,int order)
    {
        float unitWidth=source.rect.width/source.pixelsPerUnit,unitHeight=source.rect.height/source.pixelsPerUnit;
        float drawnY=0;int row=0;
        while(drawnY<area.height-.0001f)
        {
            float height=Mathf.Min(unitHeight,area.height-drawnY),drawnX=0;int column=0;
            while(drawnX<area.width-.0001f)
            {
                float width=Mathf.Min(unitWidth,area.width-drawnX);bool flipX=(column&1)!=0,flipY=(row&1)!=0;
                float pixelWidth=Mathf.Min(source.rect.width,width*source.pixelsPerUnit),pixelHeight=Mathf.Min(source.rect.height,height*source.pixelsPerUnit);
                Rect rect=new Rect(flipX?source.rect.xMax-pixelWidth:source.rect.x,
                    flipY?source.rect.y:source.rect.yMax-pixelHeight,pixelWidth,pixelHeight);
                string name=source.name+"_"+pixelWidth.ToString("F3",CultureInfo.InvariantCulture)+"_"+pixelHeight.ToString("F3",CultureInfo.InvariantCulture)+"_"+flipX+flipY;
                Sprite cut=Save(name,Sprite.Create(source.texture,rect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect));
                var art=Place(parent,cut,new Vector2(area.x+drawnX+width*.5f,area.yMax-drawnY-height*.5f),1,order);
                art.flipX=flipX;art.flipY=flipY;
                drawnX+=width;column++;
            }
            drawnY+=height;row++;
        }
    }
    static SpriteRenderer Place(Transform parent,Sprite sprite,Vector2 position,float scale,int order)
    {
        var go=new GameObject(sprite.name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        go.transform.localScale=new Vector3(scale,scale,1);
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;return renderer;
    }
    static Sprite Slice(string name,Rect topRect,float pixelsPerUnit,Vector2 pivot)
    {
        return Save(name,Sprite.Create(atlas,new Rect(topRect.x,atlas.height-topRect.y-topRect.height,topRect.width,topRect.height),pivot,pixelsPerUnit,0,SpriteMeshType.FullRect));
    }
    static Sprite Save(string name,Sprite sprite)
    {
        string path=DirectoryPath+"/"+name+".asset";sprite.name=name;
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(existing){EditorUtility.CopySerialized(sprite,existing);Object.DestroyImmediate(sprite);EditorUtility.SetDirty(existing);return existing;}
        AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
}

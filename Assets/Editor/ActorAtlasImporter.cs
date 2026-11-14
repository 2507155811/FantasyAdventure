using System.Globalization;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ActorAtlasImporter
{
    static readonly Dictionary<string,Sprite[]> cache=new Dictionary<string,Sprite[]>();
    public static void BeginBuild(){cache.Clear();}
    // Explicit frame rectangles replace the assumption that generated artwork follows an exact grid.
    public static Sprite[] Import(Texture2D texture,string layoutPath,string directory,float pixelsPerUnit,float footBelowOrigin,int first=0,int count=int.MaxValue)
    {
        Sprite[] cached;if(cache.TryGetValue(directory,out cached))return cached;
        if(!AssetDatabase.IsValidFolder(directory)){Directory.CreateDirectory(directory);AssetDatabase.Refresh();}
        string[] lines=File.ReadAllLines(layoutPath);
        var frames=new Sprite[lines.Length-1];
        for(int i=1;i<lines.Length;i++)
        {
            if(i-1<first||(long)i-1>=(long)first+count)continue;
            string[] values=lines[i].Split(',');
            float x=Read(values[0]),top=Read(values[1]),width=Read(values[2]),height=Read(values[3]);
            float anchor=Read(values[4]),foot=Read(values[5]);
            float pivotPixels=height-foot+pixelsPerUnit*footBelowOrigin;
            // Very flat death poses need transparent space above their root, not an out-of-range pivot.
            if(pivotPixels>height)
            {
                float padding=Mathf.Min(top,pivotPixels-height+1);top-=padding;height+=padding;
            }
            var rect=new Rect(x,texture.height-top-height,width,height);
            var pivot=new Vector2(anchor/width,pivotPixels/height);
            Sprite sprite=Sprite.Create(texture,rect,pivot,pixelsPerUnit,0,SpriteMeshType.FullRect);
            sprite.name=texture.name+"_"+(i-1);
            string path=directory+"/Frame"+(i-1).ToString("00")+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(existing)
            {
                EditorUtility.CopySerialized(sprite,existing);Object.DestroyImmediate(sprite);EditorUtility.SetDirty(existing);frames[i-1]=existing;
            }
            else{AssetDatabase.CreateAsset(sprite,path);frames[i-1]=sprite;}
        }
        cache[directory]=frames;return frames;
    }
    public static Sprite Asset(Texture2D texture,Rect rect,Vector2 pivot,float pixelsPerUnit,string path)
    {
        var sprite=Sprite.Create(texture,rect,pivot,pixelsPerUnit,0,SpriteMeshType.FullRect);
        sprite.name=Path.GetFileNameWithoutExtension(path);
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(existing){EditorUtility.CopySerialized(sprite,existing);Object.DestroyImmediate(sprite);EditorUtility.SetDirty(existing);return existing;}
        AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static float Read(string value){return float.Parse(value,CultureInfo.InvariantCulture);}
}

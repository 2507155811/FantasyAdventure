using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Lossless production packing: keep approved art unchanged and add climb frames to one gameplay texture.
public static class HeroAtlasBaker
{
    sealed class Frame
    {
        public Texture2D texture;
        public int x,top,width,height;
        public float anchor,foot;
    }
    public static void Bake()
    {
        var frames=new List<Frame>();var sources=new List<Texture2D>();
        Load("Docs/ArtSources/Hero/HeroEquippedBase",frames,sources);
        Load("Docs/ArtSources/Hero/HeroClimbSource",frames,sources);
        if(frames.Count!=54)throw new System.InvalidOperationException("Expected 48 base and 6 climb frames.");
        int cellWidth=0,cellHeight=0;
        foreach(var frame in frames){cellWidth=Mathf.Max(cellWidth,frame.width+12);cellHeight=Mathf.Max(cellHeight,frame.height+12);}
        var atlas=new Texture2D(cellWidth*6,cellHeight*9,TextureFormat.RGBA32,false);
        atlas.SetPixels(new Color[atlas.width*atlas.height]);
        var csv=new StringBuilder("x,top,width,height,anchor_x,foot_from_top\n");
        for(int i=0;i<frames.Count;i++)
        {
            Frame frame=frames[i];
            int x=i%6*cellWidth+(cellWidth-frame.width)/2;
            int top=i/6*cellHeight+(cellHeight-frame.height)/2;
            Color[] pixels=frame.texture.GetPixels(frame.x,frame.texture.height-frame.top-frame.height,frame.width,frame.height);
            atlas.SetPixels(x,atlas.height-top-frame.height,frame.width,frame.height,pixels);
            csv.AppendFormat(CultureInfo.InvariantCulture,"{0},{1},{2},{3},{4},{5}\n",x,top,frame.width,frame.height,frame.anchor,frame.foot);
        }
        atlas.Apply();
        File.WriteAllBytes("Assets/Art/Hero/HeroEquipped.png",atlas.EncodeToPNG());
        File.WriteAllText("Assets/Art/Hero/HeroEquipped.csv",csv.ToString());
        Object.DestroyImmediate(atlas);foreach(var source in sources)Object.DestroyImmediate(source);
        UnityEditor.AssetDatabase.Refresh();
    }
    static void Load(string path,List<Frame> frames,List<Texture2D> sources)
    {
        var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
        if(!texture.LoadImage(File.ReadAllBytes(path+".png")))throw new IOException("Cannot load animation source: "+path);
        sources.Add(texture);
        string[] lines=File.ReadAllLines(path+".csv");
        for(int i=1;i<lines.Length;i++)
        {
            string[] v=lines[i].Split(',');
            frames.Add(new Frame{texture=texture,x=int.Parse(v[0]),top=int.Parse(v[1]),width=int.Parse(v[2]),height=int.Parse(v[3]),
                anchor=float.Parse(v[4],CultureInfo.InvariantCulture),foot=float.Parse(v[5],CultureInfo.InvariantCulture)});
        }
    }
}

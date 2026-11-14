using System.IO;
using UnityEditor;
using UnityEngine;

// Reuse the approved hero frame for the launcher icon; no additional generated artwork.
public static class ApplicationArtworkBaker
{
    public static void Bake()
    {
        const int size=256;
        var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
        source.LoadImage(File.ReadAllBytes("Docs/ArtSources/Hero/HeroEquippedBase.png"));
        var icon=new Texture2D(size,size,TextureFormat.RGBA32,false);
        // Square portrait crop from the first idle frame, sampled at a uniform scale.
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float u=(x+.5f)/size,v=(y+.5f)/size;
            Color background=Color.Lerp(new Color(.08f,.17f,.16f),new Color(.28f,.40f,.27f),v);
            Color art=source.GetPixelBilinear((34+u*132)/source.width,(source.height-8-132+v*132)/source.height);
            Color color=Color.Lerp(background,art,art.a);color.a=1;
            if(x<5||y<5||x>=size-5||y>=size-5)color=new Color(.72f,.54f,.25f);
            icon.SetPixel(x,y,color);
        }
        icon.Apply();const string path="Assets/Art/Interface/ApplicationIcon.png";
        File.WriteAllBytes(path,icon.EncodeToPNG());Object.DestroyImmediate(source);Object.DestroyImmediate(icon);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown,new[]{AssetDatabase.LoadAssetAtPath<Texture2D>(path)},IconKind.Any);
    }
}

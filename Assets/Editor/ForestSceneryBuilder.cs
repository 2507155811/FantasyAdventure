using System.IO;
using UnityEditor;
using UnityEngine;

public static class ForestSceneryBuilder
{
    const string DirectoryPath = "Assets/Art/Forest";
    static Texture2D atlas;

    // Pixel rectangles refer to the approved 2172 x 724 atlas, measured from its top left.
    public static Sprite Build(Transform cameraTransform)
    {
        string path = DirectoryPath + "/ForestAtlas.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new FileNotFoundException("Missing approved forest atlas", path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        Sprite sky = Slice("DistantForest", new Rect(0, 7, 2172, 160), 8, new Vector2(.5f, 0));
        Sprite forest = Slice("OldForest", new Rect(0, 181, 2172, 200), 26, new Vector2(.5f, 0));
        Sprite ground = Slice("GrassEarth", new Rect(0, 405, 2172, 110), 110, new Vector2(.5f, 1));
        Layer(cameraTransform, sky, "Distant forest", .85f, -30, -1);
        Layer(cameraTransform, forest, "Old woodland", .45f, -20, -.6f);

        Sprite[] props = {
            Slice("LeafyTree", new Rect(105, 523, 300, 195), 90, new Vector2(.5f, 0)),
            Slice("Fern", new Rect(506, 570, 245, 148), 160, new Vector2(.5f, 0)),
            Slice("MossRock", new Rect(842, 561, 286, 157), 160, new Vector2(.5f, 0)),
            Slice("Mushrooms", new Rect(1220, 577, 221, 141), 180, new Vector2(.5f, 0)),
            Slice("Wildflowers", new Rect(1538, 568, 240, 150), 190, new Vector2(.5f, 0)),
            Slice("Waystone", new Rect(1845, 524, 262, 194), 140, new Vector2(.5f, 0))
        };
        var details = new GameObject("Forest details").transform;
        // Curated clusters: entrance, woodland path, raised ledges and guardian clearing.
        Place(details, props[0], new Vector2(-2, -.49f), 1.5f, -5);
        Place(details, props[5], new Vector2(3, -.49f), 1, -4);
        Place(details, props[0], new Vector2(16, -.49f), 1.8f, -5);
        Place(details, props[0], new Vector2(34, -.49f), 1.6f, -5);
        Place(details, props[5], new Vector2(36, -.49f), 1.15f, -4);
        Place(details, props[0], new Vector2(44, -.49f), 2, -5);
        for (int i = 0; i < 70; i++)
        {
            int kind = 1 + i % 4;
            float x = -2 + i * 1.9f;
            float size = .65f + i % 3 * .12f;
            Place(details, props[kind], new Vector2(x, -.49f), size, -3);
        }
        Place(details, props[4], new Vector2(8, 1.875f), .7f, -3);
        Place(details, props[3], new Vector2(19.7f, 2.875f), .65f, -3);
        Place(details, props[1], new Vector2(31, 1.875f), .7f, -3);
        Place(details, props[5], new Vector2(41, -.49f), 1.15f, -4);
        Place(details, props[0], new Vector2(55, -.49f), 1.7f, -5);
        Place(details, props[0], new Vector2(72, -.49f), 1.6f, -5);
        Place(details, props[5], new Vector2(81, -.49f), 1.3f, -4);
        Place(details, props[0], new Vector2(92, -.49f), 1.8f, -5);
        Place(details, props[5], new Vector2(111, -.49f), 1.5f, -4);
        Place(details, props[5], new Vector2(126, -.49f), 1.5f, -4);
        Place(details, props[0], new Vector2(131, -.49f), 2, -5);
        return ground;
    }

    static void Layer(Transform cameraTransform, Sprite sprite, string name, float travel, int order, float bottom)
    {
        var group = new GameObject(name);
        var parallax = group.AddComponent<ForestParallax2D>();
        parallax.cameraTransform = cameraTransform;
        parallax.cameraTravel = travel;
        float width = sprite.bounds.size.x;
        for (int i = -1; i <= 1; i++)
            Place(group.transform, sprite, new Vector2(21 + i * width, bottom), 1, order);
    }

    static void Place(Transform parent, Sprite sprite, Vector2 position, float scale, int order)
    {
        var go = new GameObject(sprite.name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        // Uniform scaling preserves source proportions; never stretch x/y independently.
        go.transform.localScale = new Vector3(scale, scale, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
    }

    static Sprite Slice(string name, Rect topRect, float pixelsPerUnit, Vector2 pivot)
    {
        Rect rect = new Rect(topRect.x, atlas.height - topRect.y - topRect.height, topRect.width, topRect.height);
        string assetPath = DirectoryPath + "/" + name + ".asset";
        Sprite sprite = Sprite.Create(atlas, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existing)
        {
            EditorUtility.CopySerialized(sprite, existing);
            Object.DestroyImmediate(sprite);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(sprite, assetPath);
        return sprite;
    }
}

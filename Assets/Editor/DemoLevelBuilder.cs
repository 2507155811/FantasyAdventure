using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DemoLevelBuilder
{
    const string ScenePath = "Assets/Scenes/ForestStage01.unity";
    [MenuItem("Fantasy Adventure/Build Demo Level")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 6;
        camera.transform.position = new Vector3(1, 2, -10);
        camera.backgroundColor = new Color(.42f,.68f,.68f);
        camera.gameObject.tag = "MainCamera";
        camera.gameObject.AddComponent<AudioListener>();
        var follow = camera.gameObject.AddComponent<CameraFollow2D>(); follow.maximumX = 122;
        Sprite ground = ForestSceneryBuilder.Build(camera.transform);
        Texture2D heroAtlas = LoadTexture("Assets/Art/Hero/HeroEquipped.png");
        Texture2D enemyAtlas = LoadTexture("Assets/Art/Enemies/ForestEnemies.png");
        Texture2D weapons = LoadTexture("Assets/Art/Weapons/Weapons.png");

        Platform("Forest path",new Vector2(64,-1),new Vector2(136,1),ground);
        float[] ledges = { 9,19,30,47,59,70,88,99,108 };
        for (int i=0;i<ledges.Length;i++)
            Platform("Moss ledge "+i,new Vector2(ledges[i],i%3==1?2.6f:1.6f),new Vector2(3.6f,.55f),ground);
        Boundary(-4); Boundary(133);

        var playerObject = Actor("Red Hood",new Vector2(1,1),new Vector2(.75f,1.3f));
        var player = playerObject.AddComponent<PlayerController2D>();
        var heroAnimation = playerObject.AddComponent<HeroSpriteAnimator2D>(); heroAnimation.actionAtlas = heroAtlas;
        var equipment = playerObject.AddComponent<WeaponEquipment>(); equipment.weaponAtlas = weapons;
        follow.target = player.transform;
        float[] positions = { 8,15,25,35,48,57,66,75,89,98,107 };
        for (int i=0;i<positions.Length;i++) Enemy(positions[i],false,enemyAtlas, i/4);
        EnemyAI2D boss = Enemy(118,true,enemyAtlas,2);
        var chapter = new GameObject("Chapter one progression").AddComponent<ChapterOne>();chapter.player=player;
        var hud = new GameObject("Adventure interface").AddComponent<DemoHUD>();hud.player=player;hud.boss=boss;
        hud.interfaceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/AdventureCJK.otf");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
        Selection.activeObject=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if(!Application.isBatchMode)EditorUtility.DisplayDialog("Fantasy Adventure","第一大关已生成：森林入口、林间古道、守卫遗迹。","确定");
    }
    static Texture2D LoadTexture(string path)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new FileNotFoundException("Required finished artwork missing",path);
        importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.maxTextureSize=4096;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static GameObject Actor(string name,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name);go.transform.position=position;
        var visual=new GameObject("Visual");visual.transform.SetParent(go.transform,false);
        var art=visual.AddComponent<SpriteRenderer>();art.sortingOrder=2;
        var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.2f;body.freezeRotation=true;
        body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var collider=go.AddComponent<CapsuleCollider2D>();collider.size=size;
        var feedback=go.AddComponent<ActorVisual2D>();feedback.visual=visual.transform;feedback.sprite=art;
        return go;
    }
    static EnemyAI2D Enemy(float x,bool boss,Texture2D atlas,int region)
    {
        var go=Actor(boss?"Ancient forest guardian":"Moss slime",new Vector2(x,2),boss?new Vector2(1.8f,2.1f):new Vector2(1,.9f));
        Object.DestroyImmediate(go.GetComponent<ActorVisual2D>());
        var animation=go.AddComponent<EnemySpriteAnimator2D>();animation.actionAtlas=atlas;animation.boss=boss;
        var ai=go.AddComponent<EnemyAI2D>();ai.isBoss=boss;
        ai.maxHealth=boss?600:45+region*20;ai.contactDamage=boss?18:6+region*2;
        ai.moveSpeed=boss?1.4f:1.8f;ai.attackRange=boss?2.1f:1.2f;
        ai.attackInterval=boss?1.9f:1.5f;ai.coinReward=boss?60:10+region*3;
        return ai;
    }
    static void Platform(string name,Vector2 position,Vector2 size,Sprite sprite)
    {
        var go=new GameObject(name);go.transform.position=position;
        go.AddComponent<BoxCollider2D>().size=size;go.AddComponent<GroundPlatform2D>();
        var art=new GameObject("Tiled artwork");art.transform.SetParent(go.transform,false);
        art.transform.localPosition=new Vector3(0,size.y*.5f,0);
        var renderer=art.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=-2;
        renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;renderer.size=size;
    }
    static void Boundary(float x)
    {
        var wall=new GameObject("Forest boundary");wall.transform.position=new Vector3(x,4,0);
        wall.AddComponent<BoxCollider2D>().size=new Vector2(.5f,16);
    }
}

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DemoLevelBuilder
{
    const string ScenePath="Assets/Scenes/ForestStage01.unity";
    [MenuItem("Fantasy Adventure/Build Demo Level")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");ActorAtlasImporter.BeginBuild();HeroAtlasBaker.Bake();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4.8f;
        camera.gameObject.tag="MainCamera";camera.transform.position=new Vector3(5,2,-10);camera.backgroundColor=new Color(.55f,.75f,.65f);
        camera.gameObject.AddComponent<AudioListener>();var follow=camera.gameObject.AddComponent<CameraFollow2D>();
        var playerObject=Actor("Red Hood",new Vector2(.75f,1.3f));
        var player=playerObject.AddComponent<PlayerController2D>();
        var hero=playerObject.AddComponent<HeroSpriteAnimator2D>();hero.actionAtlas=Load("Assets/Art/Hero/HeroEquipped.png");
        hero.sourceFrames=ActorAtlasImporter.Import(hero.actionAtlas,"Assets/Art/Hero/HeroEquipped.csv","Assets/Art/Hero/Frames",114,.65f);
        var equipment=playerObject.AddComponent<WeaponEquipment>();equipment.weaponAtlas=Load("Assets/Art/Weapons/Weapons.png");
        var feedback=playerObject.AddComponent<ActorVisual2D>();feedback.visual=playerObject.transform.Find("Visual");feedback.sprite=feedback.visual.GetComponent<SpriteRenderer>();
        var controls=playerObject.AddComponent<AdventureControls>();controls.player=player;follow.target=player.transform;
        var hud=new GameObject("Adventure interface").AddComponent<DemoHUD>();hud.player=player;
        var interfaceTexture=Load("Assets/Art/Interface/AdventureUI.png");
        hud.interfaceArt=ActorAtlasImporter.Import(interfaceTexture,"Assets/Art/Interface/AdventureUI.csv","Assets/Art/Interface/Frames",100,0);
        hud.interfaceFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/AdventureCJK.otf");
        var chapter=new GameObject("Campaign").AddComponent<ChapterOne>();chapter.player=player;chapter.hud=hud;
        var stage=chapter.gameObject.AddComponent<CampaignStageGenerator>();stage.player=player;stage.hud=hud;chapter.stage=stage;
        var interaction=playerObject.AddComponent<InteractionController>();interaction.player=player;interaction.hud=hud;
        var template=Actor("Enemy template",new Vector2(.95f,1.1f));template.SetActive(false);
        template.AddComponent<EnemySpriteAnimator2D>();template.AddComponent<EnemyAI2D>();stage.enemyTemplate=template;
        Texture2D forestEnemies=Load("Assets/Art/Enemies/ForestEnemies.png");
        stage.forestEnemies=ActorAtlasImporter.Import(forestEnemies,"Assets/Art/Enemies/ForestEnemies.csv","Assets/Art/Enemies/SlimeFrames",130,.45f,0,16);
        stage.guardianEnemies=ActorAtlasImporter.Import(forestEnemies,"Assets/Art/Enemies/ForestEnemies.csv","Assets/Art/Enemies/GuardianFrames",90,1.05f,16,16);
        Texture2D enemies=Load("Assets/Art/Campaign/Enemies.png");
        stage.compactEnemies=ActorAtlasImporter.Import(enemies,"Assets/Art/Campaign/Enemies.csv","Assets/Art/Campaign/EnemyFrames",90,1.05f);
        var cast=Load("Assets/Art/Campaign/CastItems.png");
        stage.castItems=ActorAtlasImporter.Import(cast,"Assets/Art/Campaign/CastItems.csv","Assets/Art/Campaign/CastFrames",200,0);
        var forest=Load("Assets/Art/Forest/ForestAtlas.png");
        stage.ladderSprite=ActorAtlasImporter.Asset(forest,new Rect(910,forest.height-981-246,88,246),new Vector2(.5f,0),100,"Assets/Art/Forest/Ladder.asset");
        var worlds=Load("Assets/Art/Campaign/Worlds.png");stage.backgrounds=new Sprite[8];stage.terrain=new Sprite[8];
        float width=worlds.width/4f,height=worlds.height/2f;
        float[] groundTops={334,334,334,334,763,752,754,749};
        for(int i=0;i<8;i++)
        {
            float x=i%4*width,top=i/4*height;
            Rect background=new Rect(x+2,worlds.height-groundTops[i]+2,width-4,groundTops[i]-top-4);
            Rect terrain=new Rect(x+2,worlds.height-top-height+2,width-4,top+height-groundTops[i]-2);
            stage.backgrounds[i]=ActorAtlasImporter.Asset(worlds,background,new Vector2(.5f,0),background.height/12,"Assets/Art/Campaign/Background"+i+".asset");
            stage.terrain[i]=ActorAtlasImporter.Asset(worlds,terrain,new Vector2(.5f,1),terrain.height,"Assets/Art/Campaign/Terrain"+i+".asset");
        }
        var audio=new GameObject("Game audio").AddComponent<GameAudio>();
        string[] names={"Sword","Hammer","Spear","Magic","Hit","Jump","Hurt","Coin","Menu","Upgrade","Clear","Step"};
        audio.effects=new AudioClip[names.Length];
        for(int i=0;i<names.Length;i++)audio.effects[i]=Audio("Assets/Audio/"+names[i]+".ogg",false);
        audio.music=Audio("Assets/Audio/ForestMusic.ogg",true);
        audio.themes=new AudioClip[8];
        for(int i=0;i<8;i++)audio.themes[i]=Audio("Assets/Audio/Theme"+i+".ogg",true);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
        if(!Application.isBatchMode)EditorUtility.DisplayDialog("Fantasy Adventure","单机主菜单、世界流程与存档场景已生成。","确定");
    }
    static AudioClip Audio(string path,bool stream)
    {
        var importer=(AudioImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new FileNotFoundException("Audio missing",path);
        var settings=importer.defaultSampleSettings;
        settings.loadType=stream?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.8f;
        settings.preloadAudioData=!stream;importer.defaultSampleSettings=settings;importer.loadInBackground=stream;
        importer.forceToMono=!stream;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static Texture2D Load(string path)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new FileNotFoundException("Required artwork missing",path);
        importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
        importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static GameObject Actor(string name,Vector2 size)
    {
        var go=new GameObject(name);go.transform.position=new Vector3(3,1,0);
        var visual=new GameObject("Visual");visual.transform.SetParent(go.transform,false);visual.AddComponent<SpriteRenderer>().sortingOrder=2;
        var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.2f;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.interpolation=RigidbodyInterpolation2D.Interpolate;
        go.AddComponent<CapsuleCollider2D>().size=size;go.AddComponent<FootShadow2D>();return go;
    }
}

using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.SceneManagement;using System.IO;
public static class DemoLevelBuilder{
 const string ScenePath="Assets/Scenes/ForestStage01.unity";
 [MenuItem("Fantasy Adventure/Build Demo Level")]
 public static void Build(){
 Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Art/Generated");
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 var cam=new GameObject("Main Camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6;cam.transform.position=new Vector3(0,2,-10);cam.backgroundColor=new Color(.42f,.68f,.88f);cam.gameObject.tag="MainCamera";cam.gameObject.AddComponent<AudioListener>();cam.gameObject.AddComponent<CameraFollow2D>();
 Sprite hero=MakeSprite("Hero_RedHood",new Color(.88f,.12f,.18f),new Color(1,.78f,.42f));
 Sprite slime=MakeSprite("Enemy_ForestSlime",new Color(.28f,.78f,.34f),Color.white);
 Sprite boss=MakeSprite("Boss_ForestGuardian",new Color(.38f,.24f,.58f),new Color(.95f,.35f,.25f));
 Sprite ground=MakeSprite("ForestGround",new Color(.28f,.48f,.25f),new Color(.48f,.3f,.16f));
 Platform("Forest Floor",new Vector2(21,-1),new Vector2(48,1),ground);Platform("Ledge A",new Vector2(9,2),new Vector2(4,.55f),ground);Platform("Ledge B",new Vector2(19,3.2f),new Vector2(4,.55f),ground);Platform("Ledge C",new Vector2(30,1.8f),new Vector2(5,.55f),ground);
 var player=Actor("Little Red",new Vector2(1,1),hero,new Vector2(.8f,1.3f));player.AddComponent<PlayerController2D>();cam.GetComponent<CameraFollow2D>().target=player.transform;
 Enemy("Moss Slime",new Vector2(7,1),slime,45,7,1.8f,false);Enemy("Moss Slime",new Vector2(13,1),slime,55,8,1.9f,false);Enemy("Forest Bat",new Vector2(23,1),slime,65,9,2.2f,false);Enemy("Ancient Forest Guardian",new Vector2(39,1.2f),boss,420,16,1.5f,true);
 new GameObject("Demo HUD and Touch Controls").AddComponent<DemoHUD>();
 EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};Selection.activeObject=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
 EditorUtility.DisplayDialog("Fantasy Adventure","第一关场景已生成！打开 ForestStage01 并点击 Play。键盘：A/D移动、Space跳跃、Shift闪避、J攻击、K技能、H/M药水。","知道了");
 }
 static Sprite MakeSprite(string name,Color main,Color accent){
 int w=64,h=96;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);Color clear=new Color(0,0,0,0);var p=new Color[w*h];for(int i=0;i<p.Length;i++)p[i]=clear;
 void R(int x0,int y0,int x1,int y1,Color c){for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(x>=0&&x<w&&y>=0&&y<h)p[y*w+x]=c;}
 void E(int cx,int cy,int rx,int ry,Color c){for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(((x-cx)*(x-cx))/(float)(rx*rx)+((y-cy)*(y-cy))/(float)(ry*ry)<=1)p[y*w+x]=c;}
 R(17,8,47,51,main);R(20,4,29,13,new Color(.35f,.2f,.12f));R(36,4,45,13,new Color(.35f,.2f,.12f));E(32,65,20,19,accent);E(32,79,23,13,main);R(12,78,52,88,main);
 tex.SetPixels(p);tex.Apply();string path="Assets/Art/Generated/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
 var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.alphaIsTransparency=true;ti.filterMode=FilterMode.Bilinear;ti.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
 }
 static GameObject Platform(string n,Vector2 pos,Vector2 size,Sprite sprite){var go=new GameObject(n);go.transform.position=pos;go.transform.localScale=new Vector3(size.x,size.y,1);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=-2;go.AddComponent<BoxCollider2D>();go.AddComponent<GroundPlatform2D>();return go;}
 static GameObject Actor(string n,Vector2 pos,Sprite sprite,Vector2 size){var go=new GameObject(n);go.transform.position=pos;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=2;var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=3.2f;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var col=go.AddComponent<CapsuleCollider2D>();col.size=size;return go;}
 static void Enemy(string n,Vector2 pos,Sprite sprite,int hp,int damage,float speed,bool boss){var go=Actor(n,pos,sprite,boss?new Vector2(1.8f,2.2f):new Vector2(.9f,1.1f));var ai=go.AddComponent<EnemyAI2D>();ai.maxHealth=hp;ai.contactDamage=damage;ai.moveSpeed=speed;ai.isBoss=boss;ai.coinReward=boss?100:8;}
}
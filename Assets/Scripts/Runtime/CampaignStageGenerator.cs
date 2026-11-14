using System.Collections.Generic;
using UnityEngine;

public class CampaignStageGenerator : MonoBehaviour
{
    public PlayerController2D player;
    public DemoHUD hud;
    public GameObject enemyTemplate;
    public Sprite[] backgrounds,terrain,castItems,compactEnemies,forestEnemies,guardianEnemies;
    public Sprite ladderSprite;
    public float Width=100;
    GameObject world;
    readonly List<Sprite> cuts=new List<Sprite>();
    CampaignMap current;

    public void Build(CampaignMap map)
    {
        if(world){world.SetActive(false);Destroy(world);}
        foreach(var cut in cuts)if(cut)Destroy(cut);cuts.Clear();
        Width=map.Safe?64:map.Boss?112:88+map.Id%4*12;
        current=map;world=new GameObject("Region "+map.Id+" "+map.Name);
        var backdrop=new GameObject("Backdrop");backdrop.transform.SetParent(world.transform,false);
        var parallax=backdrop.AddComponent<ForestParallax2D>();parallax.cameraTransform=Camera.main.transform;parallax.cameraTravel=.72f;
        Sprite background=backgrounds[map.Theme];float step=background.bounds.size.x;
        for(int i=-6;i<=12;i++)
        {
            var art=Art(backdrop.transform,background,new Vector2(20+i*step,-.6f),1,-20);art.flipX=(i&1)!=0;
        }
        Ground("Main ground",new Vector2(Width*.5f-2,-1),new Vector2(Width+8,1),false);
        int pathCount=map.Safe?3:5+map.Id%3;
        for(int i=0;i<pathCount;i++)
        {
            float x=12+(Width-26)*(i+.5f)/pathCount;
            float elevation=1.1f+((i+map.Id)%3)*.55f;
            float span=3.6f+((map.Id+i)%3)*.8f;
            Ground("Upper path",new Vector2(x,elevation),new Vector2(span,.5f),true);
        }
        // Extend the same earth surface down to the viewport edge; never show a blank strip under the floor.
        Tile(terrain[map.Theme],world.transform,new Rect(-6,-5.5f,Width+8,4),-2,true);
        Boundary(-6);Boundary(Width+2);
        WorldInteraction npc=Interact("Resident",castItems[map.Npc],new Vector2(map.Boss?Width-10:7,-.5f),WorldInteraction.Kind.Npc);
        npc.label=map.Id==39?"戈登博士":CampaignCatalog.NpcNames[map.Npc];
        WorldInteraction portal=Interact("Time gate",castItems[12],new Vector2(Width-4,.1f),WorldInteraction.Kind.Exit);
        portal.label="下一地区";portal.transform.localScale=Vector3.one*.7f;
        portal.gameObject.AddComponent<TimeGateEffect>();
        if(!map.Safe)
        {
            int count=6;
            for(int i=0;i<count;i++)SpawnEnemy(13+(i+1)*(Width-31)/7,map.Species,false);
            if(map.Boss)hud.boss=SpawnEnemy(Width-13,map.Species<0?-2:map.Species,true);
            else hud.boss=null;
        }
        else hud.boss=null;
        if(map.Id==66)SpawnEnemy(30,2,false);
        int pickup=0;
        foreach(var quest in CampaignCatalog.Quests)
        {
            if(quest.Map!=map.Id||quest.Goal!=QuestGoal.Collect||CampaignQuestLog.Done(quest))continue;
            for(int i=0;i<quest.Count;i++)
            {
                string key="pickup."+map.Id+"."+quest.Id+"."+i;
                if(GameSave.GetInt(key,0)!=0)continue;
                float x=13+(pickup++*13)%(Width-26);
                var go=new GameObject(CampaignCatalog.ItemNames[quest.Item]);go.transform.SetParent(world.transform,false);go.transform.position=new Vector3(x,.1f,0);
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=castItems[8+quest.Item];renderer.sortingOrder=3;
                go.transform.localScale=Vector3.one*.35f;
                go.AddComponent<BoxCollider2D>().isTrigger=true;
                var item=go.AddComponent<WorldPickup2D>();item.item=quest.Item;item.saveKey=key;
            }
        }
        var camera=Camera.main.GetComponent<CameraFollow2D>();camera.worldLeft=-6;camera.worldRight=Width+2;camera.maximumX=Width;
    }
    void Ground(string name,Vector2 position,Vector2 size,bool oneWay)
    {
        var go=new GameObject(name);go.transform.SetParent(world.transform,false);go.transform.position=position;
        var collider=go.AddComponent<BoxCollider2D>();collider.size=size;go.AddComponent<GroundPlatform2D>();
        if(oneWay)
        {
            collider.usedByEffector=true;var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.useOneWayGrouping=true;
            Ladder(position.x,-.5f,position.y+size.y*.5f,collider);
        }
        Tile(terrain[current.Theme],go.transform,new Rect(-size.x*.5f,-size.y*.5f,size.x,size.y),-2,false);
    }
    void Ladder(float x,float bottom,float top,Collider2D platform)
    {
        var go=new GameObject("Ladder");go.transform.SetParent(world.transform,false);go.transform.position=new Vector3(x,(bottom+top)*.5f,0);
        var ladder=go.AddComponent<Ladder2D>();ladder.bottomY=bottom;ladder.topY=top;ladder.landingPlatform=platform;
        var trigger=go.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(1.2f,top-bottom+2);
        Art(go.transform,ladderSprite,new Vector2(0,-(top-bottom)*.5f),(top-bottom)/ladderSprite.bounds.size.y,-1);
    }
    void Tile(Sprite source,Transform parent,Rect area,int order,bool soilOnly)
    {
        Rect region=source.rect;
        if(soilOnly){region.height*=.5f;}
        float nativeWidth=region.width/source.pixelsPerUnit,nativeHeight=region.height/source.pixelsPerUnit;
        float y=0;int row=0;
        while(y<area.height-.0001f)
        {
            float height=Mathf.Min(nativeHeight,area.height-y),x=0;int col=0;
            while(x<area.width-.0001f)
            {
                float width=Mathf.Min(nativeWidth,area.width-x);bool flipX=(col&1)!=0,flipY=soilOnly?(row&1)==0:(row&1)!=0;
                float pw=Mathf.Min(region.width,width*source.pixelsPerUnit),ph=Mathf.Min(region.height,height*source.pixelsPerUnit);
                Rect rect=new Rect(flipX?region.xMax-pw:region.x,flipY?region.y:region.yMax-ph,pw,ph);
                var cut=Sprite.Create(source.texture,rect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect);cuts.Add(cut);
                var renderer=Art(parent,cut,new Vector2(area.x+x+width*.5f,area.yMax-y-height*.5f),1,order);renderer.flipX=flipX;renderer.flipY=flipY;
                x+=width;col++;
            }
            y+=height;row++;
        }
    }
    EnemyAI2D SpawnEnemy(float x,int species,bool boss)
    {
        GameObject go=Instantiate(enemyTemplate,world.transform);go.name=boss?"Region boss":"Enemy";go.transform.position=new Vector3(x,1,0);
        var ai=go.GetComponent<EnemyAI2D>();ai.isBoss=boss;ai.species=species;ai.isTraining=current.Id==66;
        int level=Mathf.Clamp(current.Order*3/2+1+(current.Id>=52&&current.Id<=64?GameSave.GetInt("dungeonCycle",0)*3:0),1,99);
        ai.maxHealth=boss?180+level*32:40+level*6;ai.contactDamage=boss?12+level:5+level/2;
        ai.coinReward=boss?35+level:5+level/3;ai.moveSpeed=boss?1.6f:2;
        ai.detectRange=9;ai.attackRange=species==3?5:boss?2.1f:1.3f;ai.attackInterval=boss?1.7f:1.5f;
        var animation=go.GetComponent<EnemySpriteAnimator2D>();animation.boss=boss;
        animation.compactSet=species>=0;animation.species=Mathf.Max(0,species);
        animation.sourceFrames=species>=0?compactEnemies:boss?guardianEnemies:forestEnemies;
        Transform visual=go.transform.Find("Visual");
        float scale=boss?1:species>=0?.55f:1;
        visual.localScale=Vector3.one*scale;visual.localPosition=new Vector3(0,species>=0&&!boss?.0275f:!boss?-.1f:0,0);
        var collider=go.GetComponent<CapsuleCollider2D>();collider.size=boss?new Vector2(1.7f,2.1f):new Vector2(.95f,1.1f);
        go.SetActive(true);return ai;
    }
    WorldInteraction Interact(string name,Sprite sprite,Vector2 position,WorldInteraction.Kind kind)
    {
        var go=new GameObject(name);go.transform.SetParent(world.transform,false);go.transform.position=position;
        var art=go.AddComponent<SpriteRenderer>();art.sprite=sprite;art.sortingOrder=2;
        var interaction=go.AddComponent<WorldInteraction>();interaction.kind=kind;return interaction;
    }
    void Boundary(float x)
    {
        var go=new GameObject("Boundary");go.transform.SetParent(world.transform,false);go.transform.position=new Vector3(x,4,0);go.AddComponent<BoxCollider2D>().size=new Vector2(.4f,16);
    }
    static SpriteRenderer Art(Transform parent,Sprite sprite,Vector2 position,float scale,int order)
    {
        var go=new GameObject(sprite.name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=Vector3.one*scale;
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;return renderer;
    }
}

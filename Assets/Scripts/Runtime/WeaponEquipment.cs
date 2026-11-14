using UnityEngine;

public enum WeaponFamily { Sword, Hammer, Spear, Staff }

[DefaultExecutionOrder(20)]
public class WeaponEquipment : MonoBehaviour
{
    public Texture2D weaponAtlas;
    public WeaponFamily Family { get; private set; }
    public int Tier { get { return tiers[(int)Family]; } }
    public string FamilyName { get { return Names[(int)Family]; } }
    public string SkillName { get { return Skills[(int)Family]; } }
    public static readonly string[] Names = { "剑", "锤", "长枪", "法杖" };
    public static readonly string[] Skills = { "破风剑气", "撼地重击", "贯穿突刺", "星光爆发" };
    public static readonly string[] Qualities = { "普通", "精良", "稀有", "史诗" };
    public float AttackInterval { get { return new[] { .32f, .65f, .45f, .5f }[(int)Family]; } }
    public int Damage { get { return new[] { 22, 38, 26, 20 }[(int)Family] + Tier * 10 + (ChapterOne.Instance ? ChapterOne.Instance.Level - 1 : 0) * 2 + CampaignInventory.Passive(1)*3 + GameSave.GetInt("relic",0)*2; } }
    public int UpgradeCost { get { return 30 * (Tier + 1); } }
    public Color EffectColor { get { return new[] { new Color(1,.83f,.4f), new Color(.4f,.9f,.4f), new Color(.25f,.75f,1), new Color(1,.25f,.3f) }[Tier]; } }
    readonly int[] tiers = new int[4];
    Sprite[] icons,heldSprites;
    static readonly Vector2[] Grips={
        new Vector2(.43f,.20f),new Vector2(.40f,.19f),new Vector2(.40f,.18f),new Vector2(.46f,.15f),
        new Vector2(.43f,.30f),new Vector2(.40f,.29f),new Vector2(.39f,.29f),new Vector2(.36f,.29f),
        new Vector2(.43f,.37f),new Vector2(.43f,.37f),new Vector2(.41f,.37f),new Vector2(.43f,.37f),
        new Vector2(.40f,.34f),new Vector2(.38f,.34f),new Vector2(.35f,.34f),new Vector2(.35f,.34f)};
    static readonly Vector2[][] Hands={
        new[]{new Vector2(-.08f,.18f),new Vector2(.05f,.35f),new Vector2(.35f,.25f),new Vector2(.48f,.12f),new Vector2(.24f,.02f),new Vector2(.12f,.04f)},
        new[]{new Vector2(.1f,.05f),new Vector2(.02f,.45f),new Vector2(-.02f,.75f),new Vector2(.1f,.82f),new Vector2(.46f,-.15f),new Vector2(.22f,-.15f)},
        new[]{new Vector2(.1f,.04f),new Vector2(-.1f,.1f),new Vector2(.26f,.1f),new Vector2(.5f,.12f),new Vector2(.24f,.1f),new Vector2(.12f,.04f)},
        new[]{new Vector2(.1f,.04f),new Vector2(.05f,.24f),new Vector2(.02f,.3f),new Vector2(.08f,.2f),new Vector2(.1f,.05f),new Vector2(.1f,.04f)}};
    static readonly float[][] Angles={new[]{15f,-55,-10,80,115,15},new[]{20f,-35,-75,-65,125,65},new[]{60f,65,75,85,75,60},new[]{5f,0,-5,-5,0,5}};
    SpriteRenderer weapon;
    HeroSpriteAnimator2D animation2D;
    PlayerController2D player;

    void Awake()
    {
        ReloadSave();
        icons = HeroSpriteAnimator2D.Slice(weaponAtlas, 4, 4, .9f);
        heldSprites=new Sprite[16];
        float cellWidth=weaponAtlas.width/4f,cellHeight=weaponAtlas.height/4f;
        for(int i=0;i<16;i++)heldSprites[i]=Sprite.Create(weaponAtlas,new Rect(i%4*cellWidth,(3-i/4)*cellHeight,cellWidth,cellHeight),Grips[i],cellHeight*.9f/1.5f,0,SpriteMeshType.FullRect);
        var go = new GameObject("Equipped weapon");
        go.transform.SetParent(transform, false);
        weapon = go.AddComponent<SpriteRenderer>();
        weapon.sortingOrder = 4;
        animation2D = GetComponent<HeroSpriteAnimator2D>();
        player = GetComponent<PlayerController2D>();
    }

    public void ReloadSave()
    {
        Family=(WeaponFamily)Mathf.Clamp(GameSave.GetInt("FA.weapon",0),0,3);
        if(!CampaignInventory.OwnsWeapon((int)Family))Family=WeaponFamily.Sword;
        for(int i=0;i<4;i++)tiers[i]=Mathf.Clamp(GameSave.GetInt("FA.tier."+i,0),0,3);
    }
    int Form(int family)
    {
        int levelForm = ChapterOne.Instance ? (ChapterOne.Instance.Level - 1) / 4 : 0;
        return family * 4 + Mathf.Clamp(tiers[family] + levelForm, 0, 3);
    }
    public Sprite Icon(int family){return icons[Form(family)];}
    public void Equip(int family)
    {
        if(!CampaignInventory.OwnsWeapon(family)){DemoHUD.ShowMessage("先从商店或首领奖励获得这类武器");return;}
        if(player)player.CancelAttack();
        Family = (WeaponFamily)Mathf.Clamp(family, 0, 3);
        GameSave.SetInt("FA.weapon", (int)Family);
        GameSave.Save();
    }
    public bool Upgrade()
    {
        if (Tier >= 3 || !ChapterOne.Instance || !ChapterOne.Instance.SpendCoins(UpgradeCost)) return false;
        GameAudio.Play(GameSound.Upgrade);
        tiers[(int)Family]++;
        GameSave.SetInt("FA.tier." + (int)Family, Tier);
        GameSave.SetInt("upgrades",GameSave.GetInt("upgrades",0)+1);
        if(ChapterOne.Instance)CampaignQuestLog.Event(QuestGoal.Upgrade,ChapterOne.Instance.MapId);
        GameSave.Save();
        return true;
    }
    void LateUpdate()
    {
        weapon.enabled = player.health > 0 && !player.IsClimbing;
        weapon.sprite=heldSprites[Form((int)Family)];
        int direction=player.Facing;
        int frame=animation2D?animation2D.CurrentFrame:0;
        Vector2 grip=new Vector2(.12f,.04f);
        float targetAngle=Family==WeaponFamily.Spear?60:Family==WeaponFamily.Staff?5:15;
        if(frame>=18&&frame<42)
        {
            int family=(frame-18)/6,index=(frame-18)%6;
            grip=Hands[family][index];targetAngle=Angles[family][index];
        }
        else if(frame>=42)grip+=new Vector2(-.08f,-.08f);
        weapon.flipX=direction<0;
        float scale=Family==WeaponFamily.Hammer?.72f:Family==WeaponFamily.Spear?1.65f:.85f;
        weapon.transform.localScale=new Vector3(scale,scale,1);
        weapon.transform.localRotation=Quaternion.Euler(0,0,(20-targetAngle)*direction);
        weapon.transform.localPosition=new Vector3(grip.x*direction,grip.y,0);
    }
    void OnDestroy(){if(icons!=null)foreach(var icon in icons)if(icon)Destroy(icon);if(heldSprites!=null)foreach(var sprite in heldSprites)if(sprite)Destroy(sprite);}
}

using UnityEngine;

public enum WeaponFamily { Sword, Hammer, Spear, Staff }

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
    public int Damage { get { return new[] { 22, 38, 26, 20 }[(int)Family] + Tier * 10 + (ChapterOne.Instance ? ChapterOne.Instance.Level - 1 : 0) * 2; } }
    public int UpgradeCost { get { return 30 * (Tier + 1); } }
    public Color EffectColor { get { return new[] { new Color(1,.83f,.4f), new Color(.4f,.9f,.4f), new Color(.25f,.75f,1), new Color(1,.25f,.3f) }[Tier]; } }
    readonly int[] tiers = new int[4];
    Sprite[] icons;
    SpriteRenderer weapon;
    HeroSpriteAnimator2D animation2D;
    PlayerController2D player;

    void Awake()
    {
        Family = (WeaponFamily)Mathf.Clamp(PlayerPrefs.GetInt("FA.weapon", 0), 0, 3);
        for (int i = 0; i < 4; i++) tiers[i] = Mathf.Clamp(PlayerPrefs.GetInt("FA.tier." + i, 0), 0, 3);
        icons = HeroSpriteAnimator2D.Slice(weaponAtlas, 4, 4, .9f);
        var go = new GameObject("Equipped weapon");
        go.transform.SetParent(transform, false);
        weapon = go.AddComponent<SpriteRenderer>();
        weapon.sortingOrder = 4;
        animation2D = GetComponent<HeroSpriteAnimator2D>();
        player = GetComponent<PlayerController2D>();
    }

    public Sprite Icon(int family)
    {
        int levelForm = ChapterOne.Instance ? (ChapterOne.Instance.Level - 1) / 4 : 0;
        return icons[family * 4 + Mathf.Clamp(tiers[family] + levelForm, 0, 3)];
    }
    public void Equip(int family)
    {
        Family = (WeaponFamily)Mathf.Clamp(family, 0, 3);
        PlayerPrefs.SetInt("FA.weapon", (int)Family);
        PlayerPrefs.Save();
    }
    public bool Upgrade()
    {
        if (Tier >= 3 || !ChapterOne.Instance || !ChapterOne.Instance.SpendCoins(UpgradeCost)) return false;
        tiers[(int)Family]++;
        PlayerPrefs.SetInt("FA.tier." + (int)Family, Tier);
        PlayerPrefs.Save();
        return true;
    }
    void LateUpdate()
    {
        weapon.enabled = player.health > 0;
        weapon.sprite = Icon((int)Family);
        int direction = player.Facing;
        bool attacking = animation2D && animation2D.IsAttacking;
        float p = animation2D ? animation2D.ActionProgress : 0;
        float angle = Family == WeaponFamily.Staff ? -12 : Family == WeaponFamily.Spear ? -65 : -25;
        Vector2 grip = new Vector2(.3f, -.04f);
        if (attacking)
        {
            if (Family == WeaponFamily.Hammer) { angle = Mathf.Lerp(100, -110, p); grip.y = Mathf.Sin(p * Mathf.PI) * .35f; }
            else if (Family == WeaponFamily.Spear) { angle = -70; grip.x += Mathf.Sin(p * Mathf.PI) * .5f; }
            else if (Family == WeaponFamily.Sword) angle = Mathf.Lerp(65, -105, p);
            else grip.y = .1f;
        }
        weapon.flipX = direction < 0;
        float scale = Family == WeaponFamily.Hammer ? .72f : Family == WeaponFamily.Spear ? 1 : .7f;
        weapon.transform.localScale = new Vector3(scale, scale, 1);
        weapon.transform.localRotation = Quaternion.Euler(0, 0, angle * direction);
        // The texture's grip is below its center; place its lower grip point at the hand.
        Vector3 handleOffset = weapon.transform.localRotation * new Vector3(0, .43f * scale, 0);
        weapon.transform.localPosition = new Vector3(grip.x * direction, grip.y, 0) + handleOffset;
    }
    void OnDestroy() { if (icons != null) foreach (var icon in icons) if (icon) Destroy(icon); }
}

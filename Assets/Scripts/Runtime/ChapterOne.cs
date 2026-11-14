using UnityEngine;

public class ChapterOne : MonoBehaviour
{
    public static ChapterOne Instance { get; private set; }
    public PlayerController2D player;
    public int Coins { get; private set; }
    public int Experience { get; private set; }
    public int Defeated { get; private set; }
    public int Region { get; private set; }
    public bool Completed { get; private set; }
    public bool MenuOpen { get; set; }
    public int Level { get { return 1 + Experience / 80; } }
    public string RegionName { get { return new[] { "1-1 森林入口", "1-2 林间古道", "1-3 守卫遗迹" }[Region]; } }
    public string Objective { get { return Completed ? "第一大关完成！领取奖励后可重玩" : Region == 2 ? "击败古树守卫" : "沿林间道路向右前进"; } }
    float checkpoint = 1;
    void Awake()
    {
        Instance = this;
        Coins = PlayerPrefs.GetInt("FA.coins", 0);
        Experience = PlayerPrefs.GetInt("FA.exp", 0);
    }
    void Start()
    {
        Region = Mathf.Clamp(PlayerPrefs.GetInt("FA.checkpoint", 0), 0, 2);
        checkpoint = Region == 0 ? 1 : Region == 1 ? 42 : 82;
        player.RecoverAt(new Vector3(checkpoint, 1, 0));
        SnapCamera();
        DemoHUD.ShowMessage(RegionName + "：左右移动，跳跃越过高台，攻击与技能击退敌人");
    }
    void Update()
    {
        if (!player || MenuOpen || Completed || player.health <= 0) return;
        if (player.transform.position.y < -8) { player.TakeDamage(9999); return; }
        int next = player.transform.position.x >= 80 ? 2 : player.transform.position.x >= 40 ? 1 : 0;
        if (next <= Region) return;
        Region = next;
        checkpoint = Region == 1 ? 42 : 82;
        player.health = player.maxHealth;
        player.mana = player.maxMana;
        player.redPotions = Mathf.Max(player.redPotions, 2);
        PlayerPrefs.SetInt("FA.checkpoint", Region);
        Save();
        DemoHUD.ShowMessage(RegionName + "：检查点已保存，生命和魔法已恢复");
    }
    public void Reward(int coins, bool boss)
    {
        Defeated++;
        Coins += coins;
        Experience += boss ? 100 : 20;
        if (boss)
        {
            Completed = true;
            Coins += 100;
            PlayerPrefs.SetInt("FA.chapter1.cleared", 1);
            PlayerPrefs.SetInt("FA.checkpoint", 0);
            DemoHUD.ShowMessage("古树守卫已净化！第一大关完成，奖励 100 金币");
        }
        Save();
    }
    public bool SpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount) return false;
        Coins -= amount; Save(); return true;
    }
    public void Retry()
    {
        player.RecoverAt(new Vector3(checkpoint, 1, 0));
        SnapCamera();
        player.redPotions = Mathf.Max(1, player.redPotions);
        MenuOpen = false;
        DemoHUD.ShowMessage("已回到检查点");
    }
    void SnapCamera()
    {
        if(Camera.main)Camera.main.transform.position = new Vector3(player.transform.position.x,player.transform.position.y+1.4f,-10);
    }
    public void Save()
    {
        PlayerPrefs.SetInt("FA.coins", Coins);
        PlayerPrefs.SetInt("FA.exp", Experience);
        PlayerPrefs.Save();
    }
    void OnDestroy() { if (Instance == this) Instance = null; }
}

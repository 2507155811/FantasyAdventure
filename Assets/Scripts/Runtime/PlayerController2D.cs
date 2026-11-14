using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 6, jumpForce = 12;
    public int maxHealth = 100, maxMana = 60, health = 100, mana = 60, redPotions = 3, bluePotions = 3;
    public WeaponEquipment equipment;
    public bool IsGrounded { get { return groundContacts.Count > 0; } }
    public int Facing { get; private set; } = 1;
    public float SkillRemaining { get { return Mathf.Max(0, skillTimer); } }
    readonly HashSet<int> groundContacts = new HashSet<int>();
    Rigidbody2D body;
    float attackTimer, skillTimer, invulnerableUntil, manaClock;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>(); body.freezeRotation = true;
        equipment = GetComponent<WeaponEquipment>();
        health = maxHealth; mana = maxMana;
    }
    void Update()
    {
        var chapter = ChapterOne.Instance;
        if (health <= 0 || chapter && (chapter.MenuOpen || chapter.Completed))
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
            DemoInput.ClearOneShot(); return;
        }
        attackTimer -= Time.deltaTime; skillTimer -= Time.deltaTime;
        manaClock += Time.deltaTime;
        if (manaClock >= 1) { mana = Mathf.Min(maxMana, mana + 3); manaClock -= 1; }
        float move = DemoInput.Horizontal;
        if (Mathf.Abs(move) < .01f) move = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(move) > .01f) Facing = move > 0 ? 1 : -1;
        body.linearVelocity = new Vector2(move * moveSpeed, body.linearVelocity.y);
        if ((Input.GetKeyDown(KeyCode.Space) || DemoInput.JumpPressed) && IsGrounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce); groundContacts.Clear();
        }
        if (Input.GetKey(KeyCode.J) || DemoInput.AttackPressed) Attack();
        if (Input.GetKeyDown(KeyCode.K) || DemoInput.SkillPressed) Skill();
        DemoInput.ClearOneShot();
    }
    void Attack()
    {
        if (attackTimer > 0 || !equipment) return;
        attackTimer = equipment.AttackInterval;
        GetComponent<ActorVisual2D>().Attack(Facing);
        CombatEffect2D.Spawn(transform.position, Facing, false, equipment.EffectColor);
        if (equipment.Family == WeaponFamily.Staff)
            MagicBolt2D.Fire(transform.position + Vector3.right * Facing * .7f, Facing, equipment.Damage, equipment.EffectColor);
        else Hit(equipment.Family == WeaponFamily.Spear ? 1.35f : equipment.Family == WeaponFamily.Hammer ? 1.1f : .85f,
            equipment.Family == WeaponFamily.Spear ? 1.4f : .75f, equipment.Damage);
    }
    void Skill()
    {
        if (!equipment || skillTimer > 0 || attackTimer > 0 || mana < 15) return;
        mana -= 15; skillTimer = 4; attackTimer = equipment.AttackInterval;
        GetComponent<ActorVisual2D>().Attack(Facing, true);
        Vector3 center = transform.position;
        if (equipment.Family == WeaponFamily.Staff) center += Vector3.right * Facing * 3;
        CombatEffect2D.Spawn(center, Facing, true, equipment.EffectColor);
        if (equipment.Family == WeaponFamily.Sword)
        {
            MagicBolt2D.Fire(center, Facing, equipment.Damage * 3, equipment.EffectColor);
            Hit(1.1f, 1, equipment.Damage);
        }
        else if (equipment.Family == WeaponFamily.Staff) Area(center, 2, equipment.Damage * 2);
        else if (equipment.Family == WeaponFamily.Hammer) Hit(2.1f, .65f, equipment.Damage * 2);
        else Hit(1.65f, 1.65f, equipment.Damage * 2);
    }
    void Hit(float radius, float forward, int damage)
    {
        Area((Vector2)transform.position + Vector2.right * Facing * forward, radius, damage);
    }
    void Area(Vector2 center, float radius, int damage)
    {
        var seen = new HashSet<EnemyAI2D>();
        foreach (var collider in Physics2D.OverlapCircleAll(center, radius))
        {
            var enemy = collider.GetComponent<EnemyAI2D>();
            if (enemy && seen.Add(enemy)) enemy.TakeDamage(damage, Facing);
        }
    }
    public void RecoverAt(Vector3 position)
    {
        transform.position = position;
        body.linearVelocity = Vector2.zero;
        groundContacts.Clear();
        health = maxHealth; mana = maxMana;
        attackTimer = 0; skillTimer = 0; invulnerableUntil = Time.time + 1;
        GetComponent<HeroSpriteAnimator2D>().Play("idle");
    }
    public void UseRedPotion()
    {
        if (health > 0 && redPotions > 0 && health < maxHealth) { redPotions--; health = Mathf.Min(maxHealth, health + 45); }
    }
    public void UseBluePotion()
    {
        if (health > 0 && bluePotions > 0 && mana < maxMana) { bluePotions--; mana = Mathf.Min(maxMana, mana + 30); }
    }
    public void TakeDamage(int amount)
    {
        if (health <= 0 || amount <= 0 || Time.time < invulnerableUntil || ChapterOne.Instance && ChapterOne.Instance.Completed) return;
        invulnerableUntil = Time.time + .55f;
        health = Mathf.Max(0, health - amount);
        if (health == 0)
        {
            GetComponent<HeroSpriteAnimator2D>().Play("death"); DemoHUD.ShowMessage("点击返回检查点继续冒险");
        }
        else
        {
            GetComponent<ActorVisual2D>().Hurt();
            if (health <= maxHealth * .35f) UseRedPotion();
        }
    }
    void Ground(Collision2D collision)
    {
        int id = collision.collider.GetInstanceID();
        if (!collision.collider.GetComponent<GroundPlatform2D>()) return;
        for (int i = 0; i < collision.contactCount; i++)
            if (collision.GetContact(i).normal.y > .5f) { groundContacts.Add(id); return; }
        groundContacts.Remove(id);
    }
    void OnCollisionEnter2D(Collision2D collision) { Ground(collision); }
    void OnCollisionStay2D(Collision2D collision) { Ground(collision); }
    void OnCollisionExit2D(Collision2D collision) { groundContacts.Remove(collision.collider.GetInstanceID()); }
}

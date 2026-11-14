using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyAI2D : MonoBehaviour
{
    public int maxHealth = 55, contactDamage = 8, coinReward = 8;
    public float moveSpeed = 2, detectRange = 8, attackRange = 1.4f, attackInterval = 1.4f;
    public bool isBoss;
    public bool IsDead { get; private set; }
    public int Health { get { return hp; } }
    public bool WindingUp { get { return windup > 0; } }
    int hp, direction = -1;
    float cooldown, windup, stagger;
    Rigidbody2D body;
    PlayerController2D target;
    EnemySpriteAnimator2D animation2D;
    void Awake() { body = GetComponent<Rigidbody2D>(); body.freezeRotation = true; }
    void Start()
    {
        hp = maxHealth;
        target = FindFirstObjectByType<PlayerController2D>();
        animation2D = GetComponent<EnemySpriteAnimator2D>();
        if(target)Physics2D.IgnoreCollision(GetComponent<Collider2D>(),target.GetComponent<Collider2D>());
    }
    void Update()
    {
        if (IsDead || !target || target.health <= 0 || ChapterOne.Instance && (ChapterOne.Instance.MenuOpen || ChapterOne.Instance.Completed))
        { body.linearVelocity = new Vector2(0, body.linearVelocity.y); return; }
        cooldown -= Time.deltaTime; stagger -= Time.deltaTime;
        float distance = Vector2.Distance(transform.position, target.transform.position);
        direction = target.transform.position.x >= transform.position.x ? 1 : -1;
        if (windup > 0)
        {
            windup -= Time.deltaTime;
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
            if (windup <= 0)
            {
                float radius = isBoss ? 2.5f : attackRange + .4f;
                if (distance < radius) target.TakeDamage(contactDamage + (isBoss && hp < maxHealth / 2 ? 4 : 0));
                CombatEffect2D.Spawn(transform.position, direction, isBoss, new Color(1,.5f,.2f));
                cooldown = isBoss && hp < maxHealth / 2 ? attackInterval * .7f : attackInterval;
            }
            return;
        }
        if (stagger > 0) return;
        if (distance > detectRange) { body.linearVelocity = new Vector2(0, body.linearVelocity.y); return; }
        if (distance > attackRange)
            body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocity.y);
        else
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
            if (cooldown <= 0) { windup = isBoss ? .65f : .45f; animation2D.Play("attack", direction); }
        }
    }
    public void TakeDamage(int amount, int knockDirection)
    {
        if (IsDead) return;
        hp = Mathf.Max(0, hp - amount);
        if (hp == 0) { Die(); return; }
        if (!isBoss) { stagger = .18f; windup = 0; body.linearVelocity = new Vector2(knockDirection * 3, 2); }
        animation2D.Play("hurt");
    }
    void Die()
    {
        IsDead = true;
        animation2D.Play("death");
        body.linearVelocity = Vector2.zero; body.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        if (ChapterOne.Instance) ChapterOne.Instance.Reward(coinReward, isBoss);
        Destroy(gameObject, .85f);
    }
}

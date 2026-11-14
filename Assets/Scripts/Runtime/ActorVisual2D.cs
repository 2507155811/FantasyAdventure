using UnityEngine;

// Animate a visual child only: collision shapes and physics keep their original size.
public class ActorVisual2D : MonoBehaviour
{
    public Transform visual;
    public SpriteRenderer sprite;
    Rigidbody2D body;
    HeroSpriteAnimator2D heroAnimation;
    Color baseColor;
    Vector3 baseScale;
    float attackUntil, hurtUntil, dodgeUntil, deathAt = -1;
    int facing = 1;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        heroAnimation = GetComponent<HeroSpriteAnimator2D>();
        baseScale = visual.localScale;
        baseColor = sprite.color;
    }

    public void Attack(int direction, bool skill = false)
    {
        facing = direction;
        if (heroAnimation) heroAnimation.Play(skill ? "skill" : "attack");
        attackUntil = Time.time + (skill ? .35f : .22f);
    }

    public void Hurt() { hurtUntil = Time.time + .16f; GetComponent<HeroSpriteAnimator2D>()?.Play("hurt"); }
    public void Dodge(int direction) { facing = direction; dodgeUntil = Time.time + .18f; GetComponent<HeroSpriteAnimator2D>()?.Play("dodge"); }
    public void Die() { deathAt = Time.time; }

    void LateUpdate()
    {
        float speed = body ? body.linearVelocity.x : 0;
        if (Mathf.Abs(speed) > .1f) facing = speed > 0 ? 1 : -1;
        sprite.flipX = facing < 0;
        if (heroAnimation)
        {
            sprite.color = Time.time < hurtUntil ? new Color(1, .4f, .35f, baseColor.a) : baseColor;
            return;
        }
        float stride = Mathf.Sin(Time.time * 15);
        bool moving = Mathf.Abs(speed) > .2f;
        float stretch = moving ? stride * .045f : Mathf.Sin(Time.time * 3) * .018f;
        Vector3 scale = new Vector3(1 - stretch, 1 + stretch, 1);
        float tilt = moving ? -stride * 5 : 0;
        Vector3 offset = new Vector3(0, moving ? Mathf.Abs(stride) * .035f : 0, 0);
        if (body && Mathf.Abs(body.linearVelocity.y) > 1)
            scale = new Vector3(.92f, 1.08f, 1);
        if (Time.time < attackUntil) { tilt = -facing * 22; offset.x = facing * .12f; }
        if (Time.time < dodgeUntil) { scale = new Vector3(1.2f, .75f, 1); tilt = -facing * 15; }
        Color color = Time.time < hurtUntil ? new Color(1, .4f, .35f, baseColor.a) : baseColor;
        if (deathAt >= 0)
        {
            float progress = Mathf.Clamp01((Time.time - deathAt) / .35f);
            scale *= 1 - progress * .6f;
            tilt = facing * progress * 80;
            color.a *= 1 - progress;
        }
        visual.localScale = Vector3.Scale(baseScale, scale);
        visual.localPosition = offset;
        visual.localRotation = Quaternion.Euler(0, 0, tilt);
        sprite.color = color;
    }
}

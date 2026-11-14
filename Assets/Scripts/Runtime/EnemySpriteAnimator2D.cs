using UnityEngine;

public class EnemySpriteAnimator2D : MonoBehaviour
{
    public Texture2D actionAtlas;
    public bool boss;
    Sprite[] frames;
    SpriteRenderer art;
    Rigidbody2D body;
    string action;
    float since;
    int facing = -1;
    void Awake()
    {
        art = GetComponentInChildren<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        frames = HeroSpriteAnimator2D.Slice(actionAtlas, 8, 4, boss ? .6f : 1.1f);
        art.sprite = frames[boss ? 16 : 0];
    }
    public void Play(string state, int direction = 0)
    {
        action = state; since = Time.time;
        if (direction != 0) facing = direction;
    }
    void LateUpdate()
    {
        float elapsed = Time.time - since;
        if (Mathf.Abs(body.linearVelocity.x) > .1f) facing = body.linearVelocity.x > 0 ? 1 : -1;
        art.flipX = facing > 0; // Source enemies face left.
        int frame;
        if (action == "death") frame = boss ? 27 + Mathf.Min(4, (int)(elapsed / .15f)) : 13 + Mathf.Min(2, (int)(elapsed / .2f));
        else if (action == "hurt" && elapsed < .2f) frame = boss ? 24 + Mathf.Min(2, (int)(elapsed / .07f)) : 11 + Mathf.Min(1, (int)(elapsed / .1f));
        else if (action == "attack" && elapsed < .6f) frame = boss ? 20 + Mathf.Min(3, (int)(elapsed / .15f)) : 8 + Mathf.Min(2, (int)(elapsed / .2f));
        else frame = boss ? 16 + (int)(Time.time * 6) % 4 : (int)(Time.time * 9) % 8;
        art.sprite = frames[frame];
    }
    void OnDestroy() { if (frames != null) foreach (var frame in frames) if (frame) Destroy(frame); }
}

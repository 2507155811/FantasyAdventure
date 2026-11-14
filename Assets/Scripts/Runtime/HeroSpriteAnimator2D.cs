using UnityEngine;

[DefaultExecutionOrder(10)]
public class HeroSpriteAnimator2D : MonoBehaviour
{
    public Texture2D actionAtlas;
    public Sprite[] sourceFrames;
    bool ownsFrames;
    Sprite[] frames;
    public int CurrentFrame {get;private set;}
    public Sprite Portrait{get{return frames!=null&&frames.Length>0?frames[0]:null;}}
    SpriteRenderer art;
    Rigidbody2D body;
    PlayerController2D player;
    string action;
    float actionStart, landedAt = -1;
    bool wasAirborne;
    public float ActionProgress { get { return player ? player.AttackProgress : 1; } }
    public bool IsAttacking { get { return (action == "attack" || action == "skill") && player && player.IsAttacking; } }

    void Awake()
    {
        art = GetComponentInChildren<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController2D>();
        ownsFrames=sourceFrames==null||sourceFrames.Length!=54;
        frames = ownsFrames?Slice(actionAtlas, 6, 9, .9f):sourceFrames;
        art.sprite = frames[0];
    }

    public void Play(string name) { action = name; actionStart = Time.time; }

    void LateUpdate()
    {
        float elapsed = Time.time - actionStart;
        int frame;
        if (action == "death") frame = 45 + Mathf.Min(2, (int)(elapsed / .2f));
        else if (action == "hurt" && elapsed < .24f) frame = 42 + Mathf.Min(2, (int)(elapsed / .08f));
        else if(player.IsClimbing)frame=48+(Mathf.Abs(body.linearVelocity.y)>.05f?(int)(Time.time*8)%6:0);
        else if (IsAttacking)
        {
            int family = player.equipment ? (int)player.equipment.Family : 0;
            frame = 18 + family * 6 + Mathf.Min(5, (int)(ActionProgress * 6));
        }
        else if (!player.IsGrounded)
        {
            float vertical = body.linearVelocity.y;
            frame = 12 + (vertical > 2 ? 2 : vertical > -.5f ? 3 : 4);
            wasAirborne = true;
        }
        else
        {
            if (wasAirborne) { landedAt = Time.time; wasAirborne = false; }
            frame = Time.time - landedAt < .1f ? 17 :
                Mathf.Abs(body.linearVelocity.x) > .2f ? 6 + (int)(Time.time * 12) % 6 : (int)(Time.time * 6) % 6;
        }
        CurrentFrame=frame;
        art.sprite = frames[frame];
    }

    public static Sprite[] Slice(Texture2D sheet, int columns, int rows, float occupiedHeight)
    {
        float width = sheet.width / (float)columns, height = sheet.height / (float)rows;
        var result = new Sprite[columns * rows];
        for (int i = 0; i < result.Length; i++)
        {
            var rect = new Rect(i % columns * width, (rows - 1 - i / columns) * height, width, height);
            result[i] = Sprite.Create(sheet, rect, new Vector2(.5f, .5f), height * occupiedHeight / 1.5f, 0, SpriteMeshType.FullRect);
        }
        return result;
    }

    void OnDestroy() { if (ownsFrames && frames != null) foreach (var frame in frames) if (frame) Destroy(frame); }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed=6,jumpForce=14.5f;
    public int maxHealth=100,maxMana=60,health=100,mana=60,redPotions=3,bluePotions=3;
    public WeaponEquipment equipment;
    public bool IsClimbing{get;private set;}
    public bool IsGrounded {get{return groundContacts.Count>0;}}
    public int Facing {get;private set;}=1;
    public float SkillRemaining {get{return Mathf.Max(0,skillTimer);}}
    public bool IsAttacking {get{return action.IsActive(Time.time);}}
    public float AttackProgress {get{return action.Progress(Time.time);}}
    readonly HashSet<int> groundContacts=new HashSet<int>();
    readonly CombatActionClock action=new CombatActionClock();
    Rigidbody2D body;
    Collider2D ownCollider,standingPlatform;
    Ladder2D nearbyLadder,activeLadder;
    readonly Dictionary<Collider2D,float> ignoredPlatforms=new Dictionary<Collider2D,float>();
    float verticalMovement,originalGravity,climbLockUntil;
    bool ladderHelpShown;
    float movement,skillTimer,invulnerableUntil,manaClock;
    float jumpBufferedUntil=-1,lastGrounded=-10,skillBufferedUntil=-1,blockedUntil;
    WeaponFamily attackFamily;
    int attackDirection,attackDamage;
    bool attackingWithSkill;
    Color attackColor;
    bool Blocked {get{return health<=0||ChapterOne.Instance&&(ChapterOne.Instance.MenuOpen||ChapterOne.Instance.Completed);}}

    void Awake()
    {
        body=GetComponent<Rigidbody2D>();body.freezeRotation=true;ownCollider=GetComponent<Collider2D>();originalGravity=body.gravityScale;
        equipment=GetComponent<WeaponEquipment>();health=maxHealth;mana=maxMana;
    }
    void Update()
    {
        if(Blocked){movement=verticalMovement=0;DemoInput.ClearOneShot();return;}
        skillTimer-=Time.deltaTime;
        manaClock+=Time.deltaTime;
        if(manaClock>=1){mana=Mathf.Min(maxMana,mana+3+CampaignInventory.Passive(2));manaClock-=1;}
        movement=DemoInput.Horizontal;
        if(Mathf.Abs(movement)<.01f)movement=Input.GetAxisRaw("Horizontal");
        verticalMovement=DemoInput.Vertical;
        if(Mathf.Abs(verticalMovement)<.01f)verticalMovement=Input.GetAxisRaw("Vertical");
        if(!IsAttacking&&Mathf.Abs(movement)>.01f)Facing=movement>0?1:-1;
        if(Input.GetKeyDown(KeyCode.Space)||DemoInput.JumpPressed)jumpBufferedUntil=Time.time+.12f;
        if(Input.GetKeyDown(KeyCode.K)||DemoInput.SkillPressed)skillBufferedUntil=Time.time+.25f;
        if(action.TryImpact(Time.time))ResolveAttack();
        if(Time.time>=blockedUntil&&!IsAttacking&&!IsClimbing&&!(nearbyLadder&&Mathf.Abs(verticalMovement)>.5f))
        {
            if(skillBufferedUntil>=Time.time&&skillTimer<=0&&mana>=15){BeginAttack(true);skillBufferedUntil=-1;}
            else if(Input.GetKey(KeyCode.J)||DemoInput.AttackHeld||DemoInput.AttackPressed)BeginAttack(false);
        }
        DemoInput.ClearOneShot();
    }
    void FixedUpdate()
    {
        RestorePlatforms();
        if(Blocked){body.linearVelocity=IsClimbing?Vector2.zero:new Vector2(0,body.linearVelocity.y);return;}
        if(IsGrounded)lastGrounded=Time.time;
        bool jump=jumpBufferedUntil>=Time.time;
        if(jump&&verticalMovement<-.5f&&IsGrounded&&standingPlatform&&standingPlatform.usedByEffector)
        {
            Pass(standingPlatform);groundContacts.Clear();standingPlatform=null;
            StopClimbing();climbLockUntil=Time.time+.25f;lastGrounded=-10;jumpBufferedUntil=-1;jump=false;
            body.linearVelocity=new Vector2(body.linearVelocity.x,-3);
        }
        if(!IsClimbing&&nearbyLadder&&Time.time>=climbLockUntil&&
            (verticalMovement>.5f&&body.position.y<nearbyLadder.topY+.60f||verticalMovement<-.5f&&body.position.y>nearbyLadder.bottomY+.7f))
        {
            activeLadder=nearbyLadder;IsClimbing=true;action.Cancel();body.gravityScale=0;
            Pass(activeLadder.landingPlatform);groundContacts.Clear();lastGrounded=-10;
        }
        if(IsClimbing&&activeLadder)
        {
            if(jump)
            {
                StopClimbing();climbLockUntil=Time.time+.25f;jumpBufferedUntil=-1;lastGrounded=-10;
                GameAudio.Play(GameSound.Jump);body.linearVelocity=new Vector2(movement*moveSpeed,jumpForce*.85f);return;
            }
            if(Mathf.Abs(movement)>.2f&&Mathf.Abs(verticalMovement)<.2f)StopClimbing();
            else
            {
                float y=body.position.y;
                if(verticalMovement>0&&y>=activeLadder.topY+.64f)
                {
                    body.position=new Vector2(activeLadder.transform.position.x,activeLadder.topY+.67f);
                    StopClimbing();body.linearVelocity=Vector2.zero;return;
                }
                if(verticalMovement<0&&y<=activeLadder.bottomY+.66f)
                {
                    body.position=new Vector2(activeLadder.transform.position.x,activeLadder.bottomY+.65f);
                    StopClimbing();body.linearVelocity=Vector2.zero;return;
                }
                body.linearVelocity=new Vector2(Mathf.Clamp((activeLadder.transform.position.x-body.position.x)*12,-4,4),verticalMovement*3);
                return;
            }
        }
        float factor=IsAttacking?(attackFamily==WeaponFamily.Hammer?.2f:.45f):1;
        float target=movement*moveSpeed*factor;
        float acceleration=Mathf.Abs(movement)<.01f?100:IsGrounded?75:45;
        body.linearVelocity=new Vector2(Mathf.MoveTowards(body.linearVelocity.x,target,acceleration*Time.fixedDeltaTime),body.linearVelocity.y);
        if(jump&&Time.time-lastGrounded<=.1f)
        {
            GameAudio.Play(GameSound.Jump);
            body.linearVelocity=new Vector2(body.linearVelocity.x,jumpForce);
            groundContacts.Clear();lastGrounded=-10;jumpBufferedUntil=-1;
        }
    }
    void StopClimbing()
    {
        if(activeLadder&&activeLadder.landingPlatform&&ignoredPlatforms.ContainsKey(activeLadder.landingPlatform))ignoredPlatforms[activeLadder.landingPlatform]=Time.time;
        IsClimbing=false;activeLadder=null;body.gravityScale=originalGravity;
    }
    void Pass(Collider2D platform)
    {
        if(!platform)return;
        Physics2D.IgnoreCollision(ownCollider,platform,true);ignoredPlatforms[platform]=Time.time+.2f;
    }
    void RestorePlatforms()
    {
        var clear=new List<Collider2D>();
        foreach(var pair in ignoredPlatforms)
        {
            if(!pair.Key){clear.Add(pair.Key);continue;}
            if(IsClimbing&&activeLadder&&pair.Key==activeLadder.landingPlatform)continue;
            if(Time.time<pair.Value||pair.Key.bounds.Intersects(ownCollider.bounds))continue;
            Physics2D.IgnoreCollision(ownCollider,pair.Key,false);clear.Add(pair.Key);
        }
        foreach(var item in clear)ignoredPlatforms.Remove(item);
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        var ladder=collider.GetComponent<Ladder2D>();if(!ladder)return;
        nearbyLadder=ladder;
        if(!ladderHelpShown){ladderHelpShown=true;DemoHUD.ShowMessage("上下爬梯；向下 + 跳跃可以落下高台");}
    }
    void OnTriggerExit2D(Collider2D collider){if(nearbyLadder&&collider.GetComponent<Ladder2D>()==nearbyLadder)nearbyLadder=null;}
    void BeginAttack(bool skill)
    {
        if(!equipment)return;
        attackingWithSkill=skill;attackFamily=equipment.Family;attackDirection=Facing;
        attackDamage=equipment.Damage;attackColor=equipment.EffectColor;
        if(skill)
        {
            int rank=CampaignInventory.SkillRank((int)equipment.Family);
            mana-=15;skillTimer=Mathf.Max(2.5f,4-(rank-1)*.15f);
            attackDamage=Mathf.RoundToInt(attackDamage*(1+(rank-1)*.1f));
            if(ChapterOne.Instance)CampaignQuestLog.Event(QuestGoal.Skill,ChapterOne.Instance.MapId);
        }
        float duration=skill?Mathf.Max(.45f,equipment.AttackInterval):equipment.AttackInterval;
        float impact=attackFamily==WeaponFamily.Hammer?.58f:attackFamily==WeaponFamily.Spear?.4f:.42f;
        action.Start(Time.time,duration,impact);
        GetComponent<ActorVisual2D>().Attack(Facing,skill);
    }
    void ResolveAttack()
    {
        GameAudio.Play(attackingWithSkill?GameSound.Magic:attackFamily==WeaponFamily.Hammer?GameSound.Hammer:attackFamily==WeaponFamily.Spear?GameSound.Spear:attackFamily==WeaponFamily.Staff?GameSound.Magic:GameSound.Sword);
        Vector3 origin=transform.position;
        if(attackingWithSkill)
        {
            if(attackFamily==WeaponFamily.Sword)
            {
                MagicBolt2D.Fire(origin+Vector3.right*attackDirection*.6f,attackDirection,attackDamage*3,attackColor);
                CombatEffect2D.Spawn(origin,attackDirection,false,attackColor);
            }
            else if(attackFamily==WeaponFamily.Hammer)
            {
                HitBox(origin,new Vector2(4.2f,1.5f),attackDamage*2);
                CombatEffect2D.Spawn(origin,attackDirection,true,attackColor);
            }
            else if(attackFamily==WeaponFamily.Spear)
            {
                MagicBolt2D.Fire(origin+Vector3.right*attackDirection*.7f,attackDirection,attackDamage*2,attackColor,true);
                CombatEffect2D.Spawn(origin+Vector3.right*attackDirection,attackDirection,false,attackColor);
            }
            else
            {
                Vector3 center=origin+Vector3.right*attackDirection*3;
                Damage(Physics2D.OverlapCircleAll(center,2),attackDamage*2);
                CombatEffect2D.Spawn(center,attackDirection,true,attackColor);
            }
            return;
        }
        if(attackFamily==WeaponFamily.Staff)
            MagicBolt2D.Fire(origin+Vector3.right*attackDirection*.6f,attackDirection,attackDamage,attackColor);
        else
        {
            float forward=attackFamily==WeaponFamily.Spear?1.25f:attackFamily==WeaponFamily.Hammer?1:.9f;
            Vector2 size=attackFamily==WeaponFamily.Spear?new Vector2(1.95f,.85f):attackFamily==WeaponFamily.Hammer?new Vector2(1.8f,1.5f):new Vector2(1.45f,1.4f);
            HitBox(origin+Vector3.right*attackDirection*forward,size,attackDamage);
            CombatEffect2D.Spawn(origin,attackDirection,false,attackColor);
        }
    }
    void HitBox(Vector2 center,Vector2 size,int damage){Damage(Physics2D.OverlapBoxAll(center,size,0),damage);}
    void Damage(Collider2D[] hits,int damage)
    {
        var seen=new HashSet<EnemyAI2D>();
        foreach(var collider in hits)
        {
            var enemy=collider.GetComponent<EnemyAI2D>();
            if(enemy&&seen.Add(enemy))enemy.TakeDamage(damage,attackDirection);
        }
    }
    public void CancelAttack(){action.Cancel();skillBufferedUntil=-1;GetComponent<HeroSpriteAnimator2D>().Play("idle");}
    public void RecoverAt(Vector3 position)
    {
        StopClimbing();nearbyLadder=null;standingPlatform=null;
        foreach(var pair in ignoredPlatforms)if(pair.Key)Physics2D.IgnoreCollision(ownCollider,pair.Key,false);
        ignoredPlatforms.Clear();
        transform.position=position;body.linearVelocity=Vector2.zero;groundContacts.Clear();
        health=maxHealth;mana=maxMana;skillTimer=0;invulnerableUntil=Time.time+1;
        jumpBufferedUntil=-1;lastGrounded=-10;movement=0;blockedUntil=0;CancelAttack();
    }
    public void UseRedPotion(){if(health>0&&redPotions>0&&health<maxHealth){redPotions--;health=Mathf.Min(maxHealth,health+Mathf.RoundToInt(maxHealth*.45f));}}
    public void UseBluePotion(){if(health>0&&bluePotions>0&&mana<maxMana){bluePotions--;mana=Mathf.Min(maxMana,mana+Mathf.RoundToInt(maxMana*.5f));}}
    public void TakeDamage(int amount)
    {
        if(health<=0||amount<=0||Time.time<invulnerableUntil||ChapterOne.Instance&&ChapterOne.Instance.Completed)return;
        DamagePopup.Show(transform.position+Vector3.up*.8f,Mathf.Max(1,amount-GameSave.GetInt("relic",0)),true);
        GameAudio.Play(GameSound.Hurt);
        invulnerableUntil=Time.time+.55f;health=Mathf.Max(0,health-Mathf.Max(1,amount-GameSave.GetInt("relic",0)));action.Cancel();StopClimbing();blockedUntil=Time.time+.18f;
        if(health==0){GetComponent<HeroSpriteAnimator2D>().Play("death");DemoHUD.ShowMessage("点击返回检查点继续冒险");}
        else{GetComponent<ActorVisual2D>().Hurt();if(health<=maxHealth*.35f)UseRedPotion();}
    }
    void Ground(Collision2D collision)
    {
        int id=collision.collider.GetInstanceID();
        if(!collision.collider.GetComponent<GroundPlatform2D>())return;
        for(int i=0;i<collision.contactCount;i++)if(collision.GetContact(i).normal.y>.5f){groundContacts.Add(id);standingPlatform=collision.collider;return;}
        groundContacts.Remove(id);
    }
    void OnCollisionEnter2D(Collision2D collision){Ground(collision);}
    void OnCollisionStay2D(Collision2D collision){Ground(collision);}
    void OnCollisionExit2D(Collision2D collision){groundContacts.Remove(collision.collider.GetInstanceID());if(standingPlatform==collision.collider)standingPlatform=null;}
}

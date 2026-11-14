using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour {
 public float moveSpeed=6f,jumpForce=12f,dodgeSpeed=12f,dodgeDuration=.18f,attackRange=1.25f,attackCooldown=.32f;
 public int attackDamage=20,maxHealth=100,maxMana=60,health=100,mana=60,redPotions=3,bluePotions=3;
 Rigidbody2D body; bool grounded; float attackTimer,dodgeTimer; int facing=1;
 void Awake(){body=GetComponent<Rigidbody2D>();body.freezeRotation=true;health=maxHealth;mana=maxMana;}
 void Update(){
  attackTimer-=Time.deltaTime;dodgeTimer-=Time.deltaTime;
  float move=DemoInput.Horizontal;if(Mathf.Abs(move)<.01f)move=Input.GetAxisRaw("Horizontal");
  if(Mathf.Abs(move)>.01f)facing=move>0?1:-1;
  body.velocity=new Vector2(dodgeTimer>0?facing*dodgeSpeed:move*moveSpeed,body.velocity.y);
  if((Input.GetKeyDown(KeyCode.Space)||DemoInput.JumpPressed)&&grounded){body.velocity=new Vector2(body.velocity.x,jumpForce);grounded=false;}
  if(Input.GetKeyDown(KeyCode.LeftShift)||DemoInput.DodgePressed)dodgeTimer=dodgeDuration;
  if(Input.GetKeyDown(KeyCode.J)||DemoInput.AttackPressed)Attack();
  if(Input.GetKeyDown(KeyCode.K)||DemoInput.SkillPressed)Skill();
  if(Input.GetKeyDown(KeyCode.H)||DemoInput.RedPotionPressed)UseRedPotion();
  if(Input.GetKeyDown(KeyCode.M)||DemoInput.BluePotionPressed)UseBluePotion();
  DemoInput.ClearOneShot();
 }
 void Attack(){if(attackTimer>0)return;attackTimer=attackCooldown;Hit(attackRange*.55f,attackRange*.65f,attackDamage);}
 void Skill(){if(mana<15)return;mana-=15;Hit(2.1f,1.1f,45);}
 void Hit(float radius,float forward,int damage){var hits=Physics2D.OverlapCircleAll((Vector2)transform.position+Vector2.right*facing*forward,radius);foreach(var h in hits){var e=h.GetComponent<EnemyAI2D>();if(e)e.TakeDamage(damage,facing);}}
 public void UseRedPotion(){if(redPotions>0&&health<maxHealth){redPotions--;health=Mathf.Min(maxHealth,health+45);}}
 public void UseBluePotion(){if(bluePotions>0&&mana<maxMana){bluePotions--;mana=Mathf.Min(maxMana,mana+30);}}
 public void TakeDamage(int n){health=Mathf.Max(0,health-n);if(health==0)DemoHUD.ShowMessage("冒险暂时结束！");}
 bool IsGround(Collision2D c){return c.collider.GetComponent<GroundPlatform2D>()!=null;}
 void OnCollisionEnter2D(Collision2D c){if(IsGround(c))grounded=true;}
 void OnCollisionStay2D(Collision2D c){if(IsGround(c))grounded=true;}
 void OnCollisionExit2D(Collision2D c){if(IsGround(c))grounded=false;}
}
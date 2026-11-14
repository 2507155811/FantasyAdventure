using UnityEngine;
[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D))]
public class EnemyAI2D:MonoBehaviour{
 public int maxHealth=55,contactDamage=8,coinReward=5;public float moveSpeed=2,detectRange=7,attackRange=1.15f,attackInterval=1.2f;public bool isBoss;
 int hp,dir=-1;float timer;Rigidbody2D body;Transform target;bool dead;
 void Awake(){body=GetComponent<Rigidbody2D>();hp=maxHealth;}
 void Start(){var p=FindObjectOfType<PlayerController2D>();if(p)target=p.transform;}
 void Update(){if(dead||!target)return;timer-=Time.deltaTime;float d=Vector2.Distance(transform.position,target.position);
 if(d<=detectRange){dir=target.position.x>=transform.position.x?1:-1;if(d>attackRange)body.velocity=new Vector2(dir*moveSpeed,body.velocity.y);else{body.velocity=new Vector2(0,body.velocity.y);if(timer<=0){var p=target.GetComponent<PlayerController2D>();if(p)p.TakeDamage(contactDamage);timer=attackInterval;}}}
 else body.velocity=new Vector2(dir*moveSpeed*.35f,body.velocity.y);}
 public void TakeDamage(int amount,int knockDir){if(dead)return;hp-=amount;body.velocity=new Vector2(knockDir*4,2);if(hp<=0)Die();}
 void Die(){dead=true;body.velocity=Vector2.zero;DemoHUD.AddCoins(coinReward);DemoHUD.ShowMessage(isBoss?"森林守卫已击败！第一关通关！":"获得金币 +"+coinReward);Destroy(gameObject,.15f);}
}
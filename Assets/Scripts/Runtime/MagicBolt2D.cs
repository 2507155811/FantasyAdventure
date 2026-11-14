using UnityEngine;

public class MagicBolt2D : MonoBehaviour
{
    Vector3 velocity;
    int damage, direction;
    LineRenderer trail;
    Material material;
    float remaining = 1.1f;
    public static void Fire(Vector3 origin, int direction, int damage, Color color)
    {
        var go = new GameObject("Magic bolt");
        go.transform.position = origin;
        var bolt = go.AddComponent<MagicBolt2D>();
        bolt.direction = direction; bolt.damage = damage;
        bolt.velocity = Vector3.right * direction * 10;
        bolt.trail = go.AddComponent<LineRenderer>();
        bolt.material = new Material(Shader.Find("Sprites/Default"));
        bolt.trail.sharedMaterial = bolt.material;
        bolt.trail.positionCount = 2; bolt.trail.startWidth = .12f; bolt.trail.endWidth = .02f;
        bolt.trail.startColor = color; bolt.trail.endColor = new Color(color.r,color.g,color.b,0);
        bolt.trail.sortingOrder = 6;
        bolt.trail.SetPosition(0, origin); bolt.trail.SetPosition(1, origin - Vector3.right * direction * .4f);
    }
    void OnDestroy() { if (material) Destroy(material); }
    void Update()
    {
        if (ChapterOne.Instance && ChapterOne.Instance.MenuOpen) return;
        Vector3 travel = velocity * Time.deltaTime;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, .2f, travel.normalized, travel.magnitude);
        foreach (var hit in hits)
        {
            var enemy = hit.collider.GetComponent<EnemyAI2D>();
            if (!enemy || enemy.IsDead) continue;
            enemy.TakeDamage(damage, direction);
            CombatEffect2D.Spawn(hit.point, direction, false);
            Destroy(gameObject); return;
        }
        transform.position += travel;
        trail.SetPosition(0, transform.position);
        trail.SetPosition(1, transform.position - Vector3.right * direction * .5f);
        remaining -= Time.deltaTime;
        if (remaining <= 0) Destroy(gameObject);
    }
}

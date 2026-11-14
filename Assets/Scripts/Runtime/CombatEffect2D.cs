using UnityEngine;

public class CombatEffect2D : MonoBehaviour
{
    LineRenderer line;
    Material material;
    Color tint;
    float age;
    bool ring;
    int facing;

    public static void Spawn(Vector3 position, int direction, bool skill, Color? color = null)
    {
        var go = new GameObject(skill ? "Skill wave" : "Sword arc");
        go.transform.position = position;
        var effect = go.AddComponent<CombatEffect2D>();
        effect.ring = skill;
        effect.facing = direction;
        effect.tint = color ?? (skill ? new Color(.25f, .85f, 1) : new Color(1, .86f, .38f));
        effect.line = go.AddComponent<LineRenderer>();
        effect.material = new Material(Shader.Find("Sprites/Default"));
        effect.line.sharedMaterial = effect.material;
        effect.line.useWorldSpace = false;
        effect.line.positionCount = 33;
        effect.line.sortingOrder = 5;
        effect.line.numCapVertices = 3;
        effect.Draw(0);
    }

    void Update()
    {
        age += Time.deltaTime;
        float progress = age / .3f;
        if (progress >= 1) { Destroy(gameObject); return; }
        Draw(progress);
    }

    void Draw(float progress)
    {
        float radius = ring ? Mathf.Lerp(.3f, 2.1f, progress) : Mathf.Lerp(.7f, 1.2f, progress);
        for (int i = 0; i < 33; i++)
        {
            float angle = ring ? i / 32f * Mathf.PI * 2 : Mathf.Lerp(-1.1f, 1.1f, i / 32f);
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius * facing, Mathf.Sin(angle) * radius, 0));
        }
        line.startWidth = line.endWidth = (ring ? .09f : .16f) * (1 - progress);
        Color color = tint;
        color.a = 1 - progress;
        line.startColor = line.endColor = color;
    }

    void OnDestroy() { if (material) Destroy(material); }
}

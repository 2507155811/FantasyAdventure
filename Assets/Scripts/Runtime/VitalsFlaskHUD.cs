using UnityEngine;

// Original procedural glass artwork. Cached liquid levels avoid per-frame allocations.
public sealed class VitalsFlaskHUD
{
    const int Steps = 32;
    readonly Texture2D[,] textures = new Texture2D[2, Steps + 1];
    GUIStyle caption, value;

    public void Draw(PlayerController2D player, Rect area, Font font = null)
    {
        if (caption == null)
        {
            caption = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                fontSize = 13, fontStyle = FontStyle.Bold };
            value = new GUIStyle(caption) { fontSize = 16 };
            if(font){caption.font=font;value.font=font;}
        }
        DrawFlask(new Rect(area.x, area.y, area.width * .5f, area.height),
            0, player.health, player.maxHealth, "生命 HP");
        DrawFlask(new Rect(area.x + area.width * .5f, area.y, area.width * .5f, area.height),
            1, player.mana, player.maxMana, "魔法 MP");
    }

    void DrawFlask(Rect rect, int kind, int current, int maximum, string name)
    {
        float ratio = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0;
        int level = Mathf.RoundToInt(ratio * Steps);
        if (!textures[kind, level]) textures[kind, level] = MakeTexture(kind, level);
        GUI.DrawTexture(rect, textures[kind, level], ScaleMode.StretchToFill, true);
        ShadowLabel(new Rect(rect.x, rect.y + rect.height * .46f, rect.width, 24),
            Mathf.Max(0, current).ToString(), value);
        ShadowLabel(new Rect(rect.x, rect.y + rect.height * .64f, rect.width, 21),
            "/ " + maximum, caption);
        ShadowLabel(new Rect(rect.x, rect.y + rect.height - 15, rect.width, 22), name, caption);
    }

    static void ShadowLabel(Rect rect, string text, GUIStyle style)
    {
        Color previous = GUI.color;
        GUI.color = new Color(.12f, .07f, .04f, 1);
        GUI.Label(new Rect(rect.x + 1, rect.y + 2, rect.width, rect.height), text, style);
        GUI.color = new Color(1, .95f, .78f, 1);
        GUI.Label(rect, text, style);
        GUI.color = previous;
    }

    static Texture2D MakeTexture(int kind, int level)
    {
        const int width = 80, height = 96;
        var pixels = new Color[width * height];
        Color liquid = kind == 0 ? new Color(.95f, .15f, .045f) : new Color(.03f, .58f, .96f);
        float surface = 9 + 59f * level / Steps;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float dx = x - 39.5f, dy = y - 38;
            float radius = Mathf.Sqrt(dx * dx + dy * dy);
            bool neck = x >= 28 && x <= 51 && y >= 62 && y <= 87;
            bool rim = x >= 23 && x <= 56 && y >= 80 && y <= 90;
            if (radius > 36 && !neck && !rim) continue;
            Color color = new Color(.18f, .13f, .09f);
            bool inner = radius < 31 || (x >= 33 && x <= 46 && y >= 62 && y <= 84);
            if (radius >= 33 && radius <= 34 || rim && y >= 83 && y <= 87)
                color = new Color(.88f, .79f, .52f);
            if (inner)
            {
                color = new Color(.17f, .24f, .24f);
                if (level > 0 && y <= surface)
                {
                    float light = Mathf.Clamp01((x + y) / 110f);
                    color = Color.Lerp(liquid * .5f, liquid, light);
                    color.a = 1;
                    if (Mathf.Abs(y - surface) < 2) color = Color.Lerp(liquid, Color.white, .4f);
                }
                // A glass reflection stays visible even when the bottle is empty.
                float glint = ((x - 20) * (x - 20)) / 12f + ((y - 53) * (y - 53)) / 100f;
                if (glint < 1) color = Color.Lerp(color, Color.white, .6f);
            }
            pixels[y * width + x] = color;
        }
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "Flask_" + kind + "_" + level;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    public void Dispose()
    {
        foreach (Texture2D texture in textures)
            if (texture) Object.Destroy(texture);
    }
}

using UnityEngine;

// Yasin: sprites my scripts need, loaded at runtime so nobody has to drag them into the prefab.
// Pixel art lives in Assets/Resources/Art (copies of Art/Bellrum/pizza_box.png and car.png).
// If a file is missing it falls back to a plain colored box instead of crashing.
public static class BellrumArt
{
    const float PixelsPerUnit = 16f;   // same as the art bible / Shubh's street

    static Sprite pizzaBox, car, white, puddle;

    public static Sprite PizzaBox
    {
        get { if (pizzaBox == null) pizzaBox = Load("pizza_box", new Color(0.86f, 0.62f, 0.36f), 16, 5); return pizzaBox; }
    }

    public static Sprite Car
    {
        get { if (car == null) car = Load("car", new Color(0.3f, 0.45f, 0.85f), 32, 18); return car; }
    }

    // 1x1 white square, 1 unit big. Scale it and tint it for bars and meters.
    public static Sprite White
    {
        get
        {
            if (white == null)
            {
                Texture2D t = Solid(1, 1, Color.white);
                white = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return white;
        }
    }

    // pixel-art puddle drawn in code (2 x 0.75 units)
    public static Sprite Puddle
    {
        get
        {
            if (puddle == null)
            {
                const int w = 32, h = 12;
                Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
                t.filterMode = FilterMode.Point;
                Color clear = new Color(0, 0, 0, 0);
                Color water = new Color(0.33f, 0.47f, 0.72f, 0.85f);
                Color edge = new Color(0.22f, 0.33f, 0.55f, 0.9f);
                Color shine = new Color(0.75f, 0.85f, 1f, 0.9f);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        // ellipse test
                        float dx = (x + 0.5f - w / 2f) / (w / 2f);
                        float dy = (y + 0.5f - h / 2f) / (h / 2f);
                        float d = dx * dx + dy * dy;
                        Color c = d > 1f ? clear : d > 0.7f ? edge : water;
                        if (d <= 0.7f && y == h - 4 && x > w / 2 - 6 && x < w / 2 + 2) c = shine;   // little highlight
                        t.SetPixel(x, y, c);
                    }
                t.Apply();
                puddle = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            }
            return puddle;
        }
    }

    static Sprite Load(string name, Color fallback, int w, int h)
    {
        Texture2D tex = Resources.Load<Texture2D>("Art/" + name);
        if (tex == null)
        {
            Debug.LogWarning("BellrumArt: no texture at Resources/Art/" + name + ", using a plain box");
            tex = Solid(w, h, fallback);
        }
        tex.filterMode = FilterMode.Point;   // crisp pixels
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    static Texture2D Solid(int w, int h, Color c)
    {
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    // world-space text (for "Splash!", "Wind!" and the steady hint)
    public static TextMesh MakeText(Transform parent, Vector3 localPos, string text, Color color, int sortingOrder)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        TextMesh tm = go.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        tm.fontSize = 48;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        tm.text = text;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = sortingOrder;
        return tm;
    }
}

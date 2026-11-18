using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// little helper so each screen can build its UI in code (no prefabs to wire up)
public static class UIHelper
{
    static Font font;

    static Font GetFont()
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font;
    }

    // canvas that scales with the screen + an event system so buttons work
    public static Canvas MakeCanvas(Color background)
    {
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        // full screen background color
        if (background.a > 0)
        {
            Image bg = new GameObject("Background", typeof(Image)).GetComponent<Image>();
            bg.transform.SetParent(go.transform, false);
            bg.color = background;
            RectTransform r = bg.rectTransform;
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        return canvas;
    }

    // text placed by its center (0,0 = middle of the screen)
    public static Text MakeText(Canvas canvas, string text, Vector2 pos, int size, Color color)
    {
        Text t = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        t.transform.SetParent(canvas.transform, false);
        t.font = GetFont();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.rectTransform.anchoredPosition = pos;
        t.rectTransform.sizeDelta = new Vector2(900, size * 2);
        return t;
    }

    // yellow button like the prototype
    public static Button MakeButton(Canvas canvas, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + " Button", typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);
        go.GetComponent<Image>().color = new Color(1f, 0.86f, 0.3f);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(320, 60);

        Text t = MakeText(canvas, label, Vector2.zero, 26, Color.black);
        t.transform.SetParent(go.transform, false);
        t.rectTransform.anchoredPosition = Vector2.zero;

        Button b = go.GetComponent<Button>();
        b.onClick.AddListener(onClick);
        return b;
    }
}

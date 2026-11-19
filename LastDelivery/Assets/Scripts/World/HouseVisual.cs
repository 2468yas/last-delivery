using System.Collections.Generic;
using UnityEngine;

// Put this on each house. It shows the vivid / normal / faded version
// from the saved memory score, and updates the moment a delivery changes it.
[RequireComponent(typeof(SpriteRenderer))]
public class HouseVisual : MonoBehaviour
{
    [Header("Save key (must be unique per house, and match StoryData)")]
    public string houseId = "june_14_elm";

    [Header("Shown in game")]
    public string displayName = "14 Elm St";     // address label above the house
    public string residentName = "June";         // who signs the note

    [Header("Legacy (old test street, not used by ShiftManager)")]
    public string goodMemory = "";
    public string badMemory = "";

    [Header("Art (leave empty to use colors on a placeholder square)")]
    public Sprite vividSprite;
    public Sprite normalSprite;
    public Sprite fadedSprite;
    public Color baseColor = new Color(0.18f, 0.82f, 0.78f);

    [Header("Where the player stands to deliver (relative to the house)")]
    public Vector2 doorOffset = new Vector2(0f, -1.6f);

    // every house in the open scene (so other scripts don't need to search for them)
    public static readonly List<HouseVisual> All = new List<HouseVisual>();

    SpriteRenderer sr;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public Vector2 DoorPosition => (Vector2)transform.position + doorOffset;

    // top of the house, for the address label
    public Vector3 LabelPosition
    {
        get
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            return new Vector3(transform.position.x, sr.bounds.max.y + 0.35f, 0f);
        }
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        GameState gs = GameState.Get();
        gs.HouseChanged += OnHouseChanged;
        gs.Changed += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (GameState.Instance == null) return;
        GameState.Instance.HouseChanged -= OnHouseChanged;
        GameState.Instance.Changed -= Refresh;
    }

    void OnHouseChanged(string id)
    {
        if (id == houseId) Refresh();
    }

    public void Refresh()
    {
        if (sr == null) return;
        MemoryState state = GameState.Get().GetHouseState(houseId);

        Sprite art = state == MemoryState.Vivid ? vividSprite : state == MemoryState.Faded ? fadedSprite : normalSprite;
        if (art != null)
        {
            sr.sprite = art;
            sr.color = Color.white;
            return;
        }

        // placeholder look until the pixel art exists
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        switch (state)
        {
            case MemoryState.Vivid:  sr.color = Color.HSVToRGB(h, Mathf.Min(1f, s * 1.3f), 1f); break;
            case MemoryState.Faded:  sr.color = Color.Lerp(baseColor, new Color(0.55f, 0.55f, 0.6f), 0.8f); break;
            default:                 sr.color = Color.HSVToRGB(h, s * 0.8f, v * 0.85f); break;
        }
    }

    // shows the delivery spot in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(DoorPosition, 0.4f);
    }
}

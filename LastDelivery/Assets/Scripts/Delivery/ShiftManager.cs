using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Runs one shift: intro card -> pick up at Corner Slice -> find the house from the clue
// -> deliver -> read the note -> repeat -> clock out -> Yasin's summary screen (which saves).
//
// Controls: move with Yasin's player controls, SPACE / ENTER / E or controller A to interact.
// Debug: C or controller Y crushes the pizza (until Yasin's balancing calls PizzaStack.Crush()).
//        O opens the save folder.
public class ShiftManager : MonoBehaviour
{
    enum Phase { Intro, NeedPickup, Delivering, ReadingNote, ClockOut, Leaving }

    public float interactRange = 1.4f;
    public float hintAfterSeconds = 40f;     // the right house's label lights up after this long
    public float realSecondsPerGameMinute = 1.5f;

    Phase phase;
    DayInfo day;
    int orderIndex;
    OrderInfo order;
    float orderTimer;
    float gameMinutes;
    int intactCount;
    string message = "";
    float messageTimer;
    float noteTimer;

    HouseVisual[] houses;
    readonly Dictionary<HouseVisual, Text> labels = new Dictionary<HouseVisual, Text>();
    CornerSlice shop;
    Transform player;
    Rigidbody2D playerBody;
    Behaviour playerMovement;
    Camera cam;

    Canvas canvas;
    RectTransform canvasRect;
    Text hud, orderTitle, orderClue, prompt, noteTitle, noteBody, noteHint, introBody, introHint;
    GameObject orderCard, notePanel, introPanel;

    void Start()
    {
        GameState gs = GameState.Get();
        day = StoryData.GetDay(gs.data.currentDay);
        PizzaStack.Clear();

        cam = Camera.main;
        houses = HouseVisual.All.ToArray();
        shop = CornerSlice.Instance;
        PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
        if (pm != null)
        {
            player = pm.transform;
            playerMovement = pm;
            playerBody = pm.GetComponent<Rigidbody2D>();
        }

        if (cam != null) cam.backgroundColor = day.grass;
        BuildUI();
        AudioManager.PlayMusic(day.music);
        ShowIntro();
    }

    void Update()
    {
        if (GameInput.OpenSaveFolderPressed()) SaveManager.OpenSaveFolder();
        if (phase != Phase.Intro) gameMinutes += Time.deltaTime / realSecondsPerGameMinute;
        if (messageTimer > 0) messageTimer -= Time.deltaTime;

        bool interact = GameInput.InteractPressed();

        switch (phase)
        {
            case Phase.Intro:
                if (interact) StartPlaying();
                break;

            case Phase.NeedPickup:
                if (NearShop())
                {
                    SetPrompt("Press " + GameInput.InteractName + " to pick up the next order");
                    if (interact) StartOrder();
                }
                else SetPrompt("Head to Corner Slice to pick up the next order");
                break;

            case Phase.Delivering:
                orderTimer += Time.deltaTime;
                if (GameInput.DebugCrushPressed()) PizzaStack.Crush();
                HouseVisual near = NearestHouse();
                if (near != null)
                {
                    SetPrompt(near.displayName + "  -  press " + GameInput.InteractName + " to deliver");
                    if (interact) TryDeliver(near);
                }
                else SetPrompt(PizzaStack.Crushed ? "The pizza is crushed... deliver it anyway." : "");
                break;

            case Phase.ReadingNote:
                noteTimer += Time.deltaTime;
                if (noteTimer > 0.4f && interact) CloseNote();
                break;

            case Phase.ClockOut:
                if (NearShop())
                {
                    SetPrompt("Press " + GameInput.InteractName + " to clock out");
                    if (interact) EndShift();
                }
                else SetPrompt("That's every order. Head back to Corner Slice to clock out.");
                break;
        }

        UpdateHud();
    }

    void LateUpdate()
    {
        UpdateLabels();
    }

    // ---------------- flow ----------------

    void ShowIntro()
    {
        phase = Phase.Intro;
        SetPlayerControl(false);
        introPanel.SetActive(true);
        introBody.text = string.Join("\n\n", day.intro);
        introHint.text = "Press " + GameInput.InteractName + " to start your shift";
    }

    void StartPlaying()
    {
        introPanel.SetActive(false);
        SetPlayerControl(true);
        AudioManager.Play("start");
        phase = Phase.NeedPickup;
    }

    void StartOrder()
    {
        order = day.orders[orderIndex];
        orderTimer = 0f;
        PizzaStack.PickUp();
        AudioManager.Play("pickup");
        orderCard.SetActive(true);
        orderTitle.text = "ORDER " + (orderIndex + 1) + "/" + day.orders.Count + ":  " + order.food.ToUpper();
        orderClue.text = "Deliver to the house with " + order.clue + ".";
        phase = Phase.Delivering;
    }

    void TryDeliver(HouseVisual house)
    {
        if (house.houseId != order.houseId)
        {
            AudioManager.Play("wrong");
            Flash("Hmm, this isn't it. Look for " + order.clue + ".");
            return;
        }

        bool intact = !PizzaStack.Crushed;
        GameState gs = GameState.Get();
        gs.RecordDelivery(house.houseId, intact, "");   // the shift's fragment is given at clock out
        PizzaStack.Deliver();
        if (intact) intactCount++;
        orderIndex++;
        orderCard.SetActive(false);

        StartCoroutine(DeliverySounds(intact));
        ShowNote(house, intact);
    }

    IEnumerator DeliverySounds(bool intact)
    {
        AudioManager.Play("knock");
        yield return new WaitForSeconds(0.45f);
        AudioManager.Play(intact ? "deliver_good" : "deliver_bad");
        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("note");
    }

    void ShowNote(HouseVisual house, bool intact)
    {
        phase = Phase.ReadingNote;
        noteTimer = 0f;
        SetPlayerControl(false);
        SetPrompt("");
        notePanel.SetActive(true);
        noteTitle.text = "NOTE FROM " + house.residentName.ToUpper() + ":";
        noteBody.text = StoryData.Note(house.houseId, GameState.Get().data.currentDay, intact);
        noteHint.text = (intact ? "The memory gets stronger." : "The memory starts to fade...") +
                        "      " + GameInput.InteractName + " to continue";
        noteHint.color = intact ? new Color(1f, 0.78f, 0.35f) : new Color(0.66f, 0.74f, 1f);
    }

    void CloseNote()
    {
        notePanel.SetActive(false);
        SetPlayerControl(true);
        phase = orderIndex < day.orders.Count ? Phase.NeedPickup : Phase.ClockOut;
    }

    void EndShift()
    {
        phase = Phase.Leaving;
        GameState gs = GameState.Get();
        bool goodShift = intactCount * 2 >= day.orders.Count;
        gs.AddMemoryFragment(goodShift ? day.goodMemory : day.badMemory);
        AudioManager.Play("clockout");
        SceneManager.LoadScene(SceneNames.Summary);
    }

    // ---------------- helpers ----------------

    bool NearShop()
    {
        return shop != null && player != null && Vector2.Distance(player.position, shop.DoorPosition) < interactRange + 0.4f;
    }

    HouseVisual NearestHouse()
    {
        if (player == null) return null;
        HouseVisual best = null;
        float bestDist = interactRange;
        foreach (HouseVisual h in houses)
        {
            float d = Vector2.Distance(player.position, h.DoorPosition);
            if (d < bestDist) { bestDist = d; best = h; }
        }
        return best;
    }

    void SetPlayerControl(bool on)
    {
        if (playerMovement != null) playerMovement.enabled = on;
        if (!on && playerBody != null) playerBody.linearVelocity = Vector2.zero;
    }

    void SetPrompt(string text)
    {
        prompt.text = messageTimer > 0 ? message : text;
    }

    void Flash(string text)
    {
        message = text;
        messageTimer = 2.5f;
        prompt.text = text;
    }

    void UpdateHud()
    {
        SaveData d = GameState.Get().data;
        int minutes = Mathf.FloorToInt(gameMinutes);
        int hour = 5 + minutes / 60;
        string clock = (hour > 12 ? hour - 12 : hour) + ":" + (minutes % 60).ToString("00") + " PM";
        hud.text = day.name.ToUpper() + "   " + clock + "      TIPS $" + d.tips +
                   "      MEMORIES " + d.memoryFragments.Count + "/5      DELIVERED " + orderIndex + "/" + day.orders.Count;
    }

    void UpdateLabels()
    {
        if (cam == null) return;
        bool hint = phase == Phase.Delivering && orderTimer > hintAfterSeconds;
        foreach (KeyValuePair<HouseVisual, Text> pair in labels)
        {
            Vector3 screen = cam.WorldToScreenPoint(pair.Key.LabelPosition);
            bool onScreen = screen.z > 0 && screen.x > -100 && screen.x < Screen.width + 100;
            pair.Value.enabled = onScreen;
            if (!onScreen) continue;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
            pair.Value.rectTransform.anchoredPosition = local;

            bool isTarget = hint && order != null && pair.Key.houseId == order.houseId;
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 6f);
            pair.Value.color = isTarget ? new Color(1f, 0.86f, 0.2f, pulse + 0.2f) : new Color(1f, 1f, 1f, 0.85f);
            pair.Value.text = isTarget ? "v " + pair.Key.displayName + " v" : pair.Key.displayName;
        }
    }

    // ---------------- UI ----------------

    void BuildUI()
    {
        canvas = UIHelper.MakeCanvas(day.tint);     // tint = weather overlay for the day
        canvasRect = canvas.GetComponent<RectTransform>();

        foreach (HouseVisual h in houses)
        {
            Text label = UIHelper.MakeText(canvas, h.displayName, Vector2.zero, 16, Color.white);
            label.raycastTarget = false;
            labels[h] = label;
        }

        Panel(new Vector2(0, 322), new Vector2(1280, 44), new Color(0.04f, 0.05f, 0.1f, 0.75f));
        hud = UIHelper.MakeText(canvas, "", new Vector2(0, 322), 20, Color.white);

        orderCard = Panel(new Vector2(0, -258), new Vector2(760, 74), new Color(0.04f, 0.05f, 0.1f, 0.85f)).gameObject;
        orderTitle = Child(orderCard, UIHelper.MakeText(canvas, "", Vector2.zero, 22, new Color(1f, 0.86f, 0.3f)), new Vector2(0, 14));
        orderClue = Child(orderCard, UIHelper.MakeText(canvas, "", Vector2.zero, 18, Color.white), new Vector2(0, -16));
        orderCard.SetActive(false);

        prompt = UIHelper.MakeText(canvas, "", new Vector2(0, -190), 22, Color.white);
        Outline o = prompt.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0, 0, 0, 0.8f);

        UIHelper.MakeText(canvas, "Move: WASD / stick     Interact: SPACE / A     (debug) C: crush pizza     O: open save folder",
            new Vector2(0, -336), 14, new Color(1f, 1f, 1f, 0.7f));

        notePanel = Panel(new Vector2(0, 40), new Vector2(640, 250), new Color(0.98f, 0.91f, 0.63f, 0.97f)).gameObject;
        noteTitle = Child(notePanel, UIHelper.MakeText(canvas, "", Vector2.zero, 26, new Color(0.47f, 0.24f, 0.08f)), new Vector2(0, 85));
        noteBody = Child(notePanel, UIHelper.MakeText(canvas, "", Vector2.zero, 22, new Color(0.16f, 0.12f, 0.08f)), new Vector2(0, 10));
        noteBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        noteBody.rectTransform.sizeDelta = new Vector2(560, 120);
        noteHint = Child(notePanel, UIHelper.MakeText(canvas, "", Vector2.zero, 18, Color.white), new Vector2(0, -95));
        noteHint.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.7f);
        notePanel.SetActive(false);

        introPanel = Panel(Vector2.zero, new Vector2(1400, 800), new Color(0.03f, 0.04f, 0.09f, 0.94f)).gameObject;
        introBody = Child(introPanel, UIHelper.MakeText(canvas, "", Vector2.zero, 26, Color.white), new Vector2(0, 40));
        introBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        introBody.rectTransform.sizeDelta = new Vector2(900, 360);
        introHint = Child(introPanel, UIHelper.MakeText(canvas, "", Vector2.zero, 22, new Color(1f, 0.86f, 0.3f)), new Vector2(0, -230));
    }

    Image Panel(Vector2 pos, Vector2 size, Color color)
    {
        Image img = new GameObject("Panel", typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(canvas.transform, false);
        img.color = color;
        img.raycastTarget = false;
        img.rectTransform.anchoredPosition = pos;
        img.rectTransform.sizeDelta = size;
        return img;
    }

    static Text Child(GameObject parent, Text t, Vector2 pos)
    {
        t.transform.SetParent(parent.transform, false);
        t.rectTransform.anchoredPosition = pos;
        t.raycastTarget = false;
        return t;
    }
}

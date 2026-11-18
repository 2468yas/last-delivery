using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shubh's test street: walk to a house's door and deliver.
//   WASD / arrows = walk
//   SPACE at a door = intact delivery     C at a door = crushed delivery
//   E = end shift (goes to Yasin's summary screen, which saves)
//   O = open the folder with the save file
// Each house can be delivered to once per shift.
public class TestStreetShift : MonoBehaviour
{
    static readonly string[] Days = { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

    public float deliverRange = 1.2f;

    Text hud, prompt, pathText;
    HouseVisual[] houses;
    Transform player;
    readonly HashSet<string> deliveredToday = new HashSet<string>();
    string lastMessage = "";
    float messageTimer;

    void Start()
    {
        GameState.Get();
        houses = FindObjectsByType<HouseVisual>(FindObjectsSortMode.None);
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;

        Canvas c = UIHelper.MakeCanvas(Color.clear);
        hud = UIHelper.MakeText(c, "", new Vector2(0, 320), 24, Color.white);
        prompt = UIHelper.MakeText(c, "", new Vector2(0, -250), 26, new Color(1f, 0.86f, 0.3f));
        UIHelper.MakeText(c, "WASD walk   SPACE deliver intact   C deliver crushed   E end shift   O open save folder",
            new Vector2(0, -320), 18, Color.white);
        pathText = UIHelper.MakeText(c, "", new Vector2(0, -290), 14, new Color(0.75f, 0.75f, 0.75f));
    }

    void Update()
    {
        GameState gs = GameState.Get();
        Keyboard k = Keyboard.current;
        HouseVisual near = NearestHouse();

        if (k != null)
        {
            if (near != null && (k.spaceKey.wasPressedThisFrame || k.cKey.wasPressedThisFrame))
                TryDeliver(near, k.spaceKey.wasPressedThisFrame);
            if (k.eKey.wasPressedThisFrame) SceneManager.LoadScene(SceneNames.Summary);
            if (k.oKey.wasPressedThisFrame) SaveManager.OpenSaveFolder();
        }

        // HUD
        SaveData d = gs.data;
        hud.text = Days[d.currentDay] + "   |   Tips $" + d.tips + " (+" + gs.shiftTips + " today)   |   Memories "
                   + d.memoryFragments.Count + "/5   |   Delivered " + deliveredToday.Count + "/" + houses.Length;

        if (messageTimer > 0) { messageTimer -= Time.deltaTime; prompt.text = lastMessage; }
        else if (near == null) prompt.text = "";
        else if (deliveredToday.Contains(near.houseId)) prompt.text = near.displayName + ": already delivered today";
        else prompt.text = near.displayName + "  (" + gs.GetHouseState(near.houseId) + ")  -  SPACE intact / C crushed";

        pathText.text = SaveManager.HasSave() ? "Save file: " + SaveManager.SavePath : "No save file yet (end a shift to save)";
    }

    void TryDeliver(HouseVisual house, bool intact)
    {
        if (deliveredToday.Contains(house.houseId)) return;

        deliveredToday.Add(house.houseId);
        GameState.Get().RecordDelivery(house.houseId, intact, intact ? house.goodMemory : house.badMemory);

        lastMessage = intact ? "Delivered intact! +$" + GameState.TipsIntact + "  Memory: " + house.goodMemory
                             : "...it's a little squished. +$" + GameState.TipsCrushed + "  Memory: " + house.badMemory;
        messageTimer = 2.5f;
    }

    HouseVisual NearestHouse()
    {
        if (player == null) return null;
        HouseVisual best = null;
        float bestDist = deliverRange;
        foreach (HouseVisual h in houses)
        {
            float dist = Vector2.Distance(player.position, h.DoorPosition);
            if (dist < bestDist) { bestDist = dist; best = h; }
        }
        return best;
    }
}

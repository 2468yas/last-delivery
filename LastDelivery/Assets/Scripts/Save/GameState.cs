using System;
using UnityEngine;

// Shubh's GameState: holds the current SaveData and sticks around between scenes.
// Keeps every name Yasin's screens use (Get, data, shiftTips, shiftFragment,
// RecordDelivery, GetHouseState, NextDay, NewGame) and adds events so the
// houses and HUD can update themselves.
public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public SaveData data = new SaveData();

    // current shift only (not saved, used by the summary screen)
    public int shiftTips = 0;
    public string shiftFragment = "";

    public const int TipsIntact = 12;
    public const int TipsCrushed = 2;
    public const int MinScore = -3;
    public const int MaxScore = 3;
    public const int LastDay = 5;

    // fired when anything changes (tips, day, new game, load)
    public event Action Changed;
    // fired with the houseId when one house's memory changes
    public event Action<string> HouseChanged;

    // makes sure there's always one GameState, even if you hit play in any scene
    public static GameState Get()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GameState");
            Instance = go.AddComponent<GameState>();
            DontDestroyOnLoad(go);
        }
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // a delivery finished: good = intact, otherwise crushed
    public void RecordDelivery(string houseId, bool good, string fragment)
    {
        int tips = good ? TipsIntact : TipsCrushed;
        data.tips += tips;
        shiftTips += tips;
        ChangeHouseScore(houseId, good ? 1 : -1);

        if (!string.IsNullOrEmpty(fragment) && !data.memoryFragments.Contains(fragment))
        {
            data.memoryFragments.Add(fragment);
            shiftFragment = fragment;
        }
        Changed?.Invoke();
    }

    // good = +1, crushed = -1, kept between -3 and 3
    public void ChangeHouseScore(string houseId, int change)
    {
        HouseMemory house = FindHouse(houseId);
        if (house == null)
        {
            house = new HouseMemory { houseId = houseId, score = 0 };
            data.houseMemories.Add(house);
        }
        house.score = Mathf.Clamp(house.score + change, MinScore, MaxScore);
        HouseChanged?.Invoke(houseId);
    }

    public int GetHouseScore(string houseId)
    {
        HouseMemory house = FindHouse(houseId);
        return house == null ? 0 : house.score;
    }

    // turns the score into faded / normal / vivid for the art
    public MemoryState GetHouseState(string houseId)
    {
        int score = GetHouseScore(houseId);
        if (score >= 1) return MemoryState.Vivid;
        if (score <= -1) return MemoryState.Faded;
        return MemoryState.Normal;
    }

    // move to the next day and clear the shift stuff
    public void NextDay()
    {
        data.currentDay = Mathf.Min(data.currentDay + 1, LastDay);
        shiftTips = 0;
        shiftFragment = "";
        Changed?.Invoke();
    }

    // start over on Monday
    public void NewGame()
    {
        data = new SaveData();
        shiftTips = 0;
        shiftFragment = "";
        RefreshEverything();
    }

    // load from the save file (same as the start screen's Continue)
    public void LoadFromDisk()
    {
        data = SaveManager.Load();
        shiftTips = 0;
        shiftFragment = "";
        RefreshEverything();
    }

    public bool SaveToDisk() { return SaveManager.Save(data); }

    HouseMemory FindHouse(string houseId)
    {
        return data.houseMemories.Find(h => h.houseId == houseId);
    }

    void RefreshEverything()
    {
        Changed?.Invoke();
        foreach (HouseMemory h in data.houseMemories) HouseChanged?.Invoke(h.houseId);
    }
}

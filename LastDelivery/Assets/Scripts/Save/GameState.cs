using UnityEngine;

// TEMPORARY (Shubh's part) - swap in his version when it's ready
// holds the current SaveData and sticks around between scenes
public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public SaveData data = new SaveData();

    // stuff from the current shift only (not saved, just for the summary screen)
    public int shiftTips = 0;
    public string shiftFragment = "";

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
        int tips = good ? 12 : 2;
        data.tips += tips;
        shiftTips += tips;
        ChangeHouseScore(houseId, good ? 1 : -1);
        if (fragment != "" && !data.memoryFragments.Contains(fragment))
        {
            data.memoryFragments.Add(fragment);
            shiftFragment = fragment;
        }
    }

    // good = +1, crushed = -1, kept between -3 and 3
    public void ChangeHouseScore(string houseId, int change)
    {
        HouseMemory house = data.houseMemories.Find(h => h.houseId == houseId);
        if (house == null)
        {
            house = new HouseMemory { houseId = houseId, score = 0 };
            data.houseMemories.Add(house);
        }
        house.score = Mathf.Clamp(house.score + change, -3, 3);
    }

    // turns the score into faded / normal / vivid for the art
    public MemoryState GetHouseState(string houseId)
    {
        HouseMemory house = data.houseMemories.Find(h => h.houseId == houseId);
        int score = house == null ? 0 : house.score;
        if (score >= 1) return MemoryState.Vivid;
        if (score <= -1) return MemoryState.Faded;
        return MemoryState.Normal;
    }

    // move to the next day and clear the shift stuff
    public void NextDay()
    {
        data.currentDay = Mathf.Min(data.currentDay + 1, 5);
        shiftTips = 0;
        shiftFragment = "";
    }

    // start over on Monday
    public void NewGame()
    {
        data = new SaveData();
        shiftTips = 0;
        shiftFragment = "";
    }
}

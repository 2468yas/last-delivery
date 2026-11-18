using System.IO;
using UnityEngine;

// TEMPORARY (Shubh's part) - swap in his version when it's ready
// reads and writes the save as a json file
public static class SaveManager
{
    // lives in the computer's app data folder so it survives closing the game
    static string SavePath => Path.Combine(Application.persistentDataPath, "lastdelivery_save.json");

    public static void Save(SaveData data)
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log("Saved to " + SavePath);
    }

    // returns the saved data, or a fresh one if there's no save
    public static SaveData Load()
    {
        if (!HasSave()) return new SaveData();
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }

    public static bool HasSave() { return File.Exists(SavePath); }

    public static void DeleteSave()
    {
        if (HasSave()) File.Delete(SavePath);
    }
}

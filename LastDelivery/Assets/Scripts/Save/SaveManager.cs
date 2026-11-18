using System;
using System.IO;
using UnityEngine;

// Shubh's save system: reads and writes the SaveData as a JSON file.
// Same function names as Yasin's temporary version, so none of his screens change.
public static class SaveManager
{
    const string FileName = "lastdelivery_save.json";

    // Windows: C:\Users\<you>\AppData\LocalLow\<Company>\<Product>\lastdelivery_save.json
    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave() { return File.Exists(SavePath); }

    // EXPORT: SaveData -> pretty-printed JSON -> file.
    // Writes to a temp file first, so a crash mid-save can't wipe the old save.
    public static bool Save(SaveData data)
    {
        if (data == null) { Debug.LogWarning("SaveManager: nothing to save."); return false; }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string temp = SavePath + ".tmp";
            File.WriteAllText(temp, json);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temp, SavePath);
            Debug.Log("Saved to " + SavePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("SaveManager: could not save. " + e.Message);
            return false;
        }
    }

    // IMPORT: file -> JSON -> SaveData.
    // Returns a fresh SaveData if there's no save or the file is broken (never null).
    public static SaveData Load()
    {
        if (!HasSave()) return new SaveData();

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            if (data == null) throw new Exception("file was empty");
            return Sanitize(data);
        }
        catch (Exception e)
        {
            // keep the broken file for debugging instead of silently losing it
            string backup = Path.Combine(Application.persistentDataPath, "lastdelivery_save.corrupt.json");
            try { File.Copy(SavePath, backup, true); } catch { }
            Debug.LogWarning("SaveManager: save was unreadable (" + e.Message + "). Backed up to " + backup + " and starting fresh.");
            return new SaveData();
        }
    }

    public static void DeleteSave()
    {
        if (HasSave()) File.Delete(SavePath);
    }

    // Fixes anything a hand-edited or old save could get wrong.
    static SaveData Sanitize(SaveData d)
    {
        if (d.houseMemories == null) d.houseMemories = new System.Collections.Generic.List<HouseMemory>();
        if (d.ownedUpgrades == null) d.ownedUpgrades = new System.Collections.Generic.List<string>();
        if (d.memoryFragments == null) d.memoryFragments = new System.Collections.Generic.List<string>();
        d.currentDay = Mathf.Clamp(d.currentDay, 1, 5);
        d.tips = Mathf.Max(0, d.tips);
        foreach (HouseMemory h in d.houseMemories) h.score = Mathf.Clamp(h.score, -3, 3);
        return d;
    }

    // ---- extras for the demo ----

    // Opens the folder with the save file in Explorer / Finder.
    public static void OpenSaveFolder()
    {
        Application.OpenURL("file:///" + Application.persistentDataPath.Replace("\\", "/"));
    }

    // Copies the save to any path (e.g. the Desktop) and back.
    public static bool ExportTo(string path)
    {
        if (!HasSave()) return false;
        try { File.Copy(SavePath, path, true); return true; }
        catch (Exception e) { Debug.LogError("Export failed: " + e.Message); return false; }
    }

    public static bool ImportFrom(string path)
    {
        try
        {
            if (JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) == null) return false;
            File.Copy(path, SavePath, true);
            return true;
        }
        catch (Exception e) { Debug.LogError("Import failed: " + e.Message); return false; }
    }
}

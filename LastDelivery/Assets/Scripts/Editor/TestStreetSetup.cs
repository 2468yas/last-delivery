using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// menu: Tools > Last Delivery > Build Test Street
// makes Assets/Scenes/TestStreet.unity (4 houses, a road, the player) and adds it to Build Settings.
// Run it ONCE, then commit the scene. After that, edit the scene by hand like normal.
public static class TestStreetSetup
{
    const string ScenePath = "Assets/Scenes/TestStreet.unity";
    const string SquarePath = "Assets/Art/Placeholder/square.png";

    struct HouseInfo
    {
        public string id, name, good, bad;
        public Color color;
        public float x;
    }

    static readonly HouseInfo[] Houses =
    {
        new HouseInfo { id = "june_14_elm", name = "June - 14 Elm St", good = "Sunday dinners at June's", bad = "A door that stayed closed", color = new Color(0.18f, 0.82f, 0.78f), x = -6f },
        new HouseInfo { id = "parks_22_elm", name = "The Parks - 22 Elm St", good = "Babysitting the Park twins", bad = "The twins stopped waving", color = new Color(1f, 0.75f, 0.1f), x = -2f },
        new HouseInfo { id = "theo_5_willow", name = "Theo - 5 Willow Ln", good = "Late nights talking with Theo", bad = "Theo's unanswered texts", color = new Color(1f, 0.45f, 0.75f), x = 2f },
        new HouseInfo { id = "kessler_9_river", name = "Mr. Kessler - 9 Riverside", good = "Kessler almost smiled", bad = "Kessler's angry notes", color = new Color(0.45f, 0.55f, 1f), x = 6f },
    };

    [MenuItem("Tools/Last Delivery/Build Test Street")]
    public static void Build()
    {
        if (File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog("Rebuild Test Street?", "TestStreet.unity already exists. Rebuilding replaces any changes made to it by hand.", "Rebuild", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite square = GetSquare();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.42f, 0.66f, 0.31f);   // grass
        }

        // road and sidewalks
        MakeBlock("Sidewalk Top", square, new Vector2(0, -0.6f), new Vector2(20, 0.5f), new Color(0.84f, 0.84f, 0.84f), -5);
        MakeBlock("Road", square, new Vector2(0, -2.5f), new Vector2(20, 3.2f), new Color(0.59f, 0.59f, 0.59f), -5);
        MakeBlock("Sidewalk Bottom", square, new Vector2(0, -4.35f), new Vector2(20, 0.5f), new Color(0.84f, 0.84f, 0.84f), -5);
        for (int i = -9; i <= 9; i += 2)
            MakeBlock("Lane Dash", square, new Vector2(i, -2.5f), new Vector2(0.9f, 0.12f), Color.white, -4);

        // houses
        GameObject houseParent = new GameObject("Houses");
        foreach (HouseInfo info in Houses)
        {
            GameObject house = MakeBlock(info.name, square, new Vector2(info.x, 1.6f), new Vector2(3f, 2.4f), info.color, 0);
            house.transform.SetParent(houseParent.transform);
            house.AddComponent<BoxCollider2D>();

            HouseVisual hv = house.AddComponent<HouseVisual>();
            hv.houseId = info.id;
            hv.displayName = info.name;
            hv.goodMemory = info.good;
            hv.badMemory = info.bad;
            hv.baseColor = info.color;
            hv.doorOffset = new Vector2(0f, -1.7f);

            // roof and door (children, so they move with the house)
            GameObject roof = MakeBlock("Roof", square, Vector2.zero, Vector2.one, new Color(0.25f, 0.15f, 0.35f), 1);
            roof.transform.SetParent(house.transform, false);
            roof.transform.localPosition = new Vector3(0, 0.6f, 0);
            roof.transform.localScale = new Vector3(1.08f, 0.25f, 1);

            GameObject door = MakeBlock("Door", square, Vector2.zero, Vector2.one, new Color(0.43f, 0.24f, 0.13f), 1);
            door.transform.SetParent(house.transform, false);
            door.transform.localPosition = new Vector3(0, -0.32f, 0);
            door.transform.localScale = new Vector3(0.2f, 0.36f, 1);
        }

        // player (same setup as Yasin's test player)
        GameObject player = MakeBlock("Player", square, new Vector2(0, -2.5f), new Vector2(0.6f, 1f), new Color(0.75f, 0.22f, 0.17f), 5);
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<PlayerMovement>();

        new GameObject("TestStreet Controller", typeof(TestStreetShift));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log("TestStreet built. Commit Assets/Scenes/TestStreet.unity, then press Play on StartScreen.");
    }

    static GameObject MakeBlock(string name, Sprite sprite, Vector2 pos, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        return go;
    }

    // adds TestStreet to Build Settings without removing Yasin's scenes
    static void AddToBuildSettings()
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (list.Exists(s => s.path == ScenePath)) return;
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    // same 16x16 white square Yasin's setup makes (shared path, so only one copy exists)
    static Sprite GetSquare()
    {
        if (!File.Exists(SquarePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SquarePath));
            Texture2D tex = new Texture2D(16, 16);
            Color[] px = new Color[256];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            File.WriteAllBytes(SquarePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(SquarePath);

            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(SquarePath);
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
    }
}

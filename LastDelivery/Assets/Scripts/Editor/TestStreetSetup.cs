using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// menu: Tools > Last Delivery > Build Bellrum Street
// Builds Assets/Scenes/TestStreet.unity: Corner Slice, 8 houses with clue art, the road,
// trees, Willow Pond, the player (from Assets/Prefabs/Player.prefab) and the ShiftManager.
// Run it once, then commit the scene. Re-running replaces hand edits to the scene.
public static class TestStreetSetup
{
    const string ScenePath = "Assets/Scenes/TestStreet.unity";
    const string ArtFolder = "Assets/Art/Bellrum";
    const string SquarePath = "Assets/Art/Placeholder/square.png";
    const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";

    const float StreetLeft = -8f;
    const float StreetRight = 62f;
    const float BuildingBottom = 2.55f;   // where house fronts meet the sidewalk
    const float DoorY = 2.7f;             // where the player stands to deliver

    struct HouseInfo
    {
        public string id, address, resident;
        public float x;
        public HouseInfo(string id, string address, string resident, float x)
        {
            this.id = id; this.address = address; this.resident = resident; this.x = x;
        }
    }

    static readonly HouseInfo[] Houses =
    {
        new HouseInfo("nate_1_elm",      "1 Elm St",     "Nate",        8f),
        new HouseInfo("june_14_elm",     "14 Elm St",    "June",        14f),
        new HouseInfo("dana_7_elm",      "7 Elm St",     "Dana",        20f),
        new HouseInfo("parks_22_elm",    "22 Elm St",    "The Parks",   26f),
        new HouseInfo("theo_5_willow",   "5 Willow Ln",  "Theo",        32f),
        new HouseInfo("kessler_9_river", "9 Riverside",  "Mr. Kessler", 38f),
        new HouseInfo("rourke_3_river",  "3 Riverside",  "Mr. Rourke",  44f),
        new HouseInfo("home_12_mill",    "12 Mill Rd",   "???",         54f),
    };

    [MenuItem("Tools/Last Delivery/Build Bellrum Street")]
    public static void Build()
    {
        if (File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog("Rebuild the street?",
                "This replaces Assets/Scenes/TestStreet.unity (any hand edits to it are lost).", "Rebuild", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigurePixelArt();
        Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
        if (square == null) { Debug.LogError("Missing " + SquarePath + " (pull Yasin's commit)."); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ground
        Block("Road", square, new Vector2(27f, 0f), new Vector2(72f, 3.2f), new Color(0.45f, 0.45f, 0.48f), -6);
        Block("Sidewalk North", square, new Vector2(27f, 2.05f), new Vector2(72f, 0.9f), new Color(0.82f, 0.82f, 0.82f), -5);
        Block("Sidewalk South", square, new Vector2(27f, -2.05f), new Vector2(72f, 0.9f), new Color(0.82f, 0.82f, 0.82f), -5);
        GameObject dashes = new GameObject("Lane Dashes");
        for (float x = StreetLeft; x <= StreetRight; x += 3f)
            Block("Dash", square, new Vector2(x, 0f), new Vector2(1.2f, 0.12f), new Color(0.95f, 0.9f, 0.6f), -4).transform.SetParent(dashes.transform);

        // invisible walls: the player stays on the road and sidewalks
        GameObject walls = new GameObject("Walls");
        Wall(walls, "North", new Vector2(27f, BuildingBottom + 0.5f), new Vector2(72f, 1f));
        Wall(walls, "South", new Vector2(27f, -3f), new Vector2(72f, 1f));
        Wall(walls, "West", new Vector2(StreetLeft - 0.5f, 0f), new Vector2(1f, 10f));
        Wall(walls, "East", new Vector2(StreetRight + 0.5f, 0f), new Vector2(1f, 10f));

        // Corner Slice
        Sprite shopSprite = Art("corner_slice");
        GameObject shop = Building("Corner Slice", shopSprite, 0f);
        CornerSlice cs = shop.AddComponent<CornerSlice>();
        cs.doorOffset = new Vector2(0f, DoorY - shop.transform.position.y);

        // houses
        GameObject houseParent = new GameObject("Houses");
        foreach (HouseInfo info in Houses)
        {
            Sprite normal = Art("house_" + info.id + "_normal");
            GameObject house = Building(info.address, normal, info.x);
            house.transform.SetParent(houseParent.transform);

            HouseVisual hv = house.AddComponent<HouseVisual>();
            hv.houseId = info.id;
            hv.displayName = info.address;
            hv.residentName = info.resident;
            hv.normalSprite = normal;
            hv.vividSprite = Art("house_" + info.id + "_vivid");
            hv.fadedSprite = Art("house_" + info.id + "_faded");
            hv.doorOffset = new Vector2(0f, DoorY - house.transform.position.y);
        }

        // decoration
        GameObject decor = new GameObject("Decor");
        Sprite tree = Art("tree"), lamp = Art("lamppost"), pond = Art("pond");
        for (float x = StreetLeft + 1f; x < StreetRight; x += 4.5f)
        {
            if (x > 24f && x < 36f) continue;   // leave room for the pond
            Decor(decor, "Tree", tree, new Vector2(x + Mathf.Sin(x) * 0.8f, -4.1f + Mathf.Cos(x * 1.7f) * 0.3f), 0);
        }
        Decor(decor, "Willow Pond", pond, new Vector2(30f, -4.3f), -3);
        foreach (float x in new[] { 4f, 11f, 17f, 23f, 29f, 35f, 41f, 49f, 58f })
            Decor(decor, "Lamppost", lamp, new Vector2(x, BuildingBottom + 0.6f), 3);

        // player (prefab, so Yasin can add his balancing scripts without touching this scene)
        GameObject prefab = GetOrMakePlayerPrefab();
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        player.transform.position = new Vector3(0f, 0.6f, 0f);

        // camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.42f, 0.66f, 0.31f);
            cam.transform.position = new Vector3(StreetLeft + 6f * 16f / 9f, 1.2f, -10f);
            CameraFollow follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = player.transform;
            follow.minX = StreetLeft;
            follow.maxX = StreetRight;
            follow.fixedY = 1.2f;
        }

        new GameObject("Shift Manager", typeof(ShiftManager));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log("Bellrum street built. Open StartScreen and press Play. Commit TestStreet.unity, Assets/Art/Bellrum, Assets/Prefabs and Assets/Resources.");
    }

    // ---------------- helpers ----------------

    static GameObject Building(string name, Sprite sprite, float x)
    {
        float height = sprite != null ? sprite.bounds.size.y : 3.5f;
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        go.transform.position = new Vector3(x, BuildingBottom + height / 2f, 0f);
        go.AddComponent<BoxCollider2D>();
        return go;
    }

    static GameObject Block(string name, Sprite sprite, Vector2 pos, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    static void Decor(GameObject parent, string name, Sprite sprite, Vector2 pos, int order)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        go.transform.position = pos;
        go.transform.SetParent(parent.transform);
    }

    static void Wall(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Wall " + name);
        go.transform.position = pos;
        go.AddComponent<BoxCollider2D>().size = size;
        go.transform.SetParent(parent.transform);
    }

    static Sprite Art(string name)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/" + name + ".png");
        if (s == null) Debug.LogWarning("Missing sprite " + ArtFolder + "/" + name + ".png");
        return s;
    }

    static GameObject GetOrMakePlayerPrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (existing != null) return existing;   // never overwrite Yasin's changes to the prefab

        Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPath));
        GameObject go = new GameObject("Player");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art("player");
        sr.sortingOrder = 5;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();   // feet only, for the top-down look
        col.size = new Vector2(0.7f, 0.45f);
        col.offset = new Vector2(0f, -0.75f);
        go.AddComponent<PlayerMovement>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PlayerPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // pixel art import settings from the art bible: 16 PPU, point filter, no compression
    static void ConfigurePixelArt()
    {
        if (!Directory.Exists(ArtFolder)) { Debug.LogError("Missing " + ArtFolder); return; }
        foreach (string file in Directory.GetFiles(ArtFolder, "*.png"))
        {
            string path = file.Replace("\\", "/");
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            bool changed = ti.textureType != TextureImporterType.Sprite || ti.spritePixelsPerUnit != 16f ||
                           ti.filterMode != FilterMode.Point || ti.textureCompression != TextureImporterCompression.Uncompressed ||
                           ti.spriteImportMode != SpriteImportMode.Single || ti.mipmapEnabled;
            if (!changed) continue;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16f;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    static void AddToBuildSettings()
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (list.Exists(s => s.path == ScenePath)) return;
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}

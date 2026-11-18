using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// menu: Tools > Last Delivery > Build Yasin Scenes
// makes the Start, PlayerTest, ShiftSummary and UpgradeShop scenes and adds them to Build Settings
public static class LastDeliverySetup
{
    const string SceneFolder = "Assets/Scenes";
    const string SquarePath = "Assets/Art/Placeholder/square.png";

    [MenuItem("Tools/Last Delivery/Build Yasin Scenes")]
    public static void BuildScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(SceneFolder);
        Sprite square = MakeSquareSprite();

        MakeScene(SceneNames.Start, typeof(StartScreen), null);
        MakeScene(SceneNames.Game, typeof(PlayerTestShift), square);
        MakeScene(SceneNames.Summary, typeof(ShiftSummary), null);
        MakeScene(SceneNames.Shop, typeof(UpgradeShop), null);

        // start screen first in the build order
        var list = new List<EditorBuildSettingsScene>();
        foreach (string n in new[] { SceneNames.Start, SceneNames.Game, SceneNames.Summary, SceneNames.Shop })
            list.Add(new EditorBuildSettingsScene(SceneFolder + "/" + n + ".unity", true));
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.OpenScene(SceneFolder + "/" + SceneNames.Start + ".unity");
        Debug.Log("Last Delivery scenes built! Press Play on StartScreen.");
    }

    // new scene with the default camera, plus one object that runs the screen's script
    static void MakeScene(string name, System.Type screenScript, Sprite square)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.13f, 0.29f);
        }

        new GameObject(name + " Controller", screenScript);

        // the test shift gets a red square player that can walk around
        if (square != null)
        {
            GameObject player = new GameObject("Player");
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = new Color(0.75f, 0.22f, 0.17f);
            player.transform.localScale = new Vector3(0.6f, 1f, 1f);
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<PlayerMovement>();
        }

        EditorSceneManager.SaveScene(scene, SceneFolder + "/" + name + ".unity");
    }

    // 16x16 white square png, imported as a pixel art sprite
    static Sprite MakeSquareSprite()
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

            // pixel art settings from the art bible
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

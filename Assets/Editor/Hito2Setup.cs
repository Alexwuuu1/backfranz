using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class Hito2Setup
{
    private const string MarkerPath = "ProjectSettings/Hito2SetupComplete.txt";
    private const string ScenePath = "Assets/Scenes/Nivel01.unity";
    private const string PlayerSheetPath = "Assets/Art/Characters/PlayerWalkRight.png";

    static Hito2Setup()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Tools/Hito 2/Crear o reconstruir escena")]
    public static void BuildScene()
    {
        EnsureFolders();
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        Sprite background = CreateTexture("Assets/Art/Backgrounds/CampusBackground.png", 32, 18, DrawBackground, 1f);
        Sprite grass = CreateTexture("Assets/Art/Tiles/GrassTile.png", 16, 16, DrawGrass, 16f);
        Sprite path = CreateTexture("Assets/Art/Tiles/PathTile.png", 16, 16, DrawPath, 16f);
        Sprite wall = CreateTexture("Assets/Art/Tiles/WallTile.png", 16, 16, DrawWall, 16f);
        Sprite roof = CreateTexture("Assets/Art/Tiles/RoofTile.png", 16, 16, DrawRoof, 16f);
        Sprite tree = CreateTexture("Assets/Art/Decorations/Tree.png", 16, 24, DrawTree, 16f);
        Sprite bench = CreateTexture("Assets/Art/Decorations/Bench.png", 24, 12, DrawBench, 16f);
        Sprite sign = CreateTexture("Assets/Art/Decorations/CampusSign.png", 20, 20, DrawSign, 16f);
        Sprite[] studentFrames = ImportPlayerSpriteSheet();

        Tile grassTile = CreateTile("Assets/Art/Tiles/GeneratedTiles/Grass.asset", grass);
        Tile pathTile = CreateTile("Assets/Art/Tiles/GeneratedTiles/Path.asset", path);
        Tile wallTile = CreateTile("Assets/Art/Tiles/GeneratedTiles/Wall.asset", wall);
        Tile roofTile = CreateTile("Assets/Art/Tiles/GeneratedTiles/Roof.asset", roof);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Nivel01";

        GameObject root = new GameObject("ESCENA INICIAL - Entrada al Campus");

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(root.transform);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(22, 57, 49, 255);

        GameObject backgroundObject = CreateSpriteObject("Fondo del Campus", background, Vector3.zero, -10, root.transform);
        backgroundObject.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

        GameObject gridObject = new GameObject("Grid del Campus", typeof(Grid));
        gridObject.transform.SetParent(root.transform);
        Grid grid = gridObject.GetComponent<Grid>();
        grid.cellSize = Vector3.one;

        Tilemap groundMap = CreateTilemap("Tilemap - Suelo y Senderos", gridObject.transform, 0);
        for (int x = -15; x < 15; x++)
        {
            for (int y = -8; y < 8; y++)
            {
                TileBase selected = (Mathf.Abs(x) <= 1 || (y >= -3 && y <= -2)) ? pathTile : grassTile;
                groundMap.SetTile(new Vector3Int(x, y, 0), selected);
            }
        }

        Tilemap buildingMap = CreateTilemap("Tilemap - Edificio Principal", gridObject.transform, 2);
        for (int x = -6; x <= 6; x++)
        {
            for (int y = 3; y <= 7; y++)
            {
                TileBase selected = y >= 6 ? roofTile : wallTile;
                if (y == 3 && Mathf.Abs(x) <= 1)
                {
                    continue;
                }
                buildingMap.SetTile(new Vector3Int(x, y, 0), selected);
            }
        }

        GameObject decorations = new GameObject("Decoración del Campus");
        decorations.transform.SetParent(root.transform);
        Vector3[] treePositions =
        {
            new Vector3(-11f, 5f), new Vector3(-9f, 1f), new Vector3(-12f, -5f),
            new Vector3(10f, 5f), new Vector3(9f, 0f), new Vector3(12f, -5f),
            new Vector3(-6f, -6f), new Vector3(6f, -6f)
        };
        for (int i = 0; i < treePositions.Length; i++)
        {
            CreateSpriteObject("Árbol " + (i + 1), tree, treePositions[i], 4, decorations.transform);
        }

        CreateSpriteObject("Banco", bench, new Vector3(4.5f, -0.8f), 4, decorations.transform);
        CreateSpriteObject("Señal de Bienvenida", sign, new Vector3(-4f, -0.5f), 5, decorations.transform);
        GameObject player = CreateSpriteObject("Estudiante Provisional", studentFrames[0], new Vector3(0.5f, -5f), 6, root.transform);
        ConfigurePlayer(player, studentFrames);

        GameObject notes = new GameObject("NOTA - Escena 2D jugable con movimiento horizontal");
        notes.transform.SetParent(root.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "Grupo 64";
        PlayerSettings.productName = "Backfranz";
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EditorSettings.externalVersionControl = "Visible Meta Files";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CreatePreview(camera);

        File.WriteAllText(MarkerPath,
            "Escena del Hito 2 creada el " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
            "Para reconstruirla: Tools > Hito 2 > Crear o reconstruir escena" + Environment.NewLine);
        AssetDatabase.Refresh();
        Debug.Log("Hito 2 preparado correctamente. Escena: " + ScenePath);
    }

    private static void RunOnce()
    {
        // Hito 3 owns scene setup; never reopen or rebuild Nivel01 automatically.
        if (File.Exists("Assets/Scenes/MenuPrincipal.unity")) return;
        try
        {
            if (File.Exists(MarkerPath))
            {
                EnsureExistingSceneIsPlayable();
                return;
            }

            BuildScene();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void EnsureFolders()
    {
        string[] folders =
        {
            "Assets/Art/Backgrounds",
            "Assets/Art/Characters",
            "Assets/Art/Decorations",
            "Assets/Art/Tiles/GeneratedTiles",
            "Assets/Art/UI",
            "Assets/Materials",
            "Assets/Prefabs",
            "Assets/Scenes",
            "Assets/Scripts",
            "Docs"
        };

        foreach (string folder in folders)
        {
            Directory.CreateDirectory(folder);
        }
    }

    private static Tilemap CreateTilemap(string name, Transform parent, int sortingOrder)
    {
        GameObject tilemapObject = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        tilemapObject.transform.SetParent(parent);
        TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return tilemapObject.GetComponent<Tilemap>();
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, int order, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        return gameObject;
    }

    private static void EnsureExistingSceneIsPlayable()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        if (!File.Exists(ScenePath) || !File.Exists(PlayerSheetPath))
        {
            return;
        }

        Sprite[] frames = ImportPlayerSpriteSheet();
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject player = GameObject.Find("Estudiante Provisional");
        if (player == null)
        {
            Debug.LogWarning("No se encontró el objeto Estudiante Provisional en Nivel01.");
            return;
        }

        ConfigurePlayer(player, frames);
        GameObject oldNote = GameObject.Find("NOTA - Escena visual sin mecánicas (Hito 2)");
        if (oldNote != null)
        {
            oldNote.name = "NOTA - Escena 2D jugable con movimiento horizontal";
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Movimiento 2D configurado. Usa A/D o las flechas izquierda y derecha.");
    }

    private static void ConfigurePlayer(GameObject player, Sprite[] frames)
    {
        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        renderer.sprite = frames[0];

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = player.AddComponent<Rigidbody2D>();
        }
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = player.AddComponent<BoxCollider2D>();
        }
        collider.size = new Vector2(0.8f, 1.7f);
        collider.offset = new Vector2(0f, -0.1f);

        PlayerMovement2D controller = player.GetComponent<PlayerMovement2D>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerMovement2D>();
        }
        controller.Configure(renderer, frames);
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(controller);
    }

    private static Sprite[] ImportPlayerSpriteSheet()
    {
        AssetDatabase.ImportAsset(PlayerSheetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PlayerSheetPath);
        bool needsSetup = importer.textureType != TextureImporterType.Sprite ||
                          importer.spriteImportMode != SpriteImportMode.Multiple ||
                          !Mathf.Approximately(importer.spritePixelsPerUnit, 16f) ||
                          importer.filterMode != FilterMode.Point ||
                          importer.mipmapEnabled;

        if (needsSetup)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;

            SpriteMetaData[] metadata = new SpriteMetaData[4];
            for (int i = 0; i < metadata.Length; i++)
            {
                metadata[i] = new SpriteMetaData
                {
                    name = "PlayerWalkRight_" + i,
                    rect = new Rect(i * 24, 0, 24, 32),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }
            importer.spritesheet = metadata;
            importer.SaveAndReimport();
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(PlayerSheetPath);
        Sprite[] frames = Array.FindAll(Array.ConvertAll(assets, asset => asset as Sprite), sprite => sprite != null);
        Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
        if (frames.Length != 4)
        {
            throw new InvalidOperationException("La hoja del personaje debe contener cuatro sprites de 24x32.");
        }
        return frames;
    }

    private static Tile CreateTile(string assetPath, Sprite sprite)
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(tile, assetPath);
        }
        else
        {
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    private static Sprite CreateTexture(string assetPath, int width, int height, Action<Texture2D> draw, float pixelsPerUnit)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Clear(texture, new Color32(0, 0, 0, 0));
        draw(texture);
        texture.Apply();

        string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            throw new InvalidOperationException("No se pudo importar el sprite: " + assetPath);
        }
        return sprite;
    }

    private static void CreatePreview(Camera camera)
    {
        const int width = 960;
        const int height = 540;
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, false);
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        preview.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        preview.Apply();
        File.WriteAllBytes("Docs/preview-campus.png", preview.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(renderTexture);
        UnityEngine.Object.DestroyImmediate(preview);
    }

    private static void Clear(Texture2D texture, Color32 color)
    {
        Color32[] pixels = new Color32[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels32(pixels);
    }

    private static void Fill(Texture2D texture, int x, int y, int width, int height, Color32 color)
    {
        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    texture.SetPixel(px, py, color);
            }
        }
    }

    private static void DrawBackground(Texture2D texture)
    {
        Fill(texture, 0, 0, texture.width, texture.height, new Color32(44, 104, 72, 255));
        Fill(texture, 0, 0, texture.width, 2, new Color32(28, 72, 55, 255));
        Fill(texture, 0, texture.height - 2, texture.width, 2, new Color32(28, 72, 55, 255));
    }

    private static void DrawGrass(Texture2D texture)
    {
        Fill(texture, 0, 0, 16, 16, new Color32(77, 151, 79, 255));
        for (int i = 1; i < 16; i += 4)
        {
            texture.SetPixel(i, (i * 3) % 15, new Color32(96, 176, 91, 255));
            texture.SetPixel((i + 7) % 16, (i * 5) % 15, new Color32(54, 126, 68, 255));
        }
    }

    private static void DrawPath(Texture2D texture)
    {
        Fill(texture, 0, 0, 16, 16, new Color32(203, 184, 135, 255));
        Fill(texture, 0, 0, 16, 1, new Color32(166, 145, 104, 255));
        Fill(texture, 0, 15, 16, 1, new Color32(232, 213, 160, 255));
        texture.SetPixel(3, 5, new Color32(181, 159, 116, 255));
        texture.SetPixel(11, 10, new Color32(181, 159, 116, 255));
    }

    private static void DrawWall(Texture2D texture)
    {
        Fill(texture, 0, 0, 16, 16, new Color32(225, 205, 164, 255));
        Fill(texture, 0, 0, 16, 2, new Color32(158, 113, 82, 255));
        Fill(texture, 4, 5, 8, 7, new Color32(91, 157, 181, 255));
        Fill(texture, 5, 6, 6, 5, new Color32(154, 209, 218, 255));
    }

    private static void DrawRoof(Texture2D texture)
    {
        Fill(texture, 0, 0, 16, 16, new Color32(154, 67, 58, 255));
        for (int y = 0; y < 16; y += 4)
            Fill(texture, 0, y, 16, 1, new Color32(112, 48, 46, 255));
        for (int x = 0; x < 16; x += 8)
            Fill(texture, x, 0, 1, 16, new Color32(185, 84, 67, 255));
    }

    private static void DrawTree(Texture2D texture)
    {
        Fill(texture, 6, 0, 4, 9, new Color32(104, 69, 45, 255));
        Fill(texture, 3, 7, 10, 10, new Color32(38, 111, 63, 255));
        Fill(texture, 1, 11, 14, 8, new Color32(49, 139, 70, 255));
        Fill(texture, 4, 17, 8, 6, new Color32(69, 164, 80, 255));
    }

    private static void DrawBench(Texture2D texture)
    {
        Fill(texture, 2, 5, 20, 5, new Color32(130, 79, 48, 255));
        Fill(texture, 3, 1, 3, 5, new Color32(70, 69, 66, 255));
        Fill(texture, 18, 1, 3, 5, new Color32(70, 69, 66, 255));
        Fill(texture, 2, 9, 20, 2, new Color32(171, 105, 57, 255));
    }

    private static void DrawSign(Texture2D texture)
    {
        Fill(texture, 8, 0, 4, 9, new Color32(92, 66, 43, 255));
        Fill(texture, 1, 7, 18, 11, new Color32(41, 93, 132, 255));
        Fill(texture, 2, 8, 16, 9, new Color32(235, 222, 167, 255));
        Fill(texture, 4, 11, 12, 2, new Color32(41, 93, 132, 255));
    }

    private static void DrawStudent(Texture2D texture)
    {
        Fill(texture, 5, 16, 6, 6, new Color32(221, 174, 132, 255));
        Fill(texture, 4, 10, 8, 7, new Color32(49, 91, 164, 255));
        Fill(texture, 3, 8, 3, 8, new Color32(49, 91, 164, 255));
        Fill(texture, 10, 8, 3, 8, new Color32(49, 91, 164, 255));
        Fill(texture, 5, 1, 3, 9, new Color32(43, 52, 74, 255));
        Fill(texture, 9, 1, 3, 9, new Color32(43, 52, 74, 255));
        Fill(texture, 4, 21, 8, 2, new Color32(55, 36, 30, 255));
    }
}

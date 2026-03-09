using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// 一键自动生成2048Merge完整游戏场景
/// 使用方法：Unity菜单 → Tools → 2048 AutoSetup
/// </summary>
public class AutoSetup : EditorWindow
{
    [MenuItem("Tools/2048 AutoSetup ▶")]
    public static void RunSetup()
    {
        if (!EditorUtility.DisplayDialog("2048 AutoSetup",
            "将自动生成完整游戏场景，包括：\n\n" +
            "✅ 棋盘 (4x4)\n" +
            "✅ 计分/计时UI\n" +
            "✅ 胜利/失败弹窗\n" +
            "✅ 广告管理器\n" +
            "✅ 分数管理器\n" +
            "✅ 触摸输入\n\n" +
            "是否继续？", "开始生成", "取消"))
            return;

        // 新建场景
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 设置相机
        SetupCamera();

        // 创建Canvas根节点
        GameObject canvasGO = CreateCanvas();
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // 创建背景
        CreateBackground(canvasGO);

        // 创建顶部信息栏
        GameObject scoreText = CreateTopBar(canvasGO,
            out TextMeshProUGUI scoreTMP,
            out TextMeshProUGUI timerTMP,
            out TextMeshProUGUI bestTMP,
            out TextMeshProUGUI targetTMP);

        // 创建棋盘
        GameObject boardParent = CreateBoard(canvasGO, out GameObject[,] tiles);

        // 创建胜利/失败弹窗
        GameObject winPanel = CreateResultPanel(canvasGO, "🎉 YOU WIN!", new Color(0.2f, 0.8f, 0.4f));
        GameObject losePanel = CreateResultPanel(canvasGO, "😅 GAME OVER", new Color(0.9f, 0.3f, 0.3f));

        // 创建GameBoard脚本载体
        GameObject gameBoardGO = new GameObject("GameBoard");
        GameBoard gb = gameBoardGO.AddComponent<GameBoard>();
        gb.boardSize = 4;
        gb.targetValue = 2048;
        gb.timeLimit = 60f;
        gb.boardParent = boardParent.transform;
        gb.scoreText = scoreTMP;
        gb.timerText = timerTMP;
        gb.targetText = targetTMP;
        gb.gameOverPanel = losePanel;
        gb.winPanel = winPanel;

        // 创建Tile预制体（用代码生成占位）
        gb.tilePrefab = CreateTilePrefab();

        // 创建SwipeInput
        GameObject inputGO = new GameObject("SwipeInput");
        SwipeInput swipe = inputGO.AddComponent<SwipeInput>();
        swipe.gameBoard = gb;
        swipe.minSwipeDistance = 50f;

        // 创建ScoreManager
        GameObject scoreManagerGO = new GameObject("ScoreManager");
        ScoreManager sm = scoreManagerGO.AddComponent<ScoreManager>();
        sm.bestScoreText = bestTMP;

        // 创建AdManager
        GameObject adManagerGO = new GameObject("AdManager");
        adManagerGO.AddComponent<AdManager>();

        // 保存场景
        EditorSceneManager.MarkSceneDirty(scene);
        string path = "Assets/Scenes/GameScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);

        EditorUtility.DisplayDialog("✅ 完成！",
            "场景已生成并保存至：\nAssets/Scenes/GameScene.unity\n\n" +
            "接下来只需：\n" +
            "1. 在AdManager填入IronSource AppKey\n" +
            "2. 点击Play测试游戏\n" +
            "3. 替换美术素材（可选）",
            "明白了！");

        Debug.Log("[AutoSetup] 🎮 2048Merge场景生成完成！");
    }

    // ────────────────────────────────────────────
    // 辅助生成方法
    // ────────────────────────────────────────────

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.backgroundColor = new Color(0.18f, 0.17f, 0.16f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
    }

    static GameObject CreateCanvas()
    {
        GameObject go = new GameObject("Canvas");
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void CreateBackground(GameObject parent)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent.transform, false);
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.18f, 0.17f, 0.16f);
        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateTopBar(GameObject parent,
        out TextMeshProUGUI scoreTMP,
        out TextMeshProUGUI timerTMP,
        out TextMeshProUGUI bestTMP,
        out TextMeshProUGUI targetTMP)
    {
        // 顶部容器
        GameObject topBar = CreateUIRect("TopBar", parent,
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -200), new Vector2(0, 0));

        // 分数
        scoreTMP = CreateLabel("ScoreLabel", topBar, "分数: 0",
            new Vector2(0, 0.5f), new Vector2(0.3f, 0.5f),
            new Vector2(0, 0), 52, Color.white);

        // 计时
        timerTMP = CreateLabel("TimerLabel", topBar, "⏱ 60s",
            new Vector2(0.35f, 0.5f), new Vector2(0.65f, 0.5f),
            new Vector2(0, 0), 52, Color.yellow);

        // 最高分
        bestTMP = CreateLabel("BestLabel", topBar, "最高: 0",
            new Vector2(0.7f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0, 0), 36, new Color(0.8f, 0.8f, 0.8f));

        // 目标
        targetTMP = CreateLabel("TargetLabel", parent, "目标: 2048",
            new Vector2(0.1f, 1), new Vector2(0.9f, 1),
            new Vector2(0, -230), 42, new Color(1f, 0.9f, 0.3f));
        targetTMP.alignment = TextAlignmentOptions.Center;

        return topBar;
    }

    static GameObject CreateBoard(GameObject parent, out GameObject[,] tiles)
    {
        int size = 4;
        float boardWidth = 900f;
        float tileSize = boardWidth / size - 10;
        float gap = 10f;

        GameObject board = CreateUIRect("BoardParent", parent,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(boardWidth, boardWidth));

        // 棋盘背景
        Image boardBg = board.AddComponent<Image>();
        boardBg.color = new Color(0.73f, 0.68f, 0.63f);

        GridLayoutGroup grid = board.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(tileSize, tileSize);
        grid.spacing = new Vector2(gap, gap);
        grid.padding = new RectOffset(5, 5, 5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = size;

        tiles = new GameObject[size, size];
        for (int i = 0; i < size * size; i++)
        {
            int r = i / size;
            int c = i % size;
            tiles[r, c] = CreateTileGO("Tile_" + r + "_" + c, board, tileSize);
        }

        return board;
    }

    static GameObject CreateTileGO(string name, GameObject parent, float size)
    {
        GameObject tile = new GameObject(name);
        tile.transform.SetParent(parent.transform, false);

        RectTransform rt = tile.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        Image bg = tile.AddComponent<Image>();
        bg.color = new Color(0.80f, 0.75f, 0.71f);

        // 数字文字
        GameObject textGO = new GameObject("NumberText");
        textGO.transform.SetParent(tile.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 60;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.47f, 0.43f, 0.40f);

        return tile;
    }

    static GameObject CreateTilePrefab()
    {
        // 创建轻量占位tile预制体供GameBoard.Instantiate用
        GameObject tile = new GameObject("TilePrefab");
        Image bg = tile.AddComponent<Image>();
        bg.color = new Color(0.80f, 0.75f, 0.71f);

        GameObject textGO = new GameObject("NumberText");
        textGO.transform.SetParent(tile.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 60;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        // 保存为预制体
        string prefabPath = "Assets/Prefabs/TilePrefab.prefab";
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, prefabPath);
        Object.DestroyImmediate(tile);
        return prefab;
    }

    static GameObject CreateResultPanel(GameObject parent, string title, Color bgColor)
    {
        GameObject panel = CreateUIRect("Panel_" + title, parent,
            new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.7f),
            Vector2.zero, Vector2.zero);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0.95f);

        // 标题
        TextMeshProUGUI titleTMP = CreateLabel("Title", panel, title,
            new Vector2(0, 0.6f), new Vector2(1, 0.9f),
            Vector2.zero, 72, Color.white);
        titleTMP.alignment = TextAlignmentOptions.Center;

        // 重试按钮
        GameObject btnGO = CreateButton("RetryBtn", panel, "🔄 再来一局",
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.45f));

        // 看广告按钮（激励视频）
        GameObject adBtnGO = CreateButton("WatchAdBtn", panel, "📺 看广告复活",
            new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.75f),
            new Color(1f, 0.76f, 0.15f));

        panel.SetActive(false);
        return panel;
    }

    // ────────────────────────────────────────────
    // UI工具方法
    // ────────────────────────────────────────────

    static GameObject CreateUIRect(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return go;
    }

    static TextMeshProUGUI CreateLabel(string name, GameObject parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    static GameObject CreateButton(string name, GameObject parent, string label,
        Vector2 anchorMin, Vector2 anchorMax,
        Color? bgColor = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = bgColor ?? new Color(0.3f, 0.6f, 0.9f);
        Button btn = go.AddComponent<Button>();

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 44;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }
}

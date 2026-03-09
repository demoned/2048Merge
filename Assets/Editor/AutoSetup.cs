using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

/// <summary>
/// 一键生成完整游戏场景
/// 菜单：Tools → 2048 AutoSetup ▶
/// </summary>
public class AutoSetup : EditorWindow
{
    [MenuItem("Tools/2048 AutoSetup ▶")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog("2048 AutoSetup",
            "自动生成完整游戏场景：\n\n" +
            "✅ 主菜单\n✅ 游戏棋盘 4x4\n✅ 计分/计时UI\n" +
            "✅ 胜利/失败弹窗\n✅ 广告/分数/输入管理器\n\n继续？",
            "生成", "取消"))
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 相机
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.11f, 0.10f);
        cam.orthographic = true;
        camGO.tag = "MainCamera";

        // Canvas
        var canvas = MakeCanvas();

        // 背景
        MakeImageFull("BG", canvas, new Color(0.12f, 0.11f, 0.10f));

        // 管理器
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("ScoreManager").AddComponent<ScoreManager>();
        new GameObject("AdManager").AddComponent<AdManager>();
        new GameObject("SwipeInput").AddComponent<SwipeInput>();

        // ── 主菜单 ──
        var menuPanel = MakePanelFull("MainMenuPanel", canvas);
        MakeImageFull("MenuBG", menuPanel, new Color(0.18f, 0.17f, 0.16f));

        var titleTxt = MakeTextCenter("TitleText", menuPanel, "2048 MERGE",
            0f, 250f, 800f, 130f, 90, Color.white, FontStyles.Bold);
        titleTxt.alignment = TextAlignmentOptions.Center;

        var bestMenuTxt = MakeTextCenter("BestText", menuPanel, "最高分: 0",
            0f, 100f, 500f, 70f, 44, new Color(0.9f, 0.8f, 0.3f));
        bestMenuTxt.alignment = TextAlignmentOptions.Center;

        var playBtn = MakeButtonCenter("PlayButton", menuPanel, "开始游戏",
            0f, -80f, 420f, 110f, new Color(0.3f, 0.7f, 0.4f));

        // ── 游戏界面 ──
        var gamePanel = MakePanelFull("GamePanel", canvas);

        var scoreTxt   = MakeTextAnchor("ScoreText",  gamePanel, "分数: 0",  0f,0f,0.35f,1f, -180f,140f, 0f,0f, 46, Color.white);
        var timerTxt   = MakeTextAnchor("TimerText",  gamePanel, "⏱ 60s",   0.33f,0f,0.67f,1f,-180f,140f, 0f,0f, 50, Color.yellow, FontStyles.Bold);
        timerTxt.alignment = TextAlignmentOptions.Center;
        var bestTxt    = MakeTextAnchor("BestText",   gamePanel, "最高: 0",  0.66f,0f,1f,1f,  -180f,140f, 0f,0f, 36, new Color(0.7f,0.7f,0.7f));
        bestTxt.alignment = TextAlignmentOptions.Right;

        var levelTxt   = MakeTextCenter("LevelText",  gamePanel, "第 1 关",  0f, 760f, 600f, 60f, 36, new Color(0.7f,0.9f,1f));
        levelTxt.alignment = TextAlignmentOptions.Center;
        var targetTxt  = MakeTextCenter("TargetText", gamePanel, "目标: 2048",0f,690f,600f,60f, 40, new Color(1f,0.9f,0.3f));
        targetTxt.alignment = TextAlignmentOptions.Center;

        // 棋盘背景
        MakeImageCenter("BoardBG", gamePanel, new Color(0.73f, 0.68f, 0.63f), 0f, -60f, 860f, 860f);

        // 棋盘根节点
        var boardRoot = new GameObject("BoardRoot");
        boardRoot.transform.SetParent(gamePanel.transform, false);
        var boardRT = boardRoot.AddComponent<RectTransform>();
        boardRT.anchorMin = boardRT.anchorMax = new Vector2(0.5f, 0.5f);
        boardRT.anchoredPosition = new Vector2(0f, -60f);
        boardRT.sizeDelta = new Vector2(820f, 820f);

        var grid = boardRoot.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(192, 192);
        grid.spacing = new Vector2(10, 10);
        grid.padding = new RectOffset(5, 5, 5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // Tile 预制体
        var tilePrefab = BuildTilePrefab();
        for (int i = 0; i < 16; i++)
            PrefabUtility.InstantiatePrefab(tilePrefab, boardRoot.transform);

        // ── 胜利弹窗 ──
        var winPanel   = BuildResultPanel("WinPanel",     canvas, "🎉 YOU WIN!",   new Color(0.15f,0.70f,0.35f,0.97f),
            out var winScoreTxt, out var winDoubleBtn, out var winNextBtn);

        // ── 失败弹窗 ──
        var overPanel  = BuildResultPanel("GameOverPanel",canvas, "😅 GAME OVER", new Color(0.75f,0.20f,0.20f,0.97f),
            out var overScoreTxt, out var reviveBtn, out var retryBtn);

        // 失败弹窗按钮文字改为复活/重试
        reviveBtn.GetComponentInChildren<TextMeshProUGUI>().text  = "📺 看广告复活";
        retryBtn.GetComponentInChildren<TextMeshProUGUI>().text   = "🔄 重试";

        // ── 绑定脚本引用 ──
        var gb = boardRoot.AddComponent<GameBoard>();
        gb.boardSize  = 4;
        gb.boardRoot  = boardRT;
        gb.tilePrefab = tilePrefab;

        var ui = canvas.AddComponent<UIManager>();
        ui.mainMenuPanel = menuPanel;
        ui.menuBestText  = bestMenuTxt;
        ui.playButton    = playBtn.GetComponent<Button>();
        ui.gamePanel     = gamePanel;
        ui.scoreText     = scoreTxt;
        ui.bestText      = bestTxt;
        ui.timerText     = timerTxt;
        ui.targetText    = targetTxt;
        ui.levelText     = levelTxt;
        ui.winPanel      = winPanel;
        ui.winScoreText  = winScoreTxt;
        ui.winDoubleBtn  = winDoubleBtn.GetComponent<Button>();
        ui.winNextBtn    = winNextBtn.GetComponent<Button>();
        ui.gameOverPanel = overPanel;
        ui.overScoreText = overScoreTxt;
        ui.overBestText  = overScoreTxt;
        ui.reviveBtn     = reviveBtn.GetComponent<Button>();
        ui.retryBtn      = retryBtn.GetComponent<Button>();

        gamePanel.SetActive(false);
        winPanel.SetActive(false);
        overPanel.SetActive(false);

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");

        EditorUtility.DisplayDialog("✅ 完成",
            "场景已保存：Assets/Scenes/GameScene.unity\n\n" +
            "接下来：\n1. 在AdManager填入IronSource AppKey\n2. 点 ▶ 测试游戏",
            "好的！");
    }

    // ─────────────────────────────────────────────
    // UI 工具方法（类型统一，不再混用Vector4/Vector2[]）
    // ─────────────────────────────────────────────

    static GameObject MakeCanvas()
    {
        var go = new GameObject("Canvas");
        var c  = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // 全屏Panel（无背景色）
    static GameObject MakePanelFull(string name, GameObject parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    // 全屏Image
    static void MakeImageFull(string name, GameObject parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    // 居中Image（anchoredPosition + sizeDelta）
    static void MakeImageCenter(string name, GameObject parent, Color color,
        float x, float y, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        go.AddComponent<Image>().color = color;
    }

    // 居中Text
    static TextMeshProUGUI MakeTextCenter(string name, GameObject parent, string text,
        float x, float y, float w, float h,
        int fontSize, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.color = color; tmp.fontStyle = style;
        return tmp;
    }

    // 锚点Text（anchorMin/Max + offset）
    static TextMeshProUGUI MakeTextAnchor(string name, GameObject parent, string text,
        float ancMinX, float ancMinY, float ancMaxX, float ancMaxY,
        float offMinX, float offMinY, float offMaxX, float offMaxY,
        int fontSize, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ancMinX, ancMinY);
        rt.anchorMax = new Vector2(ancMaxX, ancMaxY);
        rt.offsetMin = new Vector2(offMinX, offMinY);
        rt.offsetMax = new Vector2(offMaxX, offMaxY);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.color = color; tmp.fontStyle = style;
        return tmp;
    }

    // 居中Button
    static GameObject MakeButtonCenter(string name, GameObject parent, string label,
        float x, float y, float w, float h, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<Button>();
        var lgo = new GameObject("Label");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 52;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    // 结果弹窗（胜利/失败）
    static GameObject BuildResultPanel(string name, GameObject parent,
        string title, Color bgColor,
        out TextMeshProUGUI scoreTxt,
        out GameObject btn1,
        out GameObject btn2)
    {
        var panel = MakePanelFull(name, parent);
        MakeImageFull("Dim", panel, new Color(0, 0, 0, 0.65f));

        var box = new GameObject("Box");
        box.transform.SetParent(panel.transform, false);
        var brt = box.AddComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = new Vector2(900f, 900f);
        box.AddComponent<Image>().color = bgColor;

        var titleT = MakeTextCenter("Title", box, title,
            0f, 280f, 820f, 130f, 72, Color.white, FontStyles.Bold);
        titleT.alignment = TextAlignmentOptions.Center;

        scoreTxt = MakeTextCenter("ScoreTxt", box, "得分: 0",
            0f, 130f, 700f, 80f, 52, Color.white);
        scoreTxt.alignment = TextAlignmentOptions.Center;

        btn1 = MakeButtonCenter("Btn1", box, "📺 看广告双倍",
            0f, -30f, 700f, 110f, new Color(1f, 0.76f, 0.15f));

        btn2 = MakeButtonCenter("Btn2", box, "下一关 ▶",
            0f, -170f, 700f, 110f, new Color(0.25f, 0.55f, 0.85f));

        return panel;
    }

    // Tile 预制体
    static GameObject BuildTilePrefab()
    {
        var tile = new GameObject("TilePrefab");
        tile.AddComponent<RectTransform>().sizeDelta = new Vector2(192, 192);
        var bg = tile.AddComponent<Image>();
        bg.color = new Color(0.8f, 0.75f, 0.71f);
        var tv = tile.AddComponent<TileView>();
        tv.background = bg;

        var tgo = new GameObject("NumberText");
        tgo.transform.SetParent(tile.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 64; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tv.numberText = tmp;

        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(tile, "Assets/Prefabs/TilePrefab.prefab");
        Object.DestroyImmediate(tile);
        return prefab;
    }
}

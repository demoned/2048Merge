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

        // ── 背景 ──
        MakeImage("BG", canvas, new Color(0.12f, 0.11f, 0.10f), AnchorFull());

        // ── 管理器 ──
        MakeManager<GameManager>("GameManager");
        MakeManager<ScoreManager>("ScoreManager");
        MakeManager<AdManager>("AdManager");

        var swipeGO = MakeManager<SwipeInput>("SwipeInput");

        // ── 主菜单 Panel ──
        var menuPanel = MakePanel("MainMenuPanel", canvas, Color.clear, AnchorFull());
        MakeImage("MenuBG", menuPanel, new Color(0.18f, 0.17f, 0.16f), AnchorFull());
        var titleTxt = MakeText("TitleText", menuPanel, "2048 MERGE",
            AnchorCenter(0f, 200f, 700f, 120f), 90, Color.white, FontStyles.Bold);
        titleTxt.alignment = TextAlignmentOptions.Center;
        var bestMenuTxt = MakeText("BestText", menuPanel, "最高分: 0",
            AnchorCenter(0f, 80f, 400f, 60f), 44, new Color(0.9f, 0.8f, 0.3f));
        bestMenuTxt.alignment = TextAlignmentOptions.Center;
        var playBtn = MakeButton("PlayButton", menuPanel, "开始游戏",
            AnchorCenter(0f, -80f, 400f, 100f), new Color(0.3f, 0.7f, 0.4f));

        // ── 游戏界面 Panel ──
        var gamePanel = MakePanel("GamePanel", canvas, Color.clear, AnchorFull());

        // 顶栏
        var topBar = MakePanel("TopBar", gamePanel, Color.clear,
            new Vector4(0, 1, 1, 1), new Vector2(0, -140), new Vector2(0, 140));
        var scoreTxt = MakeText("ScoreText", topBar, "分数: 0",
            new Vector4(0, 0, 0.35f, 1), 46, Color.white);
        var timerTxt = MakeText("TimerText", topBar, "⏱ 60s",
            new Vector4(0.33f, 0, 0.66f, 1), 50, Color.yellow, FontStyles.Bold);
        timerTxt.alignment = TextAlignmentOptions.Center;
        var bestTxt = MakeText("BestText", topBar, "最高: 0",
            new Vector4(0.67f, 0, 1, 1), 36, new Color(0.7f, 0.7f, 0.7f));
        bestTxt.alignment = TextAlignmentOptions.Right;

        var targetTxt = MakeText("TargetText", gamePanel, "目标: 2048",
            new Vector4(0.1f, 0.86f, 0.9f, 0.93f), 40, new Color(1f, 0.9f, 0.3f));
        targetTxt.alignment = TextAlignmentOptions.Center;
        var levelTxt = MakeText("LevelText", gamePanel, "第 1 关",
            new Vector4(0.1f, 0.92f, 0.9f, 0.99f), 36, new Color(0.7f, 0.9f, 1f));
        levelTxt.alignment = TextAlignmentOptions.Center;

        // 棋盘
        var boardBG = MakeImage("BoardBG", gamePanel, new Color(0.73f, 0.68f, 0.63f),
            AnchorCenter(0f, -60f, 860f, 860f));

        var boardRoot = MakePanel("BoardRoot", gamePanel, Color.clear,
            AnchorCenter(0f, -60f, 820f, 820f));
        var grid = boardRoot.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(192, 192);
        grid.spacing = new Vector2(10, 10);
        grid.padding = new RectOffset(5, 5, 5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // 创建Tile预制体
        var tilePrefab = BuildTilePrefab();

        // 实例化16个tile（Editor显示用，Runtime由GameBoard管理）
        for (int i = 0; i < 16; i++)
            PrefabUtility.InstantiatePrefab(tilePrefab, boardRoot.transform);

        // ── 胜利弹窗 ──
        var winPanel = BuildResultPanel("WinPanel", canvas,
            "🎉 YOU WIN!", new Color(0.15f, 0.70f, 0.35f, 0.97f),
            out var winScoreTxt, out var winDoubleBtn, out var winNextBtn, out var _r1, out var _r2);

        // ── 失败弹窗 ──
        var overPanel = BuildResultPanel("GameOverPanel", canvas,
            "😅 GAME OVER", new Color(0.75f, 0.20f, 0.20f, 0.97f),
            out var overScoreTxt, out var overDoubleBtn, out var overNextBtn,
            out var reviveBtn, out var retryBtn);
        overDoubleBtn.gameObject.SetActive(false);
        overNextBtn.GetComponentInChildren<TextMeshProUGUI>().text = "重试";

        // ── 绑定引用到脚本 ──
        var gb = boardRoot.AddComponent<GameBoard>();
        gb.boardSize = 4;
        gb.boardRoot = boardRoot.GetComponent<RectTransform>();
        gb.tilePrefab = tilePrefab;

        var ui = canvas.AddComponent<UIManager>();
        ui.mainMenuPanel = menuPanel;
        ui.menuBestText = bestMenuTxt;
        ui.playButton = playBtn.GetComponent<Button>();
        ui.gamePanel = gamePanel;
        ui.scoreText = scoreTxt;
        ui.bestText = bestTxt;
        ui.timerText = timerTxt;
        ui.targetText = targetTxt;
        ui.levelText = levelTxt;
        ui.winPanel = winPanel;
        ui.winScoreText = winScoreTxt;
        ui.winDoubleBtn = winDoubleBtn.GetComponent<Button>();
        ui.winNextBtn = winNextBtn.GetComponent<Button>();
        ui.gameOverPanel = overPanel;
        ui.overScoreText = overScoreTxt;
        ui.overBestText = overScoreTxt; // 复用（可单独加）
        ui.reviveBtn = reviveBtn.GetComponent<Button>();
        ui.retryBtn = retryBtn.GetComponent<Button>();

        // 默认隐藏游戏/弹窗Panel
        gamePanel.SetActive(false);
        winPanel.SetActive(false);
        overPanel.SetActive(false);

        // 保存场景
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");

        EditorUtility.DisplayDialog("✅ 完成",
            "场景已保存：Assets/Scenes/GameScene.unity\n\n" +
            "接下来：\n" +
            "1. 在AdManager填入IronSource AppKey\n" +
            "2. 点 ▶ 测试游戏",
            "好的！");
    }

    // ─────────────────────────────────────────────
    // 工具方法
    // ─────────────────────────────────────────────

    static GameObject MakeCanvas()
    {
        var go = new GameObject("Canvas");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static T MakeManager<T>(string name) where T : MonoBehaviour
    {
        var go = new GameObject(name);
        return go.AddComponent<T>();
    }

    static GameObject MakePanel(string name, GameObject parent, Color color,
        Vector4 anchor, Vector2? anchoredPos = null, Vector2? sizeDelta = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchor.x, anchor.y);
        rt.anchorMax = new Vector2(anchor.z, anchor.w);
        rt.anchoredPosition = anchoredPos ?? Vector2.zero;
        rt.sizeDelta = sizeDelta ?? Vector2.zero;
        if (color != Color.clear)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
        }
        return go;
    }

    static Image MakeImage(string name, GameObject parent, Color color, Vector4 anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchor.x, anchor.y);
        rt.anchorMax = new Vector2(anchor.z, anchor.w);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static Image MakeImage(string name, GameObject parent, Color color,
        Vector2 center, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI MakeText(string name, GameObject parent, string text,
        Vector4 anchor, int fontSize, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchor.x, anchor.y);
        rt.anchorMax = new Vector2(anchor.z, anchor.w);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        return tmp;
    }

    static GameObject MakeButton(string name, GameObject parent, string label,
        Vector2[] anchor, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchor[0];
        rt.sizeDelta = anchor[1];
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        go.AddComponent<Button>();
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 52;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    static GameObject BuildResultPanel(string name, GameObject parent,
        string title, Color bgColor,
        out TextMeshProUGUI scoreTxt,
        out GameObject doubleBtn,
        out GameObject nextBtn,
        out GameObject btn3,
        out GameObject btn4)
    {
        var panel = MakePanel(name, parent, Color.clear, AnchorFull());
        MakeImage("Dim", panel, new Color(0, 0, 0, 0.6f), AnchorFull());
        var box = MakePanel("Box", panel, bgColor,
            AnchorCenter(0, 0, 900f, 900f));

        MakeText("Title", box, title,
            new Vector4(0.05f, 0.7f, 0.95f, 0.95f), 72, Color.white, FontStyles.Bold)
            .alignment = TextAlignmentOptions.Center;

        scoreTxt = MakeText("ScoreTxt", box, "得分: 0",
            new Vector4(0.1f, 0.55f, 0.9f, 0.68f), 52, Color.white);
        scoreTxt.alignment = TextAlignmentOptions.Center;

        doubleBtn = MakeButton("DoubleBtn", box, "📺 看广告双倍",
            new Vector2[] { new Vector2(0, 60), new Vector2(700, 110) },
            new Color(1f, 0.76f, 0.15f));
        nextBtn = MakeButton("NextBtn", box, "下一关 ▶",
            new Vector2[] { new Vector2(0, -70), new Vector2(700, 110) },
            new Color(0.25f, 0.55f, 0.85f));

        btn3 = doubleBtn;
        btn4 = nextBtn;

        return panel;
    }

    static GameObject BuildTilePrefab()
    {
        var tile = new GameObject("TilePrefab");
        var rt = tile.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(192, 192);
        var bg = tile.AddComponent<Image>();
        bg.color = new Color(0.8f, 0.75f, 0.71f);
        var tv = tile.AddComponent<TileView>();
        tv.background = bg;

        var textGO = new GameObject("NumberText");
        textGO.transform.SetParent(tile.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 64; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tv.numberText = tmp;

        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(tile, "Assets/Prefabs/TilePrefab.prefab");
        Object.DestroyImmediate(tile);
        return prefab;
    }

    // ── 锚点快捷方式 ──
    static Vector4 AnchorFull() => new Vector4(0, 0, 1, 1);
    static Vector2[] AnchorCenter(float x, float y, float w, float h)
        => new Vector2[] { new Vector2(x, y), new Vector2(w, h) };
    static Vector4 AnchorCenter(float x, float y, float w, float h)  // 重载给Image用
        => new Vector4(x, y, w, h); // 实际用anchoredPos+size传入，这里只是占位
}

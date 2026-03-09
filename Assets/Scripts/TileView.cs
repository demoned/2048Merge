using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单个方块的视图层 - 颜色/数字/动画
/// </summary>
public class TileView : MonoBehaviour
{
    [Header("References")]
    public Image background;
    public TextMeshProUGUI numberText;

    // 颜色配置（欧美流行明亮色系）
    static readonly Color[] TileColors = {
        new Color(0.80f, 0.75f, 0.71f), // 0 (空)
        new Color(0.93f, 0.89f, 0.85f), // 2
        new Color(0.93f, 0.88f, 0.78f), // 4
        new Color(0.95f, 0.69f, 0.47f), // 8
        new Color(0.96f, 0.58f, 0.39f), // 16
        new Color(0.96f, 0.49f, 0.37f), // 32
        new Color(0.96f, 0.37f, 0.23f), // 64
        new Color(0.93f, 0.81f, 0.45f), // 128
        new Color(0.93f, 0.80f, 0.38f), // 256
        new Color(0.93f, 0.78f, 0.31f), // 512
        new Color(0.93f, 0.77f, 0.25f), // 1024
        new Color(0.93f, 0.76f, 0.18f), // 2048
        new Color(0.30f, 0.85f, 0.50f), // 4096+
    };

    public void SetValue(int value)
    {
        if (value == 0)
        {
            numberText.text = "";
            background.color = TileColors[0];
            return;
        }

        numberText.text = value.ToString();

        // 根据log2计算颜色索引
        int idx = Mathf.Min(Mathf.RoundToInt(Mathf.Log(value, 2)), TileColors.Length - 1);
        background.color = TileColors[idx];

        // 小数字用深色文字，大数字用白色
        numberText.color = value <= 4
            ? new Color(0.47f, 0.43f, 0.40f)
            : Color.white;

        // 字体大小自适应
        numberText.fontSize = value >= 1000 ? 44 : value >= 100 ? 54 : 64;
    }

    public void PlaySpawnAnim()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleAnim(0f, 1f, 0.12f));
    }

    public void PlayMergeAnim()
    {
        StopAllCoroutines();
        StartCoroutine(BounceAnim());
    }

    IEnumerator ScaleAnim(float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    IEnumerator BounceAnim()
    {
        float duration = 0.15f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t / duration * Mathf.PI) * 0.25f;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}

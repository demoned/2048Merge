using UnityEngine;

/// <summary>
/// 本地存档管理（PlayerPrefs封装）
/// </summary>
public static class SaveManager
{
    public static void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public static int LoadInt(string key, int defaultVal = 0) => PlayerPrefs.GetInt(key, defaultVal);
    public static void SaveFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public static float LoadFloat(string key, float defaultVal = 0f) => PlayerPrefs.GetFloat(key, defaultVal);
    public static void SaveString(string key, string value) => PlayerPrefs.SetString(key, value);
    public static string LoadString(string key, string defaultVal = "") => PlayerPrefs.GetString(key, defaultVal);
    public static void DeleteAll() => PlayerPrefs.DeleteAll();
}

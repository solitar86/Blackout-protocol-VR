using UnityEngine;

public static class PlayerSettingsStorage
{
    public static void Save<T>(string key , T value)
    {
        string data = JsonUtility.ToJson(value);
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }
/// <summary>
/// Load settings or return defaults if none are saved.
/// </summary>
/// <typeparam name="T">Type of settings objects</typeparam>
/// <param name="key">String to identify the object</param>
/// <param name="defaults">Return this if no saved settings found</param>
/// <returns></returns>
    public static T Load<T>(string key , T defaults)
    {
        if(PlayerPrefs.HasKey(key))
        {
            return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));
        }
        return defaults;
    }
}

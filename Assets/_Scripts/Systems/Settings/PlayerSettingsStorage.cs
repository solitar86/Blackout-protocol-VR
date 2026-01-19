using UnityEngine;

public static class PlayerSettingsStorage
{
    public static void Save<T>(string key , T value)
    {
        string data = JsonUtility.ToJson(value);
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    public static T Load<T>(string key , T defaults)
    {
        if(PlayerPrefs.HasKey(key))
        {
            return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));
        }
        return defaults;
    }
}

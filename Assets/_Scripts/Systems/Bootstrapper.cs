using System;
using UnityEngine;
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeSystemsObject()
    {
        GameObject.DontDestroyOnLoad(GameObject.Instantiate(Resources.Load("Systems")));
        ResetStaticVariables();
        Debug.Log("Datapath is: " + Application.dataPath);
        Debug.Log("Persistant datapath is: " + Application.persistentDataPath);
    }

    private static void ResetStaticVariables()
    {
        TTSPlayer.ResetStaticVariables();
        ConversationManager.ResetStaticVariables();
    }
}

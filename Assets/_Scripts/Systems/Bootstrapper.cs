using System;
using UnityEngine;
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeSystemsObject()
    {
        GameObject.DontDestroyOnLoad(GameObject.Instantiate(Resources.Load("Systems")));
        ResetStaticVariables();
    }

    private static void ResetStaticVariables()
    {
        TTSPlayer.ResetStaticVariables();
    }
}

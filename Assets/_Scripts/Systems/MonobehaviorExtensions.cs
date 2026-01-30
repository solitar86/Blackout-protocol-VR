using System.Collections;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public static class MonoBehaviourExtensions
{
    public static void CallWithDelay(this MonoBehaviour mono, Action method, float delay)
        => mono.StartCoroutine(CallWithDelayRoutine(method, delay));

    private static IEnumerator CallWithDelayRoutine(Action method, float delay)
    {
        if (delay <= 0)
        {
            method?.Invoke();
            yield break;
        }
        yield return new WaitForSeconds(delay);
        method?.Invoke();
    }


    public static bool IsInLayerMask(this GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

/// <summary>
/// This wrapper is necessary since SnapTurnProvider is a package class
/// and gets regenerated on package import, removing modifications.
/// </summary>
public class CustomSnapTurnProviderWrapper : SnapTurnProvider
{
    public static Action<bool> OnPlayerSnapTurn;
    public static bool IsSnapTurning
    {
        get
        {
            return NextTimeAllowEventCall > Time.time;
        }
    }

    public static float NextTimeAllowEventCall = 0f;

    protected override float GetTurnAmount(Vector2 input)
    {
        float amount = base.GetTurnAmount(input);

        if (Mathf.Approximately(amount, 0f) == false && NextTimeAllowEventCall < Time.time)
        {
            bool isRightTurn = amount > 0f;
            OnPlayerSnapTurn?.Invoke(isRightTurn);
            NextTimeAllowEventCall = Time.time + debounceTime;
            Debugger.Log("Calling snap turn event");

        }
        return amount;
    }


    public void ChangeTurnAmountToAngle(float snapTurnAngle)
    {
        turnAmount = snapTurnAngle;
    }


}
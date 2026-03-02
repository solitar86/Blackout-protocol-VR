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

    protected override float GetTurnAmount(Vector2 input)
    {
        float amount = base.GetTurnAmount(input);

        if (Mathf.Approximately(amount, 0f) == false)
        {
            bool isRightTurn = amount > 0f;
            OnPlayerSnapTurn?.Invoke(isRightTurn);
        }
        return amount;
    }

    public void ChangeTurnAmountToAngle(float snapTurnAngle)
    {
        turnAmount = snapTurnAngle;
    }


}
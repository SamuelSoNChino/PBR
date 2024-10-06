using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the Berserk power, which .
/// </summary>
public class BerserkPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BerserkPower"/> class.
    /// </summary>
    public BerserkPower() : base("Berserk", 5, false, true, 5)
    {
    }

    /// <summary>
    /// Activates the Berserk power.
    /// </summary>
    /// <param name="targetPlayer">The player on whom the power is activated.</param>
    public override void Activate(Player userPlayer, Player targetPlayer)
    {
        UseTimer(userPlayer, targetPlayer, 5);
    }

    private IEnumerator UseTimer(Player userPlayer, Player targetPlayer, float useDuration)
    {
        userPlayer.OwnerOfPuzzleCurrentlyManipulating = targetPlayer;
        targetPlayer.AddPlayerCurrenlyManipulatingPuzzle(userPlayer);
        userPlayer.HasPuzzleTileMovementPermission = true;
        yield return new WaitForSeconds(useDuration);
        userPlayer.HasPuzzleTileMovementPermission = false;
        targetPlayer.RemovePlayerCurrenlyManipulatingPuzzle(userPlayer);
    }
}

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents the Berserk power, which .
/// </summary>
public class BerserkPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BerserkPower"/> class.
    /// </summary>
    public BerserkPower() : base("Berserk", 5, false, false, 5)
    {
    }

    /// <summary>
    /// Activates the Berserk power.
    /// </summary>
    public override void Activate(Player userPlayer)
    {
        float useDuration = 5;
        UseTimer(userPlayer, useDuration);
    }

    private IEnumerator UseTimer(Player userPlayer, float useDuration)
    {
        PuzzleManager puzzleManager = Object.FindAnyObjectByType<PuzzleManager>();
        PeekManager peekManager = Object.FindAnyObjectByType<PeekManager>();

        if (userPlayer.IsPeeking)
        {
            peekManager.UpdatePeekIndicator(userPlayer.TargetOfPeekPlayer);
        }

        puzzleManager.EnableTileMovement(userPlayer);

        yield return new WaitForSeconds(useDuration);

        if (userPlayer.IsPeeking)
        {
            puzzleManager.DisableTileMovement(userPlayer);
        }
    }
}

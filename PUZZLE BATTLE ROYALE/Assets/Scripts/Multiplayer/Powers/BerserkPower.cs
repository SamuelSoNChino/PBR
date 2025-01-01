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
    public BerserkPower() : base("Berserk", 5, false, false, 15)
    {
    }

    /// <summary>
    /// Activates the Berserk power.
    /// </summary>
    public override void Activate(Player userPlayer)
    {
        float useDuration = 3;
        CoroutineHelper.Instance.StartHelperCoroutine(UseTimer(userPlayer, useDuration));
    }

    private IEnumerator UseTimer(Player userPlayer, float useDuration)
    {
        PuzzleManager puzzleManager = GameObject.Find("Puzzle").GetComponent<PuzzleManager>();
        PeekManager peekManager = GameObject.Find("PeekComponents").GetComponent<PeekManager>();


        if (userPlayer.IsPeeking)
        {
            peekManager.UpdatePeekIndicator(userPlayer.TargetOfPeekPlayer);
        }

        puzzleManager.EnableTileMovement(userPlayer);

        yield return new WaitForSeconds(useDuration);

        if (userPlayer.IsPeeking && !userPlayer.TargetOfPeekPlayer.IsPeeking)
        {
            puzzleManager.DisableTileMovement(userPlayer);
        }
    }
}

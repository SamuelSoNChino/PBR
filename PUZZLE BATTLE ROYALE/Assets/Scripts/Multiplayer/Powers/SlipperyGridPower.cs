using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the Slippery Grid power, which temporarily disables a player's ability to snap to the grid.
/// </summary>
public class SlipperyGridPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SlipperyGridPower"/> class.
    /// </summary>
    public SlipperyGridPower() : base("Slippery Grid", 3, false, true, 5)
    {
    }

    /// <summary>
    /// A dictionary that keeps track of active coroutines associated with each player.
    /// </summary>
    private Dictionary<Player, Coroutine> activeCoroutines = new();

    /// <summary>
    /// Activates the Slippery Grid power on the specified target player.
    /// </summary>
    /// <param name="targetPlayer">The player on whom the power is activated.</param>
    public override void Activate(Player userPlayer, Player targetPlayer)
    {
        if (activeCoroutines.Keys.Contains(targetPlayer))
        {
            CoroutineHelper.Instance.StopHelperCoroutine(activeCoroutines[targetPlayer]);
        }

        Coroutine newCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(UseTimer(targetPlayer, 5));
        activeCoroutines[targetPlayer] = newCoroutine;
    }

    /// <summary>
    /// A coroutine that disables the player's ability to snap to the grid for a specified duration.
    /// </summary>
    /// <param name="targetPlayer">The player whose grid snapping is disabled.</param>
    /// <param name="useDuration">The duration for which grid snapping is disabled.</param>
    /// <returns>An enumerator for the coroutine.</returns>
    private IEnumerator UseTimer(Player targetPlayer, float useDuration)
    {
        targetPlayer.SnapToGridEnabled = false;
        yield return new WaitForSeconds(useDuration);
        targetPlayer.SnapToGridEnabled = true;
    }
}

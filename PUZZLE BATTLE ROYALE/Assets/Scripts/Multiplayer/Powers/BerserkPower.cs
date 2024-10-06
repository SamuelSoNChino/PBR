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
        
    }

    private IEnumerator UseTimer(Player targetPlayer, float useDuration)
    {
        
        yield return new WaitForSeconds(useDuration);
        
    }
}

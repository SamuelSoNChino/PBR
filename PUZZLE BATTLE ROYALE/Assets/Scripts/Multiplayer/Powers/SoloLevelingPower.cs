/// <summary>
/// Represents the "Solo Leveling" power which makes the player ranking invisible to all people and also disables the player for using 
/// peek or being the target of peek.
/// </summary>
public class SoloLevelingPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoloLevelingPower"/> class.
    /// </summary>
    public SoloLevelingPower() : base("Solo Leveling", 4, true, false, 0)
    {

    }
}

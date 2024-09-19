/// <summary>
/// Represents the "Solo Leveling" power which enables player play without.
/// </summary>
public class SoloLevelingPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoloLevelingPower"/> class.
    /// </summary>
    public SoloLevelingPower() : base("Solo Leveling", 4, true, false, 0)
    {

    }

    /// <summary>
    /// Overrides the base activation method. As this power is passive, this method does nothing.
    /// </summary>
    public override void Activate()
    {
        return;
    }
}

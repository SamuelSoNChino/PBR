/// <summary>
/// Represents the "Secret Peek" power which enables player to peek at othr players without the game notifying them.
/// </summary>
public class SecretPeekPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretPeekPower"/> class.
    /// </summary>
    public SecretPeekPower() : base("Secret Peek", 1, true, false, 0)
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

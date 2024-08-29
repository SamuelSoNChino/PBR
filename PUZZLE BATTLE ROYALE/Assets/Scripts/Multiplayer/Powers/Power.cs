/// <summary>
/// Represents a base class for different types of powers in the game.
/// </summary>
public abstract class Power
{
    /// <summary>
    /// Gets the name of the power.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the unique identifier for the power.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the power is passive.
    /// </summary>
    public bool IsPassive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the power is targetable by the player.
    /// </summary>
    public bool IsTargetable { get; private set; }

    /// <summary>
    /// Gets the cooldown duration for the power's use.
    /// </summary>
    public float CooldownDuration { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Power"/> class.
    /// </summary>
    /// <param name="name">The name of the power.</param>
    /// <param name="id">The unique identifier of the power.</param>
    /// <param name="isPassive">Indicates if the power is passive.</param>
    /// <param name="isTargetable">Indicates if the power is targetable by the player.</param>
    /// <param name="cooldownDuration">The cooldown duration of the power.</param>
    protected Power(string name, int id, bool isPassive, bool isTargetable, float cooldownDuration)
    {
        Name = name;
        Id = id;
        IsPassive = isPassive;
        IsTargetable = isTargetable;
        CooldownDuration = cooldownDuration;
    }

    /// <summary>
    /// Activates the power. This method can be overridden by derived classes to provide specific behavior.
    /// </summary>
    public virtual void Activate()
    {
    }

    /// <summary>
    /// Activates the power on a specific player. This method can be overridden by derived classes to provide specific behavior.
    /// </summary>
    /// <param name="player">The player on whom the power is activated.</param>
    public virtual void Activate(Player player)
    {
    }
}

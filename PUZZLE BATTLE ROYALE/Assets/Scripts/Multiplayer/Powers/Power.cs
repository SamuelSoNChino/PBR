public abstract class Power
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public bool IsPassive { get; private set; }
    public bool IsTargetable { get; private set; }
    public float CooldownDuration { get; private set; }

    protected Power(string name, int id, bool isPassive, bool isTargetable, float cooldownDuration)
    {
        Name = name;
        Id = id;
        IsPassive = isPassive;
        IsTargetable = isTargetable;
        CooldownDuration = cooldownDuration;
    }

    public abstract void Activate();
}
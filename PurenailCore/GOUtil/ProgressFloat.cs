namespace PurenailCore.GOUtil;

public class ProgressFloat
{
    private readonly float growSpeed;
    private readonly float shrinkSpeed;

    public float Value;

    public ProgressFloat(float value, float growSpeed, float shrinkSpeed)
    {
        this.Value = value;
        this.growSpeed = growSpeed;
        this.shrinkSpeed = shrinkSpeed;
    }

    public bool Advance(float delta, float target)
    {
        if (Value < target)
        {
            Value += delta * growSpeed;
            if (Value > target) Value = target;
            return true;
        }
        else if (Value > target)
        {
            Value -= delta * shrinkSpeed;
            if (Value < target) Value = target;
            return true;
        }

        return false;
    }
}

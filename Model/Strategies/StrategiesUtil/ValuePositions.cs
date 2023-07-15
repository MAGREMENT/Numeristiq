namespace Model.Strategies.StrategiesUtil;

public class ValuePositions
{
    public ValuePositions(Positions positions, int value)
    {
        Positions = positions;
        Value = value;
    }

    public Positions Positions { get; }
    public int Value { get; }
}
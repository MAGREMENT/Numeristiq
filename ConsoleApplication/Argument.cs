namespace ConsoleApplication;

public class Argument
{
    public string Description { get; }
    public ValueType ValueType { get; }
    
    public Argument(string description, ValueType valueType)
    {
        if (valueType == ValueType.None)
            throw new ArgumentException("Value type cannot be none for argument", nameof(valueType));
        
        Description = description;
        ValueType = valueType;
    }

    public override string ToString()
    {
        return $"({ValueType.ToString().ToLower()} value) -> {Description}";
    }
}
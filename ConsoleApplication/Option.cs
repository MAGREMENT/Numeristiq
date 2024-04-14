namespace ConsoleApplication;

public class Option
{
    public string Name { get; }
    public string Description { get; }
    public ValueRequirement ValueRequirement { get; }
    public ValueType ValueType { get; }
    
    public Option(string name, string description, ValueRequirement valueRequirement = ValueRequirement.None, 
        ValueType valueType = ValueType.None)
    {
        if (name[0] != '-') throw new ArgumentException("An option name must start with a '-'", nameof(name));
        if ((valueRequirement == ValueRequirement.None && valueType != ValueType.None)
            || (valueType == ValueType.None && valueRequirement != ValueRequirement.None))
            throw new ArgumentException("If the value type or requirement is set the none, the other has to be too");
        
        Name = name;
        Description = description;
        ValueRequirement = valueRequirement;
        ValueType = valueType;
    }

    public override string ToString()
    {
        var valueString = ValueRequirement == ValueRequirement.None
            ? "(no value)"
            : $"({ValueRequirement.ToString().ToLower()} {ValueType.ToString().ToLower()} value)";

        return $"{Name} {valueString} -> {Description}";
    }
}

public enum ValueRequirement
{
    None, Mandatory, Optional
}

public enum ValueType
{
    None, Int, String, File
}
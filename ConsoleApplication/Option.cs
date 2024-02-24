namespace ConsoleApplication;

public record Option(string Name, string Description, OptionValueRequirement ValueRequirement = OptionValueRequirement.None,
    OptionValueType ValueType = OptionValueType.None);

public enum OptionValueRequirement
{
    None, Mandatory, Optional
}

public enum OptionValueType
{
    None, Int, String, File
}
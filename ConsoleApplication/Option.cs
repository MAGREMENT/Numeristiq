namespace ConsoleApplication;

public record Option(string Name, OptionValueRequirement ValueRequirement = OptionValueRequirement.None,
    OptionValueType ValueType = OptionValueType.None);

public enum OptionValueRequirement
{
    None, Mandatory, Optional
}

public enum OptionValueType
{
    None, Int, String, File
}
namespace ConsoleApplication.Commands;

public class ThemeCommand : Command
{
    public override string Description => "Transforms a theme in the repositories to a hard-coded string";
    
    public ThemeCommand() : base("Theme", 
        new Argument("Theme name", ValueType.String))
    {
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        //TODO
    }
}
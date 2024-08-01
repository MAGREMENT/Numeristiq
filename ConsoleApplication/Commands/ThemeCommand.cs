using System.Text;

namespace ConsoleApplication.Commands;

public class ThemeCommand : Command
{
    public const int NameIndex = 0;
    
    public override string Description => "Transforms a theme in the repositories to a hard-coded string";
    
    public ThemeCommand() : base("Theme", 
        new Argument("Theme name", ValueType.String))
    {
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var repo = interpreter.Instantiator.InstantiateThemeRepository();
        var theme = repo.FindTheme((string)report.GetArgumentValue(NameIndex));
        if (theme is null)
        {
            Console.WriteLine("No theme with this name found");
            return;
        }

        var builder = new StringBuilder($"new Theme({theme.Name}");
        int n = 0;
        foreach (var color in theme.AllColors())
        {
            builder.Append(',');
            builder.Append(n == 0 ? '\n' : ' ');
            builder.Append(color.Item2.ToHex());

            n = (n + 1) % 4;
        }

        builder.Append(')');
        Console.WriteLine(builder.ToString());
    }
}
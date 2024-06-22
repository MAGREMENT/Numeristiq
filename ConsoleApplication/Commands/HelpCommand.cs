using Model.Utility;

namespace ConsoleApplication.Commands;

public class HelpCommand : Command
{
    private const string FirstIndentation = "     ";
    private const string SecondIndentation = "        ";
    private const string ThirdIndentation = "           ";

    private const int CommandIndex = 0;
    
    public override string Description => "Gives instruction on how to use the application";
    
    public HelpCommand() : base("Help",
        new Option("-c", "A specific command", ValueRequirement.Mandatory, ValueType.String))
    {
    }

    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (report.IsOptionUsed(CommandIndex))
        {
            var name = (string)report.GetOptionValue(CommandIndex)!;
            Command? command = null;
            foreach (var c in report.Directory.Commands)
            {
                if (c.Name.EqualsCaseInsensitive(name))
                {
                    command = c;
                    break;
                }
            }

            if (command is null) Console.WriteLine("No corresponding command found");
            else ShowCommand(command, -1, "", FirstIndentation, SecondIndentation);
            
            return;
        }
        
        Console.WriteLine("\nUsage : [Directory]* [Command] [Argument] ([Option] [Value]?)*");

        if(report.Directory.Directories.Count == 0) Console.WriteLine("\nNo directory");
        else
        {
            Console.WriteLine("\nDirectories :");
            var counter = 1;
            
            foreach (var directory in report.Directory.Directories)
            {
                Console.WriteLine($"{(counter++ + ".").FillRightWith(' ', FirstIndentation.Length)}{directory.Name}");
            } 
        }

        if (report.Directory.Commands.Count == 0) Console.WriteLine("\nNo command");
        else
        {
            Console.WriteLine("\nCommands :");
            var counter = 1;

            foreach (var command in report.Directory.Commands)
            {
                ShowCommand(command, counter, FirstIndentation, SecondIndentation, ThirdIndentation);
                counter++;
            }
        }
    }

    private static void ShowCommand(Command command, int number, string ind1, string ind2, string ind3)
    {
        var start = number > 0 ? (number + ".").FillRightWith(' ', ind1.Length) : ind1;
        Console.WriteLine($"{start}{command.Name} -> {command.Description}");
        if (command.Arguments.Count == 0) Console.WriteLine(ind2 + "No argument");
        else
        {
            var secondCounter = 1;
            Console.WriteLine(ind2 + "Arguments : ");
            foreach (var argument in command.Arguments)
            {
                Console.WriteLine($"{ind3}{secondCounter++}. {argument}");
            }
        }
        if(command.Options.Count == 0) Console.WriteLine(ind2 + "No option");
        else
        {
            Console.WriteLine(ind2 + "Options : ");
            foreach (var option in command.Options)
            {
                Console.WriteLine(ind3 + option);
            }
        }
    }
}
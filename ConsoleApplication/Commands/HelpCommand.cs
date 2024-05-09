using Model.Utility;

namespace ConsoleApplication.Commands;

public class HelpCommand : Command
{
    private const string FirstIndentation = "     ";
    private const string SecondIndentation = "        ";
    private const string ThirdIndentation = "           ";
    
    public override string Description => "Gives instruction on how to use the application";
    
    public HelpCommand() : base("Help")
    {
    }

    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
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
                Console.WriteLine($"{(counter++ + ".").FillRightWith(' ', FirstIndentation.Length)}{command.Name} -> {command.Description}");
                if (command.Arguments.Count == 0) Console.WriteLine(SecondIndentation + "No argument");
                else
                {
                    var secondCounter = 1;
                    Console.WriteLine(SecondIndentation + "Arguments : ");
                    foreach (var argument in command.Arguments)
                    {
                        Console.WriteLine($"{ThirdIndentation}{secondCounter++}. {argument}");
                    }
                }
                if(command.Options.Count == 0) Console.WriteLine(SecondIndentation + "No option");
                else
                {
                    Console.WriteLine(SecondIndentation + "Options : ");
                    foreach (var option in command.Options)
                    {
                        Console.WriteLine(ThirdIndentation + option);
                    }
                }
            }
        }
    }
}
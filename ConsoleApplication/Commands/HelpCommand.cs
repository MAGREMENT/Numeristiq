namespace ConsoleApplication.Commands;

public class HelpCommand : Command
{
    public override string Description => "Gives instruction on how to use the application";
    
    public HelpCommand() : base("Help")
    {
    }

    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        Console.WriteLine("\nUsage : [Command] ([Option] [Value]?)*\n\nCommands :");
        int counter = 1;

        foreach (var command in interpreter.Commands)
        {
            Console.WriteLine($"{counter++}. {command.Name} -> {command.Description}");
            if(command.Options.Count == 0) Console.WriteLine("   No option");
            else
            {
                Console.WriteLine("   Options : ");
                foreach (var option in command.Options)
                {
                    Console.WriteLine($"   {option.Name} ({option.ValueRequirement.ToString().ToLower()}" +
                                      $"{option.ValueType.ToString().ToLower()} value) -> {option.Description}");
                }
            }
        }
    }
}
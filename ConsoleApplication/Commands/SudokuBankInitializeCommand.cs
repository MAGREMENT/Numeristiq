namespace ConsoleApplication.Commands;

public class SudokuBankInitializeCommand : Command
{
    public SudokuBankInitializeCommand() : base("Initialize")
    {
    }

    public override string Description => "Initialize the Sudoku bank";
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var repo = interpreter.Instantiator.InstantiateBankRepository();
        try
        {
            repo.Initialize();
            Console.WriteLine("Operation was done successfully");
        }
        catch(Exception e)
        {
           Console.WriteLine(e.Message); 
        }
    }
}
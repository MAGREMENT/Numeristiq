namespace ConsoleApplication.Commands;

public class SudokuBankClearCommand : Command
{
    public SudokuBankClearCommand() : base("Clear"){}

    public override string Description => "Clears the Sudoku bank";
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var repo = interpreter.Instantiator.InstantiateBankRepository();
        try
        {
            repo.Clear();
            Console.WriteLine("Operation was done successfully");
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message); 
        }
    }
}